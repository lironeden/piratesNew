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
        public PirateLogics()
        {
            Conquerors = new Stack<Pirate>();
        }

        public void DoTurn(IPirateGame state)
        {
            SendConquerors(state);
        }

        public void SendConquerors(IPirateGame pirateGame)
        {
            Stack<Pirate> tempStack = new Stack<Pirate>();
            Pirate tempPirate;
            List<Island> notOurIslands = pirateGame.NotMyIslands();
            if(notOurIslands.Count == 0)
            {
                return;
            }
            int minDistance = pirateGame.Distance(Conquerors.Peek(), notOurIslands[0]);
            int tempDistance;
            List<Island> targetedIslands = new List<Island>();
            Island ClosestIsland = notOurIslands[0];
            List<Direction> tempDirection;
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
                pirateGame.Debug("list " + tempDirection.ToString());  // Debug
                pirateGame.SetSail(tempPirate, tempDirection[0]);
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
