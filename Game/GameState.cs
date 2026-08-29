namespace HighFog;

/// <summary>Persistent narrative switches for the first playable chapter.</summary>
public sealed class GameState
{
    public bool MetClara { get; set; }
    public bool PoliceStationUnlocked { get; set; }
    public bool FoundBasement { get; set; }
    public bool FoundGun { get; set; }
    public bool FirstWalkerDefeated { get; set; }
    public bool ReadProjectHaze { get; set; }
    public bool SawFogSilhouette { get; set; }
    public string Objective { get; set; } = "FIND SOMEONE WHO KNOWS WHAT HAPPENED.";
}

public enum ScreenState
{
    Title,
    MainMenu,
    Intro,
    Playing,
    Paused,
    Inventory,
    Dead,
    Ending
}
