using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNALabyrinth
{
    static class Global
    {
        public const float gAngleLimit = MathHelper.Pi * .05f;
        public const float gFrameTime = 0.05f;
        public const float ballRadius = 1.5f;
        public const float wallWidth = 3;
        public const float wallLenght = 9;
        public static int[,] mazeMap = {{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                                        {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                                        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
                                        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};
    }
}
