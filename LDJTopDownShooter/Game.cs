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

        public static float delta_time { get; private set; }
        public static Random random = new Random();

        public static float randomf() => (float)random.NextDouble();

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _sprite_batch;

        private SpriteFont _arial10;
        private Texture2D _character_texture;
        private Player _player;
        private WeaponType _current_weapon;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 1024;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _player = new Player();
            _current_weapon = WeaponType.Shotgun;
        }

        protected override void LoadContent()
        {
            _sprite_batch = new SpriteBatch(GraphicsDevice);
            _arial10 = Content.Load<SpriteFont>("fonts/Arial10");
            _character_texture = Content.Load<Texture2D>("player");

            World.load_content(GraphicsDevice);
            EnemiesManager.load_content(Content);
            Shotgun.load_content(Content);
            Scythe.load_content(GraphicsDevice);
        }

        protected override void UnloadContent() {
            base.UnloadContent();

            // _arial10.Dispose(); ??
            _character_texture.Dispose();

            EnemiesManager.dispose();
            World.dispose();
            Shotgun.dispose();
            Scythe.dispose();
        }

        protected override void Update(GameTime game_time)
        {
            CustomInput.update(
                Keyboard.GetState(),
                Mouse.GetState());

            delta_time = (float)game_time.ElapsedGameTime.TotalSeconds;

            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();

            EnemiesManager.update();
            Scythe.update(game_time);
            Shotgun.update();

            // movement
            Vector2 move = Vector2.Zero;
            bool moved = false;
            bool moveLeft = keyboard.IsKeyDown(Keys.A);
            bool moveUp = keyboard.IsKeyDown(Keys.W);
            bool moveRight = keyboard.IsKeyDown(Keys.D);
            bool moveDown = keyboard.IsKeyDown(Keys.S);

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
                _player.position += (move * delta_time * Player.MOVEMENT_SPEED);
            }

            // movement

            //rotation
            if (keyboard.IsKeyDown(Keys.Q)) {
                float rotation_angle = Player.ROTATION_SPEED * delta_time;

                _player.facing = World.rotate_vector2d_by_angle(_player.facing, rotation_angle);
            } else if (keyboard.IsKeyDown(Keys.E)) {
                float rotation_angle = (-1f) * Player.ROTATION_SPEED * delta_time;

                _player.facing = World.rotate_vector2d_by_angle(_player.facing, rotation_angle);
            }
            //rotation

            // enemies spawn
            double current_game_time = game_time.TotalGameTime.TotalSeconds;

            if ((current_game_time - last_spawn_time) > spawn_every) {

                EnemiesManager.spawn_random_enemy();

                last_spawn_time = game_time.TotalGameTime.TotalSeconds;
            }

            // enemies spawn

            // change weapon
            if (CustomInput.is_key_down(Keys.D1)) {
                _current_weapon = WeaponType.Shotgun;
            } else if (CustomInput.is_key_down(Keys.D2)) {
                _current_weapon = WeaponType.Scythe;
            } else if (CustomInput.is_key_down(Keys.D3)) {
                _current_weapon = WeaponType.Laser;
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

                }
            }

            // shooting

            base.Update(game_time);
        }

        private static double last_spawn_time;
        private static double spawn_every = 1.0;

        protected override void Draw(GameTime gameTime)
        {
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

            _sprite_batch.DrawString(
                _arial10,
                weapons_names[_current_weapon],
                new Vector2(1100, 32),
                Color.Black);

            _sprite_batch.End();

            base.Draw(gameTime);
        }
    }
}