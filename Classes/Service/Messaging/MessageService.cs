using Bamboozlers.Classes.Events;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Service.Messaging;

public class MessageService : IMessageService
{
    private IDbContextFactory<AppDbContext.AppDbContext> _db;
    
    public void Init(IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory)
    {
        _db = dbContextFactory;
        MessageEvents.MessageSent.Register(async (_, _, _, _, _, _, mesageSupplier, _) =>
        {
            await using var db = await _db.CreateDbContextAsync();
            var message = mesageSupplier?.Invoke();
            if (message == null) return null;
            
            db.Messages.Add(message);
            await db.SaveChangesAsync();
            return message;
        });
    }
}

public interface IMessageService
{
    public void Init(IDbContextFactory<AppDbContext.AppDbContext> db);
}