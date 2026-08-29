using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Expanded, modular Grayhaven town slice. Geometry doubles as collision layout.
/// Features Town Square, Houses, Police Precinct with interior cell block & reception,
/// Sub-level Basement, Abandoned Cruiser, Barricades, Utility Poles, and Props.
/// </summary>
public sealed class World
{
    private readonly List<WorldBounds> _colliders = new();
    private readonly WorldBounds _policeDoor = new(new Vector3(17f, 0f, -11.25f), new Vector2(3.1f, 0.7f));

    public World()
    {
        // 1. Residential & Town Buildings
        AddBuildingCollider(-16f, 8f, 8f, 8f);
        AddBuildingCollider(-15f, -14f, 9f, 7f);
        AddBuildingCollider(1f, 14f, 8f, 7f);
        AddBuildingCollider(18f, 15f, 9f, 8f);
        AddBuildingCollider(-31f, -3f, 7f, 10f);

        // 2. Police Precinct Exterior Walls & Interior Furniture
        AddWall(new Vector3(10.25f, 0f, -6f), new Vector2(.7f, 11.5f));
        AddWall(new Vector3(23.75f, 0f, -6f), new Vector2(.7f, 11.5f));
        AddWall(new Vector3(17f, 0f, -.55f), new Vector2(14.2f, .8f));
        AddWall(new Vector3(12.1f, 0f, -11.35f), new Vector2(3.8f, .8f));
        AddWall(new Vector3(21.9f, 0f, -11.35f), new Vector2(3.8f, .8f));
        
        // Police Interior Desks & Lockers
        AddWall(new Vector3(13.3f, 0f, -5.4f), new Vector2(2.2f, 2.2f)); // Main desk
        AddWall(new Vector3(20.9f, 0f, -3.2f), new Vector2(1.8f, 1.3f)); // Side desk
        AddWall(new Vector3(11.5f, 0f, -3.0f), new Vector2(1.2f, 2.8f)); // File cabinets
        AddWall(new Vector3(22.5f, 0f, -8.5f), new Vector2(1.2f, 3.2f)); // Holding cell bars

        // 3. Basement Sub-level Room Boundary & Pillars
        AddWall(new Vector3(31f, 0f, 0f), new Vector2(.8f, 12f));
        AddWall(new Vector3(49f, 0f, 0f), new Vector2(.8f, 12f));
        AddWall(new Vector3(40f, 0f, -6f), new Vector2(18.8f, .8f));
        AddWall(new Vector3(40f, 0f, 6f), new Vector2(18.8f, .8f));
        AddWall(new Vector3(36f, 0f, -2.5f), new Vector2(1.2f, 1.2f)); // Support pillar 1
        AddWall(new Vector3(44f, 0f, 2.5f), new Vector2(1.2f, 1.2f));  // Support pillar 2
        AddWall(new Vector3(40f, 0f, 3.2f), new Vector2(3.6f, 1.4f));  // Storage table

        // 4. Abandoned Police Cruiser & Street Props
        AddWall(new Vector3(12.5f, 0f, -15.5f), new Vector2(2.6f, 4.6f)); // Police Cruiser
        AddWall(new Vector3(-2f, 0f, -3f), new Vector2(2.8f, .75f));       // Square Bench
        AddWall(new Vector3(4f, 0f, 4f), new Vector2(.8f, 2.4f));          // Square Kiosk
        AddWall(new Vector3(0f, 0f, 0f), new Vector2(2.2f, 2.2f));         // Town Square Memorial
        AddWall(new Vector3(6f, 0f, -34f), new Vector2(7.5f, 1.2f));       // North Barricade
        AddWall(new Vector3(0f, 0f, 38f), new Vector2(18.0f, 1.2f));       // South Road Fog Blockade
    }

    private void AddBuildingCollider(float x, float z, float width, float depth) =>
        _colliders.Add(new WorldBounds(new Vector3(x, 0f, z), new Vector2(width, depth)));

    private void AddWall(Vector3 center, Vector2 size) => _colliders.Add(new WorldBounds(center, size));

