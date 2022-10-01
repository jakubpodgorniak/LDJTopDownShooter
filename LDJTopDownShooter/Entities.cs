﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LDJTopDownShooter;

public enum WeaponType {
    Shotgun = 0,
    Scythe = 1,
    Laser = 2
}

public static class CharacterHelper {
    public static float facing_2_rotation(Vector2 facing)
        => MathF.Atan2(facing.Y, facing.X);
}

public class Player
{
    public const float MOVEMENT_SPEED = 2.0f;
    public const float ROTATION_SPEED = 10.0f;

    public Vector2 position;
    public Vector2 facing = Vector2.UnitX;
    public bool is_dead = false;

    public float get_rotation() => CharacterHelper.facing_2_rotation(facing);

    public void update() {
        // movement
        Vector2 move = Vector2.Zero;
        bool moved = false;
        bool moveLeft = CustomInput.is_key_pressed(Keys.A);
        bool moveUp = CustomInput.is_key_pressed(Keys.W);
        bool moveRight = CustomInput.is_key_pressed(Keys.D);
        bool moveDown = CustomInput.is_key_pressed(Keys.S);

        if (moveLeft && !moveRight) {
            move -= Vector2.UnitX;
            moved = true;
        }

        if (moveRight && !moveLeft) {
            move += Vector2.UnitX;
            moved = true;
        }

        if (moveUp && !moveDown) {
            move -= Vector2.UnitY;
            moved = true;
        }

        if (moveDown && !moveUp) {
            move += Vector2.UnitY;
            moved = true;
        }

        if (moved) {
            move.Normalize();
            Vector2 new_position = position + (move * Game.delta_time * MOVEMENT_SPEED);

            position = World.clamp_to_boundries(new_position);
        }

        // movement

        //rotation
        Vector2 mouse_pos = CustomInput.mouse_vec2;
        Vector2 mouse_world_pos = World.screen_2_world(mouse_pos);
        Vector2 new_facing = mouse_world_pos - position;
        new_facing.Normalize();
        facing = Vector2.Lerp(facing, new_facing, ROTATION_SPEED * Game.delta_time);

        //if (CustomInput.is_key_pressed(Keys.Q)) {
        //    float rotation_angle = ROTATION_SPEED * Game.delta_time;

        //    facing = World.rotate_vector2d_by_angle(facing, rotation_angle);
        //} else if (CustomInput.is_key_pressed(Keys.E)) {
        //    float rotation_angle = (-1f) * ROTATION_SPEED * Game.delta_time;

        //    facing = World.rotate_vector2d_by_angle(facing, rotation_angle);
        //}
        //rotation
    }
}

public struct Tile {
    public float heat;
    public Vector2 guide_direction;
}

public static class EnemiesManager {
    public const int MAX_ENEMIES_COUNT = 300;
    private static Enemy[] enemies = new Enemy[MAX_ENEMIES_COUNT];
    private static int next_enemy_id = 0;

    private static List<Enemy> enemies_going_in = new List<Enemy>(MAX_ENEMIES_COUNT);
    private static List<Enemy> fighting_enemies = new List<Enemy>(MAX_ENEMIES_COUNT);

    public const int SPAWNERS_COUNT = 9;
    private static EnemySpawner[] spawners = new EnemySpawner[SPAWNERS_COUNT];

    public const int TILE_MAP_WIDTH = 44;
    public const int TILE_MAP_HEIGHT = 36;
    public const float TILE_MAP_TILE_SIZE = 0.25f;
    public static Tile[,] tile_map = new Tile[TILE_MAP_HEIGHT, TILE_MAP_WIDTH];
    
    private static Texture2D _pixel_texture;
    private static Texture2D _enemy_texture;
    private static Texture2D _circle_texture;
    private static Texture2D _shadow_texture;
    private static SpriteFont _arial10;

    private static readonly Vector2 right_up_unit_vec2 = new Vector2(1, -1);
    private static readonly Vector2 right_down_unit_vec2 = new Vector2(1, 1);
    private static readonly Vector2 left_up_unit_vec2 = new Vector2(-1, -1);
    private static readonly Vector2 left_down_unit_vec2 = new Vector2(-1, 1);

