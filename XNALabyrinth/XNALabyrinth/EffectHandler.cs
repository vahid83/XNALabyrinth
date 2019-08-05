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


namespace XNALabyrinth
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class EffectHandler
    {
        #region Fields

        Game game;
        Effect baseEffect;

        #endregion

        #region Properties

        public Effect BaseEffect
        {
            get { return baseEffect; }
            set { baseEffect = value; }
        }

        #endregion

        #region Methods

        public EffectHandler(Game game)
        {
            // TODO: Construct any child components here
            this.game = game;
        }

        public void LoadContent(string fxFileName)
        {
            baseEffect = game.Content.Load<Effect>(fxFileName);
            baseEffect.CurrentTechnique = baseEffect.Techniques["BaseTech"];
            
            // Try to bind textures
        }

        public void ApplyEffect()
        {
            baseEffect.CurrentTechnique.Passes[0].Apply();
        }

        #endregion
    }
}
