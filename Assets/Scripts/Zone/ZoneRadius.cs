using UnityEngine;

namespace MaterialAccumulation
{
    /// <summary>
    /// Cyclic zone radius: <c>baseRadius + amplitude * curve(phase)</c>, where the phase runs over [0, 1)
    /// <c>frequency</c> times per second.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneRadius : MonoBehaviour
    {
        private const float MinRadius = 0.05f;
        private const float SineSlope = 2f * Mathf.PI;

        [SerializeField, Min(MinRadius)]
        private float _baseRadius = 1.5f;

        [SerializeField, Min(0f)]
        private float _amplitude = 0.5f;

        [SerializeField, Min(0f), Tooltip("Cycles per second.")]
        private float _frequency = 0.5f;

        [SerializeField, Tooltip("Radius offset over one cycle: time in [0, 1], value usually in [-1, 1].")]
        private AnimationCurve _curve = new AnimationCurve(
            new Keyframe(0f, 0f, SineSlope, SineSlope),
            new Keyframe(0.25f, 1f),
            new Keyframe(0.5f, 0f, -SineSlope, -SineSlope),
            new Keyframe(0.75f, -1f),
            new Keyframe(1f, 0f, SineSlope, SineSlope));

        // Phase is accumulated rather than derived from Time.time,
        // so changing the frequency at runtime does not make the radius jump.
        private float _phase;

        public float Radius { get; private set; }

        private void Awake()
        {
            Radius = Evaluate();
        }

        private void Update()
        {
            _phase = Mathf.Repeat(_phase + _frequency * Time.deltaTime, 1f);
            Radius = Evaluate();
        }

        private float Evaluate() => Mathf.Max(MinRadius, _baseRadius + _amplitude * _curve.Evaluate(_phase));
    }
}
