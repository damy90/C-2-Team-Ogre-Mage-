using System;

class Player : GameFieldObject
{
    public Player(int playerX, int playerY)
    {
        Str = "===";
        Color = ConsoleColor.Red;
        X = playerX;
        Y = playerY;
    }

    public void MovePlayer(int increment)
    {
        X += increment;
    }
}
