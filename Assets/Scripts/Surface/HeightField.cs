using System;
using UnityEngine;

namespace MaterialAccumulation
{
    /// <summary>
    /// Accumulated material state: a regular grid of heights, independent of any mesh.
    /// Heights can only grow (except for <see cref="Clear"/>), so deposited material is never removed.
    /// </summary>
    public sealed class HeightField
    {
        private readonly float[] _heights;

        // Grid region changed since the last ConsumeDirtyRegion call (max is exclusive).
        private RectInt _dirtyRegion;
        private bool _isDirty;

        public HeightField(int resolution, float size)
        {
            if (resolution < 2)
                throw new ArgumentOutOfRangeException(nameof(resolution), "At least 2 samples per side are required.");

            Resolution = resolution;
            Size = size;
            CellSize = size / (resolution - 1);
            _heights = new float[resolution * resolution];
        }

        /// <summary>Number of samples along each side.</summary>
        public int Resolution { get; }

        /// <summary>Length of each side in local units. The grid is centered at the local origin.</summary>
        public float Size { get; }

        /// <summary>Distance between neighbouring samples.</summary>
        public float CellSize { get; }

        public float GetHeight(int x, int z) => _heights[ToIndex(x, z)];

        /// <summary>Raises the sample to <paramref name="height"/>. Lower values are ignored.</summary>
        public void RaiseTo(int x, int z, float height)
        {
            int index = ToIndex(x, z);
            if (height <= _heights[index])
                return;

            _heights[index] = height;
            MarkDirty(x, z);
        }

        public void Clear()
        {
            Array.Clear(_heights, 0, _heights.Length);
            _dirtyRegion = new RectInt(0, 0, Resolution, Resolution);
            _isDirty = true;
        }

        /// <summary>Returns the region changed since the previous call, if any, and resets it.</summary>
        public bool TryConsumeDirtyRegion(out RectInt region)
        {
            region = _dirtyRegion;
            if (!_isDirty)
                return false;

            _isDirty = false;
            return true;
        }

        public Vector2 GridToLocal(int x, int z)
        {
            float half = Size * 0.5f;
            return new Vector2(x * CellSize - half, z * CellSize - half);
        }

        /// <summary>Converts a local XZ position to fractional grid coordinates.</summary>
        public Vector2 LocalToGrid(Vector2 local)
        {
            float half = Size * 0.5f;
            return new Vector2((local.x + half) / CellSize, (local.y + half) / CellSize);
        }

        private int ToIndex(int x, int z) => z * Resolution + x;

        private void MarkDirty(int x, int z)
        {
            var cell = new Vector2Int(x, z);
            if (!_isDirty)
            {
                _dirtyRegion = new RectInt(cell, Vector2Int.one);
                _isDirty = true;
                return;
            }

            _dirtyRegion.SetMinMax(
                Vector2Int.Min(_dirtyRegion.min, cell),
                Vector2Int.Max(_dirtyRegion.max, cell + Vector2Int.one));
        }
    }
}
