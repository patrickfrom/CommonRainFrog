using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer;

public class Framebuffer
{
    public readonly int Id = GL.GenFramebuffer();
    public readonly int TextureId = GL.GenTexture();

    public Framebuffer(int width, int height)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, TextureId, 0);GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedInt, 0);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
    }

    public void Resize(int width, int height)
    {
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0,
            PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
    }

    public void Dispose()
    {
        GL.DeleteTexture(TextureId);
        GL.DeleteFramebuffer(Id);
    }
}