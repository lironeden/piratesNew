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
        public bool IsAttackersUnited { get; set; }
        public PirateLogics()
        {
            AssignedConquerors = new Dictionary<int, int>();
            MyAttackers = new Stack<int>();
            Conquerors = new Stack<Pirate>();
            KnowFirst = 0;
            MyPirateAlive = 5;
            EnemyPiratesAlive = 5;
            Init = true;
            IsAttackersUnited = false;
        }

        public void DoTurn(IPirateGame state)
        {
            int myPirateAlive = AllMyAlivePirates(state).Count;
            int enemyPiratesAlive = AllEnemyAlivePirates(state).Count;
            if(Init || !CheckStateHasChanged(state, myPirateAlive, enemyPiratesAlive))
            {
                state.Debug("called");
                SetStacks(state);
                Init = false;
            }
            state.Debug("Stack 1: " + MyAttackers.Count);
            state.Debug("Stack 2: " + Conquerors.Count);
            if (!IsAttackersUnited)
            {
                UinteAttackers(state);
                state.Debug("Distance " + state.Distance(state.AllMyPirates()[0], state.AllMyPirates()[1]));
            }
            else
            {
                Location temoLocationToEnemy = GetLoctionToDrown(state);
                MoveToAttack(temoLocationToEnemy, state);
            }
            SendConquerors(state);
        }
        public void AddAttackerWhenRevive(IPirateGame state)
        {
            List<Pirate> allPirates = AllMyAlivePirates(state);
            foreach(Pirate p in allPirates)
            {
                if (!MyAttackers.Contains(p.Id) && !Conquerors.Contains(p))
                {
                    MyAttackers.Push(p.Id);
                }
            }
        }
        public bool CheckStateHasChanged(IPirateGame state, int myAlive, int enemyAlive)
        {
            
            if(MyPirateAlive == myAlive && EnemyPiratesAlive == enemyAlive)
            {
                return true;
            }
            else if(MyPirateAlive != myAlive)
            {
                MyPirateAlive = myAlive;
                AddAttackerWhenRevive(state);
                return false;
            }
            else
            {
                EnemyPiratesAlive = enemyAlive;
                return false;
            }
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
            tempArr = AssignedConquerors.Keys.ToArray();
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
            pirateGame.Debug("Length" + AvailablePirates.Count);
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
                if (pirateGame.GetMyPirate(tempPirate.Id).IsLost)
                {
                    pirateGame.Debug("Lost from dict");
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
            if(MyAttackers.Count == 0)
            {
                return new Location(0, 0);
            }
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
            state.Debug("alive pir" + myPirates.Count);
            state.Debug("alive enem" + enemiesNumber);

            // if onky 2 pirates left
            if (myPirates.Count <= 2)
            {
                state.Debug("1");
                foreach (Pirate pirate in myPirates)
                {
                    Conquerors.Push(pirate);
                }
            }
            else if(Conquerors.Count == 0 && MyAttackers.Count != 0)
            {
                if(MyAttackers.Count > 2)
                {
                    Conquerors.Push(state.GetMyPirate(MyAttackers.Pop()));
                }
            }
            // need to change the stacks - too much attackers - need one more conquer
            else if (MyAttackers.Count + 1 > enemiesNumber)
            {
                // add to the conwuers one pirate from the attackers
                state.Debug("2");
                Conquerors.Push(state.GetMyPirate(MyAttackers.Pop()));
            }

            else if (MyAttackers.Count == 0 && Conquerors.Count == 0 && myPirates.Count != 0)
            {
                // can happened when all the ship are dead and come back slowly - add to conquers
                state.Debug("3");
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
            else if (MyAttackers.Count + 2 < enemiesNumber)
            {
                // add one more attacker only if there are more than one conquer
                if (Conquerors.Count > 1)
                {
                    MyAttackers.Push(Conquerors.Pop().Id);
                }
            }
            else
            {
                state.Debug("last");
                foreach (Pirate pirate in myPirates)
                {
                    if (!MyAttackers.Contains(pirate.Id) && !Conquerors.Contains(pirate))
                    {
                        MyAttackers.Push(pirate.Id);
                    }
                }
            }

        }
        public void UinteAttackers(IPirateGame state)
        {
            state.Debug("unite");
            Stack<int> tmp = new Stack<int>();
            Location firstAttackerLocation = state.GetMyPirate(MyAttackers.Peek()).Loc;

            tmp.Push(MyAttackers.Pop());        // enter the first attacker because already use

            bool isAllOk = true;

            // go over the the stack and check for each pirate its location
            while (MyAttackers.Count != 0)
            {
                if (state.GetMyPirate(MyAttackers.Peek()).Loc.Col == firstAttackerLocation.Col && state.GetMyPirate(MyAttackers.Peek()).Loc.Row - firstAttackerLocation.Row <= 3)
                {
                    state.SetSail(state.GetMyPirate(MyAttackers.Peek()), state.GetDirections(state.GetMyPirate(MyAttackers.Peek()), firstAttackerLocation)[0]);
                    isAllOk = false;            // it means that one of the pirates is in the wrong location
                }

                else if (state.GetMyPirate(MyAttackers.Peek()).Loc.Row == firstAttackerLocation.Row && state.GetMyPirate(MyAttackers.Peek()).Loc.Col - firstAttackerLocation.Col <= 3)
                {

                    // move the pirate one step to the main pirate location
                    state.SetSail(state.GetMyPirate(MyAttackers.Peek()), state.GetDirections(state.GetMyPirate(MyAttackers.Peek()), firstAttackerLocation)[0]);
                    isAllOk = false;            // it means that one of the pirates is in the wrong location
                }
                tmp.Push(MyAttackers.Pop());        // pop the pirate and get the next one
            }

            if (isAllOk)
            {
                IsAttackersUnited = true;           // set to true only if all the pirates are in the good location
            }

            // return all the attackers to the stack
            while (tmp.Count != 0)
            {
                MyAttackers.Push(tmp.Pop());
            }
        }

    }
}
