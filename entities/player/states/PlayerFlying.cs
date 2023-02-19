using Godot;

public partial class PlayerFlying : PlayerState
{
    private Timer dashCooldownTimer;

    public override void _Ready()
    {
        base._Ready();
        dashCooldownTimer = GetNode<Timer>("DashCooldown");
    }

    public override void PhysicsProcess(float delta)
    {
        player.Fly(delta);
        player.DetectTwigContainer();

        if (Input.IsActionPressed("dash") && dashCooldownTimer.IsStopped())
        {
            player.Dash();
            dashCooldownTimer.Start();
        }
    }

    public void _on_Player_LandRequested()
    {
        targetState = ACTIVE;
    }
}
