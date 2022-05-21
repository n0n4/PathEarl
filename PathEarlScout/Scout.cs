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
        public List<Rule<T>> GlobalRules;
        public List<Layer<T>> Layers;

        public ScoutRecycler<T> Recycler;
        public InfoAccess<T> InfoAccess;

        public Scout(Map<T> map, MapScratch<T> scratch, ScoutRecycler<T> recycler, InfoAccess<T> infoAccess)
        {
            Map = map;
            Scratch = scratch;

            Recycler = recycler;
            InfoAccess = infoAccess;
            InfoAccess.Map = map;

            GlobalRules = recycler.GetRuleList();
            Layers = recycler.GetLayerList();
        }

        public void Run()
        {
            for (int i = 0; i < Repeats + 1; i++)
                foreach (Layer<T> layer in Layers)
                    RunLayer(layer);
        }

        public void RunLayer(Layer<T> layer)
        {
            for (int i = 0; i < layer.Repeats + 1; i++)
                Optimizer.RunLayer(this, layer, GlobalRules);
        }
    }
}
