using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading;

internal class MainGame
{
    private static readonly Random random = new Random(); // Generator for pulling random questions.

    private static readonly List<string> questions = (File.ReadAllLines(@"questions\questions.txt")).ToList();
    // Load all questions from file.

    private static readonly List<string> answers = (File.ReadAllLines(@"questions\answers.txt")).ToList();
    // Load all answers from file.

    private static readonly int consoleWidth = 107; //Console.LargestWindowWidth - 60;   <-- depends on the screen resolution and default properties
    private static readonly int consoleHeight = 50; //Console.LargestWindowHeight - 20;

    private static int score = 0;
    static int livesCount = 3;
    static int gameFieldTop = 12;

    static string container;
    static string gameOverMessage = "GAME OVER!";
    static int index = 0;
    static char[] addLetter;
    static int nextQuestion = random.Next(questions.Count);
    static string correctAnswer = GetAnswer(nextQuestion).ToUpper();

    static StringBuilder padding = new StringBuilder();
    static StringBuilder secondPadding = new StringBuilder();
    static StringBuilder thirdPadding = new StringBuilder();
    private static int oldPosition;

    static int indexCurrentPlayer;
    //static string inputUsername;
    static string pathHistory = @"questions\username.txt";
    private static List<string> username;

    static void Main(string[] args)
    {
        CreateFile();
        Console.SetWindowSize(consoleWidth, consoleHeight);
        Console.SetBufferSize(consoleWidth, consoleHeight + 1);//+10

        Console.CursorVisible = false;
        StartScreen();

        StartGame(DrawNewQuestion(), container, consoleWidth, consoleHeight);
    }

    static string DrawNewQuestion()
    {
        string question = GetQuestion(nextQuestion);
        container = new string('*', correctAnswer.Length);
        //PrintStartScreen(consoleWidth, consoleHeight); // Timer start.
        return question;
    }

