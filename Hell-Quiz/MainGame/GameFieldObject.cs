using System;

    abstract class GameFieldObject
    {
        //TODO position structure
        public int XPosition { get; protected set; }
        public int YPosition { get; protected set; }
        public string Str { get; protected set; }
        public ConsoleColor Color { get; protected set; }
    }

    
