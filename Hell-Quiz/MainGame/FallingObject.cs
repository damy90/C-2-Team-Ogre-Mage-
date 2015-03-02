using System;

abstract class FallingObject : GameFieldObject
{
    public FallingObject(int xPos, int gameFieldTop, string letter, ConsoleColor color)
    {
        this.X = xPos;
        this.Y = gameFieldTop;
        this.Str = letter;
        this.Color = color;
    }

    public void FallDown()
    {
        this.Y++;
    }

    public override int GetHashCode()
        {
            unchecked
            {
                int result = 17;
                result = result * 23 + ((this.X != null) ? this.X.GetHashCode() : 0);
                result = result * 23 + ((this.Y != null) ? this.Y.GetHashCode() : 0);
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            try
            {
                FallingObject objAsFallingObject = (FallingObject)obj;
                return (this.X == objAsFallingObject.X && this.Y == objAsFallingObject.Y);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
}
