using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Tests.Provider.MockAppDbContext;

public class MockEntityEntry<T> where T : class
{
    private Mock<EntityEntry<T>> MoqEntityEntry { get; set; }
    
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public MockEntityEntry(T model)
    {
        var entityTypeMock = new Mock<IRuntimeEntityType>();
        entityTypeMock.SetupGet(x => x.EmptyShadowValuesFactory)
            .Returns(() => new Mock<ISnapshot>().Object);
        entityTypeMock.SetupGet(x => x.Counts)
            .Returns(new PropertyCounts(1, 1, 1, 1, 1, 1, 1));
        entityTypeMock.Setup(x => x.GetProperties())
            .Returns(Enumerable.Empty<IProperty>());
        
        var internalEntity = new InternalEntityEntry(Mock.Of<IStateManager>(), entityTypeMock.Object, model);
        MoqEntityEntry = new Mock<EntityEntry<T>>(internalEntity);
        MoqEntityEntry.SetupGet(x => x.Entity).Returns(model);
    }

    public EntityEntry<T> GetEntry()
    {
        return MoqEntityEntry.Object;
    }
}