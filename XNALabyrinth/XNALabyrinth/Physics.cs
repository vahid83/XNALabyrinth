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
    public enum COLLISION
    { 
        HitXY, HitX, HitY, HitEdgeX, HitEdgeY, NoMove, NoHit 
    }

    public class Physics
    {
        #region Fields
        ModelContainar model;
        Vector2 deltaAngle;
        

        Vector2 prevVelocity;

        const float g = 1000f;
        const float epsilon = float.Epsilon;

        
        #endregion

        #region Properties
        public Vector2 DeltaAngle
        {
            get { return deltaAngle; }
            set { deltaAngle = value; }
        }

        public Vector2 PreVelocity
        {
            get { return prevVelocity; }
            set { prevVelocity = value; }
        }

        public float PreVelocityX
        {
            get { return prevVelocity.X; }
            set { prevVelocity.X = value; }
        }

        public float PreVelocityY
        {
            get { return prevVelocity.Y; }
            set { prevVelocity.Y = value; }
        }

        #endregion

        #region Methods
        public Physics(ModelContainar model)
        {
            this.model = model;
        }

        public Vector2 CurrentVelocity(Vector2 tableAngle, Vector2 externalAcceleration)
        {
            Vector2 acceleration = Vector2.Zero;
            Vector2 velocity = Vector2.Zero;
            
            acceleration.X = externalAcceleration.X + (float)(g * Math.Sin(tableAngle.X) - model.KineticFriction * g * Math.Cos(tableAngle.X) * Math.Sign(prevVelocity.X));
            acceleration.Y = externalAcceleration.Y + (float)(g * Math.Sin(-tableAngle.Y) - model.KineticFriction * g * Math.Cos(tableAngle.Y) * Math.Sign(prevVelocity.Y));

            velocity = prevVelocity + acceleration * Globals.config.gFrameTime;

            if (externalAcceleration == Vector2.Zero)
            {
                velocity.X = ((Math.Sign(velocity.X) * Math.Sign(prevVelocity.X)) <= 0 && Math.Abs(Math.Atan(tableAngle.X)) < model.StaticFriction) ? 0 : velocity.X;
                velocity.Y = ((Math.Sign(velocity.Y) * Math.Sign(prevVelocity.Y)) <= 0 && Math.Abs(Math.Atan(tableAngle.Y)) < model.StaticFriction) ? 0 : velocity.Y;
            }
            
            
            return velocity;
        }

        public Vector2 Collision(BoundingSphere sphere, List<Vector3> vertices, Vector2 velocity, ref COLLISION collision)
        {
            Ray[] ray = new Ray[3];

            ray[0].Position = sphere.Center;
            ray[0].Direction = Vector3.Normalize(new Vector3(-velocity.X, 0, 0));

            ray[1].Position = sphere.Center;
            ray[1].Direction = Vector3.Normalize(new Vector3(0, 0, -velocity.Y));

            Vector2 distance = Vector2.One * float.MaxValue;
            Vector3 Edge = Vector3.Zero;
            float? distanceEdge = float.MaxValue;
            for (int i = 0; i < vertices.Count; i += 3)
            {
                Vector2 tmpDistance;
                tmpDistance.X = RayTriangleIntersects(ray[0], vertices[i], vertices[i + 1], vertices[i + 2]);
                tmpDistance.Y = RayTriangleIntersects(ray[1], vertices[i], vertices[i + 1], vertices[i + 2]);
                if (tmpDistance.X < distance.X)
                    distance.X = tmpDistance.X;
                if (tmpDistance.Y < distance.Y)
                    distance.Y = tmpDistance.Y;
                if (vertices[i].Y != vertices[i + 1].Y && vertices[i].X == vertices[i + 1].X && vertices[i + 1].Z == vertices[i + 1].Z)
                {
                    ray[2].Position = new Vector3(vertices[i].X, (vertices[i].Y + vertices[i + 1].Y) / 2, vertices[i].Z);
                    ray[2].Direction = Vector3.Normalize(new Vector3(velocity.X, 0, velocity.Y));
                    float? tmp = sphere.Intersects(ray[2]);
                    if (tmp == null)
                        tmp = float.MaxValue;
                    if (tmp < distanceEdge)
                    {
                        Edge = ray[2].Position;
                        distanceEdge = tmp;
                    }
                }
            }
            
            if (!(distance.X != float.MaxValue && distance.X > 0 && (distance.X <= Globals.config.ballRadius || distance.X <= Math.Abs(velocity.X * Globals.config.gFrameTime) + Globals.config.ballRadius))) //|| distance <= Math.Abs(velocity.Y * Globals.config.gFrameTime) + Globals.config.ballRadius))
            {
                distance.X = float.MaxValue;
            }
            if (!(distance.Y != float.MaxValue && distance.Y > 0 && (distance.Y <= Globals.config.ballRadius || distance.Y <= Math.Abs(velocity.Y * Globals.config.gFrameTime) + Globals.config.ballRadius))) //|| distance <= Math.Abs(velocity.Y * Globals.config.gFrameTime) + Globals.config.ballRadius))
            {
                distance.Y = float.MaxValue;
            }
            if (!(distanceEdge != float.MaxValue && distanceEdge > 0 && (distanceEdge <= Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2)) * Globals.config.gFrameTime)))
            {
                distanceEdge = float.MaxValue;
            }

            if (distance.X != float.MaxValue && distance.Y != float.MaxValue && distance.X - Globals.config.ballRadius < distanceEdge/2 && distance.Y - Globals.config.ballRadius < distanceEdge/2)
                collision = COLLISION.HitXY;
            else if (distance.X != float.MaxValue && distance.X - Globals.config.ballRadius < distanceEdge/2)
                collision = COLLISION.HitX;
            else if (distance.Y != float.MaxValue && distance.Y - Globals.config.ballRadius < distanceEdge/2)
                collision = COLLISION.HitY;
            else if (distanceEdge != float.MaxValue)
            {
                float angleBefore = (float)(Math.Atan2(sphere.Center.Z - Edge.Z, sphere.Center.X - Edge.X));
                if (angleBefore != -MathHelper.PiOver2)
                    collision = COLLISION.NoHit;

                if (angleBefore > 0 && angleBefore < MathHelper.PiOver2)
                {
                    if (velocity.X <= 0 && velocity.Y >= 0)
                        collision = COLLISION.HitEdgeY;
                    else if (velocity.X >= 0 && velocity.Y <= 0)
                        collision = COLLISION.HitEdgeX;
                    else
                        collision = COLLISION.NoMove;
                }

                else if (angleBefore > MathHelper.PiOver2 && angleBefore < MathHelper.Pi)
                {
                    if (velocity.X >= 0 && velocity.Y >= 0)
                        collision = COLLISION.HitEdgeY;
                    else if (velocity.X <= 0 && velocity.Y <= 0)
                        collision = COLLISION.HitEdgeX;
                    else
                        collision = COLLISION.NoMove;
                }

                else if (angleBefore < -MathHelper.PiOver2 && angleBefore > -MathHelper.Pi)
                {
                    if (velocity.X >= 0 && velocity.Y <= 0)
                        collision = COLLISION.HitEdgeY;
                    else if (velocity.X <= 0 && velocity.Y >= 0)
                        collision = COLLISION.HitEdgeX;
                    else
                        collision = COLLISION.NoMove;
                }

                else if (angleBefore < 0 && angleBefore > -MathHelper.PiOver2)
                {
                    if (velocity.X <= 0 && velocity.Y <= 0)
                        collision = COLLISION.HitEdgeY;
                    else if (velocity.X >= 0 && velocity.Y >= 0)
                        collision = COLLISION.HitEdgeX;
                    else
                        collision = COLLISION.NoMove;
                }

                else if (angleBefore == 0 || angleBefore == -MathHelper.Pi || angleBefore == MathHelper.Pi)
                {
                    collision = COLLISION.HitEdgeX;
                }
                else if (angleBefore == MathHelper.PiOver2 || angleBefore == -MathHelper.PiOver2)
                {
                    collision = COLLISION.HitEdgeY;
                }

            }

            return distance;
        }

        
        

        public float RayTriangleIntersects(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            // The algorithm is based on Moller, Tomas and Trumbore, "Fast, Minimum Storage 
            // Ray-Triangle Intersection", Journal of Graphics Tools, vol. 2, no. 1, 
            // pp 21-28, 1997.

            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;

            Vector3 p = Vector3.Cross(ray.Direction, e2);

            float det = Vector3.Dot(e1, p);

            float t;
            if (det >= epsilon)
            {
                // Determinate is positive (front side of the triangle).
                Vector3 s = ray.Position - v0;
                float u = Vector3.Dot(s, p);
                if (u < 0 || u > det)
                    return float.MaxValue;

                Vector3 q = Vector3.Cross(s, e1);
                float v = Vector3.Dot(ray.Direction, q);
                if (v < 0 || ((u + v) > det))
                    return float.MaxValue;

                t = Vector3.Dot(e2, q);
                if (t < 0)
                    return float.MaxValue;
            }
            else if (det <= -epsilon)
            {
                // Determinate is negative (back side of the triangle).
                Vector3 s = ray.Position - v0;
                float u = Vector3.Dot(s, p);
                if (u > 0 || u < det)
                    return float.MaxValue;

                Vector3 q = Vector3.Cross(s, e1);
                float v = Vector3.Dot(ray.Direction, q);
                if (v > 0 || ((u + v) < det))
                    return float.MaxValue;

                t = Vector3.Dot(e2, q);
                if (t > 0)
                    return float.MaxValue;
            }
            else
            {
                // Parallel ray.
                return float.MaxValue;
            }

            return t / det;
        }

        public static Vector2 ExplosionForce(ModelContainar ball, Vector3 bombPosition)
        {
            Vector2 direction = new Vector2(ball.Position.X - bombPosition.X, ball.Position.Z - bombPosition.Z);
            float distance = (float)(Math.Pow(direction.X, 2) + Math.Pow(direction.Y, 2));
            direction.Normalize();
            Vector2 force = 10000000 / distance * direction;
            force.X = (Math.Abs(force.X) > 100000) ? Math.Sign(force.X) * 100000 : force.X;
            force.Y = (Math.Abs(force.Y) > 100000) ? Math.Sign(force.Y) * 100000 : force.Y;
            return -force / ball.Mass;
        }


        #endregion
    }
}
