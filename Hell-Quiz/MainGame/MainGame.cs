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

    private static readonly int consoleWidth = 107; //Console.LargestWindowWidth - 60;   <-- depends on the screen resolution and default properties
    private static readonly int consoleHeight = 50; //Console.LargestWindowHeight - 20;

    private static int score = 0;
    static int livesCount = 3;
    static int gameFieldTop = 12;

    static string container; //TODO: container should be used and generated only when drawing. Introduce var playerAnswer
    static string gameOverMessage = "GAME OVER!";
    static int index = 0;   //TODO: Hu? <sarcasm>Verry descriptive!</sarcasm> Dany
    static char[] addLetter;
    static int nextQuestion = random.Next(questions.Count);
    static string correctAnswer = GetAnswer(nextQuestion).ToUpper();

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

        StartGame(DrawNewQuestion(), container);
    }

    static string DrawNewQuestion()
    {
        string question = GetQuestion(nextQuestion);
        container = new string('*', correctAnswer.Length);
        //PrintStartScreen(consoleWidth, consoleHeight); // Timer start.
        return question;
    }

    static void StartGame(string question, string answer)
    {
        bool isGameOver = false;
        ModifyInfoBar(question, answer);
        var randomGenerator = new Random();

        var player = new Player(consoleWidth / 2, consoleHeight - 4);

        PrintOnPosition(player.X, player.Y, player.Str, player.Color);

        var watch = Stopwatch.StartNew();
        var watchBombs = Stopwatch.StartNew();   // Define dropping bombs in given time i.e how frequent will drop new bomb
        var watchLetters = Stopwatch.StartNew(); // Same for letters
        var watchDels = Stopwatch.StartNew();    // Same for deletes

        //TODO use only 1 List<GamefieldObjects>() and let the collision detection figure out which is which
        //this is somewhat stupid
        var fallingObjects = new List<FallingObject>(); //TODO: compare objects by their position, use a data structure that doesn't allow multiple equal(on the same position) objects

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
                int randomXPosition = randomGenerator.Next(2, consoleWidth - 2);
                var bomb = new Bomb(randomXPosition, gameFieldTop);

                fallingObjects.Add(bomb);
                watchBombs.Restart();
            }
            #endregion

            #region Add letters to list
            // Add letters to list
            //if (watchLetters.ElapsedMilliseconds >= 300) // Define how frequent letters are dropping
            //{
            //    int randomXPosition = randomGenerator.Next(2, consoleWidth - 2);
            //    string randomLetter = ((char)correctAnswer[randomGenerator.Next(0, correctAnswer.Length)]).ToString();
            //    var letter = new Letter(randomXPosition, gameFieldTop, randomLetter);

            //    fallingObjects.Add(letter);
            //    watchLetters.Restart();
            //}
            #endregion

            #region Add deletes to list
            // Add dels to list
            if (watchDels.ElapsedMilliseconds >= 5000) // Define how frequent deletes are droping
            {
                var del = new Del(randomGenerator.Next(2, consoleWidth - 2), gameFieldTop);

                fallingObjects.Add(del);
                watchDels.Restart();
            }
            #endregion

            #endregion

            if (watch.ElapsedMilliseconds >= 200)
            {
                int lettersPerRowMax = 2;
                int lettersCountNewRow = random.Next(0, lettersPerRowMax);
                for (int i = 0; i < lettersCountNewRow; i++)
                {
                    int randomXPosition = randomGenerator.Next(2, consoleWidth - 2);
                    string randomLetter = ((char)correctAnswer[randomGenerator.Next(0, correctAnswer.Length)]).ToString();
                    var letter = new Letter(randomXPosition, gameFieldTop, randomLetter);

                    fallingObjects.Add(letter);
                }

                //move everything
                for (int i = 0; i < fallingObjects.Count; i++)
                {
                    if (fallingObjects[i].Y < consoleHeight - 4)
                    {
                        PrintOnPosition(fallingObjects[i].X, fallingObjects[i].Y, " ", ConsoleColor.White);
                        fallingObjects[i].FallDown();
                        PrintOnPosition(fallingObjects[i].X, fallingObjects[i].Y, fallingObjects[i].Str, fallingObjects[i].Color);

                        CollisionDetection(fallingObjects[i], player, ref isGameOver);// TODO: call when player moves
                    }

                    // erace, move out of the field and forget
                    else if (fallingObjects[i].Y == consoleHeight - 4)
                    {
                        PrintOnPosition(fallingObjects[i].X, fallingObjects[i].Y, " ", fallingObjects[i].Color);
                        fallingObjects[i].FallDown();
                    }
                }

                watch.Restart();

                //falling objects garbage collection
                //CAUTION !1!!1ONE!!11 do not delete anything that hasn't been eraced from the console yet
                //Side effects may include Falling Snow Effect (the object sticks to the bottom of the screen)
                if (fallingObjects.Count > 70 && fallingObjects[40].Y >= consoleHeight - 4)
                {
                    fallingObjects.RemoveRange(0, 30);
                }
            }

            //game end handeling
            if (isGameOver)
            {
                container = container.ToUpper();
                if (correctAnswer.Equals(container))
                {
                    score += (answer.Length * 20);
                    Console.SetCursorPosition(0, 1);
                    RedrawLivesBar();
                    Console.ReadLine();
                    // TODO: Overwrite, factor in time, Now what?
                }
                else //lives=0, incorrect answer
                {
                    // Lose game.
                    GameOverScreen();
                    Console.ReadLine();
                    return;
                }
            }
        }   //end while true
    }

    private static void CollisionDetection(FallingObject fallingObject, Player player, ref bool isGameOver)
    {
        if ((fallingObject.Y == consoleHeight - 4) &&
            (fallingObject.X == player.X || fallingObject.X == player.X + 1 || fallingObject.X == player.X + 2))
        {
            //redraw player after collision
            PrintOnPosition(player.X, player.Y, player.Str, player.Color);
            //object already eraced from the console
            fallingObject.FallDown();

            if (fallingObject is Bomb)
            {
                if (livesCount > 1)
                {
                    livesCount--;
                    Console.SetCursorPosition(0, 1);
                    RedrawLivesBar();
                }
                else if (livesCount == 1)
                {
                    livesCount--;
                    isGameOver = true;
                }
            }
            else if (fallingObject is Del)
            {
                if (index != 0)
                {
                    UpdateAnswerWhenDeleteCaught();
                }
                Console.SetCursorPosition(0, 8);
                RedrawAnswerBar(container);     // Put here redrawing only answer bar
            }
            else
            {
                if (container.Trim('*').Length < correctAnswer.Length)
                {
                    UpdateAnswerWhenLetterCaught(fallingObject);
                    Console.SetCursorPosition(0, 8);
                    RedrawAnswerBar(container);

                    if (container.Equals(correctAnswer))
                    {
                        isGameOver = true;
                    }
                }
                else
                {
                    isGameOver = true;
                }
            }
        }
    }

    private static void GameOverScreen()
    {
        Console.Clear();
        string gameOverInfo = "The correct answer is: ";
        PrintOnPosition((consoleWidth / 2) - gameOverMessage.Length + 4, (consoleHeight / 2 - 1), gameOverMessage, ConsoleColor.DarkRed);
        PrintOnPosition(consoleWidth / 2 - gameOverInfo.Length + 6, (consoleHeight / 2), gameOverInfo + correctAnswer, ConsoleColor.DarkYellow);
        Console.SetCursorPosition(consoleWidth / 2 - 15, consoleHeight);


        //writing to the scoreFile
        if (username.Count() == 1 || string.IsNullOrEmpty(username[indexCurrentPlayer + 1]))  // if the player's name is new 
        {
            if (username.Count() != 1)
            {
                username.RemoveAt(indexCurrentPlayer + 1);
            }

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

        InputUsername(Console.ReadLine());

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

    private static void InputUsername(string inputUsername)
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


    private static void UpdateAnswerWhenLetterCaught(FallingObject letter)
    {
        addLetter = container.ToCharArray();
        addLetter[index] = letter.Str[0];
        index++;
        container = string.Join("", addLetter);
    }

    private static void UpdateAnswerWhenDeleteCaught()
    {
        addLetter = container.ToCharArray();
        index--;
        addLetter[index] = '*';
        container = string.Join("", addLetter);
    }

    private static void RedrawLivesBar()
    {
        char heart = '♥';
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
        Console.SetCursorPosition(0, 0);

        #region Draw Infobar
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(new string(' ', consoleWidth));

        RedrawLivesBar();

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
            Console.SetCursorPosition((consoleHeight / 2) - 6, (consoleWidth / 2) - 2);
            Console.WriteLine("STARTING IN: {0}", count);
            Thread.Sleep(1000);
            count--;
        }
        Console.SetCursorPosition((consoleHeight / 2) - 6, (consoleWidth / 2) - 2);
        Console.WriteLine(padding.Append(' ', 14));
    }
}