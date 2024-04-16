using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommonRainFrog.Renderer;
using CommonRainFrog.Renderer.Lights;
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
        NumberOfSamples = 8
    })
{
    private double _timeElapsed;
    private int _frameCount;

    private Camera? _camera;

    private Shader? _quadShader;
    private Shader? _pbrShader;
    private Shader? _equirectangularToCubemapShader;
    private Shader? _irradianceShader;
    private Shader? _backgroundShader;
    private Shader? _skyboxShader;

    private Quad? _quad;
    private Plane? _plane;
    private Cube? _cube;
    private Sphere? _sphere;

    private Skybox? _skybox;

    private Texture2D? _stackedStoneAlbedoMap;
    private Texture2D? _stackedStoneAmbientOcclusionMap;
    private Texture2D? _stackedStoneMetallicMap;
    private Texture2D? _stackedStoneRoughnessMap;
    private Texture2D? _stackedStoneNormalMap;

    private Texture2D? _texturedAluminumAlbedoMap;
    private Texture2D? _texturedAluminumAmbientOcclusionMap;
    private Texture2D? _texturedAluminumMetallicMap;
    private Texture2D? _texturedAluminumRoughnessMap;
    private Texture2D? _texturedAluminumNormalMap;

    private static readonly string[] CalmSkyboxImagePaths =
    [
        "Assets/Skybox/Calm/px.png",
        "Assets/Skybox/Calm/nx.png",
        "Assets/Skybox/Calm/py.png",
        "Assets/Skybox/Calm/ny.png",
        "Assets/Skybox/Calm/pz.png",
        "Assets/Skybox/Calm/nz.png"
    ];

    private int _uboMatrices;

    private readonly Stopwatch _stopwatch = new();


    private Framebuffer _postprocessFramebuffer;
    private Renderbuffer _postprocessRenderbuffer;

    private Vector3 _lightDirection = new(2.0f, -1.0f, 0.5f);
    
    private PointLight[] _pointLights = [
        new PointLight(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(600.0f))
    ];

    protected override void OnLoad()
    {
        _stopwatch.Start();

        GlDebugger.Init();

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);

        GL.CullFace(CullFaceMode.Back);

        _camera = new Camera(Vector3.UnitZ * 3, ClientSize.X / (float)ClientSize.Y, KeyboardState,
            MouseState);

        CursorState = CursorState.Grabbed;


        _quadShader = new Shader("Assets/Shaders/quad.vert", "Assets/Shaders/quad.frag");

        _pbrShader = new Shader("Assets/Shaders/pbr.vert", "Assets/Shaders/pbr.frag");
        _pbrShader.Use();
        _pbrShader.SetVector3("lightDirection", _lightDirection);
        _pbrShader.SetInt("albedoMap", 0);
        _pbrShader.SetInt("ambientOcclusionMap", 1);
        _pbrShader.SetInt("metallicMap", 2);
        _pbrShader.SetInt("roughnessMap", 3);
        _pbrShader.SetInt("normalMap", 4);
        _pbrShader.SetInt("shadowMap", 5);
        _pbrShader.SetInt("irradianceMap", 6);

        SetupImageBasedLighting();

        _quad = new Quad(_quadShader);
        _plane = new Plane(_pbrShader);
        _cube = new Cube(_pbrShader);
        _sphere = new Sphere(_pbrShader);

        _skyboxShader = new Shader("Assets/Shaders/skybox.vert", "Assets/Shaders/skybox.frag");
        _skybox = new Skybox(_skyboxShader);
        _skybox.SetTexture(CalmSkyboxImagePaths);

        _stackedStoneAlbedoMap = new Texture2D("Assets/Textures/StackedStone/Albedo.png");
        _stackedStoneAmbientOcclusionMap = new Texture2D("Assets/Textures/StackedStone/AmbientOcclusion.png");
        _stackedStoneMetallicMap = new Texture2D("Assets/Textures/StackedStone/Metallic.png");
        _stackedStoneRoughnessMap = new Texture2D("Assets/Textures/StackedStone/Roughness.png");
        _stackedStoneNormalMap = new Texture2D("Assets/Textures/StackedStone/Normal.png");

        _texturedAluminumAlbedoMap = new Texture2D("Assets/Textures/TexturedAluminum/Albedo.png");
        _texturedAluminumAmbientOcclusionMap = new Texture2D("Assets/Textures/TexturedAluminum/AmbientOcclusion.png");
        _texturedAluminumMetallicMap = new Texture2D("Assets/Textures/TexturedAluminum/Metallic.png");
        _texturedAluminumRoughnessMap = new Texture2D("Assets/Textures/TexturedAluminum/Roughness.png");
        _texturedAluminumNormalMap = new Texture2D("Assets/Textures/TexturedAluminum/Normal.png");

        _postprocessFramebuffer = new Framebuffer(ClientSize.X, ClientSize.Y);
        _postprocessFramebuffer.SetFramebufferTexture2D(FramebufferAttachment.ColorAttachment0);
        _postprocessFramebuffer.SetMinMagFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        _postprocessFramebuffer.SetTextureImage2D(PixelInternalFormat.Rgb, PixelFormat.Rgb, PixelType.UnsignedInt);

        _postprocessRenderbuffer = new Renderbuffer(ClientSize.X, ClientSize.Y,
            RenderbufferStorage.Depth24Stencil8, FramebufferAttachment.DepthStencilAttachment);

        SetupUniformBufferObject();
    }

    private void SetupImageBasedLighting()
    {
        _equirectangularToCubemapShader = new Shader("Assets/Shaders/IBL/cubemap.vert",
            "Assets/Shaders/IBL/equirectangularToCubemap.frag");
        _equirectangularToCubemapShader.Use();
        _equirectangularToCubemapShader.SetInt("equirectangularMap", 0);

        _irradianceShader =
            new Shader("Assets/Shaders/IBL/cubemap.vert", "Assets/Shaders/IBL/irradianceConvolution.frag");
        _irradianceShader.Use();
        _irradianceShader.SetInt("environmentMap", 0);

        _backgroundShader = new Shader("Assets/Shaders/IBL/background.vert", "Assets/Shaders/IBL/background.frag");
        _backgroundShader.Use();
        _backgroundShader.SetInt("environmentMap", 0);
    }

    protected override void OnUnload()
    {
        GL.DeleteBuffer(_uboMatrices);

        _postprocessFramebuffer.Dispose();
        _postprocessRenderbuffer.Dispose();

        _stackedStoneAlbedoMap!.Dispose();
        _stackedStoneAmbientOcclusionMap!.Dispose();
        _stackedStoneMetallicMap!.Dispose();
        _stackedStoneRoughnessMap!.Dispose();
        _stackedStoneNormalMap!.Dispose();

        _texturedAluminumNormalMap!.Dispose();
        _texturedAluminumAmbientOcclusionMap!.Dispose();
        _texturedAluminumMetallicMap!.Dispose();
        _texturedAluminumRoughnessMap!.Dispose();
        _texturedAluminumNormalMap!.Dispose();

        _quad!.Dispose();
        _plane!.Dispose();
        _cube!.Dispose();
        _sphere!.Dispose();
        _skybox!.Dispose();

        _quadShader!.Dispose();
        _pbrShader!.Dispose();
        _skyboxShader!.Dispose();
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

        if (KeyboardState.IsKeyDown(Keys.Down))
        {
            Quaternion rotation = Quaternion.FromAxisAngle(Vector3.UnitY, 10.0f * (float)e.Time);
            _lightDirection = Vector3.Transform(_lightDirection, rotation);
        }

        if (KeyboardState.IsKeyDown(Keys.Up))
        {
            Quaternion rotation = Quaternion.FromAxisAngle(-Vector3.UnitY, 10.0f * (float)e.Time);
            _lightDirection = Vector3.Transform(_lightDirection, rotation);
        }
        
        if (KeyboardState.IsKeyDown(Keys.I))
            _pointLights[0].Position += Vector3.UnitY * 2.0f * (float)e.Time;        
        
        if (KeyboardState.IsKeyDown(Keys.Y))
            _pointLights[0].Position -= Vector3.UnitY * 2.0f * (float)e.Time;
        
        if (KeyboardState.IsKeyDown(Keys.U))
            _pointLights[0].Position += Vector3.UnitX * 2.0f * (float)e.Time;        
        
        if (KeyboardState.IsKeyDown(Keys.J))
            _pointLights[0].Position -= Vector3.UnitX * 2.0f * (float)e.Time;
        
        if (KeyboardState.IsKeyDown(Keys.K))
            _pointLights[0].Position += Vector3.UnitZ * 2.0f * (float)e.Time;

        if (KeyboardState.IsKeyDown(Keys.H))
            _pointLights[0].Position -= Vector3.UnitZ * 2.0f * (float)e.Time;        
        
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        if (ClientSize == Vector2i.Zero)
            return;

        CheckHotReloadShader();

        DisplayFps(e.Time);

        FillUniformBufferObject();


        _postprocessFramebuffer.Bind();
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Render2DScene();
        RenderSkybox();
        Render3DScene();


        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Disable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _quadShader!.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _postprocessFramebuffer.TextureId);
        _quad!.Draw();

        SwapBuffers();
    }
    
    private DateTime _lastCheckTime = DateTime.Now;
    
    private void CheckHotReloadShader()
    {
        DateTime fragmentLastWriteTime = File.GetLastWriteTime("Assets/Shaders/pbr.frag");

        if (fragmentLastWriteTime <= _lastCheckTime) return;
        _pbrShader!.Dispose();
        _pbrShader = new Shader("Assets/Shaders/pbr.vert", "Assets/Shaders/pbr.frag");
        _pbrShader.Use();
        _pbrShader.SetVector3("lightDirection", _lightDirection);
        _pbrShader.SetInt("albedoMap", 0);
        _pbrShader.SetInt("ambientOcclusionMap", 1);
        _pbrShader.SetInt("metallicMap", 2);
        _pbrShader.SetInt("roughnessMap", 3);
        _pbrShader.SetInt("normalMap", 4);
        _pbrShader.SetInt("shadowMap", 5);
        _pbrShader.SetInt("irradianceMap", 6);
        _lastCheckTime = DateTime.Now;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        _postprocessFramebuffer.Resize(ClientSize.X, ClientSize.Y);
        _postprocessRenderbuffer.Resize(ClientSize.X, ClientSize.Y);

        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
    }

    private void Render2DScene()
    {
        GL.Disable(EnableCap.CullFace);
    }

    private void RenderSkybox()
    {
        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Lequal);
        _skybox!.Draw();
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
    }

    private void Render3DScene()
    {
        GL.Enable(EnableCap.CullFace);
        _pbrShader!.Use();
        _pbrShader.SetVector3("cameraPosition", _camera!.Position);
        _pbrShader.SetVector3("lightDirection", _lightDirection);

        for (int i = 0; i < _pointLights.Length; i++)
        {
            _pbrShader.SetVector3($"pointLights[{i}]", _pointLights[i].Position);
            _pbrShader.SetVector3($"pointLightsColor[{i}]", _pointLights[i].Color);
        }
        
        _stackedStoneAlbedoMap!.Bind();
        _pbrShader.SetVector3("albedoColor", new Vector3(0.0f, 0.0f, 0.0f));
        _stackedStoneAmbientOcclusionMap!.Bind(1);
        _stackedStoneMetallicMap!.Bind(2);
        _stackedStoneRoughnessMap!.Bind(3);
        _stackedStoneNormalMap!.Bind(4);
        _sphere!.Draw(new Vector3(0.0f, 3.0f, 0.0f));
        _plane!.Draw(new Vector3(0.0f, -1.5f, 0.0f), new Vector3(MathHelper.DegreesToRadians(-90.0f), 0.0f, 0.0f),
            3.0f);

        _texturedAluminumAlbedoMap!.Bind();
        _pbrShader.SetVector3("albedoColor", new Vector3(1.0f, 0.25f, 0.25f));
        _texturedAluminumAmbientOcclusionMap!.Bind(1);
        _texturedAluminumMetallicMap!.Bind(2);
        _texturedAluminumRoughnessMap!.Bind(3);
        _texturedAluminumNormalMap!.Bind(4);
        _sphere.Draw(new Vector3(0.0f, -3.0f, 0.0f));

        _cube!.Draw(Vector3.One, new Vector3(0.0f, 0.0f, 0.0f), 0.5f, (float)_stopwatch.Elapsed.TotalSeconds);
        _cube.Draw(new Vector3(5.0f, 0.5f, 0.5f), new Vector3(0.0f, 1.0f, 0.0f), 0.5f);
        _cube.Draw(new Vector3(0.0f, 2.0f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), 0.5f);
        
        _pbrShader.SetVector3("albedoColor", new Vector3(1.0f, 1.0f, 1.0f));
        _cube.Draw(_pointLights[0].Position, 0.15f);
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