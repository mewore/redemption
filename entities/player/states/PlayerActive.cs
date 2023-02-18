using Godot;

public partial class PlayerActive : PlayerState
{
    private Timer flyRequestTimer;

    public override void _Ready()
    {
        base._Ready();
        flyRequestTimer = GetNode<Timer>("FlyRequestTimer");
    }

    public override void Enter()
    {
        flyRequestTimer.Stop();
    }

    public override void PhysicsProcess(float delta) => player.Move(delta);

    public void _on_Player_FlyRequested()
    {
        targetState = FLYING;
    }

    public override void UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("jump"))
        {
            if (flyRequestTimer.IsStopped())
            {
                flyRequestTimer.Start();
            }
            else
            {
                player.InitiateFlight();
            }
        }
    }
}
