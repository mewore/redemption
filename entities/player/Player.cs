using Godot;
using System;

public partial class Player : KinematicBody2D
{
    // Movement
    [Export]
    private float acceleration = 4000.0f;

    [Export]
    private float maxSpeed = 400.0f;

    // Jumping
    private float jumpSpeed;

    [Export(PropertyHint.Range, "0,1")]
    private float jumpSpeedRetention = .5f;

    [Export]
    private float jumpGraceTime = .1f;
    private double now = .0;
    private float lastWantedToJumpAt = -Mathf.Inf;
    private float lastAbleToJumpAt = -Mathf.Inf;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = (int)ProjectSettings.GetSetting("physics/2d/default_gravity");

    private Sprite sprite;

    private Vector2 velocity;
    public bool CanMove = true;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        jumpSpeed = Mathf.Sqrt(gravity * (-GetNode<Node2D>("JumpHeight").Position.y) * GlobalScale.y);
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

        // Handle Jump.
        if (Input.IsActionJustReleased("jump") && velocity.y < 0f)
        {
            velocity.y *= jumpSpeedRetention;
        }
        if (Input.IsActionJustPressed("jump"))
        {
            lastWantedToJumpAt = (float)now;
        }
        if (IsOnFloor())
        {
            lastAbleToJumpAt = (float)now;
        }
        else
        {
            velocity.y += gravity * (float)delta;
        }
        if (canControl && Mathf.Min(lastWantedToJumpAt, lastAbleToJumpAt) + jumpGraceTime > (float)now)
        {
            velocity.y = -jumpSpeed;
            lastWantedToJumpAt = -Mathf.Inf;
        }

        float desiredVelocityX = canControl ? Input.GetAxis("move_left", "move_right") * maxSpeed : 0f;
        velocity.x = Mathf.MoveToward(velocity.x, desiredVelocityX, acceleration * (float)delta);

        int lookSign = Mathf.Sign(desiredVelocityX);
        if (lookSign != 0 && lookSign != Mathf.Sign(sprite.Scale.x))
        {
            sprite.Scale = new Vector2(-sprite.Scale.x, sprite.Scale.y);
        }

        velocity = MoveAndSlide(velocity, Vector2.Up);
    }
}
