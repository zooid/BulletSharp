﻿using System;
using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BasicDemo
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class BasicDemo : Game
    {
        Vector3 _activeColor = Color.Orange.ToVector3();
        Vector3 _passiveColor = Color.DarkOrange.ToVector3();
        Vector3 _groundColor = Color.Green.ToVector3();

        Vector3 eye = new Vector3(30, 20, 10);
        Vector3 target = new Vector3(0, 5, 0);

        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        BasicEffect _debugEffect;
        Physics physics;
        Physics.PhysicsDebugDraw DebugDrawer;
        bool IsDebugDrawEnabled;
        bool f3KeyPressed;

        private Matrix _view, _projection;
        private Model _ground, _box;

        public BasicDemo()
        {
            graphics = new GraphicsDeviceManager(this);

            Window.Title = "BulletSharp - MonoGame Basic Demo";
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            Content.RootDirectory = "Content";
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 200.0f);

            _debugEffect.Projection = _projection;
            UpdateModel(_ground);
            UpdateModel(_box);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            physics = new Physics();

            DebugDrawer = new Physics.PhysicsDebugDraw(graphics.GraphicsDevice);
            physics.World.DebugDrawer = DebugDrawer;

            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void EndRun()
        {
            physics.ExitPhysics();
            base.EndRun();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            _debugEffect = new BasicEffect(device);
            _debugEffect.World = Matrix.Identity;

            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 200.0f);

            _ground = Content.Load<Model>("ground");
            _box = Content.Load<Model>("cube");
            LoadModel(_ground);
            LoadModel(_box);
        }

        private void LoadModel(Model model)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        private void UpdateModel(Model model)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.AmbientLightColor = Color.Gray.ToVector3();
                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor = Color.LemonChiffon.ToVector3();

                    effect.View = _view;
                    effect.Projection = _projection;
                }
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState ns = Keyboard.GetState();
            if (ns.IsKeyDown(Keys.Escape) || ns.IsKeyDown(Keys.Q))
            {
                Exit();
            }

            // Toggle debug
            if (ns.IsKeyDown(Keys.F3))
            {
                if (f3KeyPressed == false)
                {
                    f3KeyPressed = true;
                    if (IsDebugDrawEnabled == false)
                    {
                        DebugDrawer.DebugMode = DebugDrawModes.DrawAabb;
                        IsDebugDrawEnabled = true;
                    }
                    else
                    {
                        DebugDrawer.DebugMode = DebugDrawModes.None;
                        IsDebugDrawEnabled = false;
                    }
                }
            }
            if (f3KeyPressed)
            {
                if (ns.IsKeyUp(Keys.F3))
                    f3KeyPressed = false;
            }

            _view = Matrix.CreateLookAt(eye, target, Vector3.UnitY);
            _debugEffect.View = _view;
            UpdateModel(_ground);
            UpdateModel(_box);

            physics.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        private void DrawModel(Model model, Matrix worldTransform)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldTransform;
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            // Debug draw
            _debugEffect.LightingEnabled = false;
            _debugEffect.VertexColorEnabled = true;
            _debugEffect.CurrentTechnique.Passes[0].Apply();
            DebugDrawer.DrawDebugWorld(physics.World);


            // Draw shapes
            _debugEffect.VertexColorEnabled = false;
            _debugEffect.LightingEnabled = true;

            foreach (var colObj in physics.World.CollisionObjectArray)
            {
                if ("Ground".Equals(colObj.UserObject))
                {
                    DrawModel(_ground, Matrix.Identity);
                    continue;
                }

                var body = colObj as RigidBody;
                /*
                if (colObj.ActivationState == ActivationState.ActiveTag)
                    basicEffect.DiffuseColor = _activeColor;
                else
                    basicEffect.DiffuseColor = _passiveColor;
                */
                _debugEffect.CurrentTechnique.Passes[0].Apply();
                DrawModel(_box, body.WorldTransform);
            }

            base.Draw(gameTime);
        }
    }
}