using System;

class Letter : FallingObject
{
    public Letter(int xPos, int gameFieldTop, string letter)
        : base(xPos, gameFieldTop, letter, ConsoleColor.White)
    {

    }
}
