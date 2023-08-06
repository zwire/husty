using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Husty.Lawicel;
using Reactive.Bindings;

namespace CanAnalyzer;

public class MainWindowViewModel : IDisposable
{

  // ------ fields ------ //

  private CanUsbAdapter? _adapter;
  private CanMessage[] _sendMessages;
  private Dictionary<string, string> _recvMessages = new();


  // ------ properties ------ //

  public ReactiveCommand StartCommand { get; }

  public ReactiveCommand StopCommand { get; }

  public ReactiveCommand SetCommand { get; } = new();

  public ReactiveCommand SearchCommand { get; } = new();

  public ReactivePropertySlim<bool> RecordingFlag { set; get; } = new(false);

  public ReactivePropertySlim<bool> ExtendedFlag { set; get; } = new(true);

  public ReactivePropertySlim<int> SelectedBaudrateIndex { set; get; } = new(6);

  public ReactivePropertySlim<int> SelectedFrequencyIndex { set; get; } = new(3);

  public ReactivePropertySlim<string> CommandText { set; get; } = new("0xABCDEF:0x0001");

  public ReactivePropertySlim<string> SearchText { set; get; } = new("");

  public ReactivePropertySlim<string> ViewId { set; get; } = new();

  public ReactivePropertySlim<string> ViewData { set; get; } = new();



  // ------ constructors ------ //

  public MainWindowViewModel()
  {

    var running = new BehaviorSubject<bool>(false);
    IDisposable connector = null;

    StartCommand = running
        .Select(x => !x && CanUsbAdapter.FindAdapterNames().Length > 0)
        .ToReactiveCommand()
        .WithSubscribe(() =>
        {
          _adapter = new(null, Parser.GetBaudrate(SelectedBaudrateIndex.Value));
          _adapter.Open();
          running.OnNext(true);
          connector = StartLoop();
        });

    StopCommand = running
        .Select(x => x)
        .ToReactiveCommand()
        .WithSubscribe(() =>
        {
          connector?.Dispose();
          running.OnNext(false);
        });

    SetCommand.Subscribe(_ =>
    {
      var lines = CommandText.Value?.Split('\n').Where(x => x.Contains(':')).ToArray();
      if (lines is null || lines.Length is 0)
        return;
      var msgs = new List<CanMessage>();
      foreach (var line in lines)
      {
        var strs = line.Split(':').Select(x => x.Replace(" ", "")).ToArray();
        var msg = Parser.CreateMessage(strs[0], strs[1], ExtendedFlag.Value);
        msgs.Add(msg);
      }
      _sendMessages = msgs.ToArray();
    });

  }


  // ------ public methods ------ //

  public void Dispose()
  {
    _adapter?.Dispose();
  }


  // ------ private methods ------ //

  private IDisposable StartLoop()
  {
    var writer = RecordingFlag.Value ? new CanLogger() : null;
    var search = SearchText.Value ?? "";
    SearchCommand.Subscribe(_ => search = SearchText.Value ?? "");
    return Observable
        .Repeat(0, new EventLoopScheduler())
        .Finally(() =>
        {
          writer?.Dispose();
          _adapter?.Close();
        })
        .Do(_ =>
        {
          var msg = _adapter?.Read();
          if (msg is not null)
          {
            writer?.Write(msg);
            var txt = Parser.ParseMessage(msg);
            _recvMessages[txt[0]] = txt[1];
            _recvMessages = _recvMessages.Where(x => x.Key.Contains(search.ToUpper())).ToDictionary(x => x.Key, x => x.Value);
            ViewId.Value = string.Join("\n", _recvMessages.Keys.Select(x => x.ToString()).ToArray());
            ViewData.Value = string.Join("\n", _recvMessages.Values.Select(x => x.ToString()).ToArray());
          }

        })
        .Sample(Parser.GetFreqTimeSpan(SelectedFrequencyIndex.Value))
        .Subscribe(_ =>
        {
          if (_sendMessages is not null)
          {
            foreach (var msg in _sendMessages)
            {
              _adapter?.Write(msg);
              writer?.Write(msg);
            }
          }
        });
  }

}
