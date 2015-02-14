using System;
using System.Threading;

class FallingRocks
{
    //Implement the "Falling Rocks" game in the text console.
    //*A small dwarf stays at the bottom of the screen and can move left and right (by the arrows keys).
    //*A number of rocks of different sizes and forms constantly fall down and you need to avoid a crash.
    //*Rocks are the symbols ^, @, *, &, +, %, $, #, !, ., ;, - distributed with appropriate density. The dwarf is (O).
    //Ensure a constant game speed by Thread.Sleep(150).
    //*Implement collision detection and scoring system.

    //The game doesn't blink because it doesn't use Console.Clear();
    static void Main()
    {
        //initialize
        Random random = new Random();
        int width = 40,
            heigth = 20,
            pos = 30,
            row = 0,
            waitTime = 150,
            score = 0;
        char[][] playfield = new char[heigth][];
        char[] rocks = { '^', '@', '*', '&', '+', '%', '$', '#', '!', '.', ';', '-' };

        for (int i = 0; i < heigth; i++)
        {
            playfield[i] = new char[width];
            for (int j = 0; j < width; j++)
            {
                playfield[i][j] = ' ';
            }
        }

        //game loop
        while (true)
        {
            //read keyboard buffer
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey();
                if (!Console.KeyAvailable)
                {
                    if (pressedKey.Key == ConsoleKey.RightArrow && pos < width - 4)
                        pos += 1;
                    if (pressedKey.Key == ConsoleKey.LeftArrow && pos > 0)
                        pos -= 1;
                }
            }

            //faster speed for increasing difficulty
            if (row == 0 && waitTime > 10)
            {
                waitTime -= 5;
            }

            score++;

            //generate the new line
            for (int col = 0; col < width; col++)
            {
                if (random.Next(0, 200) == 0)
                {
                    int size = random.Next(1, 4);
                    int stone = random.Next(0, rocks.Length);
                    for (int k = 0; k < size && col < width; k++, col++)
                    {
                        playfield[row][col] = rocks[stone];
                    }
                }
                else
                {
                    playfield[row][col] = ' ';
                }
            }

            //collision detection
            int bottomRow = (row == heigth - 1) ? 0 : row + 1;
            if (playfield[bottomRow][pos] != ' ' || playfield[bottomRow][pos + 1] != ' ' || playfield[bottomRow][pos + 2] != ' ')
            {
                break;
            }

            //draw player
            playfield[bottomRow][pos] = '(';
            playfield[bottomRow][pos + 1] = '0';
            playfield[bottomRow][pos + 2] = ')';
            Console.SetCursorPosition(0, 5);

            //draw scene
            for (int j = 0, n = row; j < heigth; j++)
            {
                Console.WriteLine(playfield[n]);
                if (n == 0)
                {
                    n = heigth - 1;
                }
                else
                {
                    n--;
                }
            }

            row = bottomRow;
            Console.WriteLine("score: {0}", score);
            Thread.Sleep(waitTime);
        }

        Console.WriteLine("Game over!!!");
    }
}