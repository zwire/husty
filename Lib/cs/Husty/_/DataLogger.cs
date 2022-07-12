using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Husty
{

    public enum LogFileFormat { Json, Csv }

    public class DataLogger<T> : IDisposable where T : class
    {

        // ------ fields ------ //

        private int _counter;
        private StreamWriter _sw;
        private readonly string _fileName;
        private readonly LogFileFormat _format;
        private readonly JsonSerializerOptions _option
            = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };


        // ------ constructors ------ //

        public DataLogger(LogFileFormat format, string directory = null)
        {
            _format = format;
            var ext = format is LogFileFormat.Json ? "json" : "csv";
            var time = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
            directory ??= "log";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            _fileName = $"{directory}\\{time}.{ext}";
            _sw = new(_fileName, false);
        }


        // ------ public methods ------ //

        public void Write(T value)
        {
            if (_sw?.BaseStream is null || value is null) return;
            var jstr = JsonSerializer.Serialize(value, _option);
            if (_format is LogFileFormat.Json)
            {
                _sw?.WriteLine(jstr);
            }
            else
            {
                if (_counter is 0)
                    _sw?.Write(Json2Csv.GetHeader(jstr));
                _sw?.Write(Json2Csv.GetRow(jstr));
            }
            if (++_counter is 1000)
            {
                _counter = 1;
                _sw?.Close();
                _sw = new(_fileName, true);
            }
        }

        public void Dispose()
        {
            _sw?.Close();
            _sw = null;
        }

    }
}

