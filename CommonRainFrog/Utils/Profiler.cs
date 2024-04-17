using System.Numerics;
using ImGuiNET;

namespace CommonRainFrog.Utils;

public class Profiler
{
    private readonly float[] _frameTimes = new float[50];
    private int _frameIndex;
    
    public void Render()
    {
        ImGui.Begin("Profiler");
        ImGui.PlotHistogram("FPS: ", ref _frameTimes[0], _frameTimes.Length, _frameIndex, $"FPS {ImGui.GetIO().Framerate:0}", 0.0f, 500.0f, new Vector2(ImGui.GetContentRegionAvail().X, 120.0f));
        ImGui.PlotLines("FPS: ", ref _frameTimes[0], _frameTimes.Length, _frameIndex, $"FPS {ImGui.GetIO().Framerate:0}", 0.0f, 500.0f, new Vector2(ImGui.GetContentRegionAvail().X, 120.0f));
        ImGui.End();

        _frameTimes[_frameIndex] = ImGui.GetIO().Framerate;
        _frameIndex = (_frameIndex + 1) % _frameTimes.Length;
    }
}