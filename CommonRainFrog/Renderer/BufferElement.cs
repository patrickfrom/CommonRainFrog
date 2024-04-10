namespace CommonRainFrog.Renderer;

public struct BufferElement
{
    public string Name;
    public readonly ShaderDataType Type;
    public readonly int Size;
    public int Offset;
    public readonly bool Normalized;

    public BufferElement(ShaderDataType type, string name, bool normalized = false)
    {
        Name = name;
        Type = type;
        Size = Shader.ShaderDataTypeSize(Type);
        Offset = 0;
        Normalized = normalized;
    }
    
    public int GetComponentCount() 
    {
        return Type switch
        {
            ShaderDataType.Float => 1,
            ShaderDataType.Float2 => 2,
            ShaderDataType.Float3 => 3,
            ShaderDataType.Float4 => 4,
            _ => 0
        };
    }
}