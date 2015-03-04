using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;

internal class MainGame
{
    private static readonly Random random = new Random(); // Generator for pulling random questions.

    private static List<string> questions = ReadQuestionsFromFile();   // Load all questions from file.

    private static List<string> answers = ReadAnswersFromFile();       // Load all answers from file.

    private static readonly int consoleWidth = 107;  //Console.LargestWindowWidth - 60;   <-- depends on the screen resolution and default properties
    private static readonly int consoleHeight = 50;  //Console.LargestWindowHeight - 20;

    private static int score = 0;
    private static int livesCount = 3;
    static readonly int gameFieldTop = 12;

    private static string container;
    private static readonly string gameOverMessage = "GAME OVER!";
    private static bool isGameOver = false;

    private static int indexOfCatchedLetter = 0;
    private static char[] addLetter;
    private static int nextQuestion;
    private static string correctAnswer;

    private static int oldPosition;
    private static int indexCurrentPlayer;

    private static readonly string pathHistory = @"..\..\Data\username.txt";
    private static List<string> username;
    private static readonly SoundPlayer soundPlayer = new System.Media.SoundPlayer(@"..\..\Data\Sounds\DropThat.wav");

    static void Main(string[] args)
    {
        Console.SetWindowSize(consoleWidth, consoleHeight);
        Console.SetBufferSize(consoleWidth, consoleHeight + 1); //+10
        Console.CursorVisible = false;

        CreateFile();

        StartScreen();
        ShowTimer(consoleHeight, consoleWidth);
        StartGame(DrawNewQuestion(), container);
    }

    static string DrawNewQuestion()
    {
        nextQuestion = random.Next(answers.Count);
        int num = nextQuestion;
        string question = GetQuestion(num);
        correctAnswer = GetAnswer(num).ToUpper();
        container = new string('*', correctAnswer.Length);
        return question;
    }

