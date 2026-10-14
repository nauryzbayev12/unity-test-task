using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaterialAccumulation.EditorTools
{
    /// <summary>Builds the demo setup in the open scene, so it does not have to be wired by hand.</summary>
    internal static class DemoSceneBuilder
    {
        private const string MaterialsFolder = "Assets/Materials";

        [MenuItem("Tools/Material Accumulation/Create Demo Setup")]
        private static void CreateDemoSetup()
        {
            if (Object.FindAnyObjectByType<HeightFieldSurface>() != null)
            {
                Debug.LogWarning("Demo setup already exists in the open scene.");
                return;
            }

            HeightFieldSurface surface = CreateSurface();
            CreateZone(surface);
            SetUpCamera();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static HeightFieldSurface CreateSurface()
        {
            var surfaceObject = new GameObject("Surface");
            Undo.RegisterCreatedObjectUndo(surfaceObject, "Create Demo Setup");

            var surface = surfaceObject.AddComponent<HeightFieldSurface>();
            var view = surfaceObject.AddComponent<HeightFieldMeshView>();
            SetReference(view, "_surface", surface);
            surfaceObject.GetComponent<MeshRenderer>().sharedMaterial =
                LoadOrCreateMaterial("Surface", new Color(0.85f, 0.72f, 0.5f), transparent: false);
            return surface;
        }

        private static void CreateZone(HeightFieldSurface surface)
        {
            var zoneObject = new GameObject("Zone");
            Undo.RegisterCreatedObjectUndo(zoneObject, "Create Demo Setup");

            zoneObject.AddComponent<KeyboardZoneInput>();
            SetReference(zoneObject.AddComponent<ZoneMover>(), "_surface", surface);
            var radius = zoneObject.AddComponent<ZoneRadius>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(zoneObject.transform, worldPositionStays: false);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<MeshRenderer>().sharedMaterial =
                LoadOrCreateMaterial("Zone", new Color(0.3f, 0.7f, 1f, 0.25f), transparent: true);
            SetReference(visual.AddComponent<ZoneVisual>(), "_radius", radius);
        }

        private static void SetUpCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            Undo.RecordObject(camera.transform, "Create Demo Setup");
            camera.transform.SetPositionAndRotation(new Vector3(0f, 15f, -15f), Quaternion.Euler(45f, 0f, 0f));
        }

        private static void SetReference(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material LoadOrCreateMaterial(string name, Color color, bool transparent)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets", "Materials");

            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            material = new Material(pipeline != null ? pipeline.defaultShader : Shader.Find("Standard"))
            {
                color = color
            };
            if (transparent)
                MakeTransparent(material);

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // Mirrors what the URP Lit inspector sets for Surface Type = Transparent, Blend = Alpha.
        private static void MakeTransparent(Material material)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
