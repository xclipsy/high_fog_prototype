using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

/// <summary>
/// Low-poly humanoid, NPC, monster, and prop renderer matching the authentic Nintendo 64 / PS1 era survival horror aesthetic.
/// </summary>
public sealed class CharacterRenderer
{
    private readonly PrimitiveRenderer _prim;
    private readonly TextureGenerator _textures;

    public CharacterRenderer(PrimitiveRenderer prim, TextureGenerator textures)
    {
        _prim = prim;
        _textures = textures;
    }

    public void DrawPlayer(Player player)
    {
        if (player.Health <= 0f)
        {
            // Collapsed dead on ground
            _prim.BoxRotated(player.Position + new Vector3(0, 0.2f, 0), new Vector3(0.6f, 0.35f, 1.6f), player.Facing, new Color(42, 45, 45));
            return;
        }

        Vector3 pos = player.Position;
        float facing = player.Facing;
        float anim = player.AnimationTimer;
        bool isMoving = player.IsRunning || (player.FootstepTimer > 0f && player.FootstepTimer < 0.45f);

        float legSwing = isMoving ? MathF.Sin(anim) * 0.45f : 0f;
        float armSwing = isMoving ? MathF.Cos(anim) * 0.4f : 0f;
        float bob = isMoving ? MathF.Abs(MathF.Sin(anim * 2f)) * 0.06f : MathF.Sin(anim) * 0.02f;

        Matrix rot = Matrix.CreateRotationY(facing);

        Color coatColor = new(56, 52, 46);
        Color coatLapel = new(68, 62, 54);
        Color pantsColor = new(36, 40, 42);
        Color skinColor = new(195, 160, 135);
        Color hatColor = new(38, 36, 32);
        Color beltColor = new(24, 22, 20);
        Color gunMetal = new(75, 80, 85);

        Vector3 basePos = pos + new Vector3(0, bob, 0);

        // Legs
        Vector3 leftLegOffset = Vector3.Transform(new Vector3(-0.16f, 0.42f, legSwing * 0.25f), rot);
        Vector3 rightLegOffset = Vector3.Transform(new Vector3(0.16f, 0.42f, -legSwing * 0.25f), rot);
        _prim.BoxRotated(basePos + leftLegOffset, new Vector3(0.18f, 0.78f, 0.22f), facing, pantsColor);
        _prim.BoxRotated(basePos + rightLegOffset, new Vector3(0.18f, 0.78f, 0.22f), facing, pantsColor);

        // Shoes
        Vector3 leftShoe = Vector3.Transform(new Vector3(-0.16f, 0.08f, legSwing * 0.25f - 0.04f), rot);
        Vector3 rightShoe = Vector3.Transform(new Vector3(0.16f, 0.08f, -legSwing * 0.25f - 0.04f), rot);
        _prim.BoxRotated(basePos + leftShoe, new Vector3(0.2f, 0.16f, 0.32f), facing, new Color(20, 20, 20));
        _prim.BoxRotated(basePos + rightShoe, new Vector3(0.2f, 0.16f, 0.32f), facing, new Color(20, 20, 20));

        // Torso / Trenchcoat
        Vector3 torsoPos = basePos + Vector3.Transform(new Vector3(0, 1.08f, 0), rot);
        _prim.BoxRotated(torsoPos, new Vector3(0.54f, 0.65f, 0.36f), facing, coatColor);
        // Coat Lapel detail
        _prim.BoxRotated(torsoPos + Vector3.Transform(new Vector3(0, 0.08f, -0.19f), rot), new Vector3(0.26f, 0.45f, 0.04f), facing, coatLapel);
        // Belt
        _prim.BoxRotated(torsoPos + Vector3.Transform(new Vector3(0, -0.26f, 0), rot), new Vector3(0.56f, 0.08f, 0.38f), facing, beltColor);

        // Coat Skirt
        Vector3 skirtPos = basePos + Vector3.Transform(new Vector3(0, 0.68f, 0), rot);
        _prim.BoxRotated(skirtPos, new Vector3(0.58f, 0.42f, 0.4f), facing, coatColor);

        // Head
        Vector3 headPos = basePos + Vector3.Transform(new Vector3(0, 1.55f, 0), rot);
        _prim.BoxRotated(headPos, new Vector3(0.28f, 0.3f, 0.28f), facing, skinColor);

        // Fedora Hat
        Vector3 hatPos = basePos + Vector3.Transform(new Vector3(0, 1.72f, 0), rot);
        _prim.BoxRotated(hatPos, new Vector3(0.46f, 0.08f, 0.46f), facing, hatColor);
        _prim.BoxRotated(hatPos + new Vector3(0, 0.09f, 0), new Vector3(0.28f, 0.14f, 0.28f), facing, hatColor);

        // Arms & Weapon Handling
        if (player.IsAiming || player.HasHandgun)
        {
            // Two-handed aiming pose
            Vector3 gunArmLeft = basePos + Vector3.Transform(new Vector3(-0.18f, 1.18f, -0.32f), rot);
            Vector3 gunArmRight = basePos + Vector3.Transform(new Vector3(0.14f, 1.18f, -0.38f), rot);
            _prim.BoxRotated(gunArmLeft, new Vector3(0.14f, 0.16f, 0.52f), facing, coatColor);
            _prim.BoxRotated(gunArmRight, new Vector3(0.14f, 0.16f, 0.58f), facing, coatColor);

            // Handgun Model
            Vector3 gunPos = basePos + Vector3.Transform(new Vector3(0.04f, 1.18f, -0.72f), rot);
            _prim.BoxRotated(gunPos, new Vector3(0.08f, 0.14f, 0.28f), facing, gunMetal);
            _prim.BoxRotated(gunPos + Vector3.Transform(new Vector3(0, -0.07f, 0.06f), rot), new Vector3(0.07f, 0.14f, 0.08f), facing, new Color(45, 30, 20));
        }
        else
        {
            // Natural sway
            Vector3 leftArm = basePos + Vector3.Transform(new Vector3(-0.35f, 1.05f, armSwing * 0.3f), rot);
            Vector3 rightArm = basePos + Vector3.Transform(new Vector3(0.35f, 1.05f, -armSwing * 0.3f), rot);
            _prim.BoxRotated(leftArm, new Vector3(0.16f, 0.55f, 0.18f), facing, coatColor);
            _prim.BoxRotated(rightArm, new Vector3(0.16f, 0.55f, 0.18f), facing, coatColor);
        }
    }

