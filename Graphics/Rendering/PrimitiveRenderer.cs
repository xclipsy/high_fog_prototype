using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

/// <summary>
/// Low-poly primitive and textured mesh renderer with distance fog and PointClamp texture filtering.
/// </summary>
public sealed class PrimitiveRenderer : IDisposable
{
    private readonly GraphicsDevice _device;
    private readonly BasicEffect _colorEffect;
    private readonly BasicEffect _textureEffect;
    private readonly RasterizerState _rasterizerState;

    public PrimitiveRenderer(GraphicsDevice device)
    {
        _device = device;
        
        _colorEffect = new BasicEffect(device)
        {
            VertexColorEnabled = true,
            TextureEnabled = false,
            LightingEnabled = false,
            FogEnabled = true
        };

        _textureEffect = new BasicEffect(device)
        {
            VertexColorEnabled = true,
            TextureEnabled = true,
            LightingEnabled = false,
            FogEnabled = true
        };

        _rasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            DepthClipEnable = true
        };
    }

    public void Begin(Matrix view, Matrix projection, Color fogColor, float fogStart, float fogEnd)
    {
        _device.RasterizerState = _rasterizerState;
        _device.SamplerStates[0] = SamplerState.PointClamp;
        _device.DepthStencilState = DepthStencilState.Default;
        _device.BlendState = BlendState.AlphaBlend;

        Vector3 fogVec = fogColor.ToVector3();

        _colorEffect.World = Matrix.Identity;
        _colorEffect.View = view;
        _colorEffect.Projection = projection;
        _colorEffect.FogColor = fogVec;
        _colorEffect.FogStart = fogStart;
        _colorEffect.FogEnd = fogEnd;

        _textureEffect.World = Matrix.Identity;
        _textureEffect.View = view;
        _textureEffect.Projection = projection;
        _textureEffect.FogColor = fogVec;
        _textureEffect.FogStart = fogStart;
        _textureEffect.FogEnd = fogEnd;
    }

    public void Box(Vector3 center, Vector3 size, Color color)
    {
        var h = size * .5f;
        var v = new[]
        {
            V(center + new Vector3(-h.X, -h.Y, -h.Z), color), V(center + new Vector3(h.X, -h.Y, -h.Z), color),
            V(center + new Vector3(h.X, -h.Y, h.Z), color), V(center + new Vector3(-h.X, -h.Y, h.Z), color),
            V(center + new Vector3(-h.X, h.Y, -h.Z), color), V(center + new Vector3(h.X, h.Y, -h.Z), color),
            V(center + new Vector3(h.X, h.Y, h.Z), color), V(center + new Vector3(-h.X, h.Y, h.Z), color)
        };
        DrawColor(v, new short[]
        {
            0,2,1, 0,3,2, 4,5,6, 4,6,7, 0,1,5, 0,5,4,
            1,2,6, 1,6,5, 2,3,7, 2,7,6, 3,0,4, 3,4,7
        });
    }

    public void BoxRotated(Vector3 center, Vector3 size, float yaw, Color color)
    {
        var h = size * .5f;
        var corners = new[]
        {
            new Vector3(-h.X, -h.Y, -h.Z), new Vector3(h.X, -h.Y, -h.Z),
            new Vector3(h.X, -h.Y, h.Z), new Vector3(-h.X, -h.Y, h.Z),
            new Vector3(-h.X, h.Y, -h.Z), new Vector3(h.X, h.Y, -h.Z),
            new Vector3(h.X, h.Y, h.Z), new Vector3(-h.X, h.Y, h.Z)
        };

        var rot = Matrix.CreateRotationY(yaw);
        var v = new VertexPositionColor[8];
        for (int i = 0; i < 8; i++)
        {
            v[i] = V(center + Vector3.Transform(corners[i], rot), color);
        }

        DrawColor(v, new short[]
        {
            0,2,1, 0,3,2, 4,5,6, 4,6,7, 0,1,5, 0,5,4,
            1,2,6, 1,6,5, 2,3,7, 2,7,6, 3,0,4, 3,4,7
        });
    }

    public void TexturedBox(Vector3 center, Vector3 size, Texture2D texture, Color tint)
    {
        _textureEffect.Texture = texture;
        var h = size * 0.5f;

        // 24 vertices (4 per face) with UV coordinates (0..1)
        var v = new VertexPositionColorTexture[24];
        
        // Front Face
        v[0] = VT(center + new Vector3(-h.X, -h.Y, h.Z), tint, new Vector2(0, 1));
        v[1] = VT(center + new Vector3(h.X, -h.Y, h.Z), tint, new Vector2(1, 1));
        v[2] = VT(center + new Vector3(h.X, h.Y, h.Z), tint, new Vector2(1, 0));
        v[3] = VT(center + new Vector3(-h.X, h.Y, h.Z), tint, new Vector2(0, 0));

        // Back Face
        v[4] = VT(center + new Vector3(h.X, -h.Y, -h.Z), tint, new Vector2(0, 1));
        v[5] = VT(center + new Vector3(-h.X, -h.Y, -h.Z), tint, new Vector2(1, 1));
        v[6] = VT(center + new Vector3(-h.X, h.Y, -h.Z), tint, new Vector2(1, 0));
        v[7] = VT(center + new Vector3(h.X, h.Y, -h.Z), tint, new Vector2(0, 0));

        // Top Face
        v[8] = VT(center + new Vector3(-h.X, h.Y, h.Z), tint, new Vector2(0, 1));
        v[9] = VT(center + new Vector3(h.X, h.Y, h.Z), tint, new Vector2(1, 1));
        v[10] = VT(center + new Vector3(h.X, h.Y, -h.Z), tint, new Vector2(1, 0));
        v[11] = VT(center + new Vector3(-h.X, h.Y, -h.Z), tint, new Vector2(0, 0));

        // Bottom Face
        v[12] = VT(center + new Vector3(-h.X, -h.Y, -h.Z), tint, new Vector2(0, 1));
        v[13] = VT(center + new Vector3(h.X, -h.Y, -h.Z), tint, new Vector2(1, 1));
        v[14] = VT(center + new Vector3(h.X, -h.Y, h.Z), tint, new Vector2(1, 0));
        v[15] = VT(center + new Vector3(-h.X, -h.Y, h.Z), tint, new Vector2(0, 0));

        // Left Face
        v[16] = VT(center + new Vector3(-h.X, -h.Y, -h.Z), tint, new Vector2(0, 1));
        v[17] = VT(center + new Vector3(-h.X, -h.Y, h.Z), tint, new Vector2(1, 1));
        v[18] = VT(center + new Vector3(-h.X, h.Y, h.Z), tint, new Vector2(1, 0));
        v[19] = VT(center + new Vector3(-h.X, h.Y, -h.Z), tint, new Vector2(0, 0));

        // Right Face
        v[20] = VT(center + new Vector3(h.X, -h.Y, h.Z), tint, new Vector2(0, 1));
        v[21] = VT(center + new Vector3(h.X, -h.Y, -h.Z), tint, new Vector2(1, 1));
        v[22] = VT(center + new Vector3(h.X, h.Y, -h.Z), tint, new Vector2(1, 0));
        v[23] = VT(center + new Vector3(h.X, h.Y, h.Z), tint, new Vector2(0, 0));

        short[] indices = new short[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23
        };

        DrawTexture(v, indices);
    }

    public void Roof(Vector3 center, Vector3 size, Color color)
    {
        var hx = size.X * .5f;
        var hz = size.Z * .5f;
        var bottom = center.Y - size.Y * .5f;
        var top = center.Y + size.Y * .5f;
        var v = new[]
        {
            V(new Vector3(center.X-hx, bottom, center.Z-hz), color), V(new Vector3(center.X+hx, bottom, center.Z-hz), color),
            V(new Vector3(center.X+hx, bottom, center.Z+hz), color), V(new Vector3(center.X-hx, bottom, center.Z+hz), color),
            V(new Vector3(center.X-hx, top, center.Z), color), V(new Vector3(center.X+hx, top, center.Z), color)
        };
        DrawColor(v, new short[] { 0,1,5, 0,5,4, 3,4,5, 3,5,2, 0,4,3, 1,2,5, 0,3,2, 0,2,1 });
    }

    public void Cylinder(Vector3 center, float radius, float height, int sides, Color color)
    {
        var vertices = new VertexPositionColor[sides * 2 + 2];
        var bottom = center.Y - height * .5f;
        var top = center.Y + height * .5f;
        for (var i = 0; i < sides; i++)
        {
            var a = MathHelper.TwoPi * i / sides;
            var point = new Vector3(center.X + MathF.Cos(a) * radius, 0, center.Z + MathF.Sin(a) * radius);
            vertices[i] = V(new Vector3(point.X, bottom, point.Z), color);
            vertices[sides + i] = V(new Vector3(point.X, top, point.Z), color);
        }
        vertices[^2] = V(new Vector3(center.X, bottom, center.Z), color);
        vertices[^1] = V(new Vector3(center.X, top, center.Z), color);
        var indices = new List<short>(sides * 12);
        for (short i = 0; i < sides; i++)
        {
            var next = (short)((i + 1) % sides);
            indices.Add(i); indices.Add(next); indices.Add((short)(sides + next));
            indices.Add(i); indices.Add((short)(sides + next)); indices.Add((short)(sides + i));
            indices.Add((short)(sides * 2)); indices.Add(next); indices.Add(i);
            indices.Add((short)(sides * 2 + 1)); indices.Add((short)(sides + i)); indices.Add((short)(sides + next));
        }
        DrawColor(vertices, indices.ToArray());
    }

    public void Cone(Vector3 center, float radius, float height, int sides, Color color)
    {
        var vertices = new VertexPositionColor[sides + 2];
        var bottom = center.Y - height * .5f;
        var top = center.Y + height * .5f;
        for (var i = 0; i < sides; i++)
        {
            var a = MathHelper.TwoPi * i / sides;
            vertices[i] = V(new Vector3(center.X + MathF.Cos(a) * radius, bottom, center.Z + MathF.Sin(a) * radius), color);
        }
        vertices[^2] = V(new Vector3(center.X, bottom, center.Z), color);
        vertices[^1] = V(new Vector3(center.X, top, center.Z), color);
        var indices = new List<short>(sides * 6);
        for (short i = 0; i < sides; i++)
        {
            var next = (short)((i + 1) % sides);
            indices.Add(i); indices.Add(next); indices.Add((short)(sides + 1));
            indices.Add((short)sides); indices.Add(next); indices.Add(i);
        }
        DrawColor(vertices, indices.ToArray());
    }

    public void Octahedron(Vector3 center, float radius, Color color)
    {
        var v = new[]
        {
            V(center + new Vector3(0, radius, 0), color), V(center + new Vector3(radius, 0, 0), color),
            V(center + new Vector3(0, 0, radius), color), V(center + new Vector3(-radius, 0, 0), color),
            V(center + new Vector3(0, 0, -radius), color), V(center + new Vector3(0, -radius, 0), color)
        };
        DrawColor(v, new short[] { 0,1,2, 0,2,3, 0,3,4, 0,4,1, 5,2,1, 5,3,2, 5,4,3, 5,1,4 });
    }

    private void DrawColor(VertexPositionColor[] vertices, short[] indices)
    {
        foreach (var pass in _colorEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
        }
    }

    private void DrawTexture(VertexPositionColorTexture[] vertices, short[] indices)
    {
        foreach (var pass in _textureEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
        }
    }

    private static VertexPositionColor V(Vector3 position, Color color) => new(position, color);
    private static VertexPositionColorTexture VT(Vector3 position, Color color, Vector2 uv) => new(position, color, uv);

    public void Dispose()
    {
        _colorEffect.Dispose();
        _textureEffect.Dispose();
        _rasterizerState.Dispose();
    }
}
