namespace PedaleandoGame.Managers
{
    /// <summary>
    /// Defines the contract for game management operations
    /// Following Dependency Inversion Principle
    /// </summary>
    public interface IGameManager
    {
        event System.Action<int, int> CountChanged;
        
        int CurrentCount { get; }
        int CurrentGoal { get; }

        void AddTrash(int amount);
        void Reset(int newGoal = 20);
    }
}