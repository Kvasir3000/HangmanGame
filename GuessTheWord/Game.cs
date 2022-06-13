using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessTheWord
{
   public enum DifficultyLevel
   {
        EASY,
        HARD,
        VERY_HARD
   }

    class Game
    {
        private string[] availableCharacters;
        private WordFamily family;
        private bool endGame = false;
        private int numberOfGuesses;
        private DifficultyLevel difficulty;
        private bool programIsRunning = true;
   
        void ResetGame()
        {
            availableCharacters = new string[] {"A", "B", "C","D", "E", "F", "G", "H",
                                               "I", "J", "K", "L", "M", "N","O", "P",
                                               "Q", "R", "S", "T", "U", "V", "W", "X",
                                               "Y", "Z" };
            endGame = false;
            family = new WordFamily();
            numberOfGuesses = family.GetLength() * 2;
        }

        private void ShowMainMenu()
        {
            bool levelIsChosen = false;
            while (!levelIsChosen & programIsRunning)
            {
                Console.WriteLine("Choose the level of difficulty:\n1.Easy\n2.Hard\n3.Close game");
                Console.Write("\n:> ");
                string input = Console.ReadLine();
                SetLevelOfDifficulty(input, ref levelIsChosen);
            }
            if (levelIsChosen)
            {
                ResetGame();
                StartGameLoop();
            }
        }

        private void SetLevelOfDifficulty(string input, ref bool levelIsChosen)
        {
            switch (input)
            {
                case "1":
                    difficulty = DifficultyLevel.EASY;
                    levelIsChosen = true;
                    break;
                case "2":
                    difficulty = DifficultyLevel.HARD;
                    levelIsChosen = true;
                    break;
                case "3":
                    programIsRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid input, try again");
                    Console.ReadLine();
                    Console.Clear();
                    break;
            }
        }

        private void ShowGameScreen()
        {
            Console.Clear();
            Console.WriteLine("Number of guesses: " + numberOfGuesses + "          Family size: " + family.GetFamily().Length  + 
                              "          Average unique characters in family: " + Math.Round(family.GetAvgUniqueChar(), 2) + "\n");
            var root = family.GetRoot();
            for (int i = 0; i < root.Length; i++)
            {
                Console.Write(root[i]);
                Console.Write(' ');
            }
            Console.WriteLine("\n\nChoose one of the letters below or type 0 to see all words:");
            foreach (var character in availableCharacters) Console.Write(character + " ");
            Console.Write("\n:> ");
        }

        private void ProcessInput()
        {
            string input = Console.ReadLine().ToUpper();
            if (input == "0")
            {
                Console.WriteLine("\nFamily words: ");
                foreach (string el in family.GetFamily()) Console.WriteLine(el);
                Console.WriteLine("Press Enter to continue");
                Console.Write(":> ");
                Console.ReadLine();
            }
            else if (input != "_" && availableCharacters.Contains(input))
            {
                availableCharacters[Array.IndexOf(availableCharacters, input)] = "_";
                family.Update(difficulty, input);
                numberOfGuesses--;
            }
            else
            {
                Console.WriteLine("Invalid value, please try again\n");
                Console.ReadLine();
                Console.Clear();
            }
        }

        private void UpdateEndGame()
        {
            endGame = family.WordIsMatched();
            if (endGame)
            {
                Console.Clear();
                Console.Write("You won :) \nPress Enter to return to the menu\n:> ");
                Console.ReadLine();
            }
            else if (numberOfGuesses == 0)
            {
                endGame = true;
                Console.Clear();
                Console.Write("You lost :( \nPress Enter to return to the menu\n:> ");
                Console.ReadLine();
            }
        }

        private void StartGameLoop()
        {
            while (!endGame)
            {
                ShowGameScreen();
                ProcessInput();
                UpdateEndGame();
            }
        }

        public void StartLoop()
        {
            while (programIsRunning)
            {
                ShowMainMenu();
                Console.Clear();
            }
        }
    }
}
