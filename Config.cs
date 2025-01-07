using RedLoader;

namespace StrippedWood;

public static class Config
{
    public static ConfigCategory Category { get; private set; }

    //public static ConfigEntry<bool> SomeEntry { get; private set; }

    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("StrippedWood", "StrippedWood", "StrippedWood.cfg");

        // SomeEntry = Category.CreateEntry(
        //     "some_entry",
        //     true,
        //     "Some entry",
        //     "Some entry that does some stuff.");
    }

    public static void OnSettingsUiClosed()
    {
    }
}