    static EnemiesManager() {
        right_up_unit_vec2.Normalize();
        right_down_unit_vec2.Normalize();
        left_up_unit_vec2.Normalize();
        left_down_unit_vec2.Normalize();

        for (int i = 0; i < enemies.Length; i++) {
            enemies[i] = new() {
                state = EnemyState.None
            };
        }

        for (int y = 0; y < TILE_MAP_HEIGHT; y++) {
            for (int x = 0; x < TILE_MAP_WIDTH; x++) {
                tile_map[y, x] = new Tile {
                    heat = 0f,
                    guide_direction = Vector2.Zero
                };
            }
        }

        // up 1
        spawners[0] = new EnemySpawner { position = new Vector2(2.3f, 0.1f), direction = new Vector2(0f, 1f) };
        // up 2
        spawners[1] = new EnemySpawner { position = new Vector2(7.2f, 0.1f), direction = new Vector2(0f, 1f) };

        // right 1
        spawners[2] = new EnemySpawner { position = new Vector2(9.5f, 1.5f), direction = new Vector2(-1f, 0f) };

        // right 2
        spawners[3] = new EnemySpawner { position = new Vector2(9.5f, 4f), direction = new Vector2(-1f, 0f) };

        // down 1
        spawners[4] = new EnemySpawner { position = new Vector2(7.2f, 5.5f), direction = new Vector2(0f, -1f) };

        // down 2
        spawners[5] = new EnemySpawner { position = new Vector2(4.9f, 5.5f), direction = new Vector2(0f, -1f) };

        // down 3
        spawners[6] = new EnemySpawner { position = new Vector2(2.3f, 5.5f), direction = new Vector2(0f, -1f) };

        // left 1
        spawners[7] = new EnemySpawner { position = new Vector2(0.1f, 4f), direction = new Vector2(1f, 0f) };

        // left 2
        spawners[8] = new EnemySpawner { position = new Vector2(0.1f, 1.5f), direction = new Vector2(1f, 0f) };
    }

    public static Enemy[] get_enemies() => enemies;

    public static void load_content(GraphicsDevice graphics, ContentManager content) {
        _pixel_texture = new Texture2D(graphics, 1, 1);
        _pixel_texture.SetData(new[] { Color.White });
        _enemy_texture = content.Load<Texture2D>("enemy");
        _shadow_texture = content.Load<Texture2D>("shadow");
        _arial10 = content.Load<SpriteFont>("fonts/arial10");
        _circle_texture = content.Load<Texture2D>("circle");
    }

    public static void dispose() {
     // _arial10.Dispose(); ??
        _enemy_texture.Dispose();
        _shadow_texture.Dispose();
        _pixel_texture.Dispose();
        _circle_texture.Dispose();
    }

    public static void reset() {
        foreach (var enemy in enemies) {
            enemy.state = EnemyState.None;
        }
        enemies_going_in.Clear();
        fighting_enemies.Clear();
        next_enemy_id = 0;
    }

    public static void spawn_random_enemy() {
        if (next_enemy_id == MAX_ENEMIES_COUNT) {
            return;
        }

        var spawner = spawners[Game.random.Next(0, SPAWNERS_COUNT)];
        var enemy = enemies[next_enemy_id];
        next_enemy_id++;

        enemy.position = new Vector2(spawner.position.X, spawner.position.Y);
        enemy.facing = spawner.direction;
        enemy.movement_speed = 0.5f + Game.randomf();
        enemy.rotation_speed = 3f;
        enemy.state = EnemyState.GoesIn;
        enemy.collider = new CircleCollider {
            position = enemy.position,
            radius = 0.25f
        };
    }

    private static (int tile_x, int tile_y) get_tile_position(Vector2 position) {
        int tile_map_x = (int)MathF.Floor(position.X / TILE_MAP_TILE_SIZE);
        tile_map_x = Math.Clamp(tile_map_x, 0, TILE_MAP_WIDTH - 1);
        int tile_map_y = (int)Math.Floor(position.Y / TILE_MAP_TILE_SIZE);
        tile_map_y = Math.Clamp(tile_map_y, 0, TILE_MAP_HEIGHT - 1);

        return (tile_map_x, tile_map_y);
    }

