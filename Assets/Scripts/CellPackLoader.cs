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
   
    public static void LoadCellPackResults()
    {
        #if UNITY_EDITOR

            var directory = "";
			var path = "";

            if (string.IsNullOrEmpty(PersistantSettings.Instance.LastSceneLoaded) || !Directory.Exists(Path.GetDirectoryName(PersistantSettings.Instance.LastSceneLoaded)))
            {
                directory = Application.dataPath;
            }
            else
            {
                directory = Path.GetDirectoryName(PersistantSettings.Instance.LastSceneLoaded);
            }
			if (SceneManager.Instance.sceneid==SceneManager.Instance.AllRecipes.Count){
            	path = EditorUtility.OpenFilePanel("Select .cpr", directory, "cpr");
            	if (string.IsNullOrEmpty(path)) return;
			}
			else {
				string url = SceneManager.Instance.AllRecipes[SceneManager.Instance.sceneid][0]["resultfile"];
				url=url.Replace("autoPACKserver","https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/");
				//fetch the results file from the server
				path = Helper.GetResultsFile(url);
			}
            PersistantSettings.Instance.LastSceneLoaded = path;
            LoadIngredients(path);

            // Upload scene data to the GPU
            SceneManager.Instance.UploadAllData();
			Debug.Log("*****");
			Debug.Log("Total protein atoms number: " + SceneManager.Instance.TotalNumProteinAtoms);
		#endif
	}
	
	public static void LoadIngredients(string recipePath)
    {
        Debug.Log("*****");
        Debug.Log("Loading scene: " + recipePath);
        
        var cellPackSceneJsonPath = recipePath;//Application.dataPath + "/../Data/HIV/cellPACK/BloodHIV1.0_mixed_fixed_nc1.json";
        if (!File.Exists(cellPackSceneJsonPath)) throw new Exception("No file found at: " + cellPackSceneJsonPath);
		//this assume a result file from cellpack, not a recipe file.
        var resultData = Helper.ParseJson(cellPackSceneJsonPath);

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

		if ((pdbName == "") || (pdbName == "null") || (pdbName == "None")) {
			//check for sphere file//information in the file. if not in file is it on disk ? on repo ?
			//possibly read the actuall recipe definition ?
			if (ingredientDictionary ["radii"] != null) {
				atomSpheres = Helper.gatherSphereTree(ingredientDictionary)[0];
				Debug.Log ("nbprim "+atomSpheres.Count.ToString());//one sphere
			} else {
				return;
			}
		} else {
			if (pdbName.StartsWith("EMDB")) return;
			if (pdbName.Contains("1PI7_1vpu_biounit")) return;//??
			// Load atom set from pdb file
			var atomSet = PdbLoader.LoadAtomSet(pdbName);
			
			// If the set is empty return
			if (atomSet.Count == 0) return;
		
			atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
			containsACarbonOnly = AtomHelper.ContainsACarbonOnly(atomSet);
			biomtTransforms = biomt ? PdbLoader.LoadBiomtTransforms(pdbName) : new List<Matrix4x4>();
			biomtCenter = AtomHelper.GetBiomtCenter(biomtTransforms);
		}
		
		//if (!pdbName.Contains("1TWT_1TWV")) return;
        
        // Disable biomts until loading problem is resolved
        if (biomt) return;
        

        var centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;       

        // Center atoms
        AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);

        // Compute bounds
        var bounds = AtomHelper.ComputeBounds(atomSpheres);

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
            : new List<float>() {0.10f, 0.05f, 0.01f};
        
        // Add ingredient type
        //SceneManager.Instance.AddIngredient(name, bounds, atomSpheres, color);
        SceneManager.Instance.AddIngredient(name, bounds, atomSpheres, color, clusterLevels);
        
        int instanceCount = 0;
        
        for (int k = 0; k < ingredientDictionary["results"].Count; k++)
        {
            var p = ingredientDictionary["results"][k][0];
            var r = ingredientDictionary["results"][k][1];

            var position = new Vector3(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat);
            var rotation = new Quaternion(r[0].AsFloat, r[1].AsFloat, r[2].AsFloat, r[3].AsFloat);

            var mat = Helper.quaternion_matrix(rotation);
            var euler = Helper.euler_from_matrix(mat);
            rotation = Helper.MayaRotationToUnity(euler);

            if (!biomt)
            {
                // Find centered position
                if (!center) position += Helper.QuaternionTransform(rotation, centerPosition);
                SceneManager.Instance.AddIngredientInstance(name, position, rotation);
                instanceCount++;
            }
            else
            {
                foreach (var transform in biomtTransforms)
                {
                    var biomtOffset = Helper.RotationMatrixToQuaternion(transform) * centerPosition;
                    var biomtInstanceRot = rotation * Helper.RotationMatrixToQuaternion(transform);
                    var biomtInstancePos = rotation * (new Vector3(transform.m03, transform.m13, transform.m23) + biomtOffset) + position - biomtCenter;

                    SceneManager.Instance.AddIngredientInstance(name, biomtInstancePos, biomtInstanceRot);
                    instanceCount++;
                }
            }
        }

        Debug.Log("*****");
        Debug.Log("Added ingredient: " + name);
        if (containsACarbonOnly) Debug.Log("Alpha-carbons only");
        Debug.Log("Pdb name: " + pdbName + " *** " + "Num atoms: " + atomSpheres.Count + " *** " + "Num instances: " + instanceCount + " *** " + "Total atom count: " + atomSpheres.Count * instanceCount);
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

	public static void LoadLipidsTest(){
		//use Library compute from RMSD
		//read atomic info
		
		Dictionary<string,List<List<float>>> tri_res = new Dictionary<string,List<List<float>>> ();
		
		var Lib = Helper.ReadBytesAsFloats(Application.dataPath + "/../Data/membrane/library_myco.bin");
		int step = 5;//resid,x,y,z,atomtype	
		int count = 0;
		List<Vector4> atomSpheres=new List<Vector4>();
		int previd = 0;
		Bounds bounds;
		List<List<Vector4>> atomClusters;
		Color ingrColor;
		Vector3 centerPosition;
		var clusterLevels = new List<float>() {0.80f, 0.55f, 0.21f};
		for (var i = 0; i < Lib.Length; i += step) {
			var currentAtom = new Vector4 (Lib [i + 1], Lib [i + 2], Lib [i + 3], AtomHelper.AtomRadii [(int)Lib [i + 4]]);
			var resid = (int)Lib [i];
			//Debug.Log (resid.ToString()+" "+previd.ToString()+" "+atomSpheres.Count.ToString());
			if (previd != resid) {		
				centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;
				atomClusters = new List<List<Vector4>> ();
				// Center atoms
				AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);
				// Compute bounds
				bounds = AtomHelper.ComputeBounds(atomSpheres);
				//PdbLoader.OffsetPoints (ref atomSpheres, bounds.center);//center
				//List<Vector4> atomCl1 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/2 , 1.0f);
				//List<Vector4> atomCl2 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/4 , 1.0f);
				//List<Vector4> atomCl3 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/8 , 1.0f);
				//atomClusters.Add (atomCl1);
				//atomClusters.Add (atomCl2);
				//atomClusters.Add (atomCl3);
				ingrColor = new Color (0, 1, 0);
				// Define cluster decimation levels
				SceneManager.Instance.AddIngredient ("lipids" + previd.ToString (), bounds, atomSpheres, ingrColor, clusterLevels);
				atomSpheres.Clear ();
				//Debug.Log ("added lipids" + previd.ToString ());
			}
			atomSpheres.Add (currentAtom);
			previd=resid;
		}
		//add the last one
		centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;//PdbLoader.GetBounds (atomSpheres);
		//atomClusters = new List<List<Vector4>> ();
		// Center atoms
		AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);
		// Compute bounds
		bounds = AtomHelper.ComputeBounds(atomSpheres);

		//bounds = PdbLoader.GetBounds (atomSpheres);
		//atomClusters = new List<List<Vector4>> ();
		//check the cluster radius, seems too large
		//List<Vector4> atomClustersL1 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/2 , 1.0f);
		//List<Vector4> atomClustersL2 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/4 , 1.0f);
		//List<Vector4> atomClustersL3 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/8 , 1.0f);
		//PdbLoader.OffsetPoints (ref atomSpheres, bounds.center);//center
		//PdbLoader.OffsetPoints(ref atomClustersL1, bounds.center);
		//PdbLoader.OffsetPoints(ref atomClustersL2, bounds.center);
		//PdbLoader.OffsetPoints(ref atomClustersL3, bounds.center);
		//
		//atomClusters.Add (atomClustersL1);
		//atomClusters.Add (atomClustersL2);
		//atomClusters.Add (atomClustersL3);
		ingrColor = new Color (0, 1, 0);
		SceneManager.Instance.AddIngredient ("lipids" + previd.ToString (), bounds, atomSpheres, ingrColor, clusterLevels);
		atomSpheres.Clear ();
		Debug.Log ("added lipids" + previd.ToString ());
		
		var Lipids = Helper.ReadBytesAsFloats(Application.dataPath + "/../Data/membrane/lipid_pos_myco.bin");
		step=8;//rid,pos,rot,
		Debug.Log ("total lipids " + (Lipids.Length / step).ToString ());
		for (var i = 0; i < Lipids.Length; i += step) {
			var position = new Vector3(-Lipids[i+1], Lipids[i+2],Lipids[i+3]);
			var rotation = new Quaternion(Lipids[i+4], Lipids[i+5], Lipids[i+6], Lipids[i+7]);			
			//Debug.Log (rotation.ToString ());
			var mat = Helper.quaternion_matrix(rotation);//.transpose;
			var euler = Helper.euler_from_matrix(mat);
			//Debug.Log (position.ToString()+ " " +euler.ToString ());
			rotation = Helper.MayaRotationToUnity(euler);//-Y-Z
			var resid = (int)Lipids [i];
			SceneManager.Instance.AddIngredientInstance("lipids" + resid.ToString (), position, rotation);
		}
	}



}
