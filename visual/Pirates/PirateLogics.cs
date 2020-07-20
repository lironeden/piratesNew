using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates
{
    public class PirateLogics : Pirates.IPirateBot
    {
        public Stack<int> MyAttackers { get; set; }
        public Stack<Pirate> Conquerors { get; set; }
        public Dictionary<int, int> AssignedConquerors { get; set; }
        public int KnowFirst { get; set; }
        public int MyPirateAlive { get; set; }
        public int EnemyPiratesAlive { get; set; }
        public bool Init { get; set; }
        public PirateLogics()
        {
            AssignedConquerors = new Dictionary<int, int>();
            MyAttackers = new Stack<int>();
            Conquerors = new Stack<Pirate>();
            KnowFirst = 0;
            MyPirateAlive = 5;
            EnemyPiratesAlive = 5;
            Init = true;
        }

        public void DoTurn(IPirateGame state)
        {
            int myPirateAlive = AllMyAlivePirates(state).Count;
            int enemyPiratesAlive = AllEnemyAlivePirates(state).Count;
            if(Init || CheckStateHasChanged(state, myPirateAlive, enemyPiratesAlive))
            {
                SetStacks(state);
                Init = false;
            }
            state.Debug("Stack 1: " + MyAttackers.Count);
            state.Debug("Stack 2: " + Conquerors.Count);
            Location temoLocationToEnemy = GetLoctionToDrown(state);
            MoveToAttack(temoLocationToEnemy, state);
            SendConquerors(state);
        }

        public bool CheckStateHasChanged(IPirateGame state, int myAlive, int enemyAlive)
        {
            return MyPirateAlive == myAlive && EnemyPiratesAlive == enemyAlive;
        }

        public void SendConquerors(IPirateGame pirateGame)
        {
            Stack<Pirate> tempStack = new Stack<Pirate>();
            List<Pirate> AvailablePirates = Conquerors.ToList();
            int[] tempArr = AssignedConquerors.Keys.ToArray();
            List<Island> notOurIslands = pirateGame.NotMyIslands();
            List<Direction> tempDirection;
            Pirate tempPirate;
            Island tempIsland;
            for(int i = 0; i < tempArr.Length; i++)
            {
                if (!AvailablePirates.Contains(pirateGame.GetMyPirate(tempArr[i])))
                {
                    AssignedConquerors.Remove(tempArr[i]);
                }
            }

            for (int i = 0; i < tempArr.Length; i++)  // This loop runs over the Dictionary
                                                      //to direct the players to the Island
            {
                tempIsland = pirateGame.GetIsland(AssignedConquerors[tempArr[i]]);
                tempPirate = pirateGame.GetMyPirate(tempArr[i]);
                if (tempIsland.Owner == 0 || tempPirate.IsLost)
                {
                    pirateGame.Debug("Deleting" + tempArr[i]);
                    AssignedConquerors.Remove(tempArr[i]);
                    continue;
                }

                tempDirection = pirateGame.GetDirections(tempPirate, tempIsland);
                pirateGame.SetSail(tempPirate, tempDirection[0]);
                pirateGame.Debug("Removing " + tempArr[i]);
                AvailablePirates.Remove(tempPirate);
            }

            if (notOurIslands.Count == 0 || AvailablePirates.Count == 0 || Conquerors.Count == 0)
            {
                return;
            }

            int minDistance = pirateGame.Distance(Conquerors.Peek(), notOurIslands[0]);
            int tempDistance;
            List<Island> targetedIslands = new List<Island>();
            Island ClosestIsland = notOurIslands[0];
            while (!(Conquerors.Count == 0))  // This loop passes over all the players that are not assigned and
                                              // Determines which island they will go to
            {
                tempPirate = Conquerors.Pop();
                if (tempPirate.IsLost)
                {
                    continue;
                }
                tempStack.Push(tempPirate);
                if (AssignedConquerors.ContainsKey(tempPirate.Id))
                {
                    continue;
                }
                foreach (Island island in notOurIslands) // Check which Islands are not our
                {
                    tempDistance = pirateGame.Distance(tempPirate, island);
                    if (tempDistance < minDistance)
                    {
                        minDistance = tempDistance;
                        ClosestIsland = island;
                    }
                }

                tempDirection = pirateGame.GetDirections(tempPirate, ClosestIsland); // Check List Results
                pirateGame.SetSail(tempPirate, tempDirection[0]);
                pirateGame.Debug("" + tempPirate.Id);
                AssignedConquerors.Add(tempPirate.Id, ClosestIsland.Id);
                targetedIslands.Add(ClosestIsland);
                notOurIslands.Remove(ClosestIsland);
                if (notOurIslands.Count == 0)
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

        public void MoveToAttack(Location locationTogo, IPirateGame state)
        {
            Stack<int> tempAttackers = new Stack<int>();
            int tempId;
            while (!(MyAttackers.Count == 0))
            {
                tempId = MyAttackers.Pop();
                if (state.GetMyPirate(tempId).IsLost)
                {
                    continue;
                }
                tempAttackers.Push(tempId);
                state.SetSail(state.GetMyPirate(tempId), state.GetDirections(state.GetMyPirate(tempId), locationTogo)[0]);
            }
            while (!(tempAttackers.Count == 0))
            {
                MyAttackers.Push(tempAttackers.Pop());
            }
        }

        public Location GetLoctionToDrown(IPirateGame state)
        {
            Location attackersLocation = state.GetMyPirate(MyAttackers.Peek()).Loc;        // all the attackers are going together

            int distance = int.MaxValue;
            Location locationToGo = new Location(0, 0);

            foreach (Pirate enemy in state.AllEnemyPirates())
            {
                int tmpDistance = state.Distance(enemy.Loc, attackersLocation);
                if (tmpDistance < distance)
                {
                    distance = tmpDistance;
                    locationToGo = enemy.Loc;
                }
            }

            return locationToGo;
        }
        public List<Pirate> AllMyAlivePirates(IPirateGame state)
        {
            List<Pirate> temp = state.AllMyPirates();
            List<Pirate> alive = new List<Pirate>();
            foreach(Pirate p in temp)
            {
                if (!p.IsLost)
                {
                    alive.Add(p);
                }
            }
            return alive;
        }
        public List<Pirate> AllEnemyAlivePirates(IPirateGame state)
        {
            List<Pirate> temp = state.AllEnemyPirates();
            List<Pirate> alive = new List<Pirate>();
            foreach (Pirate p in temp)
            {
                if (!p.IsLost)
                {
                    alive.Add(p);
                }
            }
            return alive;
        }
        public void SetStacks(IPirateGame state)
        {

            int enemiesNumber = AllEnemyAlivePirates(state).Count;
            List<Pirate> myPirates = AllMyAlivePirates(state);


            // if onky 2 pirates left
            if (myPirates.Count <= 2)
            {
                MyAttackers = new Stack<int>();
                Conquerors = new Stack<Pirate>();
                foreach (Pirate pirate in myPirates)
                {
                    Conquerors.Push(pirate);
                }
            }

            // need to change the stacks - too much attackers - need one more conquer
            else if (MyAttackers.Count + 1 > enemiesNumber)
            {
                // add to the conwuers one pirate from the attackers
                Conquerors.Push(state.GetMyPirate(MyAttackers.Pop()));
            }

            else if (MyAttackers.Count == 0 && Conquerors.Count == 0 && myPirates.Count != 0)
            {
                // can happened when all the ship are dead and come back slowly - add to conquers
                if (myPirates.Count <= 2)
                {
                    foreach (Pirate pirate in myPirates)
                    {
                        Conquerors.Push(pirate);
                    }
                }

                // init the stacks, or in case there are more than 3 pirates that come back to life
                else
                {
                    foreach (Pirate pirate in myPirates)
                    {
                        MyAttackers.Push(pirate.Id);
                    }

                    Conquerors.Push(state.GetMyPirate(MyAttackers.Pop()));
                    Conquerors.Push(state.GetMyPirate(MyAttackers.Pop()));
                }
            }

            // need to change the stack - too many conquers
            else if (MyAttackers.Count + 1 < enemiesNumber)
            {
                // add one more attacker only if there are more than one conquer
                if (Conquerors.Count > 1)
                {
                    MyAttackers.Push(Conquerors.Pop().Id);
                }
            }
        }

    }
}
