using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// HIGH FOG - Main Game Loop and Systems Orchestrator.
/// Coordinates 3D rendering, NPCs, enemy AI, dialogue, audio synthesis, and configurable movement controls.
/// </summary>
public sealed class HighFogGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private RetroFont _font = null!;
    private TextureGenerator _textures = null!;
    private PrimitiveRenderer _primitiveRenderer = null!;
    private CharacterRenderer _characterRenderer = null!;

    public GameConfig Config { get; } = new();
    public ThirdPersonCamera Camera { get; } = new();
    public Player Player { get; private set; } = new(new Vector3(0f, 0f, 32f));
    public PlayerController Controller { get; } = new();
    public World World { get; } = new();
    public NPCManager NPCs { get; } = new();
    public EnemyManager Enemies { get; } = new();
    public InteractionManager Interactions { get; } = new();
    public Inventory Inventory { get; } = new();
    public AudioManager Audio { get; } = new();
    public ParticleSystem Particles { get; } = new();
    public GameState State { get; } = new();
    public DialogueSequence Dialogue { get; } = new();
    public FogSettings CurrentFog { get; private set; } = FogSettings.Default;

    // UI Systems
    private readonly HUD _hud = new();
    private readonly DialogueUI _dialogueUI = new();
    private readonly DocumentUI _documentUI = new();
    private readonly InventoryUI _inventoryUI = new();
    private readonly PauseMenu _pauseMenu = new();
    private readonly MainMenuUI _mainMenuUI = new();
    private readonly IntroSequence _introSequence = new();
    private readonly GameOverUI _gameOverUI = new();
    private readonly EndingUI _endingUI = new();
    private readonly DebugOverlay _debugOverlay = new();

    public ScreenState CurrentScreen { get; private set; } = ScreenState.MainMenu;

    // Document Viewing State
    private string _activeDocTitle = string.Empty;
    private string _activeDocContent = string.Empty;

    // Toast Notification
    public string ToastMessage { get; private set; } = string.Empty;
    public float ToastTimer { get; private set; }

    private readonly InputTracker _input = new();
    private Point _screenCenter;
    private bool _isMouseLocked;
    private float _totalGameTime;

    public HighFogGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1024,
            PreferredBackBufferHeight = 768,
            SynchronizeWithVerticalRetrace = true,
            PreferHalfPixelOffset = true
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "HIGH FOG (N64 Low-Poly Survival Horror)";
    }

    protected override void Initialize()
    {
        _screenCenter = new Point(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = new RetroFont(GraphicsDevice);
        _textures = new TextureGenerator(GraphicsDevice);
        _primitiveRenderer = new PrimitiveRenderer(GraphicsDevice);
        _characterRenderer = new CharacterRenderer(_primitiveRenderer, _textures);

        SetupWorldInteractables();
        Audio.StartAmbience();
    }

    private void SetupWorldInteractables()
    {
        Interactions.Clear();

        // 1. Clara in Town Square
        Interactions.Register(new CustomInteractable(
            NPCs.Clara.Position,
            () => NPCs.Clara.GetInteractionText(this),
            () =>
            {
                NPCs.InteractWithClara(this);
                _dialogueUI.ResetTypewriter();
            }
        ));

        // 2. Arthur Miller on West Residential Porch
        Interactions.Register(new CustomInteractable(
            NPCs.Arthur.Position,
            () => NPCs.Arthur.GetInteractionText(this),
            () =>
            {
                NPCs.InteractWithArthur(this);
                _dialogueUI.ResetTypewriter();
            }
        ));

        // 3. Officer Vance in Police Reception Hallway
        Interactions.Register(new CustomInteractable(
            NPCs.OfficerVance.Position,
            () => NPCs.OfficerVance.GetInteractionText(this),
            () =>
            {
                NPCs.InteractWithOfficerVance(this);
                _dialogueUI.ResetTypewriter();
            }
        ));

        // 4. Father Thomas at Northern Church Road Gate
        Interactions.Register(new CustomInteractable(
            NPCs.FatherThomas.Position,
            () => NPCs.FatherThomas.GetInteractionText(this),
            () =>
            {
                NPCs.InteractWithFatherThomas(this);
                _dialogueUI.ResetTypewriter();
            }
        ));

        // 5. Police Precinct Front Door
        Interactions.Register(new DoorInteractable(
            new Vector3(17f, 0f, -11.25f),
            "POLICE PRECINCT",
            isLocked: true,
            requiredKey: ItemType.PoliceKey,
            onOpen: _ =>
            {
                State.PoliceStationUnlocked = true;
                ShowToast("UNLOCKED AND ENTERED POLICE PRECINCT");
            }
        ));

        // 6. Police Reception Officer's Log
        Interactions.Register(new DocumentInteractable(
            new Vector3(13.3f, 0.8f, -5.4f),
            "OFFICER'S LOG",
            "NOVEMBER 14 - 22:15\nThick fog rolled in off the mountains. Radio tower went dark.\n\n23:02\nEmergency calls flooding dispatch. Reports of tall humanoid shapes in the mist. Officers Miller and Vance dispatched to investigate the old factory gate.\n\n23:40\nOfficers did not return. Radio emitting harmonic screech. Barricading the precinct front doors."
        ));

        // 7. Police Reception First Aid Kit
        Interactions.Register(new ItemPickupInteractable(
            new Vector3(20.9f, 0.7f, -3.2f),
            Item.CreateMedkit()
        ));

        // 8. Police Station Basement Hatch (Teleport down to sub-level)
        Interactions.Register(new CustomInteractable(
            new Vector3(20.3f, 0.2f, -8.8f),
            () => "ENTER BASEMENT HATCH",
            () =>
            {
                Player.Position = new Vector3(33.5f, 0f, 0f);
                Audio.PlayCue("door");
                ShowToast("DESCENDED INTO PRECINCT BASEMENT");
                State.FoundBasement = true;
                if (!State.FoundGun)
                {
                    State.Objective = "SEARCH THE BASEMENT FOR A WEAPON.";
                }
            }
        ));

        // 9. Basement Return Ladder (Teleport up to reception)
        Interactions.Register(new CustomInteractable(
            new Vector3(32f, 0.2f, 0f),
            () => "CLIMB LADDER TO RECEPTION",
            () =>
            {
                Player.Position = new Vector3(19.5f, 0f, -7.5f);
                Audio.PlayCue("door");
                ShowToast("ASCENDED TO POLICE RECEPTION");
            }
        ));

        // 10. Basement Service Handgun Pickup (Section 48)
        Interactions.Register(new ItemPickupInteractable(
            new Vector3(40f, 0.85f, 2.8f),
            Item.CreateHandgun()
        ));

        // 11. Basement 9mm Extra Ammunition Box
        Interactions.Register(new ItemPickupInteractable(
            new Vector3(41.5f, 0.85f, 2.8f),
            Item.CreateAmmo(12)
        ));

        // 12. Basement PROJECT HAZE Classified Document (Section 50)
        Interactions.Register(new DocumentInteractable(
            new Vector3(38.5f, 0.85f, 2.8f),
            "PROJECT HAZE REPORT",
            "CLASSIFIED FACILITY REPORT - TOP SECRET\nSUBJECT: SUBTERRANEAN DRILLING ANOMALY\n\nPhase 4 deep-core resonance drilling beneath the old factory has breached an anomalous geode cavity at depth 820m.\n\nThe released particulate (designated HAZE) exhibits biological catalyst properties and temporal distortion.\n\nDO NOT DISPATCH UNPROTECTED RESCUE TEAMS. THE ANOMALY REACTS TO AUDITORY VIBRATION.",
            flagToSet: "ProjectHaze"
        ));
    }

    public void StartNewGame()
    {
        Player.Reset(new Vector3(0f, 0f, 32f));
        Camera.Reset();
        Enemies.InitializeTownEnemies();
        SetupWorldInteractables();

        State.MetClara = false;
        State.PoliceStationUnlocked = false;
        State.FoundBasement = false;
        State.FoundGun = false;
        State.FirstWalkerDefeated = false;
        State.ReadProjectHaze = false;
        State.SawFogSilhouette = false;
        State.Objective = "EXPLORE GRAYHAVEN AND FIND SURVIVORS.";

        _introSequence.Reset();
        SetScreenState(ScreenState.Intro);
    }

    public void RestartCheckpoint()
    {
        Player.Health = 100f;
        if (World.IsInBasement(Player.Position))
        {
            Player.Position = new Vector3(33.5f, 0f, 0f);
        }
        else
        {
            Player.Position = new Vector3(0f, 0f, 32f);
        }
        ResumeGame();
    }

    public void ResumeGame()
    {
        SetScreenState(ScreenState.Playing);
    }

    public void SetScreenState(ScreenState state)
    {
        CurrentScreen = state;
        _isMouseLocked = (state == ScreenState.Playing);
        IsMouseVisible = !_isMouseLocked;

        if (_isMouseLocked)
        {
            Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
        }
    }

    public void OpenDocument(string title, string content)
    {
        _activeDocTitle = title;
        _activeDocContent = content;
        SetScreenState(ScreenState.Ending);
    }

    public void ShowToast(string message)
    {
        ToastMessage = message;
        ToastTimer = 3.5f;
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _totalGameTime += dt;
        _input.Update();

        if (ToastTimer > 0f)
        {
            ToastTimer -= dt;
        }

        // Toggle Debug Mode with F3
        if (_input.Pressed(Keys.F3))
        {
            _debugOverlay.IsVisible = !_debugOverlay.IsVisible;
            Audio.PlayCue("ui_blip");
        }

        // Adjust Fog based on zone and config scale
        var baseFog = World.IsInBasement(Player.Position) ? FogSettings.Basement : FogSettings.Default;
        CurrentFog = new FogSettings
        {
            FogStart = baseFog.FogStart * Config.FogDistanceScale,
            FogEnd = baseFog.FogEnd * Config.FogDistanceScale,
            FogColor = baseFog.FogColor,
            FogDensity = baseFog.FogDensity
        };

        switch (CurrentScreen)
        {
            case ScreenState.MainMenu:
                _mainMenuUI.Update(dt, _input, this);
                break;

            case ScreenState.Intro:
                _introSequence.Update(dt, _input, this);
                break;

            case ScreenState.Playing:
                UpdatePlayingState(dt);
                break;

            case ScreenState.Paused:
                _pauseMenu.Update(_input, this);
                break;

            case ScreenState.Inventory:
                _inventoryUI.Update(_input, Inventory, Player, this);
                if (_input.Pressed(Keys.Escape) || _input.Pressed(Keys.I) || _input.Pressed(Keys.Tab))
                {
                    ResumeGame();
                }
                break;

            case ScreenState.Dead:
                _gameOverUI.Update(_input, this);
                break;

            case ScreenState.Ending:
                if (!string.IsNullOrEmpty(_activeDocTitle))
                {
                    if (_input.Pressed(Keys.Escape) || _input.Pressed(Keys.E) || _input.Pressed(Keys.Enter))
                    {
                        _activeDocTitle = string.Empty;
                        _activeDocContent = string.Empty;
                        ResumeGame();
                    }
                }
                else
                {
                    _endingUI.Update(_input, this);
                }
                break;
        }

        _debugOverlay.Update(dt);
        base.Update(gameTime);
    }

    private void UpdatePlayingState(float dt)
    {
        // Pause Game on ESC
        if (_input.Pressed(Keys.Escape))
        {
            _pauseMenu.Reset();
            SetScreenState(ScreenState.Paused);
            Audio.PlayCue("ui_blip");
            return;
        }

        // Open Inventory on I or Tab
        if (_input.Pressed(Keys.I) || _input.Pressed(Keys.Tab))
        {
            SetScreenState(ScreenState.Inventory);
            Audio.PlayCue("ui_blip");
            return;
        }

        // Handle Active Dialogue Sequence
        if (Dialogue.IsActive)
        {
            _dialogueUI.Update(dt, Dialogue, this);
            if (_input.Pressed(Keys.E) || _input.Pressed(Keys.Enter) || _input.Pressed(Keys.Space))
            {
                if (!Dialogue.Advance(this))
                {
                    // Dialogue finished
                }
                else
                {
                    _dialogueUI.ResetTypewriter();
                }
            }
            return;
        }

        // Mouse look orbiting with GameConfig
        var currentMouse = Mouse.GetState();
        int deltaX = currentMouse.X - _screenCenter.X;
        int deltaY = currentMouse.Y - _screenCenter.Y;
        if (IsActive && (deltaX != 0 || deltaY != 0))
        {
            Camera.ProcessMouseInput(deltaX, deltaY, Config);
            Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
        }

        // Player & Enemies update with GameConfig
        Controller.Update(dt, Player, Camera, World, _input, this, Config);
        Camera.Update(Player, World, GraphicsDevice.Viewport.AspectRatio, dt, Config);
        Enemies.Update(dt, Player, World, this);
        Interactions.Update(Player, this);
        Particles.Update(dt, Player.Position);

        // Check Player Death
        if (Player.Health <= 0f)
        {
            SetScreenState(ScreenState.Dead);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(CurrentFog.FogColor);

        int screenWidth = GraphicsDevice.Viewport.Width;
        int screenHeight = GraphicsDevice.Viewport.Height;

        if (CurrentScreen == ScreenState.MainMenu)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _mainMenuUI.Draw(_spriteBatch, _font, Config, screenWidth, screenHeight);
            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        // 3D Scene Rendering
        _primitiveRenderer.Begin(Camera.View, Camera.Projection, CurrentFog.FogColor, CurrentFog.FogStart, CurrentFog.FogEnd);

        // 1. World Geometry
        World.Draw(_primitiveRenderer, _totalGameTime, State.PoliceStationUnlocked);

        // 2. NPCs (Clara, Arthur, Vance, Father Thomas)
        foreach (var npc in NPCs.AllNPCs)
        {
            _characterRenderer.DrawNPC(npc, _totalGameTime);
        }

        // 3. Enemies (Fog Walkers)
        foreach (var enemy in Enemies.Enemies)
        {
            _characterRenderer.DrawFogWalker(enemy);
        }

        // 4. 3D Item Pickups
        foreach (var item in Interactions.All)
        {
            _characterRenderer.DrawItemPickup(item, _totalGameTime);
        }

        // 5. Player Character
        _characterRenderer.DrawPlayer(Player);

        // 6. Particles (Mist, Muzzle sparks, Hit splatters)
        Particles.Draw(_primitiveRenderer);

        // 2D Screen UI Rendering
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        switch (CurrentScreen)
        {
            case ScreenState.Intro:
                _introSequence.Draw(_spriteBatch, _font, screenWidth, screenHeight);
                break;

            case ScreenState.Playing:
                _hud.Draw(_spriteBatch, _font, this, screenWidth, screenHeight);
                if (Dialogue.IsActive)
                {
                    _dialogueUI.Draw(_spriteBatch, _font, Dialogue, screenWidth, screenHeight);
                }
                break;

            case ScreenState.Paused:
                _hud.Draw(_spriteBatch, _font, this, screenWidth, screenHeight);
                _pauseMenu.Draw(_spriteBatch, _font, Config, screenWidth, screenHeight);
                break;

            case ScreenState.Inventory:
                _inventoryUI.Draw(_spriteBatch, _font, Inventory, Player, screenWidth, screenHeight);
                break;

            case ScreenState.Dead:
                _gameOverUI.Draw(_spriteBatch, _font, screenWidth, screenHeight);
                break;

            case ScreenState.Ending:
                if (!string.IsNullOrEmpty(_activeDocTitle))
                {
                    _documentUI.Draw(_spriteBatch, _font, _activeDocTitle, _activeDocContent, screenWidth, screenHeight);
                }
                else
                {
                    _endingUI.Draw(_spriteBatch, _font, screenWidth, screenHeight);
                }
                break;
        }

        _debugOverlay.Draw(_spriteBatch, _font, this, screenWidth, screenHeight);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _font.Dispose();
            _textures.Dispose();
            _primitiveRenderer.Dispose();
            Audio.Dispose();
        }
        base.Dispose(disposing);
    }
}
