using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Husty.Cmac
{
    
    public record CmacParam(double InitialValue, double Min, double Max, Func<double, double> GiveWeight);

    public record CmacLabelInfo(int GridCount, double Lower, double Upper);

    public class CmacNetwork
    {

        // ------ fields ------ //

        private readonly CmacTable[] _tables;
        private readonly Func<double, double> _giveWeight;


        // ------ constructors ------ //

        public CmacNetwork(
            int count,
            IEnumerable<CmacLabelInfo> labelInfos,
            CmacParam param
        )
        {
            _giveWeight = param.GiveWeight;
            _tables = new CmacTable[count];
            var infos = labelInfos.ToArray();
            for (int i = 0; i < count; i++)
            {
                var slidedInfos = new CmacLabelInfo[infos.Length];
                for (int k = 0; k < infos.Length; k++)
                {
                    var step = (infos[k].Upper - infos[k].Lower) / infos[k].GridCount;
                    var lower = infos[k].Lower + step * (i - count / 2) / count;
                    var upper = infos[k].Upper + step * (i - count / 2) / count;
                    slidedInfos[k] = new(infos[k].GridCount, lower, upper);
                }
                _tables[i] = new(slidedInfos, param.Min / count, param.Max / count, param.InitialValue / count);
            }
            Forward(new double[labelInfos.Count()]);
        }


        // ------ public methods ------ //

        public double Forward(double[] state)
        {
            if (state.Length != _tables[0].DimensionCount)
                throw new ArgumentException(nameof(state));
            var sum = 0.0;
            foreach (var t in _tables)
            {
                t.FixLocation(state);
                sum += t.ActiveValue;
            }
            return sum;
        }

        public void Backward(double[] state, double error, double grad)
        {
            var gain = _giveWeight(error);
            var penalty = gain * grad / _tables.Length;
            foreach (var t in _tables)
            {
                t.FixLocation(state);
                t.ApplyPenalty(penalty);
            }
        }

        public void Save(string name)
        {
            using var sw = new StreamWriter(name, false);
            foreach (var t in _tables)
                sw.WriteLine(JsonSerializer.Serialize(t.GetParams()));
        }

        public void Load(string name)
        {
            using var sr = new StreamReader(name);
            foreach (var t in _tables)
                t.SetParams(JsonSerializer.Deserialize<double[]>(sr.ReadLine()));
        }

    }
}
