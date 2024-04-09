using System.Diagnostics;
using System.Drawing;
using CommonRainFrog.Renderer;
using CommonRainFrog.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CommonRainFrog;

public class RainFrogApplication(int width, int height, string title) : GameWindow(GameWindowSettings.Default,
    new NativeWindowSettings
    {
        Title = title,
        ClientSize = (width, height),
        Vsync = VSyncMode.On
    })
{
    private Shader? _computeShader;
    private Shader? _defaultShader;

    private int _vao;
    private int _vbo;
    private int _ebo;
    private int _texture;
    
    private readonly Stopwatch _stopwatch = new();

    private readonly float[] _vertices =
    {
        -1.0f, -1.0f, 0.0f, 0.0f,
        1.0f, -1.0f, 1.0f, 0.0f,
        1.0f, 1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 0.0f, 1.0f,
    };

    private readonly int[] _indices =
    {
        0, 1, 2,
        2, 3, 0
    };

    protected override void OnLoad()
    {
        _stopwatch.Start();
        
        GlDebugger.Init();
        GL.ClearColor(Color.CornflowerBlue);

        _defaultShader = new Shader("Shaders/default.vert", "Shaders/default.frag");

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(int), _indices,
            BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

        _computeShader = new Shader("Shaders/default.comp");
        
        _texture = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        GL.TextureParameter(_texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(_texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(_texture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TextureParameter(_texture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 512, 512, 0, PixelFormat.Rgba, PixelType.Float, 0);
        
        GL.BindImageTexture(0, _texture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
    }

    protected override void OnUnload()
    {
        GL.DeleteTexture(_texture);
        
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        
        _defaultShader?.Dispose();
        _computeShader?.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();
    }

    private double _timeElapsed;
    private int _frameCount;
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        _timeElapsed += e.Time;
        _frameCount++;

        if (_timeElapsed > 0.2f)
        {
            Title = $"FPS {(_frameCount / _timeElapsed):0.000}";
            _frameCount = 0;
            _timeElapsed = 0;
        }
        
        _computeShader!.Use();
        _computeShader.SetFloat("time", (float)_stopwatch.Elapsed.TotalMilliseconds);
        GL.DispatchCompute(512, 512, 1);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _defaultShader!.Use();
        _defaultShader.SetInt("tex", 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        RenderQuad();

        SwapBuffers();
    }

    private void RenderQuad()
    {
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
    }
}