namespace HighFog;

public sealed class Handgun
{
    public int Damage { get; } = 34;
    public int MagazineSize { get; } = 6;
    public int Ammo { get; set; }
    public int ReserveAmmo { get; set; }
    public float ReloadTime { get; } = 1.45f;
    public float FireRate { get; } = 0.42f;
    public bool IsReloading { get; private set; }
    private float _reloadTimer;
    private float _fireTimer;

    public void Update(float dt)
    {
        _fireTimer = MathF.Max(0f, _fireTimer - dt);
        if (!IsReloading) return;
        _reloadTimer -= dt;
        if (_reloadTimer > 0f) return;

        var needed = MagazineSize - Ammo;
        var moved = Math.Min(needed, ReserveAmmo);
        Ammo += moved;
        ReserveAmmo -= moved;
        IsReloading = false;
    }

    public bool TryFire()
    {
        if (IsReloading || _fireTimer > 0f || Ammo <= 0) return false;
        Ammo--;
        _fireTimer = FireRate;
        return true;
    }

    public bool StartReload()
    {
        if (IsReloading || Ammo >= MagazineSize || ReserveAmmo <= 0) return false;
        IsReloading = true;
        _reloadTimer = ReloadTime;
        return true;
    }
}
