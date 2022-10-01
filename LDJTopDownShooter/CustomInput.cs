using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LDJTopDownShooter;

public enum MouseButton {
    Left,
    Middle,
    Right,
    Extra1,
    Extra2
}

public static class CustomInput {
    private static List<Keys> _down_keys = new();
    private static List<Keys> _pressed_keys = new();
    private static List<Keys> _up_keys = new();

    private static List<MouseButton> _down_mouse_buttons = new();
    private static List<MouseButton> _pressed_mouse_buttons = new();
    private static List<MouseButton> _up_mouse_buttons = new();

    private static int _previous_scroll_wheel_value = 0;
    private static int _current_scroll_wheel_value = 0;
    private static int _instant_scroll_wheel_value = 0;

    private const float SMOOTH_SCROLL_WHEEL_ACC = 0.02f;
    private const float SMOOTH_SCROOL_WHEEL_DECELERATE = 0.95f;
    private const float SMOOTH_SCROLL_WHEEL_VALUE_MAX = 1.0f;
    private const float SMOOTH_SCROLL_WHEEL_VALUE_GRAVITY = 0.85f;
    private static float _smooth_scroll_wheel_value = 0f;
    private static float _smooth_scroll_wheel_velocity = 0f;

    public static Vector2 mouse_vec2 { get; private set; }

    public static void update(KeyboardState keyboard_state, MouseState mouse_state) {

        // keyboard
        var current_pressed_keys = keyboard_state.GetPressedKeys().ToList();

        _up_keys.Clear();
        foreach (var key in _pressed_keys) {
            if (!current_pressed_keys.Contains(key)) {
                _up_keys.Add(key);
            }
        }

        _down_keys.Clear();
        foreach (var key in current_pressed_keys) {
            if (!_pressed_keys.Contains(key)) {
                _down_keys.Add(key);
            }
        }

        _pressed_keys = current_pressed_keys;

        // mouse
        var current_pressed_mouse_buttons = new List<MouseButton>(5);

        if (mouse_state.LeftButton == ButtonState.Pressed) {
            current_pressed_mouse_buttons.Add(MouseButton.Left);
        }

        if (mouse_state.MiddleButton == ButtonState.Pressed) {
            current_pressed_mouse_buttons.Add(MouseButton.Middle);
        }

        if (mouse_state.RightButton == ButtonState.Pressed) {
            current_pressed_mouse_buttons.Add(MouseButton.Right);
        }

        if (mouse_state.XButton1 == ButtonState.Pressed) {
            current_pressed_mouse_buttons.Add(MouseButton.Extra1);
        }

        if (mouse_state.XButton2 == ButtonState.Pressed) {
            current_pressed_mouse_buttons.Add(MouseButton.Extra2);
        }

        _up_mouse_buttons.Clear();
        foreach (var btn in _pressed_mouse_buttons) {
            if (!current_pressed_mouse_buttons.Contains(btn)) {
                _up_mouse_buttons.Add(btn);
            }
        }

        _down_mouse_buttons.Clear();
        foreach (var btn in current_pressed_mouse_buttons) {
            if (!_pressed_mouse_buttons.Contains(btn)) {
                _down_mouse_buttons.Add(btn);
            }
        }

        _pressed_mouse_buttons = current_pressed_mouse_buttons;

        // scroll wheel
        _previous_scroll_wheel_value = _current_scroll_wheel_value;
        _current_scroll_wheel_value = mouse_state.ScrollWheelValue;
        _instant_scroll_wheel_value = Math.Sign(_previous_scroll_wheel_value - _current_scroll_wheel_value);

        if (_instant_scroll_wheel_value == 0) {
            _smooth_scroll_wheel_velocity *= SMOOTH_SCROOL_WHEEL_DECELERATE;

            if (Math.Abs(_smooth_scroll_wheel_velocity) < 1e-4) {
                _smooth_scroll_wheel_velocity = 0f;
            }
        } else {
            _smooth_scroll_wheel_velocity += _instant_scroll_wheel_value * SMOOTH_SCROLL_WHEEL_ACC;
        }

        float _new_smooth_scroll_wheel_value = _smooth_scroll_wheel_value + _smooth_scroll_wheel_velocity;
        if (_new_smooth_scroll_wheel_value > SMOOTH_SCROLL_WHEEL_VALUE_MAX) {
            _new_smooth_scroll_wheel_value = SMOOTH_SCROLL_WHEEL_VALUE_MAX;
        } else if (_new_smooth_scroll_wheel_value < -SMOOTH_SCROLL_WHEEL_VALUE_MAX) {
            _new_smooth_scroll_wheel_value = -SMOOTH_SCROLL_WHEEL_VALUE_MAX;
        }
        _new_smooth_scroll_wheel_value *= SMOOTH_SCROLL_WHEEL_VALUE_GRAVITY;

        if (Math.Abs(_new_smooth_scroll_wheel_value) < 1e-4) {
            _new_smooth_scroll_wheel_value = 0;
        }

        _smooth_scroll_wheel_value = _new_smooth_scroll_wheel_value;

        // mouse position
        mouse_vec2 = mouse_state.Position.ToVector2();
    }

    public static bool is_key_down(Keys key) {
        return _down_keys.Contains(key);
    }

    public static bool is_key_pressed(Keys key) {
        return _pressed_keys.Contains(key);
    }

    public static bool is_key_up(Keys key) {
        return _up_keys.Contains(key);
    }

    public static int get_instant_scroll_wheel_value() {
        return _instant_scroll_wheel_value;
    }

    public static float get_smooth_scrool_wheel_value() {
        return _smooth_scroll_wheel_value;
    }

    public static bool is_mouse_button_down(MouseButton btn) {
        return _down_mouse_buttons.Contains(btn);
    }

    public static bool is_mouse_button_pressed(MouseButton btn) {
        return _pressed_mouse_buttons.Contains(btn);
    }

    public static bool is_mouse_button_up(MouseButton btn) {
        return _up_mouse_buttons.Contains(btn);
    }
}
