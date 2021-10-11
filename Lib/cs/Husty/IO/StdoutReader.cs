using System;
using System.ComponentModel;
using System.Diagnostics;
using Cysharp.Diagnostics;

namespace Husty.IO
{
    public class StdOutReader<T>
    {

        // ------ Fields ------ //

        private readonly string _cmd;


        // ------ Properties ------ //

        public event EventHandler<T> ConsoleValueChanged;


        // ------ Constructors ------ //

        public StdOutReader(string pythonExe, string pythonFile, string[] args = null)
        {
            var arguments = pythonFile;
            if (args is not null)
                foreach (var a in args)
                    arguments += $" {a}";
            _cmd = $@"{pythonExe} -u {arguments}";
        }


        // ------ Methods ------ //

        public async void Start()
        {
            try
            {
                await foreach (var item in ProcessX.StartAsync(_cmd))
                {
                    ConsoleValueChanged?.Invoke(null, (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(item));
                }
            }
            catch (ProcessErrorException e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

    }
}
