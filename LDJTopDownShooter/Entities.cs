using System;
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

    public static Enemy[] get_enemies() => enemies;

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

                var (x, y) = World.get_screen_position(enemy.collider.position);
                float rotation = CharacterHelper.facing_2_rotation(enemy.facing);
                //sprite_batch.Draw(
                //    _enemy_texture,
                //    new Rectangle(x - 32, y - 32, 64, 64),
                //    null,
                //    Color.White,
                //    rotation,
                //    new Vector2(32, 32),
                //    SpriteEffects.None,
                //    0);

                sprite_batch.Draw(
                    _enemy_texture,
                    new Rectangle(x - 32, y - 32, 64, 64),
                    Color.White);
                sprite_batch.DrawString(
                    _arial10,
                    $"[{enemy.position.X:F2}, {enemy.position.Y:F2}] {rotation:F2}",
                    new Vector2(x, y),
                    Color.Black);

                int radius = (int)Math.Floor(enemy.collider.radius * World.PIXELS_PER_UNIT);
                int collider_size = 2 * radius;
                sprite_batch.Draw(
                    _circle_texture,
                    new Rectangle(x - radius, y - radius, collider_size, collider_size),
                    Color.LightGreen);
                   
            }
        }
    }

    public static void kill(Enemy enemy) {
        enemy.is_active = false;
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
    public const int BULLETS_PER_SINGLE_SHOT = 5;
    public const int MAX_BULLETS = 100;
    public const float BULLET_RADIANS_DEVIATION = 0.3f;
    public const float MAX_DISTANCE_DISCREPANCY = 0.15f;

    private static Bullet[] bullets = new Bullet[MAX_BULLETS];
    private static int next_bullet_index = 0;
    private static int first_bullet_index = 0;

    private static Texture2D _bullet_texture;

    static Shotgun() {
        for (int i = 0; i < bullets.Length; i++) {
            bullets[i] = new () {
                is_active = false
            };
        }
    }

    public static void load_content(ContentManager content) {
        _bullet_texture = content.Load<Texture2D>("bullet");
    }

    public static void dispose() {
        _bullet_texture.Dispose();
    }

    public static void fire(Vector2 origin, Vector2 direction) {
        if (next_bullet_index >= MAX_BULLETS) {
            next_bullet_index = 0;
        }

        for (int i = 0; i < BULLETS_PER_SINGLE_SHOT; i++) {
            int index = next_bullet_index + i;

            var bullet = bullets[index];

            bullet.is_active = true;
            bullet.collider = new PointCollider { 
                position = origin + (direction * Game.randomf() * MAX_DISTANCE_DISCREPANCY)
            };
            bullet.direction = World.rotate_vector2d_by_angle(direction, Game.randomf() * BULLET_RADIANS_DEVIATION);
        }

        next_bullet_index += BULLETS_PER_SINGLE_SHOT;
    }

    public static void update() {
        foreach (var bullet in bullets) {
            if (!bullet.is_active) {
                continue;
            }

            bullet.collider.position += bullet.direction * Game.delta_time * Bullet.SPEED;

            // detect collision with enemies
            foreach (var enemy in EnemiesManager.get_enemies()) {
                if (!enemy.is_active) {
                    continue;
                }

                if (CollisionDetection.check_circle_point_collision(enemy.collider, bullet.collider)) {
                    EnemiesManager.kill(enemy);
                    destroy_bullet(bullet);
                    break;
                }
            }

            if (!bullet.is_active) {
                continue;
            }

            // detect collision with enemies

            if (!World.is_position_in_boundries(bullet.collider.position)) {
                destroy_bullet(bullet);
            }
        }
    }

    public static void destroy_bullet(Bullet bullet) {
        bullet.is_active = false;
    }

    public static void render(SpriteBatch sprite_batch) {
        foreach (var bullet in bullets) {
            if (!bullet.is_active) {
                continue;
            }

            if (bullet.is_active) {
                var (x, y) = World.get_screen_position(bullet.collider.position);
                sprite_batch.Draw(
                    _bullet_texture,
                    new Rectangle(x - 4, y - 4, 8, 8),
                    Color.White);
            }
        }
    }
}

public static class Scythe {
    private const int HIT_POINTS_NUMBER = 11;
    private const float DELAY_BETWEEN_HIT_POINTS = 0.035f;
    private const float HIT_POINT_ACTIVITY_TIME = 0.25f;
    
