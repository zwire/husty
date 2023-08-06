using Husty.Lawicel;
using Xunit.Abstractions;

namespace Tests_.Husty.Lawicel;

public class CanUsbAdapter_Test
{

  private readonly ITestOutputHelper _output;

  public CanUsbAdapter_Test(ITestOutputHelper output)
  {
    _output = output;
  }

  [Fact]
  public void FindAdapterNames()
  {
    var names = CanUsbAdapter.FindAdapterNames();
    foreach (var name in names)
      _output.WriteLine(name);
  }

  [Fact]
  public void OpenClose()
  {
    using var can = new CanUsbAdapter(null, CanUsbOption.BAUD_250K);
    can.Open();
    var msg = new CanMessage(0, new byte[8]);
    for (int i = 0; i < 10; i++)
    {
      can.Write(msg);
      Thread.Sleep(10);
    }
  }

}