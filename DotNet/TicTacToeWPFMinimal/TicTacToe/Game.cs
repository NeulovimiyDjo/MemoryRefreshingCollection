using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class Game
    {
        public string[] GameGrid { get; set; }
        public int MovesCounter { get; set; }
        public bool Finished { get; set; }
        public int Width { get; set; }
        public int Hight { get; set; }
        public int Victory { get; set; }

        public Game(int w=3, int h=3, int v=3)
        {
            Width = w;
            Hight = h;
            Victory = v;
            GameGrid = new string[Width * Hight];
            MovesCounter = 0;
            Finished = false;
        }

        public bool Check()
        {
            bool res = false;
            int count = 0;

            for (int i = 0; i < Width * Hight; i++)
            {
                count = CheckElementToRight(i) + CheckElementToBottom(i) + CheckElementToRightBottom(i) + CheckElementToRighTop(i);
            
                if (count > 0)
                {
                    res = true;
                    break;
                }
            }
            
            return res;
        }


        private int CheckElementToRight(int n)
        {
            int res = 0;

            if (GameGrid[n] != null && n % Width <= Width - Victory)
            {
                string elem = GameGrid[n];
                bool chainBroken = false;
                for (int i = n + 1; i <= n + Victory - 1; i++)
                {
                    if (elem != GameGrid[i])
                    {
                        chainBroken = true;
                    }
                }
                if (!chainBroken)
                {
                    res = 1;
                }
            }

            return res;
        }

        private int CheckElementToBottom(int n)
        {
            int res = 0;

            if (GameGrid[n] != null && n / Width % Hight <= Width - Victory)
            {
                string elem = GameGrid[n];
                bool chainBroken = false;
                for (int i = n + Width; i <= n + (Victory-1)*Width; i+=Width)
                {
                    if (elem != GameGrid[i])
                    {
                        chainBroken = true;
                    }
                }
                if (!chainBroken)
                {
                    res = 1;
                }
            }

            return res;
        }


        private int CheckElementToRightBottom(int n)
        {
            int res = 0;

            if (GameGrid[n] != null && n % Width <= Width - Victory && n / Width % Hight <= Width - Victory)
            {
                string elem = GameGrid[n];
                bool chainBroken = false;
                for (int i = n + 1 + Width; i <= n + (Victory-1)*(1+Width); i+=1+Width)
                {
                    if (elem != GameGrid[i])
                    {
                        chainBroken = true;
                    }
                }
                if (!chainBroken)
                {
                    res = 1;
                }
            }

            return res;
        }

        private int CheckElementToRighTop(int n)
        {
            int res = 0;

            if (GameGrid[n] != null && n % Width <= Width - Victory && n / Width % Hight >= Victory-1)
            {
                string elem = GameGrid[n];
                bool chainBroken = false;
                for (int i = n + 1 - Width; i >= n + (Victory-1)*(1-Width); i+=1-Width)
                {
                    if (elem != GameGrid[i])
                    {
                        chainBroken = true;
                    }
                }
                if (!chainBroken)
                {
                    res = 1;
                }
            }

            return res;
        }

    }
}
