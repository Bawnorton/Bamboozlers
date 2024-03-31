using System.Linq.Expressions;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Tests.Provider.MockAppDbContext;

namespace Tests.Provider;

public class MockDatabaseProvider
{
    private readonly Mock<IDbContextFactory<AppDbContext>> _mockDbContextFactory;
    private readonly MockAppDbContext.MockAppDbContext _mockAppDbContext;
    public MockDatabaseProvider(TestContextBase ctx)
    {
        _mockDbContextFactory = new Mock<IDbContextFactory<AppDbContext>>();

        var options = new DbContextOptions<AppDbContext>();
        var mockContext = new Mock<AppDbContext>(options);
        _mockAppDbContext = new MockAppDbContext.MockAppDbContext(mockContext);
        
        _mockDbContextFactory.Setup(x => x.CreateDbContext()).Returns(_mockAppDbContext.MockDbContext.Object);
        _mockDbContextFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(_mockAppDbContext.MockDbContext.Object);
        
        ctx.Services.AddSingleton(GetDbContextFactory());
    }

    public MockAppDbContext.MockAppDbContext GetMockAppDbContext()
    {
        return _mockAppDbContext;
    }
    public IDbContextFactory<AppDbContext> GetDbContextFactory()
    {
        return _mockDbContextFactory.Object;
    }
}