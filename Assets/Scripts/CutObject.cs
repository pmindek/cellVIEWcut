using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum CutType
{
    Plane = 0,
    Sphere = 1,
    Cube = 2,
    Cylinder = 3,
    Cone = 4,
    None = 5
};

[System.Serializable]
public class CutItem
{
    public string Name;
    public bool State;
}

[System.Serializable]
public class CutItemRanges //i will maybe use this later?
{
    public string Name;
    public int r0;
    public int r1;
    public int r2;

    public float d0; // delta 0
    public float d1; // delta 1
}

public class CutParameters
{
    public float range0;
    public float range1;

    public int countAll; //parameters of the multi-range slider
    public int count0;
    public int count1;

    public float value1; //other parameters
    public float value2;
    public float fuzziness;
    public float fuzzinessDistance;
    public float fuzzinessCurve;
}


[ExecuteInEditMode]
public class CutObject : MonoBehaviour
{
    public static int UniqueId;

    public bool Hidden;
    private bool tree_isVisible = true;
    
    public CutType CutType;

    public bool Inverse;
    
    //[Range(0, 1)]
    //public float Value1;

    //[Range(0, 1)]
    //public float Value2;

    //[Range(0, 1)]
    //public float Fuzziness;

    //[Range(0, 1)]
    //public float FuzzinessDistance;

    //[Range(0.01f, 3)]
    //public float FuzzinessCurve;

    public bool Optimize;
    public bool DataSensitiveSliders;

    [HideInInspector]
    public CutType PreviousCutType;

    [HideInInspector]
    public List<CutItem> ProteinCutFilters = new List<CutItem>();

    [HideInInspector]
    public List<CutItem> HistogramProteinTypes = new List<CutItem>();

    [HideInInspector]
    public List<CutItemRanges> HistogramRanges = new List<CutItemRanges>(); //i will maybe use this later?

    [HideInInspector]
    public List<CutParameters> ProteinTypeParameters = new List<CutParameters>(); //this structure stores the cutaway parameters per protein type

	//private TreeViewControlEditor _tree;
	//private RecipeTreeUI _tree_ui;

    [HideInInspector]
    public float[] RangeValues = new float[2] { 0.2f, 0.3f };
    public List<float[]> TreeRangeValues = new List<float[]>();

    [HideInInspector]
    public bool initOptimizing = true;
    [HideInInspector]
    public bool distanceOptimized = false;
    [HideInInspector]
    public float findDistanceFrom = 0.0f;
    [HideInInspector]
    public float findDistanceTo = 1.0f;
    [HideInInspector]
    public float initialRange0 = 0.0f;
    [HideInInspector]
    public float initialRange1 = 0.0f;

    [NonSerialized] public int SelectedProteinType = -1;

    public void InitCutParameters()
    {
        ProteinTypeParameters.Clear();
        ProteinTypeParameters.AddRange(Enumerable.Repeat(new CutParameters()
        {
            range0 = 0.0f,
            range1 = 0.0f,

            countAll = 0,
            count0 = 0,
            count1 = 0,

            value1 = 0.5f,
            value2 = 0.5f,
            fuzziness = 0.0f,
            fuzzinessDistance = 1.0f,
            fuzzinessCurve = 1.0f
        }, SceneManager.Instance.ProteinNames.Count
        ));
    }

    public CutParameters GetCutParametersFor(int ingredientId)
    {
        if (ProteinTypeParameters.Count == 0 || ProteinTypeParameters.Count <= ingredientId)
        {
            InitCutParameters();
        }

        return ProteinTypeParameters[ingredientId];
    }

    public void SetCutParametersFor(int ingredientId, CutParameters cutParameters)
    {
        if (ProteinTypeParameters.Count == 0 || ProteinTypeParameters.Count <= ingredientId)
        {
            InitCutParameters();
        }

        ProteinTypeParameters[ingredientId] = cutParameters;
    }
    
