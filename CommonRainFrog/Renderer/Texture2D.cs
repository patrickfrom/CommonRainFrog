using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace CommonRainFrog.Renderer;

public class Texture2D
{
    private readonly int _textureId = GL.GenTexture();

    public Texture2D(string imagePath)
    {
        GL.BindTexture(TextureTarget.Texture2D, _textureId);

        GL.TextureParameter(_textureId, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(_textureId, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(_textureId, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TextureParameter(_textureId, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        byte[] imageBuffer = File.ReadAllBytes(imagePath);
        ImageResult image = ImageResult.FromMemory(imageBuffer, ColorComponents.RedGreenBlueAlpha);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(int index = 0)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + index);
        GL.BindTexture(TextureTarget.Texture2D, _textureId);
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        GL.DeleteTexture(_textureId);
    }
}