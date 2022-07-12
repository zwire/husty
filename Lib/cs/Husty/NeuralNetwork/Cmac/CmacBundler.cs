using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Husty.NeuralNetwork.Cmac
{
    public sealed class CmacBundler : INeuralNetwork
    {

        // ------ fields ------ //

        private readonly CmacNetwork[] _nets;


        // ------ constructors ------ //

        public CmacBundler(IEnumerable<CmacNetwork> nets)
        {
            _nets = nets.Select(x => x.Clone()).ToArray();
        }


        // ------ public methods ------ //

        public float[] Forward(float[] state)
        {
            var output = new float[_nets.Length];
            for (int i = 0; i < _nets.Length; i++)
                output[i] = _nets[i].Forward(state);
            return output;
        }

        public void Backward(float[] error)
        {
            Backward(error, null);
        }

        public void Backward(float[] error, float[]? state)
        {
            for (int i = 0; i < _nets.Length; i++)
                _nets[i].Backward(error[i], state);
        }

        public void Save(string name)
        {
            using var sw = new StreamWriter(name);
            foreach (var n in _nets)
                sw.WriteLine(n.Serialize());
        }

        public static CmacBundler Load(string name)
        {
            var lines = File.ReadAllLines(name);
            var nets = new CmacNetwork[lines.Length];
            for (int i = 0;i < lines.Length;i++)
                nets[i] = CmacNetwork.Deserialize(lines[i]);
            return new(nets);
        }

    }
}
