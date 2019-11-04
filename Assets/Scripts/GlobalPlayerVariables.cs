using System.Collections.Generic;

public static class GlobalPlayerVariables
{
    public static int HP{ get; set; } = 100;
    public static int AP{ get; set; } = 0;
    public static ArmorType aType{ get; set; } = ArmorType.Green;
    public static List<string> guns{ get; set; } = new List<string>(new string[2]{"fists", "pistol"});
    public static List<Ammo> ammos{ get; set; }

    public static bool save{ get; set; }
}