    public bool IsBlocked(Vector3 position, float radius, bool policeDoorOpen)
    {
        if (position.X < -44f || position.X > 52f || position.Z < -44f || position.Z > 44f)
            return true;
        if (!policeDoorOpen && _policeDoor.Overlaps(position, radius)) return true;
        return _colliders.Any(c => c.Overlaps(position, radius));
    }

    public float GetCameraDistance(Vector3 target, Vector3 forward, float requestedDistance)
    {
        for (var distance = requestedDistance; distance >= 1.8f; distance -= .25f)
        {
            var probe = target - forward * distance;
            if (!_colliders.Any(c => c.Overlaps(probe, .18f))) return distance;
        }
        return 1.8f;
    }

    public bool IsInBasement(Vector3 position) => position.X > 30f && position.X < 50f && position.Z > -7f && position.Z < 7f;

    public void Draw(PrimitiveRenderer p, float time, bool policeDoorOpen)
    {
        var asphalt = new Color(38, 44, 46);
        var pavement = new Color(68, 76, 76);
        var concrete = new Color(92, 98, 95);
        var wall = new Color(72, 80, 82);
        var roof = new Color(35, 42, 44);

        // Ground base turf & asphalt streets
        p.Box(new Vector3(0, -.16f, 0), new Vector3(92, .3f, 92), new Color(26, 40, 37));
        p.Box(new Vector3(0, -.03f, 4), new Vector3(88, .06f, 7.8f), asphalt);
        p.Box(new Vector3(6, -.02f, 0), new Vector3(7.8f, .07f, 88), asphalt);
        p.Box(new Vector3(0, .01f, 0), new Vector3(20, .08f, 18), pavement);
        p.Box(new Vector3(-20, .02f, 4), new Vector3(3.2f, .09f, 6.8f), new Color(115, 105, 80));

        DrawRoadMarks(p);
        DrawTownSquareMonument(p);

        // Residential houses
        DrawHouse(p, new Vector3(-16, 0, 8), new Vector3(8, 4.6f, 8), wall, roof, hasPorch: true);
        DrawHouse(p, new Vector3(-15, 0, -14), new Vector3(9, 4.5f, 7), new Color(69, 75, 74), roof, hasPorch: true);
        DrawHouse(p, new Vector3(1, 0, 14), new Vector3(8, 4.4f, 7), new Color(76, 79, 76), roof, hasPorch: false);
        DrawHouse(p, new Vector3(18, 0, 15), new Vector3(9, 4.8f, 8), new Color(63, 72, 74), roof, hasPorch: true);
        DrawHouse(p, new Vector3(-31, 0, -3), new Vector3(7, 4.7f, 10), new Color(64, 68, 67), roof, hasPorch: false);

        DrawPoliceStation(p, policeDoorOpen, concrete, roof, time);
        DrawBasement(p, time);
        DrawPoliceCruiser(p, new Vector3(12.5f, 0f, -15.5f), 0.35f, time);
        DrawBarricades(p);
        DrawUtilityPoles(p);
        DrawProps(p, time);
        DrawTrees(p);
    }

    private static void DrawRoadMarks(PrimitiveRenderer p)
    {
        var faded = new Color(118, 112, 88);
        for (var x = -37; x < 38; x += 8)
            p.Box(new Vector3(x, .04f, 4), new Vector3(3.1f, .03f, .18f), faded);
        for (var z = -38; z < 39; z += 8)
            p.Box(new Vector3(6, .04f, z), new Vector3(.18f, .03f, 3.1f), faded);
    }

    private static void DrawTownSquareMonument(PrimitiveRenderer p)
    {
        // Central stone memorial obelisk
        var stone = new Color(85, 92, 90);
        p.Box(new Vector3(0, 0.15f, 0), new Vector3(2.6f, 0.3f, 2.6f), new Color(65, 72, 70));
        p.Box(new Vector3(0, 0.55f, 0), new Vector3(1.8f, 0.5f, 1.8f), stone);
        p.Box(new Vector3(0, 1.75f, 0), new Vector3(1.1f, 1.9f, 1.1f), stone);
        p.Cone(new Vector3(0, 3.1f, 0), 0.75f, 0.9f, 4, new Color(98, 106, 104));
        // Bronze Dedication Plaque
        p.Box(new Vector3(0, 1.25f, -0.58f), new Vector3(0.6f, 0.45f, 0.04f), new Color(175, 145, 75));
    }

