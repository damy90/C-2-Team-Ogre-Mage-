using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public class MainGame
{
    private static readonly Random random = new Random(); // Generator for pulling random questions.

    private static readonly List<string> questions = (File.ReadAllLines(@"questions\questions.txt")).ToList();
    // Load all questions from file.

    private static readonly List<string> answers = (File.ReadAllLines(@"questions\answers.txt")).ToList();
    // Load all answers from file.

    private static readonly int consoleWidth = 107;
        //Console.LargestWindowWidth - 60;   <-- depends on the screen resolution and default properties

    private static readonly int consoleHeight = 50; //Console.LargestWindowHeight - 20;

    private static int score = 0;
    private static int livesCount = 3;
    private static int gameFieldTop = 12;
    private static int gameFiledBottom = 6;
    private static string container;
    private static int index;
    private static char[] addLetter;


    private static int oldPosition;

    private static void Main(string[] args)
    {
        Console.SetWindowSize(consoleWidth, consoleHeight);
        Console.SetBufferSize(consoleWidth, consoleHeight + 1); //+10

        Console.CursorVisible = false;

        int nextQuestion = random.Next(questions.Count);
        string question = GetQuestion(nextQuestion);
        string answer = GetAnswer(nextQuestion);
        container = new string('*', answer.Length);


        // PrintStartScreen(consoleWidth, consoleHeight); // Timer start.
        StartGame(question, container, consoleWidth, consoleHeight);
    }

    private static void StartGame(string question, string answer, int consoleWidth, int consoleHeight)
    {
        ModifyInfoBar(question, answer, consoleWidth, consoleHeight);
        var randomGenerator = new Random();

        var player = new Object();

        player.x = consoleWidth/2;
        player.y = consoleHeight - 4;
        player.str = "===";
        player.color = ConsoleColor.Red;

        PrintOnPosition(player.x, player.y, player.str, player.color);

        var bomb = new Object();

        bomb.x = randomGenerator.Next(2, consoleWidth - 2);
        bomb.y = gameFieldTop;
        bomb.str = "&";
        bomb.color = ConsoleColor.Red;

        PrintOnPosition(bomb.x, bomb.y, bomb.str, bomb.color);

        var letter = new Object();

        letter.x = randomGenerator.Next(2, consoleWidth - 2);
        letter.y = gameFieldTop;
        letter.c = (char) randomGenerator.Next(65, 91);
        letter.color = ConsoleColor.White;

        PrintOnPosition(letter.x, letter.y, letter.str, letter.color);

        var del = new Object();

        del.x = randomGenerator.Next(2, consoleWidth - 2);
        del.y = gameFieldTop;
        del.c = '<'; // to change
        del.color = ConsoleColor.DarkGreen;

        PrintOnPosition(del.x, del.y, del.str, del.color);

        while (true)
        {
            #region Pressed Key, Moving Playes

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

            #endregion

            //Movе Falling Bombs last edit - to try with Queue

            if (bomb.y < consoleHeight - 4)
            {
                PrintOnPosition(bomb.x, bomb.y, " ", ConsoleColor.White);
                bomb.y = bomb.y + 1;

                if (bomb.y == consoleHeight - 4)
                {
                    if (bomb.x == player.x || bomb.x == player.x + 1 || bomb.x == player.x + 2)
                    {
                        PrintOnPosition(bomb.x, bomb.y, " ", ConsoleColor.White);
                        livesCount--;
                        PrintOnPosition(bomb.x, bomb.y, "=", player.color);
                        Console.SetCursorPosition(consoleWidth/2, consoleHeight/2);
                        Console.WriteLine("BOMB");
                        ModifyInfoBar(question, answer, consoleWidth, gameFieldTop);
                    }
                }
                else
                {
                    PrintOnPosition(bomb.x, bomb.y, bomb.str, bomb.color);
                }
            }

            //Movе Falling Letters last edit - to try with Queue

            if (letter.y < consoleHeight - 4)
            {
                PrintOnPosition(letter.x, letter.y, " ", ConsoleColor.White);
                letter.y = letter.y + 1;

                if (letter.y == consoleHeight - 4)
                {
                    if (letter.x == player.x || letter.x == player.x + 1 || letter.x == player.x + 2)
                    {
                        PrintOnPosition(letter.x, letter.y, " ", ConsoleColor.White);
                        addLetter = container.ToCharArray();
                        addLetter[index] = letter.c;
                        index++;
                        PrintOnPosition(letter.x, letter.y, "=", player.color);
                        Console.SetCursorPosition(consoleWidth/2, consoleHeight/2);
                        Console.WriteLine("LETTER");
                        container = string.Join("", addLetter);
                        ModifyInfoBar(question, container, consoleWidth, gameFieldTop);
                    }
                }
                else
                {
                    PrintOnPosition(letter.x, letter.y, letter.c, letter.color);
                }
            }

            //Movе Falling DEL last edit - to try with Queue

            if (del.y < consoleHeight - 4)
            {
                PrintOnPosition(del.x, del.y, " ", ConsoleColor.White);
                del.y = del.y + 1;

                if (del.y == consoleHeight - 4)
                {
                    if (del.x == player.x || del.x == player.x + 1 || del.x == player.x + 2)
                    {
                        PrintOnPosition(del.x, del.y, " ", ConsoleColor.White);
                        addLetter = container.ToCharArray();
                        addLetter[index] = '*';
                        index--;
                        PrintOnPosition(del.x, del.y, "=", player.color);
                        Console.SetCursorPosition(consoleWidth/2, consoleHeight/2);
                        Console.WriteLine("DEL");
                        container = string.Join("", addLetter);
                        ModifyInfoBar(question, container, consoleWidth, gameFieldTop);
                    }
                }
                else
                {
                    PrintOnPosition(del.x, del.y, del.c, del.color);
                }
            }

            Console.SetCursorPosition(75, 25);
            Console.Write("Check collision: {0},{1}", livesCount, string.Join("", container));
            Thread.Sleep(100);
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

        Console.Write(" " + answer + secondPadding.Append(' ', consoleWidth - 2 - answer.Length));
        padding.Clear();

        Console.WriteLine(padding.Append(' ', consoleWidth));
        padding.Clear();

        Console.WriteLine(padding.Append(' ', consoleWidth));
        padding.Clear();
        // Check if we call the method for re-drawing after collision and if so re-draw only the infoBar
        if (consoleHeight == gameFieldTop)
        {
            Console.ResetColor();
            return;
        }

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

    private static void PrintOnPosition(int x, int y, char c, ConsoleColor color)
    {
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = color;
        Console.Write(c);
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

    private struct Object // Movement coordinates.
    {
        public char c;
        public ConsoleColor color;
        public string str;
        public int x;
        public int y;
    }
}