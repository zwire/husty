using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Husty.Geometry;
using Vortice.XInput;

namespace Husty.Gaming;

public class XPad : IDisposable
{

  // ------ fields ------ //

  private readonly int _index;
  private readonly IDisposable _connector;
  private readonly IConnectableObservable<GamepadButtons> _observable;
  private readonly BehaviorSubject<double> _triggerL = new(0);
  private readonly BehaviorSubject<double> _triggerR = new(0);
  private readonly BehaviorSubject<Vector2D> _stickL = new(Vector2D.Zero);
  private readonly BehaviorSubject<Vector2D> _stickR = new(Vector2D.Zero);


  // ------ properties ------ //

  public int DeviceIndex => _index;

  public IObservable<string[]> KeyPressed { get; }

  public IObservable<double> TriggerL { get; }

  public IObservable<double> TriggerR { get; }

  public IObservable<Vector2D> StickL { get; }

  public IObservable<Vector2D> StickR { get; }


  // ------ constructors ------ //

  public XPad(int index = 0)
  {
    _index = index;
    TriggerL = _triggerL;
    TriggerR = _triggerR;
    StickL = _stickL;
    StickR = _stickR;
    _observable = Observable
        .Interval(TimeSpan.FromMilliseconds(50), new EventLoopScheduler())
        .Select(_ =>
        {
          if (XInput.GetState(index, out var state))
          {
            _triggerL.OnNext(state.Gamepad.LeftTrigger / 255.0);
            _triggerR.OnNext(state.Gamepad.RightTrigger / 255.0);
            var leftX = (state.Gamepad.LeftThumbX > 0 ? state.Gamepad.LeftThumbX + 1 : state.Gamepad.LeftThumbX) / 32768.0;
            var leftY = (state.Gamepad.LeftThumbY > 0 ? state.Gamepad.LeftThumbY + 1 : state.Gamepad.LeftThumbY) / 32768.0;
            var rightX = (state.Gamepad.RightThumbX > 0 ? state.Gamepad.RightThumbX + 1 : state.Gamepad.RightThumbX) / 32768.0;
            var rightY = (state.Gamepad.RightThumbY > 0 ? state.Gamepad.RightThumbY + 1 : state.Gamepad.RightThumbY) / 32768.0;
            _stickL.OnNext(new(leftX, leftY));
            _stickR.OnNext(new(rightX, rightY));
            return state.Gamepad.Buttons;
          }
          return GamepadButtons.None;
        })
        .Publish();
    _connector = _observable.Connect();
    KeyPressed = _observable.Select(x => x.ToString().Replace(" ", "").Replace("None", "").Split(','));
  }


  // ------ public methods ------ //

  public void Dispose()
  {
    _connector.Dispose();
  }

  public void SetAction(GamepadButtons buttons, Action action)
  {
    _observable.Where(x => (x & buttons) > 0).Subscribe(_ => action());
  }

  public void SetAction(string buttons, Action action)
  {
    if (Enum.TryParse(typeof(GamepadButtons), buttons, out var btns))
      SetAction((GamepadButtons)btns!, action);
    else
      throw new ArgumentException($"{buttons} cannot be parsed to GamepadButtons type.");
  }

  /// <param name="level">0.0 ~ 1.0</param>
  public async void Vibrate(double level, TimeSpan time)
  {
    XInput.SetVibration(_index, (float)level, (float)level);
    await Task.Delay(time);
    XInput.SetVibration(_index, 0, 0);
  }

}