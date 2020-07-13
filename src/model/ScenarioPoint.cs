using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PedSyncer.Model
{
    class ScenarioPoint
    {
        public Vector4 Position;

        public string IType;

        public int TimeStart;

        public int TimeEnd;

        public string ModelType;

        public List<ScenarioPoint> NearScenarioPoint;
    }
}
