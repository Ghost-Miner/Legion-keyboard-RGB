using System.IO;

internal class MessageLogger
{
    private static StreamWriter? outputLogStreamwriter;

    public static void Init (string _fullFilePath)
    {
        return;

        outputLogStreamwriter = new StreamWriter(_fullFilePath, append: true)
        {
            AutoFlush = true
        };

        outputLogStreamwriter.WriteLine($"\n-- Log started on {DateTime.Now.ToLongDateString()} at {DateTime.Now.ToString("HH:mm:ss")} --");
    }

    public static void LogMessage (string _message)
    {
        return;

        if (outputLogStreamwriter == null)
        {
            throw new InvalidOperationException("MessageLogger not initialized! Call MessageLogger.Init() first!");
        }

        outputLogStreamwriter.WriteLine(_message);
    }

    public static void AppQuit()
    {
        return;

        if (outputLogStreamwriter != null)
        {
            outputLogStreamwriter.WriteLine($"-- Log ended on {DateTime.Now.ToLongDateString()} at {DateTime.Now.ToString("HH:mm:ss")} --");
            outputLogStreamwriter.Dispose();
            outputLogStreamwriter = null;
        }
    }
}    