using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace BG_3D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameObject terrain = new GameObject();
        GameObject missileLauncherBase = new GameObject();
        GameObject missileLauncherHead = new GameObject();

        const int numMissiles = 20;
        GameObject[] missiles;

        GamePadState previousState;
#if !XBOX
        KeyboardState previousKeyboardState;
#endif

        const float launcherHeadMuzzleOffset = 20.0f;
        const float missilePower = 20.0f;

        Vector3 cameraPosition = new Vector3(0.0f, 60.0f, 160.0f);
        Vector3 cameraLookAt = new Vector3(0.0f, 50.0f, 0.0f);
        Matrix cameraProjectionMatrix;
        Matrix cameraViewMatrix;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cameraViewMatrix = Matrix.CreateLookAt(
                cameraPosition,
                cameraLookAt,
                Vector3.Up);

            cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                graphics.GraphicsDevice.Viewport.AspectRatio,
                1.0f,
                10000.0f);

            terrain.model = Content.Load<Model>(
                "Models\\terrain");

            missileLauncherBase.model = Content.Load<Model>(
                "Models\\launcher_base");
            missileLauncherBase.scale = 0.2f;

            missileLauncherHead.model = Content.Load<Model>(
                "Models\\launcher_head");
            missileLauncherHead.scale = 0.2f;
            missileLauncherHead.position = missileLauncherBase.position +
                new Vector3(0.0f, 20.0f, 0.0f);

            missiles = new GameObject[numMissiles];
            for (int i = 0; i < numMissiles; i++)
            {
                missiles[i] = new GameObject();
                missiles[i].model =
                    Content.Load<Model>("Models\\missile");
                missiles[i].scale = 3.0f;
            }

            // TODO: use this.Content to load your game content here
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            missileLauncherHead.rotation.Y -=
                gamePadState.ThumbSticks.Left.X * 0.1f;

            missileLauncherHead.rotation.X +=
                gamePadState.ThumbSticks.Left.Y * 0.1f;

#if !XBOX
            KeyboardState keyboardState = Keyboard.GetState();
            if(keyboardState.IsKeyDown(Keys.Left))
            {
                missileLauncherHead.rotation.Y += 0.05f;
            }
            if(keyboardState.IsKeyDown(Keys.Right))
            {
                missileLauncherHead.rotation.Y -= 0.05f;
            }
            if(keyboardState.IsKeyDown(Keys.Up))
            {
                missileLauncherHead.rotation.X += 0.05f;
            }
            if(keyboardState.IsKeyDown(Keys.Down))
            {
                missileLauncherHead.rotation.X -= 0.05f;
            }
#endif

            missileLauncherHead.rotation.Y = MathHelper.Clamp(
                missileLauncherHead.rotation.Y,
                -MathHelper.PiOver4, MathHelper.PiOver4);

            missileLauncherHead.rotation.X = MathHelper.Clamp(
                missileLauncherHead.rotation.X,
                0, MathHelper.PiOver4);

            if (gamePadState.Buttons.A == ButtonState.Pressed &&
                previousState.Buttons.A == ButtonState.Released)
            {
                FireMissile();
            }

#if !XBOX
            if(keyboardState.IsKeyDown(Keys.Space) &&
                previousKeyboardState.IsKeyUp(Keys.Space))
            {
                FireMissile();
            }
#endif

            UpdateMissiles();

            previousState = gamePadState;
#if !XBOX
            previousKeyboardState = keyboardState;
#endif

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        void FireMissile()
        {
            foreach (GameObject missile in missiles)
            {
                if (!missile.alive)
                {
                    missile.velocity = GetMissileMuzzleVelocity();
                    missile.position = GetMissileMuzzlePosition();
                    missile.rotation = missileLauncherHead.rotation;
                    missile.alive = true;
                    break;
                }
            }
        }

        Vector3 GetMissileMuzzleVelocity()
        {
            Matrix rotationMatrix =
                Matrix.CreateFromYawPitchRoll(
                missileLauncherHead.rotation.Y,
                missileLauncherHead.rotation.X,
                0);

            return Vector3.Normalize(
                Vector3.Transform(Vector3.Forward,
                rotationMatrix)) * missilePower;
        }

        Vector3 GetMissileMuzzlePosition()
        {
            return missileLauncherHead.position +
                (Vector3.Normalize(
                GetMissileMuzzleVelocity()) *
                launcherHeadMuzzleOffset);
        }

        void UpdateMissiles()
        {
            foreach (GameObject missile in missiles)
            {
                if (missile.alive)
                {
                    missile.position += missile.velocity;
                    if (missile.position.Z < -6000.0f)
                    {
                        missile.alive = false;
                    }
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawGameObject(terrain);
            DrawGameObject(missileLauncherBase);
            DrawGameObject(missileLauncherHead);

            foreach (GameObject missile in missiles)
            {
                if (missile.alive)
                {
                    DrawGameObject(missile);
                }
            }

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        void DrawGameObject(GameObject gameobject)
        {
            foreach (ModelMesh mesh in gameobject.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.World =
                        Matrix.CreateFromYawPitchRoll(
                        gameobject.rotation.Y,
                        gameobject.rotation.X,
                        gameobject.rotation.Z) *

                        Matrix.CreateScale(gameobject.scale) *

                        Matrix.CreateTranslation(gameobject.position);

                    effect.Projection = cameraProjectionMatrix;
                    effect.View = cameraViewMatrix;
                }
                mesh.Draw();
            }
        }
    }
}
