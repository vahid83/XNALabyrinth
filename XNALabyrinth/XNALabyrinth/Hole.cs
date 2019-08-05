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
    class Hole
    {
        #region Fields

        ModelContainar[] holes;

        #endregion

        #region Methods
        public Hole()
        {
            holes = new ModelContainar[Globals.config.numberOfHoles];

            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                //holes[i] = new ModelContainar(new Vector3(Globals.config.holesXposition[i], 85f, Globals.config.holesZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            }
        }

        public void LoadHoleModel(ContentManager contentManager, string modelName)
        {
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                //holes[i].LoadModels(contentManager, modelName);
            }
        }

        public void DrawHoles()
        {
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                //holes[i].DrawModel();
            }
        }

        public void UpdateViewPerspectiveMatrix(Camera objectLocation)
        {
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                //holes[i].UpdateViewPerspectiveMatrix(objectLocation);
            }
        }

        public void UpdateWorldMatrix(Vector2 delta)
        {
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                //holes[i].UpdateWorldMatrix(delta);
            }
        }

        #endregion
    }
}
