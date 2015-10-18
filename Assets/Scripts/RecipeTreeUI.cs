using UnityEngine;
using System;
using System.Collections;
using SimpleJSON;

[ExecuteInEditMode]
public class RecipeTreeUI : MonoBehaviour {

	private bool filter_cut = true;
	public TreeViewControl m_myTreeView;
	private int anid = 0;//counter of item
	private SelectionManager _sel;
	public CutObject cutobject;
	public delegate void changeItem(TreeViewItem item,params object[] argsRest);

	private float clicked = 0;
	private float clicktime = 0;
	private float clickdelay = 0.5f;
	private bool doubleclick;
	private bool rigthclick = false;
	private bool toggle_display_on_double_click = false;
	private int clikCount;
	private bool mouseintree;
	private NavigateCamera nvcamera;
	// Use this for initialization


	public void Awake()
    {
		//_sel = GameObject.Find ("Selection").GetComponent<HandleSelection>();
		nvcamera = Camera.main.GetComponent<NavigateCamera> ();
		populateRecipeGameObject (GameObject.Find(SceneManager.Instance.scene_name));
        
	}


	public void Start () {
		//foreach (TreeViewItem item in m_myTreeView.Items) 
		//	ApplyFunctionRec (AddEvents,item);
    }


	
	bool DoubleClick(){
		if (Input.GetMouseButtonDown (0)) {
			clikCount = Event.current.clickCount;
			clicked++;
			if (clicked == 1) clicktime = Time.time;
		}         
		if (clicked > 1 && Time.time - clicktime < clickdelay) {
			clicked = 0;
			clicktime = 0;
			return true;
		} else if (clicked > 2 ) clicked = 0;         
		return false;
	}


	public void OnGUI(){
		mouseintree = m_myTreeView.HasFocus (Event.current.mousePosition);
		//nvcamera.lockInteractions = mouseintree&m_myTreeView.DisplayOnGame;
		//if mouse contains lock the navigation
		doubleclick = DoubleClick ();
		///if (Input.GetMouseButtonUp (1)) 
		//	rigthclick = false;
		doubleclick = Input.GetMouseButtonDown (1)||Input.GetMouseButtonUp (1);
		//if (clikCount > 1)
		//	doubleclick = true;
		//else 
		//	doubleclick = false;
	}

	public void toggleChecked(TreeViewItem item, params object[] argsRest ){
		if (item == m_myTreeView.RootItem)
			return;
		bool value = (bool)argsRest[0];
		item.IsChecked = value;
		int itemid = SceneManager.Instance.ProteinNames.IndexOf (item.Parent.Header + "_" + item.Header);
		//apply to the object visibility
		string ingname = item.Parent.Header + "_" + item.Header;
		if (item.Parent.Header.Contains ("membrane")) {
			ingname = item.Header;
			itemid = SceneManager.Instance.ProteinNames.IndexOf (ingname);
		}
		//Debug.Log (item.Parent.Header + "_" + item.Header);
		//Debug.Log ("toggle " + itemid.ToString ());
		if (itemid == -1) {
			itemid = SceneManager.Instance.CurveIngredientsNames.IndexOf (item.Parent.Header + "_" + item.Header);
			//Debug.Log ("toggle " + itemid.ToString ());
			if (itemid == -1) {
				return;
			} else {
				if (!filter_cut)SceneManager.Instance.CurveIngredientToggleFlags [itemid] = Convert.ToInt32 (value);
			}
		} else {
			if (!filter_cut)SceneManager.Instance.ProteinToggleFlags [itemid] = Convert.ToInt32 (value);
		}
		if (filter_cut) {
			cutobject.ToggleCutItem(ingname,value);
		}
	}

	void ApplyFunctionRec(changeItem mehod,TreeViewItem item){
		if (item.HasChildItems ()) {
			foreach (TreeViewItem i in item.Items) 
				ApplyFunctionRec (mehod, i);
		} else {
			mehod (item);
		}
	}

	void ApplyFunctionRecValue(changeItem mehod,TreeViewItem item, bool value){
		if (item.HasChildItems ()) {
			foreach (TreeViewItem i in item.Items) 
				ApplyFunctionRecValue(mehod,i,value);
		}
		mehod (item,value);
	}
	public void HandlerFilterCut(object sender, System.EventArgs args)
	{
		//Debug.Log(string.Format("{0} detected: {1}", args.GetType().Name, (sender as TreeViewItem).Header));
		TreeViewItem item = sender as TreeViewItem;
		bool update = false;
		if (args.GetType ().Name == "CheckedEventArgs") {
			//toggle on
			ApplyFunctionRecValue (toggleChecked, item, true);
			update = true;
		}
		if (args.GetType ().Name == "UncheckedEventArgs") {
			//toggle off
			ApplyFunctionRecValue (toggleChecked, item, false);
			update = true;
		}
		if (args.GetType ().Name == "SelectedEventArgs") {
			//Debug.Log ("ok selected "+item.Header);
			if (!item.HasChildItems ()) {
			}
		}
	}

