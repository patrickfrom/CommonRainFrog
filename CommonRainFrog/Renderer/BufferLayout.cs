namespace CommonRainFrog.Renderer;

public struct BufferLayout
{
    private readonly List<BufferElement> _elements;
    private int _stride;
    
    public BufferLayout(IEnumerable<BufferElement> elements)
    {
        _elements = new List<BufferElement>(elements);

        CalculateOffsetAndStride();
    }

    public int GetStride()
    {
        return _stride;
    }

    private void CalculateOffsetAndStride()
    {
        int offset = 0;
        _stride = 0;

        for (int i = 0; i < _elements.Count; i++)
        {
            BufferElement element = _elements[i];
            
            element.Offset = offset;
            offset += element.Size;
            _stride += element.Size;

            _elements[i] = element;
        }
    }

    public IEnumerable<BufferElement> GetElements()
    {
        return _elements;
    }
}