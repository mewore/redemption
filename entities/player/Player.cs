using Godot;

public partial class Player : KinematicBody2D
{
    [Signal]
    delegate void FlyRequested();

    [Signal]
    delegate void LandRequested();

    [Signal]
    delegate void DroppedAllTwigs();

    [Signal]
    delegate void ReachedTwigContainer();

    // Movement
    [Export]
    private float acceleration = 4000.0f;

    [Export]
    private float maxSpeed = 300.0f;

    // Jumping
    private float jumpSpeed;

    [Export(PropertyHint.Range, "0,1")]
    private float jumpSpeedRetention = .5f;

    private float jumpFlyY;

    [Export]
    private float jumpGraceTime = .1f;
    private double now = .0;
    private float lastWantedToJumpAt = -Mathf.Inf;
    private float lastAbleToJumpAt = -Mathf.Inf;

    // Flying

    [Export]
    private float flyAcceleration = 4000.0f;

    [Export]
    private float flyMaxSpeed = 800.0f;

    [Export]
    private float dashSpeed = 2000.0f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = (int)ProjectSettings.GetSetting("physics/2d/default_gravity");

    private Sprite sprite;

    private Vector2 velocity;
    public bool CanMove = true;

    [Export]
    private int maxTwigsWhenWalking = 10;

    [Export]
    private int maxTwigsWhenFlying = 5;

    private int maxTwigs;
    public int MaxTwigs => maxTwigs;
    private int currentTwigs;
    public int CurrentTwigs => currentTwigs;
    public bool CanCarryMoreTwigs { get => currentTwigs < maxTwigs; }
    public bool Overencumbered { get => currentTwigs > maxTwigs; }

    RayCast2D twigContainerRayCast;
    Node2D twigContainer;
    float detectionRangeSquared;

    private AnimationPlayer animationPlayer;

    private int helpers = 0;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        jumpSpeed = Mathf.Sqrt(gravity * -GetNode<Node2D>("JumpHeight").Position.y * GlobalScale.y);
        maxTwigs = maxTwigsWhenWalking;

        var twigContainerNodes = GetTree().GetNodesInGroup("twig_container");
        if (twigContainerNodes.Count > 0)
        {
            twigContainer = twigContainerNodes[0] as Node2D;
        }

        twigContainerRayCast = GetNode<RayCast2D>("TwigContainerRayCast");
        detectionRangeSquared = (twigContainerRayCast.CastTo * GlobalScale).LengthSquared();

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _PhysicsProcess(float delta)
    {
        now += delta;
    }

    public void Move(float delta, bool canControl = true)
    {
        if (!CanMove)
        {
            return;
        }
        maxTwigs = maxTwigsWhenWalking + helpers;

        // Handle Jump.
        if (Input.IsActionJustReleased("jump") && velocity.y < 0f)
        {
            velocity.y *= jumpSpeedRetention;
        }

        if (IsOnFloor())
        {
            lastAbleToJumpAt = (float)now;
            jumpFlyY = -Mathf.Inf;
        }
        else
        {
            velocity.y += gravity * (float)delta;
        }

        if (Input.IsActionJustPressed("jump"))
        {
            if (GlobalPosition.y < jumpFlyY || velocity.y < 0f)
            {
                InitiateFlight();
            }
            else
            {
                lastWantedToJumpAt = (float)now;
            }
        }

        if (canControl && Mathf.Min(lastWantedToJumpAt, lastAbleToJumpAt) + jumpGraceTime > (float)now)
        {
            velocity.y = -jumpSpeed;
            lastWantedToJumpAt = -Mathf.Inf;
            jumpFlyY = GlobalPosition.y + GetNode<Node2D>("JumpHeight").Position.y * GlobalScale.y * .2f;
        }

        float desiredVelocityX = canControl ? Input.GetAxis("move_left", "move_right") * maxSpeed : 0f;
        if (Mathf.Abs(desiredVelocityX) > .01f)
        {
            if (animationPlayer.CurrentAnimation != "walking")
            {
                animationPlayer.Play("walking");
            }
        }
        else if (animationPlayer.CurrentAnimation != "standing")
        {
            animationPlayer.Play("standing");
        }
        velocity.x = Mathf.MoveToward(velocity.x, desiredVelocityX, acceleration * (float)delta);

        int lookSign = Mathf.Sign(desiredVelocityX);
        if (lookSign != 0 && lookSign != Mathf.Sign(sprite.Scale.x))
        {
            sprite.Scale = new Vector2(-sprite.Scale.x, sprite.Scale.y);
        }

        velocity = MoveAndSlide(velocity, Vector2.Up);
    }

    public void InitiateFlight()
    {
        jumpFlyY = -Mathf.Inf;
        velocity.y = -jumpSpeed;
        EmitSignal(nameof(FlyRequested));
        lastWantedToJumpAt = -jumpGraceTime;
    }

    public void Fly(float delta)
    {
        if (!CanMove)
        {
            return;
        }
        maxTwigs = maxTwigsWhenFlying + helpers;

        Vector2 desiredVelocity = new Vector2(Input.GetAxis("move_left", "move_right"), Input.GetAxis("fly_up", "fly_down")).Normalized() * flyMaxSpeed;
        velocity = velocity.MoveToward(desiredVelocity, flyAcceleration * (float)delta);

        int lookSign = Mathf.Sign(desiredVelocity.x);
        if (lookSign != 0 && lookSign != Mathf.Sign(sprite.Scale.x))
        {
            sprite.Scale = new Vector2(-sprite.Scale.x, sprite.Scale.y);
        }

        velocity = MoveAndSlide(velocity, Vector2.Up);

        if (IsOnFloor() && Input.IsActionPressed("fly_down"))
        {
            EmitSignal(nameof(LandRequested));
        }
    }

    public void Dash()
    {
        GD.Print("Dash!");
        velocity = new Vector2(Input.GetAxis("move_left", "move_right"), Input.GetAxis("fly_up", "fly_down")).Normalized() * dashSpeed;
    }

    public void AddHelper()
    {
        helpers++;
        maxTwigs++;
    }

    public void RemoveHelper()
    {
        helpers--;
        maxTwigs--;
    }

    public bool PickTwigUp()
    {
        if (currentTwigs >= maxTwigs)
        {
            return false;
        }
        ++currentTwigs;
        return true;
    }

    public void ReleaseTwig() => --currentTwigs;

    public void DetectTwigContainer()
    {
        if (twigContainer == null || currentTwigs == 0)
        {
            return;
        }

        Vector2 distance = twigContainer.GlobalPosition - GlobalPosition;

        if (distance.LengthSquared() > detectionRangeSquared)
        {
            return;
        }
        twigContainerRayCast.CastTo = distance / GlobalScale;
        twigContainerRayCast.ForceRaycastUpdate();
        if (twigContainerRayCast.IsColliding())
        {
            return;
        }
        EmitSignal(nameof(ReachedTwigContainer));
        currentTwigs = 0;
    }
}
