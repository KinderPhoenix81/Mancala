using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace Game_Mancala
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        //Constructor / initializer for the game window
        public GameWindow(Dictionary<string, string> PlayerAtts, Dictionary<string, int> GmeSettings)
        {
            PlayerAttributes = PlayerAtts;
            GameSettings = GmeSettings;
            InitializeComponent();
            GameLogic = new GameLogic(PlayerAtts, 0, false, false, GmeSettings, gameCanvas);
            dynamicallySizeBoard(GameSettings["PitCount"]);

        }

        //pass player attributes through by making it a property of game window
        public Dictionary<string, string> PlayerAttributes;

        //pass Game settings through by making it a property of game window
        public Dictionary<string, int> GameSettings;

        //holds all validation checks and gamestate vars, like CurrentPlayer and GameOver
        GameLogic GameLogic;

        //create an array of the buttons
        public Button[,] arryButtons = new Button[2, 7];

        //When the game window is loaded, do the following, once.
        private async void gameWindowOnLoad(object sender, RoutedEventArgs e)
        {
            //Resetting the board to default values between 
            GameSettings["CycloneCurrentlyActive"] = 0;
            
            //Initializes fields of board
            GameLogic.GameBoard = new Board(PlayerAttributes, false, GameSettings, gameCanvas);
            arryButtons = new Button[2, GameSettings["PitCount"] + 1];

            //Assigns the array buttons to the property of the gameboard
            GameLogic.GameBoard.arryButtons = arryButtons;

            //Creates the new pit locations for the game board based on the specified size
            GameLogic.GameBoard.PitLocations = new Pit[2, GameSettings["PitCount"] + 1];

            //Sets the players in the array of players equal to the 2 players playing the game (assignment)
            GameLogic.GameBoard.Players[0] = GameLogic.GameBoard.player1;
            GameLogic.GameBoard.Players[1] = GameLogic.GameBoard.player2;

            //Dynamically creates the buttons (pits) of the board
            arryButtons = setButtons(arryButtons);

            //Sets up the pits
            GameLogic.GameBoard.PitSetup(GameSettings["PitCount"], gameCanvas, arryButtons, GameSettings);

            //Enables the correct player controls based on the current player turn
            GameLogic.ChangePlayerControls();

            //Clearing the content of the eventLabel
            GameLogic.ClearEventLabel(eventLabel);

            //Initializes a score value that would be used for the CPU, other methods return it, and rather than overloading those
            //With a single variable change, this dummy variable holds that unused information
            int score = 0;

            //If a CPU is player one, start their turn
            while (GameLogic.GameBoard.Players[GameLogic.CurrentPlayer].IsCPU == true && GameLogic.GameOver == false)
            {
                //Initializing variables to be used throughout the turn
                //Indicators of what happened during the turn
                int lastRowIndex;
                int lastColIndex;

                //String to hold winner text that is updated later
                string winner;

                //Stating that the turn is in progress
                GameLogic.TurnInProgress = true;

                //Disabling controls so the user cannot interact with anything during the CPU's turn
                GameLogic.DisableAllControls();

                //Clearing the content of the eventLabel for the beginning of the turn
                GameLogic.ClearEventLabel(eventLabel);

                //Plays the CPUs turn, returns where the last stone was placed during that turn
                (lastRowIndex, lastColIndex) = await GameLogic.GameBoard.Players[GameLogic.CurrentPlayer].PlayTurn(GameLogic.CurrentPlayer, arryButtons,
                    GameLogic.GameBoard.PitLocations, PlayerAttributes, GameLogic.GameBoard, GameSettings);

                //Checks for successful capture based on information returned above
                bool successfulCapture;
                (successfulCapture, score) = GameLogic.CheckForCapture(lastRowIndex, lastColIndex, GameLogic.GameBoard, GameSettings);

                //Display a successful capture in the event label
                GameLogic.DisplayEventLabelCapture(eventLabel, successfulCapture);

                //Run a method to see if the conditions have been met to end the game
                GameLogic.ValidateGameEnd(GameLogic.GameBoard);
                if (GameLogic.GameOver == true)
                {
                    //Assign winner to the result content of the ShowWinner method
                    winner = GameLogic.ShowWinner();

                    //Displays the winner in a message box
                    MessageBox.Show(winner);

                    //Debug
                    //DialogResult result = MessageBox.Show(winner, "Game Over", MessageBoxButtons.YesNo);
                }

                //change player turn and controls as long as the game is not over
                if (!GameLogic.GameOver)
                {
                    GameLogic.ChangePlayerTurn(GameLogic.CurrentPlayer, eventLabel, lastRowIndex, lastColIndex, GameSettings, GameLogic.GameBoard.PitLocations);
                }

                //Updating score labels for both players at the end of the turn
                GameLogic.UpdateScoreLabels(playerOneScoreValueLabel, playerTwoScoreValueLabel, GameLogic.GameBoard.PitLocations);

                //Updates the current player turn label
                GameLogic.UpdateTurnLabel(playerLabel, GameLogic.CurrentPlayer);

                //Updating the cyclone label contents
                await GameLogic.UpdateCycloneLabel(cycloneLabel, GameSettings);
            }

            //The turn is now over, it is not in progress
            GameLogic.TurnInProgress = false;
        }

        //Activates when any pit is selected by the player during their turn
        private async void btnPlayerClick(object sender, RoutedEventArgs e)
        {
            //If a turn is not in progress, start the turn
            if (!GameLogic.TurnInProgress)
            {
                //Starting the turn
                GameLogic.TurnInProgress = true;

                //Clearing the content of the eventLabel for the beginning of the turn
                GameLogic.ClearEventLabel(eventLabel);

                //For debugging; returns the value of CycloneMode & GoldenStone Mode
                //MessageBox.Show(GameSettings["CycloneCurrentlyActive"].ToString() + " " + GameSettings["GoldenMode"].ToString() + "\n 1 means enabled, 0 means disabled", "Cyclone Mode & Golden Mode");

                //Initializing values to be used in following methods
                Button btn = (Button)sender;
                int playerNum;
                int lastRowIndex;
                int colIndex;
                int lastColIndex;
                string winner;

                //empty variable to hold a score value; only used when CPU is deciding the optimal move
                int score = 0;

                //Assigns the values based on the button that was clicked
                playerNum = int.Parse(btn.Name.Split("_")[0].ToString().Replace("btnPlayer", ""));
                colIndex = int.Parse(btn.Name.Split("_")[1].ToString().Replace("Pit", ""));

                //check for golden stone here and then ask them where they want to place it.
                //check to see if selected pit has the golden stone, if so, ask the player where they want to place it
                if (GameSettings["GoldenMode"] == 1)
                {
                    int rowindex;

                    if(playerNum == 2) 
                    {
                        rowindex = 0;
                    } else
                    {
                        rowindex = 1;
                    }

                    //Checks if the selected pit has the golden stone
                    Dictionary<string, object> GoldenStonePit = CheckForGoldenStone(rowindex, colIndex, GameLogic.GameBoard.PitLocations);

                    //check if the bool is true, indicating that the golden stone is in the selected pit
                    if ((bool)GoldenStonePit["GoldenPit"] == true)
                    {
                        //Stores the value of the pit the golden stone landed in
                        int GoldenStoneLandingPit = await goldenStoneLandingPitModal((int)GoldenStonePit["colIndex"], (int)GoldenStonePit["StoneCount"]);
                        return;
                    }
                    else
                    {
                        //Continue the second part of the turn
                        continuePitSelected(99, colIndex);
                    }
                }
                else
                {
                    //Continue the second part of the turn
                    continuePitSelected(99, colIndex);
                }
            }
            
        }

        //Due to the async nature of the movement of stones, a turn is split into two segments
        private async void continuePitSelected(int GoldenStoneLandingPit, int colIndex)
        {
            //Initialized variables to hold values from the turn, which are used elsewhere
            int playerNum;
            int lastRowIndex;
            int lastColIndex;
            string winner;

            //Dummy variable to hold a score value; only used when CPU is deciding the optimal move
            int score = 0;

            //Disables all controls while the stones are about to be moved
            GameLogic.DisableAllControls();

            //Pass back the last rowIndex and colIndex to be used in all of the checks for the player
            (lastRowIndex, lastColIndex) = await GameLogic.GameBoard.Players[GameLogic.CurrentPlayer].PlayTurn(colIndex, GameLogic.CurrentPlayer, arryButtons, GameLogic.GameBoard.PitLocations, GameSettings, GoldenStoneLandingPit);

            //Run a method to see if the player can capture opponent stones based on the last placed stone location
            bool successfulCapture;

            (successfulCapture, score) = GameLogic.CheckForCapture(lastRowIndex, lastColIndex, GameLogic.GameBoard, GameSettings);

            //Display a successful capture in the event label
            GameLogic.DisplayEventLabelCapture(eventLabel, successfulCapture);

            //Run a method to see if the conditions have been met to end the game
            GameLogic.ValidateGameEnd(GameLogic.GameBoard);
            if (GameLogic.GameOver == true)
            {
                //Populate the winner string with the results of Show Winner
                winner = GameLogic.ShowWinner();

                //Display winner
                MessageBox.Show(winner);

                //Debug
                //DialogResult result = MessageBox.Show(winner, "Game Over", MessageBoxButtons.YesNo);
            }

            //changes player turn and appropriate controls if the game is not over
            if (!GameLogic.GameOver)
            {
                GameLogic.ChangePlayerTurn(GameLogic.CurrentPlayer, eventLabel, lastRowIndex, lastColIndex, GameSettings, GameLogic.GameBoard.PitLocations);
            }

            //Updating score labels for both players at the end of the turn
            GameLogic.UpdateScoreLabels(playerOneScoreValueLabel, playerTwoScoreValueLabel, GameLogic.GameBoard.PitLocations);

            //Updates the current player turn label
            GameLogic.UpdateTurnLabel(playerLabel, GameLogic.CurrentPlayer);

            //Updating the cyclone label contents
            await GameLogic.UpdateCycloneLabel(cycloneLabel, GameSettings);

            //Check to see if the CPU can go after the player, if so, allow the CPU to take it's turn
            while (GameLogic.GameBoard.Players[GameLogic.CurrentPlayer].IsCPU == true && GameLogic.GameOver == false)
            {
                //Clearing the content of the eventLabel for the beginning of the turn
                GameLogic.ClearEventLabel(eventLabel);

                //Plays the CPUs turn
                (lastRowIndex, lastColIndex) = await GameLogic.GameBoard.Players[GameLogic.CurrentPlayer].PlayTurn(GameLogic.CurrentPlayer, arryButtons,
                    GameLogic.GameBoard.PitLocations, PlayerAttributes, GameLogic.GameBoard, GameSettings);

                //Returns if the capture was succesful to update the label below
                (successfulCapture, score) = GameLogic.CheckForCapture(lastRowIndex, lastColIndex, GameLogic.GameBoard, GameSettings);

                //Display a successful capture in the event label
                GameLogic.DisplayEventLabelCapture(eventLabel, successfulCapture);

                //Run a method to see if the conditions have been met to end the game
                GameLogic.ValidateGameEnd(GameLogic.GameBoard);
                if (GameLogic.GameOver == true)
                {
                    //Populates the winner string with the results from Show Winner
                    winner = GameLogic.ShowWinner();

                    //Display winner
                    MessageBox.Show(winner);

                    //Debug
                    //DialogResult result = MessageBox.Show(winner, "Game Over", MessageBoxButtons.YesNo);
                }

                //changes player turn and controls if the game is not over
                if (!GameLogic.GameOver)
                {
                    GameLogic.ChangePlayerTurn(GameLogic.CurrentPlayer, eventLabel, lastRowIndex, lastColIndex, GameSettings, GameLogic.GameBoard.PitLocations);
                }

                //Updating score labels for both players at the end of the turn
                GameLogic.UpdateScoreLabels(playerOneScoreValueLabel, playerTwoScoreValueLabel, GameLogic.GameBoard.PitLocations);

                //Updates the current player turn label
                GameLogic.UpdateTurnLabel(playerLabel, GameLogic.CurrentPlayer);

                //Updating the cyclone label contents
                await GameLogic.UpdateCycloneLabel(cycloneLabel, GameSettings);
            }

            //The turn is no longer in progress, and is now over
            GameLogic.TurnInProgress = false;
        }

        //Method to exit the game window and return to the main window
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            //Closes the current window
            this.Close();
        }

        //Method to reset the game board, and any effects that may have been active at the time
        private void btnRestartGame_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            GameWindow gameWindow = new GameWindow(PlayerAttributes, GameSettings);
            gameWindow.Show();
        }

        //Method to arrange content appropriately based on how large the window is (for bigger board sizes)
        private void dynamicallySizeBoard(int pitCount)
        {
            //If the pit count is 8, set the following controls in these locations
            if (pitCount > 6)
            {
                this.Width = 1000;
                btnStore1.Margin = new Thickness(900, 140, 0, 0);
                playerOneHeaderLabel.Margin = new Thickness(233, 382, 0, 0);
                playerOneScoreValueLabel.Margin = new Thickness(270, 408, 0, 0);
                currentTurnHeadingLabel.Margin = new Thickness(429, 382, 0, 0);
                playerLabel.Margin = new Thickness(429, 416, 0, 0);
                playerTwoHeaderLabel.Margin = new Thickness(646, 382, 0, 0);
                playerTwoScoreValueLabel.Margin = new Thickness(682, 408, 0, 0);
                btnRestartGame.Margin = new Thickness(195, 520, 0, 0);
                btnExit.Margin = new Thickness(682, 520, 0, 0);
                btnGameRules.Margin = new Thickness(440, 520, 0, 0);
                eventLabel.Margin = new Thickness(631, 470, 0, 0);
                cycloneLabel.Margin = new Thickness(169, 470, 0, 0);
            }
            //If the pit count is 4, set the locations of the following controls
            else if (pitCount < 5)
            {
                btnStore1.Margin = new Thickness(625, 140, 0, 0);
                btnStore2.Margin = new Thickness(110, 140, 0, 0);
                btnPlayer2_Pit0.Margin = new Thickness(525, 64, 0, 0);
                btnPlayer2_Pit1.Margin = new Thickness(429, 64, 0, 0);
                btnPlayer2_Pit2.Margin = new Thickness(336, 64, 0, 0);
                btnPlayer2_Pit3.Margin = new Thickness(233, 64, 0, 0);
                btnPlayer1_Pit3.Margin = new Thickness(525, 278, 0, 0);
                btnPlayer1_Pit2.Margin = new Thickness(429, 278, 0, 0);
                btnPlayer1_Pit1.Margin = new Thickness(336, 278, 0, 0);
                btnPlayer1_Pit0.Margin = new Thickness(233, 278, 0, 0);
            }
            //If the pit count is 6 (default), locate the controls below appropriately
            else
            {
                //this.Width = 800;
                btnStore1.Margin = new Thickness(700, 140, 0, 0);
                btnPlayer2_Pit0.Margin = new Thickness(617, 64, 0, 0);
                btnPlayer2_Pit1.Margin = new Thickness(525, 64, 0, 0);
                btnPlayer2_Pit2.Margin = new Thickness(425, 64, 0, 0);
                btnPlayer2_Pit3.Margin = new Thickness(329, 64, 0, 0);
                btnPlayer2_Pit4.Margin = new Thickness(236, 64, 0, 0);
                btnPlayer2_Pit5.Margin = new Thickness(133, 64, 0, 0);
            }
        }

        //A method to assign buttons to a specific player, based on how many pits were selected on Main Window
        private Button[,] setButtons(Button[,] buttons)
        {
            //Variable storing how many buttons are in a row of the board
            int numButtons = buttons.GetLength(1) - 1;

            //Links the buttons in the button array to the controls on the GameWindow
            //Allows easy control of the content of each button (pit), such as displaying stone count
            arryButtons[0, 0] = btnPlayer2_Pit0;
            arryButtons[0, 1] = btnPlayer2_Pit1;
            arryButtons[1, 0] = btnPlayer1_Pit0;
            arryButtons[1, 1] = btnPlayer1_Pit1;

            //Assigns the buttons to each player, as well as if the button is visible or not
            if (numButtons > 2)
            {
                arryButtons[0, 2] = btnPlayer2_Pit2;
                arryButtons[1, 2] = btnPlayer1_Pit2;
            }
            else
            {
                btnPlayer2_Pit2.Visibility = Visibility.Collapsed;
                btnPlayer1_Pit2.Visibility = Visibility.Collapsed;
            }

            if (numButtons > 3)
            {
                arryButtons[0, 3] = btnPlayer2_Pit3;
                arryButtons[1, 3] = btnPlayer1_Pit3;
            }
            else
            {
                btnPlayer2_Pit3.Visibility = Visibility.Collapsed;
                btnPlayer1_Pit3.Visibility = Visibility.Collapsed;
            }

            if (numButtons > 4)
            {
                arryButtons[0, 4] = btnPlayer2_Pit4;
                arryButtons[1, 4] = btnPlayer1_Pit4;
            }
            else
            {
                btnPlayer2_Pit4.Visibility = Visibility.Collapsed;
                btnPlayer1_Pit4.Visibility = Visibility.Collapsed;
            }

            if (numButtons > 5)
            {
                arryButtons[0, 5] = btnPlayer2_Pit5;
                arryButtons[1, 5] = btnPlayer1_Pit5;
            }
            else
            {
                btnPlayer2_Pit5.Visibility = Visibility.Collapsed;
                btnPlayer1_Pit5.Visibility = Visibility.Collapsed;
            }

            if (numButtons > 6)
            {
                arryButtons[0, 6] = btnPlayer2_Pit6;
                arryButtons[1, 6] = btnPlayer1_Pit6;
            }
            else
            {
                btnPlayer2_Pit6.Visibility = Visibility.Collapsed;
                btnPlayer1_Pit6.Visibility = Visibility.Collapsed;
            }

            if (numButtons > 7)
            {
                arryButtons[0, 7] = btnPlayer2_Pit7;
                arryButtons[1, 7] = btnPlayer1_Pit7;
            }
            else
            {
                btnPlayer2_Pit7.Visibility = Visibility.Collapsed;
                btnPlayer1_Pit7.Visibility = Visibility.Collapsed;
            }


            //Sets up the store for each player, assigning yet more buttons in the button array
            arryButtons[1, numButtons] = btnStore1;
            arryButtons[0, numButtons] = btnStore2;

            return arryButtons;
        }

        //method to see if golden stone is in selected pit
        public Dictionary<string, object> CheckForGoldenStone(int rowIndex, int colIndex, Pit[,] PitLocations)
        {
            //Variable to hold how many stones were picked in a turn
            int pickedStones;
            
            //List to hold the stones that were picked
            List<Stone> PickedStones = new List<Stone>();

            //Dictionary object to hold information about the golden stone
            Dictionary<string, object> goldenStonePit = new Dictionary<string, object>()
            {
                { "GoldenPit", false },
                { "StoneCount", 0 },
                { "colIndex", 0 }
            };
            //count the number of stones in the pit. will be used for iterations below
            pickedStones = PitLocations[rowIndex, colIndex].Stones.Count();

            //for each stone, check if its the golden stone
            for (int i = 0; i < pickedStones; i++)
            {
                //Once true, set the information below
                if (PitLocations[rowIndex, colIndex].Stones[i].Color == "Golden")
                {
                    goldenStonePit["GoldenPit"] = true;
                    goldenStonePit["StoneCount"] = pickedStones;
                    goldenStonePit["colIndex"] = colIndex;
                }
            }

            return goldenStonePit;
        }

        //method to show modal to select golden stone landing pit
        private async Task <int> goldenStoneLandingPitModal(int colIndex, int StoneCount)
        {
            //Variable to hold where the golden stone will be selected to land
            int GoldentStoneLandingPit = 0;

            // Create the modal window & populate with information
            Window GoldenStoneDialog = new Window
            {
                Title = "Golden Stone Landing",
                Width = 300,
                Height = 600,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Create a StackPanel to hold the content
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };

            // Add a TextBlock to display the message
            TextBlock messageTextBlock = new TextBlock
            {
                Text = "Where would you like to drop the Golden Stone?",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10)
            };
            stackPanel.Children.Add(messageTextBlock);;

            //for each of the pits where the player will drop, create a button
            for (int i = 0; i < StoneCount; i++)
            {
                Button pitButton = new Button { Content = i+1 + " pits from starting pit", Tag = i, Width = 150, Margin = new Thickness(5) };
                pitButton.Click += (sender, e) =>
                {
                    GoldentStoneLandingPit = i; // Set DialogResult to true and close the dialog
                    GoldenStoneDialog.Close();
                    
                    // Call ContinuePitSelected after closing the dialog
                    continuePitSelected((int)pitButton.Tag, colIndex);
                };
                stackPanel.Children.Add(pitButton);
            };

            //if they close, send back an int to trigger the modal to reopen
            GoldenStoneDialog.Closing += (sender, e) =>
            {
                GoldentStoneLandingPit = 99;
            };

            GoldenStoneDialog.Content = stackPanel;
            GoldenStoneDialog.Show();

            // Show the dialog and return the result
            return GoldentStoneLandingPit;
        }

        //Method to display the rules of the game while still in the game window
        private void btnGameRules_Click(object sender, RoutedEventArgs e)
        {
            //Declaring and setting values of an about object
            About gameRules = new About();
                gameRules.Rules = "A standard game of mancala..." + "\n" + "Player 1 goes first, pick a pit. If it ends on your side, steal your opponents seeds. If it ends in your mancala, go again!"; 
                gameRules.CycloneMode = "\n\nCyclone Mode: The rules of the game of Mancala drastically change, where landing in your own store not only allows you to go again, but changes the direction of how stones move across the board. \n Each time a player is able to go again, Cyclone Mode will enable / disable based on it's current state.";
                gameRules.GoldenStoneMode = "\nGolden Stone Mode: In the random assortment of stones within each pit, there is a golden stone placed at random. You are allowed to decide where this stone is placed during your selection. \n If you place it in your own Mancala, you score an extra 5 points.";

            //Displaying the information on screen
            MessageBox.Show("Rules: " + gameRules.Rules + '\n' + gameRules.CycloneMode + '\n' + gameRules.GoldenStoneMode, "Game Rules");

        }
    }
}
