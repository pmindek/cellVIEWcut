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
    Cone = 4
};

[System.Serializable]
public class CutItem
{
    public string Name;
    public bool State;
}

[ExecuteInEditMode]
public class CutObject : MonoBehaviour
{
    public bool Hidden;
    private bool tree_isVisible = true;
    
    public CutType CutType;
    
    [Range(0, 1)]
    public float Value1;

    [Range(0, 1)]
    public float Value2;

    [HideInInspector]
    public CutType PreviousCutType;

    [HideInInspector]
    public List<CutItem> ProteinCutFilters = new List<CutItem>();

	private TreeViewControl _tree;
	private RecipeTreeUI _tree_ui;

    [HideInInspector]
    public float[] RangeValues = new float[2] { 0.2f, 0.3f };

    public float[] GetRangeValues(int ingredientId)
    {
        return RangeValues;
    }

    public void SetRangeValues(int ingredientId, float[] rangeValues)
    {
        RangeValues = rangeValues;
    }


    public void SetCutItems(List<string> names)
    {
        foreach(var name in names)
        {
            ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
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
	}

	public  void AddCutItem (string name)
    {
		ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
	}

	public void ToggleCutItem (string name, bool toggle)
    {
		foreach(CutItem cu in ProteinCutFilters){
			if (string.Equals(cu.Name,name)){
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

	//is it awake or load ?
    void Awake()
    {
        Debug.Log("Init cut object");
		if (ProteinCutFilters.Count == 0)
        //ProteinCutFilters.Clear();//?maybe shouldnt clear on Awake ?
        	SetCutItems(SceneManager.Instance.ProteinNames);
		_tree = GetComponent<TreeViewControl> ();
		_tree_ui = GetComponent<RecipeTreeUI> ();

        _tree.hideFlags = HideFlags.HideInInspector;
        _tree_ui.hideFlags = HideFlags.HideInInspector;

    }

    void OnEnable()
    {
        // Register this object in the cut object cache
        if (!SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Add(this);
        }
		//check the tree
		if (_tree.enabled) SetTree ();
    }

    void OnDisable()
    {
        // De-register this object in the cut object cache
        if (SceneManager.CheckInstance() && SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Remove(this);
        }
    }

	public void ToggleTree(bool value)
    {
		_tree.DisplayOnGame = value;
		tree_isVisible = value;
	}

	public void ShowTree(Vector3 pos,Vector2 size){
		_tree.DisplayOnGame = true;
		_tree.Width = (int)size.x-20;
		_tree.Height = (int)size.y-30;
		_tree.X = (int)pos.x+10;
		_tree.Y = (Screen.height - (int)size.y)+10;//invert ?
		tree_isVisible = true;
		Debug.Log ("should show tree");
	}

	public void HideTree()
    {
		Debug.Log ("should be hided");
		_tree.DisplayOnGame = false;
		tree_isVisible = false;
	}

	public void SetTree()
    {
		Debug.Log ("we are setting the tree");
		_tree_ui.ClearTree ();
		GameObject root = GameObject.Find (SceneManager.Instance.scene_name);
		if (root != null) {
			_tree_ui.populateRecipeGameObject (root);
		}
		//if (CellPackLoader.resultData != null)
			//_tree_ui.populateRecipeJson (CellPackLoader.resultData);
			//_tree_ui.populateRecipe (PersistantSettings.Instance.hierarchy);
		else {
			Debug.Log ("cellPackResult not availble");
		}
		HideTree ();
		tree_isVisible = false;
	}

	public bool TreeHasFocus(Vector2 mousepos)
    {
		Rect rect = new Rect(_tree.X-60, _tree.Y-60, _tree.Width+90, _tree.Height+90);
		return rect.Contains(mousepos);
	}

	public void Update ()
    {
		if (CutType != PreviousCutType || gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            SetMesh();
            PreviousCutType = CutType;
        }

        //if (Hidden || Application.isPlaying)
        //{
        //    GetComponent<Collider>().enabled = false;
        //    GetComponent<MeshRenderer>().enabled = false;//why ?
        //    GetComponent<TransformHandle>().enabled = false;
        //}
        //else
        //{
        //    GetComponent<Collider>().enabled = true;
        //    GetComponent<MeshRenderer>().enabled = true;
        //    GetComponent<TransformHandle>().enabled = true;
        //}
    }

    public void SetMesh()
    {
        switch (CutType)
        {
            case CutType.Plane:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Plane") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<BoxCollider>();
                break;

            case CutType.Sphere:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Sphere") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<SphereCollider>();
                break;

            case CutType.Cube:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cube") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Cone:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cone") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Cylinder:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cylinder") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