    static void StartGame(string question, string answer, int consoleWidth, int consoleHeight)
    {
        ModifyInfoBar(question, answer);
        var randomGenerator = new Random();

        var player = new Player(consoleWidth / 2, consoleHeight - 4);

        PrintOnPosition(player.X, player.Y, player.Str, player.Color);

        var watch = Stopwatch.StartNew();
        var watchBombs = Stopwatch.StartNew();   // Define dropping bombs in given time i.e how frequent will drop new bomb
        var watchLetters = Stopwatch.StartNew(); // Same for letters
        var watchDels = Stopwatch.StartNew();    // Same for deletes

        var bombs = new List<Object>();
        var letters = new List<Object>();
        var dels = new List<Object>();

        while (true)
        {
            #region Pressed Key, Moving Playes
            while (Console.KeyAvailable)
            {

                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                oldPosition = player.X;

                if (pressedKey.Key == ConsoleKey.LeftArrow)
                {
                    if ((player.X - 1) >= 1) // >= 1 Because of the boundaries of the user interface.
                    {
                        player.MovePlayer(-1);
                        PrintOnPosition(oldPosition + 2, player.Y, " ", player.Color);
                    }
                }
                if (pressedKey.Key == ConsoleKey.RightArrow)
                {
                    if (player.X + 2 < (consoleWidth - 2))
                    // < ConsoleWidth - 2, because of the boundaries of the user interface.
                    {
                        player.MovePlayer(1);
                        PrintOnPosition(oldPosition, player.Y, " ", player.Color);
                    }
                }
                PrintOnPosition(player.X, player.Y, player.Str, player.Color);
            }
            #endregion

            #region Adding falling objects to list

            #region Add bombs to list
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
            #endregion

            #region Add letters to list
            // Add letters to list
            if (watchLetters.ElapsedMilliseconds >= 300) // Define how frequent letters are dropping
            {
                var letter = new Object();

                letter.x = randomGenerator.Next(2, consoleWidth - 2);
                letter.y = gameFieldTop;

                letter.c = (char)correctAnswer[randomGenerator.Next(0, correctAnswer.Length)];
                letter.color = ConsoleColor.White;
                letters.Add(letter);
                watchLetters.Restart();
            }
            #endregion

            #region Add deletes to list
            // Add dels to list
            if (watchDels.ElapsedMilliseconds >= 5000) // Define how frequent deletes are droping
            {
                var del = new Object();

                del.x = randomGenerator.Next(2, consoleWidth - 2);
                del.y = gameFieldTop;
                del.c = '<';                           // To change 
                del.color = ConsoleColor.DarkGreen;

                dels.Add(del);
                watchDels.Restart();
            }
            #endregion

            #endregion

            if (watch.ElapsedMilliseconds >= 200)
            {
                #region Move Falling Bombs
                for (int b = 0; b < bombs.Count; b++)
                {
                    Object newBomb = bombs[b];

                    if (newBomb.y < consoleHeight - 4)
                    {
                        PrintOnPosition(newBomb.x, newBomb.y, " ", ConsoleColor.White);
                        newBomb.y = newBomb.y + 1;

                        if (newBomb.y == consoleHeight - 4)
                        {
                            if (newBomb.x == player.X || newBomb.x == player.X + 1 || newBomb.x == player.X + 2)
                            {
                                PrintOnPosition(newBomb.x, newBomb.y, " ", ConsoleColor.White);
                                if (livesCount > 1)
                                {
                                    livesCount--;
                                    PrintOnPosition(newBomb.x, newBomb.y, "=", player.Color);       // Remove "BOMB" from the screen with next re-drawing
                                    Console.SetCursorPosition(0, 1);
                                    RedrawLivesBar('♥');            // Put here method for drawing only the lives count bar.
                                }
                                else if (livesCount == 1)
                                {
                                    livesCount--;
                                    GameOverScreen(consoleWidth, consoleHeight); // GAME OVER.
                                    return;
                                }
                            }
                        }
                        else
                        {
                            PrintOnPosition(newBomb.x, newBomb.y, newBomb.str, newBomb.color);
                        }
                        bombs[b] = newBomb;
                    }
                }
                #endregion

                #region Move Falling Letters
                for (int l = 0; l < letters.Count; l++)
                {
                    var letter = letters[l];
                    if (letter.y < consoleHeight - 4)
                    {
                        PrintOnPosition(letter.x, letter.y, " ", ConsoleColor.White);
                        letter.y = letter.y + 1;

                        if (letter.y == consoleHeight - 4)
                        {
                            if (letter.x == player.X || letter.x == player.X + 1 || letter.x == player.X + 2)
                            {
                                PrintOnPosition(letter.x, letter.y, " ", ConsoleColor.White);
                                PrintOnPosition(letter.x, letter.y, "=", player.Color);
                                UpdateAnswerWhenLetterCaught(letter);
                                Console.SetCursorPosition(0, 8);
                                RedrawAnswerBar(container);
                            }
                        }
                        else
                        {
                            PrintOnPosition(letter.x, (int)letter.y, letter.c, letter.color);
                        }
                        letters[l] = letter;
                    }
                }
                #endregion

                #region Move Falling Del
                // Movе Falling DEL 
                for (int d = 0; d < dels.Count; d++)
                {
                    var del = dels[d];
                    if (del.y < consoleHeight - 4)
                    {
                        PrintOnPosition(del.x, del.y, " ", ConsoleColor.White);
                        del.y = del.y + 1;

                        if (del.y == consoleHeight - 4)
                        {
                            if (del.x == player.X || del.x == player.X + 1 || del.x == player.X + 2)
                            {
                                if (index != 0)
                                {
                                    UpdateAnswerWhenDeleteCaught(del);
                                }
                                PrintOnPosition(del.x, del.y, " ", ConsoleColor.White);
                                PrintOnPosition(del.x, del.y, "=", player.Color);
                                Console.SetCursorPosition(0, 8);
                                RedrawAnswerBar(container);     // Put here redrawing only answer bar
                            }
                        }
                        else
                        {
                            PrintOnPosition(del.x, (int)del.y, del.c, del.color);
                        }
                    }
                    dels[d] = del;
                }
                #endregion

                watch.Restart();
            }

            if (container[container.Length - 1] != '*')
            {
                container = container.ToUpper();
                if (correctAnswer.Equals(container))
                {
                    score += (answer.Length * 20);
                    // TODO: Overwrite 
                }
                else
                {
                    // Lose game.
                    GameOverScreen(consoleWidth, consoleHeight);
                    Console.ReadLine();
                    return;
                }
            }
        }   //end while true
    }

    private static void GameOverScreen(int consoleWidth, int consoleHeight)
    {
        Console.Clear();
        string gameOverInfo = "The correct answer is: ";
        PrintOnPosition((consoleWidth / 2) - gameOverMessage.Length + 4, (consoleHeight / 2 - 1), gameOverMessage, ConsoleColor.DarkRed);
        PrintOnPosition(consoleWidth / 2 - gameOverInfo.Length + 6, (consoleHeight / 2), gameOverInfo + correctAnswer, ConsoleColor.DarkYellow);
        Console.SetCursorPosition(consoleWidth / 2 - 15, consoleHeight);


        //writing to the scoreFile
        if (string.IsNullOrEmpty(username[indexCurrentPlayer + 1]))  // if the player's name is new 
        {
            username.RemoveAt(indexCurrentPlayer + 1);
            username.Add(score.ToString());
            File.WriteAllLines(pathHistory, username);
        }
        else if (
            !(string.IsNullOrEmpty(username[indexCurrentPlayer + 1])) &&   // check if the new score is bigger
            score > int.Parse(username[indexCurrentPlayer + 1]))
        {
            username.RemoveAt(indexCurrentPlayer + 1);
            username.Insert(indexCurrentPlayer + 1, score.ToString());
            File.WriteAllLines(pathHistory, username);
        }


        Console.SetCursorPosition(consoleWidth / 2 - 15, consoleHeight);

    }

