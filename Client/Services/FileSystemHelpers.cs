namespace Client.Services;

public static class FileSystemHelpers
{
    public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static readonly string GameDataPath = $"{AppDataPath}{Path.DirectorySeparatorChar}Client";

    public static void EnsureDirectoryExists()
        => Directory.CreateDirectory(GameDataPath);
}