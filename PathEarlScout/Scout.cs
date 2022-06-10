using PathEarlCore;
using PathEarlScout.Optimizers;
using System;
using System.Collections.Generic;

namespace PathEarlScout
{
    public class Scout<T> where T : ITileInfo
    {
        public string Name;

        public Map<T> Map;
        public MapScratch<T> Scratch;

        public IOptimizer<T> Optimizer;
        public int Repeats = 0;
        public Layer<T> GlobalLayer;
        public List<Layer<T>> Layers;

        public ScoutRecycler<T> Recycler;
        public InfoAccess<T> InfoAccess;

        public Dictionary<string, int> InputInts = new Dictionary<string, int>();
        public Dictionary<string, float> InputFloats = new Dictionary<string, float>();
        public Dictionary<string, string> InputStrings = new Dictionary<string, string>();

        public Dictionary<string, int> OutputInts = new Dictionary<string, int>();
        public Dictionary<string, float> OutputFloats = new Dictionary<string, float>();
        public Dictionary<string, string> OutputStrings = new Dictionary<string, string>();

        public Scout(Map<T> map, MapScratch<T> scratch, ScoutRecycler<T> recycler, InfoAccess<T> infoAccess)
        {
            Map = map;
            Scratch = scratch;

            Recycler = recycler;
            InfoAccess = infoAccess;
            InfoAccess.Map = map;
            infoAccess.LoadOutputs(OutputInts, OutputFloats, OutputStrings);

            GlobalLayer = recycler.GetLayer();
            GlobalLayer.Name = "GLOBAL";
            Layers = recycler.GetLayerList();
        }

        public void Run()
        {
            // copy inputs into outputs
            OutputInts.Clear();
            foreach (var kvp in InputInts)
                OutputInts.Add(kvp.Key, kvp.Value);

            OutputFloats.Clear();
            foreach (var kvp in InputFloats)
                OutputFloats.Add(kvp.Key, kvp.Value);

            OutputStrings.Clear();
            foreach (var kvp in InputStrings)
                OutputStrings.Add(kvp.Key, kvp.Value);

            // run each layer
            for (int i = 0; i < Repeats + 1; i++)
                foreach (Layer<T> layer in Layers)
                    RunLayer(layer);
        }

        public void RunLayer(Layer<T> layer)
        {
            for (int i = 0; i < layer.Repeats + 1; i++)
                Optimizer.RunLayer(this, layer, GlobalLayer);
        }
    }
}