    private static void CreateFile()
    {
        try
        {
            if (File.Exists(pathHistory))
            {
                username = (File.ReadAllLines(@"questions\username.txt")).ToList();
                return;
            }
            using (FileStream fs = File.Create(pathHistory))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes("");
                fs.Write(info, 0, info.Length);
            }
            username = (File.ReadAllLines(@"questions\username.txt")).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static void StartScreen()
    {
        //Console.BackgroundColor = ConsoleColor.DarkGray;


        string gameDescription = "Game Description:";
        string enterPlayerName = "Enter username:";
        string howToPlay = "Answer the question.";
        string howToPlay2 = "Collect falling letters in order, to form the answer.";
        string howToPlay3 = "Catch a “backspace” symbol to delete the last letter.";
        string howToPlay4 = "Beware the bombs. Enjoy the game!";

        //Adding Username
        Console.SetCursorPosition(consoleWidth / 2 - enterPlayerName.Length / 2, consoleHeight / 2 - 10);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(enterPlayerName);
        Console.SetCursorPosition(consoleWidth / 2, consoleHeight / 2 - 9);

        inputUsername(Console.ReadLine());

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.SetCursorPosition(consoleWidth / 2 - gameDescription.Length / 2, consoleHeight / 2 - 6);
        Console.WriteLine(gameDescription);
        Console.SetCursorPosition(consoleWidth / 2 - howToPlay.Length / 2, consoleHeight / 2 - 4);
        Console.WriteLine(howToPlay);

        Console.SetCursorPosition(consoleWidth / 2 - howToPlay2.Length / 2, consoleHeight / 2 - 2);
        Console.WriteLine(howToPlay2);

        Console.SetCursorPosition(consoleWidth / 2 - howToPlay3.Length / 2, consoleHeight / 2);
        Console.WriteLine(howToPlay3);
        Console.SetCursorPosition(consoleWidth / 2 - howToPlay4.Length / 2, consoleHeight / +2);
        Console.WriteLine(howToPlay4);

        Console.ReadKey();
        Console.Clear();
    }

    private static void inputUsername(string inputUsername)
    {
        if (username.Contains(inputUsername))
        {
            indexCurrentPlayer = username.IndexOf(inputUsername);

        }
        else
        {
            username.Add(inputUsername);
            username.Add("");
            indexCurrentPlayer = username.IndexOf(inputUsername);
            File.AppendAllText(pathHistory, inputUsername + Environment.NewLine);
        }
    }


    private static void UpdateAnswerWhenLetterCaught(Object letter)
    {
        addLetter = container.ToCharArray();
        addLetter[index] = letter.c;
        index++;
        container = string.Join("", addLetter);
    }

    private static void UpdateAnswerWhenDeleteCaught(Object del)
    {
        addLetter = container.ToCharArray();
        index--;
        addLetter[index] = '*';
        container = string.Join("", addLetter);
    }

    private static void RedrawLivesBar(char heart)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(" LIVES: "
                    + (new string(heart, livesCount))
                    + (new string(' ', consoleWidth - 21 - Convert.ToString(score).Length - (-3 + livesCount)))
                    + "SCORE: " + score
                    + (new string(' ', 3))
          );
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void RedrawQuestionBar(string question)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" QUESTION:".PadRight(consoleWidth));
        Console.Write(" " + question.ToUpper().PadRight(consoleWidth - 2));
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void RedrawAnswerBar(string answer)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" ANSWER:".PadRight(consoleWidth));
        Console.Write(" {0}", answer.PadRight(consoleWidth));

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void ModifyInfoBar(string question, string answer)
    {
        char heart = '♥';

        Console.SetCursorPosition(0, 0);

        #region Draw Infobar
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(new string(' ', consoleWidth));

        RedrawLivesBar(heart);

        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(new string(' ', consoleWidth));
        Console.Write(new string(' ', consoleWidth));

        // Draw question and answer bars.
        RedrawQuestionBar(question);

        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("".PadRight(consoleWidth));
        Console.WriteLine("".PadRight(consoleWidth));

        RedrawAnswerBar(answer);

        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("".PadRight(consoleWidth));
        Console.Write("".PadRight(consoleWidth));
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;

        // Print left boundary.
        for (int i = 10, k = 0; i < consoleHeight - 1; i++)
        {
            Console.SetCursorPosition(k, i);
            Console.Write(' ');
        }

        // Print bottom boundary.
        Console.SetCursorPosition(0, consoleHeight - 3);
        Console.Write("".PadRight(consoleWidth));
        Console.WriteLine("".PadRight(consoleWidth - 1));
        Console.Write("".PadRight(consoleWidth));
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

    private static void PrintOnPosition(int x, int y, string str, ConsoleColor color)
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
        public int y;
    }
}