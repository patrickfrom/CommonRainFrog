using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer.Meshes;

public class Quad
{
    private readonly Shader _shader;
    private readonly VertexArray _vao;
    private readonly VertexBuffer<float> _vbo;

    private readonly float[] _vertices =
    [
        1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
        1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
    ];

    public Quad(Shader shader)
    {
        _shader = shader;

        _vao = new VertexArray();
        _vao.Bind();

        _vbo = new VertexBuffer<float>(_vertices, _vertices.Length * sizeof(float));
        _vbo.SetLayout(new BufferLayout(new[]
        {
            new BufferElement(ShaderDataType.Float3, "aPosition"),
            new BufferElement(ShaderDataType.Float2, "aTexCoords"),
        }));

        _vao.AddVertexBuffer(ref _vbo);
    }

    public void Draw()
    {
        _shader.Use();
        _vao.Bind();
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, _vertices.Length);
    }

    public void Dispose()
    {
        _vao.Dispose();
        _vbo.Dispose();
    }
}