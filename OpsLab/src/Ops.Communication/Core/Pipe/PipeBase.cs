namespace Ops.Communication.Core.Pipe;

public abstract class PipeBase : IDisposable
{
    private readonly SimpleHybirdLock _hybirdLock;

    private readonly AsyncSimpleHybirdLock _asyncHybirdLock;

    public PipeBase()
    {
        _hybirdLock = new();
        _asyncHybirdLock = new();
    }

    public void PipeLockEnter()
    {
        _hybirdLock.Enter();
    }

    public void PipeLockLeave()
    {
        _hybirdLock.Leave();
    }

    public async Task AsyncPipeLockEnter()
    {
        await _asyncHybirdLock.EnterAsync().ConfigureAwait(false);
    }

    public void AsyncPipeLockLeave()
    {
        _asyncHybirdLock.Leave();
    }

    public virtual void Dispose()
    {
        _hybirdLock?.Dispose();
        _asyncHybirdLock?.Dispose();
    }
}
