using Narula.AI.SystemOne.SDK.Clef.Test.Config;
using Narula.AI.SystemOne.SDK.Clef.Configuration;

namespace Narula.AI.SystemOne.SDK.Clef.Test;

/// <summary>`config show|path|set` sub-commands.</summary>
public static class ConfigCommands
{
    public static int Run(SettingsStore store, string[] args)
    {
        switch (args.ElementAtOrDefault(0)?.ToLowerInvariant())
        {
            case "path":
                Console.WriteLine(store.FilePath);
                return 0;
            case "show":
                foreach (var r in store.GetAll())
                    Console.WriteLine($"{r.Key,-28} = {Mask(r)}   # {r.Description}");
                return 0;
            case "set" when args.Length >= 3:
                store.Set(args[1], args[2]);
                Console.WriteLine($"Updated {args[1]}.");
                return 0;
            default:
                Console.WriteLine("Usage: config show | path | set <key> <value>");
                return 1;
        }
    }

    // Never echo secrets: show only whether one is set.
    private static string Mask(SettingRow r) =>
        r.IsSecret ? (r.Value.Length == 0 ? "(not set)" : "(set, hidden)") : r.Value;
}
