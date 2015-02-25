using System;

    abstract class GameFieldObject
    {
        //TODO position structure
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public string Str { get; protected set; }
        public ConsoleColor Color { get; protected set; }
    }

    
