public partial class PlayerState : State
{
    protected const string ACTIVE = "Active";
    protected const string FLYING = "Flying";

    protected Player player;

    public override void _Ready()
    {
        player = GetOwner<Player>();
    }
}
