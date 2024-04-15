using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer;

public class Renderbuffer
{
    public readonly int Id = GL.GenRenderbuffer();

    public Renderbuffer(int width, int height)
    {
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Id);

        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width,
            height);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Id);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer, Id);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }

    public void Bind()
    {
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Id);
    }

    public void Resize(int width, int height)
    {
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Id);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width,
            height);
    }

    public void Dispose()
    {
        GL.DeleteRenderbuffer(Id);
    }
}