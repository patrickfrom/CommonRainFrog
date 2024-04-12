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
    private Shader? _pbrShader;
    private Shader? _skyboxShader;

    private Quad? _quad;
    private Cube? _cube;

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

    private int _postprocessFramebuffer;
    private int _postprocessRenderbuffer;
    private int _postprocessTexture;

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
        _pbrShader.SetInt("albedoMap", 0);
        _pbrShader.SetInt("ambientOcclusionMap", 1);
        _pbrShader.SetInt("metallicMap", 2);
        _pbrShader.SetInt("roughnessMap", 3);
        _pbrShader.SetInt("normalMap", 4);

        _quad = new Quad(_quadShader);
        _cube = new Cube(_pbrShader);

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

        CreatePostprocessFramebuffer();

        SetupUniformBufferObject();
    }

    private void CreatePostprocessFramebuffer()
    {
        _postprocessFramebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _postprocessFramebuffer);

        _postprocessTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _postprocessTexture);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, ClientSize.X, ClientSize.Y, 0,
            PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _postprocessTexture, 0);

        _postprocessRenderbuffer = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _postprocessRenderbuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, ClientSize.X,
            ClientSize.Y);

        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer, _postprocessRenderbuffer);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new Exception("ERROR::FRAMEBUFFER:: Framebuffer is not complete!");

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }

    protected override void OnUnload()
    {
        GL.DeleteBuffer(_uboMatrices);

        GL.DeleteFramebuffer(_postprocessFramebuffer);
        GL.DeleteRenderbuffer(_postprocessRenderbuffer);
        GL.DeleteTexture(_postprocessTexture);

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
        _cube!.Dispose();
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
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        if (ClientSize == Vector2i.Zero)
            return;

        DisplayFps(e.Time);

        FillUniformBufferObject();


        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _postprocessFramebuffer);

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
        GL.BindTexture(TextureTarget.Texture2D, _postprocessTexture);
        _quad!.Draw();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.BindTexture(TextureTarget.Texture2D, _postprocessTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, ClientSize.X, ClientSize.Y, 0,
            PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _postprocessRenderbuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, ClientSize.X,
            ClientSize.Y);


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

        _stackedStoneAlbedoMap!.Bind();
        _stackedStoneAmbientOcclusionMap!.Bind(1);
        _stackedStoneMetallicMap!.Bind(2);
        _stackedStoneRoughnessMap!.Bind(3);
        _stackedStoneNormalMap!.Bind(4);
        RenderSphere(new Vector3(0.0f, 3.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.5f));

        _texturedAluminumAlbedoMap!.Bind();
        _texturedAluminumAmbientOcclusionMap!.Bind(1);
        _texturedAluminumMetallicMap!.Bind(2);
        _texturedAluminumRoughnessMap!.Bind(3);
        _texturedAluminumNormalMap!.Bind(4);
        RenderSphere(new Vector3(0.0f, -3.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.5f));

        _cube!.Draw(Vector3.One, new Vector3(0.0f, 0.0f, 0.0f), 0.5f, (float)_stopwatch.Elapsed.TotalSeconds);
        _cube!.Draw(new Vector3(5.0f, 0.5f, 0.5f), new Vector3(0.0f, 1.0f, 0.0f), 0.5f);
        _cube!.Draw(new Vector3(0.0f, 2.0f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), 0.5f);
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

    private int _sphereVao;
    private int _sphereVbo;
    private int _sphereEbo;

    private int _indexCount;

    private void RenderSphere(Vector3 position, Vector3 color, float scale = 1.0f)
    {
        if (_sphereVao == 0)
        {
            _sphereVao = GL.GenVertexArray();
            GL.BindVertexArray(_sphereVao);

            _sphereVbo = GL.GenBuffer();
            _sphereEbo = GL.GenBuffer();

            List<Vector3> positions = [];
            List<Vector2> uv = [];
            List<Vector3> normals = [];
            List<int> indices = [];

            const int xSegments = 64;
            const int ySegments = 64;

            for (int x = 0; x <= xSegments; ++x)
            {
                for (int y = 0; y <= ySegments; ++y)
                {
                    float xSegment = x / (float)xSegments;
                    float ySegment = y / (float)ySegments;
                    float xPosition = MathF.Cos(xSegment * 2.0f * MathF.PI) * MathF.Sin(ySegment * MathF.PI);
                    float yPosition = MathF.Cos(ySegment * MathF.PI);
                    float zPosition = MathF.Sin(xSegment * 2.0f * MathF.PI) * MathF.Sin(ySegment * MathF.PI);

                    positions.Add(new Vector3(xPosition, yPosition, zPosition));
                    uv.Add(new Vector2(xSegment, ySegment));
                    normals.Add(new Vector3(xPosition, yPosition, zPosition));
                }
            }

            bool oddRow = false;

            for (int y = 0; y < ySegments; ++y)
            {
                if (!oddRow)
                {
                    for (int x = 0; x <= xSegments; ++x)
                    {
                        indices.Add(y * (xSegments + 1) + x);
                        indices.Add((y + 1) * (xSegments + 1) + x);
                    }
                }
                else
                {
                    for (int x = xSegments; x >= 0; --x)
                    {
                        indices.Add((y + 1) * (xSegments + 1) + x);
                        indices.Add(y * (xSegments + 1) + x);
                    }
                }

                oddRow = !oddRow;
            }

            _indexCount = indices.Count;

            List<float> data = [];

            for (int i = 0; i < positions.Count; i++)
            {
                data.Add(positions[i].X);
                data.Add(positions[i].Y);
                data.Add(positions[i].Z);

                if (normals.Count > 0)
                {
                    data.Add(normals[i].X);
                    data.Add(normals[i].Y);
                    data.Add(normals[i].Z);
                }

                if (uv.Count <= 0) continue;
                data.Add(uv[i].X);
                data.Add(uv[i].Y);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _sphereVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Count * sizeof(float), data.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _sphereEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(),
                BufferUsageHint.StaticDraw);

            const int stride = (3 + 2 + 3) * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        }

        Matrix4 model = Matrix4.CreateTranslation(position) * Matrix4.CreateScale(scale);

        _pbrShader!.SetMatrix4("model", model);

        GL.BindVertexArray(_sphereVao);
        GL.DrawElements(BeginMode.TriangleStrip, _indexCount, DrawElementsType.UnsignedInt, 0);
    }
}