    public static void update(Player player) {
        const float OUT_OF_BOUNDRIES_HEAT = 10f;

        //var (player_tile_x, player_tile_y) = get_tile_position(player.position);
        //tile_map[player_tile_y, player_tile_x].heat += 0.2f;

        for (int y = 0; y < TILE_MAP_HEIGHT; y++) {
            for (int x = 0; x < TILE_MAP_WIDTH; x++) {
                float top_heat = y == 0 ? OUT_OF_BOUNDRIES_HEAT : tile_map[y - 1, x].heat;
                float right_heat = x == (TILE_MAP_WIDTH - 1) ? OUT_OF_BOUNDRIES_HEAT : tile_map[y, x + 1].heat;
                float bottom_heat = y == (TILE_MAP_HEIGHT - 1) ? OUT_OF_BOUNDRIES_HEAT : tile_map[y + 1, x].heat;
                float left_heat = x == 0 ? OUT_OF_BOUNDRIES_HEAT : tile_map[y, x - 1].heat;

                float top_left_heat = (x == 0 || y == 0) ? OUT_OF_BOUNDRIES_HEAT : tile_map[y - 1, x - 1].heat;
                float top_right_heat = (x == (TILE_MAP_WIDTH - 1) || y == 0) ? OUT_OF_BOUNDRIES_HEAT : tile_map[y - 1, x + 1].heat;
                float bottom_right_heat = (x == (TILE_MAP_WIDTH - 1) || (y == TILE_MAP_HEIGHT - 1)) ? OUT_OF_BOUNDRIES_HEAT : tile_map[y + 1, x + 1].heat;
                float bottom_left_heat = (x == 0 || (y == TILE_MAP_HEIGHT - 1)) ? OUT_OF_BOUNDRIES_HEAT : tile_map[y + 1, x - 1].heat;

                tile_map[y, x].guide_direction = (Vector2.UnitY * top_heat)
                    + (Vector2.UnitX * (-1) * right_heat)
                    + (Vector2.UnitY * (-1) * bottom_heat)
                    + (Vector2.UnitY * left_heat)
                    + (right_down_unit_vec2 * top_left_heat)
                    + (left_down_unit_vec2 * top_right_heat)
                    + (right_up_unit_vec2 * bottom_left_heat)
                    + (left_up_unit_vec2 * bottom_right_heat);
            }
        }

        enemies_going_in.Clear();
        fighting_enemies.Clear();

        foreach (var enemy in enemies) {
            if (enemy.state == EnemyState.GoesIn) {
                enemies_going_in.Add(enemy);
            } else if (enemy.state == EnemyState.FightsJunky) {
                fighting_enemies.Add(enemy);
            }
        }

        foreach (var enemy in enemies_going_in) {
            Vector2 move = enemy.facing * Game.delta_time * enemy.movement_speed;
            enemy.position += move;
            enemy.collider.position = enemy.position;

            if (World.is_position_in_boundries(enemy.position)) {
                enemy.state = EnemyState.FightsJunky;
            }
        }

        foreach (var enemy in fighting_enemies) {
            var (tile_x, tile_y) = get_tile_position(enemy.position);

            Vector2 towards_player_dir = player.position - enemy.collider.position;
            towards_player_dir.Normalize();
            Vector2 guide = tile_map[tile_y, tile_x].guide_direction;

            if (guide.LengthSquared() > 1e-6) {
                guide.Normalize();
            }

            Vector2 move_dir = towards_player_dir + guide;
            Vector2 move = enemy.movement_speed * Game.delta_time * move_dir;

            Vector2 new_position = World.clamp_to_boundries(enemy.position + move);

            enemy.position = new_position;
            enemy.collider.position = enemy.position;

            Vector2 move_direction = move;
            move_direction.Normalize();     // new facing

            enemy.facing = Vector2.Lerp(enemy.facing, move_direction, enemy.rotation_speed * Game.delta_time);

            if (Vector2.DistanceSquared(player.position, enemy.position) < 0.1f) {
                player.is_dead = true;
            }
        }

        for (int y = 0; y < TILE_MAP_HEIGHT; y++) {
            for (int x = 0; x < TILE_MAP_WIDTH; x++) {
                tile_map[y, x].heat = 0f;
            }
        }

        for (int i = 0; i < next_enemy_id; i++) {
            var enemy = enemies[i];

            if (enemy.state == EnemyState.None) {
                continue;
            }

            var (tile_x, tile_y) = get_tile_position(enemy.position);

            tile_map[tile_y, tile_x].heat += 1f;
        }
    }

