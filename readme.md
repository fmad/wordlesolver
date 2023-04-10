# Wordle Solver
This utility will give you a list of possible words that match the criteria specified - this is to be used after at least one try otherwise it will just list all the words
It will also show the frequency of letters in the potential solutions, and even attempts to provide some "burn" words - words designed to test the existance of unknown letters that can help decide what the final word will be - this last bit is still a work in progress.

## Parameters
-f | -file           = specify a word list file (default words-all.txt)
-w | -wordLength     = word length to consider, usually 5 or 6, default 5
-i | -invalidLetters = list of invalid letters, i.e., letters already found to NOT be part of the desired word
-v | -validLetters   = list of valid letters, i.e., letters known to BE part of the desired word - optional as it can be inferred from -c
-c | -constraints    = list of constraints in the format ABC, where A = letter, B = position, C= - or +
                       e.g.: -c A2-I3+ => There is an A, but NOT in the 2nd position (-) and there is an I IN the 3rd position (+)
## Examples
dotnet run -- -f words.txt -w 5 -v "IK" -i "WHALEDRN" -c "I3+K5-S1+K2-P4-S5-"
dotnet run -- -i caremnchlt -c s5-u2-s3-y5+