using OpenTK.Mathematics;

namespace CommonRainFrog.Renderer.Lights;

public struct PointLight(Vector3 position, Vector3 color)
{
    public Vector3 Position = position;
    public Vector3 Color = color;
}