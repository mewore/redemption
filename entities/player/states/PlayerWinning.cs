using Godot;

public partial class PlayerWinning : PlayerState
{
    public override void PhysicsProcess(float delta)
    {
        player.Move(delta, false);
    }

    public override void Enter()
    {
        GetNode<Timer>("WinDuration").Start();
    }

    public override void Exit()
    {
        GetNode<Timer>("WinDuration").Stop();
    }
}
