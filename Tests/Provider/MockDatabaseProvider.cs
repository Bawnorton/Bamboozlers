using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

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
        var mockFacade = new Mock<DatabaseFacade>(mockContext.Object);
        
        _mockAppDbContext = new MockAppDbContext.MockAppDbContext(mockContext);
        
        mockContext.Setup(x => x.Database).Returns(mockFacade.Object);
        mockFacade.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(() => new Mock<IDbContextTransaction>().Object);
        
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