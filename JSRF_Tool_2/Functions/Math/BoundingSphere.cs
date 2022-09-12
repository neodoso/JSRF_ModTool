using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace JSRF_ModTool.Functions.Math
{
    public class BoundingSphere
    {
        /// <summary>
        /// Get the almost minimum sphere that contains all of a set of vertices by Ritter's algorithm.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>A tuple (center, radius) of the bounding sphere.</returns>
        public static (Vector3, float) GetMinimumBoundingSphere(List<Vector3> vertices)
        {
            // Initialize the 6 vertices having the extreme single value for each axis
            Vector3[] extremes = new Vector3[6] {
            new Vector3(Single.PositiveInfinity),    // low x
            new Vector3(Single.PositiveInfinity),    // low y
            new Vector3(Single.PositiveInfinity),    // low z
            new Vector3(Single.NegativeInfinity),    // high x
            new Vector3(Single.NegativeInfinity),    // high y
            new Vector3(Single.NegativeInfinity) };  // high z

            // Find exteme vertices
            foreach (var vertex in vertices)
            {
                if (vertex.X < extremes[0].X) extremes[0] = vertex;
                if (vertex.Y < extremes[1].Y) extremes[1] = vertex;
                if (vertex.Z < extremes[2].Z) extremes[2] = vertex;
                if (vertex.X > extremes[3].X) extremes[3] = vertex;
                if (vertex.Y > extremes[4].Y) extremes[4] = vertex;
                if (vertex.Z > extremes[5].Z) extremes[5] = vertex;
            }

            // Find the pair of extremes that are furthest apart from each other
            Tuple<int, int> furthestIndexes = new Tuple<int, int>(0, 0);
            float furthestDistance = 0;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (i == j) continue;
                    float distance = Vector3.Distance(extremes[i], extremes[j]);
                    if (distance > furthestDistance)
                    {
                        furthestIndexes = new Tuple<int, int>(i, j);
                        furthestDistance = distance;
                    }
                }
            }

            // Take that pair and make a sphere that has it as its diameter
            Vector3 center = Vector3.Lerp(extremes[furthestIndexes.Item1], extremes[furthestIndexes.Item2], 0.5f);
            float radius = furthestDistance / 2;

            // From the vertices that aren't enclosed by that sphere, find the furthest out one
            Vector3 furthestOutlier = new Vector3(0);
            float furthestOutlierDistance = 0;
            foreach (var vertex in vertices)
            {
                float distance = Vector3.Distance(center, vertex);
                if (distance > radius)
                    if (distance > furthestOutlierDistance)
                    {
                        furthestOutlierDistance = distance;
                        furthestOutlier = vertex;
                    }
            }

            // Take that outlier, and the opposite point of the original sphere, and make a new sphere of this new diameter
            // If no outliers, return the original sphere
            if (furthestOutlierDistance > 0)
            {
                center = Vector3.Lerp(furthestOutlier, center, (radius / furthestOutlierDistance + 1f) / 2f);
                radius = furthestOutlierDistance;
            }
            return (center, radius);
        }
    }
}
