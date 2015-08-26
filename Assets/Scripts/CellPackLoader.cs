using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

public static class CellPackLoader
{
	public static int current_color;
	public static List<Vector3> ColorsPalette;
	public static List<Vector3> ColorsPalette2;
	public static Dictionary<int,List<int>> usedColors;
   
    public static void LoadCellPackResults()
    {
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
    }

    public static void LoadIngredients(string recipePath)
    {
        Debug.Log("*****");
        Debug.Log("Loading scene: " + recipePath);
        
        var cellPackSceneJsonPath = recipePath;//Application.dataPath + "/../Data/HIV/cellPACK/BloodHIV1.0_mixed_fixed_nc1.json";
        if (!File.Exists(cellPackSceneJsonPath)) throw new Exception("No file found at: " + cellPackSceneJsonPath);

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
        
        if (pdbName == "") return;
        if (pdbName == "null") return;
        if (pdbName == "None") return;
        if (pdbName.StartsWith("EMDB")) return;
        if (pdbName.Contains("1PI7_1vpu_biounit")) return;

        // Disable biomts until loading problem is resolved
        if (biomt) return;
        
        // Load atom set from pdb file
        var atomSet = PdbLoader.LoadAtomSet(pdbName);
        
        // If the set is empty return
        if (atomSet.Count == 0) return;

        var atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
        var centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;
        
        var biomtTransforms = biomt ? PdbLoader.LoadBiomtTransforms(pdbName) : new List<Matrix4x4>();
        var biomtCenter = AtomHelper.GetBiomtCenter(biomtTransforms);
        
        var containsACarbonOnly = AtomHelper.ContainsACarbonOnly(atomSet);

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
}
