using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Threading;

class MainGame
{
    static int consoleWidth = 107;//Console.LargestWindowWidth - 2;
    static int consoleHeight = 50;//Console.LargestWindowHeight - 1;

    private static int gameFieldTopPosition = 12;

    static List<string> questions = (File.ReadAllLines(@"questions\questions.txt")).ToList();
    static List<string> answers = (File.ReadAllLines(@"questions\answers.txt")).ToList();

    static int oldPosition;

    static int score = 0;
    static int livesCount = 3;

    static List<char> symbols = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '&', '<' };//TODO change to list, add and remoove bonus characters to controll the frequency

    private static readonly Random random = new Random();

    struct Object // Movement coordinates.
    {
        public int x;
        public int y;
        public string str;
        public ConsoleColor color;
    }

    private static int lettersSpeed = 300;
    private static string container;

    static void Main(string[] args)
    {
        Console.SetBufferSize(consoleWidth, consoleHeight + 10);
        Console.SetWindowSize(consoleWidth, consoleHeight);
        Console.CursorVisible = false;

        int nextQuestion = random.Next(questions.Count);
        string question = GetRandomQuestion(nextQuestion); //Must create a random generator for the questions (the questions must not repeat during game).
        string answer = GetAnswer(nextQuestion);

        container = new string('*', answer.Length);

        //PrintStartScreen(consoleWidth, consoleHeight);   // Start timer.
        ModifyInfoBar(question, container, consoleWidth, consoleHeight);

        Object player = new Object();

        player.x = consoleWidth / 2;
        player.y = consoleHeight - 4;
        player.str = "===";
        player.color = ConsoleColor.Red;

        PrintOnPosition(player.x, player.y, player.str, player.color);

        int gameFieldWidth = consoleWidth - 2,
            gameFieldHeigth = consoleHeight - 15,
            bottomRow = 0;

        char[][] gameField = new char[gameFieldHeigth][];

        for (int i = 0; i < gameFieldHeigth; i++)
        {
            gameField[i] = new char[gameFieldWidth];
            for (int j = 0; j < gameFieldWidth; j++)
            {
                gameField[i][j] = ' ';
            }
        }

        var watch = Stopwatch.StartNew();
        string playerAnswewr = "";
        while (true)
        {
            if (watch.ElapsedMilliseconds >= lettersSpeed)
            {
                //collision detection
                for (int i = 0; i < 3; i++)
                {
                    if (gameField[bottomRow][player.x + i - 1] != ' ')
                    {
                        
                        if (gameField[bottomRow][player.x + i - 1] == '<')
                        {
                            playerAnswewr = playerAnswewr.Substring(0, playerAnswewr.Length - 1);
                            container = playerAnswewr.PadRight(answer.Length, '*');
                            ModifyInfoBar(question, container, consoleWidth, consoleHeight);
                        }
                        else if (gameField[bottomRow][player.x + i - 1] == '&')
                        {
                            PrintOnPosition(10, 10, "BANG! ", ConsoleColor.Red);
                        }
                        else
                        {
                            playerAnswewr += gameField[bottomRow][player.x + i - 1];
                            container = playerAnswewr.PadRight(answer.Length, '*');
                            ModifyInfoBar(question, container, consoleWidth, consoleHeight);
                        }
                    }
                }

                // TODO: if symbols remains static use it directly, don't pass it to method
                GetNewGamefieldRow(gameFieldWidth, symbols, gameField, bottomRow);
                // The bottom row by order of printing
                bottomRow = PrintGameField(gameFieldHeigth, gameField, player, bottomRow);
                watch.Restart();
            }

            //move player
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                oldPosition = player.x;

                if (pressedKey.Key == ConsoleKey.LeftArrow)
                {
                    if ((player.x - 1) >= 1)    // >= 1 Because of the boundaries of the user interface.
                    {
                        player.x = (player.x - 1);
                        PrintOnPosition(oldPosition + 2, player.y, " ", player.color);
                    }
                }
                if (pressedKey.Key == ConsoleKey.RightArrow)
                {
                    if (player.x + 2 < (consoleWidth - 2))  // < ConsoleWidth - 2, because of the boundaries of the user interface.
                    {
                        player.x = (player.x + 1);
                        PrintOnPosition(oldPosition, player.y, " ", player.color);
                    }
                }
                PrintOnPosition(player.x, player.y, player.str, player.color);
            }
        }
    }

    private static int PrintGameField(int gameFieldHeigth, char[][] gameField, Object player, int bottomRow)
    {
        for (int j = 0, n = bottomRow; j < gameFieldHeigth; j++)
        {
            PrintOnPosition(1, j + gameFieldTopPosition, new string(gameField[n]), ConsoleColor.White);

            //change color of special chars
            var delIndexes = Enumerable.Range(0, gameField[n].Length)
                .Where(i => gameField[n][i] == '<')
                .ToList();
            var bombIndexes = Enumerable.Range(0, gameField[n].Length)
                .Where(i => gameField[n][i] == '&')
                .ToList();
            foreach (int index in delIndexes)
            {
                PrintOnPosition(index + 1, j + gameFieldTopPosition, "<", ConsoleColor.Green);
            }
            foreach (int index in bombIndexes)
            {
                PrintOnPosition(index + 1, j + gameFieldTopPosition, "&", ConsoleColor.Red);
            }

            //Determines the order in which the rows in the array are printed
            if (n == 0)
            {
                n = gameFieldHeigth - 1;
                //TODO What if I remove a non existing symbol?
                //test increasing the frecuency of bombs
                //symbols.Add('&');
                //test decreasing the frequency (the two should balance eachother out in this test) 
                //symbols.Remove('&');
            }
            else
            {
                n--;
            }
        }

        // The bottom row by order of printing
        bottomRow = (bottomRow == gameFieldHeigth - 1) ? 0 : bottomRow + 1;

        PrintOnPosition(player.x, player.y, player.str, player.color);
        return bottomRow;
    }

    private static void GetNewGamefieldRow(int gameFieldWidth, List<char> symbols, char[][] gameField, int row)
    {
        for (int col = 0; col < gameFieldWidth; col++)
        {
            if (random.Next(0, 200) == 0)
            {
                int stone = random.Next(0, symbols.Count);
                gameField[row][col] = symbols[stone];
            }
            else
            {
                gameField[row][col] = ' ';
            }
        }
    }

    private static void ModifyInfoBar(string question, string answer, int consoleWidth, int consoleHeight)
    {
        char heart = '♥';
        int questionLength = (consoleWidth - question.Length - 2);

        StringBuilder padding = new StringBuilder();
        StringBuilder secondPadding = new StringBuilder();
        StringBuilder thirdPadding = new StringBuilder();

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

        Console.Write(" " + answer + secondPadding.Append(' ', consoleWidth - 2 - answer.Length));
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
        Console.SetCursorPosition((consoleWidth / 2 - 10), consoleHeight - 2);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("O G R E  M A G E");

        #endregion
        Console.ResetColor();
    }
    static void PrintOnPosition(int x, int y, string str, ConsoleColor color)
    {
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = color;
        Console.Write(str);
    }

    static string GetRandomQuestion(int nextQuestion) // Gets the question to be displayed. TODO handle 0 questions left, Test in an actual game when ready
    {

        string question = questions[nextQuestion];
        questions.Remove(question);
        return question;
    }

    static string GetAnswer(int nextAnswer)     // Gets the number of the answer.
    {
        string answer = answers[nextAnswer];
        answers.RemoveAt(nextAnswer);
        return answer;
    }

    static void PrintStartScreen(int consoleHeight, int consoleWidth)
    {
        StringBuilder padding = new StringBuilder();
        int count = 3;
        while (count > 0)
        {
            Console.SetCursorPosition((consoleHeight / 2) - 6, (consoleWidth / 2) - 2);
            Console.WriteLine("STARTING IN: {0}", count);
            Thread.Sleep(1000);
            count--;
        }
        Console.SetCursorPosition((consoleHeight / 2) - 6, (consoleWidth / 2) - 2);
        Console.WriteLine(padding.Append(' ', 14));
    }
}