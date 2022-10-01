using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LDJTopDownShooter;

public static class World {
    public const int PIXELS_PER_UNIT = 128;
    public const float MIN_X = 0.25f;
    public const float MAX_X = 5f;
    public const float MIN_Y = 0.25f;
    public const float MAX_Y = 5f;

    private static Texture2D _pixel_texture;

    public static void load_content(GraphicsDevice device) {
        _pixel_texture = new Texture2D(device, 1, 1);
        _pixel_texture.SetData(new[] { new Color(255, 255, 255, 255) });
    }

    public static void dispose() {
        _pixel_texture.Dispose();
    }

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

    public static bool is_position_in_boundries(Vector2 position) {
        return position.X >= MIN_X && position.X <= MAX_X
            && position.Y >= MIN_Y && position.Y <= MAX_Y;
    }

    public static void render(SpriteBatch sprite_batch) {
        int left = (int)Math.Floor(MIN_X * PIXELS_PER_UNIT);
        int top = (int)Math.Floor(MIN_Y * PIXELS_PER_UNIT);
        int right = (int)Math.Floor(MAX_X * PIXELS_PER_UNIT);
        int bottom = (int)Math.Floor(MAX_Y * PIXELS_PER_UNIT);

        int width = (int)Math.Floor((MAX_X - MIN_X) * PIXELS_PER_UNIT);
        int height = (int)Math.Floor((MAX_Y - MIN_Y) * PIXELS_PER_UNIT);

        // top
        sprite_batch.Draw(_pixel_texture, new Rectangle(left, top, width, 4), Color.Red);

        // right
        sprite_batch.Draw(_pixel_texture, new Rectangle(right, top, 4, height), Color.Red);

        // bottom
        sprite_batch.Draw(_pixel_texture, new Rectangle(left, bottom, width, 4), Color.Red);

        // left
        sprite_batch.Draw(_pixel_texture, new Rectangle(left, top, 4, height), Color.Red);
    }
}
