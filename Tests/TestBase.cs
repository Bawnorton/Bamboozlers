namespace Tests;

public class TestBase : IDisposable
{
    protected readonly TestContext Ctx;
    
    protected TestBase()
    {
        Ctx = new TestContext();
    }

    public void Dispose()
    {
        Ctx.Dispose();
        GC.SuppressFinalize(this);
    }
}