#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharedConfig;
#endregion

namespace XNALabyrinth
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields
        Game game;

        float pauseAlpha;
        
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(Game game)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            this.game = game;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            
            if (!Globals.config.isComponentAdded)
            {
                game.AddGameModels();
            }
            game.ResetGameState(false);
           

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            //Thread.Sleep(100);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            //content.Unload();
        }


        #endregion

        #region Update and Draw
        
        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);


            game.cam = game.input.UpdateWorld();
            game.cam.UpdateCamera(gameTime);

            CheckExplosionCondition(gameTime);

            /// Particle effect update section
            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                if (!game.bomb[i].IsExploded && game.bomb[i].Visible)
                {
                    if (game.bomb[i].BombTimer > TimeSpan.FromSeconds(3) || game.bomb[i].IsDirect)
                    {
                        if (!game.bomb[i].IsDirect)
                            game.bomb[i].Sound1.Stop();

                        game.bomb[i].Sound2.Play();

                        Globals.config.shaking = true;
                        Globals.config.shakeTimer = 0;
                        game.ball.ExternalAcceleration = Physics.ExplosionForce(game.ball, game.bomb[i].Position);

                        Globals.timeToNextProjectile = TimeSpan.Zero;
                        UpdateExplosions(gameTime, i);
                        UpdateProjectiles(gameTime);
                        
                        game.bomb[i].IsExploded = true;
                        game.bomb[i].Visible = false;
                        game.bombHole[i].Visible = true;
                    }
                }

                if (game.bomb[i].IsNeutralized && game.bomb[i].Visible)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        UpdateSmokePlume(Vector3.Transform(new Vector3(0, 0, 0), game.bomb[i].World));
                    }
                    game.bomb[i].Sound3.Play();
                    UpdateProjectiles(gameTime);
                    game.bomb[i].IsNeutralized = false;
                    game.bomb[i].Visible = false;
                }
                    
            }

            CheckReachCheckpoint();

            CheckGameOverCondition();
        }

        private void UpdateExplosions(GameTime gameTime, int i)
        {
            Globals.timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (Globals.timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                Vector3 explosionPosition = Vector3.Transform(new Vector3(0, 0, 0), game.bomb[i].World);
                game.projectiles.Add(new Projectile(game.explosionParticles,
                                                    game.explosionSmokeParticles,
                                                    game.projectileTrailParticles,
                                                    explosionPosition));

                Globals.timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }

        private void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < game.projectiles.Count)
            {
                if (!game.projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    game.projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }

        void UpdateSmokePlume(Vector3 position)
        {
            // This is trivial: we just create one new smoke particle per frame.
            game.smokePlumeParticles.AddParticle(position, Vector3.Zero);
        }

        private void CheckGameOverCondition()
        {
            for (int i = 0; i < Globals.config.numberOfHoles; i++)
            {
                float distance = (float)(Math.Sqrt(Math.Pow(game.ball.Position.X - Globals.config.holesXposition[i], 2) + Math.Pow(game.ball.Position.Z - Globals.config.holesZposition[i], 2)));

                if (distance < 2)
                {
                    game.ResetGameState(true);
                }
            }

            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                if (game.bombHole[i].Visible == true)
                {
                    float distance = (float)(Math.Sqrt(Math.Pow(game.ball.Position.X - Globals.config.bombsXposition[i], 2) + Math.Pow(game.ball.Position.Z - Globals.config.bombsZposition[i], 2)));

                    if (distance < 2)
                    {
                        game.ResetGameState(true);
                    }

                }
            }

        }

        private void CheckReachCheckpoint()
        {
            for (int i = 0; i < Globals.config.numberOfCheckpoint; i++)
            {
                if (game.checkpointIndex != i)
                {
                    float distance = (float)(Math.Sqrt(Math.Pow(game.ball.Position.X - Globals.config.checkpointXposition[i], 2) + Math.Pow(game.ball.Position.Z - Globals.config.checkpointZposition[i], 2)));

                    if (distance < 2)
                    {
                        game.checkpoint[i].Sound1.Play();
                        game.checkpointIndex = i;
                    }
                }
            }
        }

        private void CheckExplosionCondition(GameTime gameTime)
        {
            for (int i = 0; i < Globals.config.numberOfBombs; i++)
            {
                float distance = (float)(Math.Sqrt(Math.Pow(game.ball.Position.X - Globals.config.redButtonXposition[i], 2) + Math.Pow(game.ball.Position.Z - Globals.config.redButtonZposition[i], 2)));
                if (distance < 2 && game.bomb[i].Visible)
                {
                    game.redButton[i].Position = new Vector3(game.redButton[i].Position.X, 84.55f, game.redButton[i].Position.Z);
                    game.greenButton[i].Position = new Vector3(game.greenButton[i].Position.X, 84.55f, game.greenButton[i].Position.Z);

                    if (!game.bomb[i].IsTrig)
                    {
                        game.bomb[i].Sound1.Play();
                        game.bomb[i].IsTrig = true;
                        game.bomb[i].IsExploded = false;
                    }

                }

                distance = (float)(Math.Sqrt(Math.Pow(game.ball.Position.X - Globals.config.greenButtonXposition[i], 2) + Math.Pow(game.ball.Position.Z - Globals.config.greenButtonZposition[i], 2)));
                if (distance < 2 && game.bomb[i].Visible)
                {
                    game.greenButton[i].Position = new Vector3(game.greenButton[i].Position.X, 84.55f, game.greenButton[i].Position.Z);
                    game.redButton[i].Position = new Vector3(game.redButton[i].Position.X, 84.55f, game.redButton[i].Position.Z);

                    game.bomb[i].IsNeutralized = true;
                }

                distance = (float)(Math.Sqrt(Math.Pow(game.ball.Position.X - Globals.config.bombsXposition[i], 2) + Math.Pow(game.ball.Position.Z - Globals.config.bombsZposition[i], 2)));
                if (distance < 2 && game.bomb[i].Visible)
                {
                    game.redButton[i].Position = new Vector3(game.redButton[i].Position.X, 84.55f, game.redButton[i].Position.Z);
                    game.greenButton[i].Position = new Vector3(game.greenButton[i].Position.X, 84.55f, game.greenButton[i].Position.Z);

                    game.bomb[i].IsDirect = true;
                    game.bomb[i].IsExploded = false;

                }

            }

        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                game.RemoveGameModels();

                ScreenManager.AddScreen(new PauseMenuScreen(game), ControllingPlayer);
            }
            else
            {
                if (!Globals.config.isComponentAdded)
                    game.AddGameModels();

            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            game.explosionParticles.SetCamera(game.cam.ViewMatrix, game.cam.PerspectiveMatrix);
            game.explosionSmokeParticles.SetCamera(game.cam.ViewMatrix, game.cam.PerspectiveMatrix);
            game.projectileTrailParticles.SetCamera(game.cam.ViewMatrix, game.cam.PerspectiveMatrix);
            game.smokePlumeParticles.SetCamera(game.cam.ViewMatrix, game.cam.PerspectiveMatrix);

            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
            
        }


        #endregion
    }
}
