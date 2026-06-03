using System.Text.RegularExpressions;
using Godot;

namespace ExtraEvents.ExtraEventsCode;

public static class Res
{
    public static string EventPortrait<T>() => EventPortrait(typeof(T).Name);
    public static string EventPortrait(string className) => ImagePath("events", "event", className);

    public static string CardPortrait<T>() => CardPortrait(typeof(T).Name);
    public static string CardPortrait(string className) => ImagePath("cards", "card", className);

    public static string RelicIcon<T>() => RelicIcon(typeof(T).Name);
    public static string RelicIcon(string className) => ImagePath("relics", "relic", className);

    public static string RelicOutline<T>() => RelicOutline(typeof(T).Name);
    public static string RelicOutline(string className) => ImagePath("relics", "relic", className);

    public static string RelicBigIcon<T>() => RelicBigIcon(typeof(T).Name);
    public static string RelicBigIcon(string className) => ImagePath("relics", "relic", className);

    private static string ImagePath(string categoryFolder, string categorySingular, string className)
    {
        var modSnake = PascalToSnake(Entry.ModId);
        var classSnake = PascalToSnake(className);
        var filename = $"{modSnake}_{categorySingular}_{classSnake}.png";
        var specific = $"{Entry.ResPath}/images/{categoryFolder}/{filename}";
        var fallback = $"{Entry.ResPath}/images/{categoryFolder}/common.png";
        return ResourceLoader.Exists(specific) ? specific : fallback;
    }

    private static string PascalToSnake(string pascal)
    {
        var result = Regex.Replace(pascal, "([a-z])([A-Z])", "$1_$2");
        return result.ToLowerInvariant();
    }
}
