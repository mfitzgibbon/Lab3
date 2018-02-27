using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Example Yahtzee website if you've never played
// https://cardgames.io/yahtzee/

namespace Yahtzee
{
    public partial class yahtzeeForm : Form
    {
        public yahtzeeForm()
        {
            InitializeComponent();
        }

        // you may find these helpful in manipulating the scorecard and in other places in your code
        private const int NONE = -1;
        private const int ONES = 0;
        private const int TWOS = 1;
        private const int THREES = 2;
        private const int FOURS = 3;
        private const int FIVES = 4;
        private const int SIXES = 5;
        private const int THREE_OF_A_KIND = 6;
        private const int FOUR_OF_A_KIND = 7;
        private const int FULL_HOUSE = 8;
        private const int SMALL_STRAIGHT = 9;
        private const int LARGE_STRAIGHT = 10;
        private const int CHANCE = 11;
        private const int YAHTZEE = 12;

        private int rollCount = 0;
        private int uScoreCardCount = 0;

        // you'll need an instance variable for the user's scorecard - an array of 13 ints
        private int[] userScores = new int[13];
        // as well as an instance variable for 0 to 5 dice as the user rolls - array or list<int>?
        private List<int> userRolls = new List<int>();
        // as well as an instance variable for 0 to 5 dice that the user wants to keep - array or list<int>?
        private List<int> userKeeps = new List<int>();

        // this is the list of methods that I used

        // START WITH THESE 2
        // This method rolls numDie and puts the results in the list
        public void Roll(int numDie, List<int> dice)
        {
            Random rand = new Random();

            for(int count = 0; count < numDie; count++)
            {
                dice.Add(rand.Next(1, 7));
            }
        }

        // This method moves all of the rolled dice to the keep dice before scoring.  All of the dice that
        // are being scored have to be in the same list 
        public void MoveRollDiceToKeep(List<int> roll, List<int> keep)
        {

            foreach(int num in roll.ToList())
            {
                keep.Add(num);
                roll[roll.IndexOf(num)] = NONE;
                
            }
        }

        #region Scoring Methods
        /* This method returns the number of times the parameter value occurs in the list of dice.
         * Calling it with 5 and the following dice 2, 3, 5, 5, 6 returns 2.
         */
        private int Count(int value, List<int> dice)
        {
            int count = 0;

            foreach(int roll in dice)
            {
                if(roll == value)
                {
                    count++;
                }
            }

            return count;
        }

        /* This method counts how many 1s, 2s, 3s ... 6s there are in a list of ints that represent a set of dice
         * It takes a list of ints as it's parameter.  It should create an array of 6 integers to store the counts.
         * It should then call Count with a value of 1 and store the result in the first element of the array.
         * It should then repeat the process of calling Count with 2 - 6.
         * It returns the array of counts.
         * All of the rest of the scoring methods can be "easily" calculated using the array of counts.
         */
        private int[] GetCounts(List<int> dice)
        {
            int[] counts = new int[6];

            for (int i = 1; i <= 6; i++)
            {
                counts[i-1] = Count(i, dice);
            }

            return counts;
        }

        /* Each of these methods takes the array of counts as a parameter and returns the score for a dice value.
         */
        private int ScoreOnes(int[] counts)
        {
            int score = 0;

            score = counts[0] * 1;

            return score;
        }

        private int ScoreTwos(int[] counts)
        {
            int score = 0;

            score = counts[1] * 2;

            return score;
        }

        private int ScoreThrees(int[] counts)
        {
            int score = 0;

            score = counts[2] * 3;

            return score;
        }

        private int ScoreFours(int[] counts)
        {
            int score = 0;

            score = counts[3] * 4;

            return score;
        }

        private int ScoreFives(int[] counts)
        {
            int score = 0;

            score = counts[4] * 5;

            return score;
        }

        private int ScoreSixes(int[] counts)
        {
            int score = 0;

            score = counts[5] * 6;

            return score;
        }

        /* This method can be used to determine if you have 3 of a kind (or 4? or  5?).  The output parameter
         * whichValue tells you which dice value is 3 of a kind.
         */ 
        private bool HasCount(int howMany, int[] counts, out int whichValue)
        {
            int index = ONES;
            foreach (int count in counts)
            {
                if (howMany == count)
                {
                    whichValue = index;
                    return true;
                }
            }
            whichValue = NONE;
            return false;
        }

        /* This method returns the sum of the dice represented in the counts array.
         * The sum is the # of 1s * 1 + the # of 2s * 2 + the number of 3s * 3 etc
         */ 
        private int Sum(int[] counts)
        {
            int sum = 0;

            for(int i = 0; i < counts.Length; i++)
            {
                sum += counts[i] * (i + 1);
            }

            return sum;
        }

