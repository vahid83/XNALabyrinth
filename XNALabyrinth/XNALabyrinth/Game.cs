using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    /// Sample showing how to manage different game states, with transitions
    /// between menu screens, a loading screen, the game itself, and a pause
    /// menu. This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        RenderTargetCube renderTarget;
        
        // By preloading any assets used by UI rendering, we avoid framerate glitches
        // when they suddenly need to be loaded in the middle of a menu transition.
        static readonly string[] preloadAssets =
        {
            "Menu\\gradient",
        };

        public EnvironmentMap environmentMap;

        public ModelContainar baseTable;
        public ModelContainar table;
        public ModelContainar ball;
        public ModelContainar maze;
        public ModelContainar[] bomb;
        public ModelContainar[] hole;
        public ModelContainar[] redButton;
        public ModelContainar[] greenButton;
        public ModelContainar[] checkpoint;
        public ModelContainar[] bombHole;

        public LensFlareComponent lensFlare;

        public Camera cam;
        public Input input;

        public int checkpointIndex;
        public bool isCube;

        public ParticleSystem explosionParticles;
        public ParticleSystem explosionSmokeParticles;
        public ParticleSystem projectileTrailParticles;
        public ParticleSystem smokePlumeParticles;
        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        public List<Projectile> projectiles = new List<Projectile>();
        
        #endregion

        #region Methods
        
        /// <summary>
        /// The main game constructor.
        /// </summary>
        public Game()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            
            Configuration conf = Content.Load<Configuration>("Config");
            Globals.config = conf;

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;

            explosionParticles = new ParticleSystem(this, Content, Globals.config.particleExplosion);
            explosionSmokeParticles = new ParticleSystem(this, Content, Globals.config.particleExplosionSmoke);
            projectileTrailParticles = new ParticleSystem(this, Content, Globals.config.particleProjectileTrail);
            smokePlumeParticles = new ParticleSystem(this, Content, Globals.config.particleSmokePlume);


            smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;

            environmentMap = new EnvironmentMap(this);
            
            table = new ModelContainar(this);
            baseTable = new ModelContainar(this);
            maze = new ModelContainar(this);

            ball = new ModelContainar(this, Globals.config.ballRollingPath, Globals.config.ballHittingPath, string.Empty);

            lensFlare = new LensFlareComponent(this);

            hole = new ModelContainar[Globals.config.numberOfHoles];

            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                hole[i] = new ModelContainar(this);
            }

            bomb = new ModelContainar[Globals.config.numberOfBombs];
            bombHole = new ModelContainar[Globals.config.numberOfBombs];
            redButton = new ModelContainar[Globals.config.numberOfBombs];
            greenButton = new ModelContainar[Globals.config.numberOfBombs];

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                bomb[i] = new ModelContainar(this, Globals.config.explosionTimerPath, Globals.config.explosionPath, Globals.config.neutralizationPath);
                bombHole[i] = new ModelContainar(this);
                redButton[i] = new ModelContainar(this);
                greenButton[i] = new ModelContainar(this);
            }

            checkpoint = new ModelContainar[Globals.config.numberOfCheckpoint];

            for (int i = 0; i < Globals.config.numberOfCheckpoint; i++)
            {
                checkpoint[i] = new ModelContainar(this, Globals.config.checkpointPath, string.Empty, string.Empty);
            }

            
            // Create the screen manager component.
            screenManager = new ScreenManager(this);
            screenManager.DrawOrder = 1000;
            Components.Add(screenManager);

            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(this), null);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            renderTarget = new RenderTargetCube(GraphicsDevice,
                                                512,
                                                true,
                                                GraphicsDevice.PresentationParameters.BackBufferFormat,
                                                GraphicsDevice.PresentationParameters.DepthStencilFormat);

            environmentMap.InitPosition(new Vector3(0, 100, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            table.InitPosition(new Vector3(0, 85, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            baseTable.InitPosition(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            ball.InitPosition(new Vector3(0, 86.5f, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), .2f, .4f, .05f, .05f);
            maze.InitPosition(new Vector3(0, 85, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            
            ball.ModelNamePath = Globals.config.ballPath;
            table.ModelNamePath = Globals.config.tablePath;
            baseTable.ModelNamePath = Globals.config.baseTablePath;
            maze.ModelNamePath = Globals.config.mazePath;
                       
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                hole[i].InitPosition(new Vector3(Globals.config.holesXposition[i], 85f, Globals.config.holesZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                hole[i].ModelNamePath = Globals.config.holeModelPath;
            }

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                bomb[i].InitPosition(new Vector3(Globals.config.bombsXposition[i], 85f, Globals.config.bombsZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                bomb[i].ModelNamePath = Globals.config.bombModelPath;
                bombHole[i].InitPosition(new Vector3(Globals.config.bombsXposition[i], 85f, Globals.config.bombsZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                bombHole[i].ModelNamePath = Globals.config.bombHoleModelPath;
                redButton[i].InitPosition(new Vector3(Globals.config.redButtonXposition[i], 85f, Globals.config.redButtonZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                redButton[i].ModelNamePath = Globals.config.redButtonModelPath;
                greenButton[i].InitPosition(new Vector3(Globals.config.greenButtonXposition[i], 85f, Globals.config.greenButtonZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                greenButton[i].ModelNamePath = Globals.config.greenButtonModelPath;
            }

            for (int i = 0; i < Globals.config.numberOfCheckpoint; i++)
            {
                checkpoint[i].InitPosition(new Vector3(Globals.config.checkpointXposition[i], 85f, Globals.config.checkpointZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                checkpoint[i].ModelNamePath = Globals.config.checkpointModelPath;
            }

            
            IsMouseVisible = true;

            graphics.PreferMultiSampling = true;
            //GraphicsDevice.PresentationParameters.MultiSampleCount = 16;
            graphics.ApplyChanges();

            cam = new Camera(GraphicsDevice.Viewport.AspectRatio);
            input = new Input(cam);

            //cam.CameraPosition = new Vector3(0f, 300f, 300f);
            cam.CameraLookAt = table.Position;
            cam.CameraUpDir = new Vector3(0f, 1f, 0f);


            ball.IsBall = true;
            baseTable.IsStatic = true;

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                bomb[i].IsExploded = true;
                bombHole[i].Visible = false;
            }

            checkpointIndex = -1;

            base.Initialize();
        }

        /// <summary>
        /// Loads graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            foreach (string asset in preloadAssets)
            {
                Content.Load<object>(asset);
            }
            
        }
                
        public void AddGameModels()
        {
            Globals.config.isComponentAdded = true;
            
            
            Components.Add(baseTable);
            Components.Add(ball); 
            Components.Add(table);
            
            Components.Add(maze);

            Components.Add(environmentMap);

            //Components.Add(lensFlare);
            Components.Add(explosionParticles);
            Components.Add(explosionSmokeParticles);
            Components.Add(projectileTrailParticles);
            Components.Add(smokePlumeParticles);
            
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                Components.Add(hole[i]);
            }

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                Components.Add(bomb[i]);
                Components.Add(bombHole[i]);
                Components.Add(redButton[i]);
                Components.Add(greenButton[i]);
            }

            for (int i = 0; i < Globals.config.numberOfCheckpoint; i++)
            {
                Components.Add(checkpoint[i]);
            }

            
            
            ball.HasCubeMap = true;
        }

        public void RemoveGameModels()
        {
            Globals.config.isComponentAdded = false;

            Components.Remove(environmentMap);
            Components.Remove(ball);
            Components.Remove(table);
            Components.Remove(baseTable);
            
            //Components.Remove(lensFlare);
                       
            Components.Remove(explosionParticles);
            Components.Remove(explosionSmokeParticles);
            Components.Remove(projectileTrailParticles);
            Components.Remove(smokePlumeParticles);

            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                Components.Remove(hole[i]);
            }

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                Components.Remove(bomb[i]);
                Components.Remove(bombHole[i]);
                Components.Remove(redButton[i]);
                Components.Remove(greenButton[i]);
            }

            for (int i = 0; i < Globals.config.numberOfCheckpoint; i++)
            {
                Components.Remove(checkpoint[i]);
            }

            Components.Remove(maze);

            //ball.HasCubeMap = true;
            //ball.SkyCube = scene.EnvMap.SkyBoxTextureCube;
        }

        public void ResetGameState(bool isGameOver)
        {
            ball.ExternalAcceleration = Vector2.Zero;

            table.InitPosition(new Vector3(0, 85, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            baseTable.InitPosition(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            maze.InitPosition(new Vector3(0, 85, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            

            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                hole[i].InitPosition(new Vector3(Globals.config.holesXposition[i], 85f, Globals.config.holesZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            }

            for (int i = 0; i < Globals.config.numberOfCheckpoint; i++)
            {
                checkpoint[i].InitPosition(new Vector3(Globals.config.checkpointXposition[i], 85f, Globals.config.checkpointZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            }

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                bomb[i].BombTimer = TimeSpan.Zero;
                bomb[i].IsDirect = bomb[i].IsTrig = false;
                bomb[i].InitPosition(new Vector3(Globals.config.bombsXposition[i], 85f, Globals.config.bombsZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
            }

            cam.CameraLookAt = table.Position;
            cam.InitCamera();

            input.CurrentAngle = new Vector2(0f, 0f);
            input.Delta = new Vector2(0f, 0f);

            if (!isGameOver)
            {
                for (int i = 0; i < Globals.config.numberOfBombs; i++)
                {
                    bombHole[i].InitPosition(new Vector3(Globals.config.bombsXposition[i], 85f, Globals.config.bombsZposition[i]), new Vector3(.5f - (float)new Random(2 * i).NextDouble(), 0, .5f - (float)new Random(2 * i + 1).NextDouble()), new Vector3(0, 1, 0));
                    redButton[i].InitPosition(new Vector3(Globals.config.redButtonXposition[i], 85f, Globals.config.redButtonZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                    greenButton[i].InitPosition(new Vector3(Globals.config.greenButtonXposition[i], 85f, Globals.config.greenButtonZposition[i]), new Vector3(0, 0, 1), new Vector3(0, 1, 0));
                    bomb[i].IsExploded = true;
                    bomb[i].Visible = true;
                    bombHole[i].Visible = false;
                }
                checkpointIndex = -1;
            }

            if (checkpointIndex != -1)
            {
                ball.InitPosition(new Vector3(Globals.config.checkpointXposition[checkpointIndex], 86.5f, Globals.config.checkpointZposition[checkpointIndex]), new Vector3(0, 0, 1), new Vector3(0, 1, 0), .2f, .4f, .05f, .05f);
            }
            else
            {
                ball.InitPosition(new Vector3(-40, 86.5f, -40), new Vector3(0, 0, 1), new Vector3(0, 1, 0), .2f, .4f, .05f, .05f);
            }
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // The real drawing happens inside the screen manager component.
            
            EnvironmentEffect(gameTime);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
            base.Draw(gameTime);
            
        }

        protected void EnvironmentEffect(GameTime gameTime)
        {
            ball.Visible = false;
            Vector3 camPosition = cam.CameraPosition;
            Vector3 camLookAt = cam.CameraLookAt;

            graphics.PreferredBackBufferWidth = 64;
            graphics.PreferredBackBufferHeight = 64;
            cam.FieldOfView = 90f;

            isCube = true;

            cam.CameraPosition = Vector3.Transform(Vector3.Zero, ball.World);

            Matrix reverse = Matrix.Identity;
            
            
            for (int i = 0; i < 6; i++)
            {
                // Set the render target
                GraphicsDevice.SetRenderTarget(renderTarget, CubeMapFace.PositiveX + i);

                switch (i)
                { 
                    case 0:
                        cam.CameraLookAt = Vector3.Right * 100 + cam.CameraPosition;
                        cam.CameraUpDir = Vector3.Up;
                        reverse = Matrix.CreateReflection(new Plane(new Vector4(0, 0, 1, -ball.Position.Z)));
                        
                        break;

                    case 1:
                        cam.CameraLookAt = Vector3.Left * 100 + cam.CameraPosition;
                        cam.CameraUpDir = Vector3.Up;
                        reverse = Matrix.CreateReflection(new Plane(new Vector4(0, 0, 1, -ball.Position.Z)));
                        
                        break;

                    case 2:
                        cam.CameraLookAt = Vector3.Up * 100 + cam.CameraPosition;
                        cam.CameraUpDir = Vector3.Forward;
                        reverse = Matrix.CreateReflection(new Plane(new Vector4(1, 0, 0, -ball.Position.X)));
                        break;

                    case 3:
                        cam.CameraLookAt = Vector3.Down * 100 + cam.CameraPosition;
                        cam.CameraUpDir = Vector3.Forward;
                        reverse = Matrix.CreateReflection(new Plane(new Vector4(0, 0, 1, -ball.Position.Z)));
                        break;

                    case 4:
                        cam.CameraLookAt = Vector3.Backward * 100 + cam.CameraPosition;
                        cam.CameraUpDir = Vector3.Up;
                        reverse = Matrix.CreateReflection(new Plane(new Vector4(1, 0, 0, -ball.Position.X)));
                        break;

                    case 5:
                        cam.CameraLookAt = Vector3.Forward * 100 + cam.CameraPosition;
                        cam.CameraUpDir = Vector3.Up;
                        reverse = Matrix.CreateReflection(new Plane(new Vector4(1, 0, 0, -ball.Position.X)));                        
                        break;                
                }

                cam.UpdateCamera(reverse);

                UpdateCubeObjects();
                // Draw the scene
                base.Draw(gameTime);
            }

            GraphicsDevice.SetRenderTarget(null);
            ball.SkyCube = renderTarget;

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;

            

            cam.CameraLookAt = camLookAt;
            cam.CameraPosition = camPosition;
            cam.CameraUpDir = Vector3.Up;
            cam.FieldOfView = 45f;
            cam.UpdateCamera(gameTime);

            UpdateCubeObjects();
            
            isCube = false;
            ball.Visible = true;
        }

        protected void UpdateCubeObjects()
        {
            environmentMap.UpdateViewPerspectiveMatrix();
            maze.UpdateViewPerspectiveMatrix();
            table.UpdateViewPerspectiveMatrix();
            baseTable.UpdateViewPerspectiveMatrix();

            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                hole[i].UpdateViewPerspectiveMatrix();
            }

            for (int i = 0; i < Globals.config.numberOfCheckpoint; i++)
            {
                checkpoint[i].UpdateViewPerspectiveMatrix();
            }

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                bomb[i].UpdateViewPerspectiveMatrix();
                bombHole[i].UpdateViewPerspectiveMatrix();
                redButton[i].UpdateViewPerspectiveMatrix();
                greenButton[i].UpdateViewPerspectiveMatrix();
            }

            if (explosionParticles.IsContentLoaded)
            {
                explosionParticles.SetCamera(cam.ViewMatrix, cam.PerspectiveMatrix);
                explosionSmokeParticles.SetCamera(cam.ViewMatrix, cam.PerspectiveMatrix);
                projectileTrailParticles.SetCamera(cam.ViewMatrix, cam.PerspectiveMatrix);
                smokePlumeParticles.SetCamera(cam.ViewMatrix, cam.PerspectiveMatrix);
            }
            
        }

        
    #endregion
    
    }
}
