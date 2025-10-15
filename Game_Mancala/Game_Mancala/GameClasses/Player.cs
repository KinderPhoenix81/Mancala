using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace Game_Mancala
{
    //player class will hold variables relevant to the individual player
    public class Player
    {
        //player constructor
        public Player(string? playerName, bool CPU, string? playerDifficulty, Canvas gmeCanvas)
        {
            Name = playerName;
            IsCPU = CPU;
            Difficulty = playerDifficulty;
            GameCanvas = gmeCanvas;
        }

        //Overloaded player constructor for 
        public Player(string? playerName, bool CPU, string? playerDifficulty)
        {
            Name = playerName;
            IsCPU = CPU;
            Difficulty = playerDifficulty;
        }

        //Denotes if user is Player1 or Player2
        public string Name;
        public string Difficulty;

        //wether or not the player is a person or a computer
        public bool IsCPU;

        public Canvas GameCanvas;

        //main gamplay loop function.
        //Plays the turn, moves stones into pits
        //accepts which player is active and which index they clicked
        //to intiate the main gamplay loop
        public async Task<(int, int)> PlayTurn(int colIndex, int CurrentPlayer, Button[,] arryButtons,
            Pit[,] PitLocations, Dictionary<string, int> GameSettings, int GoldenStoneLandingPit)
        {
            //initiate variables we will need to iterate over
            //looping and any subsequent decisions
            int rowIndex;
            int pickedStones;
            //may need another to store initial player, in case the loop
            //goes beyond one switch from player1 to player2 or vice versa

            //new list to hold stones pulled out of selected pit
            //this will not exist beyond the player action
            List<Stone> PickedStones = new List<Stone>();

            //setting index for PitLocation array iteration below
            if (CurrentPlayer == 0)
            {
                rowIndex = 1;
            }
            else
            {
                rowIndex = 0;
            }

            //check to see if selected pit has the golden stone, if so, ask the player where they want to place it
            if (GameSettings["GoldenMode"] == 1)
            {
                Tuple<bool, int> GoldenStonePit = CheckForGoldenStone(rowIndex, colIndex, PitLocations);
            }

            PickedStones = PickUpStones(rowIndex, colIndex, PitLocations, arryButtons);

            //begin stone placing iteration. will loop for a number of times equal to the number of stones picked
            //up from the clicked PitLocation button.For the number of loops, will add a new stone and set the button
            //value to the new PitLocations.Stones.Count value. Handles adding stones to  pits and removing them
            //from the temporary PickedStones array that was generated above.
            int j;

            if (GameSettings["CycloneCurrentlyActive"] == 0)
            {
                j = colIndex + 1;
            }
            else
            {
                j = colIndex - 1;
            }

            //made the return of MoveStones on a separate line from the return of PlayTurn for readability
            (rowIndex, colIndex) = await MoveStones(j, CurrentPlayer, rowIndex, PickedStones, arryButtons, PitLocations,
                GameSettings, GoldenStoneLandingPit);
            return (rowIndex, colIndex);

        }

        //Method to pick up the stones from a pit, and store them into a temporary list
        public List<Stone> PickUpStones(int rowIndex, int colIndex, Pit[,] PitLocations, Button[,] arryButtons)
        {
            int pickedStones;
            List<Stone> PickedStones = new List<Stone>();

            //count the number of stones we will be moving. will be used for iterations below
            pickedStones = PitLocations[rowIndex, colIndex].Stones.Count();

            //for each stone, add it to the next PitLocation, then remove it
            //from the main PitLocations array index.. this should be a self contained
            //loop to maintain the integrity of the PitLocations array stones
            for (int i = 0; i < pickedStones; i++)
            {
                PickedStones.Add(PitLocations[rowIndex, colIndex].Stones[0]);
                PitLocations[rowIndex, colIndex].Stones.RemoveAt(0);
            }

            //set the button value/content to the count of the array list count. necessary to show 
            //the player how many stones are now in the clicked pit. should always be 0
            ClearVisualStones(GameCanvas, arryButtons[rowIndex, colIndex]);

            return PickedStones;
        }

        //Overloaded method of PickUpStones that doesn't modify a button value, used for simulations with CPU
        public List<Stone> PickUpStones(int rowIndex, int potentialPitIndex, Pit[,] PitLocations)
        {
            int pickedStones;
            List<Stone> PickedStones = new List<Stone>();

            //count the number of stones we will be moving. will be used for iterations below
            pickedStones = PitLocations[rowIndex, potentialPitIndex].Stones.Count();

            //for each stone, add it to the next PitLocation, then remove it
            //from the main PitLocations array index.. this should be a self contained
            //loop to maintain the integrity of the PitLocations array stones
            for (int i = 0; i < pickedStones; i++)
            {
                PickedStones.Add(PitLocations[rowIndex, potentialPitIndex].Stones[0]);
                PitLocations[rowIndex, potentialPitIndex].Stones.RemoveAt(0);
            }

            return PickedStones;
        }

        //Method to move stones on the main board
        public async Task<(int rowIndex, int j)> MoveStones(int j, int CurrentPlayer, int rowIndex,
            List<Stone> PickedStones, Button[,] arryButtons, Pit[,] PitLocations, Dictionary<string, int> GameSettings,
            int GoldenStoneCounter)
        {
            //A flag for whether to skip a store or not
            bool skipStoreFlag;

            //Movement Loop
            while (PickedStones.Count > 0)
            {
                //Initializing the flag after each stone placement
                skipStoreFlag = false;

                //Check to see if j could be out of bounds, and correct the value
                if (j == -1 && GameSettings["CycloneCurrentlyActive"] == 1)
                {
                    if (rowIndex == 0)
                    {
                        rowIndex = 1;
                    }
                    else
                    {
                        rowIndex = 0;
                    }

                    //Set j and bool variable to skip the opponent store
                    j = PitLocations.GetLength(1) - 1;
                    skipStoreFlag = true;
                }

                //Logic for moving stones on the top row
                if (rowIndex == 0 && PickedStones.Count != 0)
                {
                    //if the player turn is 1, do not place a stone in the store [0,6] in arryButtons if that is current location
                    if ((CurrentPlayer == 0 && rowIndex == 0 && j == PitLocations.GetLength(1) - 1) || skipStoreFlag)
                    {
                        //Skip over the opposing store, do not drop a stone\
                    }
                    else //Places stones in pits regardless of player turn, as long as it isn't the opposing store
                    {
                        //if if goldenstonecounter > 50 place any stone, else if goldenstonecounter = 0 then place golden stone, otherwise do NOT place golden stone
                        if (GoldenStoneCounter > 50)
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                        }
                        else if (GoldenStoneCounter == 0)
                        {
                            for (int i = 0; i < PickedStones.Count; i++)
                            {
                                if (PickedStones[i].Color == "Golden")
                                {
                                    PitLocations[rowIndex, j].Stones.Add(PickedStones[i]);
                                    PickedStones.RemoveAt(i);
                                }
                            }
                        }
                        else if (GoldenStoneCounter > 0)
                        {
                            if (PickedStones[0].Color == "Golden" )
                            {
                                PitLocations[rowIndex, j].Stones.Add(PickedStones[1]);
                                PickedStones.RemoveAt(1);
                            }
                            else
                            {
                                PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                                PickedStones.RemoveAt(0);
                            }

                        }
                        else
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                        }

                        GoldenStoneCounter--;

                        ClearVisualStones(GameCanvas, arryButtons[rowIndex, j]);
                        foreach (Stone currentStone in PitLocations[rowIndex, j].Stones)
                        {
                            CreateVisualStone(currentStone.Color, GameCanvas, arryButtons[rowIndex, j]);
                        }

                        //Places modified button to focus, displays animation
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].Focus();
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].Background = Brushes.BlanchedAlmond;
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].ClearValue(Button.BackgroundProperty);
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].IsEnabled = false;
                        await Task.Delay(100);
                    }

                    //If Cyclone Mode is disabled, complete regular movement
                    ////If end of the row(store) in PitLocations array has been reached for the top row,
                    ////move down to the bottom row to continue movement
                    if (GameSettings["CycloneCurrentlyActive"] == 0)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Increment movement variable for navigating the internal array
                            j++;

                            if (j == PitLocations.GetLength(1))
                            {
                                rowIndex = 1;
                                j = 0;
                            }
                        }
                    }

                    //If Cyclone Mode is enabled, go backwards
                    ////If the end (beginning because its backwards) of PitLocations has been reached for the top row
                    if (GameSettings["CycloneCurrentlyActive"] == 1)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Decrement the movement variable for navigating the internal array
                            j--;

                            if (j == -1)
                            {
                                rowIndex = 1;
                                j = PitLocations.GetLength(1) - 1;
                            }
                        }

                        //For debugging
                        //MessageBox.Show(j.ToString());
                    }
                }

                //Logic for moving stones on bottom row
                if (rowIndex == 1 && PickedStones.Count != 0)
                {
                    //if the player turn is 2, do not place a stone in the store in arryButtons if that is current location
                    if ((CurrentPlayer == 1 && rowIndex == 1 && j == PitLocations.GetLength(1) - 1) || skipStoreFlag)
                    {
                        //Skip over the opponent store, do not drop a store
                    }
                    else //Place stones in pits regardless of player turn, as long as it isn't the opposing store
                    {
                        //if if goldenstonecounter > 50 place any stone, else if goldenstonecounter = 0 then place golden stone, otherwise do NOT place golden stone
                        if (GoldenStoneCounter > 50)
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                        }
                        else if (GoldenStoneCounter == 0)
                        {
                            for (int i = 0; i < PickedStones.Count; i++)
                            {
                                if (PickedStones[i].Color == "Golden")
                                {
                                    PitLocations[rowIndex, j].Stones.Add(PickedStones[i]);
                                    PickedStones.RemoveAt(i);
                                }
                            }
                        }
                        else if (GoldenStoneCounter > 0 /* && PickedStones.Count > 1 */)
                        {
                            if (PickedStones[0].Color == "Golden")
                            {
                                PitLocations[rowIndex, j].Stones.Add(PickedStones[1]);
                                PickedStones.RemoveAt(1);
                            }
                            else
                            {
                                PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                                PickedStones.RemoveAt(0);
                            }

                        }
                        else
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                        }

                        GoldenStoneCounter--;

                        ClearVisualStones(GameCanvas, arryButtons[rowIndex, j]);
                        foreach (Stone currentStone in PitLocations[rowIndex, j].Stones)
                        {
                            CreateVisualStone(currentStone.Color, GameCanvas, arryButtons[rowIndex, j]);
                        }
                        
                        arryButtons[rowIndex, j].IsEnabled = true;
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].Focus();
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].Background = Brushes.BlanchedAlmond;
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].ClearValue(Button.BackgroundProperty);
                        await Task.Delay(100);
                        arryButtons[rowIndex, j].IsEnabled = false;
                        await Task.Delay(100);
                    }

                    //If Cyclone Mode is disabled, continue regular movement
                    ////If end of the row (store) in PitLocations array has been reached for the bottom row,
                    ////move up to the top row to continue moving if possible
                    if (GameSettings["CycloneCurrentlyActive"] == 0)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Increment movement variable for navigating the internal array
                            j++;

                            if (j == PitLocations.GetLength(1))
                            {
                                rowIndex = 0;
                                j = 0;
                            }
                        }
                    }

                    //If Cyclone Mode is enabled, continue backwards movement
                    ////If the end (beginning because of cyclone) of PitLocations has been reached on the top bottom row, go to the top row
                    if (GameSettings["CycloneCurrentlyActive"] == 1)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Decrement the movement variable for navigating the internal array
                            j--;

                            if (j == -1)
                            {
                                rowIndex = 0;
                                j = PitLocations.GetLength(1) - 1;
                            }
                        }

                        //For debugging
                        //MessageBox.Show(j.ToString());
                    }
                }

            }

            //Returns the values of the rowIndex and J to be used elsewhere
            return (rowIndex, j);
        }

        //Overloaded MoveStones used for the Advanced Difficulty decision making
        //Used for movement on the simulate board
        public (int rowIndex, int j, int score) MoveStones(int j, int CurrentPlayer, int rowIndex,
            List<Stone> PickedStones, Pit[,] PitLocations, Dictionary<string, int> GameSettings, Pit[,] BasePitLocations)
        {
            //Score intialization
            int score = 0;

            //A flag for whether to skip a store or not
            bool skipStoreFlag;

            //A flag for if the golden stone is in the selected pit
            bool goldenStoneInPit = false;

            //An integer for storing the location of where the golden stone should go
            //Set out of bounds to prevent potential issues
            int GoldenStoneCounter = 99;

            //Dynamic setting of a variable to be passed into GetGoldenStoneCounter
            //Dependent on Cyclone Mode
            int CorrectedPitIndex = 0;

            //Declaring a dynamic variable to be set based on cyclone mode conditions
            int CycloneActive = 0;

            //Setting of dynamic variable
            if (GameSettings["CycloneCurrentlyActive"] == 0)
            {
                CorrectedPitIndex = j - 1;
                CycloneActive = 0;
            }
            else
            {
                CorrectedPitIndex = j + 1;
                CycloneActive = 1;
            }

            GoldenStoneCounter = GetGoldenStoneCounter(GameSettings["GoldenMode"], rowIndex, CorrectedPitIndex, BasePitLocations,
                CycloneActive);

            //Prevents the golden stone from being moved if it would give it to the opponent
            if(GoldenStoneCounter == -1)
            {
                score -= 1000;
            }

            //Movement Loop
            while (PickedStones.Count > 0)
            {
                //Initializing the flag after each stone placement
                skipStoreFlag = false;

                //Check to see if j could be out of bounds, and correct the value
                if (j == -1 && GameSettings["CycloneCurrentlyActive"] == 1)
                {
                    if (rowIndex == 0)
                    {
                        rowIndex = 1;
                    }
                    else
                    {
                        rowIndex = 0;
                    }

                    //Set j and bool variable to skip the opponent store
                    j = PitLocations.GetLength(1) - 1;
                    skipStoreFlag = true;
                }

                //Logic for moving stones on the top row
                if (rowIndex == 0)
                {
                    //if the player turn is 1, do not place a stone in the store [1,6] in arryButtons if that is current location
                    if ((CurrentPlayer == 0 && rowIndex == 0 && j == PitLocations.GetLength(1) - 1) || skipStoreFlag)
                    {
                        //Skip over the opposing store, do not drop a stone
                    }
                    else //Places stones in pits regardless of player turn, as long as it isn't the opposing store
                    {
                        //if if goldenstonecounter > 50 place any stone, else if goldenstonecounter = 0 then place golden stone, otherwise do NOT place golden stone
                        if (GoldenStoneCounter > 50)
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                            score += 5;
                        }
                        //The golden stone is about to be placed into the store
                        else if (GoldenStoneCounter == 0)
                        {
                            for (int i = 0; i < PickedStones.Count; i++)
                            {
                                if (PickedStones[i].Color == "Golden")
                                {
                                    PitLocations[rowIndex, j].Stones.Add(PickedStones[i]);
                                    PickedStones.RemoveAt(i);
                                }
                            }
                        }
                        //The golden stone is picked, but is not ready to be placed to it's selected index
                        else if (GoldenStoneCounter > 0)
                        {
                            score += 5;
                            if (PickedStones[0].Color == "Golden" && PickedStones.Count > 1)
                            {
                                    PitLocations[rowIndex, j].Stones.Add(PickedStones[1]);
                                    PickedStones.RemoveAt(1);
                            }
                            else
                            {
                                PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                                PickedStones.RemoveAt(0);
                            }

                        }
                        //The golden stone has already been placed in this turn
                        else
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                            score += 5;
                        }

                        GoldenStoneCounter--;

                    }

                    //If end of the row (store) in PitLocations array has been reached for the top row,
                    //move down to the bottom row to continue movement
                    if (GameSettings["CycloneCurrentlyActive"] == 0)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Increment movement variable for navigating the internal array
                            j++;

                            if (j == PitLocations.GetLength(1))
                            {
                                rowIndex = 1;
                                j = 0;
                            }
                        }
                    }

                    //If Cyclone Mode is enabled, go backwards
                    ////If the end (beginning because its backwards) of PitLocations has been reached for the top row
                    if (GameSettings["CycloneCurrentlyActive"] == 1)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Decrement the movement variable for navigating the internal array
                            j--;

                            if (j == -1)
                            {
                                rowIndex = 1;
                                j = PitLocations.GetLength(1) - 1;
                            }
                        }

                        //For debugging
                        //MessageBox.Show(j.ToString());
                    }
                }

                //Logic for moving stones on bottom row
                if (rowIndex == 1 && PickedStones.Count != 0)
                {
                    //if the player turn is 2, do not place a stone in the store [0,6] in arryButtons if that is current location
                    if ((CurrentPlayer == 1 && rowIndex == 1 && j == PitLocations.GetLength(1) - 1) || skipStoreFlag)
                    {
                        //Skip over the opponent store, do not drop a store
                    }
                    else //Place stones in pits regardless of player turn, as long as it isn't the opposing store
                    {
                        //if if goldenstonecounter > 50 place any stone, else if goldenstonecounter = 0 then place golden stone, otherwise do NOT place golden stone
                        if (GoldenStoneCounter > 50)
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                            score += 5;
                        }
                        //The golden stone is about to be placed into the store
                        else if (GoldenStoneCounter == 0)
                        {
                            for (int i = 0; i < PickedStones.Count; i++)
                            {
                                if (PickedStones[i].Color == "Golden")
                                {
                                    PitLocations[rowIndex, j].Stones.Add(PickedStones[i]);
                                    PickedStones.RemoveAt(i);
                                }
                            }
                        }
                        //The golden stone is picked, but is not ready to be placed to it's selected index
                        else if (GoldenStoneCounter > 0)
                        {
                            score += 5;
                            if (PickedStones[0].Color == "Golden" && PitLocations[rowIndex, j].Stones.Count > 1)
                            {
                                PitLocations[rowIndex, j].Stones.Add(PickedStones[1]);
                                PickedStones.RemoveAt(1);
                            }
                            else
                            {
                                PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                                PickedStones.RemoveAt(0);
                            }

                        }
                        //The golden stone has already been placed in this turn
                        else
                        {
                            PitLocations[rowIndex, j].Stones.Add(PickedStones[0]);
                            PickedStones.RemoveAt(0);
                            score += 5;
                        }

                        GoldenStoneCounter--;

                    }

                    //If Cyclone Mode is disabled, continue regular movement
                    ////If end of the row (store) in PitLocations array has been reached for the bottom row,
                    ////move up to the top row to continue moving if possible
                    if (GameSettings["CycloneCurrentlyActive"] == 0)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Increment movement variable for navigating the internal array
                            j++;

                            if (j == PitLocations.GetLength(1))
                            {
                                rowIndex = 0;
                                j = 0;
                            }
                        }
                    }

                    //If Cyclone Mode is enabled, continue backwards movement
                    ////If the end (beginning because of cyclone) of PitLocations has been reached on the top bottom row, go to the top row
                    if (GameSettings["CycloneCurrentlyActive"] == 1)
                    {
                        if (PickedStones.Count != 0)
                        {
                            //Decrement the movement variable for navigating the internal array
                            j--;

                            if (j == -1)
                            {
                                rowIndex = 0;
                                j = PitLocations.GetLength(1) - 1;
                            }
                        }

                        //For debugging
                        //MessageBox.Show(j.ToString());
                    }
                }
            }

            //Debug
            //MessageBox.Show(rowIndex.ToString() + "\n" + j.ToString() + "\n" + score.ToString(),
            //    "Row Index & J & Score");

            if (GoldenStoneCounter < 50)
            {
                score += 100;
            }
            //Returns the values of the rowIndex and J to be used elsewhere
            return (rowIndex, j, score);
        }

        //Playturn that is overloaded to run for the CPU, providing all necessary objects to complete a turn
        public async Task<(int, int)> PlayTurn(int CurrentPlayer, Button[,] arryButtons, Pit[,] PitLocations,
            Dictionary<string, string> PlayerAtts, Board originalBoard, Dictionary<string, int> GameSettings)
        {
            int lastRowIndex = 0;
            int lastColumnIndex = 0;
            int ChosenPitIndex = 9;
            int rowIndex;
            List<Stone> PickedStones = new List<Stone>();

            if (CurrentPlayer == 0)
            {
                rowIndex = 1;
            }
            else
            {
                rowIndex = 0;
            }

            //all of the methods to determine which pit will be selected by the difficult CPU
            ChosenPitIndex = DecideMove(Difficulty, PitLocations, rowIndex, PlayerAtts, CurrentPlayer, originalBoard,
                GameSettings);

            //An indexer variable to be used when moving stones
            int j;

            //A correction for the pit index for Cyclone Mode
            int CorrectedPitIndex;

            //Updates the indexer for MoveStones to be correct dependent on cyclone mode being active
            if (GameSettings["CycloneCurrentlyActive"] == 0)
            {
                j = ChosenPitIndex + 1;
                CorrectedPitIndex = j - 1;
            }
            else
            {
                j = ChosenPitIndex - 1;
                CorrectedPitIndex = j + 1;
            }

            //Obtains the count of how many pits the golden stone should move
            int GoldenStoneCounter = GetGoldenStoneCounter(GameSettings["GoldenMode"], rowIndex, CorrectedPitIndex, PitLocations,
                GameSettings["CycloneCurrentlyActive"]);

            //Debug
            //MessageBox.Show(GoldenStoneCounter.ToString());

            //to simulate cpu thinking, wait a random amount of milliseconds
            Random random = new Random();
            int thinkingWait = random.Next(2000, 5000);
            await Task.Delay(thinkingWait);

            //pick the stones into a list
            PickedStones = PickUpStones(rowIndex, ChosenPitIndex, PitLocations, originalBoard.arryButtons);

            //MoveStones Method
            (lastRowIndex, lastColumnIndex) = await MoveStones(j, CurrentPlayer, rowIndex, PickedStones,
                originalBoard.arryButtons, PitLocations, GameSettings, GoldenStoneCounter);

            return (lastRowIndex, lastColumnIndex);
        }

        //Determines what pit to select based on the difficulty, and state of the game board (difficult CPU mode)
        public int DecideMove(string Difficulty, Pit[,] PitLocations, int startingRowIndex,
            Dictionary<string, string> PlayerAtts, int CurrentPlayer, Board originalBoard,
            Dictionary<string, int> GameSettings)
        {
            //value out of bounds by default for now, will help potentially with
            //debugging. we don't want a valid value being passed incorrectly
            int ChosenPitIndex = 11;

            //no logic, just a shell for how we could possibly handle it
            if (Difficulty.ToUpper() == "EASY")
            {
                bool validPit = false;

                //ChosenPitIndex = Result of Randomized Pit Selection Method
                Random random = new Random();

                //while something is false
                do
                {
                    ChosenPitIndex = random.Next(0, PitLocations.GetLength(1) - 1);
                    if (PitLocations[startingRowIndex, ChosenPitIndex].Stones.Count() > 0)
                    {
                        validPit = true;
                    }
                } while (!validPit);
                //check if pit is empty, or button is accessible
                //if not empty, set to true and finish function
            }
            else
            {
                //Create a dictionary to hold all of the scores for each potential turn
                Dictionary<int, int> scores = new Dictionary<int, int>();

                //Instantiating Game Logic to be used in the advanced-decision making
                GameLogic tempGameLogic =
                    new GameLogic(PlayerAtts, CurrentPlayer, false, true, GameSettings, GameCanvas);

                //For loop to iterate over each potential pit
                for (int potentialPitIndex = 0; potentialPitIndex < PitLocations.GetLength(1) - 1; potentialPitIndex++)
                {
                    //Create a board in memory of the current gamestate of the original board
                    Board tempBoard = new Board(PlayerAtts, true, GameSettings, GameCanvas);
                    Board staticTempBoard = new Board(PlayerAtts, true, GameSettings, GameCanvas);
                    tempBoard = tempGameLogic.CreateMemoryBoard(originalBoard, originalBoard.PitLocations, PlayerAtts,
                        GameSettings);
                    staticTempBoard = tempGameLogic.CreateMemoryBoard(originalBoard, originalBoard.PitLocations,
                        PlayerAtts, GameSettings);

                    //Making sure the pit has at least one stone to be able to be selected
                    if (tempBoard.PitLocations[startingRowIndex, potentialPitIndex].Stones.Count() > 0)
                    {
                        //Run PlayTurn with that board
                        int score = SimulateTurn(potentialPitIndex, CurrentPlayer, tempBoard.PitLocations,
                            tempGameLogic, tempBoard, GameSettings, staticTempBoard);

                        //Assign score value to the dictionary
                        scores[potentialPitIndex] = score;
                    }
                    else
                    {
                        //Assign a negative value to the pit that cannot be selected, to prevent it from causing an 'out-of-bounds' error
                        scores[potentialPitIndex] = -10;
                    }
                }

                //Compare values of each turn to find the highest scoring turn
                //Instantiated variables to find the highest score value in the dictionary
                int currentHighestKey = 0;
                int currentHighestValue = 0;

                //Looping through the dictionary
                for (int key = 0; key < scores.Count; key++)
                {
                    //Determining the highest value
                    if (scores[key] > currentHighestValue)
                    {
                        currentHighestKey = key;
                        currentHighestValue = scores[key];
                    }
                }

                //The key that has the highest score value will be the chosenPitIndex
                ChosenPitIndex = currentHighestKey;

                //For debugging, show the highest score as well as the chosen pit index
                //MessageBox.Show(currentHighestValue.ToString() + " " + ChosenPitIndex.ToString(),
                //    "Highest Score Value & Chosen Index");
            }

            //Return this to be used to actually move a piece on the game board
            return ChosenPitIndex;
        }

        //Method used to simulate stone movement on for a pit
        public (int, int, int) SimulateStoneMovement(int potentialPitIndex, int CurrentPlayer, Pit[,] PitLocations,
            Board tempBoard, Dictionary<string, int> GameSettings, Board staticTempBoard)
        {
            //Moving stones score variable
            int score = 0;

            //Instantiating a rowIndex & pickedStones amount to be used in methods below
            int rowIndex;
            int pickedStones;

            List<Stone> PickedStones = new List<Stone>();

            //Checks for current player value
            if (CurrentPlayer == 0)
            {
                rowIndex = 1;
            }
            else
            {
                rowIndex = 0;
            }

            //Assigns the stones that were picked up into the list of PickedStones
            PickedStones = PickUpStones(rowIndex, potentialPitIndex, PitLocations);

            //Intializes j for movement in moving stones
            int j;

            if (GameSettings["CycloneCurrentlyActive"] == 0)
            {
                j = potentialPitIndex + 1;
            }
            else
            {
                j = potentialPitIndex - 1;
            }

            //Moves stones on the board in memory and returns a score on how well it 'played'
            (rowIndex, potentialPitIndex, score) = MoveStones(j, CurrentPlayer, rowIndex, PickedStones,
                tempBoard.PitLocations, GameSettings, staticTempBoard.PitLocations);

            return (rowIndex, potentialPitIndex, score);
        }

        //Method used to simulate an entire turn, including the logic checks for the turn such as captures and going again
        public int SimulateTurn(int potentialPitIndex, int CurrentPlayer, Pit[,] PitLocations, GameLogic tempGameLogic,
            Board tempBoard, Dictionary<string, int> GameSettings, Board staticTempBoard)
        {
            int lastRowIndex;
            int lastColIndex;
            int score = 0;
            int captureScore = 0;

            //Simulates the movement of stones on the virtual board & assigns points based on the amount of stones moved in a turn
            (lastRowIndex, lastColIndex, score) = SimulateStoneMovement(potentialPitIndex, CurrentPlayer,
                tempBoard.PitLocations, tempBoard, GameSettings, staticTempBoard);

            //Checks for capture on virtual board
            bool successfulCapture;
            (successfulCapture, captureScore) =
                tempGameLogic.CheckForCapture(lastRowIndex, lastColIndex, tempBoard, GameSettings);

            //Adds the score granted from capturing stones to the total score value
            score += captureScore;

            //For debugging
            //Check to see if they can go again
            bool goAgain;
            goAgain = SimulatePlayerGoAgain(CurrentPlayer, lastRowIndex, lastColIndex, PitLocations, GameSettings);
            if (goAgain)
            {
                //Increment score by 50
                score += 50;
            }

            //Validate the end of the Game
            tempGameLogic.ValidateGameEnd(tempBoard);

            //For debugging
            //MessageBox.Show(goAgain.ToString() + potentialPitIndex);

            return score;
        }

        //Checks to see if the player (CPU) would be able to go again, and returns that value
        public bool SimulatePlayerGoAgain(int PlayerTurn, int lastRowIndex, int lastColIndex, Pit[,] PitLocations,
            Dictionary<string, int> GameSettings)
        {
            //Declaring a variable to determine when the player can go again
            int goAgainIndex;

            //Determining the goAgainIndex by the length of the PitLocations array
            goAgainIndex = PitLocations.GetLength(1) - 1;

            //Debug
            //MessageBox.Show(lastColIndex.ToString() + "\n" + goAgainIndex.ToString(),
            //    "Last Col Index & Go Again Index");

            //Check to see if player 1 could go again
            if (PlayerTurn == 0)
            {
                //If the player landed at their store, allow them to go again
                if ((lastRowIndex == 1 && lastColIndex == goAgainIndex))
                {
                    //Allows the first player to go again
                    return true;
                }
                else
                {
                    //Allows the second player to go and changes controls
                    return false;
                }
            }

            //Check to see if player 2 could go again
            if (PlayerTurn == 1)
            {
                if ((lastRowIndex == 0 && lastColIndex == goAgainIndex))
                {
                    //Allows the second player to go again
                    return true;
                }
                else
                {
                    //Allows the first player to go & changes controls
                    return false;
                }
            }

            return false;
        }

        //method to see if golden stone is in selected pit
        public Tuple<bool, int> CheckForGoldenStone(int rowIndex, int colIndex, Pit[,] PitLocations)
        {
            int pickedStones;
            List<Stone> PickedStones = new List<Stone>();
            Tuple<bool, int> goldenStonePit = Tuple.Create(false, 0);


            //count the number of stones we will be moving. will be used for iterations below
            pickedStones = PitLocations[rowIndex, colIndex].Stones.Count();

            //for each stone, add it to the next PitLocation, then remove it
            //from the main PitLocations array index.. this should be a self contained
            //loop to maintain the integrity of the PitLocations array stones
            for (int i = 0; i < pickedStones; i++)
            {
                if (PitLocations[rowIndex, colIndex].Stones[i].Color == "Golden")
                {
                    goldenStonePit = Tuple.Create(true, colIndex);
                }
            }

            return goldenStonePit;
        }

        //Clears the colored stones that have populated a pit
        public void ClearVisualStones(Canvas gameCanvas, Button curButton)
        {
            if (curButton.Content is Canvas buttonCanvas)
            {
                buttonCanvas.Children.Clear(); // Remove all children from the Canvas
            }
        }

        //Creates a stone to be placed on a pit with a random position and given color
        public void CreateVisualStone(string stoneColor, Canvas gameCanvas, Button curButton)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            //set fill for stones
            switch (stoneColor)
            {
                case "Red":
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                    break;
                case "Blue":
                    ellipse.Fill = new SolidColorBrush(Colors.Blue);
                    break;
                case "Green":
                    ellipse.Fill = new SolidColorBrush(Colors.Green);
                    break;
                case "Purple":
                    ellipse.Fill = new SolidColorBrush(Colors.Purple);
                    break;
                case "Golden":
                    ellipse.Fill = new SolidColorBrush(Colors.Gold);
                    break;
                default:
                    ellipse.Fill = new SolidColorBrush(Colors.Gray);
                    break;
            }

            //check to see if current button is a canvas already to avoid over-writing the children (visual Stones)
            if (curButton.Content is Canvas buttonCanvas)
            {
                buttonCanvas.Children.Add(ellipse);
            }
            else
            {
                Canvas newCanvas = new Canvas();
                newCanvas.Children.Add(ellipse);
                curButton.Content = newCanvas;
            }

            //set position of stone in the button
            Random random = new Random();
            int xPositionModifier;
            int yPositionModifer;
            if (curButton.Height < 120)
            {
                xPositionModifier = random.Next(50, 95);
                yPositionModifer = random.Next(50, 95);
            }
            else
            {
                xPositionModifier = random.Next(50, 100);
                yPositionModifer = random.Next(80, 190);
            }


            Canvas.SetLeft(ellipse, curButton.ActualWidth + ellipse.Width - xPositionModifier);
            Canvas.SetTop(ellipse, curButton.ActualHeight + ellipse.Width - yPositionModifer);
        }

        //Used to obtain how many pits the golden stone needs to travel to reach a desired destination
        //Used for CPU only
        public int GetGoldenStoneCounter(int goldenMode, int rowIndex, int potentialPitIndex, Pit[,] PitLocations,
            int cycloneActive)
        {
            bool goldenStoneInPit = false;
            int GoldenStoneCounter = 99;
            List<Stone> PickedStones = new List<Stone>();

            //count the number of stones we will be moving. will be used for iterations below
            int pickedStones = PitLocations[rowIndex, potentialPitIndex].Stones.Count();

            //for each stone, add it to the next PitLocation, then remove it
            //from the main PitLocations array index.. this should be a self contained
            //loop to maintain the integrity of the PitLocations array stones
            for (int i = 0; i < pickedStones; i++)
            {
                PickedStones.Add(PitLocations[rowIndex, potentialPitIndex].Stones[i]);
            }

            //If Golden Stone Mode is active
            if (goldenMode == 1)
            {
                //Looping through the stones to see if the golden stone is present in the selected pit
                foreach (Stone stone in PickedStones)
                {
                    if (stone.Color == "Golden")
                    {
                        //Setting the flag to true
                        goldenStoneInPit = true;
                    }
                }
            }

            //Debug
            //MessageBox.Show(goldenStoneInPit.ToString() + "\n" + potentialPitIndex.ToString(), "Golden Stone In Pit (Bool)");

            //If the golden stone exists in the pit
            if (goldenStoneInPit)
            {
                //If normal movement is in place, calculate if the golden stone can land in the store
                if (cycloneActive == 0)
                {
                    if ((potentialPitIndex) + PickedStones.Count >= PitLocations.GetLength(1) - 1)
                    {
                        //Place it in the store
                        GoldenStoneCounter = (PitLocations.GetLength(1) - 1) - (potentialPitIndex - 1) - 2;

                        //Debug
                        //MessageBox.Show(GoldenStoneCounter.ToString());

                    }
                    else
                    {
                        //Place it in the closest pit to the selected pit
                        GoldenStoneCounter = 0;
                    }
                }
                else //Cyclone movement is in play, rework through the calculations above
                {
                    //If the golden stone is not in the closest pit to the opponent store AND it CAN reach the owner's store
                    //Place it in the owner's store
                    if (PickedStones.Count >= (PitLocations.GetLength(1)) + (potentialPitIndex))
                    {
                        GoldenStoneCounter = PitLocations.GetLength(1) + potentialPitIndex - 1;

                        //Debug
                        //MessageBox.Show("Golden Stone Counter & Potential Pit Index \n" + GoldenStoneCounter.ToString() + "\n" + potentialPitIndex, "The Golden Stone can reach it's own store");
                    }
                    //If the golden stone is not in the closest pit to the opponent store AND it cannot reach the owner's store
                    //try and place it into that closest pit
                    else if (PickedStones.Count < (PitLocations.GetLength(1)) + (potentialPitIndex))
                    {
                        //If the golden stone can reach the closest pit to the opponent store, place it in there
                        if (PickedStones.Count >= potentialPitIndex)
                        {
                            //Place it into the closest pit to the opponent store
                            GoldenStoneCounter = potentialPitIndex - 1;

                            //Debug
                            //MessageBox.Show(GoldenStoneCounter.ToString() + "\n" + potentialPitIndex, "The Golden Stone cannot reach the store, but CAN reach the ending pit");
                        }
                        else
                        //If the golden stone can NOT reach that pit, nudge it one pit over
                        //If the stone cannot be placed into that furthest pit, nudge it closer to the end pit
                        {
                            //Place it in the closest pit to the selected pit
                            GoldenStoneCounter = 0;

                            //Debug
                            //MessageBox.Show(GoldenStoneCounter.ToString() + "\n", "The Golden stone cannot reach the store, and cannot reach the ending pit");
                        }
                    }
                }
            }
            return GoldenStoneCounter;
        }
    }
}

