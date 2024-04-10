using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using CommonRainFrog.Renderer;
using CommonRainFrog.Renderer.Meshes;
using CommonRainFrog.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CommonRainFrog;

public class RainFrogApplication(int width, int height, string title) : GameWindow(GameWindowSettings.Default,
    new NativeWindowSettings
    {
        Title = title,
        ClientSize = (width, height),
        NumberOfSamples = 4
    })
{
    private double _timeElapsed;
    private int _frameCount;

    private Camera? _camera;

    private Shader? _quadShader;
    private Shader? _cubeShader;
    
    private Quad? _quad;
    private Cube? _cube;

    private int _uboMatrices;

    private readonly Stopwatch _stopwatch = new();

    protected override void OnLoad()
    {
        _stopwatch.Start();

        GlDebugger.Init();
        GL.ClearColor(Color.Coral);
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);
        
        GL.CullFace(CullFaceMode.Back);

        _camera = new Camera(Vector3.UnitZ * 3, ClientSize.X / (float)ClientSize.Y, KeyboardState,
            MouseState);
        CursorState = CursorState.Grabbed;
        
        
        _quadShader = new Shader("Shaders/quad.vert", "Shaders/quad.frag");
        _cubeShader = new Shader("Shaders/cube.vert", "Shaders/cube.frag");

        _quad = new Quad(_quadShader);
        _cube = new Cube(_cubeShader);
        
        SetupUniformBufferObject();
    }

    protected override void OnUnload()
    {
        GL.DeleteBuffer(_uboMatrices);
        
        _quad!.Dispose();
        _cube!.Dispose();
        
        _quadShader!.Dispose();
        _cubeShader!.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        if (KeyboardState.IsKeyPressed(Keys.F11))
            WindowState = WindowState != WindowState.Fullscreen ? WindowState.Fullscreen : WindowState.Normal;
        
        if (KeyboardState.IsKeyPressed(Keys.F))
            CursorState = CursorState == CursorState.Grabbed ? CursorState.Normal : CursorState.Grabbed;
        
        if (CursorState == CursorState.Grabbed)
            _camera!.Update(e.Time);
    }


    protected override void OnRenderFrame(FrameEventArgs e)
    {
        DisplayFps(e.Time);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        FillUniformBufferObject();

        Render2DScene();
        Render3DScene();
        
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
    }

    private void Render2DScene()
    {
        GL.Disable(EnableCap.CullFace);
        _quad!.Draw( Vector3.Zero, 0.5f);
    }

    private void Render3DScene()
    {
        GL.Enable(EnableCap.CullFace);
        _cube!.Draw( Vector3.One, 0.5f, (float)_stopwatch.Elapsed.TotalSeconds);
        _cube!.Draw( new Vector3(5.0f, 0.5f, 0.5f), new Vector4(1.0f, 0.25f, 0.25f, 1.0f), 0.5f, (float)_stopwatch.Elapsed.TotalSeconds);
        
    }

    private void DisplayFps(double time)
    {
        _timeElapsed += time;
        _frameCount++;

        if (!(_timeElapsed > 0.2f)) return;
        Title = $"FPS {(_frameCount / _timeElapsed):0.000}";
        _frameCount = 0;
        _timeElapsed = 0;
    }
    
    private void SetupUniformBufferObject()
    {
        _uboMatrices = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
        GL.BufferData(BufferTarget.UniformBuffer, 2 * Unsafe.SizeOf<Matrix4>(), IntPtr.Zero,
            BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, _uboMatrices, 0, 2 * Unsafe.SizeOf<Matrix4>());
    }
    
    private void FillUniformBufferObject()
    {
        Matrix4 projection = _camera!.GetProjectionMatrix();
        Matrix4 view = _camera.GetViewMatrix();
        
        GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
        GL.BufferSubData(BufferTarget.UniformBuffer, 0, Unsafe.SizeOf<Matrix4>(), ref projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Unsafe.SizeOf<Matrix4>(), Unsafe.SizeOf<Matrix4>(), ref view);
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
    }
}