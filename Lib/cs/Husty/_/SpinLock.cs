namespace Husty;

public class SpinLock
{

    private System.Threading.SpinLock _locker;

    public SpinLock()
    {
        _locker = new();
    }

    public void Safeguard(Action any)
    {
        var gotLock = false;
        try
        {
            _locker.Enter(ref gotLock);
            any.Invoke();
        }
        finally
        {
            if (gotLock) _locker.Exit();
        }
    }

}
