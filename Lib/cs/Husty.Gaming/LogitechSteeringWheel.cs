// Driver: https://support.logi.com/hc/ja/articles/360025298053
// G-HUB : https://support.logi.com/hc/ja/articles/360024850133

using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace Husty.Gaming;

public class LogitechSteeringWheel : IDisposable
{

    // ------ fields ------ //

    private readonly Freq _freq;
    private readonly BehaviorSubject<Button[]> _buttons;
    private readonly BehaviorSubject<float> _steering;
    private readonly BehaviorSubject<float> _accel;
    private readonly BehaviorSubject<float> _brake;
    private readonly BehaviorSubject<float> _clutch;
    private CancellationTokenSource _cts;
    private Task _task;


    // ------ properties ------ //


    public IObservable<Button[]> Buttons => _buttons;

    // -1.0 ~ +1.0
    public IObservable<float> Steering => _steering;

    // 0.0 ~ 1.0
    public IObservable<float> AccelPedal => _accel;

    // 0.0 ~ 1.0
    public IObservable<float> BrakePedal => _brake;

    // 0.0 ~ 1.0
    public IObservable<float> ClutchPedal => _clutch;


    // ------ constructors ------ //

    public LogitechSteeringWheel(Freq? frequancy = default)
    {
        _freq = frequancy ?? new Freq(20);
        _buttons = new(Array.Empty<Button>());
        _steering = new(0f);
        _accel = new(0f);
        _brake = new(0f);
        _clutch = new(0f);
    }


    // ------ public methods ------ //

    public async Task StartAsync()
    {
        _cts = new();
        for (int i = 0; i < 50; i++)
        {
            await Task.Delay(100).ConfigureAwait(false);
            if (LogitechSteeringWheelLib.SteeringInitialize(false) &&
                LogitechSteeringWheelLib.Update() &&
                LogitechSteeringWheelLib.IsConnected(0)
            ) break;
            if (i is 49) throw new Exception("failed to initialize controller");
        }
        var first = true;
        var steering0 = 0f;
        var accel0 = 0f;
        var brake0 = 0f;
        var clutch0 = 0f;
        var buttons0 = Array.Empty<Button>();
        _task = Observable
            .Interval(_freq.TimeSpan, ThreadPoolScheduler.Instance)
            .TakeUntil(_ => _cts.IsCancellationRequested)
            .Finally(LogitechSteeringWheelLib.SteeringShutdown)
            .Do(_ =>
            {
                if (
                    LogitechSteeringWheelLib.Update() &&
                    LogitechSteeringWheelLib.IsConnected(0) &&
                    LogitechSteeringWheelLib.GetState(0) is LogitechSteeringWheelLib.DIJOYSTATE2ENGINES r
                )
                {
                    var steering = r.lX / 32768f;
                    var accel = 0.5f - r.lY / 65536f;
                    var brake = 0.5f - r.lRz / 65536f;
                    var clutch = 0.5f - r.rglSlider[0] / 65536f;
                    if (first)
                    {
                        steering0 = steering;
                        accel0 = accel;
                        brake0 = brake;
                        clutch0 = clutch;
                        first = false;
                    }
                    if (steering != steering0)
                    {
                        steering0 = steering;
                        _steering.OnNext(steering);
                    }
                    if (accel != accel0)
                    {
                        accel0 = accel;
                        _accel.OnNext(accel > 0.01f ? accel : 0f);
                    }
                    if (brake != brake0)
                    {
                        brake0 = brake;
                        _brake.OnNext(brake > 0.01f ? brake : 0f);
                    }
                    if (clutch != clutch0)
                    {
                        clutch0 = clutch;
                        _clutch.OnNext(clutch > 0.01f ? clutch : 0f);
                    }
                    var list = new List<Button>();
                    for (int i = 0; i < r.rgbButtons.Length; i++)
                        if (r.rgbButtons[i] > 0)
                            list.Add((Button)i);
                    var dpad = r.rgdwPOV[0];
                    if (dpad is 0 or 4500 or 31500) list.Add(Button.Up);
                    if (dpad is 4500 or 9000 or 13500) list.Add(Button.Right);
                    if (dpad is 13500 or 18000 or 22500) list.Add(Button.Down);
                    if (dpad is 22500 or 27000 or 31500) list.Add(Button.Left);
                    var buttons = list.ToArray();
                    if (!buttons.SequenceEqual(buttons0))
                    {
                        buttons0 = buttons;
                        _buttons.OnNext(buttons);
                    }
                }
            })
            .ToTask();
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        if (_task is Task t)
            await t.ConfigureAwait(false);
    }

    public void Dispose()
    {
        StopAsync().Wait();
    }

}

public enum Button
{
    Cross                   = 0,
    Rectangle               = 1,
    Circle                  = 2,
    Triangle                = 3,
    ShiftR                  = 4,
    ShiftL                  = 5,
    R2                      = 6,
    L2                      = 7,
    Share                   = 8,
    Option                  = 9,
    R3                      = 10,
    L3                      = 11,
    Plus                    = 19,
    Minus                   = 20,
    SelectClockwise         = 21,
    SelectCounterClockwise  = 22,
    Enter                   = 23,
    PlayStation             = 24,
    Up                      = 996,
    Down                    = 997,
    Left                    = 998,
    Right                   = 999
}