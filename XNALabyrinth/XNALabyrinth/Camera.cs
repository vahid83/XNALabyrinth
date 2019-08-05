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
    public class Camera
    {
        #region Fields

        Vector3 camPosition;
        Vector3 lookAt;
        Vector3 upDir;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix perspectiveMatrix;

        float eyeTheta;
        float eyePhi;
        float eyeDistance;
        
        float fov;
        float farPlaneDistance;
        float nearPlaneDistance;
        
        float aspectRatio;
        private static readonly Random random = new Random();

        #endregion

        #region Properties

        public Vector3 CameraPosition
        {
            get { return camPosition; }
            set { camPosition = value; }
        }

        public Vector3 CameraLookAt
        {
            get { return lookAt; }
            set { lookAt = value; }
        }

        public Vector3 CameraUpDir
        {
            get { return upDir; }
            set { upDir = value; }
        }

        public Matrix WorldMatrix
        {
            get { return worldMatrix; }
            set { worldMatrix = value; }
        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        public Matrix PerspectiveMatrix
        {
            get { return perspectiveMatrix; }
            set { perspectiveMatrix = value; }
        }

        public float EyeTheta
        {
            get { return eyeTheta; }
            set { eyeTheta = value; }
        }

        public float EyePhi
        {
            get { return eyePhi; }
            set { eyePhi = value; }
        }

        public float EyeDistance
        {
            get { return eyeDistance; }
            set { eyeDistance = value; }
        }

        public float FieldOfView
        {
            get { return fov; }
            set { fov = value; }
        }

        #endregion

        #region Methods

        public Camera(float aspectRatio)
        {
            // TODO: Construct any child components here
            this.aspectRatio = aspectRatio;

            InitCamera();
        }

        public void InitCamera()
        {
            eyeTheta = 0;
            eyePhi = 20;
            eyeDistance = 200f;

            fov = 45f;
            farPlaneDistance = 10000f;
            nearPlaneDistance = .1f;

        }

        public void UpdateCamera(GameTime gameTime)
        {
            //worldMatrix = Matrix.Identity;

            camPosition = new Vector3( (float)(eyeDistance * Math.Sin(MathHelper.ToRadians(eyePhi)) * Math.Sin(MathHelper.ToRadians(eyeTheta)) ),
                                       (float)(eyeDistance * Math.Cos(MathHelper.ToRadians(eyePhi))),
                                       (float)(eyeDistance * Math.Sin(MathHelper.ToRadians(eyePhi)) * Math.Cos(MathHelper.ToRadians(eyeTheta))) );

            if (Globals.config.shaking)
            {
                Vector3 shakeOffset;
                // Move our timer ahead based on the elapsed time
                Globals.config.shakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // If we're at the max duration, we're not going to be shaking anymore
                if (Globals.config.shakeTimer >= Globals.config.shakeDuration)
                {
                    Globals.config.shaking = false;
                    Globals.config.shakeTimer = Globals.config.shakeDuration;
                }

                // Compute our progress in a [0, 1] range
                float progress = Globals.config.shakeTimer / Globals.config.shakeDuration;

                // Compute our magnitude based on our maximum value and our progress. This causes
                // the shake to reduce in magnitude as time moves on, giving us a smooth transition
                // back to being stationary. We use progress * progress to have a non-linear fall 
                // off of our magnitude. We could switch that with just progress if we want a linear 
                // fall off.
                float magnitude = Globals.config.shakeMagnitude * (1f - (progress * progress));

                // Generate a new offset vector with three random values and our magnitude
                shakeOffset = new Vector3(NextFloat(), NextFloat(), NextFloat()) * magnitude;
                Vector3 cameraPosition = camPosition + shakeOffset;
                Vector3 LookAt = lookAt + shakeOffset;
                viewMatrix = Matrix.CreateLookAt(cameraPosition, LookAt, upDir);
            }
            else
            {
                viewMatrix = Matrix.CreateLookAt(camPosition, lookAt, upDir);
            }
            
            perspectiveMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        public void UpdateCamera(Matrix reverse)
        {
                viewMatrix = Matrix.CreateLookAt(camPosition, lookAt, upDir);
                viewMatrix = reverse * viewMatrix;

                perspectiveMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        private float NextFloat()
        {
            return (float)random.NextDouble() * 2f - 1f;
        }

        #endregion
    }
}
