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
        NumberOfSamples = 4
    })
{
    private double _timeElapsed;
    private int _frameCount;

    private Shader? _defaultShader;
    private Quad? _quad;

    private readonly Stopwatch _stopwatch = new();

    protected override void OnLoad()
    {
        _stopwatch.Start();

        GlDebugger.Init();
        GL.ClearColor(Color.Coral);

        _defaultShader = new Shader("Shaders/default.vert", "Shaders/default.frag");
        _defaultShader.Use();

        _quad = new Quad();
    }

    protected override void OnUnload()
    {
        _quad!.Dispose();
        _defaultShader!.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();
    }


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

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _defaultShader!.Use();
        _quad.Draw();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
    }
}