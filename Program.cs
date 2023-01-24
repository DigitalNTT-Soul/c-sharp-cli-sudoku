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
                if (inputString[i] < '0' && '9' < inputString[i])
                {
                    this.state[i / 2].value = 0;
                    this.state[i / 2].isPreset = false;
                    continue;
                }

                this.state[i / 2].value = inputString[i] - '0';
                this.state[i / 2].isPreset = inputString[i + 1] == 't';
            }
        }

        private int calculateRowFromIndex(int cellIndex)
        {
            return cellIndex / 9;
        }

        private int calculateColFromIndex(int cellIndex)
        {
            return cellIndex % 9;
        }

        private int calculateBlockFromIndex(int cellIndex)
        {
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
            bool[] rowContains = new bool[9]; // bools default to false in C#, which is desired behavior in cell testing

            // looping
            for (int i = rowStartIndex; i < rowStartIndex + 9; i++)
            {
                if (i == cellIndex && value >= 0)
                    checkValue = value;
                else
                    checkValue = this.state[i].value;

                if (checkValue == 0) continue; // empty cells don't count against the bool array

                checkValue--; // offset by 1 so it works as an index in the bool array
                if (rowContains[checkValue])
                    return false;
                else
                    rowContains[checkValue] = true;
            }
            return true;
        }

        private bool colIsLegal(int col, int value = -1, int cellIndex = -1)
        {
            // sanity checks
            if (col < 0 || 8 < col) return false;

            // loop setup
            int checkValue;
            bool[] colContains = new bool[9]; // bools default to false in C#, which is desired behavior in cell testing

            // looping
            for (int i = col; i < this.state.Length; i += 9)
            {
                if (i == cellIndex && value >= 0)
                    checkValue = value;
                else
                    checkValue = this.state[i].value;

                if (checkValue == 0) continue; // empty cells don't count against the bool array

                checkValue--; // offset by 1 so it works as an index int he bool array
                if (colContains[checkValue])
                    return false;
                else
                    colContains[checkValue] = true;
            }
            return true;
        }

        private bool blockIsLegal(int block, int value = -1, int cellIndex = -1)
        {
            // sanity checks
            if (block < 0 || 8 < block) return false;

            // loop setup
            int checkValue;
            bool[] blockContains = new bool[9];
            int blockRowStart = (block / 3) * 27;
            int blockColStart = (block % 3) * 3;
            int blockStart = blockRowStart + blockColStart;
            int blockEnd = blockStart + 20;

            // looping
            for (int i = blockStart; i < blockEnd; i += 9)
            {
                for (int j = 0; j < 3; j++)
                {
                    int index = i + j;
                    if (index == cellIndex && value >= 0)
                        checkValue = value;
                    else
                        checkValue = this.state[index].value;

                    if (checkValue == 0) continue;

                    checkValue--;
                    if (blockContains[checkValue])
                        return false;
                    else
                        blockContains[checkValue] = true;
                }
            }
            return true;
        }

        public bool isLegal()
        {
            for (int i = 0; i < 9; i ++)
            {
                if (!(
                    rowIsLegal(i) &&
                    colIsLegal(i) &&
                    blockIsLegal(i)
                    ))
                    return false;
            }
            return true;
        }

        public bool valueIsLegalInCell(int value, int cellIndex)
        {
            if (value == 0) return true;
            if (value < 0 || 9 < value) return false;

            int rowNum = calculateRowFromIndex(cellIndex);
            int colNum = calculateColFromIndex(cellIndex);
            int blockNum = calculateBlockFromIndex(cellIndex);

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
                "I {72} {73} {74} | {75} {76} {77} | {78} {79} {80}\n" +
                "\n";

            string formattedBoard = String.Format(formatString, board.state);

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
                if (invalidCell)
                    Console.WriteLine("Error: Please enter a valid, 2-character cell coordinate (as described in the prompt) to a changeable cell.");

                // only reason we would pass back to the top is for an invalid cell
                //      as valid cells would immediately break the loop, calculate
                //      and return to caller.
                invalidCell = true;

                Console.Write("Please select a cell [A-I][1-9]: ");
                string coords = Console.ReadLine().Trim().ToLower();

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
                cellIndex = row * 9 + col;
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

            // value entering loop
            string stringValue = "";
            while (true)
            {
                Console.Write("Please enter a value between 1 and 9 (or 0 to erase the current cell value)\nYou can also enter an 'x' to cancel and return to the menu: ");
                stringValue = Console.ReadLine().Trim().ToLower();
                if (stringValue.Length != 1)
                {
                    // invalid because too long
                    Console.WriteLine("Error Invalid entry. Please ensure your entry is a single numerical character.");
                    continue;
                }

                if (stringValue == "x") return; // 'cancel' option

                int value = stringValue[0] - '0'; // char subtraction to convert to int
                if (value > 9)
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
            Console.WriteLine("Value Saved.");
            
        }
        static void promptAndListLegalCellValues(Sudokuboard board)
        {
            // get cellIndex firsst
            int cellIndex = promptForCell(board);

            // Value finding loop
            string legalValues = "";
            bool firstValue = true;
            for (int value = 1; value < 10; value++)
            {
                if (board.valueIsLegalInCell(value, cellIndex))
                {
                    if (!firstValue)
                        legalValues += ", ";
                    else
                        firstValue = false;

                    legalValues += value;
                }
            }

            if (legalValues == "")
                legalValues = "none";

            Console.WriteLine($"The legal values for that cell are: {legalValues}");

        }
        static void promptAndSaveGameToFile(Sudokuboard board)
        {
            Console.Write("Please enter a file name to save the game to: ");
            string filename = Console.ReadLine().Trim();

            // overwrite check
            if (File.Exists(filename))
            {
                Console.Write("That file already exists. Are you sure you'd like to overwrite it? [Y/n]: ");
                string selection = Console.ReadLine().Trim().ToLower();
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
            Console.Write("Please enter a file name to save the puzzle to: ");
            string filename = Console.ReadLine().Trim();

            // overwrite check
            if (File.Exists(filename))
            {
                Console.Write("That file already exists. Are you sure you'd like to overwrite it? [Y/n]: ");
                string selection = Console.ReadLine().Trim().ToLower();
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
            while (true)
            {
                Console.Write("Please enter a file name to load the game from: ");
                string filename = Console.ReadLine().Trim();
                if (!File.Exists(filename))
                {
                    Console.WriteLine("That file does not exist. Please enter a valid save file to load from.");
                    continue;
                }

                string filetext = File.ReadAllText(filename).Trim();
                if (filetext.Length != 81*2)
                {
                    Console.WriteLine("That file is either invalid or corrupted. Please enter another save file to load from.");
                    continue;
                }

                for (int i = 0; i < 81*2; i++)
                {
                    if (i % 2 == 0)
                    {
                        if (filetext[i] < '0' || '9' < filetext[i])
                        {
                            Console.WriteLine("That file is either invalid or corrupted. Please enter another save file to load from.");
                            continue;
                        }
                    }
                    else
                    {
                        if (filetext[i] != 't' && filetext[i] != 'f')
                        {
                            Console.WriteLine("That file is either invalid or corrupted. Please enter another save file to load from.");
                            continue;
                        }
                    }
                }

                board.loadString(filetext);
                if (!board.isLegal())
                {
                    Console.WriteLine("That file is either invalid or corrupted. Please enter another save file to load from.");
                    continue;
                }
                else
                    break;
            }
            Console.WriteLine("Game Loaded!");
        }
        static bool isGameComplete(Sudokuboard board)
        {
            for (int i = 0; i < 81; i++)
                if (board.state[i].value == 0)
                    return false;
            return board.isLegal();
        }
        static void displayMainMenuLoop(Sudokuboard board)
        {
            string menuText = "Options:\n" +
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
                        displayBoard(board);
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

/*
 * allow player to update the board state by adding numbers to specific cells in the board
 * Check to see if player's move is legal and warn them if it isn't.
 * Give player option to see all legal values for a given cell
 *      use same subfunctions as legality check from previous step
 */