using System;
using System.Linq;
using System.Threading;

namespace Husty.Extensions
{
    public static class ConsoleEx
    {

        public static void Spin(Func<bool> func, TimeSpan time = default)
        {
            if (time == default)
                time = TimeSpan.FromMilliseconds(10);
            var spinner = new SpinWait();
            while(func())
                spinner.SpinOnce(time.Milliseconds);
        }

        public static void WaitKeyUntil(Func<ConsoleKey, bool> func, TimeSpan time = default)
        {
            Spin(() =>
            {
                if (Console.KeyAvailable)
                    return !func(Console.ReadKey().Key);
                return true;
            }, time);
        }

        public static ConsoleKey WaitKey(params ConsoleKey[] keys)
        {
            ConsoleKey pressed = default;
            Spin(() =>
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey().Key;
                    if (keys.Length is 0 || keys.Contains(key))
                    {
                        pressed = key;
                        return false;
                    }
                }
                return true;
            });
            return pressed;
        }

    }
}