    private static void DrawHouse(PrimitiveRenderer p, Vector3 center, Vector3 size, Color wall, Color roof, bool hasPorch)
    {
        p.Box(center + new Vector3(0, size.Y * .5f, 0), size, wall);
        p.Roof(center + new Vector3(0, size.Y + .5f, 0), new Vector3(size.X + .7f, 1.4f, size.Z + .7f), roof);
        
        // Windows
        p.Box(center + new Vector3(-size.X * .25f, size.Y * .54f, -size.Z * .51f), new Vector3(1.1f, 1.05f, .07f), new Color(105, 138, 138));
        p.Box(center + new Vector3(size.X * .25f, size.Y * .54f, -size.Z * .51f), new Vector3(1.1f, 1.05f, .07f), new Color(105, 138, 138));
        
        // Wooden Door
        p.Box(center + new Vector3(0f, 1.15f, -size.Z * .52f), new Vector3(1.2f, 2.25f, .13f), new Color(48, 42, 35));

        // Brick Chimney
        p.Box(center + new Vector3(size.X * 0.35f, size.Y + 0.8f, size.Z * 0.2f), new Vector3(0.9f, 1.8f, 0.9f), new Color(95, 55, 48));

        // Wooden Porch with steps
        if (hasPorch)
        {
            Vector3 porchPos = center + new Vector3(0f, 0.2f, -size.Z * 0.5f - 1.1f);
            p.Box(porchPos, new Vector3(3.2f, 0.38f, 2.2f), new Color(55, 46, 38));
            p.Box(porchPos + new Vector3(0f, -0.15f, -1.25f), new Vector3(2.4f, 0.16f, 0.65f), new Color(48, 40, 34)); // Step
            // Porch roof overhang
            p.Box(porchPos + new Vector3(0f, 2.6f, 0.2f), new Vector3(3.4f, 0.18f, 2.4f), roof);
            // Porch support posts
            p.Box(porchPos + new Vector3(-1.45f, 1.3f, -0.9f), new Vector3(0.14f, 2.6f, 0.14f), new Color(55, 46, 38));
            p.Box(porchPos + new Vector3(1.45f, 1.3f, -0.9f), new Vector3(0.14f, 2.6f, 0.14f), new Color(55, 46, 38));
        }
    }

    private static void DrawPoliceStation(PrimitiveRenderer p, bool doorOpen, Color concrete, Color roof, float time)
    {
        p.Box(new Vector3(17, 2.35f, -6), new Vector3(14, 4.7f, 11), concrete);
        p.Roof(new Vector3(17, 5.35f, -6), new Vector3(14.8f, 1.2f, 11.8f), roof);

        // Entrance Doorway
        p.Box(new Vector3(17, 1.35f, -11.55f), new Vector3(3.05f, 2.7f, .18f), new Color(20, 27, 29));
        if (!doorOpen)
            p.Box(new Vector3(17, 1.35f, -11.67f), new Vector3(2.8f, 2.65f, .12f), new Color(63, 71, 72));

        // Police Precinct Gold Sign
        p.Box(new Vector3(17, 4.25f, -11.6f), new Vector3(6.2f, .9f, .14f), new Color(25, 38, 55));
        p.Box(new Vector3(17, 4.25f, -11.72f), new Vector3(5.2f, .45f, .04f), new Color(215, 185, 85));

        // Windows
        p.Box(new Vector3(11.5f, 2.7f, -11.58f), new Vector3(1.1f, 1.35f, .12f), new Color(94, 134, 134));
        p.Box(new Vector3(22.5f, 2.7f, -11.58f), new Vector3(1.1f, 1.35f, .12f), new Color(94, 134, 134));

        // Interior: Reception Desk, Desks, File Cabinets
        p.Box(new Vector3(13.3f, .75f, -5.4f), new Vector3(2.2f, 1.4f, 2.2f), new Color(52, 45, 40));
        p.Box(new Vector3(20.9f, .55f, -3.2f), new Vector3(1.8f, 1.0f, 1.3f), new Color(48, 44, 38));
        p.Box(new Vector3(11.5f, 1.4f, -3.0f), new Vector3(0.9f, 2.6f, 2.4f), new Color(65, 72, 75)); // Lockers

        // Holding Cell Iron Bars
        for (float z = -9.8f; z <= -7.2f; z += 0.45f)
        {
            p.Cylinder(new Vector3(22.5f, 1.4f, z), 0.04f, 2.8f, 4, new Color(42, 45, 48));
        }

        // Sub-level Hatch Rim & Gate
        p.Box(new Vector3(20.3f, .12f, -8.8f), new Vector3(2.2f, .12f, 1.4f), new Color(32, 38, 40));
        p.Box(new Vector3(20.3f, .22f, -8.8f), new Vector3(1.9f, .06f, 1.1f), new Color(75, 45, 35)); // Rusted Iron Hatch

        // Emergency Red Wall Light (Flickering)
        int redFlicker = (int)(180 + MathF.Sin(time * 12f) * 65f);
        p.Box(new Vector3(17f, 4.4f, -1.1f), new Vector3(1.8f, .35f, .15f), new Color(redFlicker, 35, 25));
    }

