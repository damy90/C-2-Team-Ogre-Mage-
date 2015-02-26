using System;


    class Del:Letter
    {
        public Del(int xPos, int gameFieldTop)
            : base(xPos, gameFieldTop, "<")//call constructor for letter
        {
            //TODO don't set color twice
            Color = ConsoleColor.DarkGreen;
        }
    }
