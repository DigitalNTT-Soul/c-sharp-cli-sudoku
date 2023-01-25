using System;
using System.Linq;
using System.IO;

namespace Sudoku
{
    class Cell
    {
        public int value;
        public bool isPreset;
        public Cell()
        {
            this.value = 0;
            this.isPreset = false;
        }

        public override string ToString()
        {
            if (value == 0)
                return " ";
            else
                return value.ToString();
        }
    }
    class Sudokuboard
    {
        public Cell[] state;
        public Sudokuboard()
        {
            this.state = new Cell[81];
            for (int i = 0; i < this.state.Length; i++)
                this.state[i] = new Cell();
        }
        public string getSaveString()
        {
            string result = "";
            for (int i = 0; i < this.state.Length; i++)
            {
                result += this.state[i].value;
                result += (this.state[i].isPreset) ? "t" : "f";
            }
            return result;
        }
        public string getTemplateString()
        {
            string result = "";
            for (int i = 0; i < this.state.Length; i++)
            {
                result += this.state[i].value;
                result += (this.state[i].value == 0) ? "f" : "t"; // all filled cells are saved as Preset, for the creation of entirely new game boards
            }
            return result;
        }
        public void loadString(string inputString)
        {
            // public method misuse paranoia
            inputString = inputString.ToLower();
            if (inputString.Length != 81 * 2)
                return;

            for (int i = 0; i < inputString.Length; i += 2)
            {
                // public method misuse paranoia
                //  if the character that should be a number isn't a number,
                //  we make that cell blank and move to the next batch of chars
                if (inputString[i] < '0' && '9' < inputString[i])
                {
                    this.state[i / 2].value = 0;
                    this.state[i / 2].isPreset = false;
                    continue;
                }

                // otherwise, we do some simple char subtraction to convert numerical characters to ints
                this.state[i / 2].value = inputString[i] - '0';
                // and we set the isPreset value to true if the char after the number is 't'
                //  defaulting to false if the next char is any other char
                this.state[i / 2].isPreset = inputString[i + 1] == 't';
            }
        }

        private int calculateRowFromIndex(int cellIndex)
        {
            // simple formula to calculate row number from
            //  a linear cell index
            return cellIndex / 9;
        }

        private int calculateColFromIndex(int cellIndex)
        {
            // simple formula to calculate column number from
            //  a linear cell index
            return cellIndex % 9;
        }

        private int calculateBlockFromIndex(int cellIndex)
        {
            // slightly less simple formula to calculate block number
            //  from a linear cell index
            int blockRow = cellIndex / 27;
            int blockCol = (cellIndex % 9) / 3;
            return (blockRow * 3) + blockCol;
        }

        private bool rowIsLegal(int row, int value = -1, int cellIndex = -1)
        {
            // sanity checks
            if (row < 0 || 8 < row) return false;

            // loop setup
            int rowStartIndex = row * 9;
            int checkValue;
            bool[] rowAlreadyContains = new bool[9]; // bools default to false in C#, which is desired behavior in cell testing

            // looping
            for (int i = rowStartIndex; i < rowStartIndex + 9; i++)
            {
                // if the i matches the given cellIndex, and a value has been given
                if (i == cellIndex && value >= 0) 
                    // we use the given value for our legality checks, rather than the
                    //  value already in the cell
                    checkValue = value;
                else
                    // otherwise, the use the value of the cell that i is pointing at
                    checkValue = this.state[i].value;

                if (checkValue == 0) continue; // empty cells don't count against the bool array

                checkValue--; // offset by 1 so it works as an index in the bool array

                if (rowAlreadyContains[checkValue])
                    // show that the row is not legal
                    return false; 
                else
                    // remember that the value in question has already appeared in the row
                    rowAlreadyContains[checkValue] = true; 
            }
            // if the loop finishes without any duplicate values to break the rules of Sudoku
            //  we return that the row is legal
            return true;
        }

