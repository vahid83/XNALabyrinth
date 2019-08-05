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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Scene : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Fields

        Game game;

        public EnvironmentMap envMap;
                
        #endregion

        #region Properties
        public EnvironmentMap EnvMap
        {
            get { return envMap; }
        }
        
        #endregion

        #region Methods

        public Scene(Game game)
            : base(game)
        {            
            this.game = game;
            //envMap = new EnvironmentMap(new Vector3(0, 100, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));   
        }

        public override void Initialize()
        {
            UpdateSceneObjects();
            
            base.Initialize();
        }
        
        protected override void LoadContent()
        {               
            //envMap.LoadContents(game.Content, Globals.config.skySphereFX, Globals.config.envTexCube, Globals.config.envSkyModel);
            
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            envMap.Draw();
            
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateSceneObjects();
            UpdateObjectAngle();

            base.Update(gameTime);
        }

        protected void UpdateSceneObjects()
        {                            
            envMap.UpdateViewPerspectiveMatrix(game.cam);
        }

        protected void UpdateObjectAngle()
        {           
            game.input.Delta = new Vector2(0f, 0f);
        }

        

        #endregion
    }
}
