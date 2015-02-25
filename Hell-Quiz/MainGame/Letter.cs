using System;

    class Letter : GameFieldObject
    {
        public Letter(int gameFieldTop, int xPos, string letter)
        {
            X = xPos;
            Y = gameFieldTop;
            Str = letter;
            Color = ConsoleColor.White;
        }

        public void MoveDown()
        {
            Y--;
        }
    }
