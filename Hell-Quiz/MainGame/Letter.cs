using System;

class Letter : GameFieldObject
{
    public Letter(int xPos, int gameFieldTop, string letter)
    {
        X = xPos;
        Y = gameFieldTop;
        Str = letter;
        Color = ConsoleColor.White;
    }

    public void FallDown()
    {
        Y++;
    }
}