	public void highlightProteinID(int ingredientId){
		/*if (ingredientId < 0 || ingredientId > SceneManager.Instance.ProteinInstanceID.Count) {
			return;
		}
		Vector4 pinst = SceneManager.Instance.ProteinInstanceID [ingredientId];
		for (int i= (int)pinst.x;i <= (int)(pinst.x+pinst.y);i++){
		//for (int i=0;i <SceneManager.Instance.ProteinInstanceInfos.Count;i++){
			//if (SceneManager.Instance.ProteinInstanceInfos[i].x == ingredientId){
			SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 1, SceneManager.Instance.ProteinInstanceInfos[i].z); 
			//}
		}*/
	}

	public void highlightCurveID (int curveID){
		for (int i=0;i <SceneManager.Instance.CurveIngredientsInfos.Count;i++){
			//if (SceneManager.Instance.CurveControlPointsInfos[i].x == curveID)
			SceneManager.Instance.CurveControlPointsInfos[i] = new Vector4(SceneManager.Instance.CurveControlPointsInfos[i].x, SceneManager.Instance.CurveControlPointsInfos[i].y,1, 0); 
		}
	}

	public void highlightHierarchy(GameObject parent){
		if (parent.name.Contains ("cytoplasme") || parent.name.Contains ("membrane") || parent.name.Contains ("surface") || parent.name.Contains ("interior")) {
			Debug.Log ("toggle everything else");
			foreach (Transform child in parent.transform) {
				var ingredientId = SceneManager.Instance.ProteinNames.IndexOf (parent.name + "_" + child.name);
				if (parent.name.Contains ("membrane")) {
					ingredientId = SceneManager.Instance.ProteinNames.IndexOf (child.name);
				}
				if (ingredientId == -1){
					ingredientId = SceneManager.Instance.CurveIngredientsNames.IndexOf (parent.name + "_" + child.name);
					if (ingredientId == -1)
						continue;
					else {
						highlightCurveID(ingredientId);
					}
					continue;
				}
				//Debug.Log (parent.name + "_" + child.name+ " " + ingredientId.ToString());
				highlightProteinID (ingredientId);
			}
		} else {
			foreach (Transform child in parent.transform) {
				highlightHierarchy(child.gameObject);
			}
		}
	}

    [HideInInspector]
    public int currentSelectedIngredient = -1;

