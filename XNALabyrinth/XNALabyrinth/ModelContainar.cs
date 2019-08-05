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
    public class ModelContainar : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Fields
        Game game;
        
        Model model;
        Physics physics;

        TextureCube skyCube;
        bool hasCubeMap;

        bool isStatic;
        bool isBall;

        Vector3 position;
        Vector3 forward;
        Vector3 up;

        Matrix world;
        Matrix view;
        Matrix perspective;
        
        float mass;
        float reflectionIndex;
        float kineticFriction;
        float staticFriction;
        
        string sound1FileName = string.Empty;
        string sound2FileName = string.Empty;
        string sound3FileName = string.Empty;

        SoundEffect sound1;
        SoundEffectInstance sound1Instance;

        SoundEffect sound2;
        SoundEffectInstance sound2Instance;

        SoundEffect sound3;
        SoundEffectInstance sound3Instance;

        TimeSpan bombTimer = TimeSpan.Zero;
        bool isTrig = false;
        bool isExploded;
        bool isNeutralized;
        bool isDirect = false;
        bool isHitted = false;

        Dictionary<string, List<Vector3>> tagData;
        List<Vector3> vertList;

        string modelName;

        Vector2 delta;
        Vector2 externalAcceleration = Vector2.Zero;
        
        
        #endregion
        
        #region Properties
        public SoundEffectInstance Sound1
        {
            get { return sound1Instance; }
            set { sound1Instance = value; }
        }

        public SoundEffectInstance Sound2
        {
            get { return sound2Instance; }
            set { sound2Instance = value; }
        }

        public SoundEffectInstance Sound3
        {
            get { return sound3Instance; }
            set { sound3Instance = value; }
        }

        public TimeSpan BombTimer
        {
            get { return bombTimer; }
            set { bombTimer = value; }
        }

        public List<Vector3> VertList
        {
            get { return vertList; }
        }

        public string ModelNamePath
        {
            get { return modelName; }
            set { modelName = value; }
        }

        public Model ModelObj
        {
            get { return model; }
            set { model = value; }
        }

        public TextureCube SkyCube
        {
            get { return skyCube; }
            set { skyCube = value; }
        }

        public Physics Physic
        {
            get { return physics; }
            set { physics = value; }
        }

        public Matrix Perspective
        {
            get { return perspective; }
            set { perspective = value; }
        }

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        public bool HasCubeMap
        {
            get { return hasCubeMap; }
            set { hasCubeMap = value; }
        }

        public bool IsBall
        {
            get { return isBall; }
            set { isBall = value; }
        }

        public bool IsStatic
        {
            get { return isStatic; }
            set { isStatic = value; }
        }

        public bool IsTrig
        {
            get { return isTrig; }
            set { isTrig = value; }
        }

        public bool IsDirect
        {
            get { return isDirect; }
            set { isDirect = value; }
        }

        public bool IsHitted
        {
            get { return isHitted; }
            set { isHitted = value; }
        }

        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        public float ReflectionIndex
        {
            get { return reflectionIndex; }
            set { reflectionIndex = value; }
        }

        public float KineticFriction
        {
            get { return kineticFriction; }
            set { kineticFriction = value; }
        }

        public bool IsExploded
        {
            get { return isExploded; }
            set { isExploded = value; }
        }

        public bool IsNeutralized
        {
            get { return isNeutralized; }
            set { isNeutralized = value; }
        }

        public float StaticFriction
        {
            get { return staticFriction; }
            set { staticFriction = value; }
        }

        public Vector2 ExternalAcceleration
        {
            get { return externalAcceleration; }
            set { externalAcceleration = value; }
        }

        #endregion

        #region Methods
        public ModelContainar(Game game)
            : base(game)
        {
            this.game = game;
            isBall = false;
            isStatic = false;
        }

        public ModelContainar(Game game, string sound1FileName, string sound2FileName, string sound3FileName)
            : base(game)
        {
            this.game = game;
            isBall = false;
            isStatic = false;
            this.sound1FileName = sound1FileName;
            this.sound2FileName = sound2FileName;
            this.sound3FileName = sound3FileName;
        }

        public override void Initialize()
        {
            hasCubeMap = false;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            model = game.Content.Load<Model>(modelName);

            if (sound1FileName != string.Empty )
            {
                sound1 = game.Content.Load<SoundEffect>(sound1FileName);
                sound1Instance = sound1.CreateInstance();
            }

            if (sound2FileName != string.Empty)
            {
                sound2 = game.Content.Load<SoundEffect>(sound2FileName);
                sound2Instance = sound2.CreateInstance();
            }

            if (sound3FileName != string.Empty)
            {
                sound3 = game.Content.Load<SoundEffect>(sound3FileName);
                sound3Instance = sound3.CreateInstance();
            }

            tagData = (Dictionary<string, List<Vector3>>)model.Tag;
            if (tagData != null)
                TransformVertices();

            base.LoadContent();
        }

        public void InitPosition(Vector3 position, Vector3 forward, Vector3 up)
        {
            physics = new Physics(this);
            this.position = position;
            this.forward = forward;
            this.up = up;
            world = Matrix.CreateWorld(position, forward, up);
        }

        public void InitPosition(Vector3 position, Vector3 forward, Vector3 up, float mass, float reflectionIndex, float kineticFriction, float staticFriction)
        {
            physics = new Physics(this);
            this.position = position;
            this.forward = forward;
            this.up = up;
            world = Matrix.CreateWorld(position, forward, up);

            this.mass = mass;
            this.reflectionIndex = reflectionIndex;
            this.kineticFriction = kineticFriction;
            this.staticFriction = staticFriction;

        }
                
        private void TransformVertices()
        {
            vertList = new List<Vector3>();

            for (int i = 0; i < tagData.Count; i++)
            {
                foreach (Vector3 v in TagData(i))
                {
                    vertList.Add(Vector3.Transform(v, world));
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);


            if (hasCubeMap)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (EnvironmentMapEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.EnvironmentMap = skyCube;
                        effect.FresnelFactor = 0.0f;

                        effect.World = transforms[mesh.ParentBone.Index] * world;
                        effect.View = view;
                        effect.Projection = perspective;

                    }
                    mesh.Draw();
                }
            }
            else
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        if (effect is BasicEffect)
                        {
                            BasicEffect baseEffect = effect as BasicEffect;

                            //baseEffect.EnableDefaultLighting();

                            baseEffect.LightingEnabled = true;
                            //baseEffect.DiffuseColor = new Vector3(1f);
                            baseEffect.AmbientLightColor = new Vector3(0.5f);

                            baseEffect.DirectionalLight0.Enabled = true;
                            baseEffect.DirectionalLight0.DiffuseColor = Vector3.One;
                            baseEffect.DirectionalLight0.Direction = game.lensFlare.LightDirection;

                            baseEffect.World = transforms[mesh.ParentBone.Index] * world;
                            baseEffect.View = view;
                            baseEffect.Projection = perspective;
                        }
                        else
                        {
                            effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * world);
                            effect.Parameters["View"].SetValue(view);
                            effect.Parameters["Projection"].SetValue(perspective);
                        }
                    }
                    mesh.Draw();
                }
            }
            
            game.lensFlare.View = view;
            game.lensFlare.Projection = perspective;

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            delta = game.input.CurrentAngle;

            UpdateViewPerspectiveMatrix();
            
            if (!isStatic)
                UpdateWorldMatrix();

            if (IsTrig)
                bombTimer += gameTime.ElapsedGameTime;

            base.Update(gameTime);
        }

        public void UpdateViewPerspectiveMatrix()
        {
            view = game.cam.ViewMatrix;
            perspective = game.cam.PerspectiveMatrix;
        }

        public void UpdateWorldMatrix()
        {
            if (isBall)
            {
                UpdateWorldMatrixBall();
            }
            else
            {
                world = Matrix.CreateWorld(position, forward, up);
                world *= Matrix.CreateTranslation(0f, -game.table.position.Y, 0f);
                world *= Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(0f, 0f, 1f)), delta.X);
                world *= Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(1f, 0f, 0f)), delta.Y);
                world *= Matrix.CreateTranslation(0f, game.table.position.Y, 0f);
            }
        }

        public void UpdateWorldMatrixBall()
        {
            List<Vector3> verticesSoup = game.maze.VertList;
            Vector3 tableCenter = game.table.Position;

            Vector2 velocity = physics.CurrentVelocity(delta, externalAcceleration);
            externalAcceleration = Vector2.Zero;

            Vector3 prevPosition = position;

            COLLISION collision = COLLISION.NoHit;
            
            Vector2 distance = physics.Collision(new BoundingSphere(position, Globals.config.ballRadius), verticesSoup, velocity, ref collision);

            switch (collision)
            { 
                case COLLISION.HitXY:
                    if (Math.Abs(physics.PreVelocityX) > 1 || Math.Abs(physics.PreVelocityY) > 1)
                    {

                        float vol = (float)((Math.Max(Math.Abs(velocity.X), Math.Abs(velocity.Y)))) / 200;
                        sound2Instance.Volume = vol > 1 ? 1 : vol;
                        sound2Instance.Play();

                        physics.PreVelocityX = -reflectionIndex * velocity.X;
                        position.X -= physics.PreVelocityX * Globals.config.gFrameTime;

                        physics.PreVelocityY = -reflectionIndex * velocity.Y;
                        position.Z -= physics.PreVelocityY * Globals.config.gFrameTime;
                    }
                    else
                    {
                        sound1Instance.Stop();
                        physics.PreVelocityX = 0;
                        physics.PreVelocityY = 0;
                    }

                    break;

                case COLLISION.HitX:
                    
                    if (Math.Abs(physics.PreVelocityX) > 1)
                    {
                        float vol = (float)(Math.Abs(velocity.X)) / 200;
                        sound2Instance.Volume = vol > 1 ? 1 : vol;
                        sound2Instance.Play();

                        physics.PreVelocityX = -reflectionIndex * velocity.X;
                        physics.PreVelocityY = velocity.Y;

                        position.X -= physics.PreVelocityX * Globals.config.gFrameTime;
                        position.Z -= physics.PreVelocityY * Globals.config.gFrameTime;
                    }
                    else
                    {
                        if (physics.PreVelocity != Vector2.Zero)
                        {
                            float velocityMagnitude = (float)(Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2))) / 200 - 1;
                            sound1Instance.Pitch = velocityMagnitude > 1 ? 1 : velocityMagnitude;
                            sound1Instance.Play();
                        }
                        else
                            sound1Instance.Stop();

                        physics.PreVelocityY = velocity.Y;
                        physics.PreVelocityX = 0;
                        position.Z -= velocity.Y * Globals.config.gFrameTime;
                    }

                    break;

                case COLLISION.HitY:
                    if (Math.Abs(physics.PreVelocityY) > 1)
                    {
                        float vol = (float)(Math.Abs(velocity.Y)) / 200;
                        sound2Instance.Volume = vol > 1 ? 1 : vol;
                        sound2Instance.Play();
                        
                        physics.PreVelocityX = velocity.X;
                        physics.PreVelocityY = - reflectionIndex * velocity.Y;

                        position.X -= physics.PreVelocityX * Globals.config.gFrameTime;
                        position.Z -= physics.PreVelocityY * Globals.config.gFrameTime;
                    }
                    else
                    {
                        if (physics.PreVelocity != Vector2.Zero)
                        {
                            float velocityMagnitude = (float)(Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2))) / 200 - 1;
                            sound1Instance.Pitch = velocityMagnitude > 1 ? 1 : velocityMagnitude;
                            sound1Instance.Play();
                        }
                        else
                            sound1Instance.Stop();

                        physics.PreVelocityX = velocity.X;
                        physics.PreVelocityY = 0;
                        position.X -= velocity.X * Globals.config.gFrameTime;
                    }
                    break;

                case COLLISION.HitEdgeX:
                    if (Math.Abs(physics.PreVelocityX) > 1)
                    {
                        float vol = (float)(Math.Abs(velocity.X)) / 200;
                        sound2Instance.Volume = vol > 1 ? 1 : vol;
                        sound2Instance.Play();

                        physics.PreVelocityX = -reflectionIndex * velocity.X;
                        physics.PreVelocityY = velocity.Y;

                        position.X -= physics.PreVelocityX * Globals.config.gFrameTime;
                        position.Z -= physics.PreVelocityY * Globals.config.gFrameTime;
                    }
                    else
                    {
                        if (physics.PreVelocity != Vector2.Zero)
                        {
                            float velocityMagnitude = (float)(Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2))) / 200 - 1;
                            sound1Instance.Pitch = velocityMagnitude > 1 ? 1 : velocityMagnitude;
                            sound1Instance.Play();
                        }
                        else
                            sound1Instance.Stop();
                        
                        physics.PreVelocityY = velocity.Y;
                        physics.PreVelocityX = 0;
                        position.Z -= velocity.Y * Globals.config.gFrameTime;
                    }

                    break;

                case COLLISION.HitEdgeY:
                    if (Math.Abs(physics.PreVelocityY) > 1)
                    {
                        float vol = (float)(Math.Abs(velocity.Y)) / 200;
                        sound2Instance.Volume = vol > 1 ? 1 : vol;
                        sound2Instance.Play();

                        physics.PreVelocityX = velocity.X;
                        physics.PreVelocityY = -reflectionIndex * velocity.Y;

                        position.X -= physics.PreVelocityX * Globals.config.gFrameTime;
                        position.Z -= physics.PreVelocityY * Globals.config.gFrameTime;
                    }
                    else
                    {
                        if (physics.PreVelocity != Vector2.Zero)
                        {
                            float velocityMagnitude = (float)(Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2))) / 200 - 1;
                            sound1Instance.Pitch = velocityMagnitude > 1 ? 1 : velocityMagnitude;
                            sound1Instance.Play();
                        }
                        else
                            sound1Instance.Stop();

                        physics.PreVelocityX = velocity.X;
                        physics.PreVelocityY = 0;
                        position.X -= velocity.X * Globals.config.gFrameTime;
                    }

                    break;

                case COLLISION.NoMove:
                    if (physics.PreVelocity != Vector2.Zero)
                    {
                        float vol = (float)(Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2))) / 200;
                        sound2Instance.Volume = vol > 1 ? 1 : vol;
                        sound2Instance.Play();
                    }
                    sound1Instance.Stop();
                    physics.PreVelocity = Vector2.Zero;

                    break;
                    


                default:
                    if (physics.PreVelocity != Vector2.Zero)
                    {
                        float velocityMagnitude = (float)(Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2))) / 200 - 1;
                        sound1Instance.Pitch = velocityMagnitude > 1 ? 1 : velocityMagnitude;
                        sound1Instance.Play();
                    }
                    else
                        sound1Instance.Stop();

                    position.X -= velocity.X * Globals.config.gFrameTime;
                    position.Z -= velocity.Y * Globals.config.gFrameTime;
                
                    physics.PreVelocityX = velocity.X;
                    physics.PreVelocityY = velocity.Y;
                    break;                
            }

            
            
            world = Matrix.Identity;
            world *= Matrix.CreateTranslation(0f, -tableCenter.Y, 0f);
            world *= Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(0f, 0, 1f)), delta.X);
            world *= Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(1f, 0f, 0f)), delta.Y);
            world *= Matrix.CreateTranslation(0f, tableCenter.Y, 0f);

            Vector4 deltaPos = Vector4.Transform(position, world);

            world = Matrix.CreateTranslation(deltaPos.X, deltaPos.Y, deltaPos.Z);
            
        }

        public List<Vector3> TagData(int i)
        {
            List<string> keys = new List<string>(tagData.Keys);
            return tagData[keys[i]];
        }

        #endregion

    }
}