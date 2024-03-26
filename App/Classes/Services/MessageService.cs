using Bamboozlers.Classes.Events;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.Services;

public class MessageService : IMessageService
{
    private IDbContextFactory<AppDbContext.AppDbContext> _db;
    
    public void Init(IDbContextFactory<AppDbContext.AppDbContext> dbContextFactory)
    {
        _db = dbContextFactory;
        MessageEvents.MessageCreated.Register(async message =>
        {
            if (message.ID != 0)
            {
                // Message already exists in db as ID is auto-assigned
                return message;
            }
            await using var db = await _db.CreateDbContextAsync();
            
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