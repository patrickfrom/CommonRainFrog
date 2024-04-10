using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer;

public class Quad
{
    private readonly VertexArray _vao;
    private readonly VertexBuffer<float> _vbo;
    private readonly IndexBuffer _ebo;
    
    private readonly float[] _vertices =
    [
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
        1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 0.0f, 0.0f, 1.0f
    ];

    private readonly int[] _indices =
    [
        0, 1, 2,
        2, 3, 0
    ];

    public Quad()
    {
        _vao = new VertexArray();
        _vao.Bind();

        _vbo = new VertexBuffer<float>(_vertices, _vertices.Length * sizeof(float));
        _vbo.SetLayout(new BufferLayout(new[]
        {
            new BufferElement(ShaderDataType.Float3, "aPosition"),
            new BufferElement(ShaderDataType.Float2, "aTexCoords"),
        }));

        _ebo = new IndexBuffer(_indices, _indices.Length * sizeof(int));
        
        _vao.AddVertexBuffer(ref _vbo);
    }

    public void Draw()
    {
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