using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Renderer;

public class VertexArray
{
    private readonly int _rendererId = GL.GenVertexArray();

    public void Bind()
    {
        GL.BindVertexArray(_rendererId);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void AddVertexBuffer<T>(ref VertexBuffer<T> vertexBuffer) where T : struct
    {
        GL.BindVertexArray(_rendererId);
        vertexBuffer.Bind();

        BufferLayout layout = vertexBuffer.GetLayout();
        
        int index = 0;
        foreach (BufferElement element in layout.GetElements())
        {
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, element.GetComponentCount(), ShaderDataTypeToOpenGl(element.Type), element.Normalized, layout.GetStride(), element.Offset);
            index++;
        }
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_rendererId);
    }

    private static VertexAttribPointerType ShaderDataTypeToOpenGl(ShaderDataType type)
    {
        return type switch
        {
            ShaderDataType.Float => VertexAttribPointerType.Float,
            ShaderDataType.Float2 => VertexAttribPointerType.Float,
            ShaderDataType.Float3 => VertexAttribPointerType.Float,
            ShaderDataType.Float4 => VertexAttribPointerType.Float,
            _ => 0
        };
    }
}