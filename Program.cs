using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace wordlesolver
{
    
    static internal class Solver
    {
        private const int Suggest = 10;
        internal class Constraint
        {
            public char Character { get; set; }
            public int Position { get; set; }
            public char Action { get; set; }
        }
        internal enum GameMode
        {
            FindWord,
            GuessWord
        }
        static internal GameMode gameMode { get; set; } = GameMode.FindWord;
        static internal Dictionary<string, int>burnWords { get; } = new Dictionary<string, int>();
        static internal List<Constraint>constraints { get; } = new List<Constraint>();
        static internal string file { get; set; } = "words-all.txt";
        static internal List<string> filteredWords { get; } = new List<string>();
        static internal string guessLetters { get; set; } = "";
        static internal string invalidLetters { get; set; } = "";
        static internal Dictionary<char, int>letterFrequency { get; } = new Dictionary<char, int>();
        static internal Dictionary<int, Dictionary<char, int>>letterFrequencyByPosition { get; } = new Dictionary<int, Dictionary<char, int>>();
        static internal string validLetters { get; set; } = "";
        static internal int wordLength { get; set; } = 5;
        static internal List<string> words { get; } = new List<string>();
        static internal Dictionary<string, int>wordScores { get; } = new Dictionary<string, int>();
        static internal int WordScore(Dictionary<char, int>letterFrequency, string word)
        {
            int score = 0;
            foreach (var letter in word)
            {
                score += letterFrequency[letter];
            }
            return score;
        }

        static internal void   LoadWordsFromFile()
        {
            // Load words from file with length equal to wordLength into words
            words.AddRange(
                File.ReadLines(file)
                .Where(line => line.Length == wordLength)
                .ToList()
            );
        }
        static internal int    ParseArgs(Dictionary<string, string> argsDict, string argLong, string argShort, int defaultValue = 5)
        {
            if( null == argLong )
                throw new ArgumentException("argLong cannot both be null");
            if( null == argShort )
                argShort = argLong.Substring(0, 1);
            return argsDict.ContainsKey($"-{argLong}") ? int.Parse(argsDict[$"-{argLong}"]) : (argsDict.ContainsKey($"-{argShort}") ? int.Parse(argsDict[$"-{argShort}"]) : defaultValue);
        }
        static internal string ParseArgs(Dictionary<string, string> argsDict, string argLong, string argShort, string defaultValue)
        {
            if( String.IsNullOrEmpty(argLong ) )  throw new ArgumentException("argLong cannot both be null");
            if( String.IsNullOrEmpty(argShort) )  argShort = argLong.Substring(0, 1);
            return argsDict.ContainsKey($"-{argLong}") ? argsDict[$"-{argLong}"] : (argsDict.ContainsKey($"-{argShort}") ? argsDict[$"-{argShort}"] : defaultValue);
        }
        static internal void   ParseConstraints(string constraintsStr)
        {
            for (int i = 0; i < constraintsStr.Length; i += 3)
            {
                char character = constraintsStr[i];
                int position = constraintsStr[i + 1] - '0';
                char action = constraintsStr[i + 2];

                if (position > 0 && position <= wordLength)
                {
                    constraints.Add(new Constraint { Character = character, Position = position, Action = action });
                    // if validLetters does not contain character, add it
                    if (!validLetters.Contains(character))
                    {
                        validLetters += character;
                    }
                }
            }
        }
        static internal void   CalculateFrequency()
        {
            // Populate letterFrequency with all the letters found from a to z and set the frequency to 0
            for (char c = 'a'; c <= 'z'; c++)
            {
                letterFrequency.Add(c, 0);
            }
            // Calculate the frequency of all the letters found in the filteredWords
            foreach (var word in Solver.filteredWords)
            {
                foreach (var letter in word)
                {
                    letterFrequency[letter]++;
                }
            }
            // Calculate the frequency of letters for each position in the word based on the filteredWords found
            for (int i = 0; i < wordLength; i++)
            {
                letterFrequencyByPosition.Add(i, new Dictionary<char, int>());
                for (char c = 'a'; c <= 'z'; c++)
                {
                    letterFrequencyByPosition[i].Add(c, 0);
                }
            }
            foreach (var word in Solver.filteredWords)
            {
                for (int i = 0; i < wordLength; i++)
                {
                    letterFrequencyByPosition[i][word[i]]++;
                }
            }
        }
        static internal void   CalculateScore(){
            // Calculate a score for each word based on the frequency of each letter
            foreach (var word in Solver.filteredWords)
            {
                int score = Solver.WordScore(Solver.letterFrequency, word);
                wordScores.Add(word, score);
            }
        }
        static internal void   FilterWords()
        {
            foreach (var word in words)
            {
                // Skip word if it contains ANY of the invalid characters
                if (word.Any(c => invalidLetters.Contains(c)))
                {
                    continue;
                }
                // Skip word if it fails ANY of the constraints
                bool matchesConstraints = true;
                foreach (var c in constraints)
                {
                    char upperChar = word[c.Position - 1];
                    if (c.Action == '+' && upperChar != c.Character)
                    {
                        matchesConstraints = false;
                        break;
                    }
                    if (c.Action == '-' && upperChar == c.Character)
                    {
                        matchesConstraints = false;
                        break;
                    }
                }

                if (matchesConstraints)
                {
                    // Skip word unles it contains at least one of EACH valid characters
                    if (validLetters.All(c => word.Contains(c))) {
                        filteredWords.Add(word);
                    }
                }
            }
        }
        static internal void   FindBurnWords(){
            // Find some burn words - a word is a burn word if it contains at least 2 unique consonant letters that are
            // not in the validLetters and are not in the invalidLetters

            var uniqueLetters = new HashSet<char>();
            foreach (var word in Solver.words)
            {
                int burnCount = 0;
                uniqueLetters.Clear();
                foreach (var letter in word)
                {
                    if (!"aeiouy".Contains(letter) && !validLetters.Contains(letter) && !invalidLetters.Contains(letter) && !uniqueLetters.Contains(letter))
                    {
                        uniqueLetters.Add(letter);
                        burnCount++;
                    }
                }
                // Calculate a score for each word based on the frequency of each letter given it's position
                int score = 0;
                for (int i = 0; i < wordLength; i++)
                {
                    score += letterFrequencyByPosition[i][word[i]];
                }
                // Reduce score for each violated constraint
                foreach (var c in constraints)
                {
                    char upperChar = word[c.Position - 1];
                    if (c.Action == '+' && upperChar != c.Character)
                    {
                    }
                    if (c.Action == '-' && upperChar == c.Character)
                    {
                        score-=letterFrequency[c.Character];
                    }
                }
                // Only add to burnWords if not already in there
                if (burnCount >= 3 && !burnWords.ContainsKey(word)) {
                    burnWords.Add(word, score); // *burnCount);
                }
            }
        }
        static internal void   FixInconsistencies(){
            // Remove from invalidLetters any letters that are in validLetters
            foreach (var letter in validLetters)
            {
                invalidLetters = invalidLetters.Replace(letter.ToString(), "");
            }
        }
        static internal void   GuessLetters(){
            // Find all words that contain only letters in guessLetters and add them to filteredWords
            foreach (var word in Solver.words)
            {
                if (word.All(c => guessLetters.Contains(c)))
                {
                    filteredWords.Add(word);
                }
            }
            // For each letter in guessLetters, get the number of occurrences of that letter in guessLetters
            var guessLettersCount = new Dictionary<char, int>();
            foreach (var letter in guessLetters)
            {
                if (!guessLettersCount.ContainsKey(letter))
                {
                    guessLettersCount.Add(letter, 0);
                }
                guessLettersCount[letter]++;
            }
            // For each filtered word, add it to a new list invalidWords if it contains more than the number of occurrences of each letter in guessLetters
            var invalidWords = new List<string>();
            foreach (var word in filteredWords)
            {
                foreach (var letter in guessLetters)
                {
                    if (word.Count(c => c == letter) > guessLettersCount[letter])
                    {
                        invalidWords.Add(word);
                        break;
                    }
                }
            }
            // Remove all invalidWords from filteredWords
            foreach (var word in invalidWords)
            {
                filteredWords.Remove(word);
            }     
        }
        static internal void   Init(string[] args)
        {
            var argsDict = args
                .Select((arg, index) => new { arg, index })
                .Where(x => x.arg.StartsWith("-"))
                .ToDictionary(x => x.arg, x => args[x.index + 1].ToLower());

                file           = ParseArgs(argsDict, "file",           "f", "words-all.txt" );
                wordLength     = ParseArgs(argsDict, "wordLength",     "l",   5 );
                validLetters   = ParseArgs(argsDict, "validLetters",   "v",  "" );
                invalidLetters = ParseArgs(argsDict, "invalidLetters", "i",  "" );
                guessLetters   = ParseArgs(argsDict, "guessLetters",   "g",  "" ); 
            var constraintsStr = ParseArgs(argsDict, "constraints",    "c",  "" );
            if ("" == validLetters && "" == invalidLetters && "" == guessLetters && "" == constraintsStr)
            {
                throw new ArgumentException("You must specify either validLetters/invalidLetters or guessLetters or constraints");
            }
            if (("" != validLetters || "" != invalidLetters || "" != constraintsStr) && "" != guessLetters)
            {
                throw new ArgumentException("You cannot specify guessLetters with validLetters, invalidLetters or constraints");
            }
            LoadWordsFromFile();
            if ("" != constraintsStr ) {
                ParseConstraints(constraintsStr);
            }
            if ("" != guessLetters) {
                gameMode = GameMode.GuessWord;
            } else {
                gameMode = GameMode.FindWord;
            }
        }
        static internal void   PrintFoundWords() {
            // Print the filtered word in descending score order with the score
            Console.WriteLine($"{wordScores.Count} Possible words:");
            foreach (var word in wordScores.Keys.OrderByDescending(x => wordScores[x]))
            {
                Console.WriteLine($"{word}: {wordScores[word]}");
            }
            Console.WriteLine();
            // Print the frequency of each letter in descending order
            Console.WriteLine("Letter frequency:");
            foreach (var letter in letterFrequency.Keys.OrderBy(x => -letterFrequency[x]))
            {
                Console.Write($"{letter}: {letterFrequency[letter]}, ");
            }
            Console.WriteLine();
            Console.WriteLine();
            // Print the frequency of each letter for each position in the word in descending order but only for those with a frequency > 0
            Console.WriteLine("Letter frequency by position:");
            for (int i = 0; i < wordLength; i++)
            {
                Console.Write($"Position {i + 1}: ");
                foreach (var letter in letterFrequencyByPosition[i].Keys.OrderBy(x => -letterFrequencyByPosition[i][x]))
                {
                    if (letterFrequencyByPosition[i][letter] > 0)
                    {
                        Console.Write($"{letter}: {letterFrequencyByPosition[i][letter]}, ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine($"First {Suggest} burn words:");
            // Print the top 5 burn words with their score
            foreach (var word in burnWords.Keys.OrderByDescending(x => Solver.burnWords[x]).Take(Suggest))
            {
                Console.WriteLine($"{word}: {burnWords[word]}");
            }
        }
        static internal void   PrintGuessedWords() {
            // Print the filtered word in alphabetic order
            Console.WriteLine($"{filteredWords.Count} Possible words:");
            foreach (var word in filteredWords.OrderBy(x => x))
            {
                Console.WriteLine(word);
            }
        }
        static internal void   Solve(){
            if (gameMode == GameMode.FindWord) {
                FixInconsistencies();
                FilterWords();
                CalculateFrequency();
                CalculateScore();
                FindBurnWords();
                PrintFoundWords();
            } else {
                GuessLetters();
                PrintGuessedWords();
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Solver.Init(args);
            Solver.Solve();
        }
    }
}
