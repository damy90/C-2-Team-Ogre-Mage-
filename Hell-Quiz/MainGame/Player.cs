using System;

class Player : GameFieldObject
{
    Player(int playerX, int playerY)
    {
        Str = "===";
        Color = ConsoleColor.Red;
        XPosition = playerX;
        YPosition = playerY;
    }

    public void MovePlayer(byte newPosX)
    {
        XPosition = newPosX;
    }
}
