using System.IO;
using System.Text.Json;

namespace Husty
{
    public class UserSetting<T>
    {

        // ------ fields ------ //

        private readonly T _defaultValue;


        // ------ constructors ------ //

        public UserSetting(T defaultValue)
        {
            _defaultValue = defaultValue;
        }


        // ------ public methods ------ //
        
        public T Load()
        {
            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText("setting.json"));
            }
            catch 
            {
                return _defaultValue;
            }
        }

        public void Save(T value)
        {
            using var sw = new StreamWriter("setting.json", false);
            sw.WriteLine(JsonSerializer.Serialize(value));
        }

    }
}
