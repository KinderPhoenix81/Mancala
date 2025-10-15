using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Game_Mancala
{
    public class GameLogic
    {
        public GameLogic(Dictionary <string, string> PlayerAtts, int CurPlayer, bool GameEnded, bool tempBoard, Dictionary<string, int> GmeSettings, Canvas GmeCanvas) 
        { 
            GameBoard = new Board(PlayerAtts, tempBoard, GmeSettings, GmeCanvas);
            CurrentPlayer = CurPlayer;
            GameOver = GameEnded;
            GameSettings = GmeSettings;
            GameCanvas = GmeCanvas;
        }

        //Fields of game logic
        public Board GameBoard;
        public int CurrentPlayer;
        public bool GameOver;
        public bool TurnInProgress = true;
        public Dictionary<string, int> GameSettings;
        public Canvas GameCanvas;

        //check landing pit to see if player can capture stones from opponents adjacent pit
        public (bool, int) CheckForCapture(int lastRowIndex, int lastColIndex, Board gameBoard, Dictionary <string, int> GameSettings)
        {
            int LandingPitStoneCount;
            //int ActualLastColIndex;
            int PlayerStoreRowIndex;
            int OpponentRowIndex;

            int score = 0;
            int size = gameBoard.PitLocations.GetLength(1) - 1;
            //get the opponents pit index. this is the last column index that was taken in minus
            //the size of the pitlocations array, absolute value. ex: on an 8 pitted board
            //if the final stone was placed in the 4th pit, we'd look at the 3rd indexed pit
            //on the opponents board
            int oppositePitIndex = Math.Abs(lastColIndex - size);

            //Debugging
            //MessageBox.Show(CurrentPlayer.ToString() + lastColIndex.ToString());

            //if they landed on their on side (player turn does not match the rowIndex...) AND they did not land in a store
            if (lastRowIndex != CurrentPlayer && lastColIndex != size)
            {
                //Reduces the opposite pit index due to off-by-one from array length and indexing
                oppositePitIndex--;

                //get the count of stones to check if there is only 1 stone in last pit
                LandingPitStoneCount = gameBoard.PitLocations[lastRowIndex, lastColIndex].Stones.Count;
                int oppositePitStoneCount = gameBoard.PitLocations[CurrentPlayer, oppositePitIndex].Stones.Count;

                //if the last pit now contains 1 stone
                if (LandingPitStoneCount == 1 && oppositePitStoneCount > 0)
                {
                    //set PlayerStoreRowIndex to be opposite of PlayerTurn
                    if (CurrentPlayer == 0)
                    {
                        PlayerStoreRowIndex = 1;
                    }
                    else
                    {
                        PlayerStoreRowIndex = 0;
                    }

                    //set OpponentRowIndex for readability (we can remove this and use PlayerTurn if we want)
                    OpponentRowIndex = CurrentPlayer;


                    //create list for captured stones
                    List<Stone> CapturedStones = new List<Stone>();

                    //count the number of stones we will be capturing. will be used for iterations below
                    int CapturedStonesCount = gameBoard.PitLocations[OpponentRowIndex, oppositePitIndex].Stones.Count();

                    //capture landing pit
                    CapturedStones.Add(gameBoard.PitLocations[lastRowIndex, lastColIndex].Stones[0]);
                    gameBoard.PitLocations[lastRowIndex, lastColIndex].Stones.RemoveAt(0);
                    gameBoard.PitLocations[PlayerStoreRowIndex, size].Stones.Add(CapturedStones[0]);
                    CapturedStones.RemoveAt(0);

                    //for each stone in the capture pit, place it in the store and remove it from the capture pit
                    for (int i = 0; i < CapturedStonesCount; i++)
                    {
                        CapturedStones.Add(gameBoard.PitLocations[OpponentRowIndex, oppositePitIndex].Stones[0]);
                        gameBoard.PitLocations[OpponentRowIndex, oppositePitIndex].Stones.RemoveAt(0);
                        gameBoard.PitLocations[PlayerStoreRowIndex, size].Stones.Add(CapturedStones[0]);
                        CapturedStones.RemoveAt(0);
                        score += 15;
                    }

                    //Only change the button content on the actual game board, not the virtual board in memory
                    if(gameBoard.tempBoard == false)
                    {
                        gameBoard.player1.ClearVisualStones(GameCanvas, gameBoard.arryButtons[PlayerStoreRowIndex, size]);
                        gameBoard.player1.ClearVisualStones(GameCanvas, gameBoard.arryButtons[OpponentRowIndex, oppositePitIndex]);
                        gameBoard.player1.ClearVisualStones(GameCanvas, gameBoard.arryButtons[lastRowIndex, lastColIndex]);
                        foreach (Stone currentStone in gameBoard.PitLocations[PlayerStoreRowIndex, size].Stones)
                        {
                            gameBoard.player1.CreateVisualStone(currentStone.Color, GameCanvas, gameBoard.arryButtons[PlayerStoreRowIndex, size]);
                        }
                    }

                    //Return true, the capture was successful
                    return (true, score);
                }
            }
            //Return false, the capture did not occur
            return (false, 0);
        }

        //displays game winner. maybe prompts user for new game/quit
        public string ShowWinner()
        {
            //Values to track how many stones are in each player pit
            int player1Score = GameBoard.PitLocations[1, GameBoard.PitLocations.GetLength(1) - 1].Stones.Sum(stone => stone.PointValue);
            int Player2Score = GameBoard.PitLocations[0, GameBoard.PitLocations.GetLength(1) - 1].Stones.Sum(stone => stone.PointValue);
            //Tracks who the winner is
            string winner;


            if (player1Score > Player2Score)
            {
                winner = "Player One Wins!";
            }
            else
            {
                winner = "Player Two Wins!";
            }

            //Displays the winner with their score as a messagebox
            winner = winner + "\n\nPlayer One Score: " + player1Score.ToString();
            winner = winner + "\nPlayer Two Score: " + Player2Score.ToString();
            winner = winner + "\n\nPlay Again?";

            //Returns the winner
            return winner;
        }

        //checks for game end condition - if one array side is empty
        public void ValidateGameEnd(Board gameBoard)
        {
            int rowStoneSum = 0;

            //loop through both rows of pitlocation array. sum stone counts.
            //if either row sum is 0, the game is over
            for (int row = 0; row < 2; row++)
            {
                for (int i = 0; i < gameBoard.PitLocations.GetLength(1) - 1; i++)
                {
                    rowStoneSum += gameBoard.PitLocations[row, i].Stones.Count;
                }
                //after summing stone counts on the row, check if 
                if (rowStoneSum == 0)
                {
                    //if the row pits are empty, set game variable
                    GameOver = true;
                    //stop looping if game is over
                    break;
                }
                //reset sum for next row check
                rowStoneSum = 0;
            }

            //disable all buttons if game is over. also clear out all pits and put stones in their players stores
            if (GameOver)
            {
                //Iterates through both rows
                for (int row = 0; row < 2; row++)
                {
                    //Iterates through each pit
                    for (int col = 0; col <= gameBoard.PitLocations.GetLength(1) - 1; col++)
                    {
                        if(gameBoard.tempBoard == false)
                        gameBoard.arryButtons[row, col].IsEnabled = false;

                        //move all stones to respective player's store
                        //at end of game, each player gets all of the stones still on their side
                        //one side should always already be empty, but the loop shouldn't care
                        //and should still function fine iterating over empty pits.
                        int initialStoneCount = gameBoard.PitLocations[row, col].Stones.Count();
                        for (int tmpCounter = 0;
                             tmpCounter < initialStoneCount;
                             tmpCounter++)
                        {
                            //add to store, remove from pit
                            gameBoard.PitLocations[row, gameBoard.PitLocations.GetLength(1)-1].Stones.Add(gameBoard.PitLocations[row, col].Stones[0]);
                            gameBoard.PitLocations[row, col].Stones.RemoveAt(0);
                        }
                        //update button contents. should always be 0
                        if(gameBoard.tempBoard == false)
                        {
                            gameBoard.player1.ClearVisualStones(GameCanvas, gameBoard.arryButtons[row, col]);
                        }
                        
                    }
                }

                //update store button content with new stone counts
                if(gameBoard.tempBoard == false)
                {
                    foreach (Stone currentStone in gameBoard.PitLocations[0, gameBoard.PitLocations.GetLength(1) - 1].Stones)
                    {
                        gameBoard.player1.CreateVisualStone(currentStone.Color, GameCanvas, gameBoard.arryButtons[0, gameBoard.arryButtons.GetLength(1) - 1]);
                    }

                    foreach (Stone currentStone in gameBoard.PitLocations[1, gameBoard.PitLocations.GetLength(1) - 1].Stones)
                    {
                        gameBoard.player1.CreateVisualStone(currentStone.Color, GameCanvas, gameBoard.arryButtons[1, gameBoard.arryButtons.GetLength(1) - 1]);
                    }
                }
            }
        }

        //Changes the player's controls based on who's turn it is
        public void ChangePlayerControls()
        {
            int size = GameBoard.arryButtons.GetLength(1) - 1;
            //Variables to hold which row of controls should be enabled or disabled
            int enableRow;
            int disableRow;

            //0 is player 1, 1 is player 2. set vars for which row to enable and which to disable
            if (CurrentPlayer == 0)
            {
                //Enable the bottom row of button controls (pits) & disable the top row
                enableRow = 1;
                disableRow = 0;
            }
            else
            {
                //Enable the top row of button controls (pits) & disable the bottom row
                enableRow = 0;
                disableRow = 1;
            }

            //Loop to enable / disable the button controls one by one
            for (int col = 0; col < size; col++)
            {
                //if pitlocation.stones.count == 0, don't enable
                GameBoard.arryButtons[enableRow, col].IsEnabled = true;

                GameBoard.arryButtons[disableRow, col].IsEnabled = false;
            }


            //Loop to enable / disable the button controls one by one
            for (int col = 0; col < size; col++)
            {
                //If the pit has 0 stones, disable the pit so the player cannot interact with it
                if (GameBoard.PitLocations[enableRow, col].Stones.Count == 0)
                {
                    GameBoard.arryButtons[enableRow, col].IsEnabled = false;
                }
                else
                {
                    //Enable the pits of the enable row
                    GameBoard.arryButtons[enableRow, col].IsEnabled = true;
                }

                //Disable to opponents row of pits based on player turn
                GameBoard.arryButtons[disableRow, col].IsEnabled = false;
            }
        }

        //with async tasks, buttons can be pushed while a task is running. this disables everything so tasks don't step on eachother
        public void DisableAllControls()
        {
            int size = GameBoard.arryButtons.GetLength(1) - 1;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    GameBoard.arryButtons[row, col].IsEnabled = false;
                }
            }
        }

        //Current method to change the player turn one by one
        public void ChangePlayerTurn(int PlayerTurn, Label eventLabel, int lastRowIndex, int lastColIndex, Dictionary <string, int> GameSettings, Pit[,] PitLocations)
        {
            //Declaring a variable to determine where the tests should be ran for allowing a player to go again
            //Due to the backwards cycling of values, going again was activating when it shouldn't have
            int goAgainIndex;

            //Assigning the goAgainIndex to the store location
            goAgainIndex = PitLocations.GetLength(1) - 1;
            
            if (PlayerTurn == 0)
            {
                //If the player landed at their store, allow them to go again
                if ( (lastRowIndex == 1 && lastColIndex == goAgainIndex) )
                {
                    //Allows the first player to go again
                    CurrentPlayer = 0;
                    eventLabel.Content = "Player 1 Goes Again!";

                    //If Cyclone Mode is Active, enable / disable the cyclone based on it's current state
                    if (GameSettings["CycloneMode"] == 1)
                    {
                        if (GameSettings["CycloneCurrentlyActive"] == 0)
                        {
                            //Begin a cyclone
                            GameSettings["CycloneCurrentlyActive"] = 1;
                        }
                        else
                        {
                            //End a cyclone
                            GameSettings["CycloneCurrentlyActive"] = 0;
                        }
                    }

                    //Updates controls on board
                    ChangePlayerControls();

                    //Ends statement
                    return;
                } else
                {
                    //Allows the second player to go and changes controls
                    CurrentPlayer = 1;
                    this.ChangePlayerControls();
                    return;
                }
            }
                
            if (PlayerTurn == 1)
            {
                if ( (lastRowIndex == 0 && lastColIndex == goAgainIndex) )
                {
                    //Allows the second player to go again
                    CurrentPlayer = 1;
                    eventLabel.Content = "Player 2 Goes Again!";

                    //If Cyclone Mode is Active, enable / disable the cyclone based on it's current state
                    if (GameSettings["CycloneMode"] == 1)
                    {
                        if (GameSettings["CycloneCurrentlyActive"] == 0)
                        {
                            GameSettings["CycloneCurrentlyActive"] = 1;
                        }
                        else
                        {
                            GameSettings["CycloneCurrentlyActive"] = 0;
                        }
                    }

                    this.ChangePlayerControls();
                    return;
                }
                else
                {
                    //Allows the first player to go & changes controls
                    CurrentPlayer = 0;
                    this.ChangePlayerControls();
                    return;
                }
            }
        }

        //Creates a board in memory to be used by the CPU for each turn it simulates
        //Copies the original state of the board for each instance
        public Board CreateMemoryBoard(Board originalBoard, Pit[,] PitLocations, Dictionary<string, string> PlayerAtts, Dictionary<string, int> GameSettings)
        {
            //Creating a new board in memory to store the values of PitLocations
            Board memoryBoard = new Board(PlayerAtts, true, GameSettings);

            //Creating a new array of pits for the board in memory
            memoryBoard.PitLocations = new Pit[2, PitLocations.GetLength(1)];

            //Looping through the current pit locations array and assigning those values into the new memory board
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < PitLocations.GetLength(1); j++)
                {
                    //Create a new pit object for each instance of the pit
                    memoryBoard.PitLocations[i, j] = new Pit();

                    for (int  k = 0; k < originalBoard.PitLocations[i, j].Stones.Count; k++)
                    {
                        //Fill the selected pit object with the same stones from the original board
                        memoryBoard.PitLocations[i, j].Stones.Add(originalBoard.PitLocations[i, j].Stones[k]);
                    }
                }
            }

            //Returning a the memory board
            return memoryBoard;
        }

        //Updates the score labels after each turn
        public void UpdateScoreLabels(Label playerOneScoreValueLabel, Label playerTwoScoreValueLabel, Pit[,] pitLocations)
        {
            playerOneScoreValueLabel.Content = pitLocations[1, pitLocations.GetLength(1) - 1].Stones.Sum(stone => stone.PointValue).ToString();
            playerTwoScoreValueLabel.Content = pitLocations[0, pitLocations.GetLength(1) - 1].Stones.Sum(stone => stone.PointValue).ToString();
        }

        //Updates the turn label
        public void UpdateTurnLabel(Label playerLabel, int CurrentPlayer)
        {
            if (CurrentPlayer == 0)
            {
                playerLabel.Content = "Player 1";
            }
            else
            {
                playerLabel.Content = "Player 2";
            }
        }

        //Updates the event label to display a capture
        public void DisplayEventLabelCapture(Label eventLabel, bool successfulCapture)
        {
            if (successfulCapture)
            {
                eventLabel.Content = "A capture was successful!";
            }
        }

        //Clears the event label contents
        public void ClearEventLabel(Label eventLabel)
        {
            eventLabel.Content = "";
        }

        //Updates the Cyclone Label Contents
        public async Task UpdateCycloneLabel(Label cycloneLabel, Dictionary <string, int> GameSettings)
        {
            if (GameSettings["CycloneCurrentlyActive"] == 1)
            {
                cycloneLabel.Content = "The Cyclone Is Active!";
            } else
            {
                cycloneLabel.Content = "The Storm is calm";
            }

            if (GameSettings["CycloneMode"] == 0)
            {
                cycloneLabel.Content = "Cyclone Mode is Disabled.";
            }

            //Adding a delay to observe changing labels before they are cleared at beginning of turn
            await Task.Delay(800);
        }
    }
}
