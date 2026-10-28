#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates Derek's two layout scenes by copying the supplied Fernando scenes.
/// It uses only Project 6 assets: missing design items are recorded by name and
/// are never replaced with generated stand-ins.
/// </summary>
public static class DerekSceneLayoutBuilder
{
    private const string ApartmentSource = "Assets/Scenes/Apartment Gbox.unity";
    private const string ParadiseSource = "Assets/Scenes/Beach Scene.unity";
    private const string ApartmentTarget = "Assets/Scenes/Derek Scene Apartment.unity";
    private const string ParadiseTarget = "Assets/Scenes/Derek Scene Paradise.unity";

    [MenuItem("Tools/Project 6/Build Derek Apartment and Paradise Layouts")]
    public static void BuildBoth()
    {
        BuildApartment();
        BuildParadise();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ApartmentTarget, OpenSceneMode.Single);
        Debug.Log("Derek layouts created from Fernando's original grayboxes. No substitute assets were generated.");
    }

    [MenuItem("Tools/Project 6/Report Apartment Prop Dimensions")]
    public static void ReportApartmentPropDimensions()
    {
        string[] names =
        {
            "Laptop — digital balance visual bridge ($0; headset asset unavailable)",
            "Used Cup — desk dressing", "Curtains — dirty window dressing", "Mini Fridge",
            "Lamp — weak return lighting prop", "Dirty Clothes — green shirt",
            "Dirty Clothes — black shirt", "Dirty Clothes — blue shirt", "Trash Cluster A",
            "Trash Cluster A — second bag", "Trash Cluster B"
        };
        foreach (string name in names)
        {
            Transform item = Find(name);
            if (item == null) { Debug.LogWarning("Apartment prop missing: " + name); continue; }
            Bounds bounds = RendererBounds(item.gameObject);
            Debug.Log("APARTMENT PROP | " + name + " | bounds=" + bounds.size + " | scale=" + item.localScale);
        }
    }

    [MenuItem("Tools/Project 6/Fit Apartment Props to Realistic Scale")]
    public static void FitApartmentPropsToRealisticScale()
    {
        Transform floor = Find("Floor");
        float floorTop = floor != null ? RendererBounds(floor.gameObject).max.y : 0f;

        // Keep the furniture already at human scale; correct only the oversized imported props.
        FitAndGround("Lamp — weak return lighting prop", 1.65f, floorTop);
        FitAndGround("Trash Cluster A", 0.75f, floorTop);
        FitAndGround("Trash Cluster A — second bag", 0.62f, floorTop);
        FitAndGround("Trash Cluster B", 0.70f, floorTop);
        FitAndGround("Dirty Clothes — green shirt", 0.60f, floorTop);
        FitAndGround("Dirty Clothes — black shirt", 0.60f, floorTop);
        FitAndGround("Dirty Clothes — blue shirt", 0.60f, floorTop);

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Apartment oversized props were resized to human-scale dimensions and grounded on the Fernando floor.");
    }

    [MenuItem("Tools/Project 6/Align Apartment Clothes and Fridge to Floor")]
    public static void AlignApartmentClothesAndFridgeToFloor()
    {
        Transform floor = Find("Floor");
        if (floor == null)
        {
            Debug.LogWarning("Apartment floor not found; no prop alignment was applied.");
            return;
        }
        float floorTop = RendererBounds(floor.gameObject).max.y;

        LayFlatAndGround("Dirty Clothes — green shirt", 12f, floorTop);
        LayFlatAndGround("Dirty Clothes — black shirt", -18f, floorTop);
        LayFlatAndGround("Dirty Clothes — blue shirt", 28f, floorTop);
        GroundOnly("Mini Fridge", floorTop);

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Apartment shirts lie flat on the graybox floor and the mini fridge base is floor-aligned.");
    }

    [MenuItem("Tools/Project 6/Polish Apartment Layout")]
    public static void PolishApartmentLayout()
    {
        Transform floor = Find("Floor");
        Transform clothesAnchor = Find("Dirty clothes");
        Transform trashAnchor = Find("trash cluster");
        Transform trashAnchorTwo = Find("trash cluster 2");
        if (floor == null) { Debug.LogWarning("Apartment floor not found; no polish was applied."); return; }
        float floorTop = RendererBounds(floor.gameObject).max.y;

        // Preserve the source graybox markers; make the imported shirts a loose, readable floor pile.
        if (clothesAnchor != null)
        {
            SetFloorPosition("Dirty Clothes — green shirt", clothesAnchor.position + new Vector3(-0.28f, 0f, 0.08f), 8f, floorTop);
            SetFloorPosition("Dirty Clothes — black shirt", clothesAnchor.position + new Vector3(0.10f, 0f, -0.20f), -22f, floorTop);
            SetFloorPosition("Dirty Clothes — blue shirt", clothesAnchor.position + new Vector3(0.30f, 0f, 0.18f), 32f, floorTop);
        }
        if (trashAnchor != null)
        {
            SetGroundPosition("Trash Cluster A", trashAnchor.position + new Vector3(-0.18f, 0f, 0.05f), floorTop);
            SetGroundPosition("Trash Cluster A — second bag", trashAnchor.position + new Vector3(0.24f, 0f, -0.16f), floorTop);
        }
        if (trashAnchorTwo != null)
            SetGroundPosition("Trash Cluster B", trashAnchorTwo.position + new Vector3(0.08f, 0f, 0.10f), floorTop);

        Transform lamp = Find("Lamp — weak return lighting prop");
        if (lamp != null)
        {
            GroundOnly(lamp, floorTop);
            Light lampLight = lamp.GetComponent<Light>();
            if (lampLight == null) lampLight = lamp.gameObject.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.color = new Color(1f, 0.62f, 0.33f);
            lampLight.intensity = 1.3f;
            lampLight.range = 3.2f;
            lampLight.shadows = LightShadows.Soft;
        }

        RenderSettings.ambientIntensity = 0.55f;
        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Apartment layout polished: grounded prop clusters, loose clothes pile, and weak warm lamp light.");
    }

    private static void BuildApartment()
    {
        Scene scene = CopyAndOpen(ApartmentSource, ApartmentTarget);
        Transform layout = PrepareLayoutRoot("DEREK LAYOUT — APARTMENT / FORCED RETURN", scene);

        Transform desk = Find("desk");
        Transform window = Find("Window");
        Transform fridge = Find("Minifridge");
        Transform lamp = Find("lamp") ?? Find("Lamp Stand");
        Transform clothes = Find("Dirty clothes");
        Transform trashA = Find("trash cluster");
        Transform trashB = Find("trash cluster 2");
        Transform bed = Find("Bed");

        // Existing graybox landmarks remain at their original positions.
        Note(layout, "Source: Fernando Apartment Gbox — preserved copy");
        Note(layout, "8m x 6m footprint; fixed bed position; stationary camera switch; no locomotion");
        if (bed != null) Note(layout, "Player start/return: Bed (headset off)");

        if (desk != null)
        {
            Place("Assets/Imports/PKS_Laptoplow/PKS_Laptop_low/Prefabs/SM_PKS_Laptop_low.prefab", layout,
                "Laptop — digital balance visual bridge ($0; headset asset unavailable)", desk.position + Vector3.up * 0.52f, desk.rotation, 0.7f);
            Place("Assets/Prefabs/CoffeeCup.fbx", layout, "Used Cup — desk dressing", desk.position + desk.right * 0.42f + Vector3.up * 0.48f, desk.rotation, 0.16f);
        }
        else MissingAnchor("desk", "Laptop and used cup");

        if (window != null)
            Place("Assets/Prefabs/Curtains.fbx", layout, "Curtains — dirty window dressing", window.position + Vector3.up * 0.1f, window.rotation, 1.7f);
        else MissingAnchor("Window", "Curtains");

        if (fridge != null)
            Place("Assets/Imports/nappin/HouseInteriorPack/Prefabs/(Prb)Minifridge.prefab", layout, "Mini Fridge", fridge.position, fridge.rotation, 1.15f);
        else MissingAnchor("Minifridge", "Mini fridge");

        if (lamp != null)
            Place("Assets/Prefabs/Lamp.fbx", layout, "Lamp — weak return lighting prop", lamp.position, lamp.rotation, 1.25f);
        else MissingAnchor("lamp", "Lamp");

        if (clothes != null)
        {
            Place("Assets/Prefabs/greenShirt.fbx", layout, "Dirty Clothes — green shirt", clothes.position + new Vector3(-0.22f, 0f, 0.08f), clothes.rotation, 0.6f);
            Place("Assets/Prefabs/blackShirt.fbx", layout, "Dirty Clothes — black shirt", clothes.position + new Vector3(0.16f, 0f, -0.08f), clothes.rotation, 0.6f);
            Place("Assets/Prefabs/blueShirt.fbx", layout, "Dirty Clothes — blue shirt", clothes.position + new Vector3(0.05f, 0f, 0.24f), clothes.rotation, 0.6f);
        }
        else MissingAnchor("Dirty clothes", "shirt props");

        if (trashA != null)
        {
            Place("Assets/Prefabs/Trash Bag.fbx", layout, "Trash Cluster A", trashA.position, trashA.rotation, 0.75f);
            Place("Assets/Prefabs/Trash Bag.fbx", layout, "Trash Cluster A — second bag", trashA.position + new Vector3(0.3f, 0f, -0.18f), trashA.rotation, 0.6f);
        }
        if (trashB != null)
            Place("Assets/Prefabs/Trash Bag.fbx", layout, "Trash Cluster B", trashB.position, trashB.rotation, 0.65f);
        if (trashA == null && trashB == null) MissingAnchor("trash cluster", "trash bag props");

        Note(layout, "Not placed — no Project 6 model asset: headset; digital balance display; palm tree");
        if (bed != null) ConfigureStationaryCamera(bed.position + new Vector3(0f, 1.35f, -4.2f), bed.position + new Vector3(0.5f, 0.35f, 0f));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ApartmentTarget);
    }

    private static void BuildParadise()
    {
        Scene scene = CopyAndOpen(ParadiseSource, ParadiseTarget);
        Transform layout = PrepareLayoutRoot("DEREK LAYOUT — PARADISE / START HERE", scene);
        Transform floor = Find("Floor");
        if (floor == null)
        {
            MissingAnchor("Floor", "beach props");
            EditorSceneManager.SaveScene(scene, ParadiseTarget);
            return;
        }

        Renderer floorRenderer = floor.GetComponent<Renderer>();
        Vector3 center = floorRenderer != null ? floorRenderer.bounds.center : floor.position;
        float top = floorRenderer != null ? floorRenderer.bounds.max.y : floor.position.y;
        Note(layout, "Source: Fernando Beach Scene — preserved copy");
        Note(layout, "8m x 6m shared footprint; player remains seated; stationary camera / no locomotion");

        Vector3 chairPosition = center + new Vector3(-1.35f, top, -0.55f);
        Place("Assets/Prefabs/beachChair.fbx", layout, "Beach Chair — fixed Paradise start position", chairPosition, Quaternion.Euler(0f, 25f, 0f), 1.45f);
        Place("Assets/Prefabs/coconutDrink.fbx", layout, "Coconut Drink — beach chair dressing", center + new Vector3(-0.75f, top + 0.18f, -0.65f), Quaternion.identity, 0.32f);
        Place("Assets/Prefabs/umbrella.fbx", layout, "Beach Umbrella — paradise shade", center + new Vector3(1.65f, top, 0.35f), Quaternion.identity, 2.5f);
        Note(layout, "Not placed — no Project 6 model asset: palm tree; water/ocean horizon mesh");
        ConfigureStationaryCamera(center + new Vector3(0f, 1.8f, -4.6f), chairPosition + new Vector3(0.25f, 0.65f, 0f));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ParadiseTarget);
    }

    private static Scene CopyAndOpen(string source, string target)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(target) == null)
        {
            if (!AssetDatabase.CopyAsset(source, target))
                throw new System.InvalidOperationException("Could not copy source scene " + source);
            AssetDatabase.Refresh();
        }
        return EditorSceneManager.OpenScene(target, OpenSceneMode.Single);
    }

    private static Transform PrepareLayoutRoot(string name, Scene scene)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Object.DestroyImmediate(old);
        GameObject root = new GameObject(name);
        return root.transform;
    }

    private static void Place(string assetPath, Transform parent, string name, Vector3 position, Quaternion rotation, float largestDimension)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            Debug.LogWarning("Requested asset was not found and was not substituted: " + assetPath);
            return;
        }
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        instance.name = name;
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        ScaleToLargestDimension(instance, largestDimension);
    }

    private static Transform Find(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, objectName);
            if (found != null) return found;
        }
        return null;
    }

    private static Transform FindRecursive(Transform current, string objectName)
    {
        if (current.name == objectName) return current;
        for (int i = 0; i < current.childCount; i++)
        {
            Transform found = FindRecursive(current.GetChild(i), objectName);
            if (found != null) return found;
        }
        return null;
    }

    private static void Note(Transform parent, string note)
    {
        GameObject marker = new GameObject("GUIDELINE — " + note);
        marker.transform.SetParent(parent);
    }

    private static void MissingAnchor(string anchor, string requestedProps)
    {
        Debug.LogWarning("Graybox anchor '" + anchor + "' missing; did not place " + requestedProps + ".");
    }

    private static void ScaleToLargestDimension(GameObject instance, float desiredLargestDimension)
    {
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        float current = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (current > 0.0001f) instance.transform.localScale *= desiredLargestDimension / current;
    }

    private static Bounds RendererBounds(GameObject instance)
    {
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(instance.transform.position, Vector3.zero);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static void FitAndGround(string name, float desiredLargestDimension, float floorTop)
    {
        Transform item = Find(name);
        if (item == null) { Debug.LogWarning("Apartment prop missing: " + name); return; }
        Bounds before = RendererBounds(item.gameObject);
        float largest = Mathf.Max(before.size.x, Mathf.Max(before.size.y, before.size.z));
        if (largest <= 0.0001f) return;
        item.localScale *= desiredLargestDimension / largest;
        Bounds after = RendererBounds(item.gameObject);
        item.position += Vector3.up * (floorTop - after.min.y);
        PrefabUtility.RecordPrefabInstancePropertyModifications(item);
        EditorUtility.SetDirty(item.gameObject);
    }

    private static void LayFlatAndGround(string name, float yaw, float floorTop)
    {
        Transform item = Find(name);
        if (item == null) { Debug.LogWarning("Apartment prop missing: " + name); return; }
        item.rotation = Quaternion.Euler(90f, yaw, 0f);
        GroundOnly(item, floorTop);
    }

    private static void GroundOnly(string name, float floorTop)
    {
        Transform item = Find(name);
        if (item == null) { Debug.LogWarning("Apartment prop missing: " + name); return; }
        GroundOnly(item, floorTop);
    }

    private static void GroundOnly(Transform item, float floorTop)
    {
        Bounds bounds = RendererBounds(item.gameObject);
        item.position += Vector3.up * (floorTop - bounds.min.y);
        PrefabUtility.RecordPrefabInstancePropertyModifications(item);
        EditorUtility.SetDirty(item.gameObject);
    }

    private static void SetFloorPosition(string name, Vector3 position, float yaw, float floorTop)
    {
        Transform item = Find(name);
        if (item == null) { Debug.LogWarning("Apartment prop missing: " + name); return; }
        item.position = position;
        item.rotation = Quaternion.Euler(90f, yaw, 0f);
        GroundOnly(item, floorTop);
    }

    private static void SetGroundPosition(string name, Vector3 position, float floorTop)
    {
        Transform item = Find(name);
        if (item == null) { Debug.LogWarning("Apartment prop missing: " + name); return; }
        item.position = position;
        GroundOnly(item, floorTop);
    }

    private static void ConfigureStationaryCamera(Vector3 position, Vector3 lookAt)
    {
        Camera camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null) return;
        camera.transform.position = position;
        camera.transform.LookAt(lookAt);
        camera.fieldOfView = 60f;
    }
}
#endif
