using System;
using UnityEngine;
public class NewBehaviourScript : MonoBehaviour
{
    private static NewBehaviourScript _instance = null;
    public static NewBehaviourScript Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<NewBehaviourScript>();
            if (_instance == null)
            {
                var go = GameObject.Find("_SceneManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_SceneManager") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<NewBehaviourScript>();
            }

            //_instance.OnUnityReload();
            return _instance;
        }
    }

    public static bool CheckInstance()
    {
        return _instance != null;
    }


    public TreeViewController TreeViewController;

	// Use this for initialization
	void Start ()
    {
        int x;
	    foreach (var node in PersistantSettings.Instance.hierachy)
	    {
            TreeViewController.AddNodeObject(node.path, new object[] { node.name }, "Text");
        }
        TreeViewController.Init();
    }

    public void UpdateValues()
    {
        TreeViewController.UpdateRangeValues();
    }
}
