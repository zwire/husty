using System.Reactive.Linq;
using Husty.Extensions;
using Husty.Gaming;

using var pad = new XPad();
Observable.Zip(
        pad.KeyPressed, pad.StickL, pad.StickR, pad.TriggerL, pad.TriggerR,
        (a, b, c, d, e) => new { Keys = a, StickL = b, StickR = c, TriggerL = d, TriggerR = e }
    )
    .Subscribe(p =>
    {
      ConsoleEx.WriteLines(1,
          $"Pressed -> ({string.Join(',', p.Keys)})",
          $"Stick L -> ({p.StickL.X:f2}, {p.StickL.Y:f2})",
          $"Stick R -> ({p.StickR.X:f2}, {p.StickR.Y:f2})",
          $"Trigger -> ({p.TriggerL:f2}, {p.TriggerR:f2})"
      );
      // your preference operation
    });

ConsoleEx.WaitKey(ConsoleKey.X);