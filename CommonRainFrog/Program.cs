namespace CommonRainFrog;

public static class Program
{
    public static void Main(string[] args)
    {
        using RainFrogApplication application = new(512, 512, "Common Rain Frog");
        application.Run();
    }
}