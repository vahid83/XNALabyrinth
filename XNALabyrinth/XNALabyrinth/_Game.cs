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
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields
                
        public GraphicsDeviceManager graphics;

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        public Scene scene;

        public ModelContainar baseTable;
        public ModelContainar table;
        public ModelContainar ball;
        public ModelContainar maze;
        public ModelContainar bomb;
        public ModelContainar[] holes;

        public Camera cam;
        public Input input;

        public ParticleSystem explosionParticles;
        public ParticleSystem explosionSmokeParticles;
        public ParticleSystem projectileTrailParticles;
        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();
        bool isExploded = true;
        #endregion

        #region Properties

     
        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Configuration conf = Content.Load<Configuration>("Config");
            Globals.config = conf;

            explosionParticles = new ParticleSystem(this, Content, "Particle\\ExplosionSettings");
            explosionSmokeParticles = new ParticleSystem(this, Content, "Particle\\ExplosionSmokeSettings");
            projectileTrailParticles = new ParticleSystem(this, Content, "Particle\\ProjectileTrailSettings");

            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;

            scene       = new Scene(this);
            ball        = new ModelContainar(this);
            table       = new ModelContainar(this);
            baseTable   = new ModelContainar(this);
            maze        = new ModelContainar(this);
            bomb        = new ModelContainar(this);
            
            holes = new ModelContainar[Globals.config.numberOfHoles];
            
            Components.Add(scene);
            Components.Add(ball);
            Components.Add(table);
            Components.Add(baseTable);
            Components.Add(maze);
            Components.Add(bomb);

            Components.Add(explosionParticles);
            Components.Add(explosionSmokeParticles);
            Components.Add(projectileTrailParticles);


            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                holes[i] = new ModelContainar(this);
                Components.Add(holes[i]);
            }
        }

        ~Game()
        { }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            table.InitPosition(new Vector3(0, 85, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            baseTable.InitPosition(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            ball.InitPosition(new Vector3(0, 86.5f, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), .2f, .4f, .05f, .05f);
            maze.InitPosition(new Vector3(0, 85, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            bomb.InitPosition(new Vector3(0, 85, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));

            ball.ModelNamePath      = Globals.config.ballPath;
            table.ModelNamePath     = Globals.config.tablePath;
            baseTable.ModelNamePath = Globals.config.baseTablePath;
            maze.ModelNamePath      = Globals.config.mazePath;
            bomb.ModelNamePath      = Globals.config.bombPath;


            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                holes[i].InitPosition(new Vector3(Globals.config.holesXposition[i], 85f, Globals.config.holesZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                holes[i].ModelNamePath = Globals.config.holeModelPath;
            }

            IsMouseVisible = true;

            graphics.PreferMultiSampling = true;           
            //GraphicsDevice.PresentationParameters.MultiSampleCount = 16;
            graphics.ApplyChanges();

            cam = new Camera(GraphicsDevice.Viewport.AspectRatio);
            input = new Input(cam);

            cam.CameraPosition = new Vector3(0f, 300f, 300f);
            cam.CameraLookAt = table.Position;
            cam.CameraUpDir = new Vector3(0f, 1f, 0f);

            ball.IsBall = true;
            baseTable.IsStatic = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            /// These are here since they must be called after envmap init.
            ball.HasCubeMap = true;
            ball.SkyCube = scene.EnvMap.SkyBoxTextureCube;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            cam = input.UpdateWorld();
            cam.UpdateCamera(gameTime);

            input.UpdateObject();

            CheckGameOverCondition();

            /// Particle effect update section
            if (isExploded)
            {
                UpdateExplosions(gameTime);
                isExploded = false;                
            }
            UpdateProjectiles(gameTime);
            
            

            if (input.Quit())
                Exit();
            
            base.Update(gameTime);
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                projectiles.Add(new Projectile(explosionParticles,
                                               explosionSmokeParticles,
                                               projectileTrailParticles));

                timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }

        private void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }

        private void CheckGameOverCondition()
        {
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                float distance = (float)(Math.Sqrt(Math.Pow(ball.Position.X - Globals.config.holesXposition[i], 2) + Math.Pow(ball.Position.Z - Globals.config.holesZposition[i], 2)));

                if (distance < 2)
                    ball.Position = new Vector3(0, 86.5f, 0);
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            explosionParticles.SetCamera(cam.ViewMatrix, cam.PerspectiveMatrix);
            explosionSmokeParticles.SetCamera(cam.ViewMatrix, cam.PerspectiveMatrix);
            projectileTrailParticles.SetCamera(cam.ViewMatrix, cam.PerspectiveMatrix);

            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

        #endregion
    }
}
