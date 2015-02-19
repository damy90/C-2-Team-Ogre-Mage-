using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

internal class MainGame
{
    private static readonly Random random = new Random(); // Generator for pulling random questions.

    private static readonly List<string> questions = (File.ReadAllLines(@"questions\questions.txt")).ToList();
        // Load all questions from file.

    private static readonly List<string> answers = (File.ReadAllLines(@"questions\answers.txt")).ToList();
        // Load all answers from file.

    private static readonly int consoleWidth = Console.LargestWindowWidth - 60;
    private static readonly int consoleHeight = Console.LargestWindowHeight - 20;

    private static int score = 0;
    private static int livesCount = 3;

    private static int oldPosition;

    private static void Main(string[] args)
    {
        Console.SetBufferSize(consoleWidth, consoleHeight + 10);
        Console.SetWindowSize(consoleWidth, consoleHeight);
        Console.CursorVisible = false;

        int nextQuestion = random.Next(questions.Count);
        string question = GetQuestion(nextQuestion);
        string answer = GetAnswer(nextQuestion);

        // PrintStartScreen(consoleWidth, consoleHeight); // Timer start.
        StartGame(question, answer, consoleWidth, consoleHeight);
    }

    private static void StartGame(string question, string answer, int consoleWidth, int consoleHeight)
    {
        ModifyInfoBar(question, answer, consoleWidth, consoleHeight);

        var player = new Object();

        player.x = consoleWidth/2;
        player.y = consoleHeight - 4;
        player.str = "===";
        player.color = ConsoleColor.Red;

        PrintOnPosition(player.x, player.y, player.str, player.color);

        #region Dany letters drop
        int gameFieldWidth = consoleWidth - 2,
            gameFieldHeigth = consoleHeight - 15,
            bottomRow = 0;

        char[][] gameField = new char[gameFieldHeigth][];
        char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        for (int i = 0; i < gameFieldHeigth; i++)
        {
            gameField[i] = new char[gameFieldWidth];
            for (int j = 0; j < gameFieldWidth; j++)
            {
                gameField[i][j] = ' ';
            }
        }

        var watch = Stopwatch.StartNew();
        #endregion

        while (true)
        {
            #region Dany letterz drop
            if (watch.ElapsedMilliseconds >= 300)
            {
                //TODO check for collisions here
                GetNewGamefieldRow(gameFieldWidth, letters, gameField, bottomRow);
                PrintGameField(gameFieldHeigth, gameField, player, ref bottomRow);
                watch.Restart();
            }
            #endregion

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                oldPosition = player.x;

                if (pressedKey.Key == ConsoleKey.LeftArrow)
                {
                    if ((player.x - 1) >= 1) // >= 1 Because of the boundaries of the user interface.
                    {
                        player.x = (player.x - 1);
                        PrintOnPosition(oldPosition + 2, player.y, " ", player.color);
                    }
                }
                if (pressedKey.Key == ConsoleKey.RightArrow)
                {
                    if (player.x + 2 < (consoleWidth - 2))
                        // < ConsoleWidth - 2, because of the boundaries of the user interface.
                    {
                        player.x = (player.x + 1);
                        PrintOnPosition(oldPosition, player.y, " ", player.color);
                    }
                }
                PrintOnPosition(player.x, player.y, player.str, player.color);
            }
        }
    }

    private static void ModifyInfoBar(string question, string answer, int consoleWidth, int consoleHeight)
    {
        char heart = '♥';
        int questionLength = (consoleWidth - question.Length - 2);

        var padding = new StringBuilder();
        var secondPadding = new StringBuilder();
        var thirdPadding = new StringBuilder();

        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;

        #region Draw Infobar

        Console.Write(new string(' ', consoleWidth));

        Console.Write(" LIVES: "
                      + (new string(heart, livesCount))
                      + (new string(' ', consoleWidth - 21 - Convert.ToString(score).Length - (-3 + livesCount)))
                      + "SCORE: " + score
                      + (new string(' ', 3))
            );

        Console.Write(new string(' ', consoleWidth));
        Console.Write(new string(' ', consoleWidth));

        // Set questions & answers color.
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" " + "QUESTION:" + padding.Append(' ', consoleWidth - 10));
        padding.Clear();

        Console.Write(" " + question.ToUpper() + padding.Append(' ', questionLength));
        padding.Clear();

        Console.Write(padding.Append(' ', consoleWidth));
        padding.Clear();

        Console.WriteLine(padding.Append(' ', consoleWidth));
        padding.Clear();

        Console.Write(" " + "ANSWER:" + padding.Append(' ', consoleWidth - 8));
        padding.Clear();

        Console.Write(" " + padding.Append('*', answer.Length) +
                      secondPadding.Append(' ', consoleWidth - 2 - answer.Length));
        padding.Clear();

        Console.WriteLine(padding.Append(' ', consoleWidth));
        padding.Clear();

        Console.WriteLine(padding.Append(' ', consoleWidth));
        padding.Clear();

        // Print left boundary.
        for (int i = 10, k = 0; i < consoleHeight - 1; i++)
        {
            Console.SetCursorPosition(k, i);
            Console.Write(' ');
        }
        // Print bottom boundary.
        for (int i = consoleHeight - 1, k = 0; k < consoleWidth; k++)
        {
            Console.SetCursorPosition(k, i);
            Console.Write(' ');
        }
        for (int i = consoleHeight - 2, k = 0; k < consoleWidth; k++)
        {
            Console.SetCursorPosition(k, i);
            Console.Write(' ');
        }
        for (int i = consoleHeight - 3, k = 0; k < consoleWidth; k++)
        {
            Console.SetCursorPosition(k, i);
            Console.Write(' ');
        }
        // Print right boundary.
        for (int i = consoleHeight - 1, k = consoleWidth - 1; i >= 0; i--)
        {
            Console.SetCursorPosition(k, i);
            Console.Write(' ');
        }
        // Print Team name.
        Console.SetCursorPosition((consoleWidth/2 - 10), consoleHeight - 2);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("O G R E  M A G E");

        #endregion

        Console.ResetColor();
    }

    private static void PrintOnPosition(int x, int y, string str, ConsoleColor color)
    {
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = color;
        Console.Write(str);
    }

    private static string GetQuestion(int nextQuestion) // Gets the number of the question to be displayed.
    {
        string question = questions[nextQuestion];
        questions.Remove(question);
        return question;
    }

    private static string GetAnswer(int nextAnswer) // Gets the number of the answer.
    {
        string answer = answers[nextAnswer];
        answers.RemoveAt(nextAnswer);
        return answer;
    }

    private static void PrintStartScreen(int consoleHeight, int consoleWidth)
    {
        var padding = new StringBuilder();
        int count = 3;
        while (count > 0)
        {
            Console.SetCursorPosition((consoleHeight/2) - 6, (consoleWidth/2) - 2);
            Console.WriteLine("STARTING IN: {0}", count);
            Thread.Sleep(1000);
            count--;
        }
        Console.SetCursorPosition((consoleHeight/2) - 6, (consoleWidth/2) - 2);
        Console.WriteLine(padding.Append(' ', 14));
    }

    private static int PrintGameField(int gameFieldHeigth, char[][] gameField, Object player, ref int row)
    {
        for (int j = 0, n = row; j < gameFieldHeigth; j++)
        {
            PrintOnPosition(1, j + 12, new string(gameField[n]), ConsoleColor.White);
            if (n == 0)
            {
                n = gameFieldHeigth - 1;
            }
            else
            {
                n--;
            }
        }

        row = (row == gameFieldHeigth - 1) ? 0 : row + 1;

        PrintOnPosition(player.x, player.y, player.str, player.color);
        return row;
    }

    private static void GetNewGamefieldRow(int gameFieldWidth, char[] rocks, char[][] gameField, int row)
    {
        for (int col = 0; col < gameFieldWidth; col++)
        {
            if (random.Next(0, 200) == 0)
            {
                int stone = random.Next(0, rocks.Length);
                gameField[row][col] = rocks[stone];
            }
            else
            {
                gameField[row][col] = ' ';
            }
        }
    }

    private struct Object // Movement coordinates.
    {
        public ConsoleColor color;
        public string str;
        public int x;
        public int y;
    }
}