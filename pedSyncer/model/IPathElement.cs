using AltV.Net;
using MessagePack;
using NavMesh_Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace pedSyncer.model
{
    /**
     * 
     * Base element of navMeshes and StreetCrossings
     * 
     * Providing functions for path calculation
     * 
     */

    [Union(0, typeof(NavigationMeshPolyFootpath))]
    [MessagePackObject]
    public abstract class IPathElement: IWritable
    {
        [Key(2)]
        public WorldVector3 Position { get; set; }

        public abstract List<IPathElement> GetNeighbours();

        //Select a random neighbour by a given navMesh
        public IPathElement GetRandomNeighbour()
        {
            List<IPathElement> neighbours = this.GetNeighbours();

            //Calculate the random key of the random neighbour
            int randomKey = new Random().Next(0, neighbours.Count - 1);

            return neighbours[randomKey];
        }

        //Select a random new neighbour by the given path
        public IPathElement GetRandomNewNeighbour(List<IPathElement> pathElements)
        {
            int trys = 0;
            IPathElement pathElement = this.GetRandomNeighbour();
            while (pathElements.Contains(pathElement) && trys < 20)
            {
                pathElement = this.GetRandomNeighbour();
                trys++;
            }
            return pathElement;
        }

        /*
         * Expand the given navMesh and give that neighbour navMesh back which has the minimum difference to the given gon
         *
         * Optional: Exclude the navMeshes which are already included in the path calculated till now
         */
        public IPathElement GetRandomNewNeighbourByDirectionAndPath(double preGon = 0, List<IPathElement> pathTillHere = null)
        {
            //If this neighbour hasn't any neighbours: Stop
            if (this.GetNeighbours().Count == 0) return null;

            if (preGon == null) preGon = 0;

            //Prepare the minimum difference search
            double minDifference = 500;
            IPathElement minNavMesh = null;

            //Iterate over all neighbours of the given startNavMesh
            foreach (IPathElement neighbourMesh in this.GetNeighbours())
            {
                if (neighbourMesh == null) continue;

                /*
                 * If the difference of the given gon and the gon calculated by the startNavMesh to the current viewed member
                 * is smaller than the current minimum and the neighbour is not already visited: store it.
                 */
                if (Math.Abs(preGon - WorldVector3.directionalAngle(this.Position, neighbourMesh.Position)) <
                    minDifference && ((pathTillHere != null && !pathTillHere.Contains(neighbourMesh)) || pathTillHere == null))
                {
                    minDifference =
                        Math.Abs(preGon - WorldVector3.directionalAngle(this.Position, neighbourMesh.Position));
                    minNavMesh = neighbourMesh;
                }
            }

            return minNavMesh;
        }

        //Calculate a random path starting with the given navMesh and a directional angle
        public List<IPathElement> GetWanderingPathByDirection(double gon)
        {
            //Prepare the path
            List<IPathElement> path = new List<IPathElement>();

            //Add start navMesh to the path
            path.Add(this);

            //Expand the starting navMesh, select the neighbour by the given directional angle
            IPathElement pathMesh = this.GetRandomNewNeighbourByDirectionAndPath(gon);

            //If this new station is noll (no neighbours could be selected): Stop
            if (pathMesh == null) return path;

            //Add this navMesh to the path
            path.Add(pathMesh);

            for (int i = path.Count; i < 200; i++)
            {
                //Expand the previous navMesh, select the neighbour by the given directional angle
                pathMesh = path[path.Count - 1].GetRandomNewNeighbourByDirectionAndPath(
                    WorldVector3.directionalAngle(path[path.Count - 2].Position, path[path.Count - 1].Position), path);

                //If this new station is noll (no neighbours could be selected): Stop
                if (pathMesh == null) return path;

                //Add this navMesh to the path
                path.Add(pathMesh);
            }

            return path;
        }

        //Calculate a random path starting with the given navMesh
        public List<IPathElement> GetWanderingPath()
        {
            //Prepare the path
            List<IPathElement> path = new List<IPathElement>();

            //Add start navMesh to the path
            path.Add(this);

            //If the given navMesh doesn't have a neighbour: Stop
            if (this.GetNeighbours().Count == 0) return path;

            //Select a random neighbour of this navMesh to start
            IPathElement randomNavMesh = this.GetRandomNeighbour();

            //If the random navMesh is null (no neighbours could be selected): Stop
            if (randomNavMesh == null) return path;

            //Add random navMesh to the path
            path.Add(randomNavMesh);

            //Repeat till the path includes 50 stations
            for (int i = path.Count; i < 200; i++)
            {
                //Expand the previous navMesh, select the neighbour by the given directional angle
                IPathElement pathMesh = path[path.Count - 1].GetRandomNewNeighbourByDirectionAndPath(
                    WorldVector3.directionalAngle(path[path.Count - 2].Position, path[path.Count - 1].Position), path);

                //If this new station is noll (no neighbours could be selected): Stop
                if (pathMesh == null) return path;

                //Add this navMesh to the path
                path.Add(pathMesh);
            }

            return path;
        }

        public void OnWrite(IMValueWriter writer)
        {
            writer.BeginObject();
            writer.Name("x");
            writer.Value(this.Position.X);
            writer.Name("y");
            writer.Value(this.Position.Y);
            writer.Name("z");
            writer.Value(this.Position.Z);
            writer.Name("streetCrossing");
            writer.Value(typeof(StreetCrossing).IsInstanceOfType(this));
            writer.EndObject();
        }
    }
}
