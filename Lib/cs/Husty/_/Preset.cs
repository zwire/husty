using System.IO;
using System.Text.Json;

namespace Husty
{
    public sealed class Preset<T>
    {

        // ------ fields ------ //

        private readonly T _defaultValue;


        // ------ constructors ------ //

        public Preset(T defaultValue)
        {
            _defaultValue = defaultValue;
        }


        // ------ public methods ------ //
        
        public T Load()
        {
            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText("preset.json"));
            }
            catch 
            {
                return _defaultValue;
            }
        }

        public void Save(T value)
        {
            using var sw = new StreamWriter("preset.json", false);
            sw.WriteLine(JsonSerializer.Serialize(value));
        }

    }
}