        /* This method calls HasCount(3...) and if there are 3 of a kind calls Sum to calculate the score.
         */ 
        private int ScoreThreeOfAKind(int[] counts)
        {
            int whichValue = NONE;

            if(HasCount(3, counts, out whichValue))
            {
                return Sum(counts);
            }
            return 0;
        }

        private int ScoreFourOfAKind(int[] counts)
        {
            int whichValue = NONE;

            if(HasCount(4, counts, out whichValue))
            {
                return Sum(counts);
            }

            return 0;
        }

        private int ScoreYahtzee(int[] counts)
        {
            int whichValue = NONE;

            if(HasCount(5, counts, out whichValue))
            {
                return 50;
            }

            return 0;
        }

        /* This method calls HasCount(2 and HasCount(3 to determine if there's a full house.  It calls sum to 
         * calculate the score.
         */ 
        private int ScoreFullHouse(int[] counts)
        {
            int whichValueTwo = NONE;
            int whichValueThree = NONE;

            if(HasCount(2, counts, out whichValueTwo) && HasCount(3, counts, out whichValueThree))
            {
                return 25;
            }

            return 0;
        }

        private int ScoreSmallStraight(int[] counts)
        {
            //viable die to achieve small straight
            //1234,2345,3456
            bool isSmallStraight = false;

            if (counts[0] > 0 && counts[1] > 0 && counts[2] > 0 && counts[3] > 0)
            {
                isSmallStraight = true;
            }
            else if (counts[1] > 0 && counts[2] > 0 && counts[3] > 0 && counts[4] > 0)
            {
                isSmallStraight = true;
            }
            else if (counts[2] > 0 && counts[3] > 0 && counts[4] > 0 && counts[5] > 0)
            {
                isSmallStraight = true;
            }

            if (isSmallStraight == true)
                return 30;
            else
                return 0;

        }

        private int ScoreLargeStraight(int[] counts)
        {
            bool isLargeStraight = false;

            if(counts[0] > 0 && counts[1] > 0 && counts[2] > 0 && counts[3] > 0 && counts[4] > 0)
            {
                isLargeStraight = true;
            }
            else if (counts[1] > 0 && counts[2] > 0 && counts[3] > 0 && counts[4] > 0 && counts[5] > 0)
            {
                isLargeStraight = true;
            }

            if (isLargeStraight)
                return 40;
            else
                return 0;
        }

        private int ScoreChance(int[] counts)
        {
            return Sum(counts);
        }

        /* This method makes it "easy" to call the "right" scoring method when you click on an element
         * in the user score card on the UI.
         */ 
        private int Score(int whichElement, List<int> dice)
        {
            int[] counts = GetCounts(dice);
            switch (whichElement)
            {
                case ONES:
                    return ScoreOnes(counts);
                case TWOS:
                    return ScoreTwos(counts);
                case THREES:
                    return ScoreThrees(counts);
                case FOURS:
                    return ScoreFours(counts);
                case FIVES:
                    return ScoreFives(counts);
                case SIXES:
                    return ScoreSixes(counts);
                case THREE_OF_A_KIND:
                    return ScoreThreeOfAKind(counts);
                case FOUR_OF_A_KIND:
                    return ScoreFourOfAKind(counts);
                case FULL_HOUSE:
                    return ScoreFullHouse(counts);
                case SMALL_STRAIGHT:
                    return ScoreSmallStraight(counts);
                case LARGE_STRAIGHT:
                    return ScoreLargeStraight(counts);
                case CHANCE:
                    return ScoreChance(counts);
                case YAHTZEE:
                    return ScoreYahtzee(counts);
                default:
                    return 0;
            }
        }
        #endregion

        // set each value to some negative number because 
        // a 0 or a positive number could be an actual score
        private void ResetScoreCard(int[] scoreCard, out int scoreCardCount)
        {
            for(int i = 0; i < scoreCard.Length; i++)
            {
                scoreCard[i] = NONE;
            }
            scoreCardCount = 0;
        }

        // this set has to do with user's scorecard UI
        private void ResetUserUIScoreCard()
        {
            Label userScoreLabel = new Label();
            ResetScoreCard(userScores, out uScoreCardCount);
            for(int i = 0; i < userScores.Length; i++)
            {
                userScoreLabel = GetUserScoreLabel(i);
                userScoreLabel.Text = " ";
                userScoreLabel.Enabled = true;
            }
            userSum.Text = " ";
            userBonus.Text = " ";
            userTotalScore.Text = " ";
        }