    private static void DrawBasement(PrimitiveRenderer p, float time)
    {
        var stone = new Color(45, 52, 54);
        var floor = new Color(28, 32, 33);

        // Floor & Walls
        p.Box(new Vector3(40, -.1f, 0), new Vector3(18, .2f, 12), floor);
        p.Box(new Vector3(31, 2.3f, 0), new Vector3(.75f, 4.6f, 12), stone);
        p.Box(new Vector3(49, 2.3f, 0), new Vector3(.75f, 4.6f, 12), stone);
        p.Box(new Vector3(40, 2.3f, -6), new Vector3(18.7f, 4.6f, .75f), stone);
        p.Box(new Vector3(40, 2.3f, 6), new Vector3(18.7f, 4.6f, .75f), stone);
        p.Box(new Vector3(40, 4.65f, 0), new Vector3(18.7f, .4f, 12.7f), new Color(18, 22, 24));

        // Concrete Support Pillars
        p.Box(new Vector3(36f, 2.3f, -2.5f), new Vector3(1.2f, 4.6f, 1.2f), new Color(55, 62, 64));
        p.Box(new Vector3(44f, 2.3f, 2.5f), new Vector3(1.2f, 4.6f, 1.2f), new Color(55, 62, 64));

        // Overhead Pipes & Ceiling Beams
        p.Cylinder(new Vector3(40f, 4.2f, -2.5f), 0.12f, 17.5f, 6, new Color(75, 55, 45));
        p.Cylinder(new Vector3(40f, 4.2f, 2.5f), 0.12f, 17.5f, 6, new Color(75, 55, 45));

        // Storage Table & Weapon Bench
        p.Box(new Vector3(40, .75f, 2.8f), new Vector3(3.6f, 1.4f, 1.2f), new Color(48, 42, 36));
        p.Box(new Vector3(46f, 1.5f, -5.2f), new Vector3(2.4f, 3.0f, 0.8f), new Color(42, 48, 50)); // Steel Shelves
        p.Box(new Vector3(33f, 1.1f, 5.2f), new Vector3(1.4f, 2.2f, 0.9f), new Color(36, 40, 42));   // Heavy Safe

        // Red Emergency Beacon Light (Pulsing)
        int beaconGlow = (int)(160 + MathF.Sin(time * 8f) * 85f);
        p.Box(new Vector3(35.5f, 3.1f, -5.55f), new Vector3(.45f, .75f, .12f), new Color(beaconGlow, 30, 20));
    }

