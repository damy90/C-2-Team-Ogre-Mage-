using System;

class FallingObject : GameFieldObject
{
    public FallingObject(int xPos, int gameFieldTop, string letter, ConsoleColor color)
    {
        X = xPos;
        Y = gameFieldTop;
        Str = letter;
        Color = color;
    }

    public void FallDown()
    {
        Y++;
    }
}
