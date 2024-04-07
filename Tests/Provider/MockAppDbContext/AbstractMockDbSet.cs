using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Tests.Provider.MockAppDbContext;

// API Class
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public abstract class AbstractMockDbSet<T>(MockAppDbContext mockAppDbContext) where T : class
{
    protected MockAppDbContext MockAppDbContext { get; } = mockAppDbContext;
    public abstract void AddMock(T entry);
    public abstract void RemoveMock(T entry);

    public Mock<DbSet<T>> AddMock(T entry,
        Mock<DbSet<T>> entries,
        Func<T, T, bool> matchPredicate)
    {
        return MockAppDbContext.AddMockDbEntry(
            entry,
            entries,
            matchPredicate
        );
    }

    public Mock<DbSet<T>> RemoveMock(T entry,
        Mock<DbSet<T>> entries,
        Func<T, T, bool> matchPredicate)
    {
        return MockAppDbContext.RemoveMockDbEntry(
            entry,
            entries,
            matchPredicate
        );
    }

    public abstract void UpdateMock(T entry);

    public abstract T? FindMock(int idx);

    public abstract void ClearAll();
}