        // this method adds the subtotals as well as the bonus points when the user is done playing
        public void UpdateUserUIScoreCard()
        {
            int subtotal = userScores[ONES] + userScores[TWOS] + userScores[THREES] + userScores[FOURS]
                + userScores[FIVES] + userScores[SIXES];

            userSum.Text = subtotal.ToString();

            if (subtotal >= 63)
            {
                userBonus.Text = "35";
                subtotal += 35;
            }
            else
                userBonus.Text = "0";

            subtotal += userScores[THREE_OF_A_KIND] + userScores[FOUR_OF_A_KIND] + userScores[FULL_HOUSE] + userScores[SMALL_STRAIGHT]
                + userScores[LARGE_STRAIGHT] + userScores[CHANCE] + userScores[YAHTZEE];

            userTotalScore.Text = subtotal.ToString();
                
        }

        /* When I move a die from roll to keep, I put a -1 in the spot that the die used to be in.
         * This method gets rid of all of those -1s in the list.
         */
        private void CollapseDice(List<int> dice)
        {
            int numDice = dice.Count;
            for (int count = 0, i = 0; count < numDice; count++)
            {
                if (dice[i] == -1)
                    dice.RemoveAt(i);
                else
                    i++;
            }
        }

        /* When I move a die from roll to keep, I need to know which pb I can use.  It's the first spot with a -1 in it
         */
        public int GetFirstAvailablePB(List<int> dice)
        {
            return dice.IndexOf(-1);
        }

        #region UI Dice Methods
        /* These are all UI methods */
        private PictureBox GetKeepDie(int i)
        {
            PictureBox die = (PictureBox)this.Controls["keep" + i];
            return die;
        }

        private Label GetUserScoreLabel(int i)
        {
            Label scoreLabel = (Label)scoreCardPanel.Controls["user"+i];
            return scoreLabel;
        }

        public void HideKeepDie(int i)
        {
            GetKeepDie(i).Visible = false;
        }
        public void HideAllKeepDice()
        {
            for (int i = 0; i < 5; i++)
                HideKeepDie(i);
        }

        public void ShowKeepDie(int i)
        {
            PictureBox die = GetKeepDie(i);
            die.Image = Image.FromFile(System.Environment.CurrentDirectory + "\\..\\..\\Dice\\die" + userKeeps[i] + ".png");
            die.Visible = true;
        }

        public void ShowAllKeepDie()
        {
            for (int i = 0; i < 5; i++)
                ShowKeepDie(i);
        }

        private PictureBox GetComputerKeepDie(int i)
        {
            PictureBox die = (PictureBox)this.Controls["computerKeep" + i];
            return die;
        }

        public void HideComputerKeepDie(int i)
        {
            GetComputerKeepDie(i).Visible = false;
        }

        public void HideAllComputerKeepDice()
        {
            for (int i = 0; i < 5; i++)
                HideComputerKeepDie(i);
        }

        public void ShowComputerKeepDie(int i)
        {
            PictureBox die = GetComputerKeepDie(i);
            die.Image = Image.FromFile(System.Environment.CurrentDirectory + "\\..\\..\\Dice\\die" + userKeeps[i] + ".png");
            die.Visible = true;
        }

        public void ShowAllComputerKeepDie()
        {
            for (int i = 0; i < 5; i++)
                ShowComputerKeepDie(i);
        }

        private PictureBox GetRollDie(int i)
        {
            PictureBox die = (PictureBox)this.Controls["roll" + i];
            return die;
        }

        public void HideRollDie(int i)
        {
            GetRollDie(i).Visible = false;
        }

        public void HideAllRollDice()
        {
            for (int i = 0; i < 5; i++)
                HideRollDie(i);
        }

        public void ShowRollDie(int i)
        {
            PictureBox die = GetRollDie(i);
            die.Image = Image.FromFile(System.Environment.CurrentDirectory + "\\..\\..\\Dice\\die" + userRolls[i] + ".png");
            die.Visible = true;
        }

        public void ShowAllRollDie()
        {
            for (int i = 0; i < 5; i++)
                ShowRollDie(i);
        }
        #endregion

        #region Event Handlers
        private void Form1_Load(object sender, EventArgs e)
        {
            /* reset the user's scorecard
            * Hide the roll dice
            * Hide the keep dice
            * Hide the computer's dice
            */

            ResetScoreCard(userScores, out uScoreCardCount);
            HideAllRollDice();
            HideAllKeepDice();
            HideAllComputerKeepDice();
        }

        private void rollButton_Click(object sender, EventArgs e)
        {
            // DON'T WORRY ABOUT ROLLING MULTIPLE TIMES UNTIL YOU CAN SCORE ONE ROLL
            // hide all of the keep picture boxes
            // any of the die that were moved back and forth from roll to keep by the user
            // are "collapsed" in the keep data structure
            // show the keep dice again
            HideAllKeepDice();
            CollapseDice(userKeeps);
            for(int i = 0; i < userKeeps.Count; i++)
            {
                ShowKeepDie(i);
            }

            // START HERE
            // clear the roll data structure
            // hide all of thhe roll picture boxes
            userRolls.Clear();
            HideAllRollDice();


            // roll the right number of dice
            // show the roll picture boxes
            Roll(5 - userKeeps.Count(), userRolls);
            if(userKeeps.Count == 0)
            ShowAllRollDie();
            else
            {
                for(int i = 0; i < userRolls.Count; i++)
                {
                    ShowRollDie(i);
                }
            }

            // increment the number of rolls
            // disable the button if you've rolled 3 times
            
            rollCount++;
            if (rollCount == 3)
                rollButton.Enabled = false;
                
        }

