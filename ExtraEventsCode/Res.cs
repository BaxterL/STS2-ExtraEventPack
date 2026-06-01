using Godot;

namespace ExtraEvents.ExtraEventsCode;

public static class Res
{
    public static string EventPortrait<T>() => ImagePath("events", typeof(T).Name);
    public static string EventPortrait(string className) => ImagePath("events", className);

    public static string CardPortrait<T>() => ImagePath("cards", typeof(T).Name);
    public static string CardPortrait(string className) => ImagePath("cards", className);

    public static string RelicIcon<T>() => ImagePath("relics", typeof(T).Name);
    public static string RelicIcon(string className) => ImagePath("relics", className);

    public static string RelicOutline<T>() => ImagePath("relics", typeof(T).Name);
    public static string RelicOutline(string className) => ImagePath("relics", className);

    public static string RelicBigIcon<T>() => ImagePath("relics", typeof(T).Name);
    public static string RelicBigIcon(string className) => ImagePath("relics", className);

    private static string ImagePath(string category, string className)
    {
        var specific = $"{Entry.ResPath}/images/{category}/{className}.png";
        var fallback = $"{Entry.ResPath}/images/{category}/common.png";
        return ResourceLoader.Exists(specific) ? specific : fallback;
    }
}
