using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CommonRainFrog.Renderer.Meshes;

public class Sphere
{
    private readonly Shader _shader;
    private readonly VertexArray _vao;
    private readonly VertexBuffer<float> _vbo;
    private readonly IndexBuffer _ebo;

    private readonly int _indexCount;

    public Sphere(Shader shader)
    {
        _shader = shader;
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

        _vao = new VertexArray();
        _vao.Bind();

        _vbo = new VertexBuffer<float>(data, data.Count * sizeof(float));
        _vbo.SetLayout(new BufferLayout(new[]
        {
            new BufferElement(ShaderDataType.Float3, "aPosition"),
            new BufferElement(ShaderDataType.Float3, "aNormal"),
            new BufferElement(ShaderDataType.Float2, "aTexCoords"),
        }));
        
        _ebo = new IndexBuffer(indices.ToArray(), indices.Count);

        _vao.AddVertexBuffer(ref _vbo);
    }

    public void Draw(Vector3 position, float scale = 1.0f)
    {
        Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position) ;

        _shader.SetMatrix4("model", model);

        _vao.Bind();
        GL.DrawElements(BeginMode.TriangleStrip, _indexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
    }
}