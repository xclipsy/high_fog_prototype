using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

/// <summary>
/// Generates authentic 24x24 pixel textures matching the N64 / PS1 survival horror aesthetic.
/// Textures are rendered with PointClamp / Nearest-Neighbor filtering.
/// </summary>
public sealed class TextureGenerator : IDisposable
{
    private readonly GraphicsDevice _device;
    private readonly Dictionary<string, Texture2D> _textures = new();

    public const int Size = 24;

    public TextureGenerator(GraphicsDevice device)
    {
        _device = device;
        GenerateAll();
    }

    public Texture2D Get(string name)
    {
        return _textures.TryGetValue(name, out var tex) ? tex : _textures["Concrete"];
    }

    public Texture2D Asphalt => Get("Asphalt");
    public Texture2D Concrete => Get("Concrete");
    public Texture2D Brick => Get("Brick");
    public Texture2D Wood => Get("Wood");
    public Texture2D Metal => Get("Metal");
    public Texture2D Grass => Get("Grass");
    public Texture2D Dirt => Get("Dirt");
    public Texture2D Wall => Get("Wall");
    public Texture2D Roof => Get("Roof");
    public Texture2D Window => Get("Window");
    public Texture2D EmergencyLight => Get("EmergencyLight");
    public Texture2D BloodStain => Get("BloodStain");
    public Texture2D PaperDocument => Get("PaperDocument");
    public Texture2D PlayerCoat => Get("PlayerCoat");
    public Texture2D MonsterSkin => Get("MonsterSkin");
    public Texture2D White => Get("White");
    public Texture2D ClaraPortrait => Get("ClaraPortrait");
    public Texture2D ArthurPortrait => Get("ArthurPortrait");
    public Texture2D VancePortrait => Get("VancePortrait");
    public Texture2D ThomasPortrait => Get("ThomasPortrait");

    private void GenerateAll()
    {
        // 1x1 plain white
        var white = new Texture2D(_device, 1, 1);
        white.SetData(new[] { Color.White });
        _textures["White"] = white;

        _textures["Asphalt"] = CreateAsphalt();
        _textures["Concrete"] = CreateConcrete();
        _textures["Brick"] = CreateBrick();
        _textures["Wood"] = CreateWood();
        _textures["Metal"] = CreateMetal();
        _textures["Grass"] = CreateGrass();
        _textures["Dirt"] = CreateDirt();
        _textures["Wall"] = CreateWall();
        _textures["Roof"] = CreateRoof();
        _textures["Window"] = CreateWindow();
        _textures["EmergencyLight"] = CreateEmergencyLight();
        _textures["BloodStain"] = CreateBloodStain();
        _textures["PaperDocument"] = CreatePaperDocument();
        _textures["PlayerCoat"] = CreatePlayerCoat();
        _textures["MonsterSkin"] = CreateMonsterSkin();
        _textures["ClaraPortrait"] = CreateClaraPortrait();
        _textures["ArthurPortrait"] = CreateArthurPortrait();
        _textures["VancePortrait"] = CreateVancePortrait();
        _textures["ThomasPortrait"] = CreateThomasPortrait();
    }

    private Texture2D CreateAsphalt()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(101);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int noise = rand.Next(-6, 7);
                int baseVal = 38 + noise;
                pixels[y * Size + x] = new Color(baseVal, baseVal + 3, baseVal + 5);
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateConcrete()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(102);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int n = rand.Next(-8, 9);
                int r = Math.Clamp(85 + n, 60, 110);
                int g = Math.Clamp(90 + n, 65, 115);
                int b = Math.Clamp(88 + n, 65, 115);
                // Subtle slab seams
                if (x == 0 || y == 0 || x == Size - 1 || y == Size - 1)
                {
                    r -= 18; g -= 18; b -= 18;
                }
                pixels[y * Size + x] = new Color(r, g, b);
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateBrick()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(103);
        int mortarColor = 60;
        
