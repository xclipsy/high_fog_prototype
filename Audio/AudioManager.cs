namespace HighFog;

/// <summary>
/// Central audio manager connecting game events with procedural sound synthesis.
/// </summary>
public sealed class AudioManager : IDisposable
{
    private readonly SoundSynth _synth = new();
    public string LastCue { get; private set; } = string.Empty;
    public float SfxVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.8f;

    public void StartAmbience()
    {
        _synth.StartAmbience();
        _synth.SetAmbienceVolume(0.45f * MusicVolume);
    }

    public void StopAmbience()
    {
        _synth.StopAmbience();
    }

    public void PlayCue(string cue)
    {
        LastCue = cue;
        if (SfxVolume <= 0.01f) return;

        switch (cue.ToLowerInvariant())
        {
            case "gunshot":
            case "shoot":
                _synth.PlayGunshot();
                break;
            case "dryfire":
            case "empty":
                _synth.PlayDryFire();
                break;
            case "reload":
                _synth.PlayReload();
                break;
            case "footstep":
                _synth.PlayFootstep(false);
                break;
            case "footstep_run":
                _synth.PlayFootstep(true);
                break;
            case "growl":
            case "monster_growl":
                _synth.PlayMonsterGrowl();
                break;
            case "screech":
            case "monster_scream":
            case "monster_attack":
                _synth.PlayMonsterScreech();
                break;
            case "monster_hit":
                _synth.PlayMonsterHit();
                break;
            case "player_hurt":
                _synth.PlayPlayerHurt();
                break;
            case "item_pickup":
            case "pickup":
                _synth.PlayItemPickup();
                break;
            case "door":
            case "door_open":
                _synth.PlayDoor();
                break;
            case "ui_blip":
            case "dialogue":
                _synth.PlayUiBlip();
                break;
            case "ui_select":
            case "menu_select":
                _synth.PlayUiSelect();
                break;
            case "page_turn":
            case "document":
                _synth.PlayPageTurn();
                break;
        }
    }

    public void Dispose()
    {
        _synth.Dispose();
    }
}
