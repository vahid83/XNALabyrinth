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
    public class EnvironmentMap : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Fields

        Game game;

        Effect skySphereEffect;
        TextureCube skyBoxTextureCube;
        Model skySphereModel;

        Matrix world;
        Matrix view;
        Matrix perspective;
        #endregion

        #region Properties
        public TextureCube SkyBoxTextureCube
        {
            get { return skyBoxTextureCube; }
            set { skyBoxTextureCube = value; }
        }
        #endregion

        #region Methods
        public EnvironmentMap(Game game)
            : base(game)
        {            
            this.game = game;

        }

        public void InitPosition(Vector3 position, Vector3 forward, Vector3 up)
        {
            world = Matrix.CreateWorld(position, forward, up);
        }

        protected override void LoadContent()
        {
            skySphereEffect = game.Content.Load<Effect>(Globals.config.skySphereFX);
            skyBoxTextureCube = game.Content.Load<TextureCube>(Globals.config.envTexCube);
            skySphereModel = game.Content.Load<Model>(Globals.config.envSkyModel);

            InitEffectsParameters();
        }

        protected void InitEffectsParameters()
        {
            skySphereEffect.Parameters["WorldMatrix"].SetValue(world);
            skySphereEffect.Parameters["ViewMatrix"].SetValue(view);
            skySphereEffect.Parameters["ProjectionMatrix"].SetValue(perspective);

            skySphereEffect.Parameters["SkyboxTexture"].SetValue(skyBoxTextureCube);

            foreach (ModelMesh mesh in skySphereModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = skySphereEffect;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            UpdateViewPerspectiveMatrix();

            base.Update(gameTime);        
        }

        public void UpdateViewPerspectiveMatrix()
        {            
            view = game.cam.ViewMatrix;
            perspective = game.cam.PerspectiveMatrix;
        }

        public override void Draw(GameTime gameTime)
        {
            skySphereEffect.Parameters["WorldMatrix"].SetValue(world);
            skySphereEffect.Parameters["ViewMatrix"].SetValue(view);
            skySphereEffect.Parameters["ProjectionMatrix"].SetValue(perspective);
            // Draw the sphere model that the effect projects onto
            foreach (ModelMesh mesh in skySphereModel.Meshes)
            {
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
        #endregion
    }
}
