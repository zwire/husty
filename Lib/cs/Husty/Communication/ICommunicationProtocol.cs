using System.Text;

namespace Husty.Communication;

public interface ICommunicationProtocol : IDisposable
{

  public string NewLine { get; }
  public Encoding Encoding { get; }

  public Task<Result<IDataTransporter>> GetStreamAsync(
      TimeSpan timeout = default,
      CancellationToken ct = default
  );

}