    public void DrawNPC(NPC npc, float time)
    {
        Vector3 pos = npc.Position;
        float facing = npc.Facing;
        Matrix rot = Matrix.CreateRotationY(facing);

        float breath = MathF.Sin(time * 2f + pos.X) * 0.015f;
        Vector3 basePos = pos + new Vector3(0, breath, 0);

        Color skinColor = new(205, 175, 150);

        switch (npc.Id)
        {
            case "clara":
                // Clara: Brown parka, red scarf, beanie
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(-0.14f, 0.38f, 0), rot), new Vector3(0.16f, 0.72f, 0.2f), facing, new Color(45, 52, 58));
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0.14f, 0.38f, 0), rot), new Vector3(0.16f, 0.72f, 0.2f), facing, new Color(45, 52, 58));
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 0.98f, 0), rot), new Vector3(0.5f, 0.6f, 0.34f), facing, npc.ClothingColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.32f, 0), rot), new Vector3(0.42f, 0.16f, 0.36f), facing, npc.AccentColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.52f, 0), rot), new Vector3(0.26f, 0.28f, 0.26f), facing, skinColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.68f, 0), rot), new Vector3(0.28f, 0.14f, 0.28f), facing, new Color(42, 46, 48));
                // Arms holding hands nervously
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(-0.28f, 0.96f, -0.08f), rot), new Vector3(0.14f, 0.48f, 0.16f), facing, npc.ClothingColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0.28f, 0.96f, -0.08f), rot), new Vector3(0.14f, 0.48f, 0.16f), facing, npc.ClothingColor);
                break;

            case "arthur":
                // Arthur: Shivering old man in wool cardigan
                float shiver = MathF.Sin(time * 16f) * 0.01f;
                Vector3 arthBase = basePos + new Vector3(shiver, 0, 0);
                _prim.BoxRotated(arthBase + Vector3.Transform(new Vector3(-0.15f, 0.38f, 0), rot), new Vector3(0.18f, 0.72f, 0.22f), facing, new Color(55, 52, 48));
                _prim.BoxRotated(arthBase + Vector3.Transform(new Vector3(0.15f, 0.38f, 0), rot), new Vector3(0.18f, 0.72f, 0.22f), facing, new Color(55, 52, 48));
                _prim.BoxRotated(arthBase + Vector3.Transform(new Vector3(0, 0.96f, 0), rot), new Vector3(0.52f, 0.58f, 0.36f), facing, npc.ClothingColor);
                // Grey hair head
                _prim.BoxRotated(arthBase + Vector3.Transform(new Vector3(0, 1.48f, 0), rot), new Vector3(0.26f, 0.26f, 0.26f), facing, new Color(185, 160, 140));
                _prim.BoxRotated(arthBase + Vector3.Transform(new Vector3(0, 1.62f, 0), rot), new Vector3(0.28f, 0.1f, 0.28f), facing, new Color(175, 175, 175)); // Grey hair
                // Folded arms
                _prim.BoxRotated(arthBase + Vector3.Transform(new Vector3(0, 0.94f, -0.16f), rot), new Vector3(0.48f, 0.18f, 0.16f), facing, npc.ClothingColor);
                break;

            case "vance":
                // Officer Vance: Injured police deputy in uniform with badge and arm bandage
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(-0.16f, 0.4f, 0), rot), new Vector3(0.18f, 0.75f, 0.22f), facing, new Color(28, 36, 48));
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0.16f, 0.4f, 0), rot), new Vector3(0.18f, 0.75f, 0.22f), facing, new Color(28, 36, 48));
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.05f, 0), rot), new Vector3(0.54f, 0.62f, 0.36f), facing, npc.ClothingColor);
                // Police Badge & Belt
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0.14f, 1.22f, -0.19f), rot), new Vector3(0.08f, 0.08f, 0.02f), facing, npc.AccentColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 0.76f, 0), rot), new Vector3(0.56f, 0.08f, 0.38f), facing, new Color(20, 20, 20));
                // Head with police cap
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.52f, 0), rot), new Vector3(0.28f, 0.28f, 0.28f), facing, skinColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.68f, 0), rot), new Vector3(0.38f, 0.1f, 0.42f), facing, new Color(22, 30, 42));
                // Left arm with white bandage
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(-0.35f, 1.02f, -0.1f), rot), new Vector3(0.16f, 0.5f, 0.18f), facing, new Color(215, 215, 215));
                // Right arm holding side
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0.32f, 0.98f, -0.06f), rot), new Vector3(0.16f, 0.5f, 0.18f), facing, npc.ClothingColor);
                break;

            case "thomas":
                // Father Thomas: Mysterious priest in dark cassock with cross pendant
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 0.55f, 0), rot), new Vector3(0.56f, 1.05f, 0.42f), facing, npc.ClothingColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.15f, 0), rot), new Vector3(0.5f, 0.55f, 0.36f), facing, npc.ClothingColor);
                // Golden Cross
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.12f, -0.2f), rot), new Vector3(0.06f, 0.22f, 0.02f), facing, npc.AccentColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.18f, -0.2f), rot), new Vector3(0.16f, 0.06f, 0.02f), facing, npc.AccentColor);
                // Hood & Head
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.54f, 0), rot), new Vector3(0.36f, 0.38f, 0.38f), facing, npc.ClothingColor);
                _prim.BoxRotated(basePos + Vector3.Transform(new Vector3(0, 1.5f, -0.08f), rot), new Vector3(0.2f, 0.2f, 0.14f), facing, new Color(155, 130, 115));
                break;
        }
    }

    public void DrawFogWalker(Enemy enemy)
    {
        if (enemy.State == EnemyState.Dead)
        {
            _prim.BoxRotated(enemy.Position + new Vector3(0, 0.12f, 0), new Vector3(0.8f, 0.2f, 1.8f), enemy.Facing, new Color(25, 28, 30, 180));
            return;
        }

        Vector3 pos = enemy.Position;
        float facing = enemy.Facing;
        float anim = enemy.AnimationTime;
        Matrix rot = Matrix.CreateRotationY(facing);

        float twitch = MathF.Sin(anim * 6f) * 0.06f;
        float limbTwitch = MathF.Sin(anim * 8f) * 0.25f;

        Color shadowFlesh = new(32, 38, 42);
        Color tatteredCloth = new(22, 26, 28);
        Color glowingEye = new(245, 45, 30);

        if (enemy.State == EnemyState.Stagger)
        {
            shadowFlesh = new Color(85, 35, 35);
        }

        // Elongated Legs
        float legSwing = (enemy.State == EnemyState.Chase) ? MathF.Sin(anim * 2f) * 0.4f : 0f;
        Vector3 leftLeg = pos + Vector3.Transform(new Vector3(-0.18f, 0.55f, legSwing * 0.35f), rot);
        Vector3 rightLeg = pos + Vector3.Transform(new Vector3(0.18f, 0.55f, -legSwing * 0.35f), rot);
        _prim.BoxRotated(leftLeg, new Vector3(0.14f, 1.05f, 0.18f), facing, shadowFlesh);
        _prim.BoxRotated(rightLeg, new Vector3(0.14f, 1.05f, 0.18f), facing, shadowFlesh);

        // Twisted Hunched Torso
        Vector3 torsoPos = pos + Vector3.Transform(new Vector3(0, 1.45f, -0.15f + twitch), rot);
        _prim.BoxRotated(torsoPos, new Vector3(0.48f, 0.85f, 0.32f), facing, tatteredCloth);

        // Distorted Head
        Vector3 headPos = pos + Vector3.Transform(new Vector3(0.06f + twitch, 2.05f, -0.32f), rot);
        _prim.BoxRotated(headPos, new Vector3(0.26f, 0.34f, 0.28f), facing + twitch * 2f, shadowFlesh);

        // Glowing red eyes
        Vector3 eyeLeft = headPos + Vector3.Transform(new Vector3(-0.06f, 0.05f, -0.15f), rot);
        Vector3 eyeRight = headPos + Vector3.Transform(new Vector3(0.06f, 0.05f, -0.15f), rot);
        _prim.BoxRotated(eyeLeft, new Vector3(0.05f, 0.05f, 0.04f), facing, glowingEye);
        _prim.BoxRotated(eyeRight, new Vector3(0.05f, 0.05f, 0.04f), facing, glowingEye);

        // Elongated Arms / Claws
        if (enemy.State == EnemyState.Attack)
        {
            Vector3 clawArm = pos + Vector3.Transform(new Vector3(0.22f, 1.55f, -0.65f), rot);
            _prim.BoxRotated(clawArm, new Vector3(0.14f, 0.16f, 0.95f), facing, shadowFlesh);
        }
        else
        {
            Vector3 leftArm = pos + Vector3.Transform(new Vector3(-0.35f, 1.15f, limbTwitch * 0.2f), rot);
            Vector3 rightArm = pos + Vector3.Transform(new Vector3(0.35f, 1.15f, -limbTwitch * 0.2f), rot);
            _prim.BoxRotated(leftArm, new Vector3(0.12f, 0.95f, 0.14f), facing, shadowFlesh);
            _prim.BoxRotated(rightArm, new Vector3(0.12f, 0.95f, 0.14f), facing, shadowFlesh);
        }
    }

    public void DrawItemPickup(IInteractable interactable, float time)
    {
        if (!interactable.IsAvailable) return;

        if (interactable is ItemPickupInteractable itemPickup)
        {
            float bob = MathF.Sin(time * 3f) * 0.08f;
            float rot = time * 2.2f;
            Vector3 pos = itemPickup.Position + new Vector3(0, 0.45f + bob, 0);

            switch (itemPickup.Item.Type)
            {
                case ItemType.Handgun:
                    _prim.BoxRotated(pos, new Vector3(0.12f, 0.2f, 0.45f), rot, new Color(90, 95, 100));
                    _prim.BoxRotated(pos + new Vector3(0, -0.1f, 0.1f), new Vector3(0.1f, 0.22f, 0.12f), rot, new Color(55, 38, 25));
                    break;

                case ItemType.HandgunAmmo:
                    _prim.BoxRotated(pos, new Vector3(0.32f, 0.24f, 0.24f), rot, new Color(135, 105, 50));
                    _prim.BoxRotated(pos + new Vector3(0, 0.05f, 0), new Vector3(0.34f, 0.06f, 0.26f), rot, new Color(185, 55, 40));
                    break;

                case ItemType.PoliceKey:
                case ItemType.BasementKey:
                    _prim.BoxRotated(pos, new Vector3(0.08f, 0.08f, 0.38f), rot, new Color(205, 175, 75));
                    _prim.BoxRotated(pos + new Vector3(0, 0, 0.18f), new Vector3(0.24f, 0.06f, 0.18f), rot, new Color(215, 185, 85));
                    break;

                case ItemType.Medkit:
                    _prim.BoxRotated(pos, new Vector3(0.42f, 0.32f, 0.22f), rot, new Color(230, 230, 230));
                    _prim.BoxRotated(pos + new Vector3(0, 0, 0.12f), new Vector3(0.14f, 0.22f, 0.02f), rot, new Color(195, 35, 35));
                    _prim.BoxRotated(pos + new Vector3(0, 0, 0.12f), new Vector3(0.28f, 0.12f, 0.02f), rot, new Color(195, 35, 35));
                    break;

                default:
                    _prim.BoxRotated(pos, new Vector3(0.38f, 0.08f, 0.48f), rot, new Color(185, 175, 145));
                    break;
            }
        }
    }
}
