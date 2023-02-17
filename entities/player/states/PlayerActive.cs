using Godot;

public partial class PlayerActive : PlayerState
{
    public override void PhysicsProcess(float delta) => player.Move(delta);
}
