using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LDJTopDownShooter;

public enum WeaponType {
    Shotgun,
    Scythe,
    Laser
}

public static class CharacterHelper {
    public static float facing_2_rotation(Vector2 facing)
        => MathF.Atan2(facing.Y, facing.X);
}

public class Player
{
    public const float MOVEMENT_SPEED = 2.0f;
    public const float ROTATION_SPEED = 5.0f;

    public Vector2 position;
    public Vector2 facing = Vector2.UnitX;

    public float get_rotation() => CharacterHelper.facing_2_rotation(facing);
}

public static class EnemiesManager {
    private const int MAX_ENEMIES_COUNT = 300;
    private static Enemy[] enemies = new Enemy[MAX_ENEMIES_COUNT];
    private static int next_enemy_id = 0;
    
    private static Texture2D _enemy_texture;
    private static Texture2D _circle_texture;
    private static SpriteFont _arial10;

    static EnemiesManager() {
        for (int i = 0; i < enemies.Length; i++) {
            enemies[i] = new() {
                is_active = false
            };
        }
    }

    public static void load_content(ContentManager content) {
        _enemy_texture = content.Load<Texture2D>("enemy");
        _arial10 = content.Load<SpriteFont>("fonts/arial10");
        _circle_texture = content.Load<Texture2D>("circle");
    }

    public static void dispose() {
     // _arial10.Dispose(); ??
        _enemy_texture.Dispose();
    }

    public static void spawn_random_enemy() {
        float x = Game.randomf() * 5;
        float y = Game.randomf() * 5;

        if (next_enemy_id == MAX_ENEMIES_COUNT) {
            return;
        }

        var enemy = enemies[next_enemy_id];
        next_enemy_id++;

        enemy.position = new Vector2(x, y);
        enemy.facing = Vector2.UnitX;
        enemy.movement_speed = 1 + Game.randomf();
        enemy.rotation_speed = 5f;
        enemy.is_active = true;
        enemy.collider = new CircleCollider {
            position = enemy.position,
            radius = 0.25f
        };
    }

    public static void update() {
        for (int i = 0; i < next_enemy_id; i++) {
            var enemy = enemies[i];

            enemy.collider.position = enemy.position;
        }
    }

    public static void render(SpriteBatch sprite_batch) {
        for (int i = 0; i < next_enemy_id; i++) {
            var enemy = enemies[i];

            if (enemy.is_active) {

                var (x, y) = World.get_screen_position(enemy.position);
                float rotation = CharacterHelper.facing_2_rotation(enemy.facing);
                sprite_batch.Draw(
                    _enemy_texture,
                    new Rectangle(x, y, 64, 64),
                    null,
                    Color.White,
                    rotation,
                    new Vector2(32, 32),
                    SpriteEffects.None,
                    0);
                sprite_batch.DrawString(
                    _arial10,
                    rotation.ToString("F2", CultureInfo.InvariantCulture),
                    new Vector2(x, y),
                    Color.Black);

                int collider_size = 2 * (int)Math.Floor(enemy.collider.radius * World.PIXELS_PER_UNIT);
                sprite_batch.Draw(
                    _circle_texture,
                    new Rectangle(x, y, collider_size, collider_size),
                    null,
                    Color.LightGreen,
                    0,
                    new Vector2(64, 64),
                    SpriteEffects.None,
                    0);
            }
        }
    }
}

public class Enemy {
    public bool is_active;
    public float movement_speed;
    public float rotation_speed;
    public Vector2 position;
    public Vector2 facing;
    public CircleCollider collider;
}

public static class Shotgun {
    public const int BULLETS_PER_SINGLE_SHOT = 10;
    public const int MAX_BULLETS = 100;

    private static Bullet[] bullets = new Bullet[MAX_BULLETS];
    private static int next_bullet_index = 0;
    private static int first_bullet_index = 0;

    public static void Fire(Vector2 origin, Vector2 direction) {
        if (next_bullet_index >= MAX_BULLETS) {
            next_bullet_index = 0;
        }

        for (int i = 0; i < BULLETS_PER_SINGLE_SHOT; i++) {
            int index = next_bullet_index + i;

            var bullet = bullets[i];

            bullet.is_active = true;
            bullet.position = origin;
            bullet.direction = direction;
        }

        next_bullet_index += BULLETS_PER_SINGLE_SHOT;
    }
}

public static class Scythe {

}

public static class Laser {

}

public class Bullet { 
    public float SPEED = 10.0f;

    public bool is_active;
    public Vector2 position;
    public Vector2 direction;
}

public struct CircleCollider {
    public float radius;
    public Vector2 position;
}

public struct PointCollider { 
    public Vector2 position;
}

public static class CollisionDetection {
    public static bool check_circle_point_collision(CircleCollider circle, PointCollider point) {
        Vector2 distance = circle.position - point.position;

        return distance.Length() <= circle.radius;
    }
}
