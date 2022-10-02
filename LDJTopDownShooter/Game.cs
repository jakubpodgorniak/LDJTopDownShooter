using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LDJTopDownShooter {

    public enum GameState {
        Stopped,
        Running
    }

    public class Sound {
        public SoundEffect sound_effect;
        public float volume;

        public void Play() {
            sound_effect.Play(volume, 0, 0);
        }
    }

    public static class Sounds {
        public static Sound SHOTGUN;
        public static Sound SCYTHE;
        public static Sound LASER;
        public static Sound DESTORY;
        public static Sound TEN_SECONDS;

        public static void load(ContentManager content) {
            SHOTGUN = new() {
                sound_effect = content.Load<SoundEffect>("sounds/shotgun"),
                volume = 0.5f
            };
            SCYTHE = new() {
                sound_effect = content.Load<SoundEffect>("sounds/scythe"),
                volume = 0.5f
            };
            LASER = new() {
                sound_effect = content.Load<SoundEffect>("sounds/laser"),
                volume = 0.5f
            };
            DESTORY = new() {
                sound_effect = content.Load<SoundEffect>("sounds/destroy"),
                volume = 0.5f
            };
            TEN_SECONDS = new() {
                sound_effect = content.Load<SoundEffect>("sounds/ten_seconds"),
                volume = 1f
            };
        } 

        public static void dispose() {
            SHOTGUN.sound_effect.Dispose();
            SCYTHE.sound_effect.Dispose();
            LASER.sound_effect.Dispose();
            DESTORY.sound_effect.Dispose();
            TEN_SECONDS.sound_effect.Dispose();
        }
    }

    public class Game : Microsoft.Xna.Framework.Game
    {
        private static GameState State  = GameState.Stopped;

        public const double TEN_SECONDS = 10.0;
        public const double TEN_SECONDS_INVERSE = 1.0 / TEN_SECONDS;

        public static float delta_time { get; private set; }
        public static Random random = new Random();

        private static double game_start_time;
        private static double game_end_time;
        private static int ten_seconds_counter = 0;
        public static int TEN_SECONDS_LEVEL => ten_seconds_counter;

        public static float randomf() => (float)random.NextDouble();

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _sprite_batch;

        private SpriteFont _arial10;
        private SpriteFont _rajdhani28;
        private SpriteFont _rajdhani24;
        private SpriteFont _rajdhani16;
        private Texture2D _character_texture;
        private Texture2D _ui_texture;
        private Texture2D _pixel_texture;
        private Texture2D _map_texture;
        private Texture2D _start_ui;
        private Texture2D _jason_texture;
        private Texture2D _explosion_texture;
        private Player _player;
        private WeaponType _current_weapon;
        private RenderTarget2D _map_render_target;
        private RenderTarget2D _ui_render_target;
        private double counter_start_seconds;
        private double ten_seconds_progress = 0.0;
        
        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = false;

            base.Initialize();

            _player = new Player() { position = new Vector2 (5, 2.8125f), immortal = false };
            _current_weapon = WeaponType.Shotgun;
        }

        protected override void LoadContent()
        {
            _map_render_target = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            _ui_render_target = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            _sprite_batch = new SpriteBatch(GraphicsDevice);
            _arial10 = Content.Load<SpriteFont>("fonts/Arial10");
            _rajdhani28 = Content.Load<SpriteFont>("fonts/Rajdhani28");
            _rajdhani24 = Content.Load<SpriteFont>("fonts/Rajdhani24");
            _rajdhani16 = Content.Load<SpriteFont>("fonts/Rajdhani16");
            _character_texture = Content.Load<Texture2D>("player");
            _ui_texture = Content.Load<Texture2D>("ui");
            _map_texture = Content.Load<Texture2D>("map");
            _start_ui = Content.Load<Texture2D>("start-ui");
            _jason_texture = Content.Load<Texture2D>("jason");
            _explosion_texture = Content.Load<Texture2D>("explosion");
            _pixel_texture = new Texture2D(GraphicsDevice, 1, 1);
            _pixel_texture.SetData(new Color[] {Color.White});

            World.load_content(GraphicsDevice);
            EnemiesManager.load_content(GraphicsDevice, Content);
            Shotgun.load_content(Content);
            Scythe.load_content(GraphicsDevice);
            Laser.load_content(GraphicsDevice);
            Sounds.load(Content);
        }

        protected override void UnloadContent() {
            base.UnloadContent();

            // _arial10.Dispose(); ??
            _character_texture.Dispose();
            _ui_texture.Dispose();
            _pixel_texture.Dispose();
            _jason_texture.Dispose();
            _start_ui.Dispose();
            _explosion_texture.Dispose();

            _map_render_target.Dispose();
            _ui_render_target.Dispose();

            EnemiesManager.dispose();
            World.dispose();
            Shotgun.dispose();
            Scythe.dispose();
            Laser.dispose();
            Sounds.dispose();
        }

        bool enable_weapon_randomizer = true;
        //bool enable_weapon_randomizer = true;
        bool render_enemies_debug_data = false;

        protected override void Update(GameTime game_time)
        {
            CustomInput.update(Keyboard.GetState(), Mouse.GetState());
            delta_time = (float)game_time.ElapsedGameTime.TotalSeconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (State == GameState.Stopped) {
                UpdateStoppedGame(game_time);
            } else {
                UpdateRunningGame(game_time);
            }

            base.Update(game_time);
        }

        bool were_keys_pressed = false;

        private void UpdateStoppedGame(GameTime game_time) {
            if (!were_keys_pressed && (
                Keyboard.GetState().GetPressedKeyCount() > 0
                || CustomInput.is_mouse_button_down(MouseButton.Left)
                || CustomInput.is_mouse_button_down(MouseButton.Right)
                || CustomInput.is_mouse_button_down(MouseButton.Middle))) {
                
                _player.revive();
                Highscore.reset_score();
                ten_seconds_progress = 0;
                counter_start_seconds = game_time.TotalGameTime.TotalSeconds;
                randomize_weapon(any: true);
                game_start_time = game_time.TotalGameTime.TotalSeconds;
                State = GameState.Running;
            }

            were_keys_pressed = Keyboard.GetState().GetPressedKeyCount() > 0;
        }

        private void UpdateRunningGame(GameTime game_time) {
            double total_seconds = game_time.TotalGameTime.TotalSeconds;
            double time_till_last_ten_seconds = (total_seconds - counter_start_seconds);
            ten_seconds_progress = time_till_last_ten_seconds * TEN_SECONDS_INVERSE;

            if (time_till_last_ten_seconds > TEN_SECONDS) {
                // TEN SECONDS HAVE PASSED
                ten_seconds_counter += 1;

                Sounds.TEN_SECONDS.Play();

                if (enable_weapon_randomizer) {
                    randomize_weapon();
                }

                counter_start_seconds = total_seconds;
            }

            _player.update(_current_weapon);
            EnemiesManager.update(_player);

            if (_player.get_is_dead()) {              // finish game
                were_keys_pressed = Keyboard.GetState().GetPressedKeyCount() > 0;
                EnemiesManager.reset();
                Shotgun.reset();
                Explosions.reset();
                Laser.turn_off();
                _player.position = new Vector2(5, 2.8125f);
                set_weapon(WeaponType.Shotgun);
                ten_seconds_counter = 0;
                game_end_time = game_time.TotalGameTime.TotalSeconds;
                State = GameState.Stopped;
                return;
            }

            // enemies spawn
            double current_game_time = game_time.TotalGameTime.TotalSeconds;

            if ((current_game_time - last_spawn_time) > getNextSpawnTime()) {

                EnemiesManager.spawn_random_enemy();

                last_spawn_time = game_time.TotalGameTime.TotalSeconds;
            }

            // enemies spawn
            //// change weapon
            //if (CustomInput.is_key_down(Keys.D1)) {
            //    set_weapon(WeaponType.Shotgun);
            //} else if (CustomInput.is_key_down(Keys.D2)) {
            //    set_weapon(WeaponType.Scythe);
            //} else if (CustomInput.is_key_down(Keys.D3)) {
            //    set_weapon(WeaponType.Laser);
            //}
            // change weapon

            // shooting
            if (CustomInput.is_mouse_button_down(MouseButton.Left)) {
                if (_current_weapon == WeaponType.Shotgun) {
                    Shotgun.fire(_player.shotgun_shoot_point_world, _player.facing, game_time);
                } else if (_current_weapon == WeaponType.Scythe) {
                    Scythe.hit(_player, game_time.TotalGameTime.TotalSeconds);
                } else if (_current_weapon == WeaponType.Laser) {
                    Laser.turn_on();
                }
            }

            if (CustomInput.is_mouse_button_up(MouseButton.Left)) {
                if (_current_weapon == WeaponType.Laser) {
                    Laser.turn_off();
                }
            }

            Scythe.update(_player, game_time);
            Shotgun.update();

            var mouse_world_pos = World.screen_2_world(CustomInput.mouse_vec2);
            Laser.update(_player.laser_shoot_point_world, mouse_world_pos);
            Explosions.update(game_time);

            // shooting

            // other
            if (CustomInput.is_key_down(Keys.P)) {
                render_enemies_debug_data = !render_enemies_debug_data;
            }
        }

        private void randomize_weapon(bool any = false) {
            var other_weapons = new List<WeaponType>();

            if (any || _current_weapon != WeaponType.Shotgun) {
                other_weapons.Add(WeaponType.Shotgun);
            }

            if (any || _current_weapon != WeaponType.Scythe) {
                other_weapons.Add(WeaponType.Scythe);
            }

            if (any || _current_weapon != WeaponType.Laser) {
                other_weapons.Add(WeaponType.Laser);
            }

            set_weapon(other_weapons[random.Next(0, 2)]);
        }

        private void set_weapon(WeaponType new_weapon) {
            if (_current_weapon == WeaponType.Laser) {
                Laser.turn_off();
            }

            _current_weapon = new_weapon;
        }

        private static double getNextSpawnTime() {
            return Math.Max(spawn_every_seconds_min, spawn_every_seconds_base - (TEN_SECONDS_LEVEL * spawn_every_seconds_gain));
        }

        private static double last_spawn_time;
        private static double spawn_every_seconds_base = 1f;
        private static double spawn_every_seconds_gain = 0.1f;
        private static double spawn_every_seconds_min = 0.2f;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_map_render_target);
            GraphicsDevice.Clear(Color.Black);

            _sprite_batch.Begin(blendState: BlendState.AlphaBlend);

            // floor
            _sprite_batch.Draw(
                _map_texture,
                new Rectangle(0, 0, 1280, 720),
                new Rectangle(0, 720, 1280, 720),
                Color.White);
       
            EnemiesManager.render_enemies_going_in(_sprite_batch, render_enemies_debug_data);
            Shotgun.render(_sprite_batch);
            //Scythe.render(_sprite_batch);
            Laser.render(_sprite_batch);

            // walls and doors
            _sprite_batch.Draw(
                _map_texture,
                new Rectangle(0, 0, 1280, 720),
                new Rectangle(0, 0, 1280, 720),
                Color.White);

            // render jason
            var (x, y) = World.get_screen_position(_player.position);
            float rotation = _player.get_rotation();
            Rectangle junky_src_rect;
            if (_current_weapon == WeaponType.Shotgun) {
                junky_src_rect = new Rectangle(256, 0, 128, 128);
            } else if (_current_weapon == WeaponType.Scythe) {
                junky_src_rect = new Rectangle(128, 0, 128, 128);
            } else {
                junky_src_rect = new Rectangle(0, 0, 128, 128);
            }

            //shadow
            _sprite_batch.Draw(
                _jason_texture,
                new Rectangle(x, y, 128, 128),
                new Rectangle(384, 0, 128, 128),
                new Color(0, 0, 0, 0.2f),
                0,
                new Vector2(64, 64),
                SpriteEffects.None,
                0);

            if (_current_weapon == WeaponType.Scythe) {
                var (sx, sy) = World.get_screen_position(_player.scythe_pivot_world);

                float scythe_rotation = rotation;

                if (Scythe.is_animated(gameTime)) {
                    scythe_rotation -= (float)Scythe.get_rotation(gameTime);
                } 

                _sprite_batch.Draw(
                    _jason_texture,
                    new Rectangle(sx, sy, 128, 128),
                    new Rectangle(0, 128, 128, 128),
                    Color.White,
                    scythe_rotation,
                    new Vector2(64, 64),
                    SpriteEffects.None,
                    0);
            }

            // jason
            _sprite_batch.Draw(
                _jason_texture,
                new Rectangle(x, y, 128, 128),
                junky_src_rect,
                Color.White,
                rotation,
                new Vector2(64, 64),
                SpriteEffects.None,
                0);

            // shotgun debug
            if(false) {
                var (sx, sy) = World.get_screen_position(_player.shotgun_shoot_point_world);
                _sprite_batch.Draw(
                    _pixel_texture,
                    new Rectangle(sx - 2, sy - 2, 4, 4),
                    null,
                    Color.Red);
            }

            // laser debug
            if (false) {
                var (sx, sy) = World.get_screen_position(_player.laser_shoot_point_world);
                _sprite_batch.Draw(
                    _pixel_texture,
                    new Rectangle(sx - 2, sy - 2, 4, 4),
                    null,
                    Color.Red);
            }

            // scythe debug
            if (false) {
                var (sx, sy) = World.get_screen_position(_player.scythe_pivot_world);
                _sprite_batch.Draw(
                    _pixel_texture,
                    new Rectangle(sx - 2, sy - 2, 4, 4),
                    null,
                    Color.Red);

            }

            // end render jason

            EnemiesManager.render_fighting_enemies(_sprite_batch, render_enemies_debug_data);

            if (render_enemies_debug_data) {
                EnemiesManager.render_heat_map(_sprite_batch);
                EnemiesManager.render_spawners(_sprite_batch);
            }

            Explosions.render(_sprite_batch, _explosion_texture);

            //World.render(_sprite_batch);

            _sprite_batch.End();

            GraphicsDevice.SetRenderTarget(_ui_render_target);
            GraphicsDevice.Clear(Color.Transparent);
            _sprite_batch.Begin(blendState: BlendState.AlphaBlend);

            if (State == GameState.Stopped) {
                _sprite_batch.Draw(_start_ui, new Rectangle(0 ,0, 1280, 720), new Rectangle(0, 0, 1280, 720), Color.White);

                if (Highscore.any_score) {
                    Highscore.render_high_score(_sprite_batch, _pixel_texture, _rajdhani24, 
                        seconds_to_minutes_and_seconds(game_end_time - game_start_time));
                }
            } else if (State == GameState.Running) {
                // weapon icon
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 0, 128, 128), new Rectangle(256, 0, 128, 128), Color.White);

                if (_current_weapon == WeaponType.Shotgun) {
                    _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 0, 128, 128), new Rectangle(0, 256, 128, 128), Color.White);
                } else if (_current_weapon == WeaponType.Scythe) {
                    _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 0, 128, 128), new Rectangle(128, 128, 128, 128), Color.White);
                } else if (_current_weapon == WeaponType.Laser) {
                    _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 0, 128, 128), new Rectangle(0, 128, 128, 128), Color.White);
                }

                // ten seconds progres bar background
                var progress_bar_segment_src = new Rectangle(256, 128, 128, 64);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 128, 128, 64), progress_bar_segment_src, Color.White);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 192, 128, 64), progress_bar_segment_src, Color.White);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 256, 128, 64), progress_bar_segment_src, Color.White);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 320, 128, 64), progress_bar_segment_src, Color.White);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 384, 128, 64), progress_bar_segment_src, Color.White);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 448, 128, 64), progress_bar_segment_src, Color.White);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 512, 128, 64), progress_bar_segment_src, Color.White);
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 576, 128, 64), progress_bar_segment_src, Color.White);

                // question mark
                _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 600, 128, 128), new Rectangle(128, 0, 128, 128), Color.White);

                // ten seconds progress bar
                int PROGRESS_BAR_MAX_HEIGHT = 516;
                int progress_bar_height = (int)Math.Floor(ten_seconds_progress * PROGRESS_BAR_MAX_HEIGHT);
                _sprite_batch.Draw(
                    _pixel_texture,
                    new Rectangle(1221, 107, 8, progress_bar_height),
                    new Color(0.631f, 0.804f, 0.98f, 1.0f));

                // timer background
                _sprite_batch.Draw(
                    _ui_texture,
                    new Rectangle(90, 33, 128, 64),
                    new Rectangle(0, 384, 128, 64),
                    Color.White);

                double game_total_seconds = gameTime.TotalGameTime.TotalSeconds - game_start_time;
                string min_sec = seconds_to_minutes_and_seconds(game_total_seconds);
                Vector2 size = _rajdhani16.MeasureString(min_sec);
                _sprite_batch.DrawString(
                    _rajdhani16,
                    min_sec,
                    new Vector2(196 - size.X, 65 - (0.5f * size.Y)),
                    Color.White);

                // score
                Highscore.render(_sprite_batch, _ui_texture, _rajdhani28);

                // enemies left
                if (render_enemies_debug_data) {
                    _sprite_batch.DrawString(
                        _arial10,
                        EnemiesManager.get_enemies_left_to_spawn_number().ToString(),
                        new Vector2(25, 150),
                        Color.Black);
                }
                
                var mouse_position = CustomInput.mouse_vec2;
                int mx = (int)mouse_position.X;
                int my = (int)mouse_position.Y;

                _sprite_batch.Draw(
                    _ui_texture,
                    new Rectangle(mx - 16, my - 16, 32, 32),
                    new Rectangle(128, 256, 32, 32),
                    new Color(1f, 1f, 1f, 0.75f));
            }

            _sprite_batch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            _sprite_batch.Begin(blendState: BlendState.AlphaBlend);
            _sprite_batch.Draw(_map_render_target, new Rectangle(0, 0, _map_render_target.Width, _map_render_target.Height), Color.White);
            _sprite_batch.Draw(_ui_render_target, new Rectangle(0, 0, _ui_render_target.Width, _ui_render_target.Height), Color.White);
            _sprite_batch.End();

            base.Draw(gameTime);
        }

        private string seconds_to_minutes_and_seconds(double seconds) {
            int m = (int)Math.Floor(seconds / 60f);
            int s = (int)Math.Floor(seconds - (m * 60f));

            return $"{m} m {s} s";
        }
    }
}