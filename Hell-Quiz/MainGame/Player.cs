using System;

class Player : GameFieldObject
{
    public Player(int playerX, int playerY)
    {
        this.Str = "===";
        this.Color = ConsoleColor.Red;
        this.X = playerX;
        this.Y = playerY;
    }

    public void MovePlayer(int increment)
    {
        this.X += increment;
    }
}
