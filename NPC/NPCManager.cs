using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Manages all NPCs in Grayhaven (Clara, Arthur, Officer Vance, Father Thomas)
/// with multi-stage narrative dialogue branching and gameplay rewards.
/// </summary>
public sealed class NPCManager
{
    public NPC Clara { get; }
    public NPC Arthur { get; }
    public NPC OfficerVance { get; }
    public NPC FatherThomas { get; }

    public IReadOnlyList<NPC> AllNPCs => new[] { Clara, Arthur, OfficerVance, FatherThomas };

    // Dialogue / Reward tracking flags
    private bool _arthurGaveMedkit;
    private bool _vanceGaveAmmo;

    public NPCManager()
    {
        // 1. Clara (Town Square)
        Clara = new NPC("clara", "Clara", Personality.Nervous, new Vector3(-2.2f, 0f, -1.2f), new Color(78, 62, 54), new Color(135, 45, 40))
        {
            Facing = 0.85f
        };

        // 2. Arthur (West Residential Porch)
        Arthur = new NPC("arthur", "Arthur Miller", Personality.Sad, new Vector3(-15.2f, 0f, -9.8f), new Color(42, 65, 82), new Color(90, 85, 75))
        {
            Facing = 0f
        };

        // 3. Officer Vance (Inside Police Reception Hallway)
        OfficerVance = new NPC("vance", "Officer Vance", Personality.Aggressive, new Vector3(13.5f, 0f, -8.5f), new Color(38, 55, 75), new Color(175, 155, 90))
        {
            Facing = 1.57f
        };

        // 4. Father Thomas (Northern Church Barricade)
        FatherThomas = new NPC("thomas", "Father Thomas", Personality.Mysterious, new Vector3(6.0f, 0f, -32.0f), new Color(28, 28, 32), new Color(195, 175, 95))
        {
            Facing = 3.14f
        };
    }

