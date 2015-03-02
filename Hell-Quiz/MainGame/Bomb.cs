using System;

class Bomb : FallingObject
{
    public Bomb(int xPos, int gameFieldTop)
        : base(xPos, gameFieldTop, "&", ConsoleColor.Red)//call constructor for letter
    {

    }
}
