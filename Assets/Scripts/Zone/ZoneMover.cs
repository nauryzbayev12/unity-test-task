using UnityEngine;

namespace MaterialAccumulation
{
    /// <summary>Moves the zone over the surface plane and keeps it inside the surface bounds.</summary>
    [DisallowMultipleComponent]
    public sealed class ZoneMover : MonoBehaviour
    {
        [SerializeField] private HeightFieldSurface _surface;

        [SerializeField, Min(0f), Tooltip("Units per second.")]
        private float _speed = 5f;

        private IZoneInput _input;

        private void Awake()
        {
            _input = GetComponent<IZoneInput>();
        }

        private void Update()
        {
            Vector2 move = _input.Move;
            if (move == Vector2.zero)
                return;

            Vector3 delta = new Vector3(move.x, 0f, move.y) * (_speed * Time.deltaTime);
            transform.position = _surface.ClampToSurface(transform.position + delta);
        }
    }
}
