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
    public class Input
    {
        #region Fields
        Camera cam;

        MouseState mouseStateCurrent, mouseStatePrevious;
        GamePadState gamePadStateCurrent;

        float dragX;
        float dragY;
        float dragConstantL = .001f;
        float dragConstantR = .2f;
        float dragConstantM = .1f;

        Vector2 delta;
        Vector2 currentAngle = new Vector2(0f, 0f);
        
        #endregion

        #region Properties
        public Vector2 Delta
        {
            get { return delta; }
            set { delta = value; }
        }

        public Vector2 CurrentAngle
        {
            get { return currentAngle; }
            set { currentAngle = value; }
        }
        #endregion

        #region Method
        public Input(Camera cam)
        {
            this.cam = cam;
        }

        

        public void BeginDrag()
        {   
            mouseStateCurrent = Mouse.GetState();
            if ( (mouseStateCurrent.RightButton == ButtonState.Pressed && mouseStatePrevious.RightButton == ButtonState.Released) ||
                 (mouseStateCurrent.LeftButton  == ButtonState.Pressed && mouseStatePrevious.LeftButton  == ButtonState.Released) )
            {
                dragX = mouseStateCurrent.X;
                dragY = mouseStateCurrent.Y;
            }
        }

        public void Drag()
        {
            mouseStateCurrent = Mouse.GetState();
            if (mouseStateCurrent.LeftButton == ButtonState.Pressed && mouseStatePrevious.LeftButton == ButtonState.Pressed)
            {
                delta.X = dragConstantL * (dragX - mouseStateCurrent.X);
                delta.Y = dragConstantL * (dragY - mouseStateCurrent.Y);

                dragX = mouseStateCurrent.X;
                dragY = mouseStateCurrent.Y;

            }

            if (mouseStateCurrent.RightButton == ButtonState.Pressed && mouseStatePrevious.RightButton == ButtonState.Pressed)
            {
                cam.EyeTheta += dragConstantR * (dragX - mouseStateCurrent.X);
                cam.EyePhi   += dragConstantR * (dragY - mouseStateCurrent.Y);

                dragX = mouseStateCurrent.X;
                dragY = mouseStateCurrent.Y;
            }

            if (mouseStateCurrent.ScrollWheelValue != mouseStatePrevious.ScrollWheelValue)
            {
                cam.EyeDistance += dragConstantM * (mouseStatePrevious.ScrollWheelValue - mouseStateCurrent.ScrollWheelValue);// / 120;
            }

            mouseStatePrevious = mouseStateCurrent;

            gamePadStateCurrent = GamePad.GetState(PlayerIndex.One);
            delta = (gamePadStateCurrent.ThumbSticks.Left != Vector2.Zero) ? - gamePadStateCurrent.ThumbSticks.Left * dragConstantL * 20 : delta;
            currentAngle.X = (Math.Abs(currentAngle.X + delta.X) > Globals.config.gAngleLimit) ? currentAngle.X : currentAngle.X + delta.X;
            currentAngle.Y = (Math.Abs(currentAngle.Y + delta.Y) > Globals.config.gAngleLimit) ? currentAngle.Y : currentAngle.Y + delta.Y;

            cam.EyeTheta -= gamePadStateCurrent.ThumbSticks.Right.X * dragConstantR * 20;
            
            cam.EyePhi -= gamePadStateCurrent.ThumbSticks.Right.Y * dragConstantR * 20;

            cam.EyeDistance += gamePadStateCurrent.Triggers.Left * 4;
            cam.EyeDistance -= gamePadStateCurrent.Triggers.Right * 4;
        }

        public Camera UpdateWorld()
        {
            BeginDrag();
            Drag();

            return cam;
        }

        
        #endregion
    }
}
