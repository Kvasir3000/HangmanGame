using System;
using System.Collections.Generic;
using System.Linq;

namespace GuessTheWord
{
    class WordFamily
    {
        private string[] family;
        private int wordsLength;

        private char[] root;
        private LinkedList<string> words;
        private LinkedList<WordFamily> fListWithInput; 
        private double avgUniqueCharacters;  

        public WordFamily()
        {
            string[] dictionary = System.IO.File.ReadAllText("dictionary.txt").Split('\n');
            Random random = new Random();
            while (family == null || family.Length == 0)
            {
                wordsLength = random.Next(5, 13);
                family = Array.FindAll(dictionary, word => word.Length == wordsLength);
            }
            wordsLength = family[0].Length - 1;
            root = Enumerable.Repeat('_', wordsLength).ToArray();
        }

        private WordFamily(char[] root)
        {
            this.root = root;
            words = new LinkedList<string>();
        }

        public void Update(DifficultyLevel difficulty, string input)
        {
            if (difficulty == DifficultyLevel.EASY)
            {
                UpdateEasyLevel(input);
            }
            else if (difficulty == DifficultyLevel.HARD)
            {
                UpdateHardLevel(input);
            }
        }

        // ----------------------------------------------------------------------------------------------------------
        //                                              EASY LEVEL  
        // ----------------------------------------------------------------------------------------------------------
        private void UpdateEasyLevel(string input)
        {
            input = input.ToLower();
            string[] fWithInput = Array.FindAll(family, word => word.Contains(input));
            int lengthOfFamilyWithoutInput = family.Length - fWithInput.Length;
            if (lengthOfFamilyWithoutInput >= fWithInput.Length)
            {
                family = family.Where(word => !word.Contains(input)).ToArray();
            }
            else
            {
                GenerateFamiliesWithInput(fWithInput, input);
                string[] largestFamilyWithInput = GetLargestFamily();
                if (largestFamilyWithInput.Length > lengthOfFamilyWithoutInput)
                {
                    family = largestFamilyWithInput;
                    UpdateRoot(input.ToCharArray()[0]);
                    fListWithInput.Clear();
                }
                else
                {
                    family = family.Where(word => !word.Contains(input)).ToArray();
                }
            }
            avgUniqueCharacters = CalculateAvgUniqueCharacters(family);
        }

        private string[] GetLargestFamily()
        {
            int largestsFamilySize = 0;
            int largestFamilyIndex = 0;
            for (int i = 0; i < fListWithInput.Count(); i++)
            {
                if (fListWithInput.ElementAt(i).words.Count() > largestsFamilySize)
                {
                    largestsFamilySize = fListWithInput.ElementAt(i).words.Count();
                    largestFamilyIndex = i;
                }
            }
            return fListWithInput.ElementAt(largestFamilyIndex).words.ToArray();
        }

        // ----------------------------------------------------------------------------------------------------------

        // ----------------------------------------------------------------------------------------------------------
        //                                              HARD LEVEL  
        // ----------------------------------------------------------------------------------------------------------


        // This method will check the distrubution of letters 
        private void UpdateHardLevel(string input)
        {
            input = input.ToLower();

            string[] fWithoutInput = Array.FindAll(family, word => !word.Contains(input));
            string[] fWithInput = Array.FindAll(family, word => word.Contains(input));
            GenerateFamiliesWithInput(fWithInput, input);

            double avgUniqueCharFWithoutInput = CalculateAvgUniqueCharacters(fWithoutInput);
            double[] avgUniqueCharFWithInput = CalculateAvgUniqueCharacters();

            int fWithInputIdx = SelectFamily(fWithoutInput, avgUniqueCharFWithoutInput, avgUniqueCharFWithInput);

            if (fWithInputIdx != -1)
            {
                family = fListWithInput.ElementAt(fWithInputIdx).words.ToArray();
                UpdateRoot(input.ToCharArray()[0]);
                avgUniqueCharacters = CalculateAvgUniqueCharacters(family);
            }
            else
            {
                family = fWithoutInput;
                avgUniqueCharacters = avgUniqueCharFWithoutInput;
            }
        }

