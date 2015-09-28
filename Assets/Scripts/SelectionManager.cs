using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SelectionManager : MonoBehaviour
{
    // Declare the scene manager as a singleton
    private static SelectionManager _instance = null;
    public static SelectionManager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<SelectionManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_SelectionManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_SelectionManager") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<SelectionManager>();
            }
            return _instance;
        }
    }

    //--------------------------------------------------------------

    private int _selectedInstanceID = -1;

    private GameObject _selectionGameObject;
    public GameObject SelectionGameObject
    {
        get
        {
            if (_selectionGameObject != null) return _selectionGameObject;

            var go = GameObject.FindGameObjectWithTag("Selection");
            if (go != null) _selectionGameObject = go;
            else
            {
                _selectionGameObject = new GameObject("Selection");
                _selectionGameObject.tag = "Selection";
                _selectionGameObject.AddComponent<SphereCollider>();
            }

            return _selectionGameObject;
        }
    }

    public void SetSelectedElement(int instanceID)
    {
#if UNITY_EDITOR
        Debug.Log("Selected element id: " + instanceID);

        if (instanceID >= SceneManager.Instance.ProteinInstancePositions.Count) return;

        // If element id is different than the currently selected element
        if (SceneManager.Instance.SelectedElementID != instanceID)
        {
            // if new selected element is greater than one update set and set position to game object
            if (instanceID > -1)
            {
                float radius = SceneManager.Instance.ProteinRadii[(int)SceneManager.Instance.ProteinInstanceInfos[instanceID].x] * PersistantSettings.Instance.Scale;
                Selection.activeGameObject = SelectionGameObject;
                SelectionGameObject.GetComponent<SphereCollider>().radius = radius;

                SelectionGameObject.transform.position = SceneManager.Instance.ProteinInstancePositions[instanceID] * PersistantSettings.Instance.Scale;
                SelectionGameObject.transform.rotation = MyUtility.Vector4ToQuaternion(SceneManager.Instance.ProteinInstanceRotations[instanceID]);
            }

            _selectedInstanceID = instanceID;
        }
#endif

        
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateSelectedElement();
    }
    
    private void UpdateSelectedElement()
    {
        if (_selectedInstanceID == -1)
        {
            //SelectedElement.SetActive(false);
            return;
        }

        if (_selectionGameObject.transform.hasChanged)
        {
            //Debug.Log("Selected instance transform changed");

            SceneManager.Instance.ProteinInstancePositions[_selectedInstanceID] = _selectionGameObject.transform.position / PersistantSettings.Instance.Scale;
            SceneManager.Instance.ProteinInstanceRotations[_selectedInstanceID] = MyUtility.QuanternionToVector4(_selectionGameObject.transform.rotation);

            ComputeBufferManager.Instance.ProteinInstancePositions.SetData(SceneManager.Instance.ProteinInstancePositions.ToArray());
            ComputeBufferManager.Instance.ProteinInstanceRotations.SetData(SceneManager.Instance.ProteinInstanceRotations.ToArray());

            _selectionGameObject.transform.hasChanged = false;
        }
    }
}
