using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Husty.IO
{
    public class UdpSender
    {

        // ------ fields ------ //

        private readonly UdpClient _sock;


        // ------ properties ------ //

        public List<IPEndPoint> EndPoints { get; }

        public Encoding Encoding { get; } = Encoding.UTF8;


        // ------ constructors ------ //

        public UdpSender(params int[] ports)
        {
            EndPoints = ports.Select(p => new IPEndPoint(IPAddress.Broadcast, p)).ToList();
            _sock = new UdpClient();
        }


        // ------ public methods ------ //

        public async Task Send<T>(string key, T data, CancellationToken ct = default)
        {
            var value = Encoding.GetBytes($"{key.Length:d2}" + key + JsonSerializer.Serialize(data));
            foreach (var ep in EndPoints)
                await _sock.SendAsync(value, ep, ct);
        }

        public void Close()
        {
            _sock.Close();
        }

    }
}
