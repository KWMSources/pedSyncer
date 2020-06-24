using NavMesh_Graph;
using System;
using System.Collections.Generic;
using System.Timers;

namespace PedSyncer
{
    internal class PedMovementControl
    {
        private static PedMovementControl Instance = null;
        private Timer Timer;

        private PedMovementControl()
        {
            Timer Timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            Timer.AutoReset = true;
            Timer.Elapsed += new ElapsedEventHandler(MovePeds);
            Timer.Start();
        }

        public static PedMovementControl GetInstance()
        {
            if (Instance == null) Instance = new PedMovementControl();
            return Instance;
        }

        public void AddPedMovementCalculcation(Ped ped, bool SetCurrentNavmashPositionsIndex = true)
        {
            if (SetCurrentNavmashPositionsIndex) ped.CurrentNavmashPositionsIndex = GetNearestNavMeshOfPed(ped);

            if (ped.CurrentNavmashPositionsIndex < 0 || ped.NavmashPositions.Count >= ped.CurrentNavmashPositionsIndex)
            {
                ped.StartWandering();
                return;
            }

            ped.Position = ped.NavmashPositions[ped.CurrentNavmashPositionsIndex].Position.ToVector3();

            if (ped.NavmashPositions.Count < ped.CurrentNavmashPositionsIndex + 1)
            {
                ped.ContinueWandering();
                ped.CurrentNavmashPositionsIndex = 0;
            }

            AddPedMovement(
                (int)Math.Ceiling(Utils.GetDistanceBetweenPos(ped.Position, ped.NavmashPositions[ped.CurrentNavmashPositionsIndex + 1].Position.ToVector3())),
                ped
            );
        }

        private Dictionary<int, List<Ped>> PedMovements = new Dictionary<int, List<Ped>>();

        private void AddPedMovement(int Distance, Ped Ped)
        {
            if (!PedMovements.ContainsKey(Distance)) PedMovements.Add(Distance, new List<Ped>());
            PedMovements[Distance].Add(Ped);
        }

        private List<Ped> PopPedMovements()
        {
            //Store peds to return
            List<Ped> ToReturn = new List<Ped>();
            if (PedMovements.ContainsKey(0)) ToReturn = PedMovements[0];

            //Decrease all other keys than 0 by 1
            Dictionary<int, List<Ped>> NewPedMovements = new Dictionary<int, List<Ped>>();
            foreach (int PedMovementsKey in PedMovements.Keys)
            {
                if (PedMovementsKey == 0) continue;
                NewPedMovements[PedMovementsKey - 1] = PedMovements[PedMovementsKey];
            }

            PedMovements = NewPedMovements;

            return ToReturn;
        }

        private void MovePeds(object sender, ElapsedEventArgs e)
        {
            //Pop the peds at key 0 and decrease all other keys by 1
            List<Ped> PedsToMove = PopPedMovements();

            if (PedsToMove.Count == 0) return;

            foreach (Ped Ped in PedsToMove)
            {
                //If the ped now has a netOwner: Continue with the next ped, current position is now calculated by the netOwner
                if (Ped.NetOwner != null) continue;

                //Set ped to the next navmeshPosition
                Ped.CurrentNavmashPositionsIndex = Ped.CurrentNavmashPositionsIndex + 1;
                Ped.Position = Ped.NavmashPositions[Ped.CurrentNavmashPositionsIndex].Position.ToVector3();
                AddPedMovementCalculcation(Ped, false);
            }
        }

        public static int GetNearestNavMeshOfPed(Ped Ped)
        {
            if (Ped.NavmashPositions.Count == 0) return -1;

            int MinimumPos = -1;
            double MinimumDistance = 1000;

            int i = 0;
            foreach (NavigationMeshPolyFootpath NavMesh in Ped.NavmashPositions)
            {
                double Distance = Utils.GetDistanceBetweenPos(Ped.Position, NavMesh.Position.ToVector3());

                if (MinimumDistance > Distance)
                {
                    MinimumPos = i;
                    MinimumDistance = Distance;
                }
                i++;
            }

            return MinimumPos;
        }
    }
}