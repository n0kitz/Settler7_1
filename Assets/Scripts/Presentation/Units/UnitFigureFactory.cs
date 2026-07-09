using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Builds small procedural humanoid figures for units — workers, carriers,
    /// soldiers, clerics and generals. Companion to BuildingViewFactory: same
    /// primitive + MaterialPropertyBlock idiom, one shared material.
    /// Figures stand on their local origin (feet at y=0) and face local +Z.
    /// </summary>
    public static class UnitFigureFactory
    {
        /// <summary>Which unit silhouette to assemble.</summary>
        public enum Role { Worker, Carrier, Soldier, Cleric, General }

        /// <summary>Total figure height in world units (feet to headgear).</summary>
        public const float FIGURE_HEIGHT = 0.6f;

        private static readonly Color SKIN = new(0.87f, 0.68f, 0.50f);
        private static readonly Color TROUSER = new(0.30f, 0.22f, 0.14f);
        private static readonly Color IRON = new(0.55f, 0.57f, 0.62f);
        private static readonly Color STRAW = new(0.85f, 0.72f, 0.38f);
        private static readonly Color PLUME_RED = new(0.75f, 0.20f, 0.18f);
        private static readonly Color GOLD = new(0.87f, 0.71f, 0.22f);

        /// <summary>
        /// Assemble a figure under <paramref name="parent"/>. The tunic (torso and
        /// arms — the whole robe for clerics) is tinted <paramref name="tunic"/>.
        /// </summary>
        public static GameObject CreateFigure(Transform parent, Role role,
            Color tunic, Material mat)
        {
            var root = new GameObject("Figure");
            root.transform.SetParent(parent, false);

            if (role == Role.Cleric)
            {
                var robe = BuildingViewFactory.CreatePrim(root.transform, "Robe",
                    PrimitiveType.Cylinder, new Vector3(0.26f, 0.20f, 0.26f),
                    new Vector3(0f, 0.20f, 0f), mat);
                BuildingViewFactory.SetColor(robe, tunic);
            }
            else
            {
                AddLeg(root.transform, -0.055f, mat);
                AddLeg(root.transform, 0.055f, mat);
            }

            var torso = BuildingViewFactory.CreatePrim(root.transform, "Torso",
                PrimitiveType.Cube, new Vector3(0.22f, 0.24f, 0.14f),
                new Vector3(0f, 0.30f, 0f), mat);
            BuildingViewFactory.SetColor(torso, tunic);

            AddArm(root.transform, -0.14f, tunic, mat);
            AddArm(root.transform, 0.14f, tunic, mat);

            var head = BuildingViewFactory.CreatePrim(root.transform, "Head",
                PrimitiveType.Sphere, new Vector3(0.16f, 0.16f, 0.16f),
                new Vector3(0f, 0.50f, 0f), mat);
            BuildingViewFactory.SetColor(head, SKIN);

            AddRoleGear(root.transform, role, tunic, mat);

            // Anchor for carried goods, at hand height in front of the torso.
            var hands = new GameObject("Hands");
            hands.transform.SetParent(root.transform, false);
            hands.transform.localPosition = new Vector3(0f, 0.30f, 0.16f);

            return root;
        }

        /// <summary>Torso renderer, for runtime tunic tinting (idle/active).</summary>
        public static MeshRenderer GetTorsoRenderer(GameObject figure)
        {
            var torso = figure.transform.Find("Torso");
            return torso != null ? torso.GetComponent<MeshRenderer>() : null;
        }

        /// <summary>Anchor transform for goods carried in front of the figure.</summary>
        public static Transform GetHandsAnchor(GameObject figure)
        {
            return figure.transform.Find("Hands");
        }

        private static void AddLeg(Transform root, float x, Material mat)
        {
            var leg = BuildingViewFactory.CreatePrim(root, "Leg",
                PrimitiveType.Cube, new Vector3(0.07f, 0.18f, 0.07f),
                new Vector3(x, 0.09f, 0f), mat);
            BuildingViewFactory.SetColor(leg, TROUSER);
        }

        private static void AddArm(Transform root, float x, Color tunic, Material mat)
        {
            var arm = BuildingViewFactory.CreatePrim(root, "Arm",
                PrimitiveType.Cube, new Vector3(0.06f, 0.20f, 0.06f),
                new Vector3(x, 0.30f, 0f), mat);
            BuildingViewFactory.SetColor(arm, tunic);
        }

        private static void AddRoleGear(Transform root, Role role, Color tunic, Material mat)
        {
            switch (role)
            {
                case Role.Worker:
                    var brim = BuildingViewFactory.CreatePrim(root, "HatBrim",
                        PrimitiveType.Cylinder, new Vector3(0.24f, 0.012f, 0.24f),
                        new Vector3(0f, 0.565f, 0f), mat);
                    BuildingViewFactory.SetColor(brim, STRAW);
                    var crown = BuildingViewFactory.CreatePrim(root, "HatCrown",
                        PrimitiveType.Cylinder, new Vector3(0.13f, 0.03f, 0.13f),
                        new Vector3(0f, 0.60f, 0f), mat);
                    BuildingViewFactory.SetColor(crown, STRAW);
                    break;

                case Role.Carrier:
                    var cap = BuildingViewFactory.CreatePrim(root, "Cap",
                        PrimitiveType.Sphere, new Vector3(0.16f, 0.09f, 0.16f),
                        new Vector3(0f, 0.565f, 0f), mat);
                    BuildingViewFactory.SetColor(cap, TROUSER);
                    break;

                case Role.Soldier:
                case Role.General:
                    var helm = BuildingViewFactory.CreatePrim(root, "Helmet",
                        PrimitiveType.Sphere, new Vector3(0.18f, 0.13f, 0.18f),
                        new Vector3(0f, 0.555f, 0f), mat);
                    BuildingViewFactory.SetColor(helm, IRON);
                    if (role == Role.Soldier)
                        AddSpear(root, mat);
                    else
                        AddGeneralTrim(root, mat);
                    break;

                case Role.Cleric:
                    var hood = BuildingViewFactory.CreatePrim(root, "Hood",
                        PrimitiveType.Sphere, new Vector3(0.19f, 0.16f, 0.19f),
                        new Vector3(0f, 0.53f, -0.02f), mat);
                    BuildingViewFactory.SetColor(hood, tunic);
                    break;
            }
        }

        private static void AddSpear(Transform root, Material mat)
        {
            var shaft = BuildingViewFactory.CreatePrim(root, "Spear",
                PrimitiveType.Cylinder, new Vector3(0.025f, 0.36f, 0.025f),
                new Vector3(0.17f, 0.36f, 0f), mat);
            BuildingViewFactory.SetColor(shaft, TROUSER);
            var tip = BuildingViewFactory.CreatePrim(root, "SpearTip",
                PrimitiveType.Cube, new Vector3(0.04f, 0.08f, 0.04f),
                new Vector3(0.17f, 0.76f, 0f), mat);
            BuildingViewFactory.SetColor(tip, IRON);
        }

        private static void AddGeneralTrim(Transform root, Material mat)
        {
            var band = BuildingViewFactory.CreatePrim(root, "HelmetBand",
                PrimitiveType.Cylinder, new Vector3(0.185f, 0.012f, 0.185f),
                new Vector3(0f, 0.53f, 0f), mat);
            BuildingViewFactory.SetColor(band, GOLD);
            var plume = BuildingViewFactory.CreatePrim(root, "Plume",
                PrimitiveType.Cube, new Vector3(0.045f, 0.14f, 0.045f),
                new Vector3(0f, 0.68f, 0f), mat);
            BuildingViewFactory.SetColor(plume, PLUME_RED);
        }
    }
}
