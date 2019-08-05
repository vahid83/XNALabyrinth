using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharedConfig;

namespace XNALabyrinth
{
    public class Maze
    {
        #region Fields

        ModelContainar[] walls;
        List<Vector3> verticesSoup;
        #endregion

        #region Properties
        public List<Vector3> VerticesSoup
        {
            get { return verticesSoup; }
        }
        #endregion

        #region Methods

        public Maze()
        {
            verticesSoup = new List<Vector3>();

            walls = new ModelContainar[Globals.config.numberOfWalls];

            for (int i = 0; i < Globals.config.numberOfWalls; i++)
            {
                //walls[i] = new ModelContainar(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            }

        }

        public void LoadWallModel(ContentManager contentManager, string modelName)
        {            
            int k = 0;
            for(int j = 0; j < 22; j++)
            {
                for (int i = 0; i < 11; i++)
                {
                    if (j % 2 == 0)
                    {
                        if (Globals.mazeMap[j, i] != 0)
                        {
                            //walls[k] = new ModelContainar(new Vector3((i - 5) * Globals.config.wallLenght + Globals.config.wallLenght / 2, 85, (j / 2 - 5) * Globals.config.wallLenght), new Vector3(1, 0, 0), new Vector3(0, 1, 0));
                            //walls[k].LoadModels(contentManager, modelName);
                            //verticesSoup.AddRange(walls[k].VertList);
                            k++;
                        }
                    }
                    else if (j % 2 == 1)
                    {
                        if (Globals.mazeMap[j, i] != 0)
                        {
                            //walls[k] = new ModelContainar(new Vector3((i - 5) * Globals.config.wallLenght, 85, ((j - 1) / 2 - 5) * Globals.config.wallLenght + Globals.config.wallLenght / 2), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                            //walls[k].LoadModels(contentManager, modelName);
                            //verticesSoup.AddRange(walls[k].VertList);
                            k++;
                        }
                    }
                }
            }
        }

        public void DrawMaze()
        {
            for (int i = 0; i < Globals.config.numberOfWalls; i++)
            {
                //walls[i].DrawModel();
            }
        }

        public void UpdateViewPerspectiveMatrix(Camera objectLocation)
        {
            for (int i = 0; i < Globals.config.numberOfWalls; i++)
            {
                //walls[i].UpdateViewPerspectiveMatrix(objectLocation);
            }
        }

        public void UpdateWorldMatrix(Vector2 delta)
        {
            for (int i = 0; i < Globals.config.numberOfWalls; i++)
            {
                //walls[i].UpdateWorldMatrix(delta);
            }
        }

        #endregion

    }
}
