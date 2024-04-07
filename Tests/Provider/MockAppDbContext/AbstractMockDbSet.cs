using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

public abstract class AbstractMockDbSet<T>(MockAppDbContext mockAppDbContext) where T : class
{
    protected MockAppDbContext MockAppDbContext { get; set; } = mockAppDbContext;
    protected abstract Func<T, T, bool> MatchPredicate { get; set; }
    public Mock<DbSet<T>> MockDbSet { get; set; }
    
    public virtual void AddMock(T entry)
    {
        MockDbSet = MockAppDbContext.AddMockDbEntry(
            entry,
            MockDbSet,
            MatchPredicate
        );
        RebindMocks();
    }
    
    public virtual void RemoveMock(T entry)
    {
        MockDbSet = MockAppDbContext.RemoveMockDbEntry(
            entry,
            MockDbSet,
            MatchPredicate
        );
        RebindMocks();
    }

    public virtual void UpdateMock(T entry)
    {
        RemoveMock(entry);
        AddMock(entry);
    }

    public abstract void RebindMocks();
    
    public T? FindMock(int idx)
    {
        return MockDbSet.Object.Skip(idx - 1).FirstOrDefault();
    }

    public DbSet<T> GetMocks()
    {
        return MockDbSet.Object;
    }
    
    public void ClearAll()
    {
        var list = MockDbSet.Object.ToList();
        foreach (var item in list)
        {
            RemoveMock(item);
        }
    }
}