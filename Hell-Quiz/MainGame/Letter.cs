using System;

    class Letter : GameFieldObject
    {
        public Letter(int gameFieldTop, int xPos, string letter)
        {
            XPosition = xPos;
            YPosition = gameFieldTop;
            Str = letter;
            Color = ConsoleColor.White;
        }

        public void MoveDown()
        {
            YPosition--;
        }
    }
