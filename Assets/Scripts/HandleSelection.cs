using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HandleSelection : MonoBehaviour {
	//can we grab the selected protein information here
	//and show it in a corner, either zoom structure or details information 
	// Use this for initialization
	public string iname;
	public string description;
	public GameObject TextUI;// description_ui;
	void Start () {

	}
	
	string filterForPrefix(string name){
		string iname = name;
		var elem = name.Split('_');
		if (elem [0].StartsWith ("cytoplasme") || elem [0].StartsWith ("interior") || elem [0].StartsWith ("surface")) {
			iname = name.Replace(elem[0]+"_","");
		}
		return iname;
	}
	// Update is called once per frame
	void Update () {
		if (SceneManager.Instance.SelectedElement != -1) {
			//activate canvas
			if (!TextUI.transform.parent.gameObject.activeSelf)
				TextUI.transform.parent.gameObject.SetActive (true);
			if (SceneManager.Instance.AllIngredients == null) 
				SceneManager.Instance.AllIngredients = Helper.GetAllIngredientsInfo ();
			iname = SceneManager.Instance.ProteinNames [(int)SceneManager.Instance.ProteinInstanceInfos [SceneManager.Instance.SelectedElement].x];
			//we need to remove the prefix if any
			iname = filterForPrefix(iname);
			//iname = iname.Replace("_")//cytoplasme interior
			Debug.Log (SceneManager.Instance.AllIngredients.Count);
			Debug.Log (SceneManager.Instance.AllIngredients [iname.ToString ()].ToString ());
			description = SceneManager.Instance.AllIngredients [iname] ["description"];
			Debug.Log ("handle " + iname + " instance " + SceneManager.Instance.SelectedElement.ToString ());
			Debug.Log (description);
			TextUI.GetComponent<Text>().text = description;
			//TextUI.GetComponent<RectTransform>()
			//description_ui.CalcHeight(description, sizeX)
		} else {
			if (TextUI.transform.parent.gameObject.activeSelf)
				TextUI.transform.parent.gameObject.SetActive(false);

		}
	}
}
