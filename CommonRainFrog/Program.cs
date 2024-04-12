namespace CommonRainFrog;

public static class Program
{
    public static void Main(string[] args)
    {
        using RainFrogApplication application = new(1280, 720, "Common Rain Frog");
        application.Run();
    }
}