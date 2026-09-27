namespace Shop.Api.Startup;

/// <summary>
/// Operator notices from the <c>Notices</c> configuration section, printed before anything
/// else at startup so they are the first lines of the log.
/// </summary>
public static class Notices
{
    public static void Print(IConfiguration configuration, TextWriter output)
    {
        foreach (var notice in configuration.GetSection("Notices").Get<string[]>() ?? [])
            output.WriteLine($"ERROR {notice}");

        output.Flush();
    }
}
