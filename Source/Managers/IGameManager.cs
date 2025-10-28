namespace PedaleandoGame.Managers
{
    /// <summary>
    /// Defines the contract for game management operations
    /// Following Dependency Inversion Principle
    /// </summary>
    public interface IGameManager
    {
        void AddTrash(int amount);
    }
}