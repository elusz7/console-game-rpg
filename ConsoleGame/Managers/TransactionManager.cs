using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Data;

namespace ConsoleGame.Managers;

public class TransactionManager(GameContext context) : ITransactionManager
{
    private readonly GameContext _context = context;

    public void ExecuteInTransaction(Action action)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            action();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}