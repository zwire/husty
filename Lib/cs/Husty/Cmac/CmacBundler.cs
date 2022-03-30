using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Husty.Cmac
{
    public sealed class CmacBundler
    {

        // ------ fields ------ //

        private readonly CmacNetwork[] _nets;


        // ------ constructors ------ //

        public CmacBundler(IEnumerable<CmacNetwork> nets)
        {
            _nets = nets.ToArray();
        }


        // ------ public methods ------ //

        public float[] Forward(float[] state)
        {
            var output = new float[_nets.Length];
            for (int i = 0; i < _nets.Length; i++)
                output[i] = _nets[i].Forward(state);
            return output;
        }

        public void Backward(float[] grads, float[]? state = null)
        {
            for (int i = 0; i < _nets.Length; i++)
                _nets[i].Backward(grads[i], state);
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