    private static void DrawPoliceCruiser(PrimitiveRenderer p, Vector3 pos, float yaw, float time)
    {
        // Low-poly 1990s Police Cruiser
        var rot = Matrix.CreateRotationY(yaw);
        Color bodyWhite = new(210, 215, 218);
        Color bodyBlack = new(28, 32, 36);
        Color glass = new(75, 105, 115);
        Color wheels = new(20, 20, 22);

        Vector3 b = pos + new Vector3(0, 0.45f, 0);

        // Lower Chassis (Black)
        p.BoxRotated(b, new Vector3(2.1f, 0.5f, 4.4f), yaw, bodyBlack);

        // Hood & Trunk (Black/White)
        p.BoxRotated(b + Vector3.Transform(new Vector3(0, 0.35f, -1.1f), rot), new Vector3(2.0f, 0.32f, 1.6f), yaw, bodyBlack); // Hood
        p.BoxRotated(b + Vector3.Transform(new Vector3(0, 0.35f, 1.2f), rot), new Vector3(2.0f, 0.32f, 1.4f), yaw, bodyBlack);  // Trunk

        // Cabin & Roof (White)
        p.BoxRotated(b + Vector3.Transform(new Vector3(0, 0.58f, 0.05f), rot), new Vector3(1.85f, 0.55f, 1.9f), yaw, bodyWhite);
        // Cabin Windows
        p.BoxRotated(b + Vector3.Transform(new Vector3(0, 0.62f, 0.05f), rot), new Vector3(1.88f, 0.45f, 1.75f), yaw, glass);

        // 4 Wheels
        p.BoxRotated(b + Vector3.Transform(new Vector3(-1.05f, -0.15f, -1.2f), rot), new Vector3(0.25f, 0.6f, 0.6f), yaw, wheels);
        p.BoxRotated(b + Vector3.Transform(new Vector3(1.05f, -0.15f, -1.2f), rot), new Vector3(0.25f, 0.6f, 0.6f), yaw, wheels);
        p.BoxRotated(b + Vector3.Transform(new Vector3(-1.05f, -0.15f, 1.2f), rot), new Vector3(0.25f, 0.6f, 0.6f), yaw, wheels);
        p.BoxRotated(b + Vector3.Transform(new Vector3(1.05f, -0.15f, 1.2f), rot), new Vector3(0.25f, 0.6f, 0.6f), yaw, wheels);

        // Flashing Emergency Lightbar on Roof
        int redFlash = (int)(180 + MathF.Sin(time * 10f) * 75f);
        p.BoxRotated(b + Vector3.Transform(new Vector3(-0.4f, 0.92f, 0.05f), rot), new Vector3(0.6f, 0.16f, 0.3f), yaw, new Color(redFlash, 30, 20));
        p.BoxRotated(b + Vector3.Transform(new Vector3(0.4f, 0.92f, 0.05f), rot), new Vector3(0.6f, 0.16f, 0.3f), yaw, new Color(30, 60, 180));
    }

    private static void DrawBarricades(PrimitiveRenderer p)
    {
        // North Road Barricade (Leading to old church / thick fog)
        Vector3 northPos = new(6f, 0.75f, -34f);
        p.Box(northPos, new Vector3(7.2f, 0.4f, 0.2f), new Color(195, 165, 45)); // Yellow board
        p.Box(northPos + new Vector3(0, 0.45f, 0), new Vector3(7.2f, 0.4f, 0.2f), new Color(35, 35, 38)); // Black stripes
        p.Box(northPos + new Vector3(-3.2f, -0.25f, 0), new Vector3(0.35f, 1.2f, 0.8f), new Color(55, 48, 40));
        p.Box(northPos + new Vector3(3.2f, -0.25f, 0), new Vector3(0.35f, 1.2f, 0.8f), new Color(55, 48, 40));

        // South Road Barricade (Leading to hospital)
        Vector3 southPos = new(0f, 0.75f, 38f);
        p.Box(southPos, new Vector3(16.0f, 0.45f, 0.2f), new Color(195, 165, 45));
        p.Box(southPos + new Vector3(0, 0.5f, 0), new Vector3(16.0f, 0.45f, 0.2f), new Color(35, 35, 38));
        p.Box(southPos + new Vector3(-6.5f, -0.25f, 0), new Vector3(0.4f, 1.2f, 0.8f), new Color(55, 48, 40));
        p.Box(southPos + new Vector3(6.5f, -0.25f, 0), new Vector3(0.4f, 1.2f, 0.8f), new Color(55, 48, 40));
    }