    public void InteractWithClara(HighFogGame game)
    {
        var state = game.State;
        var dialogue = game.Dialogue;

        if (state.ReadProjectHaze)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("CLARA", "You found the report?! Project Haze... I remember the factory trucks rolling through at 3 AM every Tuesday."),
                new DialogueLine("CLARA", "They didn't create the fog from chemicals... they drilled into something ancient sleeping beneath the valley."),
                new DialogueLine("CLARA", "Arthur lost his son down there, and Father Thomas has been preaching by the church gate. We need to find a way out before the rift widens.")
            });
        }
        else if (state.FirstWalkerDefeated)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("CLARA", "You... you actually killed one of those things?! I heard the gunshots echoing through the square!"),
                new DialogueLine("CLARA", "So they bleed... they can die! But there were more deep beneath the precinct. Did you check the basement safe?"),
                new DialogueLine("CLARA", "Please find out what caused this. Before whatever is down there comes up for all of us.")
            });
        }
        else if (state.FoundGun)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("CLARA", "You found a weapon?! Keep it steady... the sound will draw whatever is lurking in the fog."),
                new DialogueLine("CLARA", "I heard metallic banging coming from the precinct corridors right after you went in."),
                new DialogueLine("CLARA", "Officer Vance was in the station hallway last time I saw him. If he's still alive, he might know what's in the basement.")
            });
        }
        else if (state.MetClara)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("CLARA", "I told you... avoid the southern road toward the hospital. The fog is so thick you can't see your own hands."),
                new DialogueLine("CLARA", "The police precinct is to the east. Use the brass key I gave you to unlock the front entrance."),
                new DialogueLine("CLARA", "Be careful in there... nobody came out after 11 PM.")
            });
        }
        else
        {
            // Initial Meeting
            state.MetClara = true;
            dialogue.Start(new[]
            {
                new DialogueLine("CLARA", "S-stay back! Oh... you're... you're human. You came from outside the valley?"),
                new DialogueLine("CLARA", "You shouldn't have come to Grayhaven. The fog rolled in from the old factory at dusk, and anyone caught outside... turned."),
                new DialogueLine("CLARA", "Whatever you do, don't go south toward the hospital. You can hear them weeping in the mist."),
                new DialogueLine("CLARA", "If you want to survive, head east to the Police Precinct. Chief Vance barricaded the building before something breached the lower floor."),
                new DialogueLine("CLARA", "Take this precinct key. Find a weapon inside before the fog closes in completely.")
                {
                    OnCompleted = g =>
                    {
                        g.State.PoliceStationUnlocked = true;
                        g.Inventory.Add(Item.CreatePoliceKey());
                        g.Audio.PlayCue("item_pickup");
                        g.State.Objective = "UNLOCK AND INVESTIGATE THE POLICE PRECINCT (EAST).";
                    }
                }
            });
        }
    }

    public void InteractWithArthur(HighFogGame game)
    {
        var dialogue = game.Dialogue;
        var state = game.State;

        if (state.ReadProjectHaze)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("ARTHUR", "Project Haze... so my boy wasn't crazy. He said the drills hit something that hummed like a choir of glass."),
                new DialogueLine("ARTHUR", "The company told us it was a natural gas leak. Liars. All of them. Take care of yourself, stranger.")
            });
        }
        else if (!_arthurGaveMedkit)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("ARTHUR", "Who's there?! Don't come any closer... I've boarded up the doors. My boy went to the factory shift and never came home."),
                new DialogueLine("ARTHUR", "The air out here burns my lungs. You look battered... here, take this first aid kit from my medicine cabinet."),
                new DialogueLine("ARTHUR", "If you go east to the precinct, watch the shadows under the streetlights. They don't cast right.")
                {
                    OnCompleted = g =>
                    {
                        _arthurGaveMedkit = true;
                        g.Inventory.Add(Item.CreateMedkit());
                        g.Audio.PlayCue("item_pickup");
                        g.ShowToast("ARTHUR GAVE YOU A FIRST AID KIT");
                    }
                }
            });
        }
        else
        {
            dialogue.Start(new[]
            {
                new DialogueLine("ARTHUR", "I'm staying right here on this porch. If my boy comes walking out of the mist... I want to be here to open the door."),
                new DialogueLine("ARTHUR", "Old Father Thomas has been standing by the church gate all night. He claims the fog has a pulse.")
            });
        }
    }

    public void InteractWithOfficerVance(HighFogGame game)
    {
        var dialogue = game.Dialogue;
        var state = game.State;

        if (state.FirstWalkerDefeated)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("OFFICER VANCE", "You... you put that monster down in the basement?! I heard the shots!"),
                new DialogueLine("OFFICER VANCE", "Good riddance. That thing used to be Deputy Jenkins. It tore through our barricade in seconds."),
                new DialogueLine("OFFICER VANCE", "Did you find the classified file in the safe? The chief took orders from the factory supervisors before everything went dark.")
            });
        }
        else if (state.FoundGun)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("OFFICER VANCE", "You found the service 9mm! Keep your finger off the trigger until you've got a clear line."),
                new DialogueLine("OFFICER VANCE", "Aim for the torso or head. The creature in the basement doesn't react to pain normally—it only staggers when hit solid.")
            });
        }
        else if (!_vanceGaveAmmo)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("OFFICER VANCE", "Hold it right there! Stop! ...Damn it, you startled me. Thought another one broke through."),
                new DialogueLine("OFFICER VANCE", "I took a slash across the ribs when the sub-level door gave way. The chief's 9mm is down on the basement storage table."),
                new DialogueLine("OFFICER VANCE", "Here... take these spare 9mm rounds from my belt. You'll need every single bullet if you open that sub-level hatch.")
                {
                    OnCompleted = g =>
                    {
                        _vanceGaveAmmo = true;
                        g.Inventory.Add(Item.CreateAmmo(12));
                        g.Audio.PlayCue("item_pickup");
                        g.ShowToast("OFFICER VANCE GAVE YOU 12 9MM ROUNDS");
                    }
                }
            });
        }
        else
        {
            dialogue.Start(new[]
            {
                new DialogueLine("OFFICER VANCE", "The hatch to the sub-level is in the corner of this room. Don't go down there unless you're ready to fight."),
                new DialogueLine("OFFICER VANCE", "Whatever came out of the floor... it wasn't human.")
            });
        }
    }

    public void InteractWithFatherThomas(HighFogGame game)
    {
        var dialogue = game.Dialogue;
        var state = game.State;

        if (state.ReadProjectHaze)
        {
            dialogue.Start(new[]
            {
                new DialogueLine("FATHER THOMAS", "Project Haze... man's arrogance dressed in corporate jargon. They did not create the fog; they awoke the deep void."),
                new DialogueLine("FATHER THOMAS", "The church bells will ring no more for Grayhaven. But you carry the truth. Go with caution, child of the mist.")
            });
        }
        else
        {
            dialogue.Start(new[]
            {
                new DialogueLine("FATHER THOMAS", "Hear the silence in the valley? It is not peace. It is the breath of the mountain holding back its wrath."),
                new DialogueLine("FATHER THOMAS", "The northern road to the church is choked with fog. The bells went silent when the ground trembled at dusk."),
                new DialogueLine("FATHER THOMAS", "The answers you seek lie not in the sky, but in the iron belly of the earth beneath the precinct.")
            });
        }
    }
}
