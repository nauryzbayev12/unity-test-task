using UnityEngine;

namespace MaterialAccumulation
{
    /// <summary>
    /// Scales a unit-diameter sphere to the current zone radius. The sphere is centered on the surface plane,
    /// so the surface hides its lower half and only the hemisphere stays visible.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneVisual : MonoBehaviour
    {
        [SerializeField] private ZoneRadius _radius;

        private void LateUpdate()
        {
            transform.localScale = Vector3.one * (2f * _radius.Radius);
        }
    }
}
