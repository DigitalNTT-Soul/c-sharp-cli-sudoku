# Overview

The purpose of this project was to give myself an interesting and motivating goalpost/milestone with which to teach myself the general gist of the C# language by creating a small, yet involved, project that uses the language to accomplish several of the common concepts of programming.

The software I wrote for this self-learning project is a somewhat rudimentary little command-line-based, playable Sudoku game. It allows for simple displaying, editing, saving, and loading of Sudoku boards. It also includes rule validation to ensure that the cardinal rule of Sudoku (that the same number cannot appear twice in any row, column, or block) cannot be broken during loading or editing of boards. The same functionality also powers a menu option that allows the player to see all currently-legal values for a single cell, as well as the final, full-board validation that occurs when the board has been filled, to ensure the solution is valid. The game also keeps track of which cells are "preset" before the game begins, and prevents those cells from being edited, to ensure that the puzzle cannot simply be changed in-game to make it easier. There is also an option to save a board state as a puzzle/template, by marking all filled cells as presets.

{Provide a link to your YouTube demonstration. It should be a 4-5 minute demo of the software running and a walkthrough of the code. Focus should be on sharing what you learned about the language syntax.}

[Demo Video](http://youtube.link.goes.here)

# Development Environment

I developed this program on a Windows 10 machine, using Visual Studio. It uses only the default functionalities of C# and the .NET 4.8 framework

# Useful Websites

A list of websites that I found helpful in this project:

- [W3 Schools](https://www.w3schools.com/cs/index.php)
- [C# Subreddit](https://www.reddit.com/r/csharp)
- [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/)

# Future Work

A list of things that I may fix, improve, or add in the future:

- I could potentially do an auto-solver that would find the solution to a given board and allow you to save it to a file. (Manually solving all 3 of the provided puzzles was more time-consuming than expected).
- I may refactor the Cell array and all references to the cells to use a two-dimensional array instead of single, linear array that requires so much math to legality-check.