    private static void DrawUtilityPoles(PrimitiveRenderer p)
    {
        Vector3[] poles =
        {
            new(-10f, 0f, 12f),
            new(-10f, 0f, -6f),
            new(-10f, 0f, -24f),
            new(22f, 0f, 12f),
            new(22f, 0f, -24f)
        };

        foreach (var pos in poles)
        {
            // Wood pole
            p.Cylinder(pos + new Vector3(0, 4.0f, 0), 0.16f, 8.0f, 6, new Color(58, 48, 36));
            // Crossarms
            p.Box(pos + new Vector3(0, 7.4f, 0), new Vector3(2.4f, 0.18f, 0.2f), new Color(55, 45, 32));
            p.Box(pos + new Vector3(0, 6.8f, 0), new Vector3(1.8f, 0.18f, 0.2f), new Color(55, 45, 32));
            // Insulators
            p.Cylinder(pos + new Vector3(-1.0f, 7.6f, 0), 0.06f, 0.25f, 4, new Color(185, 195, 205));
            p.Cylinder(pos + new Vector3(1.0f, 7.6f, 0), 0.06f, 0.25f, 4, new Color(185, 195, 205));
        }
    }

    private static void DrawProps(PrimitiveRenderer p, float time)
    {
        var lampGlow = (int)(180 + MathF.Sin(time * 6f) * 45f);
        foreach (var position in new[]
        {
            new Vector3(-7, 0, 4), new Vector3(7, 0, 10), new Vector3(7, 0, -10),
            new Vector3(-28, 0, 4), new Vector3(28, 0, 4), new Vector3(-3, 0, 20)
        })
        {
            p.Cylinder(position + new Vector3(0, 2.3f, 0), .11f, 4.6f, 6, new Color(38, 47, 47));
            p.Box(position + new Vector3(0, 4.65f, 0), new Vector3(.65f, .35f, .65f), new Color(lampGlow, 146, 74));
        }

        // Town square wooden benches & kiosks
        p.Box(new Vector3(-2f, .45f, -3), new Vector3(2.8f, .85f, .75f), new Color(54, 46, 39));
        p.Box(new Vector3(-2f, .98f, -3), new Vector3(2.7f, .18f, .16f), new Color(93, 73, 50));
        p.Box(new Vector3(4f, .55f, 4f), new Vector3(.8f, 1.1f, 2.4f), new Color(42, 50, 48));

        // Dumpsters
        p.Box(new Vector3(28f, .85f, 10f), new Vector3(3.6f, 1.35f, 1.85f), new Color(45, 68, 55));
        p.Box(new Vector3(28f, 1.55f, 10f), new Vector3(2.2f, .4f, 1.88f), new Color(38, 54, 45));
    }

    private static void DrawTrees(PrimitiveRenderer p)
    {
        foreach (var position in new[]
        {
            new Vector3(-38, 0, 20), new Vector3(-35, 0, -22), new Vector3(-27, 0, 27),
            new Vector3(32, 0, -25), new Vector3(39, 0, 25), new Vector3(29, 0, 28),
            new Vector3(12, 0, -31), new Vector3(-4, 0, -30), new Vector3(40, 0, -12),
            new Vector3(-18, 0, 28), new Vector3(16, 0, 32), new Vector3(-8, 0, -18)
        })
        {
            p.Cylinder(position + new Vector3(0, 1.7f, 0), .25f, 3.4f, 6, new Color(62, 54, 42));
            p.Cone(position + new Vector3(0, 3.8f, 0), 2.4f, 3.2f, 6, new Color(28, 52, 46));
            p.Cone(position + new Vector3(0, 5.4f, 0), 1.9f, 3.0f, 6, new Color(32, 58, 52));
            p.Cone(position + new Vector3(0, 6.8f, 0), 1.3f, 2.4f, 6, new Color(36, 65, 58));
        }
    }
}
