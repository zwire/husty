using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Husty.Extensions;

public static class ObservableEx2
{

  public static IObservable<object?> Loop(
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var observable = Observable
        .Repeat(0, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Select(_ => (object)null!)
        .Publish();
    observable.Connect();
    return observable;
  }

  public static IObservable<int> Loop(
      TimeSpan interval,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var counter = 0;
    var observable = Observable
        .Interval(interval, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Select(_ => counter++)
        .Publish();
    observable.Connect();
    return observable;
  }

  public static IObservable<object?> Loop(
      Action action,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var observable = Observable
        .Repeat(0, int.MaxValue, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Select(_ => { action(); return (object)null!; })
        .Publish();
    observable.Connect();
    return observable;
  }

  public static IObservable<int> Loop(
      Action<int> action,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var observable = Observable
        .Range(0, int.MaxValue, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Do(i => action(i))
        .Publish();
    observable.Connect();
    return observable;
  }

  public static void Loop(
      Action<long> action,
      TimeSpan interval,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var observable = Observable
        .Interval(interval, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Do(i => action(i))
        .Publish();
    observable.Connect();
  }

  public static IObservable<T> Loop<T>(
      Func<T> func,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var observable = Observable
        .Repeat(0, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Select(_ => func())
        .Publish();
    observable.Connect();
    return observable;
  }

  public static IObservable<T> Loop<T>(
      Func<T> func,
      TimeSpan interval,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var observable = Observable
        .Interval(interval, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Select(_ => func())
        .Publish();
    observable.Connect();
    return observable;
  }

  public static IObservable<(int Index, T Value)> Loop<T>(
      Func<int, T> func,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var observable = Observable
        .Range(0, int.MaxValue, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Select(i => (i, func(i)))
        .Publish();
    observable.Connect();
    return observable;
  }

  public static IObservable<(int Index, T Value)> Loop<T>(
      Func<int, T> func,
      TimeSpan interval,
      IScheduler? scheduler = default,
      CancellationToken ct = default
  )
  {
    var counter = 0;
    var observable = Observable
        .Interval(interval, scheduler ?? new EventLoopScheduler())
        .TakeUntil(_ => ct.IsCancellationRequested)
        .Select(_ => (counter, func(counter++)))
        .Publish();
    observable.Connect();
    return observable;
  }

  public static IObservable<T[]> WithHistory<T>(this IObservable<T> src, int count)
  {
    if (count < 1)
      throw new ArgumentException("count must be >= 1");
    return src.Select(x => src.TakeLast(count + 1).ToEnumerable().ToArray());
  }

}
