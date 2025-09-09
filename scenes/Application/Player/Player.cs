using Godot;
using System;
using System.Numerics;

public partial class Player : CharacterBody3D
{
    //
    // Dependent Nodes
    //
    [Export] NodePath Camera3DPath;
    Camera3D Camera;

    [Export] NodePath RayCastPath;
    RayCast3D RayCast;

    [Export] NodePath terrainPath;
    Terrain terrain;

    //
    // Data
    //

    Godot.Vector3 Direction;
    [Export] float speed = 500.0f;
    const float sensitivity = 0.002f;

    bool NoclipEnabled = true;




    //[Export] NodePath TerrainPath;
    //MCTerrain GameTerrain;

    // bude držet odkaz na terén.
    // v terénu bude funkce pro získání collision objektu.
    // také funkce pro modifikaci terénu.
    // tyto funkce se budou volat v player.

    //
    // Funkce
    //

    public override void _Ready()
    {
        // získáme referenci na kameru
        Camera = GetNode<Camera3D>(Camera3DPath);
        RayCast = GetNode<RayCast3D>(RayCastPath);

        terrain = GetNode<Terrain>(terrainPath);
        //GameTerrain = GetNode<MCTerrain>(TerrainPath);

    }

    public override void _Process(double delta)
    {
        // tohle je třeba volat každý snímek pro plynulý pohyb.
        MoveAndSlide();

    }
    public override void _PhysicsProcess(double delta)
    {
        // není třeba zpracovávat každý snímek. (bude voláno 60x za sekundu)
        ProcessInput((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                PrecessCameraRotation(mouseMotion);
            }

            if (@event is InputEventMouseButton mouseButton)
            {
                //GD.Print("mouse button event: ", mouseButton);
            }
        }
    }


    //
    // private
    //

    private void ProcessInput(float delta)
    {
        Direction = new Godot.Vector3();
        //GlobalTransform
        Godot.Vector3 InputDirection = new Godot.Vector3();

        if (Input.IsActionPressed("move_forward"))
        {
            InputDirection.Z += -1;
        }
        if (Input.IsActionPressed("move_back"))
        {
            InputDirection.Z += 1;
        }
        if (Input.IsActionPressed("move_right"))
        {
            InputDirection.X += 1;
        }
        if (Input.IsActionPressed("move_left"))
        {
            InputDirection.X += -1;
        }
        if (Input.IsActionPressed("jump"))
        {
            InputDirection.Y += 1;
        }
        InputDirection = InputDirection.Normalized();


        Direction += Camera.GlobalTransform.Basis * InputDirection;

        ProcessNoclipMovement(delta);

        ProcessTerraforming();

        ProcessCameraState();
        Processteleport();
        ProcessVsync();
    }

    private void ProcessMovement(float delta)
    {
        Godot.Vector3 velocity = Velocity;

        if (Direction.Y != 0.0f)
        {
            velocity.Y = Direction.Y * 16.0f;
        }

        velocity.Y += -32.0f * delta;

        //velocity = Direction * speed * delta;
        velocity.X = Direction.X * speed * delta;
        velocity.Z = Direction.Z * speed * delta;

        Velocity = velocity;
    }

    private void PrecessCameraRotation(InputEventMouseMotion mouseMotion)
    {
        Camera.RotateX(-mouseMotion.Relative.Y * sensitivity);
        RotateY(-mouseMotion.Relative.X * sensitivity);
    }


    private void ProcessNoclipMovement(float delta)
    {
        Godot.Vector3 velocity = Velocity;

        velocity = Direction * speed * delta;

        Velocity = velocity;
    }

    private void ProcessCameraState()
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }

    private void Processteleport()
    {
        if (Input.IsActionJustPressed("E"))
        {
            Position = new(2.0f, 2.0f, 2.0f);
        }
    }

    private void ProcessVsync()
    {
        if (Input.IsActionJustPressed("V"))
        {
            if (DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Disabled)
            {
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
            }
            else
            {
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            }
        }
    }

    private void ProcessTerraforming()
    {
        if (Input.IsActionPressed("mouse_left") && RayCast.IsColliding())
        {
            GD.Print("intersecting: ", RayCast.GetCollider(), " at: ", RayCast.GetCollisionPoint());
            terrain.ModifyTerrain(RayCast.GetCollisionPoint());
        }
    }

}

/*
* Změny
*
* [28.06.2025] myslím že jsem to vytvořil hnedka první den.
* [18.07.2025] předěláno do scény a zorganizováno.
*
*/