        for (int y = 0; y < Size; y++)
        {
            int row = y / 6; // 4 rows of 6px
            bool isMortarY = (y % 6 == 0);
            int shift = (row % 2) * 6;

            for (int x = 0; x < Size; x++)
            {
                int localX = (x + shift) % 12;
                bool isMortarX = (localX == 0);

                if (isMortarY || isMortarX)
                {
                    int m = mortarColor + rand.Next(-4, 5);
                    pixels[y * Size + x] = new Color(m, m + 2, m + 4);
                }
                else
                {
                    int noise = rand.Next(-10, 11);
                    int r = Math.Clamp(95 + noise, 70, 130);
                    int g = Math.Clamp(58 + noise / 2, 40, 80);
                    int b = Math.Clamp(52 + noise / 2, 35, 75);
                    pixels[y * Size + x] = new Color(r, g, b);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateWood()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(104);
        for (int y = 0; y < Size; y++)
        {
            bool isSeam = (y % 8 == 0);
            for (int x = 0; x < Size; x++)
            {
                int grain = (int)(MathF.Sin(x * 0.4f + y * 0.1f) * 6f) + rand.Next(-4, 5);
                if (isSeam)
                {
                    pixels[y * Size + x] = new Color(30, 24, 18);
                }
                else
                {
                    int r = Math.Clamp(75 + grain, 50, 105);
                    int g = Math.Clamp(58 + grain, 38, 85);
                    int b = Math.Clamp(42 + grain, 28, 65);
                    pixels[y * Size + x] = new Color(r, g, b);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateMetal()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(105);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int rust = rand.Next(0, 100);
                if (rust > 82)
                {
                    // Rust spots
                    pixels[y * Size + x] = new Color(110 + rand.Next(-8, 9), 55 + rand.Next(-5, 6), 35);
                }
                else
                {
                    int steel = 65 + rand.Next(-6, 7);
                    // Rivet marks at corners
                    if ((x == 2 || x == 21) && (y == 2 || y == 21)) steel += 35;
                    pixels[y * Size + x] = new Color(steel, steel + 4, steel + 8);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateGrass()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(106);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int n = rand.Next(-10, 11);
                // Cold, desaturated winter turf
                int r = Math.Clamp(34 + n / 2, 20, 50);
                int g = Math.Clamp(58 + n, 38, 78);
                int b = Math.Clamp(45 + n / 2, 30, 60);
                pixels[y * Size + x] = new Color(r, g, b);
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateDirt()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(107);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int n = rand.Next(-8, 9);
                int r = Math.Clamp(55 + n, 35, 75);
                int g = Math.Clamp(45 + n, 28, 62);
                int b = Math.Clamp(35 + n, 20, 50);
                pixels[y * Size + x] = new Color(r, g, b);
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateWall()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(108);
        for (int y = 0; y < Size; y++)
        {
            bool isSiding = (y % 4 == 0);
            for (int x = 0; x < Size; x++)
            {
                int n = rand.Next(-5, 6);
                if (isSiding)
                {
                    pixels[y * Size + x] = new Color(45, 50, 52);
                }
                else
                {
                    int val = 75 + n;
                    pixels[y * Size + x] = new Color(val, val + 6, val + 8);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateRoof()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(109);
        for (int y = 0; y < Size; y++)
        {
            int row = y / 4;
            bool isSeam = (y % 4 == 0);
            int shift = (row % 2) * 6;
            for (int x = 0; x < Size; x++)
            {
                bool isVertSeam = ((x + shift) % 8 == 0);
                if (isSeam || isVertSeam)
                {
                    pixels[y * Size + x] = new Color(22, 28, 30);
                }
                else
                {
                    int n = rand.Next(-6, 7);
                    int val = 42 + n;
                    pixels[y * Size + x] = new Color(val, val + 5, val + 8);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateWindow()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(110);
        for (int y = 0; y < Size; y++)
        {
            bool isFrame = (x_is_edge(y) || y == Size / 2);
            for (int x = 0; x < Size; x++)
            {
                bool isMullion = (x_is_edge(x) || x == Size / 2);
                if (isFrame || isMullion)
                {
                    pixels[y * Size + x] = new Color(35, 42, 44);
                }
                else
                {
                    int glow = 100 + rand.Next(-8, 9);
                    pixels[y * Size + x] = new Color(glow - 20, glow + 10, glow + 15);
                }
            }
        }
        return CreateTexture(pixels);

        static bool x_is_edge(int val) => val <= 1 || val >= Size - 2;
    }

    private Texture2D CreateEmergencyLight()
    {
        var pixels = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dx = x - 11.5f;
                float dy = y - 11.5f;
                float dist = MathF.Sqrt(dx * dx + dy * dy);
                if (dist < 4.5f)
                {
                    pixels[y * Size + x] = new Color(255, 220, 160); // Bright core
                }
                else if (dist < 9.5f)
                {
                    pixels[y * Size + x] = new Color(220, 50, 20); // Red glow
                }
                else
                {
                    pixels[y * Size + x] = new Color(50, 15, 10); // Casing
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateBloodStain()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(111);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dx = x - 11.5f;
                float dy = y - 11.5f;
                float dist = MathF.Sqrt(dx * dx + dy * dy) + rand.NextSingle() * 3f;
                if (dist < 7.5f)
                {
                    int r = Math.Clamp(120 + rand.Next(-15, 16), 80, 160);
                    pixels[y * Size + x] = new Color(r, 15, 15, 230);
                }
                else
                {
                    pixels[y * Size + x] = Color.Transparent;
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreatePaperDocument()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(112);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                if (x <= 1 || y <= 1 || x >= Size - 2 || y >= Size - 2)
                {
                    pixels[y * Size + x] = new Color(130, 118, 95);
                }
                else
                {
                    int n = rand.Next(-4, 5);
                    bool isTextLine = (y % 4 == 0 && x > 4 && x < 20);
                    if (isTextLine)
                    {
                        pixels[y * Size + x] = new Color(60, 55, 48);
                    }
                    else
                    {
                        pixels[y * Size + x] = new Color(195 + n, 185 + n, 155 + n);
                    }
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreatePlayerCoat()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(113);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int n = rand.Next(-5, 6);
                int val = 48 + n;
                // Brownish-grey trenchcoat
                pixels[y * Size + x] = new Color(val + 4, val + 2, val);
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateMonsterSkin()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(114);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int n = rand.Next(-10, 11);
                // Ashen grey-purple necrotic skin
                int r = Math.Clamp(42 + n, 25, 65);
                int g = Math.Clamp(46 + n, 30, 70);
                int b = Math.Clamp(52 + n, 35, 78);
                pixels[y * Size + x] = new Color(r, g, b);
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateClaraPortrait()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(201);
        // Nervous young woman with brown hair and red scarf
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                // Background - dark foggy
                if (x <= 1 || x >= Size - 2 || y <= 1 || y >= Size - 2)
                {
                    pixels[y * Size + x] = new Color(25, 30, 35);
                }
                // Hair - brown
                else if (y < 10 && x > 4 && x < 20)
                {
                    int n = rand.Next(-5, 6);
                    pixels[y * Size + x] = new Color(58 + n, 42 + n, 32 + n);
                }
                // Face - pale skin
                else if (y >= 8 && y < 16 && x >= 7 && x < 18)
                {
                    int n = rand.Next(-3, 4);
                    pixels[y * Size + x] = new Color(195 + n, 165 + n, 145 + n);
                }
                // Eyes - worried expression
                else if (y == 11 && (x == 9 || x == 15))
                {
                    pixels[y * Size + x] = new Color(45, 38, 35);
                }
                // Red scarf
                else if (y >= 16 && x >= 6 && x < 19)
                {
                    int n = rand.Next(-8, 9);
                    pixels[y * Size + x] = new Color(145 + n, 35 + n/2, 30 + n/2);
                }
                // Parka - muted green/brown
                else if (y >= 18)
                {
                    int n = rand.Next(-5, 6);
                    pixels[y * Size + x] = new Color(52 + n, 58 + n, 54 + n);
                }
                else
                {
                    pixels[y * Size + x] = new Color(35, 40, 45);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateArthurPortrait()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(202);
        // Old man with gray hair and wool cardigan
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                // Background - dark porch
                if (x <= 1 || x >= Size - 2 || y <= 1 || y >= Size - 2)
                {
                    pixels[y * Size + x] = new Color(30, 28, 25);
                }
                // Gray hair
                else if (y < 9 && x > 5 && x < 19)
                {
                    int n = rand.Next(-5, 6);
                    int gray = 165 + n;
                    pixels[y * Size + x] = new Color(gray, gray, gray - 5);
                }
                // Wrinkled face - aged skin
                else if (y >= 7 && y < 15 && x >= 6 && x < 18)
                {
                    int n = rand.Next(-4, 5);
                    pixels[y * Size + x] = new Color(185 + n, 155 + n, 135 + n);
                }
                // Sad eyes
                else if (y == 10 && (x == 8 || x == 14))
                {
                    pixels[y * Size + x] = new Color(55, 48, 45);
                }
                // Wool cardigan - blue-gray
                else if (y >= 15)
                {
                    int n = rand.Next(-6, 7);
                    pixels[y * Size + x] = new Color(48 + n, 58 + n, 72 + n);
                }
                else
                {
                    pixels[y * Size + x] = new Color(40, 38, 35);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateVancePortrait()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(203);
        // Injured police officer with cap and uniform
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                // Background - police station interior
                if (x <= 1 || x >= Size - 2 || y <= 1 || y >= Size - 2)
                {
                    pixels[y * Size + x] = new Color(28, 35, 40);
                }
                // Police cap - dark blue
                else if (y < 8 && x > 4 && x < 20)
                {
                    int n = rand.Next(-4, 5);
                    pixels[y * Size + x] = new Color(25 + n, 35 + n, 48 + n);
                }
                // Stern face - weathered skin
                else if (y >= 6 && y < 14 && x >= 6 && x < 18)
                {
                    int n = rand.Next(-5, 6);
                    pixels[y * Size + x] = new Color(175 + n, 145 + n, 125 + n);
                }
                // Intense eyes
                else if (y == 9 && (x == 8 || x == 14))
                {
                    pixels[y * Size + x] = new Color(48, 42, 40);
                }
                // Uniform - navy blue with badge hint
                else if (y >= 14)
                {
                    int n = rand.Next(-5, 6);
                    if (x == 12 && y == 15)
                    {
                        pixels[y * Size + x] = new Color(185, 165, 75); // Badge glint
                    }
                    else
                    {
                        pixels[y * Size + x] = new Color(32 + n, 48 + n, 68 + n);
                    }
                }
                else
                {
                    pixels[y * Size + x] = new Color(35, 42, 48);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateThomasPortrait()
    {
        var pixels = new Color[Size * Size];
        var rand = new Random(204);
        // Mysterious priest in dark cassock with hood
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                // Background - foggy church exterior
                if (x <= 1 || x >= Size - 2 || y <= 1 || y >= Size - 2)
                {
                    pixels[y * Size + x] = new Color(22, 28, 32);
                }
                // Dark hood
                else if ((y < 10 && x > 3 && x < 21) || (y >= 5 && y < 12 && (x <= 4 || x >= 19)))
                {
                    int n = rand.Next(-4, 5);
                    pixels[y * Size + x] = new Color(22 + n, 24 + n, 28 + n);
                }
                // Aged solemn face partially in shadow
                else if (y >= 8 && y < 15 && x >= 7 && x < 17)
                {
                    int n = rand.Next(-4, 5);
                    pixels[y * Size + x] = new Color(165 + n, 140 + n, 120 + n);
                }
                // Deep-set wise eyes
                else if (y == 11 && (x == 9 || x == 15))
                {
                    pixels[y * Size + x] = new Color(42, 38, 35);
                }
                // Black cassock with gold cross hint
                else if (y >= 15)
                {
                    int n = rand.Next(-3, 4);
                    if (x == 12 && y == 16)
                    {
                        pixels[y * Size + x] = new Color(185, 165, 65); // Cross glint
                    }
                    else
                    {
                        pixels[y * Size + x] = new Color(20 + n, 22 + n, 26 + n);
                    }
                }
                else
                {
                    pixels[y * Size + x] = new Color(28, 32, 36);
                }
            }
        }
        return CreateTexture(pixels);
    }

    private Texture2D CreateTexture(Color[] pixels)
    {
        var tex = new Texture2D(_device, Size, Size);
        tex.SetData(pixels);
        return tex;
    }

    public void Dispose()
    {
        foreach (var tex in _textures.Values)
        {
            tex.Dispose();
        }
        _textures.Clear();
    }
}
