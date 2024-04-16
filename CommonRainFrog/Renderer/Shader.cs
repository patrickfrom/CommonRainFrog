using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CommonRainFrog.Renderer;

public class Shader : IDisposable
{
    private bool _disposed;

    private int _handle;

    private string _vertexPath;
    private string _fragmentPath;

    public Shader(string vertexPath, string fragmentPath)
    {
        _vertexPath = vertexPath;
        _fragmentPath = fragmentPath;
        Init(vertexPath, fragmentPath);
    }
    
    ~Shader()
    {
        if (_disposed) return;
        Console.WriteLine("GPU Resources Leak! Did you forget to call Dispose()?");
    }

    private void Init(string vertexPath, string fragmentPath)
    {
        string shaderSource = File.ReadAllText(vertexPath);
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, shaderSource);
        GL.CompileShader(vertexShader);

        shaderSource = File.ReadAllText(fragmentPath);
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, shaderSource);
        GL.CompileShader(fragmentShader);

        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);
        GL.LinkProgram(_handle);

        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }
    
    public void Use()
    {
        GL.UseProgram(_handle);
    }

    public void Rebuild()
    {
        GL.DeleteProgram(_handle);
        Init(_vertexPath, _fragmentPath);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        GL.DeleteProgram(_handle);
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void SetInt(string location, int data)
    {
        GL.Uniform1(GL.GetUniformLocation(_handle, location), data);
    }

    public void SetFloat(string location, float data)
    {
        GL.Uniform1(GL.GetUniformLocation(_handle, location), data);
    }

    public void SetVector2(string location, Vector2 data)
    {
        GL.Uniform2(GL.GetUniformLocation(_handle, location), data);
    }

    public void SetVector3(string location, Vector3 data)
    {
        GL.Uniform3(GL.GetUniformLocation(_handle, location), data);
    }

    public void SetVector4(string location, Vector4 data)
    {
        GL.Uniform4(GL.GetUniformLocation(_handle, location), data);
    }

    public void SetMatrix4(string location, Matrix4 data)
    {
        GL.UniformMatrix4(GL.GetUniformLocation(_handle, location), false, ref data);
    }

    public static int ShaderDataTypeSize(ShaderDataType type)
    {
        return type switch
        {
            ShaderDataType.Float => 4,
            ShaderDataType.Float2 => 4 * 2,
            ShaderDataType.Float3 => 4 * 3,
            ShaderDataType.Float4 => 4 * 4,
            _ => 0
        };
    }

}