    public void SetFuzzinessParametersFor(int ingredientId, float value1, float value2, float value3)
    {
        ProteinTypeParameters[ingredientId].fuzziness = value1;
        ProteinTypeParameters[ingredientId].fuzzinessDistance = value2;
        ProteinTypeParameters[ingredientId].fuzzinessCurve = value3;
    }

    public float[] GetRangeValues(int ingredientId)
    {
        //return RangeValues;
        int count = TreeRangeValues.Count;
        if (count <= ingredientId)
        {
            if (ingredientId >= TreeRangeValues.Capacity)
                TreeRangeValues.Capacity = ingredientId + 1;
            TreeRangeValues.AddRange(Enumerable.Repeat(new float[2] {0.5f, 0.1f} , ingredientId + 1 - count));
        }

        return TreeRangeValues[ingredientId];
    }

    public void SetRangeValues(int ingredientId, float[] rangeValues)
    {
        RangeValues = rangeValues;
        int count = TreeRangeValues.Count;
        if (count <= ingredientId)
        {
            if (ingredientId >= TreeRangeValues.Capacity)
                TreeRangeValues.Capacity = ingredientId + 1;
            TreeRangeValues.AddRange(Enumerable.Repeat(new float[2] { 0.3f, 0.1f }, ingredientId + 1 - count));
        }
        TreeRangeValues[ingredientId] = rangeValues;
    }


    public void SetCutItems(List<string> names)
    {
        foreach(var name in names)
        {
            /*ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
            HistogramProteinTypes.Add(new CutItem() { Name = name, State = true });
            HistogramRanges.Add(new CutItemRanges() { Name = name, r0 = 0, r1 = 0, r2 = 0, d0 = 0.0f, d1 = 0.0f });*/
            AddCutItem(name);
        }
    }

	public void RemoveCutItem (string name)
    {
		CutItem toRemove=null;
		foreach(CutItem cu in ProteinCutFilters){
			if (string.Equals(cu.Name,name)){
				toRemove = cu;
				break;
			}
		}
		if (toRemove != null)ProteinCutFilters.Remove(toRemove);

        toRemove = null;
        foreach (CutItem cu in HistogramProteinTypes)
        {
            if (string.Equals(cu.Name, name))
            {
                toRemove = cu;
                break;
            }
        }
        if (toRemove != null) HistogramProteinTypes.Remove(toRemove);

        CutItemRanges toRemoveR = null;
        foreach (CutItemRanges cu in HistogramRanges)
        {
            if (string.Equals(cu.Name, name))
            {
                toRemoveR = cu;
                break;
            }
        }
        if (toRemoveR != null) HistogramRanges.Remove(toRemoveR);
    }

	public  void AddCutItem (string name)
    {
		ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
        HistogramProteinTypes.Add(new CutItem() { Name = name, State = true });
        HistogramRanges.Add(new CutItemRanges() { Name = name, r0 = 0, r1 = 0, r2 = 0, d0 = 0.0f, d1 = 0.0f });
	}

	public void ToggleCutItem (string name, bool toggle)
    {
		foreach(CutItem cu in ProteinCutFilters){
			if (string.Equals(cu.Name,name)){

                Debug.Log("toggling " + name);

				cu.State = toggle;
				break;
			}
		}
    }

	public void ToggleAllCutItem (bool toggle)
    {
		foreach(CutItem cu in ProteinCutFilters)
        {
			cu.State = toggle;
		}
    }


    public void ToggleAllHistogramCutItems(bool toggle)
    {
        foreach (CutItem cu in HistogramProteinTypes)
        {
            cu.State = toggle;
        }
    }
    public void ToggleHistogramCutItem(string name, bool toggle)
    {
        foreach (CutItem cu in HistogramProteinTypes)
        {
            if (string.Equals(cu.Name, name))
            {
                cu.State = toggle;
                break;
            }
        }
    }