        private bool colIsLegal(int col, int value = -1, int cellIndex = -1)
        {
            // sanity checks
            if (col < 0 || 8 < col) return false;

            // loop setup
            int checkValue;
            bool[] colAlreadyContains = new bool[9]; // bools default to false in C#, which is desired behavior in cell testing

            // looping
            for (int i = col; i < this.state.Length; i += 9) // the i += 9 is how we hop rows to navigate the column
            {
                // if the i matches the given cellIndex, and a value has been given
                if (i == cellIndex && value >= 0)
                    // we use the given value for our legality checks, rather than the
                    //  value already in the cell
                    checkValue = value;
                else
                    // otherwise, the use the value of the cell that i is pointing at
                    checkValue = this.state[i].value;

                if (checkValue == 0) continue; // empty cells don't count against the bool array

                checkValue--; // offset by 1 so it works as an index int he bool array

                if (colAlreadyContains[checkValue])
                    // show that the row is not legal
                    return false;
                else
                    // remember that the value in question has already appeared in the row
                    colAlreadyContains[checkValue] = true;
            }
            // if the loop finishes without any duplicate values to break the rules of Sudoku
            //  we return that the row is legal
            return true;
        }

        private bool blockIsLegal(int block, int value = -1, int cellIndex = -1)
        {
            // sanity checks
            if (block < 0 || 8 < block) return false;

            // loop setup
            int checkValue;
            bool[] blockAlreadyContains = new bool[9];
            int blockRowStart = (block / 3) * 27;
            int blockColStart = (block % 3) * 3;
            int blockStart = blockRowStart + blockColStart;
            int blockEnd = blockStart + 20;

            // looping
            for (int i = blockStart; i < blockEnd; i += 9) // hop between rows once we process each
            {
                for (int j = 0; j < 3; j++) // loop through the 3 columns of the block for each row
                {
                    int index = i + j;
                    // if the index matches the given cellIndex, and a value has been given
                    if (index == cellIndex && value >= 0)
                        // we use the given value for our legality checks, rather than the
                        //  value already in the cell
                        checkValue = value;
                    else
                        // otherwise, the use the value of the cell that index is pointing at
                        checkValue = this.state[index].value;

                    if (checkValue == 0) continue; // empty cells don't count against the bool array

                    checkValue--; // offset by 1 so it works as an index int he bool array

                    if (blockAlreadyContains[checkValue])
                        // show that the row is not legal
                        return false;
                    else
                        // remember that the value in question has already appeared in the row
                        blockAlreadyContains[checkValue] = true;
                }
            }
            // if the loop finishes without any duplicate values to break the rules of Sudoku
            //  we return that the row is legal
            return true;
        }

        public bool isLegal() // checks the legality of the entire board at once
        {
            // by only checking each column, row, and block once
            //  we only scan the legality of each cell 3 timesj.
            // If we were to check the legality of each cell individually,
            //  it would take 9x longer by scanning each cell 27 times.
            for (int i = 0; i < 9; i ++)
            {
                // every row, column, and block has to be legal
                //  so we use the same i value on all 3 simultaneously,
                //  all leading to the same "return false on fail" behavior.
                if (!(
                    rowIsLegal(i) &&
                    colIsLegal(i) &&
                    blockIsLegal(i)
                    ))
                    return false;
            }
            // if the loop completed successfully, that means
            //  every row, column, and block in the board was legal.
            return true;
        }

