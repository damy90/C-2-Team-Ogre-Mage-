using System;

class Bomb : Letter
{
    public Bomb(int xPos, int gameFieldTop)
        : base(xPos, gameFieldTop, "&")//call constructor for letter
    {
        //TODO don't set color twice
        Color = ConsoleColor.Red;
    }
}
