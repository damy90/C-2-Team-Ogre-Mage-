﻿using System;


    class Del:Letter
    {
        Del(int gameFieldTop, int xPos)
            : base(gameFieldTop, xPos, "&")//call constructor for letter
        {
            //TODO don't set color twice
            Color = ConsoleColor.DarkGreen;
        }
    }