    public static void render_enemies_going_in(SpriteBatch sprite_batch, bool render_debug_data = false) {
        foreach (var enemy in enemies_going_in) {
            render_enemy_shadow(sprite_batch, enemy);
        }

        foreach (var enemy in enemies_going_in) {
            render_enemy(sprite_batch, enemy, render_debug_data);
        }
    }

    public static void render_fighting_enemies(SpriteBatch sprite_batch, bool render_debug_data = false) {
        foreach (var enemy in fighting_enemies) {
            render_enemy_shadow(sprite_batch, enemy);
        }

        foreach (var enemy in fighting_enemies) {
           render_enemy(sprite_batch, enemy, render_debug_data);
        }
    }

    private static void render_enemy_shadow(SpriteBatch sprite_batch, Enemy enemy) {
        var (x, y) = World.get_screen_position(enemy.collider.position);
        sprite_batch.Draw(
            _shadow_texture,
            new Rectangle(x - 32, y - 32, 64, 64),
            Color.White);
    }

    private static void render_enemy(SpriteBatch sprite_batch, Enemy enemy, bool render_debug_data = false) {
        var (x, y) = World.get_screen_position(enemy.collider.position);
        float rotation = CharacterHelper.facing_2_rotation(enemy.facing);

        sprite_batch.Draw(
            _enemy_texture,
            new Rectangle(x, y, 48, 48),
            null,
            Color.White,
            rotation,
            new Vector2(24, 24),
            SpriteEffects.None,
            0);

        if (render_debug_data) {
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

    public static void render_heat_map(SpriteBatch sprite_batch) {
        for (int y = 0; y < TILE_MAP_HEIGHT; y++) {
            for (int x = 0; x < TILE_MAP_WIDTH; x++) {
                float value = tile_map[y, x].heat;

                int x_pos = x * 32;
                int y_pos = y * 32;
                int width = 32;
                int height = 32;
                float red = value * 0.1f;
                float alpha = value * 0.1f * 0.75f;

                sprite_batch.Draw(_pixel_texture, new Rectangle(x_pos, y_pos, width, height), new Color(red, 0, 0, alpha));
            }
        }
    }

    public static void render_spawners(SpriteBatch sprite_batch) {
        foreach (var spawner in spawners) {
            var (x, y) = World.get_screen_position(spawner.position);

            sprite_batch.Draw(
                _pixel_texture,
                new Rectangle(x - 6, y - 6, 12, 12),
                Color.Aqua);

            var dir_indicator_pos = spawner.position + (0.2f * spawner.direction);
            var (ind_x, ind_y) = World.get_screen_position(dir_indicator_pos);
            
            sprite_batch.Draw(
                _pixel_texture,
                new Rectangle(ind_x - 2, ind_y - 2, 4, 4),
                Color.Green);
        }
    }

    public static void gain_damage(Enemy enemy, float damage) {
        enemy.health -= damage;

        if (enemy.health <= 0) {
            Highscore.gain_score(1);
            enemy.state = EnemyState.None;
        }
    }
}

public enum EnemyState {
    None,
    GoesIn,
    FightsJunky
}

public class Enemy {
    public EnemyState state;
    public float movement_speed;
    public float rotation_speed;
    public Vector2 position;
    public Vector2 facing;
    public CircleCollider collider;
    public float health = 1.0f;
}

public class EnemySpawner {
    public Vector2 position;
    public Vector2 direction;
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
        direction.Normalize();

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
                if (enemy.state == EnemyState.None) {
                    continue;
                }

                if (CollisionDetection.check_circle_point_collision(enemy.collider, bullet.collider)) {
                    EnemiesManager.gain_damage(enemy, 10.0f);
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

    public static void reset() {
        foreach (var bullet in bullets) {
            destroy_bullet(bullet);
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
                if (enemy.state == EnemyState.None) {
                    continue;
                }

                if (CollisionDetection.check_circle_point_collision(enemy.collider, hit_point.collider)) {
                    EnemiesManager.gain_damage(enemy, 10.0f);
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
    public static float LASER_POWER = 10.0f;
    public static float LASER_LENGTH = 25.0f;
    
    private static List<Enemy> enemies_hit = new List<Enemy>(EnemiesManager.MAX_ENEMIES_COUNT);
    private static bool is_turn_on = false;

    private static Texture2D _pixel_texture;
    private static Vector2 last_shoot_origin;
    private static Vector2 last_shoot_end;

    public static void load_content(GraphicsDevice device) {
        _pixel_texture = new Texture2D(device, 1, 1);
        _pixel_texture.SetData(new[] { new Color(255, 255, 255, 255) });
    }

    public static void dispose() {
        _pixel_texture.Dispose();
    }

    public static void turn_on() {
        is_turn_on = true;
    }

    public static void turn_off() {
        is_turn_on = false;
    }

    public static void update(Vector2 origin, Vector2 direction) {
        if (!is_turn_on) {
            return;
        }

        enemies_hit.Clear();

        Vector2 end = origin + (direction * LASER_LENGTH);

        last_shoot_origin = origin;
        last_shoot_end = end;

        foreach (var enemy in EnemiesManager.get_enemies()) {
            if (enemy.state == EnemyState.None) {
                continue;
            }

            if (CollisionDetection.check_circle_line_segment_collision(enemy.collider, origin, end)) {
                enemies_hit.Add(enemy);
            }
        }

        if (enemies_hit.Count == 0) {
            return;
        }

        Enemy closest_enemy = enemies_hit[0];
        float closest_distance = Vector2.Distance(origin, closest_enemy.collider.position);

        int i = 1;
        while (i < enemies_hit.Count) {
            var enemy = enemies_hit[i];
            float distance = Vector2.Distance(origin, enemy.collider.position);

            if (distance < closest_distance) {
                closest_distance = distance;
                closest_enemy = enemy;
            }

            i++;
        }

        last_shoot_end = origin + (direction * Vector2.Distance(closest_enemy.collider.position, origin));

        EnemiesManager.gain_damage(closest_enemy, Game.delta_time * LASER_POWER);
    }

    public static void render(SpriteBatch sprite_batch) {
        if (!is_turn_on) {
            return;
        }

        Vector2 laser_direction = last_shoot_end - last_shoot_origin;
        float radians = MathF.Atan2(laser_direction.Y, laser_direction.X);
        var (x, y) = World.get_screen_position(last_shoot_origin);
        sprite_batch.Draw(
            _pixel_texture,
            new Rectangle(
                x,
                y,
                (int)Math.Floor(laser_direction.Length() * World.PIXELS_PER_UNIT),
                3),
            null,
            Color.Red,
            radians,
            Vector2.Zero,
            SpriteEffects.None,
            0);
    }
}

public class Bullet { 
    public static float SPEED = 10.0f;

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

    public static bool check_circle_line_segment_collision(CircleCollider circle, Vector2 origin, Vector2 end) {
        if (check_circle_point_collision(circle, new PointCollider { position = origin })) {
            return true;
        }

        if (check_circle_point_collision(circle, new PointCollider { position = end })) {
            return true;
        }

        Vector2 direction = end - origin;
        Vector2 lc = circle.position - origin;
        Vector2 p = project(lc, direction);
        Vector2 nearest = origin + p;

        return check_circle_point_collision(circle, new PointCollider { position = nearest })
            && p.Length() <= direction.Length()
            && 0 <= Vector2.Dot(p, direction);
    }

    private static Vector2 project(Vector2 project, Vector2 onto) {
        float d = Vector2.Dot(onto, onto);
        if (0 < d) {
            float dp = Vector2.Dot(project, onto);

            return Vector2.Multiply(onto, dp / d);
        }

        return onto;
    }
}
