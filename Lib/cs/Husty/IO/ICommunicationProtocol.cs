using System.Text;

namespace Husty.IO;

public interface ICommunicationProtocol : IDisposable
{

    public string NewLine { get; }
    public Encoding Encoding { get; }

    public ResultExpression<IDataTransporter> GetStream();

    public Task<ResultExpression<IDataTransporter>> GetStreamAsync(
        TimeSpan timeout = default,
        CancellationToken ct = default
    );

}
