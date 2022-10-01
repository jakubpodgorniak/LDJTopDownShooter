using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LDJTopDownShooter {
    public class Game : Microsoft.Xna.Framework.Game
    {
        public static float delta_time { get; private set; }
        public static Random random = new Random();

        public static float randomf() => (float)random.NextDouble();

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

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
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _arial10 = Content.Load<SpriteFont>("fonts/Arial10");
            _character_texture = Content.Load<Texture2D>("player");

            EnemiesManager.load_content(Content);
        }

        protected override void UnloadContent() {
            base.UnloadContent();

            // _arial10.Dispose(); ??
            _character_texture.Dispose();

            EnemiesManager.dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            CustomInput.update(
                Keyboard.GetState(),
                Mouse.GetState());

            delta_time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();


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
            double current_game_time = gameTime.TotalGameTime.TotalSeconds;

            if ((current_game_time - last_spawn_time) > spawn_every) {

                EnemiesManager.spawn_random_enemy();

                last_spawn_time = gameTime.TotalGameTime.TotalSeconds;
            }

            // enemies spawn

            // shooting
            if (CustomInput.is_mouse_button_down(MouseButton.Left)) {
                if (_current_weapon == WeaponType.Shotgun) {

                } else if (_current_weapon == WeaponType.Scythe) {

                } else if (_current_weapon == WeaponType.Laser) {

                }
            }

            // shooting

            base.Update(gameTime);
        }

        private static double last_spawn_time;
        private static double spawn_every = 1.0;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend);

            var (x, y) = World.get_screen_position(_player.position);
            float rotation = _player.get_rotation();
            _spriteBatch.Draw(
                _character_texture,
                new Rectangle(x, y, 64, 64),
                null,
                Color.White,
                rotation,
                new Vector2(32, 32),
                SpriteEffects.None, 0);
            
            _spriteBatch.DrawString(_arial10, rotation.ToString("F2", CultureInfo.InvariantCulture), new Vector2(x, y), Color.Black);

            EnemiesManager.render(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}