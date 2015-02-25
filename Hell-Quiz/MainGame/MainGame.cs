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

    private static readonly int consoleWidth = 107;//Console.LargestWindowWidth - 60;   <-- depends on the screen resolution and default properties
    private static readonly int consoleHeight = 50;//Console.LargestWindowHeight - 20;

    private static int score = 0;
    static int livesCount = 3;
    static int gameFieldTop = 12;
    static int gameFiledBottom = 6;
    static string container;
    static string gameover = "GAME OVER!";
    static int index = 0;
    static char[] addLetter;
    static int nextQuestion = random.Next(questions.Count);
    static string correctAnswer = GetAnswer(nextQuestion).ToUpper();

    private static int oldPosition;

    static void Main(string[] args)
    {
        Console.SetWindowSize(consoleWidth, consoleHeight);
        Console.SetBufferSize(consoleWidth, consoleHeight + 1);//+10

        Console.CursorVisible = false;

        string question = GetQuestion(nextQuestion);

        container = new string('*', correctAnswer.Length);
        // PrintStartScreen(consoleWidth, consoleHeight); // Timer start.
        StartGame(question, container, consoleWidth, consoleHeight);
    }

    static void StartGame(string question, string answer, int consoleWidth, int consoleHeight)
    {
        ModifyInfoBar(question, answer, consoleWidth, consoleHeight);
        var randomGenerator = new Random();

        var player = new Object();

        player.x = consoleWidth / 2;
        player.y = consoleHeight - 4;
        player.str = "===";
        player.color = ConsoleColor.Red;

        PrintOnPosition(player.x, player.y, player.str, player.color);

        var watch = Stopwatch.StartNew();
        var watchBombs = Stopwatch.StartNew(); // Define dropping bombs in given time i.e how frequent will drop new bomb
        var watchLetters = Stopwatch.StartNew(); // Same for letters
        var watchDels = Stopwatch.StartNew(); // Same for deletes

        var bombs = new List<Object>();
        var letters = new List<Object>();
        var dels = new List<Object>();
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
            #region Adding objects to list // TODO: Balance frequence of objects
            // Add bombs to list  
            if (watchBombs.ElapsedMilliseconds >= 400) // Define how frequent bombs are droping
            {
                var bomb = new Object();

                bomb.x = randomGenerator.Next(2, consoleWidth - 2);
                bomb.y = gameFieldTop;
                bomb.str = "&";
                bomb.color = ConsoleColor.Red;

                bombs.Add(bomb);
                watchBombs.Restart();
            }
            // Add letters to list
            if (watchLetters.ElapsedMilliseconds >= 300) // Define how frequent letters are dropping
            {
                var letter = new Object();

                letter.x = randomGenerator.Next(2, consoleWidth - 2);
                letter.y = gameFieldTop;
                // letter.c = (char) randomGenerator(65, 91) <-- Drop lettes A-Z
                // The code below drop only letters from the answer
                letter.c = (char)correctAnswer[randomGenerator.Next(0, correctAnswer.Length)];
                letter.color = ConsoleColor.White;
                letters.Add(letter);
                watchLetters.Restart();
            }
            // Add dels to list
            if (watchDels.ElapsedMilliseconds >= 500) // Define how frequent deletes are droping
            {
                var del = new Object();

                del.x = randomGenerator.Next(2, consoleWidth - 2);
                del.y = gameFieldTop;
                del.c = '<'; // To change
                del.color = ConsoleColor.DarkGreen;

                dels.Add(del);
                watchDels.Restart();
            }
            #endregion
            if (watch.ElapsedMilliseconds >= 200)
            {
                //Movе Falling Bombs 
                for (int b = 0; b < bombs.Count; b++)
                {
                    Object newBomb = bombs[b];

                    if (newBomb.y < consoleHeight - 4)
                    {
                        PrintOnPosition(newBomb.x, newBomb.y, " ", ConsoleColor.White);
                        newBomb.y = newBomb.y + 1M;

                        if (newBomb.y == consoleHeight - 4)
                        {
                            if (newBomb.x == player.x || newBomb.x == player.x + 1 || newBomb.x == player.x + 2)
                            {
                                PrintOnPosition(newBomb.x, newBomb.y, " ", ConsoleColor.White);
                                livesCount--;
                                // TODO: If livesCount == 0 
                                PrintOnPosition(newBomb.x, newBomb.y, "=", player.color);
                                Console.SetCursorPosition(consoleWidth / 2, consoleHeight / 2);
                                Console.WriteLine("BOMB");
                                // Remove "BOMB" from the screen with next re-drawing
                                ModifyInfoBar(question, container, consoleWidth, gameFieldTop);
                            }
                        }
                        else
                        {
                            PrintOnPosition(newBomb.x, newBomb.y, newBomb.str, newBomb.color);
                        }
                        bombs[b] = newBomb;
                    }
                }


                //Movе Falling Letters 
                for (int l = 0; l < letters.Count; l++)
                {
                    var letter = letters[l];
                    if (letter.y < consoleHeight - 4)
                    {
                        PrintOnPosition(letter.x, letter.y, " ", ConsoleColor.White);
                        letter.y = letter.y + 1M;

                        if (letter.y == consoleHeight - 4)
                        {
                            if (letter.x == player.x || letter.x == player.x + 1 || letter.x == player.x + 2)
                            {
                                PrintOnPosition(letter.x, letter.y, " ", ConsoleColor.White);
                                addLetter = container.ToCharArray();
                                addLetter[index] = letter.c;
                                index++;
                                PrintOnPosition(letter.x, letter.y, "=", player.color);
                                Console.SetCursorPosition(consoleWidth / 2, consoleHeight / 2);
                                Console.WriteLine("LETTER");
                                // TODO: Remove "LETTER" with next re-drawing
                                container = string.Join("", addLetter);
                                ModifyInfoBar(question, container, consoleWidth, gameFieldTop);
                            }
                        }
                        else
                        {
                            PrintOnPosition(letter.x, (int)letter.y, letter.c, letter.color);
                        }
                        letters[l] = letter;
                    }
                }

                //Movе Falling DEL 
                for (int d = 0; d < dels.Count; d++)
                {
                    var del = dels[d];
                    if (del.y < consoleHeight - 4)
                    {
                        PrintOnPosition(del.x, del.y, " ", ConsoleColor.White);
                        del.y = del.y + 1M;

                        if (del.y == consoleHeight - 4)
                        {
                            if (del.x == player.x || del.x == player.x + 1 || del.x == player.x + 2)
                            {
                                PrintOnPosition(del.x, del.y, " ", ConsoleColor.White);
                                addLetter = container.ToCharArray();
                                addLetter[index] = '*';
                                index--;
                                PrintOnPosition(del.x, del.y, "=", player.color);
                                Console.SetCursorPosition(consoleWidth / 2, consoleHeight / 2);
                                Console.WriteLine("DEL");
                                // TODO: Remove "DEL" from the screen after.. some time
                                container = string.Join("", addLetter);
                                ModifyInfoBar(question, container, consoleWidth, gameFieldTop);
                            }
                        }
                        else
                        {
                            PrintOnPosition(del.x, (int)del.y, del.c, del.color);
                        }
                    }
                    dels[d] = del;
                }//end falling dels

                watch.Restart();
            }
            if (container[container.Length - 1] != '*')
            {
                if (container == answer)
                {
                    //TODO: win
                }
                else
                {
                    //loose
                    string gameOverInfo = "The correct answer is: ";
                    PrintOnPosition(consoleWidth / 2 - gameover.Length, (consoleHeight - gameFieldTop) / 2, gameover, ConsoleColor.DarkRed);
                    PrintOnPosition(consoleWidth / 2 - gameOverInfo.Length, (consoleHeight - gameFieldTop) / 2 + 1, gameOverInfo + correctAnswer, ConsoleColor.DarkYellow);
                    Console.WriteLine();
                }
                return;
            }
        }//end while true
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
        Console.SetCursorPosition((consoleWidth / 2 - 10), consoleHeight - 2);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("O G R E  M A G E");

        #endregion

        Console.ResetColor();
    }

    private static void PrintOnPosition(int x, decimal y, string str, ConsoleColor color)
    {
        Console.SetCursorPosition(x, (int)y);
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
            Console.SetCursorPosition((consoleHeight / 2) - 6, (consoleWidth / 2) - 2);
            Console.WriteLine("STARTING IN: {0}", count);
            Thread.Sleep(1000);
            count--;
        }
        Console.SetCursorPosition((consoleHeight / 2) - 6, (consoleWidth / 2) - 2);
        Console.WriteLine(padding.Append(' ', 14));
    }
    private struct Object // Movement coordinates.
    {
        public ConsoleColor color;
        public string str;
        public char c;
        public int x;
        public decimal y;

    }
}