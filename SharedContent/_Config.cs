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

namespace SharedConfig
{
    public class Configuration
    {
        public string holeModelPath;
        public string ballPath;
        public string baseTablePath;
        public string tablePath;
        public string mazePath;
        public string bombModelPath;
        public string bombHoleModelPath;
        public string redButtonModelPath;
        public string greenButtonModelPath;
        public string glowPath;
        public string flarePath;
        public string explosionTimerPath;
        public string explosionPath;

        public string particleExplosion;
        public string particleExplosionSmoke;
        public string particleProjectileTrail;
        public string particleSmokePlume;

        public string skySphereFX;
        public string envTexCube;
        public string envSkyModel;

        public string fontPath;
        public string blankPath;
        public string backgroundPath;
        public string gradient;

        public float shakeTimer;
        public float shakeDuration;
        public float shakeMagnitude;
        public bool shaking;

        public bool isComponentAdded;

        public float gAngleLimit;
        public float gFrameTime; 

        public int numberOfHoles;
        public float[] holesXposition;
        public float[] holesZposition;

        public int numberOfBombs;
        public float[] bombsXposition;
        public float[] bombsZposition;

        public float[] redButtonXposition;
        public float[] redButtonZposition;

        public float[] greenButtonXposition;
        public float[] greenButtonZposition;

        public float ballRadius;
        

    }

    public static class Globals
    { 
        public static Configuration config;
        public static TimeSpan timeToNextProjectile = TimeSpan.Zero;
    }
}