    static void StartGame(string question, string answer)
    {
        int lettersPerRowMax = 2;
        int bonusCharsPerRowMax = 1;

        ModifyInfoBar(question, answer);

        var player = new Player(consoleWidth / 2, consoleHeight - 4);

        PrintOnPosition(player.X, player.Y, player.Str, player.Color);

        var fallingObjects = new List<FallingObject>(); //TODO: compare objects by their position, use a data structure that doesn't allow multiple equal(on the same position) objects

        var watch = Stopwatch.StartNew();

        try
        {
            soundPlayer.Load();
        }
        catch
        {
            //ignore the lack of music
        }

        while (true) //TODO: add endGame condition here
        {
            KeyboardCommands(player);

            //next game step
            if (watch.ElapsedMilliseconds >= 200)
            {
                watch.Restart();
                //move everything
                foreach (var fallingObject in fallingObjects)
                {
                    if (fallingObject.Y < consoleHeight - 4)
                    {
                        PrintOnPosition(fallingObject.X, fallingObject.Y, " ", ConsoleColor.White);//clean previous position
                        fallingObject.FallDown();
                        PrintOnPosition(fallingObject.X, fallingObject.Y, fallingObject.Str, fallingObject.Color);

                        CollisionDetection(fallingObject, player);// TODO: call when player moves
                    }

                    // erace, move out of the field and forget
                    else if (fallingObject.Y == consoleHeight - 4)
                    {
                        PrintOnPosition(fallingObject.X, fallingObject.Y, " ", fallingObject.Color);
                        fallingObject.FallDown();
                    }
                }

                //falling objects garbage collection
                //CAUTION !1!!1ONE!!11 do not delete anything that hasn't been eraced from the console yet
                //Side effects may include Falling Snow Effect (the object sticks to the bottom of the screen)
                if (fallingObjects.Count > 70 && fallingObjects[40].Y >= consoleHeight - 4)
                {
                    fallingObjects.RemoveRange(0, 30);
                }

                //generated objects must not be on the same coordinates
                //hashsets adding directly to the list would break the garbage collection
                HashSet<FallingObject> fallingObjectsNewRow = new HashSet<FallingObject>();
                RandomFallingObjectsGenerator(fallingObjectsNewRow, lettersPerRowMax, correctAnswer.ToArray());
                RandomFallingObjectsGenerator(fallingObjectsNewRow, bonusCharsPerRowMax, new char[] { '&', '&', '&', '<' });//The ratio of occurances in the game depends on the ratio in the array
                fallingObjects.AddRange(fallingObjectsNewRow);
            }

            //game end handeling
            #region Game end handling
            if (container[container.Length - 1] != '*')
            {
                container = container.ToUpper();
                if (container.Equals(correctAnswer) && questions.Count >= 1 && answers.Count >= 1)
                {
                    score += (correctAnswer.Length * 20);
                    livesCount++;
                    indexOfCatchedLetter = 0;
                    Console.SetCursorPosition(0, 1);
                    RedrawLivesBar();
                    Console.SetCursorPosition(0, 4);
                    RedrawQuestionBar(DrawNewQuestion());
                    Console.SetCursorPosition(0, 8);
                    RedrawAnswerBar(container);
                    isGameOver = false;
                }
                else if (container.Equals(correctAnswer) && (questions.Count == 0 || answers.Count == 0))
                {
                    score += (correctAnswer.Length * 20);
                    livesCount++;
                    Console.SetCursorPosition(0, 1);
                    RedrawLivesBar();
                    isGameOver = true;
                }
                else if ((!container.Equals(correctAnswer) && (questions.Count == 0 || answers.Count == 0)))
                {
                    livesCount--;
                    Console.SetCursorPosition(0, 1);
                    RedrawLivesBar();
                    isGameOver = true;
                }
                else if (questions.Count >= 1 && answers.Count >= 1)
                {
                    indexOfCatchedLetter = 0;
                    livesCount--;
                    if (livesCount == 0)
                    {
                        Console.SetCursorPosition(0, 1);
                        RedrawLivesBar();
                        isGameOver = true;
                    }
                    else
                    {
                        Console.SetCursorPosition(0, 1);
                        RedrawLivesBar();
                        Console.SetCursorPosition(0, 4);
                        RedrawQuestionBar(DrawNewQuestion());
                        Console.SetCursorPosition(0, 8);
                        RedrawAnswerBar(container);
                        isGameOver = false;
                    }
                }
                else
                {
                    isGameOver = true;
                }
            }

            if (isGameOver)
            {
                container = container.ToUpper();
                if (correctAnswer.Equals(container))
                {
                    GameOverScreen();
                    soundPlayer.Dispose();
                    Console.ReadLine();
                    return;
                }
                else if (livesCount == 0)
                {
                    GameOverScreen();
                    soundPlayer.Dispose();
                    Console.ReadLine();
                    return;
                }
            }
            #endregion
        }   //end while true
    }

    private static void KeyboardCommands(Player player)
    {
        ConsoleKeyInfo pressedKey;

        while (Console.KeyAvailable)
        {
            pressedKey = Console.ReadKey(true);
            oldPosition = player.X;

            if (pressedKey.Key == ConsoleKey.LeftArrow)
            {
                if ((player.X - 1) >= 1) // >= 1 Because of the boundaries of the user interface.
                {
                    player.MovePlayer(-1);
                    PrintOnPosition(oldPosition + 2, player.Y, " ", player.Color);
                }
            }
            else if (pressedKey.Key == ConsoleKey.RightArrow)
            {
                if (player.X + 2 < (consoleWidth - 2)) // < ConsoleWidth - 2, because of the boundaries of the user interface.
                {
                    player.MovePlayer(1);
                    PrintOnPosition(oldPosition, player.Y, " ", player.Color);
                }
            }
            if (pressedKey.Key == ConsoleKey.Q)
            {
                if (questions.Count >= 1 && answers.Count >= 1)
                {
                    indexOfCatchedLetter = 0;
                    Console.SetCursorPosition(0, 4);
                    RedrawQuestionBar(DrawNewQuestion());
                    Console.SetCursorPosition(0, 8);
                    RedrawAnswerBar(container);
                }
            }
            if (pressedKey.Key == ConsoleKey.P)
            {
                soundPlayer.Play();
            }
            if (pressedKey.Key == ConsoleKey.S)
            {
                soundPlayer.Stop();
            }
            if (pressedKey.Key == ConsoleKey.T)  //Only for the presentation. REMOVE AFTER
            {
                PauseGame();
            }
            PrintOnPosition(player.X, player.Y, player.Str, player.Color);
        }
    }