	public void Handler(object sender, System.EventArgs args)
    {
        //Debug.Log(string.Format("{0} detected: {1}", args.GetType().Name, (sender as TreeViewItem).Header));
		TreeViewItem item = sender as TreeViewItem;
		bool update=false;
		if (args.GetType ().Name == "CheckedEventArgs") {
			//toggle on
			ApplyFunctionRecValue(toggleChecked,item,true);
			update=true;
		}
		if (args.GetType ().Name == "UncheckedEventArgs") {
			//toggle off
			ApplyFunctionRecValue(toggleChecked,item,false);
			update=true;
		}
		if ((args.GetType ().Name == "ClickEventArgs"))
        {//(args.GetType ().Name == "SelectedEventArgs")||

			Debug.Log ("ok selected "+item.Header+" "+doubleclick.ToString()+" "+toggle_display_on_double_click.ToString());
			Debug.Log ("mouseClick "+clikCount+" " +Input.GetMouseButtonDown (1).ToString());
            
            if (!item.HasChildItems())
            {
                

                //could do it for the parent as well ?
                //SceneManager.Instance.SetSelectedElement(itemid);
                //Debug.Log ("update label with "+item.Header);
                //SceneManager.Instance.SelectedElement=-2;

                //_sel.UpdateDescription(item.Header);

                for (int i=0;i <SceneManager.Instance.ProteinInstanceInfos.Count;i++)
                {
					SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 3, SceneManager.Instance.ProteinInstanceInfos[i].z); 
				}

                //for (int i=0;i <SceneManager.Instance.CurveControlPointsInfos.Count;i++){
                //	SceneManager.Instance.CurveControlPointsInfos[i] = new Vector4(SceneManager.Instance.CurveControlPointsInfos[i].x, SceneManager.Instance.CurveControlPointsInfos[i].y,3, 0); 
                //}
                currentSelectedIngredient = SceneManager.Instance.ProteinNames.IndexOf(item.Parent.Header + "_" + item.Header);
				if (item.Parent.Header.Contains ("membrane"))
                {
                    currentSelectedIngredient = SceneManager.Instance.ProteinNames.IndexOf (item.Header);
				}

				if (currentSelectedIngredient == -1)
                {
                    currentSelectedIngredient = SceneManager.Instance.CurveIngredientsNames.IndexOf (item.Parent.Header + "_" + item.Header);
					if (currentSelectedIngredient == -1)
						return;
					else {
						highlightCurveID(currentSelectedIngredient);
					}
				}else
                {
					highlightProteinID (currentSelectedIngredient);
				}

                

                //all ingredient instance should have state put highlighted 
                //for (int i=0;i <SceneManager.Instance.ProteinInstanceInfos.Count;i++){
                //	if (SceneManager.Instance.ProteinInstanceInfos[i].x == ingredientId){
                //		SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 1, SceneManager.Instance.ProteinInstanceInfos[i].z); 
                //	}else {
                //		SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 3, SceneManager.Instance.ProteinInstanceInfos[i].z); 
                //	}
                //}/

                if ((doubleclick))
                {
					Debug.Log ("double click");
					bool filter_cut_old = filter_cut;
					filter_cut=false;
					ApplyFunctionRecValue (toggleChecked, m_myTreeView.RootItem, toggle_display_on_double_click);
					ApplyFunctionRecValue (toggleChecked, item, true);
					toggle_display_on_double_click = !toggle_display_on_double_click;
					update = true;
					filter_cut = filter_cut_old;
					doubleclick=false;
				}

				GPUBuffer.Instance.ProteinInstanceInfos.SetData(SceneManager.Instance.ProteinInstanceInfos.ToArray());
				GPUBuffer.Instance.CurveControlPointsInfos.SetData(SceneManager.Instance.CurveControlPointsInfos.ToArray());
				
				//update camera position to center
				//Helper.FocusCameraOnGameObject(Camera.main,Vector4.zero,5.0f/PersistantSettings.Instance.Scale);
			}
			else {
				//item.Header is a compartement, except if its root highligh all child.
				GameObject root = GameObject.Find (SceneManager.Instance.scene_name);
				//grab the gameobbject
				GameObject sel = GameObject.Find (item.Header);

				Debug.Log("selected gmeobject "+sel.name+" "+item.Header);
				if (sel != null && sel != root) {
					for (int i=0;i <SceneManager.Instance.ProteinInstanceInfos.Count;i++){
						SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 3, SceneManager.Instance.ProteinInstanceInfos[i].z); 
					}
					//for (int i=0;i <SceneManager.Instance.CurveControlPointsInfos.Count;i++){
					//	SceneManager.Instance.CurveControlPointsInfos[i] = new Vector4(SceneManager.Instance.CurveControlPointsInfos[i].x, SceneManager.Instance.CurveIngredientsInfos[i].y,3, 0); 
					//}

					highlightHierarchy(sel);

					if ((doubleclick)) {
						Debug.Log ("double click");
						bool filter_cut_old = filter_cut;
						filter_cut=false;
						ApplyFunctionRecValue (toggleChecked, m_myTreeView.RootItem, toggle_display_on_double_click);
						ApplyFunctionRecValue (toggleChecked, item, true);
						toggle_display_on_double_click = !toggle_display_on_double_click;
						update = true;
						filter_cut = filter_cut_old;
						doubleclick=false;
					}
					GPUBuffer.Instance.ProteinInstanceInfos.SetData(SceneManager.Instance.ProteinInstanceInfos.ToArray());
					GPUBuffer.Instance.CurveControlPointsInfos.SetData(SceneManager.Instance.CurveControlPointsInfos.ToArray());
				}

				//SceneManager.Instance.SelectedElement=-1;
				//for (int i=0;i <SceneManager.Instance.ProteinInstanceInfos.Count;i++){
				//	SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 0, SceneManager.Instance.ProteinInstanceInfos[i].z); 
				//}
				//ComputeBufferManager.Instance.ProteinInstanceInfos.SetData(SceneManager.Instance.ProteinInstanceInfos.ToArray());
			}
		}
		//if (args.GetType ().Name == "UnselectedEventArgs") {
			//toggle off
		//	SceneManager.Instance.SetSelectedElement(-1);
		//}
		//if selected should show the description ?
		if (update )
			SceneManager.Instance.UploadIngredientToggleData();
    }

    void AddHandlerEvent(out System.EventHandler handler)
    {
		//if (!filter_cut) handler = new System.EventHandler(Handler);
		//else handler = new System.EventHandler(HandlerFilterCut);
		handler = new System.EventHandler(Handler);
    }

	void AddEvents(TreeViewItem item,params object[] argsRest)
    {
        AddHandlerEvent(out item.Click);
        AddHandlerEvent(out item.Checked);
        AddHandlerEvent(out item.Unchecked);
        AddHandlerEvent(out item.Selected);
        AddHandlerEvent(out item.Unselected);
    }

	//calculate MaxSize Width ?
	//CalcSize(GUIContent(label));
	public void addIngredientsItemJson(JSONNode recipeData,TreeViewItem parent){
		for (int j = 0; j < recipeData["ingredients"].Count; j++)
		{
			TreeViewItem jitem =  parent.AddItem(recipeData["ingredients"][j]["name"],true,true);
			AddEvents(jitem);
			jitem.anid=anid;
			anid+=1;
		}
	}

	public void ClearTree(){
		m_myTreeView.Items.Clear ();
		m_myTreeView.SelectedItem = null;
		m_myTreeView.HoverItem = null;
		m_myTreeView.Header = "";
	}

	public TreeViewItem getItemInChild(TreeViewItem item, string name){
		TreeViewItem itemfound = null;
		if (item.HasChildItems ()) {
			foreach(TreeViewItem i in item.Items){
				itemfound = getItemInChild(i,name);
				if (itemfound!=null)
					return itemfound;
			}
		}
		if (string.Equals (item.Header, name)) {
			return item;
		}
		return itemfound;
	}


	public TreeViewItem getItemFromName(string name){
		TreeViewItem itemfound = null;
		foreach (TreeViewItem item in m_myTreeView.Items) {
			itemfound = getItemInChild(item,name);
			if (itemfound!=null) return itemfound;
		}
		return itemfound;
	}

	public void populateRecipeJson(JSONNode recipeData){
		ClearTree ();
		anid = 0;
		var item = m_myTreeView;
		item.Width = 250;
		item.Height = 500;
		item.Header = recipeData["recipe"]["name"];
		AddEvents(item.RootItem);
		//int anid = 0;
		if (recipeData ["cytoplasme"] != null) {
			TreeViewItem item1 = item.RootItem.AddItem("cytoplasme",true,true);
			AddEvents(item1);
			addIngredientsItemJson(recipeData["cytoplasme"],item1);
		}
		for (int i = 0; i < recipeData["compartments"].Count; i++)
		{
			TreeViewItem comp = item.RootItem.AddItem(recipeData["compartments"].GetKey(i),true,true);
			AddEvents(comp);
			if (recipeData["compartments"][i] ["interior"] != null) {
				TreeViewItem interior = comp.AddItem("interior"+ i.ToString(),true,true);
				AddEvents(interior);
				addIngredientsItemJson(recipeData["compartments"][i] ["interior"],interior);
			}
			if (recipeData["compartments"][i] ["surface"] != null) {
				TreeViewItem surface = comp.AddItem("surface"+ i.ToString(),true,true);
				AddEvents(surface);
				addIngredientsItemJson(recipeData["compartments"][i] ["surface"],surface);
			}
		}
	}

	public void addIngredientsItemGameObject(Transform recipeData,TreeViewItem parent){
		for (int j = 0; j < recipeData.childCount; j++)
		{
			TreeViewItem jitem =  parent.AddItem(recipeData.GetChild(j).name,true,true);
			AddEvents(jitem);
			jitem.anid=anid;
			anid+=1;
		}
	}
	
	public void populateRecipeGameObject(GameObject hierachy){
		ClearTree ();
		anid = 0;
		var item = m_myTreeView;
		item.Width = 250;
		item.Height = 500;
		Debug.Log ("setting tree with "+hierachy.name);
		item.Header = hierachy.name;
		AddEvents(item.RootItem);
		//int anid = 0;
		int i = 0;
		foreach (Transform child in hierachy.transform) {
			if (string.Equals(child.name,"cytoplasme")){
				TreeViewItem item1 = item.RootItem.AddItem("cytoplasme",true,true);
				AddEvents(item1);
				addIngredientsItemGameObject(child,item1);
			}
			else if (child.name.Contains("membrane")){
				TreeViewItem comp = item.RootItem.AddItem(child.name,false,true);
				AddEvents(comp);
				if (child.childCount!= 0){
					addIngredientsItemGameObject(child,comp);
				}
			}
			else {
				//should have two child
				TreeViewItem comp = item.RootItem.AddItem(child.name,true,true);
				AddEvents(comp);
				if (child.childCount!= 0){
					if (child.GetChild(1).childCount!= 0) {
						TreeViewItem interior = comp.AddItem(child.GetChild(1).name,true,true);
						AddEvents(interior);
						addIngredientsItemGameObject(child.GetChild(1),interior);
					}
					if (child.GetChild(0).childCount != 0) {
						TreeViewItem surface = comp.AddItem(child.GetChild(0).name,true,true);
						AddEvents(surface);
						addIngredientsItemGameObject(child.GetChild(0),surface);
					}
				}
				i+=1;
			}
			
		}
	}
}
