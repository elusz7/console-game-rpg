namespace ConsoleGame.Managers.Interfaces;

public interface ITransactionManager
{
    void ExecuteInTransaction(Action action);
}