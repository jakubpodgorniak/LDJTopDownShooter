using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LDJTopDownShooter {
    public class Game : Microsoft.Xna.Framework.Game
    {
        public static float DeltaTime { get; private set; }
        public static int PixelsPerUnit = 128;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont _arial10;
        private Texture2D _character_texture;
        private Player _player;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _player = new Player();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _arial10 = Content.Load<SpriteFont>("fonts/Arial10");
            _character_texture = Content.Load<Texture2D>("player");
        }

        protected override void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                _player.Position += (move * DeltaTime * Player.MovementSpeed);
            }

            // movement

            //rotation
            if (keyboard.IsKeyDown(Keys.Q)) {
                float rotation_angle = Player.RotationSpeed * DeltaTime;

                float x = _player.Facing.X * MathF.Cos(rotation_angle)
                    - _player.Facing.Y * MathF.Sin(rotation_angle);
                float y = _player.Facing.X * MathF.Sin(rotation_angle)
                    + _player.Facing.Y * MathF.Cos(rotation_angle);

                _player.Facing = new Vector2(x, y);
            } else if (keyboard.IsKeyDown(Keys.E)) {
                float rotation_angle = (-1f) * Player.RotationSpeed * DeltaTime;

                float x = _player.Facing.X * MathF.Cos(rotation_angle)
                    - _player.Facing.Y * MathF.Sin(rotation_angle);
                float y = _player.Facing.X * MathF.Sin(rotation_angle)
                    + _player.Facing.Y * MathF.Cos(rotation_angle);

                _player.Facing = new Vector2(x, y);
            }
            //rotation

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend);

            var (x, y) = GetScreenPosition(_player.Position);
            float rotation = _player.GetRotation();
            _spriteBatch.Draw(_character_texture, new Rectangle(x, y, 64, 64), null, Color.White, rotation, new Vector2(32, 32), SpriteEffects.None, 0);
            
            _spriteBatch.DrawString(_arial10, rotation.ToString("F2", CultureInfo.InvariantCulture), new Vector2(x, y), Color.Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private (int x, int y) GetScreenPosition(Vector2 position)
        {
            int x = (int)Math.Floor(position.X * PixelsPerUnit);
            int y = (int)Math.Floor(position.Y * PixelsPerUnit);

            return (x, y);
        }
    }
}