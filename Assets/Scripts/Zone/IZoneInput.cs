using UnityEngine;

namespace MaterialAccumulation
{
    /// <summary>Control source for the zone. Lets the zone logic stay independent of the input device.</summary>
    public interface IZoneInput
    {
        /// <summary>Movement direction on the surface plane (x → world X, y → world Z), magnitude ≤ 1.</summary>
        Vector2 Move { get; }

        bool IsDepositing { get; }

        bool IsResetRequested { get; }
    }
}
