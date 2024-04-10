using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace CommonRainFrog.Renderer;

public class Shader : IDisposable
{
    private bool _disposed;
    
    private readonly int _handle;
    
    public Shader(string vertexPath, string fragmentPath)
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
    
    public Shader(string computePath)
    {
        string shaderSource = File.ReadAllText(computePath);
        int computeShader = GL.CreateShader(ShaderType.ComputeShader);
        GL.ShaderSource(computeShader, shaderSource);
        GL.CompileShader(computeShader);
        
        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, computeShader);
        GL.LinkProgram(_handle);
        
        GL.DetachShader(_handle, computeShader);
        GL.DeleteShader(computeShader);
    }


    ~Shader()
    {
        if (_disposed) return;
        Console.WriteLine("GPU Resources Leak! Did you forget to call Dispose()?");
    }

    public void Use()
    {
        GL.UseProgram(_handle);
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

    public void SetInt(string location, int value)
    {
        GL.Uniform1(GL.GetUniformLocation(_handle, location), value);
    }

    public void SetFloat(string location, float value)
    {
        GL.Uniform1(GL.GetUniformLocation(_handle, location), value);
    }   
    
    public void SetVector2(string location, Vector2 vector2)
    {
        GL.Uniform2(GL.GetUniformLocation(_handle, location), vector2);
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