	////is it awake or load ?
 //   void Awake()
 //   {
 //       Debug.Log("Init cut object");
 //       if (ProteinCutFilters.Count == 0 || HistogramProteinTypes.Count == 0 || HistogramRanges.Count == 0)
 //       {
 //           //ProteinCutFilters.Clear();//?maybe shouldnt clear on Awake ?
 //           ProteinCutFilters.Clear();
 //           HistogramProteinTypes.Clear();
 //           HistogramRanges.Clear();
 //           SetCutItems(SceneManager.Instance.ProteinNames);
 //       }
	//	_tree = GetComponent<TreeViewControlEditor> ();
	//	_tree_ui = GetComponent<RecipeTreeUI> ();

 //       _tree.hideFlags = HideFlags.HideInInspector;
 //       _tree_ui.hideFlags = HideFlags.HideInInspector;

 ////   }

	//public void ToggleTree(bool value)
 //   {
	//	_tree.DisplayOnGame = value;
	//	tree_isVisible = value;
	//}

	//public void ShowTree(Vector3 pos,Vector2 size){
	//	_tree.DisplayOnGame = true;
	//	_tree.Width = (int)size.x-20;
	//	_tree.Height = (int)size.y-30;
	//	_tree.X = (int)pos.x+10;
	//	_tree.Y = (Screen.height - (int)size.y)+10;//invert ?
	//	tree_isVisible = true;
	//	Debug.Log ("should show tree");
	//}

	//public void HideTree()
 //   {
	//	Debug.Log ("should be hided");
	//	_tree.DisplayOnGame = false;
	//	tree_isVisible = false;
	//}

	//public void SetTree()
 //   {
	//	Debug.Log ("we are setting the tree");
	//	_tree_ui.ClearTree ();
	//	GameObject root = GameObject.Find (SceneManager.Instance.scene_name);
	//	if (root != null) {
	//		_tree_ui.populateRecipeGameObject (root);
	//	}
	//	//if (CellPackLoader.resultData != null)
	//		//_tree_ui.populateRecipeJson (CellPackLoader.resultData);
	//		//_tree_ui.populateRecipe (PersistantSettings.Instance.hierarchy);
	//	else {
	//		Debug.Log ("cellPackResult not availble");
	//	}
	//	HideTree ();
	//	tree_isVisible = false;
	//}

	//public bool TreeHasFocus(Vector2 mousepos)
 //   {
	//	Rect rect = new Rect(_tree.X-60, _tree.Y-60, _tree.Width+90, _tree.Height+90);
	//	return rect.Contains(mousepos);
	//}

    private bool previousHiddenValue;

    public void Update ()
    {
		if (CutType != PreviousCutType || gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            SetMesh();
            PreviousCutType = CutType;
        }

	    if (Hidden != previousHiddenValue)
	    {
	        previousHiddenValue = Hidden;
            SetHidden(Hidden);
	    }
    }

    void OnEnable()
    {
        UniqueId++;
        InitCutParameters();

        // Register this object in the cut object cache
        if (!SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Add(this);
        }
    }

    void OnDisable()
    {
        // De-register this object in the cut object cache
        if (SceneManager.CheckInstance() && SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Remove(this);
        }
    }

    public void SetHidden(bool value, bool highlight = false)
    {
        gameObject.GetComponent<MeshRenderer>().enabled = !value;
        gameObject.GetComponent<TransformHandle>().enabled = !value;
        if(gameObject.GetComponent<Collider>() != null) gameObject.GetComponent<Collider>().enabled = !value;
        if (highlight) SelectionManager.Instance.SetHandleSelected(gameObject.GetComponent<TransformHandle>());
    }

    public void SetMesh()
    {
        switch (CutType)
        {
            case CutType.Plane:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Plane") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<BoxCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                break;

            case CutType.Sphere:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Sphere") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<SphereCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                break;

            case CutType.Cube:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cube") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                break;

            case CutType.Cone:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cone") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                break;

            case CutType.Cylinder:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cylinder") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                break;

            case CutType.None:
                gameObject.GetComponent<MeshRenderer>().enabled = false;
                gameObject.GetComponent<TransformHandle>().enabled = false;
                break;
        }
    }
}
