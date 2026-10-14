using UnityEngine;

namespace MaterialAccumulation
{
    /// <summary>
    /// Scene entry point of the surface: owns the <see cref="HeightField"/> and maps world space onto it.
    /// The surface is expected to be unscaled and lie in the local XZ plane.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HeightFieldSurface : MonoBehaviour
    {
        [SerializeField, Min(2), Tooltip("Samples per side. Above 256 the mesh switches to 32-bit indices.")]
        private int _resolution = 201;

        [SerializeField, Min(0.1f), Tooltip("Side length of the square surface.")]
        private float _size = 20f;

        public HeightField Field { get; private set; }

        private void Awake()
        {
            Field = new HeightField(_resolution, _size);
        }

        public Vector2 WorldToLocal(Vector3 worldPosition)
        {
            Vector3 local = transform.InverseTransformPoint(worldPosition);
            return new Vector2(local.x, local.z);
        }

        /// <summary>Keeps a world position inside the surface bounds, preserving its height.</summary>
        public Vector3 ClampToSurface(Vector3 worldPosition)
        {
            Vector3 local = transform.InverseTransformPoint(worldPosition);
            float half = _size * 0.5f;
            local.x = Mathf.Clamp(local.x, -half, half);
            local.z = Mathf.Clamp(local.z, -half, half);
            return transform.TransformPoint(local);
        }

        [ContextMenu("Reset Surface")]
        public void ResetSurface()
        {
            Field?.Clear();
        }
    }
}
