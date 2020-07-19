using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates
{
    public class PirateLogics : Pirates.IPirateBot
    {
        public Stack<Pirate> Conquerors { get; set; }
        public static Dictionary<int, Island> AssignedConquerors { get; set; } = new Dictionary<int, Island>();
        public void DoTurn(IPirateGame state)
        {
            SendConquerors(state);
        }

        public void SendConquerors(IPirateGame pirateGame)
        {
            Conquerors = new Stack<Pirate>(); // Check about how do I get the stack
            List<Pirate> AvailablePirates = pirateGame.AllMyPirates();
            int[] tempArr = AssignedConquerors.Keys.ToArray();
            List<Direction> tempDirection;
            pirateGame.Debug("available: " + AvailablePirates.Count);
            pirateGame.Debug("temp arrr" + tempArr.Length);
            Pirate tempPirate;
            for (int i = 0; i < tempArr.Length; i++)
            {
                tempPirate = pirateGame.GetMyPirate(tempArr[i]);
                tempDirection = pirateGame.GetDirections(tempPirate, AssignedConquerors[tempArr[i]]);
                pirateGame.SetSail(tempPirate, tempDirection[0]);
                if (AssignedConquerors[tempArr[i]].Owner == 0)
                {
                    AssignedConquerors.Remove(tempArr[i]);
                    continue;
                }
                
                AvailablePirates.Remove(tempPirate);
            }
            pirateGame.Debug("available: " + AvailablePirates.Count);
            foreach (Pirate p in AvailablePirates)
            {
                
                if (!p.IsLost)
                {
                    Conquerors.Push(p);
                }
            }

            Stack<Pirate> tempStack = new Stack<Pirate>(); // Check about how do I get the stack
            List<Island> notOurIslands = pirateGame.NotMyIslands();
            if(notOurIslands.Count == 0 || AvailablePirates.Count == 0)
            {
                return;
            }
            int minDistance = pirateGame.Distance(Conquerors.Peek(), notOurIslands[0]);
            int tempDistance;
            List<Island> targetedIslands = new List<Island>();
            Island ClosestIsland = notOurIslands[0];
            while(!(Conquerors.Count == 0))
            {
                tempPirate = Conquerors.Pop();
                tempStack.Push(tempPirate);
                foreach(Island island in notOurIslands)
                {
                    tempDistance = pirateGame.Distance(tempPirate, island);
                    if(tempDistance < minDistance)
                    {
                        minDistance = tempDistance;
                        ClosestIsland = island;
                    }
                }
                tempDirection = pirateGame.GetDirections(tempPirate, ClosestIsland); // Check List Results
                pirateGame.SetSail(tempPirate, tempDirection[0]);
                AssignedConquerors.Add(tempPirate.Id, ClosestIsland);
                targetedIslands.Add(ClosestIsland);
                notOurIslands.Remove(ClosestIsland);
                if(notOurIslands.Count == 0)
                {
                    break;
                }
                ClosestIsland = notOurIslands[0];
                minDistance = pirateGame.Distance(tempPirate, notOurIslands[0]);
            }
            while(!(tempStack.Count == 0))
            {
                Conquerors.Push(tempStack.Pop());
            }
        }
    }
}
