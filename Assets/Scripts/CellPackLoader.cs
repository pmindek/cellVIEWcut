using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public static class CellPackLoader
{
	public static int current_color;
	public static List<Vector3> ColorsPalette;
	public static List<Vector3> ColorsPalette2;
	public static Dictionary<int,List<int>> usedColors;
	private static bool use_rigid_body = true;

	public static void AddRecipeIngredientsGameObject(JSONNode recipeData,GameObject parent){
		for (int j = 0; j < recipeData["ingredients"].Count; j++)
		{
			string iname = recipeData["ingredients"][j]["name"];
			/*if (iname.StartsWith("HIV")) 
				if (iname.Contains("NC")){
					iname = "HIV_"+iname.Split('_')[1]+"_"+iname.Split('_')[2];
				}
				else if (iname.Contains("P6_VPR"))
					iname = "HIV_"+iname.Split('_')[1]+"_"+iname.Split('_')[2];
				else 
					iname = "HIV_"+iname.Split('_')[1];
			*/
			if (!SceneManager.Instance.ProteinNames.Contains(parent.name+"_"+iname))
			{
				Debug.Log (parent.name+"_"+iname);
				continue;
			}
			Debug.Log ("create "+iname);
			var jitem = new GameObject(iname);
			jitem.transform.parent=parent.transform;
			if ((recipeData["ingredients"][j]["radii"] != null )&&(use_rigid_body)){
				//build child as rigid body
				var atomSpheres = MyUtility.gatherSphereTree(recipeData["ingredients"][j])[0];
				//build the prefab
				GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);// new GameObject(iname+"_0");
				//prefab.transform.localScale = new Vector3()
				//add sphere collision compoinent
				Rigidbody rb = prefab.AddComponent<Rigidbody>();
				rb.useGravity = false;
				rb.velocity = Vector3.zero;
				rb.drag = 100;
				rb.angularDrag =100;
				foreach (Vector4 sph in atomSpheres) {
					SphereCollider sc = prefab.AddComponent<SphereCollider>();
					sc.radius = sph.w;
					sc.center = new Vector3(sph.x,sph.y,sph.z);
					//sc.attachedRigidbody = rb;
				}
				GameObject rbroot = GameObject.Find (SceneManager.Instance.scene_name);
				prefab.transform.parent = rbroot.transform;
				prefab.hideFlags = HideFlags.HideInHierarchy;
				//add rigid body component
				//instantiate
				for (int k = 0; k < recipeData["ingredients"][j]["results"].Count; k++)
				{
					var p = recipeData["ingredients"][j]["results"][k][0];
					var r = recipeData["ingredients"][j]["results"][k][1];
					
					var position = new Vector3(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat);
					var rotation = new Quaternion(r[0].AsFloat, r[1].AsFloat, r[2].AsFloat, r[3].AsFloat);
					
					var mat = MyUtility.quaternion_matrix(rotation);
					var euler = MyUtility.euler_from_matrix(mat);
					rotation = MyUtility.MayaRotationToUnity(euler);
					//instantiate
					if (k==0){
						prefab.transform.position = position;
						prefab.transform.rotation = rotation;
					}
					else {
						GameObject inst = GameObject.Instantiate(prefab, position, rotation) as GameObject;
						inst.transform.parent = rbroot.transform;
						inst.hideFlags = HideFlags.HideInHierarchy;
						inst.name = iname+"_"+k.ToString();
					}
					//SceneManager.Instance.AddIngredientInstance(name, position, rotation);
				}
			}
			//add children invisible instance with collider and rigidBody with no gravity
			//collider should be the primitive from cellPACK
		}
	}
	
	public static void buildHierarchy(JSONNode resultData){
		SceneManager.Instance.scene_name = resultData ["recipe"] ["name"];
		var root = new GameObject(resultData["recipe"]["name"]);//in case we want to have more than one recipe loaded
		//create empty null object or sphere ?
		if (use_rigid_body) {
			GameObject rb_root = new GameObject (resultData ["recipe"] ["name"] + "_rigidbody");
		}
		if (resultData["cytoplasme"] != null)
		{
			var cyto = new GameObject("cytoplasme");
			cyto.transform.parent = root.transform;
			AddRecipeIngredientsGameObject(resultData["cytoplasme"], cyto);
		}
		
		for (int i = 0; i < resultData["compartments"].Count; i++)
		{
			var comp = new GameObject(resultData["compartments"].GetKey(i));
			comp.transform.parent = root.transform;
			var surface = new GameObject("surface"+ i.ToString());
			surface.transform.parent = comp.transform;
			AddRecipeIngredientsGameObject(resultData["compartments"][i]["surface"],surface);
			var interior = new GameObject("interior"+ i.ToString());
			interior.transform.parent = comp.transform;
			AddRecipeIngredientsGameObject(resultData["compartments"][i]["interior"], interior);
		}
	}
    public static void LoadCellPackResults(bool load=true)
    {
            #if UNITY_EDITOR
			Debug.Log("Loading");
            var directory = "";

            if (string.IsNullOrEmpty(PersistantSettings.Instance.LastSceneLoaded) || !Directory.Exists(Path.GetDirectoryName(PersistantSettings.Instance.LastSceneLoaded)))
            {
                directory = Application.dataPath;
            }
            else
            {
                directory = Path.GetDirectoryName(PersistantSettings.Instance.LastSceneLoaded);
            }

            var path = EditorUtility.OpenFilePanel("Select .cpr", directory, "cpr");
            if (string.IsNullOrEmpty(path)) return;
        
            PersistantSettings.Instance.LastSceneLoaded = path;
            LoadIngredients(path);

            Debug.Log("*****");
            Debug.Log("Total protein atoms number: " + SceneManager.Instance.TotalNumProteinAtoms);

            // Upload scene data to the GPU
            SceneManager.Instance.UploadAllData();

            // Send new protein cut filters to cut object
            SceneManager.Instance.SetCutObjects();

        #endif
    }

    public static void LoadIngredients(string recipePath)
    {
        Debug.Log("*****");
        Debug.Log("Loading scene: " + recipePath);
        
        var cellPackSceneJsonPath = recipePath;//Application.dataPath + "/../Data/HIV/cellPACK/BloodHIV1.0_mixed_fixed_nc1.json";
        if (!File.Exists(cellPackSceneJsonPath)) throw new Exception("No file found at: " + cellPackSceneJsonPath);

        var resultData = MyUtility.ParseJson(cellPackSceneJsonPath);

        //we can traverse the json dictionary and gather ingredient source (PDB,center), sphereTree, instance.geometry if we want.
        //the recipe is optional as it will gave more information than just the result file.

        //idea: use secondary color scheme for compartments, and analogous color for ingredient from the recipe baseColor
        current_color = 0;
        //first grab the total number of object
        int nIngredients = 0;
        if (resultData["cytoplasme"] != null)
            nIngredients += resultData["cytoplasme"]["ingredients"].Count;
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            nIngredients += resultData["compartments"][i]["interior"]["ingredients"].Count;
            nIngredients += resultData["compartments"][i]["surface"]["ingredients"].Count;
        }
        //generate the palette
        //ColorsPalette   = ColorGenerator.Generate(nIngredients).Skip(2).ToList(); 
        ColorsPalette = ColorGenerator.Generate(8).Skip(2).ToList();//.Skip(2).ToList();
        List<Vector3> startKmeans = new List<Vector3>(ColorsPalette);
        //paletteGenerator.initKmeans (startKmeans);

        usedColors = new Dictionary<int, List<int>>();
        ColorsPalette2 = ColorPaletteGenerator.generate(
                6, // Colors
                ColorPaletteGenerator.testfunction,
                false, // Using Force Vector instead of k-Means
                50 // Steps (quality)
                );
        // Sort colors by differenciation first
        //ColorsPalette2 = paletteGenerator.diffSort(ColorsPalette2);
        //check if cytoplasme present
        Color baseColor = new Color(1.0f, 107.0f / 255.0f, 66.0f / 255.0f);
        if (resultData["cytoplasme"] != null)
        {
            usedColors.Add(current_color, new List<int>());
            baseColor = new Color(1.0f, 107.0f / 255.0f, 66.0f / 255.0f);
            AddRecipeIngredients(resultData["cytoplasme"]["ingredients"], baseColor, "cytoplasme");
            current_color += 1;
        }

        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            baseColor = new Color(148.0f / 255.0f, 66.0f / 255.0f, 255.0f / 255.0f);
            usedColors.Add(current_color, new List<int>());
            AddRecipeIngredients(resultData["compartments"][i]["interior"]["ingredients"], baseColor, "interior" + i.ToString());
            current_color += 1;
            baseColor = new Color(173.0f / 255.0f, 255.0f / 255.0f, 66.0f / 255.0f);
            usedColors.Add(current_color, new List<int>());
            AddRecipeIngredients(resultData["compartments"][i]["surface"]["ingredients"], baseColor, "surface" + i.ToString());
            current_color += 1;
        }
		buildHierarchy (resultData);
    }

	public static void AddRecipeIngredients(JSONNode recipeDictionary, Color baseColor, string prefix)
    {
		for (int j = 0; j < recipeDictionary.Count; j++)
		{
            if (recipeDictionary[j]["nbCurve"] != null)
            {
                AddCurveIngredients(recipeDictionary[j], prefix);
            }
            else
            {
                AddProteinIngredient(recipeDictionary[j], prefix);
            }
        }
	}

    public static void AddProteinIngredient(JSONNode ingredientDictionary, string prefix)
    {
        var name = prefix + "_" + ingredientDictionary["name"];
        var biomt = (bool)ingredientDictionary["source"]["biomt"].AsBool;
        var center = (bool)ingredientDictionary["source"]["transform"]["center"].AsBool;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");
		List<Vector4> atomSpheres;
		List<Matrix4x4> biomtTransforms = new List<Matrix4x4>();
		Vector3 biomtCenter = Vector3.zero;
		bool containsACarbonOnly = false;
		bool oneLOD = false;

        if ((pdbName == "") || (pdbName == "null") || (pdbName == "None")||pdbName.StartsWith("EMDB"))
        {
            return;

            ////check for sphere file//information in the file. if not in file is it on disk ? on repo ?
            ////possibly read the actuall recipe definition ?
            ////check if bin exist
            //var filePath = PdbLoader.DefaultPdbDirectory + ingredientDictionary["name"] + ".bin";
            //if (File.Exists(filePath)){
            //	atomSpheres = new List<Vector4>();
            //	var points = MyUtility.ReadBytesAsFloats(filePath);
            //	for (var i = 0; i < points.Length; i += 4) {
            //		var currentAtom = new Vector4 (points [i], points [i + 1], points [i + 2], points [i + 3]);
            //		atomSpheres.Add (currentAtom);
            //	}
            //	containsACarbonOnly = true;
            //	oneLOD = true;
            //}
            //else if (ingredientDictionary ["radii"] != null) {
            //	atomSpheres = MyUtility.gatherSphereTree(ingredientDictionary)[0];
            //	Debug.Log ("nbprim "+atomSpheres.Count.ToString());//one sphere
            //	oneLOD = true;
            //} else {
            //	float radius = 30.0f;
            //	if (name.Contains("dLDL"))
            //		radius = 108.08f;//or use the mesh? or make sphere from the mesh ?
            //	if (name.Contains("iLDL"))
            //		radius = 105.41f;//or use the mesh? or make sphere from the mesh ?
            //	atomSpheres = new List<Vector4>();
            //	atomSpheres.Add (new Vector4(0,0,0,radius));
            //	//No LOD since only one sphere
            //	oneLOD = true;
            //}
        }
        else
        {
			//if (pdbName.StartsWith("EMDB")) return;
			//if (pdbName.Contains("1PI7_1vpu_biounit")) return;//??
			// Load atom set from pdb file
			var atomSet = PdbLoader.LoadAtomSet(pdbName);
			
			// If the set is empty return
			if (atomSet.Count == 0) return;
		
			atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
			containsACarbonOnly = AtomHelper.ContainsCarbonAlphaOnly(atomSet);
		}
		var centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;       
		
		// Center atoms
		AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);
		
		// Compute bounds
		var bounds = AtomHelper.ComputeBounds(atomSpheres);

		biomtTransforms = biomt ? PdbLoader.LoadBiomtTransforms(pdbName) : new List<Matrix4x4>();
		biomtCenter = AtomHelper.GetBiomtCenter(biomtTransforms,centerPosition);

		//if (!pdbName.Contains("1TWT_1TWV")) return;
        
        // Disable biomts until loading problem is resolved
        //if (!biomt) return;
        

        // Get ingredient color
        // TODO: Move color palette code into dedicated function
        var cid = ColorPaletteGenerator.GetRandomUniqFromSample(current_color, usedColors[current_color]);
        usedColors[current_color].Add(cid);
        var sample = ColorPaletteGenerator.colorSamples[cid];
        var c = ColorPaletteGenerator.lab2rgb(sample) / 255.0f;
        var color = new Color(c[0], c[1], c[2]);

        // Define cluster decimation levels
		var clusterLevels = (containsACarbonOnly)
		? new List<float>() {0.85f, 0.25f, 0.1f}
            : new List<float>() {0.15f, 0.10f, 0.05f};
		if (oneLOD)
			clusterLevels = new List<float> () {1, 1, 1};
        // Add ingredient type
        //SceneManager.Instance.AddIngredient(name, bounds, atomSpheres, color);
	
		SceneManager.Instance.AddIngredient(name, bounds, atomSpheres, color, clusterLevels,oneLOD);
        int instanceCount = 0;
        
        for (int k = 0; k < ingredientDictionary["results"].Count; k++)
        {
            var p = ingredientDictionary["results"][k][0];
            var r = ingredientDictionary["results"][k][1];

            var position = new Vector3(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat);
            var rotation = new Quaternion(r[0].AsFloat, r[1].AsFloat, r[2].AsFloat, r[3].AsFloat);

            var mat = MyUtility.quaternion_matrix(rotation);
            var euler = MyUtility.euler_from_matrix(mat);
            rotation = MyUtility.MayaRotationToUnity(euler);

            if (!biomt)
            {
                // Find centered position
                if (!center) position += MyUtility.QuaternionTransform(rotation, centerPosition);
                SceneManager.Instance.AddIngredientInstance(name, position, rotation);
                instanceCount++;
            }
            else
            {
                foreach (var transform in biomtTransforms)
                {
					var biomteuler = MyUtility.euler_from_matrix(transform);
					var rotBiomt = MyUtility.MayaRotationToUnity(biomteuler);
					var offset = MyUtility.QuaternionTransform(rotBiomt,centerPosition);//Helper.RotationMatrixToQuaternion(matBiomt), GetCenter());
					var posBiomt = new Vector3(-transform.m03, transform.m13, transform.m23)+offset - biomtCenter;

					var biomtOffset = MyUtility.RotationMatrixToQuaternion(transform) * centerPosition;
					var biomtInstanceRot = rotation * rotBiomt;//Helper.RotationMatrixToQuaternion(transform);
					var biomtInstancePos = rotation * posBiomt + position;

					SceneManager.Instance.AddIngredientInstance(name, biomtInstancePos, biomtInstanceRot);
                    instanceCount++;
                }
            }
        }

        Debug.Log("*****");
        Debug.Log("Added ingredient: " + name);
        //if (isCarbonAlphaOnly) Debug.Log("Alpha-carbons only");
        //Debug.Log("Pdb name: " + pdbName + " *** " + "Num atoms: " + atomSet.Count + " *** " + "Num instances: " + instanceCount + " *** " + "Total atom count: " + atomSet.Count * instanceCount);
    }

    public static void AddCurveIngredients(JSONNode ingredientDictionary, string prefix)
    {
        //in case there is curveN, grab the data if more than 4 points
        //use the given PDB for the representation.
        var numCurves = ingredientDictionary["nbCurve"].AsInt;
        var curveIngredientName = prefix + "_" + ingredientDictionary["name"].Value;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");

        SceneManager.Instance.AddCurveIngredient(curveIngredientName, pdbName);
        
        for (int i = 0; i < numCurves; i++)
        {
            //if (i < nCurve-10) continue;
            var controlPoints = new List<Vector4>();
            if (ingredientDictionary["curve" + i.ToString()].Count < 4) continue;

            for (int k = 0; k < ingredientDictionary["curve" + i.ToString()].Count; k++)
            {
                var p = ingredientDictionary["curve" + i.ToString()][k];
                controlPoints.Add(new Vector4(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat, 1));
            }

            SceneManager.Instance.AddCurve(curveIngredientName, controlPoints);
            //break;
        }

        Debug.Log("*****");
        Debug.Log("Added curve ingredient: " + curveIngredientName);
        Debug.Log("Num curves: " + numCurves);
    }
	
    public static void DebugMethod()
    {
        Debug.Log("Hello World");
    }
}
