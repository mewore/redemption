using Godot;

public partial class PlayerFlying : PlayerState
{
    public override void PhysicsProcess(float delta) => player.Fly(delta);

    public void _on_Player_LandRequested()
    {
        targetState = ACTIVE;
    }
}
