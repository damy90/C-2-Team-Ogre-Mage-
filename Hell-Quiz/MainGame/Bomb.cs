﻿using System;


    class Bomb : Letter
    {
        Bomb(int gameFieldTop, int xPos)
            : base(gameFieldTop, xPos, "&")//call constructor for letter
        {
            //TODO don't set color twice
            Color = ConsoleColor.Red;
        }
    }