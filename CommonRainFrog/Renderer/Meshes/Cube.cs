using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CommonRainFrog.Renderer.Meshes;

public class Cube
{
    private readonly Shader _shader;
    private readonly VertexArray _vao;
    private readonly VertexBuffer<float> _vbo;
    private readonly IndexBuffer _ebo;

    private readonly float[] _vertices =
    [
        -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f,
        0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f,
        0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,
        -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f,

        -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
        0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
        0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
        -0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,

        -0.5f, 0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
        -0.5f, -0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
        -0.5f, 0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f,

        0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
        0.5f, 0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
        0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
        0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f,

        -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f,
        0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f,
        -0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f,

        0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
        0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f,
        -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
        -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
    ];

    private readonly int[] _indices =
    [
        0, 3, 1,
        3, 2, 1,

        4, 5, 7,
        7, 5, 6,

        8, 9, 11,
        11, 9, 10,

        12, 15, 13,
        15, 14, 13,

        16, 17, 19,
        19, 17, 18,

        20, 23, 21,
        23, 22, 21,
    ];

    public Cube(Shader shader)
    {
        _shader = shader;

        _vao = new VertexArray();
        _vao.Bind();

        _vbo = new VertexBuffer<float>(_vertices, _vertices.Length * sizeof(float));
        _vbo.SetLayout(new BufferLayout(new[]
        {
            new BufferElement(ShaderDataType.Float3, "aPosition"),
            new BufferElement(ShaderDataType.Float3, "aNormal"),
            new BufferElement(ShaderDataType.Float2, "aTexCoords"),
        }));

        _ebo = new IndexBuffer(_indices, _indices.Length * sizeof(int));

        _vao.AddVertexBuffer(ref _vbo);
    }

    public void Draw(Vector3 position, float scale = 1.0f, float angle = 0.0f)
    {
        Draw(position, new Vector3(1.0f), scale, angle);
    }

    public void Draw(Vector3 position, Vector3 color, float scale = 1.0f, float angle = 0.0f)
    {
        _shader.Use();
        Matrix4 model = Matrix4.CreateRotationX(angle) * Matrix4.CreateRotationY(angle) * Matrix4.CreateTranslation(position) * Matrix4.CreateScale(scale);

        _shader.SetMatrix4("model", model);
        _shader.SetVector3("albedo", color);
        _vao.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
    }
}