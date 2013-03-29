#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;


namespace Platformer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        const int screenWidth = 1024;
        const int screenHeight = 768;


        private Texture2D winOverlay;
        private Texture2D diedOverlay;
        public SoundEffect VictoryMusic;
        public SoundEffect Heart;
        public SoundEffect Rock;
        public SoundEffectInstance victoryMusic;
        public SoundEffectInstance heart;
        public SoundEffectInstance rock;

        // Meta-level game state.

        int currentLevel = 0;
        private int levelIndex = -1;
        Level level;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        
        private const int numberOfLevels = 6;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";


        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
            VictoryMusic = Content.Load<SoundEffect>("RoaringWonder");
            Heart = Content.Load<SoundEffect>("Heart");
            Rock = Content.Load<SoundEffect>("Bass");
            victoryMusic = VictoryMusic.CreateInstance();
            rock = Rock.CreateInstance();
            heart = Heart.CreateInstance();
            victoryMusic.IsLooped = true;
            victoryMusic.Volume = 0.3f;
            rock.IsLooped = true;
            rock.Volume = .3f;
            heart.IsLooped = true;
            heart.Volume = .6f;
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
            }
            catch { }

            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState,  Window.CurrentOrientation);

            

            base.Update(gameTime);
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.A);

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                    {
                        if (currentLevel < numberOfLevels)
                        {
                            currentLevel++;
                            LoadNextLevel();
                        }
                        else
                            ReloadCurrentLevel();
                    }
                    else
                        ReloadCurrentLevel();
                }
            }
            if (keyboardState.IsKeyDown(Keys.Enter))
                LoadNextLevel();

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
        heart.Pause();
            victoryMusic.Pause();
            rock.Pause();
            /*levelIndex = (levelIndex + 1) % numberOfLevels;
            if (currentLevel == 0)
            {
                using (Stream fileStream = TitleContainer.OpenStream("Content/Levels/0.txt"))
                    level = new Level(Services, fileStream, levelIndex);
            }
            else if (currentLevel == 1)
            {
                using (Stream fileStream = TitleContainer.OpenStream("Content/Levels/1.txt"))
                    level = new Level(Services, fileStream, levelIndex);
            }
            else if (currentLevel == 2)
            {
                using (Stream fileStream = TitleContainer.OpenStream("Content/Levels/2.txt"))
                    level = new Level(Services, fileStream, levelIndex);
            }
            else if (currentLevel == 3)
            {
                using (Stream fileStream = TitleContainer.OpenStream("Content/Levels/3.txt"))
                    level = new Level(Services, fileStream, levelIndex);
            }*/
            // move to the next level
            if (levelIndex == 5)
                levelIndex = 0;
            else
                levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            if (levelIndex != 5)
            {
                heart.Play();
                rock.Play();
            }
            else
            {
                victoryMusic.Play();
            }
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex); //"Content/Levels/" + currentLevel + ".txt";//
            //string levelPath = "Content/Levels/5.txt";
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        public void LoadNextLevel(int number)
        {
            levelIndex = 0;
            currentLevel = 0;
            level.Dispose();
            string levelPath = string.Format("Content/Levels/0.txt");
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        public void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            level.Draw(gameTime, spriteBatch);

            DrawHud();

            //spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "Stress-o-meter: " + level.getPlayer().getCurrentStress() + " / 220", hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Red);

            if (level.getPlayer().getCurrentStress() >= 160)
            {
                rock.Volume = .1f;
                heart.Volume = .6f;
            }
            else
            {
                rock.Volume = .3f;
                heart.Volume = .3f;
            }


            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
            }
            else if (!level.Player.IsAlive)
            {
                //ReloadCurrentLevel();
            }
            if (keyboardState.IsKeyDown(Keys.R))
            {
                ReloadCurrentLevel();
            }

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                LoadNextLevel(0);
            }
            if (keyboardState.IsKeyDown(Keys.Tab))
            {
                Exit();
            }
            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                // spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
            spriteBatch.End();
        }
    }
}
