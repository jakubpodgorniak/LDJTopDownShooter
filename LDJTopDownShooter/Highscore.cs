using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LDJTopDownShooter {
    public static class Highscore {

        public static int high_score = 0;
        public static int current_score = 0;

        public static void gain_score(int amount) {
            current_score += amount;

            if (current_score > high_score) {
                high_score = current_score;
            }
        }

        public static void reset_score() {
            current_score = 0;
        }

        public static void render(SpriteBatch sprite_batch, Texture2D ui_texture, SpriteFont font) {
            sprite_batch.Draw(
                ui_texture,
                new Rectangle(0, 0, 128, 128),
                new Rectangle(0, 0, 128, 128),
                Color.White);
            
            //JD it
            string score = current_score.ToString();
            var text_size = font.MeasureString(score);
            int x = (int)(66 - (text_size.X / 2)); 
            int y = (int)(66 - (text_size.Y / 2));
            sprite_batch.DrawString(
                font,
                score,
                new Vector2(x, y),
                Color.White);
        }
    }
}