        public bool valueIsLegalInCell(int value, int cellIndex)
        {
            // erasing the value in a (changeable) cell is always legal
            if (value == 0) return true;
            // invalid values are obviously not legal anywhere in the board
            if (value < 0 || 9 < value) return false;

            // find the row, column, and block of the given cell
            int rowNum = calculateRowFromIndex(cellIndex);
            int colNum = calculateColFromIndex(cellIndex);
            int blockNum = calculateBlockFromIndex(cellIndex);

            // test all 3, returning true if all of them are legal
            //  and false if ANY of them fail.
            return rowIsLegal(rowNum, value, cellIndex) &&
                   colIsLegal(colNum, value, cellIndex) &&
                   blockIsLegal(blockNum, value, cellIndex);
        }
    }
    internal class Program
    {
        static void displayBoard(Sudokuboard board)
        {
            string formatString = "\n  1 2 3   4 5 6   7 8 9\n" +
                "A {0} {1} {2} | {3} {4} {5} | {6} {7} {8}\n" +
                "B {9} {10} {11} | {12} {13} {14} | {15} {16} {17}\n" +
                "C {18} {19} {20} | {21} {22} {23} | {24} {25} {26}\n" +
                " -------+-------+-------\n" +
                "D {27} {28} {29} | {30} {31} {32} | {33} {34} {35}\n" +
                "E {36} {37} {38} | {39} {40} {41} | {42} {43} {44}\n" +
                "F {45} {46} {47} | {48} {49} {50} | {51} {52} {53}\n" +
                " -------+-------+-------\n" +
                "G {54} {55} {56} | {57} {58} {59} | {60} {61} {62}\n" +
                "H {63} {64} {65} | {66} {67} {68} | {69} {70} {71}\n" +
                "I {72} {73} {74} | {75} {76} {77} | {78} {79} {80}\n";

            // utilizes the .ToString() override method
            //  of each cell to fill out the formatString
            string formattedBoard = String.Format(formatString, board.state);

            // output the board
            Console.Write(formattedBoard);
        }
        static int promptForCell(Sudokuboard board)
        {
            bool invalidCell = false;
            int row = -1;
            int col = -1;
            int cellIndex = -1;
            while (true)
            {
                // error message that plays on the 2nd and later iterations of this loop
                //  due to setting invalidCell = true below.
                if (invalidCell)
                    Console.WriteLine("Error: Please enter a valid, 2-character cell coordinate (as described in the prompt) to a changeable cell.");

                // only reason we would pass back to the top is for an invalid cell
                //      as valid cells would immediately calculate, break the loop,
                //      and return to caller.
                invalidCell = true;

                // prompt and receive
                Console.Write("\nPlease select a cell [A-I][1-9] (order is reversable)\nYou may also enter 'x' to cancel: ");
                string coords = Console.ReadLine().Trim().ToLower();

                // cancel option
                if (coords == "x") return -1;

                // sanity checks on length and content of coords
                if (coords.Length != 2)
                    continue;
                // if first character is the digit
                if (Char.IsDigit(coords[0]))
                {
                    // second character has to be the letter, or invalid coord
                    if (coords[1] < 'a' || 'i' < coords[1])
                        continue;

                    // process coords
                    row = coords[1] - 'a';
                    col = coords[0] - '1';
                }
                // else if second character is the digit
                else if (Char.IsDigit(coords[1]))
                {
                    // first character has to be the letter, or invalid coord
                    if (coords[0] < 'a' || 'i' < coords[0])
                        continue;

                    // process coords
                    row = coords[0] - 'a';
                    col = coords[1] - '1';
                }
                // calculate cellIndex
                cellIndex = row * 9 + col;

                // make sure cell is changeable
                if (board.state[cellIndex].isPreset)
                    Console.WriteLine("Error: This cell cannot be changed!");
                else
                    break;
            }
            return cellIndex;
        }
        static void promptAndEditCell(Sudokuboard board)
        {
            // get cellIndex first
            int cellIndex = promptForCell(board);
            if (cellIndex == -1) return;

            // value entering loop
            string stringValue = "";
            while (true)
            {
                // prompt and receive
                Console.Write("\nPlease enter a value between 1 and 9 (or 0 to erase the current cell value)\nYou can also enter an 'x' to cancel and return to the menu: ");
                stringValue = Console.ReadLine().Trim().ToLower();

                // cancel option
                if (stringValue == "x") return;

                // sanity check on input
                if (stringValue.Length != 1)
                {
                    // invalid because too long
                    Console.WriteLine("Error Invalid entry. Please ensure your entry is a single numerical character.");
                    continue;
                }

                // char subtraction to attempt conversion to int
                int value = stringValue[0] - '0';

                // make sure the value is, in fact, a single-digit number
                if (value < 0 || 9 < value)
                {
                    Console.WriteLine("Error Invalid entry. Please ensure your entry is a single numerical character.");
                    continue;
                }
                // sanity checks over

                // proceed to logic checks
                if (board.valueIsLegalInCell(value, cellIndex))
                {
                    board.state[cellIndex].value = value;
                    break;
                }
                else
                    Console.WriteLine("Error: That value is not legal in that cell.");
            }
            
        }
        static void promptAndListLegalCellValues(Sudokuboard board)
        {
            // get cellIndex firsst
            int cellIndex = promptForCell(board);
            if (cellIndex == -1) return;

            // Value finding loop
            string legalValues = "";
            bool firstValue = true;

            // loop 1-9 to check them all.
            // don't check zeroes because they're legal by default,
            //  as long as the cell is changeable.
            for (int value = 1; value < 10; value++)
            {
                // if a given value is legal in that cell
                if (board.valueIsLegalInCell(value, cellIndex))
                {
                    // if multiple values are legal, we separate them with commas
                    if (!firstValue)
                        legalValues += ", ";
                    else
                        firstValue = false;

                    // add the legal value to the list
                    legalValues += value;
                }
            }

            // this shows that the player has made a mistake somewhere,
            //  but has not necessarily made any illegal moves.
            if (legalValues == "")
                legalValues = "none";

            // display results
            Console.WriteLine($"The legal values for that cell are: {legalValues}");

        }
        static void promptAndSaveGameToFile(Sudokuboard board)
        {
            // prompt and receive filename
            Console.Write("\nPlease enter a file name to save the game to\nYou may also enter 'x' to cancel: ");
            string filename = Console.ReadLine().Trim();

            // cancel option
            if (filename == "x") return;

            // overwrite check
            if (File.Exists(filename))
            {
                // prompt and receive overwrite confirmation
                Console.Write("That file already exists. Are you sure you'd like to overwrite it? [Y/n]: ");
                string selection = Console.ReadLine().Trim().ToLower();

                // default to safest behavior. Don't overwrite unless user input specifically matches "y"
                if (selection != "y")
                {
                    Console.WriteLine("Saving Cancelled!");
                    return;
                }
            }

            // actual saving
            File.WriteAllText(filename, board.getSaveString());
            Console.WriteLine("Game Saved!");
        }
        static void promptAndSavePuzzleToFile(Sudokuboard board)
        {
            // prompt and receive filename
            Console.Write("\nPlease enter a file name to save the puzzle to\nYou may also enter 'x' to cancel: ");
            string filename = Console.ReadLine().Trim();

            // cancel option
            if (filename == "x") return;

            // overwrite check
            if (File.Exists(filename))
            {
                // prompt and receive overwrite confirmation
                Console.Write("That file already exists. Are you sure you'd like to overwrite it? [Y/n]: ");
                string selection = Console.ReadLine().Trim().ToLower();

                // default to safest behavior. Don't overwrite unless user input specifically matches "y"
                if (selection != "y")
                {
                    Console.WriteLine("Saving Cancelled!");
                    return;
                }
            }

            // actual saving
            File.WriteAllText(filename, board.getTemplateString());
            Console.WriteLine("Game Saved!");
        }
        static void promptAndLoadGameFromFile(Sudokuboard board)
        {
            // repeated prompts until valid, legal board is loaded or the user cancels the load
            while (true)
            {
                // prompt and receive filename
                Console.Write("\nPlease enter a file name to load the game from\nYou may also enter 'x' to cancel: ");
                string filename = Console.ReadLine().Trim();

                // cancel option
                if (filename == "x") return;

                // ensure file exists
                if (!File.Exists(filename))
                {
                    Console.WriteLine("That file does not exist. Please enter a valid save file to load from.");
                    continue;
                }

                // read file
                string filetext = File.ReadAllText(filename).Trim();

                // ensure filetext is correct length for a valid save
                if (filetext.Length != 81*2)
                {
                    Console.WriteLine("That file is either invalid or corrupted. Please enter another save file to load from.");
                    continue;
                }

                // ensure filetext is formatted correctly
                for (int i = 0; i < 81*2; i++)
                {
                    // numbers on the even characters
                    if (i % 2 == 0)
                    {
                        if (filetext[i] < '0' || '9' < filetext[i])
                        {
                            Console.WriteLine("That file is either invalid or corrupted. Please enter another save file to load from.");
                            continue;
                        }
                    }

                    // 't'/'f' characters on the odd characters to represent Cell.isPreset status
                    else
                    {
                        if (filetext[i] != 't' && filetext[i] != 'f')
                        {
                            Console.WriteLine("That file is either invalid or corrupted. Please enter another save file to load from.");
                            continue;
                        }
                    }
                }

                // make a temporary board to prevent overwriting the current board with an illegal one.
                Sudokuboard tempBoard = new Sudokuboard();
                tempBoard.loadString(filetext);

                // check board legality
                if (!tempBoard.isLegal())
                {
                    Console.WriteLine("That file contains a board that does not follow the rules of Sudoku. Please enter another save file to load from.");
                    continue;
                }
                else
                {
                    // now that we've confirmed that the file contained a valid, legal board,
                    //  we can safely load it into the active, permanent board object.
                    // tempBoard will get Garbage Collected.
                    board.loadString(filetext);
                    break;
                }
            }
            Console.WriteLine("Game Loaded!");
        }
        static bool isGameComplete(Sudokuboard board)
        {
            // make sure every cell is filled
            for (int i = 0; i < 81; i++)
                if (board.state[i].value == 0)
                    return false;

            // then make sure they're all legal
            return board.isLegal();
        }
        static void displayMainMenuLoop(Sudokuboard board)
        {
            string menuText = "\nOptions:\n" +
                "\t1. Show these instructions.\n" +
                "\t2. Display the board.\n" +
                "\t3. Edit a cell.\n" +
                "\t4. See all legal values for a cell.\n" +
                "\t7. Load game.\n" +
                "\t8. Save game.\n" +
                "\t9. Save board as new puzzle.\n" +
                "\t0. Quit game.\n" +
                "\n" +
                "Select an Option: ";
            bool quitting = false;
            while (!quitting)
            {
                Console.Write(menuText);
                string selection = Console.ReadLine().Trim();
                switch (selection)
                {
                    case "1":
                        continue;
                    case "2":
                        displayBoard(board);
                        continue;
                    case "3":
                        promptAndEditCell(board);
                        displayBoard(board);
                        break;
                    case "4":
                        promptAndListLegalCellValues(board);
                        continue;
                    case "7":
                        promptAndLoadGameFromFile(board);
                        displayBoard(board);
                        break;
                    case "8":
                        promptAndSaveGameToFile(board);
                        continue;
                    case "9":
                        promptAndSavePuzzleToFile(board);
                        continue;
                    case "0": 
                        quitting = true;
                        Console.Write("Would you like to save before quitting? [Y/n]: ");
                        string saveSelection = Console.ReadLine().Trim().ToLower();
                        if (saveSelection != "n")
                            promptAndSaveGameToFile(board);
                        break;
                    default:
                        Console.WriteLine("Input Error: Please ensure you are selecting one (and only one) of the provided options.");
                        continue;
                }
                if (isGameComplete(board))
                {
                    Console.WriteLine("####################################");
                    Console.WriteLine("# Board Complete! Congratulations! #");
                    Console.WriteLine("####################################");
                    break;
                }
            }
            Console.WriteLine("Press any key to exit the game.");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Sudoku!");
            Sudokuboard board = new Sudokuboard();
            displayMainMenuLoop(board);
        }
    }
}