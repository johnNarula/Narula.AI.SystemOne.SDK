using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Test;

// Entry point for Clef.exe: manage settings and run samples. Unit tests run via `dotnet test`.
var live = args.Contains("--live");
var rest = args.Where(a => a != "--live").ToArray();
var store = new SettingsStore();   // Clef.sqlite.config next to the executable

switch (rest.ElementAtOrDefault(0)?.ToLowerInvariant())
{
    case "init":
        Console.WriteLine($"Config ready: {store.FilePath}");
        return 0;
    case "config":
        return ConfigCommands.Run(store, rest.Skip(1).ToArray());
    case "sample":
        return await ClefSamples.RunAsync(store, rest.Skip(1).ToArray(), live);
    default:
        Console.WriteLine("""
            Clef.exe
              init                           create/open Clef.sqlite.config
              config show | path             list settings (secrets masked) | print file path
              config set <key> <value>       change a setting
              sample text|noul|score         dry run (prints request); add --live to call the API
              sample image <file>            image + yes/no and choice questions
              sample video <frame> ...       video as frames (needs a SystemOne-compatible provider)
            """);
        return 0;
}
