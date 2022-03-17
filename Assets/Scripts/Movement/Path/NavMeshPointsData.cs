using System.Collections.Generic;
using UnityEngine;

namespace Movement
{
    public class NavMeshPointsData : AbstractData
    {
        public List<Vector3> Points { get; private set; }
        public Rect Rect { get; private set; }

        public NavMeshPointsData(List<Vector3> points, Rect rect)
        {
            Points = points;
            Rect = rect;
        }

        public void Load()
        {
            // TODO
        }

        public void Save()
        {
            // TODO
        }
    }
}
