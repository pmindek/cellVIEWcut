using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PersistantSettings : MonoBehaviour
{
    public string LastSceneLoaded;

    // Base settings
    public float Scale = 0.065f;
    public int ContourOptions;
    public float ContourStrength;
    public bool DebugObjectCulling;
    public bool EnableOcclusionCulling;

    //DNA/RNA settings
    public bool EnableDNAConstraints;
    public float DistanceContraint;
    public float AngularConstraint;

    // Cross section
    public bool EnableCrossSection;
    public float CrossSectionPlaneDistance = 0;
    public Vector3 CrossSectionPlaneNormal;

    // Lod infos
    public bool EnableLod;
    public float FirstLevelOffset = 0;
    public Vector4[] LodLevels = new Vector4[8];
    
    // Declare the DisplaySettings as a singleton
    private static PersistantSettings _instance = null;
    public static PersistantSettings Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<PersistantSettings>();
            if (_instance == null)
            {
                var go = GameObject.Find("_PeristantSettings");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_PeristantSettings") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<PersistantSettings>();
            }
            return _instance;
        }
    }
}
