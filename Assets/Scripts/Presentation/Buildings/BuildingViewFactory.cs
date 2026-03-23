using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Factory for creating procedural building primitives.
    /// Extracted from BuildingView to keep under 300 lines.
    /// </summary>
    public static class BuildingViewFactory
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        public static MeshRenderer CreateShape(Transform parent, BaseBuildingType type,
            float width, float height, Material material, Color roofColor)
        {
            return type switch
            {
                BaseBuildingType.Lodge => CreateLodge(parent, width, height, material, roofColor),
                BaseBuildingType.Farm => CreateFarm(parent, width, height, material, roofColor),
                BaseBuildingType.MountainShelter => CreateMountainShelter(parent, width, height, material, roofColor),
                BaseBuildingType.Residence => CreateResidence(parent, width, height, material, roofColor),
                BaseBuildingType.NobleResidence => CreateNobleResidence(parent, width, height, material, roofColor),
                _ => CreateDefault(parent, width, height, material)
            };
        }

        private static MeshRenderer CreateLodge(Transform parent, float w, float h,
            Material mat, Color roofColor)
        {
            var baseObj = CreatePrim(parent, "Base", PrimitiveType.Cube,
                new Vector3(w, h * 0.6f, w * 0.8f),
                new Vector3(0f, h * 0.3f, 0f), mat);
            var roof = CreatePrim(parent, "Roof", PrimitiveType.Cube,
                new Vector3(w * 1.15f, h * 0.35f, w * 0.6f),
                new Vector3(0f, h * 0.78f, 0f), mat);
            SetColor(roof, roofColor);
            return baseObj.GetComponent<MeshRenderer>();
        }

        private static MeshRenderer CreateFarm(Transform parent, float w, float h,
            Material mat, Color roofColor)
        {
            var baseObj = CreatePrim(parent, "Base", PrimitiveType.Cube,
                new Vector3(w * 1.2f, h * 0.5f, w * 0.9f),
                new Vector3(0f, h * 0.25f, 0f), mat);
            var roof = CreatePrim(parent, "Roof", PrimitiveType.Sphere,
                new Vector3(w * 1.3f, h * 0.5f, w),
                new Vector3(0f, h * 0.7f, 0f), mat);
            SetColor(roof, roofColor);
            var silo = CreatePrim(parent, "Silo", PrimitiveType.Cylinder,
                new Vector3(w * 0.25f, h * 0.7f, w * 0.25f),
                new Vector3(w * 0.5f, h * 0.35f, 0f), mat);
            SetColor(silo, new Color(0.6f, 0.55f, 0.45f));
            return baseObj.GetComponent<MeshRenderer>();
        }

        private static MeshRenderer CreateMountainShelter(Transform parent, float w, float h,
            Material mat, Color roofColor)
        {
            var baseObj = CreatePrim(parent, "Base", PrimitiveType.Cube,
                new Vector3(w * 0.8f, h * 0.7f, w * 0.8f),
                new Vector3(0f, h * 0.35f, 0f), mat);
            var roof = CreatePrim(parent, "Roof", PrimitiveType.Sphere,
                new Vector3(w * 0.9f, h * 0.5f, w * 0.9f),
                new Vector3(0f, h * 0.85f, 0f), mat);
            SetColor(roof, roofColor);
            var chimney = CreatePrim(parent, "Chimney", PrimitiveType.Cube,
                new Vector3(w * 0.15f, h * 0.3f, w * 0.15f),
                new Vector3(w * 0.25f, h * 1.0f, w * 0.2f), mat);
            SetColor(chimney, new Color(0.35f, 0.35f, 0.4f));
            return baseObj.GetComponent<MeshRenderer>();
        }

        private static MeshRenderer CreateResidence(Transform parent, float w, float h,
            Material mat, Color roofColor)
        {
            var baseObj = CreatePrim(parent, "Ground", PrimitiveType.Cube,
                new Vector3(w, h * 0.4f, w * 0.9f),
                new Vector3(0f, h * 0.2f, 0f), mat);
            CreatePrim(parent, "Upper", PrimitiveType.Cube,
                new Vector3(w * 0.9f, h * 0.3f, w * 0.8f),
                new Vector3(0f, h * 0.55f, 0f), mat);
            var roof = CreatePrim(parent, "Roof", PrimitiveType.Sphere,
                new Vector3(w * 1.1f, h * 0.35f, w),
                new Vector3(0f, h * 0.85f, 0f), mat);
            SetColor(roof, roofColor);
            return baseObj.GetComponent<MeshRenderer>();
        }

        private static MeshRenderer CreateNobleResidence(Transform parent, float w, float h,
            Material mat, Color roofColor)
        {
            var baseObj = CreatePrim(parent, "Main", PrimitiveType.Cube,
                new Vector3(w, h * 0.55f, w * 0.9f),
                new Vector3(0f, h * 0.275f, 0f), mat);
            CreatePrim(parent, "Tower", PrimitiveType.Cylinder,
                new Vector3(w * 0.3f, h * 0.8f, w * 0.3f),
                new Vector3(-w * 0.35f, h * 0.4f, 0f), mat);
            var cap = CreatePrim(parent, "TowerCap", PrimitiveType.Sphere,
                new Vector3(w * 0.35f, h * 0.2f, w * 0.35f),
                new Vector3(-w * 0.35f, h * 0.85f, 0f), mat);
            SetColor(cap, roofColor);
            var roof = CreatePrim(parent, "Roof", PrimitiveType.Sphere,
                new Vector3(w * 1.15f, h * 0.3f, w),
                new Vector3(0f, h * 0.7f, 0f), mat);
            SetColor(roof, roofColor);
            var balcony = CreatePrim(parent, "Balcony", PrimitiveType.Cube,
                new Vector3(w * 0.3f, h * 0.05f, w * 0.15f),
                new Vector3(0f, h * 0.4f, w * 0.5f), mat);
            SetColor(balcony, new Color(0.7f, 0.6f, 0.4f));
            return baseObj.GetComponent<MeshRenderer>();
        }

        private static MeshRenderer CreateDefault(Transform parent, float w, float h,
            Material mat)
        {
            var baseObj = CreatePrim(parent, "Base", PrimitiveType.Cube,
                new Vector3(w, h, w), new Vector3(0f, h * 0.5f, 0f), mat);
            return baseObj.GetComponent<MeshRenderer>();
        }

        public static GameObject CreatePrim(Transform parent, string name,
            PrimitiveType type, Vector3 scale, Vector3 localPos, Material mat)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = scale;
            obj.transform.localPosition = localPos;
            if (mat != null)
                obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.Destroy(obj.GetComponent<Collider>());
            return obj;
        }

        public static void SetColor(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            var block = new MaterialPropertyBlock();
            block.SetColor(ColorProperty, color);
            renderer.SetPropertyBlock(block);
        }

        public static float GetHeight(BaseBuildingType type) => type switch
        {
            BaseBuildingType.Lodge => 1.5f,
            BaseBuildingType.Farm => 1.2f,
            BaseBuildingType.MountainShelter => 1.8f,
            BaseBuildingType.Residence => 2.5f,
            BaseBuildingType.NobleResidence => 3.0f,
            _ => 1.5f
        };

        public static float GetWidth(BaseBuildingType type) => type switch
        {
            BaseBuildingType.Lodge => 1.2f,
            BaseBuildingType.Farm => 1.5f,
            BaseBuildingType.MountainShelter => 1.0f,
            BaseBuildingType.Residence => 1.4f,
            BaseBuildingType.NobleResidence => 1.6f,
            _ => 1.2f
        };

        public static Color GetRoofColor(BaseBuildingType type) => type switch
        {
            BaseBuildingType.Lodge => new Color(0.35f, 0.2f, 0.1f),
            BaseBuildingType.Farm => new Color(0.6f, 0.2f, 0.15f),
            BaseBuildingType.MountainShelter => new Color(0.35f, 0.35f, 0.4f),
            BaseBuildingType.Residence => new Color(0.5f, 0.25f, 0.15f),
            BaseBuildingType.NobleResidence => new Color(0.7f, 0.6f, 0.2f),
            _ => new Color(0.4f, 0.3f, 0.2f)
        };

        public static Color GetBuildingColor(BaseBuildingType type) => type switch
        {
            BaseBuildingType.Lodge => new Color(0.55f, 0.35f, 0.15f),
            BaseBuildingType.Farm => new Color(0.45f, 0.65f, 0.25f),
            BaseBuildingType.MountainShelter => new Color(0.5f, 0.5f, 0.55f),
            BaseBuildingType.Residence => new Color(0.75f, 0.6f, 0.4f),
            BaseBuildingType.NobleResidence => new Color(0.85f, 0.75f, 0.5f),
            _ => Color.white
        };
    }
}
