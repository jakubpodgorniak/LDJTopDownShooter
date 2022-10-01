using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LDJTopDownShooter {
    public class Game : Microsoft.Xna.Framework.Game
    {
        private static Dictionary<WeaponType, string> weapons_names = new Dictionary<WeaponType, string> {
            [WeaponType.Shotgun] = "Shotgun",
            [WeaponType.Scythe] = "Scythe",
            [WeaponType.Laser] = "Laser"
        };

        public const double TEN_SECONDS = 10.0;
        public const double TEN_SECONDS_INVERSE = 1.0 / TEN_SECONDS;

        public static float delta_time { get; private set; }
        public static Random random = new Random();

        public static float randomf() => (float)random.NextDouble();

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _sprite_batch;

        private SpriteFont _arial10;
        private Texture2D _character_texture;
        private Texture2D _ui_texture;
        private Texture2D _pixel_texture;
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
            base.Initialize();

            _player = new Player() { position = new Vector2 (5, 4) };
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
            _character_texture = Content.Load<Texture2D>("player");
            _ui_texture = Content.Load<Texture2D>("ui");
            _pixel_texture = new Texture2D(GraphicsDevice, 1, 1);
            _pixel_texture.SetData(new Color[] {Color.White});

            World.load_content(GraphicsDevice);
            EnemiesManager.load_content(GraphicsDevice, Content);
            Shotgun.load_content(Content);
            Scythe.load_content(GraphicsDevice);
            Laser.load_content(GraphicsDevice);
        }

        protected override void UnloadContent() {
            base.UnloadContent();

            // _arial10.Dispose(); ??
            _character_texture.Dispose();
            _ui_texture.Dispose();
            _pixel_texture.Dispose();

            _map_render_target.Dispose();
            _ui_render_target.Dispose();

            EnemiesManager.dispose();
            World.dispose();
            Shotgun.dispose();
            Scythe.dispose();
            Laser.dispose();
        }

        bool enable_weapon_randomizer = false;
        //bool enable_weapon_randomizer = true;

        protected override void Update(GameTime game_time)
        {
            double total_seconds = game_time.TotalGameTime.TotalSeconds;
            double time_till_last_ten_seconds = (total_seconds - counter_start_seconds);
            ten_seconds_progress = time_till_last_ten_seconds * TEN_SECONDS_INVERSE;

            CustomInput.update(
                Keyboard.GetState(),
                Mouse.GetState());

            delta_time = (float)game_time.ElapsedGameTime.TotalSeconds;

            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();

            if (time_till_last_ten_seconds > TEN_SECONDS) {
                // TEN SECONDS HAVE PASSED

                if (enable_weapon_randomizer) {
                    randomize_weapon();
                }

                counter_start_seconds = total_seconds;
            }

            _player.update();
            EnemiesManager.update(_player);

            // enemies spawn
            double current_game_time = game_time.TotalGameTime.TotalSeconds;

            if ((current_game_time - last_spawn_time) > spawn_every) {

                EnemiesManager.spawn_random_enemy();

                last_spawn_time = game_time.TotalGameTime.TotalSeconds;
            }

            // enemies spawn

            // change weapon
            if (CustomInput.is_key_down(Keys.D1)) {
                set_weapon(WeaponType.Shotgun);
            } else if (CustomInput.is_key_down(Keys.D2)) {
                set_weapon(WeaponType.Scythe);
            } else if (CustomInput.is_key_down(Keys.D3)) {
                set_weapon(WeaponType.Laser);
            }
            // change weapon

            // shooting
            if (CustomInput.is_mouse_button_down(MouseButton.Left)) {
                if (_current_weapon == WeaponType.Shotgun) {
                    Shotgun.fire(_player.position, _player.facing);
                } else if (_current_weapon == WeaponType.Scythe) {
                    Scythe.hit(
                        _player.position,
                        _player.facing,
                        game_time.TotalGameTime.TotalSeconds);
                } else if (_current_weapon == WeaponType.Laser) {
                    Laser.turn_on();
                }
            }

            if (CustomInput.is_mouse_button_up(MouseButton.Left)) {
                if (_current_weapon == WeaponType.Laser) {
                    Laser.turn_off();
                }
            }

            Scythe.update(game_time);
            Shotgun.update();
            Laser.update(_player.position, _player.facing);

            // shooting

            base.Update(game_time);
        }

        private void randomize_weapon() {
            var other_weapons = new List<WeaponType>();

            if (_current_weapon != WeaponType.Shotgun) {
                other_weapons.Add(WeaponType.Shotgun);
            }

            if (_current_weapon != WeaponType.Scythe) {
                other_weapons.Add(WeaponType.Scythe);
            }

            if (_current_weapon != WeaponType.Laser) {
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

        private static double last_spawn_time;
        private static double spawn_every = 0.25;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_map_render_target);
            GraphicsDevice.Clear(Color.Gray);

            _sprite_batch.Begin(blendState: BlendState.AlphaBlend);

            World.render(_sprite_batch);

            var (x, y) = World.get_screen_position(_player.position);
            float rotation = _player.get_rotation();
            _sprite_batch.Draw(
                _character_texture,
                new Rectangle(x, y, 64, 64),
                null,
                Color.White,
                rotation,
                new Vector2(32, 32),
                SpriteEffects.None, 0);
            
            _sprite_batch.DrawString(_arial10, rotation.ToString("F2", CultureInfo.InvariantCulture), new Vector2(x, y), Color.Black);

            EnemiesManager.render(_sprite_batch);
            Shotgun.render(_sprite_batch);
            Scythe.render(_sprite_batch);
            Laser.render(_sprite_batch);
            EnemiesManager.render_heat_map(_sprite_batch);

            _sprite_batch.End();

            GraphicsDevice.SetRenderTarget(_ui_render_target);
            GraphicsDevice.Clear(Color.Transparent);
            _sprite_batch.Begin(blendState: BlendState.AlphaBlend);

            // weapon icon
            _sprite_batch.Draw(_ui_texture, new Rectangle(1160, 0, 128, 128), new Rectangle(256, 0, 128, 128), Color.White);

            // weapon name
            _sprite_batch.DrawString(
                 _arial10,
                 weapons_names[_current_weapon],
                 new Vector2(1200, 48),
                 Color.White);

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

            _sprite_batch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            _sprite_batch.Begin(blendState: BlendState.AlphaBlend);
            _sprite_batch.Draw(_map_render_target, new Rectangle(0, 0, _map_render_target.Width, _map_render_target.Height), Color.White);
            _sprite_batch.Draw(_ui_render_target, new Rectangle(0, 0, _ui_render_target.Width, _ui_render_target.Height), Color.White);
            _sprite_batch.End();

            base.Draw(gameTime);
        }
    }
}