    private static void RandomFallingObjectsGenerator(HashSet<FallingObject> fallingObjectsNewRow, int lettersPerRowMax, char[] characters)
    {
        int lettersCountNewRow = random.Next(0, lettersPerRowMax + 1);

        for (int i = 0; i < lettersCountNewRow; i++)
        {
            int randomXPosition = random.Next(2, consoleWidth - 2);
            string randomChar = (characters[random.Next(0, characters.Length)]).ToString();
            FallingObject fallingObject;

            if (randomChar.Equals("&"))
            {
                fallingObject = new Bomb(randomXPosition, gameFieldTop);
            }
            else if (randomChar.Equals("<"))
            {
                fallingObject = new Del(randomXPosition, gameFieldTop);
            }
            else
            {
                fallingObject = new Letter(randomXPosition, gameFieldTop, randomChar);
            }

            fallingObjectsNewRow.Add(fallingObject);
            //fallingObjectsNewRow.
        }
    }

    private static void CollisionDetection(FallingObject fallingObject, Player player)
    {
        if ((fallingObject.Y == consoleHeight - 4) &&
            (fallingObject.X == player.X || fallingObject.X == player.X + 1 || fallingObject.X == player.X + 2))
        {
            PrintOnPosition(player.X, player.Y, player.Str, player.Color);  // Redraw player after collision
            fallingObject.FallDown();   // Object removed from the playfield.

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
                    Console.SetCursorPosition(0, 1);
                    RedrawLivesBar();
                    isGameOver = true;
                }
            }
            else if (fallingObject is Del)
            {
                if (indexOfCatchedLetter != 0)
                {
                    UpdateAnswerWhenDeleteCaught();
                }
                Console.SetCursorPosition(0, 8);
                RedrawAnswerBar(container);
            }
            else if (fallingObject is Letter)
            {
                if (indexOfCatchedLetter < correctAnswer.Length)
                {
                    UpdateAnswerWhenLetterCaught(fallingObject);
                }
                Console.SetCursorPosition(0, 8);
                RedrawAnswerBar(container);
            }
        }
    }

    private static void GameOverScreen()
    {
        Console.Clear();
        string gameOverInfo = "The correct answer is: ";
        string scoreInfo = "Your current score is: ";
        PrintOnPosition((consoleWidth / 2) - gameOverMessage.Length / 2, (consoleHeight / 2 - 2), gameOverMessage, ConsoleColor.DarkRed);
        PrintOnPosition(consoleWidth / 2 - gameOverInfo.Length / 2 - 3, (consoleHeight / 2), gameOverInfo + correctAnswer, ConsoleColor.DarkYellow);
        PrintOnPosition(consoleWidth / 2 - scoreInfo.Length / 2 - 1, (consoleHeight / 2 + 2), scoreInfo + score, ConsoleColor.DarkYellow);

        Console.SetCursorPosition(consoleWidth / 2 - 15, consoleHeight);

        // Writing to the scoreFile
        if (username.Count() == 1)  // If the player name is new.
        {
            if (username.Count() != 1)
            {
                username.RemoveAt(indexCurrentPlayer + 1);
            }

            username.Add(score.ToString());
            File.WriteAllLines(pathHistory, username);
        }
        //if indexcurrentplayer + 1 is int 

        if (score > int.Parse(username[indexCurrentPlayer + 1]))
        {
            username.RemoveAt(indexCurrentPlayer + 1);
            username.Insert(indexCurrentPlayer + 1, score.ToString());
            File.WriteAllLines(pathHistory, username);
        }
        PrintHightScore();
        Console.SetCursorPosition(consoleWidth / 2 - 15, consoleHeight);
    }

    private static void CreateFile()
    {
        try
        {
            if (File.Exists(pathHistory))
            {
                username = (File.ReadAllLines(@"..\..\Data\username.txt")).ToList();
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
        string gameDescription = "Game Description:";
        string enterPlayerName = "Enter username:";
        string changeQuestionInfo = "PRESS \"Q\" if you would like to change the question manually.";
        string audioInfo = "Press \"P\" to play some music and \"S\" to stop it.";
        string enjoyTheGame = "Enjoy the game!";
        string howToPlay = "Answer the question.";
        string howToPlay2 = "Collect falling letters in order, to form the answer.";
        string howToPlay3 = "Catch a “backspace” symbol to delete the last letter.";
        string howToPlay4 = "Beware the bombs.";

        // Adding Username
        Console.SetCursorPosition(consoleWidth / 2 - enterPlayerName.Length / 2, consoleHeight / 2 - 10);
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(enterPlayerName);
        Console.SetCursorPosition(consoleWidth / 2, consoleHeight / 2 - 9);

        InputUsername(ReadLimitedName());

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        CreateGameDescriptionFrame();
        Console.SetCursorPosition(consoleWidth / 2 - gameDescription.Length / 2, (consoleHeight / 2) - 6);
        Console.WriteLine(gameDescription);
        Console.SetCursorPosition(consoleWidth / 2 - howToPlay.Length / 2, (consoleHeight / 2) - 4);
        Console.WriteLine(howToPlay);

        Console.SetCursorPosition(consoleWidth / 2 - howToPlay2.Length / 2, (consoleHeight / 2) - 2);
        Console.WriteLine(howToPlay2);

        Console.SetCursorPosition(consoleWidth / 2 - howToPlay3.Length / 2, (consoleHeight / 2));
        Console.WriteLine(howToPlay3);

        Console.SetCursorPosition(consoleWidth / 2 - changeQuestionInfo.Length / 2, (consoleHeight / 2) + 2);
        Console.WriteLine(changeQuestionInfo);

        Console.SetCursorPosition(consoleWidth / 2 - audioInfo.Length / 2, (consoleHeight / 2) + 4);
        Console.WriteLine(audioInfo);

        Console.SetCursorPosition(consoleWidth / 2 - howToPlay4.Length / 2, (consoleHeight / 2) + 6);
        Console.WriteLine(howToPlay4);

        Console.SetCursorPosition(consoleWidth / 2 - enjoyTheGame.Length / 2, (consoleHeight / 2) + 8);
        Console.WriteLine(enjoyTheGame);

        Console.ReadKey();
        Console.Clear();
    }

    private static void CreateGameDescriptionFrame()
    {
        for (int i = (consoleWidth / 2 - 32), k = 0; k <= 61; i++, k++)
        {
            Console.SetCursorPosition(i, (consoleHeight / 2) - 7);
            Console.Write("*");
        }
        for (int i = (consoleHeight / 2) - 7, k = 0; k < 17; i++, k++)
        {
            Console.SetCursorPosition((consoleWidth / 2 - 32), i);
            Console.Write("*");
        }
        for (int i = (consoleWidth / 2 - 31), k = 0; k <= 61; i++, k++)
        {
            Console.SetCursorPosition(i, (consoleHeight / 2) + 9);
            Console.Write("*");
        }
        for (int i = (consoleHeight / 2) - 7, k = 0; k < 17; i++, k++)
        {
            Console.SetCursorPosition((consoleWidth / 2 + 30), i);
            Console.Write("*");
        }
    }

    private static void InputUsername(string inputUsername)
    {
        if (username.Contains(inputUsername))
        {
            indexCurrentPlayer = username.IndexOf(inputUsername);

            int isInteger;

            if (username.Count > indexCurrentPlayer)
            {
                if (!(int.TryParse(username[(indexCurrentPlayer + 1)], out isInteger)))
                {
                    username.Insert(indexCurrentPlayer + 1, "0");
                    File.WriteAllLines(pathHistory, username);
                }
            }
            else if (indexCurrentPlayer + 1 == username.Count)
            {
                username.Insert(indexCurrentPlayer + 1, "0");
                File.WriteAllLines(pathHistory, username);
            }
        }
        else
        {
            username.Add(inputUsername);
            username.Add("0");
            indexCurrentPlayer = username.IndexOf(inputUsername);
            File.WriteAllLines(pathHistory, username);
        }
    }

    private static void PrintHightScore()
    {
        int highScorePosX = consoleWidth / 2;
        int hightScorePosY = consoleHeight / 2 + 5;
        string highScoreTitle = "HIGH SCORES";
        PrintOnPosition(highScorePosX - (highScoreTitle.Length / 2), hightScorePosY, highScoreTitle, ConsoleColor.DarkRed);

        PrintOnPosition(highScorePosX - (highScoreTitle.Length / 2), ++hightScorePosY, new string('=', highScoreTitle.Length), ConsoleColor.DarkRed);

        var reader = new StreamReader(@"..\..\Data\username.txt");
        var lineUser = reader.ReadLine();
        var lineScores = reader.ReadLine();
        while (lineUser != null || lineScores != null)
        {
            PrintOnPosition(highScorePosX - lineUser.Length,
                ++hightScorePosY,
             lineUser + " : " + lineScores, ConsoleColor.DarkYellow);

            lineUser = reader.ReadLine();
            lineScores = reader.ReadLine();
        }
    }

    private static void UpdateAnswerWhenLetterCaught(FallingObject letter)
    {
        addLetter = container.ToCharArray();
        addLetter[indexOfCatchedLetter] = letter.Str[0];
        indexOfCatchedLetter++;
        container = string.Join("", addLetter);
    }

    private static void UpdateAnswerWhenDeleteCaught()
    {
        addLetter = container.ToCharArray();
        indexOfCatchedLetter--;
        addLetter[indexOfCatchedLetter] = '*';
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

    private static string GetQuestion(int nextQuestion) // Gets the number of the question to be displayed.
    {
        string question = questions.ElementAt(nextQuestion);
        questions.RemoveAt(nextQuestion);
        return question;
    }

    private static string GetAnswer(int nextAnswer) // Gets the number of the answer.
    {
        string answer = answers.ElementAt(nextAnswer);
        answers.RemoveAt(nextAnswer);
        return answer;
    }

    private static List<string> ReadQuestionsFromFile()
    {
        string filePath = @"..\..\Data\questions.txt";
        questions = FileReader(filePath);

        return questions;
    }

    private static List<string> ReadAnswersFromFile()
    {
        string filePath = @"..\..\Data\answers.txt";
        answers = FileReader(filePath);

        return answers;
    }

    private static List<string> FileReader(string filePath)
    {
        // Read the file line by line.
        List<string> output = new List<string>();
        string line;
        try
        {
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                output.Add(line);
            }
        }
        catch (IOException exc)
        {
            Console.WriteLine("Invalid file path.");
            Console.WriteLine(exc.ToString());
        }
        return output;
    }

    private static string ReadLimitedName()
    {
        string inputName = string.Empty;
        do
        {
            char c = Console.ReadKey().KeyChar;
            if (c == '\n' || c == '\r')
            {
                break;
            }
            else
            {
                inputName += c;
            }
        } while (inputName.Length < 20);

        return inputName;
    }

    private static void ShowTimer(int consoleWidth, int consoleHeight)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
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

    private static void PauseGame()
    {
        Thread.Sleep(5000);
    }
}