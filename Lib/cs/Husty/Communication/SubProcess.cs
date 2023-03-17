using System.Diagnostics;

namespace Husty.Communication;

public class SubProcess : IDisposable
{

    private readonly Process _process;

    public BinaryReader BinaryReader { get; }
    public BinaryWriter BinaryWriter { get; }
    public StreamReader StreamReader { get; }
    public StreamWriter StreamWriter { get; }

    public SubProcess(string path, params object[] args)
    {
        _process = new()
        {
            StartInfo = new()
            {
                FileName = path,
                Arguments = string.Join(' ', args),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _process.Start();
        BinaryWriter = new(_process.StandardInput.BaseStream);
        BinaryReader = new(_process.StandardOutput.BaseStream);
        StreamWriter = _process.StandardInput;
        StreamReader = _process.StandardOutput;
    }

    public void Dispose()
    {
        _process.Dispose();
    }

}
