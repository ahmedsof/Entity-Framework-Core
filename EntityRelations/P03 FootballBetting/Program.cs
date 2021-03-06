﻿using System;
using P03_FootballBetting.Data;

namespace P03_FootballBetting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var context = new FootballBettingContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