        private double[] CalculateAvgUniqueCharacters()
        {
            double[] avgUniqueCharacters = new double[fListWithInput.Count()];
            for (int i = 0; i < fListWithInput.Count(); i++)
            {
                double uniqueCharacters = 0.0;

                for (int j = 0; j < fListWithInput.ElementAt(i).words.Count(); j++)
                {
                    var wordUniqueCharacters = fListWithInput.ElementAt(i).words.ElementAt(j).Distinct().ToArray();

                    wordUniqueCharacters = wordUniqueCharacters.Where(character => !root.Contains(character)).ToArray();
                    uniqueCharacters += wordUniqueCharacters.Length - 1; // 1 is substracted, because of the \r character 
                }
                avgUniqueCharacters[i] = (uniqueCharacters / fListWithInput.ElementAt(i).words.Count());
            }
            return avgUniqueCharacters;
        }

        // Returns the index of the chosen family
        // IF index is -1, the family without input character is selected
        private int SelectFamily(string[] fWithoutInput, double avgUniqueCharactersF1, double[] avgUniqueCharactersF2)      
        {
            double bestCoeficient = 0;
            int idx = 0;
            for(int i = 0; i < fListWithInput.Count(); i++)
            {
                double coeficient = GetCoefficient(fListWithInput.ElementAt(i).words.Count(), avgUniqueCharactersF2[i]);
                if (coeficient > bestCoeficient)
                {
                    bestCoeficient = coeficient;
                    idx = i;
                }
            }

            double coefficientForFamilyWithoutInput = GetCoefficient(fWithoutInput.Length, avgUniqueCharactersF1);
            return (coefficientForFamilyWithoutInput >= bestCoeficient) ?  -1 :  idx; 
        }

        // This coefficient is used to choose the family
        private double GetCoefficient(int sizeOfFamily, double avgUniqueCharacters)
        {
            return (double)(sizeOfFamily * 0.6 + avgUniqueCharacters * 0.4) / 2.0;
        }
        // ----------------------------------------------------------------------------------------------------------




       private void GenerateFamiliesWithInput(string[] family, string input)
        {
            fListWithInput = new LinkedList<WordFamily>();
            char[] root;
            for (int i = 0; i < family.Length; i++)
            {
                root = GenerateRoot(family[i], input);
                int familyWithRootIdx = FindFamilyIdx(root);
                if (familyWithRootIdx == -1) // if there is no family with the same root 
                {
                    fListWithInput.AddLast(new WordFamily(root));
                    fListWithInput.Last().words.AddLast(family[i]);
                }
                else
                {
                    fListWithInput.ElementAt(familyWithRootIdx).words.AddLast(family[i]);
                }
            }
        }

        // Root of the word is based on the position of the input character inside the word
        // E.g: word = 'dog', input character = 'o' => root = ' o ' (d and g are replaced by empty spaces)
        // E.g word = 'Palantir' , input character = 'a' => root = ' a a    '
        private char[] GenerateRoot(string word, string input)
        {
            char[] root = new char[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == input.ToCharArray()[0])
                {
                    root[i] = input.ToCharArray()[0];
                }
                else
                {
                    root[i] = ' ';
                }
            }
            return root;
        }


        // Returns the index of the family in the fWithInputList which has the same root as in parametre 
        // If there is no family with the same root -1 is returned
        private int FindFamilyIdx(char[] root)
        {
            for (int i = 0; i < fListWithInput.Count(); i++)
            {
                if (fListWithInput.ElementAt(i).root.SequenceEqual(root))
                {
                    return i;
                }
            }
            return -1;
        }

        // Updates root for the whole family, when user guesses the character
        private void UpdateRoot(char input)
        {
            for (int i = 0; i < wordsLength; i++)
            {
                if (family.ElementAt(0)[i] == input)
                {
                    root[i] = input;
                }
            }
        }

        private double CalculateAvgUniqueCharacters(string[] family)
        {
            double avgUniqueCharacters = 0.0;
            for (int i = 0; i < family.Length; i++)
            {
                var wordUniqueCharacters = family[i].Distinct().ToArray();
                wordUniqueCharacters = wordUniqueCharacters.Where(character => !root.Contains(character)).ToArray();
                avgUniqueCharacters += wordUniqueCharacters.Count() - 1; // 1 is substracted, because of the \r character 
            }
            return avgUniqueCharacters / family.Length;
        }

        // This function checks if the player guessed the word
        public bool WordIsMatched()
        {
            var unguessedCharacters = root.Where(character => character == '_');
            return (unguessedCharacters.Count() > 0) ? false : true;
        }

        public int GetLength() { return wordsLength; }
        public string[] GetFamily() { return family; }
        public char[] GetRoot() { return root; }
        public double GetAvgUniqueChar() { return avgUniqueCharacters; }
    }
}