namespace Husty;

public struct Result<T>
{

  private readonly bool _ok;
  private readonly T? _value;
  private readonly Exception? _error;

  public Result()
  {
    _ok = false;
    _value = default;
    _error = new Exception();
  }

  private Result(T value)
  {
    _ok = true;
    _value = value;
    _error = default;
  }

  private Result(Exception? e)
  {
    _ok = false;
    _value = default;
    _error = e ?? new Exception();
  }

  public readonly bool IsOk => _ok;

  public readonly T Unwrap()
  {
    if (_ok)
      return _value!;
    else
      throw _error!;
  }

  public readonly U Match<U>(Func<T, U> ok, Func<Exception, U> err)
  {
    return _ok ? ok(_value!) : err(_error!);
  }

  public readonly void Match(Action<T> ok, Action<Exception> err)
  {
    if (_ok)
      ok(_value!);
    else
      err(_error!);
  }

  public static Result<T> Ok(T value) => new(value);

  public static Result<T> Err(Exception? e) => new(e);

}