

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    struct AnimationPlayer
    {
        public Animation Animation
        {
            get { return animation; }
        }
        Animation animation;

        public int FrameIndex
        {
            get { return frameIndex; }
        }
        int frameIndex;

        private float time;

        public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }

        public void PlayAnimation(Animation animation)
        {
            if (Animation == animation)
                return;

            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            if (Animation == null)
                throw new NotSupportedException("No animation is currently playing.");

            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                if (Animation.IsLooping)
                {
                    frameIndex++;
                    // frameIndex = (frameIndex + 1) % Animation.FrameCount;
                    //frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                    if (Animation.FrameWidth == frameIndex * Animation.WindowWidth && Animation.Texture.Width != Animation.WindowWidth)
                    {
                        frameIndex = 0;
                    }
                    else if (Animation.Texture.Width > Animation.WindowWidth)
                    {
                    }

                    if (frameIndex == Animation.FrameCount - 1)
                    {
                        frameIndex = 0;
                    }
                }
                else if (!Animation.IsLooping && frameIndex * Animation.WindowWidth + Animation.WindowWidth < Animation.FrameWidth)
                {
                    frameIndex++;
                }
                else
                {
                }




            }
            Rectangle source = new Rectangle(Animation.WindowWidth * frameIndex, 0, Animation.WindowWidth, Animation.FrameHeight);
            Vector2 o = new Vector2(Animation.WindowWidth, 128);

            spriteBatch.Draw(Animation.Texture, position, source, Color.White, 0.0f, o, 1.0f, spriteEffects, 0.0f);
        }
    }
}
