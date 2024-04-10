using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer;

public class VertexBuffer<T> where T : struct
{
    private readonly int _rendererId = GL.GenBuffer();
    private BufferLayout _layout;
        
    public VertexBuffer(IEnumerable<T> vertices, int size)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _rendererId);
        GL.BufferData(BufferTarget.ArrayBuffer, size, vertices.ToArray(), BufferUsageHint.StaticDraw);
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _rendererId);
    }
    
    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public BufferLayout GetLayout()
    {
        return _layout;
    }

    public void SetLayout(BufferLayout layout)
    {
        _layout = layout;
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_rendererId);
    }
}