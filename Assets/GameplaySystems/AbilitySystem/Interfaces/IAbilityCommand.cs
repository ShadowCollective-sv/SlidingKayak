namespace HauntedHouses.GameplaySystems.AbilitySystem.Interfaces
{
    public interface IAbilityCommand
    {
        void Execute(IMovementController controller);
    }
}