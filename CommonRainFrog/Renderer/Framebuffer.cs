using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer;

public class Framebuffer
{
    public readonly int Id = GL.GenFramebuffer();
    public readonly int TextureId = GL.GenTexture(); //TODO: Make this into Texture Class

    private int _width;
    private int _height;
    
    public Framebuffer(int width, int height)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        
        _width = width;
        _height = height;
    }
    
    // TODO: Also move this into texture
    public void SetMinMagFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
    {
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
    }
    
    public void SetFramebufferTexture2D(FramebufferAttachment framebufferAttachment)
    {
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, framebufferAttachment,
            TextureTarget.Texture2D, TextureId, 0);
    }

    // TODO: Move this into Texture
    public void SetTextureImage2D(PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType)
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, _width, _height, 0, pixelFormat,
            pixelType, 0);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
    }

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        // TODO: I once again ask myself to add this into Texture
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