using System;
using Microsoft.Xna.Framework;

namespace LDJTopDownShooter;

public class Player
{
    public const float MovementSpeed = 2.0f;
    public const float RotationSpeed = 5.0f;

    public Vector2 Position { get; set; }

    public Vector2 Facing { get; set; } = Vector2.UnitX;

    public float GetRotation() => MathF.Atan2(Facing.Y, Facing.X);
}
