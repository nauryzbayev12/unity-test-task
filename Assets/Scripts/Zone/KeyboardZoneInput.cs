using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MaterialAccumulation
{
    /// <summary>WASD moves the zone, Space deposits material, R resets the surface.</summary>
    [DisallowMultipleComponent]
    public sealed class KeyboardZoneInput : MonoBehaviour, IZoneInput
    {
        public Vector2 Move
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                    return Vector2.zero;

                var move = new Vector2(
                    Axis(keyboard.dKey, keyboard.aKey),
                    Axis(keyboard.wKey, keyboard.sKey));
                return Vector2.ClampMagnitude(move, 1f);
            }
        }

        public bool IsDepositing => Keyboard.current?.spaceKey.isPressed ?? false;

        public bool IsResetRequested => Keyboard.current?.rKey.wasPressedThisFrame ?? false;

        private static float Axis(ButtonControl positive, ButtonControl negative) =>
            (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
    }
}
