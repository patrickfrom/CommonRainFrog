using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace CommonRainFrog.Renderer;

public class Skybox
{
    private int _textureId;
    private readonly Shader _shader;
    private readonly VertexArray _vao;
    private readonly VertexBuffer<float> _vbo;
    private readonly IndexBuffer _ebo;
    
    private readonly float[] _vertices =
    [
        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, 1.0f, -1.0f,
        -1.0f, 1.0f, -1.0f,

        -1.0f, -1.0f, 1.0f,
        1.0f, -1.0f, 1.0f,
        1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 1.0f,

        -1.0f, 1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f, 1.0f,
        -1.0f, 1.0f, 1.0f,

        1.0f, 1.0f, 1.0f,
        1.0f, 1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, 1.0f,

        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, 1.0f,
        -1.0f, -1.0f, 1.0f,

        1.0f, 1.0f, -1.0f,
        1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, -1.0f,
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

    public Skybox(Shader shader)
    {
        _shader = shader;

        _vao = new VertexArray();
        _vao.Bind();

        _vbo = new VertexBuffer<float>(_vertices, _vertices.Length * sizeof(float));
        _vbo.SetLayout(new BufferLayout(new[]
        {
            new BufferElement(ShaderDataType.Float3, "aPosition"),
        }));

        _ebo = new IndexBuffer(_indices, _indices.Length);

        _vao.AddVertexBuffer(ref _vbo);
    }

    public void SetTexture(IReadOnlyList<string> imagePaths)
    {
        GL.DeleteTexture(_textureId);
        
        _textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, _textureId);

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        
        for (var i = 0; i < imagePaths.Count; i++)
        {
            var imageBuffer = File.ReadAllBytes(imagePaths[i]);
            var image = ImageResult.FromMemory(imageBuffer);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb, image.Width,
                image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, image.Data);
        }

        GL.BindTexture(TextureTarget.TextureCubeMap, 0);
    }

    public void Draw()
    {
        _shader.Use();
        _shader.SetInt("skybox", 0);
        
        _vao.Bind();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, _textureId);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose()
    {
        GL.DeleteTexture(_textureId);
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
    }
}