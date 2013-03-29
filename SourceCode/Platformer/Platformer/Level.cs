

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    public class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        //private Texture2D[] layers;
        public Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private float cameraPosition;
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public Player getPlayer()
        {
            return player;
        }

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/background", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/background", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/background", 0.8f);
            /*layers = new Texture2D[3];
             *
            for (int i = 0; i < layers.Length; ++i)
            {
                // Choose a random segment if each background layer for level variety.
                int segmentIndex = levelIndex;
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            }*/

            // Load sounds.
        }
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            //if (exit == InvalidPosition)
            //    throw new NotSupportedException("A level must have an exit.");

        }

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);


                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies
                case 'a':
                    return LoadEnemyTile(x, y, "MonsterA");
                case 'b':
                    return LoadEnemyTile(x, y, "MonsterB");
                case 'c':
                    return LoadEnemyTile(x, y, "MonsterC");
                case 'd':
                    return LoadEnemyTile(x, y, "MonsterD");

                // Platform block
                case '#':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Impassable);

                // Passable block
                //case ':':
                //    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case 's':
                    return LoadStartTile(x, y);

                // Impassable block
                //case '#':
                //    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                case 'l':
                    return LoadVarietyTile("BlockLeft", 1, TileCollision.Impassable);

                case 'r':
                    return LoadVarietyTile("BlockRight", 1, TileCollision.Impassable);

                case 't':
                    return LoadVarietyTile("DoorTop", 1, TileCollision.Passable);

                case 'f':
                    return LoadVarietyTile("WallLeft", 1, TileCollision.Impassable);

                case 'w':
                    return LoadVarietyTile("WallRight", 1, TileCollision.Impassable);

                case 'p':
                    return LoadVarietyTile("WallLeft", 1, TileCollision.Passable);

                case 'j':
                    return LoadVarietyTile("WindowSillBottom", 1, TileCollision.Passable);

                case 'k':
                    return LoadVarietyTile("WindowSillTop", 1, TileCollision.Passable);

                case 'A':
                    return LoadVarietyTile("LetterA", 1, TileCollision.Passable);

                case 'R':
                    return LoadVarietyTile("LetterR", 1, TileCollision.Passable);

                case 'H':
                    return LoadVarietyTile("LetterH", 1, TileCollision.Passable);

                case 'Y':
                    return LoadVarietyTile("LetterY", 1, TileCollision.Passable);

                case 'T':
                    return LoadVarietyTile("LetterT", 1, TileCollision.Passable);

                case 'M':
                    return LoadVarietyTile("LetterM", 1, TileCollision.Passable);

                case 'I':
                    return LoadVarietyTile("LetterI", 1, TileCollision.Passable);

                case 'N':
                    return LoadVarietyTile("LetterN", 1, TileCollision.Passable);

                case 'J':
                    return LoadVarietyTile("LetterJ", 1, TileCollision.Passable);

                case 'P':
                    return LoadVarietyTile("LetterP", 1, TileCollision.Passable);

                case 'L':
                    return LoadVarietyTile("LetterL", 1, TileCollision.Passable);

                case 'G':
                    return LoadVarietyTile("LetterG", 1, TileCollision.Passable);

                case 'E':
                    return LoadVarietyTile("LetterE", 1, TileCollision.Passable);

                case 'S':
                    return LoadVarietyTile("LetterS", 1, TileCollision.Passable);

                case 'C':
                    return LoadVarietyTile("LetterC", 1, TileCollision.Passable);

                case 'F':
                    return LoadVarietyTile("LetterF", 1, TileCollision.Passable);

                case 'U':
                    return LoadVarietyTile("LetterU", 1, TileCollision.Passable);

                case 'K':
                    return LoadVarietyTile("LetterK", 1, TileCollision.Passable);

                case 'O':
                    return LoadVarietyTile("LetterO", 1, TileCollision.Passable);

                case 'V':
                    return LoadVarietyTile("LetterV", 1, TileCollision.Passable);

                case 'W':
                    return LoadVarietyTile("LetterW", 1, TileCollision.Passable);

                case 'D':
                    return LoadVarietyTile("LetterD", 1, TileCollision.Passable);

                case 'e':
                    return LoadExitTile(x, y);
                case 'g':
                    return LoadVarietyTile("TreeTop", 1, TileCollision.Passable);
                case 'v':
                    return LoadVarietyTile("Untitled", 1, TileCollision.Passable);
                case 'y':
                    return LoadVarietyTile("Chair", 1, TileCollision.Passable);
                case 'u':
                    return LoadVarietyTile("TableLeft", 1, TileCollision.Passable);
                case 'i':
                    return LoadVarietyTile("TableRight", 1, TileCollision.Passable);
                case 'X':
                    return LoadVarietyTile("LetterX", 1, TileCollision.Passable);
                case 'Z':
                    return LoadVarietyTile("LetterZ", 1, TileCollision.Passable);
                case 'B':
                    return LoadVarietyTile("LetterB", 1, TileCollision.Passable);

                
                


                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            //int index = random.Next(variationCount);
            return LoadTile(baseName, collision);
        }


        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("DoorBottom", TileCollision.Passable);
        }
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }


        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            DisplayOrientation orientation)
        {
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState, gamePadState, orientation);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }


        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    KeyboardState keyboardState = Keyboard.GetState();
                    if (keyboardState.IsKeyDown(Keys.F) && !enemy.getDead())
                    {
                        enemy.setDead();
                        Player.Stress.enemyKill();
                    }
                    else if (!enemy.getDead())
                        OnPlayerKilled(enemy);
                }
            }
        }


        private void OnPlayerKilled(Enemy killedBy)
        {
            //Player.OnKilled(killedBy);
            Player.Stress.enemyDetect();
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            //exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
        }

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;
            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        #endregion
    }
}
