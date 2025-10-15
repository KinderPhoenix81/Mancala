using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Game_Mancala
{
    public class Board
    {
        //Constructor for a board object
        public Board(Dictionary<string, string?> PlayerAttributes, bool temp, Dictionary<string, int> GmeSettings)
        {
            //Fields of the players of the board
            bool Player1IsCPU;
            bool Player2IsCPU;

            //Checks to see if player 1 is a CPU based on Main Window input, assigns values
            if (PlayerAttributes["Player1Type"] == "Human")
            {
                Player1IsCPU = false;
            }
            else
            {
                Player1IsCPU = true;
            }

            //Checks to see if player 2 is a CPU based on Main Window input, assigns values
            if (PlayerAttributes["Player2Type"] == "Human")
            {
                Player2IsCPU = false;
            }
            else
            {
                Player2IsCPU = true;
            }

            //Player fields of the board
            player1 = new Player(PlayerAttributes["Player1Name"], Player1IsCPU, PlayerAttributes["Player1Difficulty"]);
            player2 = new Player(PlayerAttributes["Player2Name"], Player2IsCPU, PlayerAttributes["Player2Difficulty"]);

            //Declaring the arrays of PitLocations and ArrayButtons
            PitLocations = new Pit[2, 7];
            arryButtons = new Button[2, 7];

            //Declaring a list of players
            Players = [player1, player2];

            //A boolean to hold if the board is a temporary board (CPU Logic)
            tempBoard = temp;

            //Logic to determine if the golden stone should be present on the board based on decisions on the main screen
            if (GmeSettings["GoldenMode"] == 0)
            {
                goldenStoneMode = false;
            }
            else
            {
                goldenStoneMode = true;
            }

        }
        
        //Additional constructor for the board, taking in a game canvas for drawing purposes
        public Board(Dictionary<string, string?> PlayerAttributes, bool temp, Dictionary<string, int> GmeSettings, Canvas gmeCanvas) 
        {
            //Fields of the players on the board
            bool Player1IsCPU;
            bool Player2IsCPU;

            //Setting the above fields based on input on Main Window
            if (PlayerAttributes["Player1Type"] == "Human")
            {
                Player1IsCPU = false;
            } else {
                Player1IsCPU = true;
            }

            if (PlayerAttributes["Player2Type"] == "Human")
            {
                Player2IsCPU = false;
            }
            else
            {
                Player2IsCPU = true;
            }

            //Two player objects for the board
            player1 = new Player(PlayerAttributes["Player1Name"], Player1IsCPU, PlayerAttributes["Player1Difficulty"], gmeCanvas);
            player2 = new Player(PlayerAttributes["Player2Name"], Player2IsCPU, PlayerAttributes["Player2Difficulty"], gmeCanvas);

            //Declaring arrays like PitLocations and ArrayButtons for the game board
            PitLocations = new Pit[2, 7];
            arryButtons = new Button[2, 7];

            //A list of players
            Players = [player1, player2];

            //A boolean to hold if the board is a temporary board (CPU logic)
            tempBoard = temp;

            //Logic to determine if the golden stone should be placed on the board
            if (GmeSettings["GoldenMode"] == 0)
            {
                goldenStoneMode = false;
            }
            else
            {
                goldenStoneMode = true;
            }

        }

        //Declaring two players that will use the board to play the game
        public Player player1;
        public Player player2;

        //Declaring a list of players
        public Player[] Players;

        //A boolean to hold if the board is temporary (CPU logic)
        public bool tempBoard;

        //Sets up the 2D array for the game to take place, containing pits and stores
        public Pit[,] PitLocations;
        public Button[,] arryButtons;

        //boolean to determine if golden stone mode is on or not
        public bool goldenStoneMode;

        //After options from MainWindow -  game initialization. Sets up the pits with stones
        public void PitSetup(int size, Canvas gameCanvas, Array arryButtons, Dictionary<string, int> gameSettings)
        {
            //A property of each stone that will increment as stones are looped through during the setup of pits
            int stoneID = 0;

            //set number of stones
            int totalStoneCount;

            //pass in dictionary values to calculate total stone count in the header 
            totalStoneCount = gameSettings["StoneCount"] * gameSettings["PitCount"];

            //new variable to get an id for the golden stone, since total stone count is no longer the count of total stones?!?
            int goldenStoneIDCount = totalStoneCount * 2;

            //stone color variables
            string stoneColor;
            Random random = new Random();
            int goldenStoneID = random.Next(goldenStoneIDCount);

            //Debug
            //int goldenStoneID = 2;

            //stone point value variable
            int stonePointValue;

            //A list that holds the potential colors of the stones
            List<string> stoneColors = new List<string>() { "Red", "Blue", "Green", "Purple" };

            //initializing the indexes of PitLocations as new Pit lists
            //loops through each row setting the column to a new pit object
            //PitLocations will be an array of [2,7] to hold all of the pits
            for (int rowIndex = 0; rowIndex < 2; rowIndex++)
            {
                //loop to set the array indicies as Pit objects
                //set for number of pits from gamesettings
                for (int colIndex = 0; colIndex <= size; colIndex++)
                {
                    PitLocations[rowIndex, colIndex] = new Pit();
                    PitLocations[rowIndex, colIndex].colIndex = colIndex;
                    PitLocations[rowIndex, colIndex].rowIndex = rowIndex;
                }
            }

            //adding stones to each Pit object in the PitLocations array
            for (int rowIndex = 0; rowIndex < 2; rowIndex++)
            {
                //this needs to be repeated twice, once for each row of the 2d PitLocations array
                for (int colIndex = 0; colIndex < size; colIndex++)
                {
                    //changing size for dictionary key to search for
                    for (int StoneCount  = 0; StoneCount < gameSettings["StoneCount"]; StoneCount++)
                    {
                        //set stone color, setting one stone to golden, if goldenMode is true
                        if (goldenStoneMode == true && stoneID == goldenStoneID)
                        {
                            stoneColor = "Golden";
                            stonePointValue = 5;
                        }
                        else
                        {
                            //Sets the color of the stone to a random color in the list of possible colors
                            int randomIndex = random.Next(stoneColors.Count);
                            stoneColor = stoneColors[randomIndex];
                            stonePointValue = 1;
                        }

                        //set stoneID based on the calculation above
                        Stone stone = new Stone(stoneID, stoneColor, stonePointValue);
                        stoneID++;

                        //do the actual work of adding the stone to the PitLocations index
                        PitLocations[rowIndex, colIndex].Stones.Add(stone);

                        //add visual representations of stones, on top of current button
                        Button curButton = FindButton(rowIndex, colIndex, arryButtons);
                        CreateVisualStone(stoneColor, gameCanvas, curButton);

                    }
                }
            }
        }

        //Pulling the current button out of the array of buttons, to have stones drawn in later
        public Button FindButton(int rowIndex, int colIndex, Array arrayButtons)
        {
            object goldenButtonObject = arrayButtons.GetValue(rowIndex, colIndex);
            Button goldenButton = (Button)goldenButtonObject;
            
            return goldenButton;
        }

        //Draws a stone in a pit, given the color of the stone
        public void CreateVisualStone(string stoneColor, Canvas gameCanvas, Button curButton)
        {
            //Creates a base stone "object" on the board, with no color or position yet
            Ellipse ellipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            //set fill for stones based on what color was selected before
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

            //Creates random values for the stone to use when placed into the pit (button)
            Random random = new Random();
            int xPositionModifier = random.Next(50, 95);
            int yPositionModifer = random.Next(50, 95);

            //Sets the position of the stone in a button randomly
            Canvas.SetLeft(ellipse, curButton.ActualWidth + ellipse.Width - xPositionModifier);
            Canvas.SetTop(ellipse, curButton.ActualHeight + ellipse.Width - yPositionModifer);
        }
    }
}
