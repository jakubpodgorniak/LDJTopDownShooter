using System;
using Microsoft.Xna.Framework;

namespace LDJTopDownShooter {
    public static  class World {
        public const int PIXELS_PER_UNIT = 128;

        public static (int x, int y) get_screen_position(Vector2 position) {
            int x = (int)Math.Floor(position.X * PIXELS_PER_UNIT);
            int y = (int)Math.Floor(position.Y * PIXELS_PER_UNIT);

            return (x, y);
        }

        public static Vector2 rotate_vector2d_by_angle(Vector2 vec2, float radians) {
            float x = vec2.X * MathF.Cos(radians)
                - vec2.Y * MathF.Sin(radians);
            float y = vec2.X * MathF.Sin(radians)
                + vec2.Y * MathF.Cos(radians);

            return new Vector2(x, y);
        }
    }
}
