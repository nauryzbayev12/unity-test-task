using UnityEngine;
using UnityEngine.Rendering;

namespace MaterialAccumulation
{
    /// <summary>
    /// Displays a <see cref="HeightField"/> as a grid mesh. The topology is built once;
    /// afterwards only heights and normals of the changed region are rewritten in preallocated buffers.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class HeightFieldMeshView : MonoBehaviour
    {
        private const MeshUpdateFlags UpdateFlags =
            MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds;

        [SerializeField] private HeightFieldSurface _surface;

        private HeightField _field;
        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private float _maxHeight;

        private void Reset()
        {
            _surface = GetComponent<HeightFieldSurface>();
        }

        private void Start()
        {
            _field = _surface.Field;
            _mesh = BuildMesh();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        private void LateUpdate()
        {
            if (_field.TryConsumeDirtyRegion(out RectInt region))
                UpdateRegion(region);
        }

        private void OnDestroy()
        {
            if (_mesh != null)
                Destroy(_mesh);
        }

        private Mesh BuildMesh()
        {
            int resolution = _field.Resolution;
            int vertexCount = resolution * resolution;

            _vertices = new Vector3[vertexCount];
            _normals = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            var triangles = new int[(resolution - 1) * (resolution - 1) * 6];

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = z * resolution + x;
                    Vector2 local = _field.GridToLocal(x, z);
                    _vertices[i] = new Vector3(local.x, _field.GetHeight(x, z), local.y);
                    _normals[i] = Vector3.up;
                    uvs[i] = new Vector2(x, z) / (resolution - 1);
                }
            }

            int t = 0;
            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int i = z * resolution + x;
                    triangles[t++] = i;
                    triangles[t++] = i + resolution;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + resolution;
                    triangles[t++] = i + resolution + 1;
                }
            }

            var mesh = new Mesh
            {
                name = "HeightField",
                indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.MarkDynamic();
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private void UpdateRegion(RectInt region)
        {
            int resolution = _field.Resolution;

            for (int z = region.yMin; z < region.yMax; z++)
            {
                for (int x = region.xMin; x < region.xMax; x++)
                {
                    float height = _field.GetHeight(x, z);
                    _vertices[z * resolution + x].y = height;
                    _maxHeight = Mathf.Max(_maxHeight, height);
                }
            }

            // Normals depend on neighbouring heights, so the affected area is one sample wider.
            int xMin = Mathf.Max(region.xMin - 1, 0);
            int xMax = Mathf.Min(region.xMax + 1, resolution);
            int zMin = Mathf.Max(region.yMin - 1, 0);
            int zMax = Mathf.Min(region.yMax + 1, resolution);

            for (int z = zMin; z < zMax; z++)
            {
                for (int x = xMin; x < xMax; x++)
                    _normals[z * resolution + x] = CalculateNormal(x, z);
            }

            // Rows are contiguous in the buffers, so only the affected row range is uploaded.
            int start = zMin * resolution;
            int count = (zMax - zMin) * resolution;
            _mesh.SetVertices(_vertices, start, count, UpdateFlags);
            _mesh.SetNormals(_normals, start, count, UpdateFlags);
            _mesh.bounds = new Bounds(
                new Vector3(0f, _maxHeight * 0.5f, 0f),
                new Vector3(_field.Size, _maxHeight, _field.Size));
        }

        // Central differences of the height map: n = (h(x-1) - h(x+1), 2 * cell, h(z-1) - h(z+1)).
        private Vector3 CalculateNormal(int x, int z)
        {
            int last = _field.Resolution - 1;
            float left = _field.GetHeight(Mathf.Max(x - 1, 0), z);
            float right = _field.GetHeight(Mathf.Min(x + 1, last), z);
            float back = _field.GetHeight(x, Mathf.Max(z - 1, 0));
            float forward = _field.GetHeight(x, Mathf.Min(z + 1, last));
            return new Vector3(left - right, 2f * _field.CellSize, back - forward).normalized;
        }
    }
}