    private static float[] scythe_range_ratios = new float[HIT_POINTS_NUMBER];
    private static float[] smooth_radians = new float[HIT_POINTS_NUMBER];
    private static ScytheHitPoint[] hit_points = new ScytheHitPoint[HIT_POINTS_NUMBER];

    private static Texture2D _pixel_texture;

    static Scythe() {
        for (int i = 0; i < HIT_POINTS_NUMBER; i++) {
            hit_points[i] = new ScytheHitPoint { is_active = false };
        }

        { 
            const float MIN_RANGE_RATIO = 0.75f;
            const float MAX_RANGE_RATIO = 1.0f;
            const float STEPS = 6;

            float step = (MAX_RANGE_RATIO - MIN_RANGE_RATIO) / (STEPS - 1);

            for (int i = 0; i < (STEPS - 1); i++) {
                scythe_range_ratios[i] = MAX_RANGE_RATIO - (i * step);
                scythe_range_ratios[HIT_POINTS_NUMBER - i - 1] = MAX_RANGE_RATIO - (i * step);
            }
            scythe_range_ratios[5] = MIN_RANGE_RATIO;
        }
        { 
            const float MIN_SMOOTH_RADIAN = 0.0f;
            const float MAX_SMOOTH_RADIAN = 1.0f;
            const float steps = HIT_POINTS_NUMBER;

            float step = (MAX_SMOOTH_RADIAN - MIN_SMOOTH_RADIAN) / (steps - 1);
            for (int i = 0; i < steps; i++) {
                smooth_radians[i] = MIN_SMOOTH_RADIAN + (i * step);
            }
        }
    }

    public static void load_content(GraphicsDevice device) {
        _pixel_texture = new Texture2D(device, 1, 1);
        _pixel_texture.SetData(new[] { new Color(255, 255, 255, 255) });
    }
     
    public static void dispose() {
        _pixel_texture.Dispose();    
    }

    public static void hit(Vector2 origin, Vector2 facing, double now) {
        const float ARCH_DISTANCE = 0.75f;
        Vector2 first_hit_point_facing = World.rotate_vector2d_by_angle(facing, 1.57f);
        float full_arch = MathF.PI;

        for (int i = 0; i < HIT_POINTS_NUMBER; i++) {
            var hit_point = hit_points[i];
                
            hit_point.activity_start_time = now + (i * DELAY_BETWEEN_HIT_POINTS);
            hit_point.activity_end_time = hit_point.activity_start_time + HIT_POINT_ACTIVITY_TIME;

            Vector2 rotated_facing = World.rotate_vector2d_by_angle(first_hit_point_facing, -(full_arch * smooth_radians[i]));

            hit_point.collider.position = origin + (rotated_facing * (ARCH_DISTANCE * scythe_range_ratios[i]));
        }
    }

    public static void update(GameTime game_time) {
        foreach (var hit_point in hit_points) {
            hit_point.is_active = hit_point.activity_start_time <= game_time.TotalGameTime.TotalSeconds
                && hit_point.activity_end_time >= game_time.TotalGameTime.TotalSeconds;
        }

        foreach (var hit_point in hit_points) {
            if (!hit_point.is_active) {
                continue;
            }

            foreach (var enemy in EnemiesManager.get_enemies()) {
                if (!enemy.is_active) {
                    continue;
                }

                if (CollisionDetection.check_circle_point_collision(enemy.collider, hit_point.collider)) {
                    EnemiesManager.kill(enemy);
                    continue;
                }
            }
        }
    }

    public static void render(SpriteBatch sprite_batch) {
        foreach (var hit_point in hit_points) {
            if (!hit_point.is_active) {
                continue;
            }

            var (x, y) = World.get_screen_position(hit_point.collider.position);
            sprite_batch.Draw(
                _pixel_texture,
                new Rectangle(x - 4, y - 4, 8, 8),
                Color.Blue);
        }
    }
}

public static class Laser {

}

public class Bullet { 
    public static float SPEED = 5.0f;

    public bool is_active;
    public PointCollider collider;
    //public Vector2 position;
    public Vector2 direction;
}

public class ScytheHitPoint { 
    public PointCollider collider;
    public double activity_start_time;
    public double activity_end_time;
    public bool is_active;
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
