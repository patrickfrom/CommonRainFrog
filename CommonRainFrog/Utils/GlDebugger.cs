using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace CommonRainFrog.Utils;

public static class GlDebugger
{
    public static void Init()
    {
        GL.Enable(EnableCap.DebugOutput);
        
        GL.DebugMessageCallback(DebugMessageCallback, IntPtr.Zero);
    }

    private static void DebugMessageCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr pMessage, IntPtr userparam)
    {
        var message = Marshal.PtrToStringAnsi(pMessage, length);

        Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
        
        if (type == DebugType.DebugTypeError)
        {
            throw new Exception(message);
        }
    }
}