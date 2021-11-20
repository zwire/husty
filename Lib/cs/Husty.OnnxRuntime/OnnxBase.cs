using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Husty.OnnxRuntime
{

    public enum Provider { CPU, OpenVINO, CUDA, DML, Dnnl, Nnapi, MIGraphX, Nuphar, ROCM, TensorRT }

    public enum OptimizationLevel { Off, Basic, Extended, Full }

    public class LayerInfo
    {

        public string Name { get; set; }

        public Type ElementType { get; set; }

        public int[] Shape { get; set; }

    }

    public abstract class OnnxBase<TInput, TOutput> : IDisposable
    {

        // ------ fields ------ //

        private readonly InferenceSession _session;


        // ------ properties ------ //

        public LayerInfo[] InputLayers { get; protected set; }

        public LayerInfo[] OutputLayers { get; protected set; }


        // ------ constructors ------ //

        public OnnxBase(string model, Provider provider, OptimizationLevel optimization)
        {
            var options = new SessionOptions();
            switch (provider)
            {
                case Provider.CPU:      options.AppendExecutionProvider_CPU();      break;
                case Provider.OpenVINO: options.AppendExecutionProvider_OpenVINO(); break;
                case Provider.CUDA:     options.AppendExecutionProvider_CUDA();     break;
                case Provider.DML:      options.AppendExecutionProvider_DML();      break;
                case Provider.Dnnl:     options.AppendExecutionProvider_Dnnl();     break;
                case Provider.MIGraphX: options.AppendExecutionProvider_MIGraphX(); break;
                case Provider.Nnapi:    options.AppendExecutionProvider_Nnapi();    break;
                case Provider.Nuphar:   options.AppendExecutionProvider_Nuphar();   break;
                case Provider.ROCM:     options.AppendExecutionProvider_ROCM();     break;
                case Provider.TensorRT: options.AppendExecutionProvider_Tensorrt(); break;
            }
            options.GraphOptimizationLevel = optimization switch
            {
                OptimizationLevel.Off       => GraphOptimizationLevel.ORT_DISABLE_ALL,
                OptimizationLevel.Basic     => GraphOptimizationLevel.ORT_ENABLE_BASIC,
                OptimizationLevel.Extended  => GraphOptimizationLevel.ORT_ENABLE_EXTENDED,
                OptimizationLevel.Full      => GraphOptimizationLevel.ORT_ENABLE_ALL,
                _                           => GraphOptimizationLevel.ORT_DISABLE_ALL
            };
            _session = new InferenceSession(model, options);
            InputLayers = _session.InputMetadata.Select(m => new LayerInfo() { Name = m.Key, ElementType = m.Value.ElementType, Shape = m.Value.Dimensions }).ToArray();
            OutputLayers = _session.OutputMetadata.Select(m => new LayerInfo() { Name = m.Key, ElementType = m.Value.ElementType, Shape = m.Value.Dimensions }).ToArray();
        }


        // ------ public methods ------ //

        protected IDictionary<string, float[]> Run_(float[] inputData)
        {
            var container = InputLayers.Select(i =>
            {
                var tensor = new DenseTensor<float>(inputData, i.Shape);
                return NamedOnnxValue.CreateFromTensor(i.Name, tensor);
            }).ToArray();
            return _session.Run(container).ToDictionary(r => r.Name, r => r.AsTensor<float>().ToArray());
        }

        public void Dispose() => _session.Dispose();

        public abstract TOutput Run(TInput input);

    }

}
