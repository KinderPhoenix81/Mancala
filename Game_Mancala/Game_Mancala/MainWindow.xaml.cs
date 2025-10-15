using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Game_Mancala
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        //Fields for information regarding player 1
        public required string Player1Type;
        public required string Player1Difficulty;
        public required string Player1Name;

        //Fields for information regarding player 2
        public required string Player2Type;
        public required string Player2Difficulty;
        public required string Player2Name;

        //A method that activates a window containing information about the game
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            //Creating a new about object
            About about = new About();

            //Defining the rules of the game
            about.Rules = "Player 1 goes first, pick a pit. If it ends on your side, steal your opponents seeds. If it ends in your store, go again!";

            //Defining how cyclone mode works
            about.CycloneMode = "\n\nCyclone Mode: Cyclone Mode changes the game of Mancala drastically, where landing in your own store not only allows you to go again, but changes the direction of how stones move across the board. \nEach time a player is able to go again, Cyclone Mode will enable or disable based on it's current state.";

            //Defining how the golden stone works
            about.GoldenStoneMode = "\n\nGolden Stone Mode: In the random assortment of stones within each pit, there is a golden stone placed at random. You are allowed to decide where this stone is placed during your selection. If you place it in your own Mancala, you score an extra 5 points. \n\n";

            //The version of the game
            about.Version = "1.0 \n";

            //The developers of the game
            about.Developers = "Mike Lincks, Alex Allen, Blake Hall, Brooke Metoxen-Smith";

            //Concatenating all of the information above to create the about screen
            MessageBox.Show("Rules: " + about.Rules + about.CycloneMode + about.GoldenStoneMode + "Version: " + about.Version + "\nDeveloped By: " + about.Developers, "About" );
        }

        //A method to close out of the program
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //A method that creates a new game window, and brings over all necessary information from the Main Window
        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            //The dictionary object that will hold the game settings the user can choose from
            Dictionary<string, int> GameSettings = defineGameSettings();

            //get inputs from main window to set play attributes 
            string Player1Name;
            string Player2Name;

            //A list of combo box items, allowing the player to choose a type (Human / CPU) and difficulty (Easy & Difficult) for the CPU
            ComboBoxItem Player1Type = (ComboBoxItem)cbPlayer1Type.SelectedItem;
            string? Player1TypeValue = Player1Type.Content.ToString();
            ComboBoxItem Player2Type = (ComboBoxItem)cbPlayer2Type.SelectedItem;
            string? Player2TypeValue = Player2Type.Content.ToString();

            ComboBoxItem Player1Difficulty = (ComboBoxItem)cbPlayer1Difficulty.SelectedItem;
            string? Player1DifficultyValue;
            ComboBoxItem Player2Difficulty = (ComboBoxItem)cbPlayer2Difficulty.SelectedItem;
            string? Player2DifficultyValue;

            //If player one is human, require them to enter a name, and set it will set difficulty
            if (Player1TypeValue == "Human")
            {
                if(string.IsNullOrWhiteSpace(tbPlayer1Name.Text))
                {
                    
                    MessageBox.Show("Please Enter a Name For Player 1.");
                    return;
                } 
                else
                {
                    Player1Name = tbPlayer1Name.Text;
                    Player1DifficultyValue = "Human";
                }
            }
            else
            {
                //If the player is a CPU, use the information on the form
                Player1Name = tbPlayer1Name.Text;
                Player1DifficultyValue = Player1Difficulty.Content.ToString();
            }

            //If the player two is human require them to enter a name, and it will set difficulty
            if (Player2TypeValue == "Human")
            {
                if (!string.IsNullOrWhiteSpace(tbPlayer2Name.Text))
                {
                    Player2DifficultyValue = "Human";
                    Player2Name = tbPlayer2Name.Text;
                }
                else
                {
                    MessageBox.Show("Please Enter a Name For Player 2.");
                    return;
                }
            }
            else
            {
                //If the player is a CPU, use the information on the form
                Player2Name = tbPlayer2Name.Text;
                Player2DifficultyValue = Player2Difficulty.Content.ToString();
            }

            //create dictionary and add player attributes
            Dictionary<string, string> PlayerAttributes = new Dictionary<string, string>();
            PlayerAttributes.Add("Player1Name", Player1Name);
            PlayerAttributes.Add("Player2Name", Player2Name);
            PlayerAttributes.Add("Player1Type", Player1TypeValue);
            PlayerAttributes.Add("Player2Type", Player2TypeValue);
            PlayerAttributes.Add("Player1Difficulty", Player1DifficultyValue);
            PlayerAttributes.Add("Player2Difficulty", Player2DifficultyValue);

            //Creates a new game window to host the mancala game
            GameWindow gameWindow = new GameWindow(PlayerAttributes, GameSettings);
            gameWindow.Show();
        }

        //Dictionary<string, int> GameSettings for degree of difficulty as a variety for how-to-play game
        private Dictionary<string, int> defineGameSettings()
        {
            //list of options for user defined pit counts during game action
            ComboBoxItem PitCount = (ComboBoxItem)cbPitCount.SelectedItem;
            //list of options for user defined stone count for each pit during game action
            ComboBoxItem StoneCount = (ComboBoxItem)cbStoneCount.SelectedItem;

            //game options to enable using int for access in GameSettings Dictionary<str, int>
            //str
            string pitCountString = PitCount.Content.ToString();
            string stoneCountString = StoneCount.Content.ToString();
            //int
            int pitCountInt = int.Parse(pitCountString);
            int stoneCountInt = int.Parse(stoneCountString);
            int CycloneMode = 0;
            int GoldenMode = 0;

            //instantiating GameSettings to enable CycloneMode/GoldenMode to be active/inactive during game action
            if (radCycloneOn.IsChecked == true)
            {
                CycloneMode = 1;
            }

            if (radGoldenOn.IsChecked == true)
            {
                GoldenMode = 1;
            }

            //Game Settings Dictionary
            Dictionary<string, int> GameSettings = new Dictionary<string, int>();
            GameSettings.Add("PitCount", pitCountInt);
            GameSettings.Add("StoneCount", stoneCountInt);
            GameSettings.Add("CycloneMode", CycloneMode);
            GameSettings.Add("CycloneCurrentlyActive", 0);
            GameSettings.Add("GoldenMode", GoldenMode);


            //returns to PitSetup in board.cs (line 120), 
            return GameSettings;

        }

        //A method to see if the player type has changed from CPU to human, or vice versa
        private void playerTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            //return which player combobox changed
            ComboBox comboBox = (ComboBox)sender;

            //get the selection(s)
            ComboBoxItem SelectedItem = (ComboBoxItem)comboBox.SelectedItem;

            //if the value = "CPU" enable the difficulty combobox and have a selection selected
            if(SelectedItem.Content.ToString() == "CPU")
            {
                //CPU comboBox selections are used; field settings initialized for CPU PlayerType and Difficulty 
                if (comboBox == cbPlayer1Type)
                {
                    //Sets information for player 1
                    tbPlayer1Name.Text = "Frankie";
                    tbPlayer1Name.IsEnabled = false;
                    cbPlayer1Difficulty.IsEnabled = true;
                    //CPU setting to always begin as player 2
                    cbPlayer1Difficulty.SelectedIndex = 0;
                } 
                else
                {
                    //Sets information for player 2
                    tbPlayer2Name.Text = "Eugene";
                    tbPlayer2Name.IsEnabled = false;
                    cbPlayer2Difficulty.IsEnabled = true;
                    cbPlayer2Difficulty.SelectedIndex = 0;
                }

            } 
            else
            {
                //Human comboBox selections are initialized to index[0] (board location for player store, pit row)
                if (comboBox == cbPlayer1Type && cbPlayer1Difficulty != null)
                {
                    //Human input for player 1
                    tbPlayer1Name.IsEnabled = true;
                    tbPlayer1Name.Text = "";
                    cbPlayer1Difficulty.IsEnabled = false;
                    //Human setting to always begin as player 1
                    cbPlayer1Difficulty.SelectedIndex = -1;
                }
                else if(comboBox == cbPlayer2Type && cbPlayer2Difficulty != null)
                {
                    //Human input for player 2
                    tbPlayer2Name.IsEnabled = true;
                    tbPlayer2Name.Text = "";
                    cbPlayer2Difficulty.IsEnabled = false;
                    cbPlayer2Difficulty.SelectedIndex = -1;
                }
            }
        }
    }
}