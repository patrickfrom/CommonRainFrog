using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer;

public class IndexBuffer
{
    private readonly int _rendererId;
    private int _count;
    
    public IndexBuffer(int[] indices, int count)
    {
        _rendererId = GL.GenBuffer();
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _rendererId);
        GL.BufferData(BufferTarget.ElementArrayBuffer, count * sizeof(int), indices, BufferUsageHint.StaticDraw);

        _count = count;
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _rendererId);
    }
    
    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _rendererId);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_rendererId);
    }
}