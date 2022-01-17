using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using Cysharp.Diagnostics;

namespace Husty
{
    public class StdOutReader : IDisposable
    {

        // ------ fields ------ //

        private readonly Subject<string> _notifier = new();


        // ------ constructors ------ //

        public StdOutReader(string cmd)
        {
            Start(cmd);
        }

        public StdOutReader(string exe, string file, string[] args = null)
        {
            var arguments = file;
            if (args is not null)
                foreach (var a in args)
                    arguments += $" {a}";
            Start($@"{exe} -u {arguments}");
        }


        // ------ public methods ------ //

        public IObservable<string> GetStream()
        {
            return _notifier;
        }

        public void Dispose()
        {
            _notifier.Dispose();
        }


        // ------ private methods ------ //

        private void Start(string cmd)
        {
            Task.Run(async () =>
            {
                try
                {
                    await foreach (var item in ProcessX.StartAsync(cmd))
                    {
                        _notifier.OnNext(item);
                    }
                }
                catch (ProcessErrorException e)
                {
                    Debug.WriteLine(e.ToString());
                }
            });
        }

    }
}