        private void userScorecard_DoubleClick(object sender, EventArgs e)
        {
            // move any rolled die into the keep dice
            // hide picture boxes for both roll and keep
            MoveRollDiceToKeep(userRolls, userKeeps);
            HideAllKeepDice();
            HideAllRollDice();

            // determine which element in the score card was clicked
            // score that element
            // put the score in the scorecard and the UI
            // disable this element in the score card
            Label clickedLabel = (Label)sender;
            string labelName = clickedLabel.Name;
            int whichLabel = NONE;

            if (char.IsDigit(labelName[labelName.Length - 2]))
            {
                whichLabel = int.Parse(labelName.Substring(labelName.Length - 2));
            }
            else
            {
                whichLabel = (int)Char.GetNumericValue(labelName[labelName.Length - 1]);
            }


           int score = Score(whichLabel, userKeeps);
            userScores[whichLabel] = score;
            clickedLabel.Text = score.ToString();
            clickedLabel.Enabled = false;

            // clear the keep dice
            // reset the roll count
            // increment the number of elements in the score card that are full
            // enable/disable buttons
            userKeeps.Clear();
            rollCount = 0;
            rollButton.Enabled = true;
            uScoreCardCount++;

            // when it's the end of the game
            // update the sum(s) and bonus parts of the score card
            // enable/disable buttons
            // display a message box?
            if(uScoreCardCount == userScores.Length)
            {
                UpdateUserUIScoreCard();
                rollButton.Enabled = false;
                //MessageBox.Show()
            }
            
        }

        private void roll_DoubleClick(object sender, EventArgs e)
        {
            // figure out which die you clicked on
            PictureBox clickedDie = (PictureBox)sender;

            // figure out where in the set of keep picture boxes there's a "space"
            // move the roll die value from this die to the keep data structure in the "right place"
            // sometimes that will be at the end but if the user is moving dice back and forth
            // it may be in the middle somewhere
            int openPB = GetFirstAvailablePB(userKeeps);
            string labelName = clickedDie.Name;
            int rollDieIndex = (int)Char.GetNumericValue(labelName[labelName.Length - 1]);
            int keepDieIndex = -1;
            int rollValue = userRolls[rollDieIndex];

            if (openPB != NONE)
            {
                userKeeps[openPB] = rollValue;
                keepDieIndex = openPB;
            }
            else
            {
                userKeeps.Add(rollValue);
                keepDieIndex = userKeeps.LastIndexOf(rollValue);
            }

            // clear the die in the roll data structure
            // hide the picture box
            userRolls[rollDieIndex] = NONE;
            clickedDie.Visible = false;
            ShowKeepDie(keepDieIndex);
        }

        private void keep_DoubleClick(object sender, EventArgs e)
        {
            // figure out which die you clicked on
            PictureBox clickedDie = (PictureBox)sender;

            // figure out where in the set of roll picture boxes there's a "space"
            // move the roll die value from this die to the roll data structure in the "right place"
            // sometimes that will be at the end but if the user is moving dice back and forth
            // it may be in the middle somewhere
            int openPB = GetFirstAvailablePB(userRolls);
            string labelName = clickedDie.Name;
            int keepDieIndex = (int)Char.GetNumericValue(labelName[labelName.Length - 1]);
            int rollDieIndex = -1;
            int rollValue = userKeeps[keepDieIndex];

            if (openPB != NONE)
            {
                userRolls[openPB] = rollValue;
                rollDieIndex = openPB;
            }
            else
            {
                userRolls.Add(rollValue);
                rollDieIndex = userRolls.LastIndexOf(rollValue);
            }


            // clear the die in the keep data structure
            // hide the picture box
            userKeeps[keepDieIndex] = NONE;
            clickedDie.Visible = false;
            ShowRollDie(rollDieIndex);
        }

        private void newGameButton_Click(object sender, EventArgs e)
        {
            //reset the user score card
            //hide both sets of dice
            //reset both sets of dice data structures
            //Enable Roll button

            ResetUserUIScoreCard();
            HideAllKeepDice();
            HideAllRollDice();
            userKeeps.Clear();
            userRolls.Clear();
            rollButton.Enabled = true;
        }
        #endregion
    }
}
