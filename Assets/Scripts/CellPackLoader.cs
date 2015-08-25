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
	public static readonly string ProteinDiretory = Application.dataPath + "/../Data/proteins/";
   
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

        Debug.Log("Num protein atoms " + SceneManager.Instance.NumProteinAtoms);

        // Upload scene data to the GPU
        SceneManager.Instance.UploadAllData();
    }

    public static void LoadIngredients(string recipePath)
    {
        Debug.Log("Loading: " + recipePath);
        
        if (!Directory.Exists(ProteinDiretory)) throw new Exception("No directory found at: " + ProteinDiretory);

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
		//from the baseColor we take variation around analogous color
		//IEnumerator<Color> colorList = ColorGenerator.Generate (recipeDictionary.Count).Skip .GetEnumerator();
		// = ColorGenerator.Generate(recipeDictionary.Count+2).Skip(2).ToList(); 

		for (int j = 0; j < recipeDictionary.Count; j++)
		{
            var iname = prefix + "_"+ recipeDictionary[j]["name"];
            
            if (recipeDictionary[j].Count > 3)
            {
                AddCurveIngredients(recipeDictionary[j]);
            }
            else
            {
                //AddProteinIngredient(recipeDictionary[j]);
            }
			
			Debug.Log("Added: " + iname + " num instances: " + recipeDictionary[j]["results"].Count);
			Debug.Log("*****");
        }
	}

    public static void AddProteinIngredient(JSONNode ingredientDictionary)
    {
        var biomt = (bool)ingredientDictionary["source"]["biomt"].AsBool;
        var center = (bool)ingredientDictionary["source"]["transform"]["center"].AsBool;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");
        
        if (pdbName == "") return;
        if (pdbName == "null") return;
        if (pdbName == "None") return;
        if (pdbName.StartsWith("EMDB")) return;
        if (pdbName.Contains("1PI7_1vpu_biounit")) return;

        // Debug biomt
        //if (!biomt) continue;
        //if (!pdbName.Contains("2plv")) continue;
        //if (!pdbName.Contains("3j3q_1vu4")) continue;
        //if (!pdbName.Contains("3gau")) continue;

        var pdbPath = ProteinDiretory + pdbName + ".pdb";
        if (!File.Exists(pdbPath))
        {
            if (pdbName.Length == 4)
            {
                PdbLoader.DownloadPdbFile(pdbName, ProteinDiretory); // If the pdb file does not exist try to download it
            }
            else
            {
                PdbLoader.DownloadPdbFromRecipeFile(pdbName, ProteinDiretory);
            }
        }

        // Load all data from text files
        var atoms = PdbLoader.ReadAtomData(pdbPath);
        var atomClusters = new List<List<Vector4>>();
        var biomtTransforms = (biomt) ? PdbLoader.ReadBiomtData(pdbPath) : new List<Matrix4x4>();

        var atomSpheres = new List<Vector4>();
        var atomClustersL1 = new List<Vector4>();
        var atomClustersL2 = new List<Vector4>();
        var atomClustersL3 = new List<Vector4>();

        // Treat this protein separatly as it has only CA in the pdb
        if (PdbLoader.IsCarbonOnly(atoms))
        {
            return;
            //atomSpheres = PdbLoader.ClusterAtomsByResidue(atoms, 1, 3);
            //atomClustersL1 = PdbLoader.ClusterAtomsByResidue(atoms, 1, 4);
            //atomClustersL2 = PdbLoader.ClusterAtomsByChain(atoms, 3, 8);
            //atomClustersL3 = PdbLoader.ClusterAtomsByChain(atoms, 10, 10);
        }
        else
        {
            atomSpheres = PdbLoader.GetAtomSpheres(atoms);
            atomClustersL1.AddRange(KMeansClustering.GetClusters(atomSpheres, atomSpheres.Count * 15 / 100));
            atomClustersL2.AddRange(KMeansClustering.GetClusters(atomSpheres, atomSpheres.Count * 5 / 100));
            atomClustersL3.AddRange(KMeansClustering.GetClusters(atomSpheres, atomSpheres.Count * 3 / 100));
        }

        // use biomt as one single instance until I find  better solution
        if (biomt)
        {
            atomSpheres = PdbLoader.BuildBiomt(atomSpheres, biomtTransforms);
            atomClustersL1 = PdbLoader.BuildBiomt(atomClustersL1, biomtTransforms);
            atomClustersL2 = PdbLoader.BuildBiomt(atomClustersL2, biomtTransforms);
            atomClustersL3 = PdbLoader.BuildBiomt(atomClustersL3, biomtTransforms);
        }

        var bounds = PdbLoader.GetBounds(atomSpheres);
        PdbLoader.OffsetPoints(ref atomSpheres, bounds.center);
        PdbLoader.OffsetPoints(ref atomClustersL1, bounds.center);
        PdbLoader.OffsetPoints(ref atomClustersL2, bounds.center);
        PdbLoader.OffsetPoints(ref atomClustersL3, bounds.center);

        atomClusters.Add(atomClustersL1);
        atomClusters.Add(atomClustersL2);
        atomClusters.Add(atomClustersL3);

        // Add ingredient to scene manager
        //Color ingrColor = ColorsPalette[current_color];// colorList.Current;

        int cid = ColorPaletteGenerator.GetRandomUniqFromSample(current_color, usedColors[current_color]);
        usedColors[current_color].Add(cid);
        Vector3 sample = ColorPaletteGenerator.colorSamples[cid];
        //we could some weigthing
        //sample[0]*=2*((float)atoms.Count/8000f);//weigth per atoms.Count
        Vector3 c = ColorPaletteGenerator.lab2rgb(sample) / 255.0f;
        Color ingrColor = new Color(c[0], c[1], c[2]);
        //Debug.Log ("color "+current_color+" "+N+" "+ingrColor.ToString());
        //should try to pick most disctinct one ?
        //shouldnt use the pdbName for the name of the ingredient, but rather the actual name
        SceneManager.Instance.AddIngredient(pdbName, bounds, atomSpheres, ingrColor, atomClusters);
        //colorList.MoveNext();
        //current_color+=1;

        for (int k = 0; k < ingredientDictionary["results"].Count; k++)
        {
            var p = ingredientDictionary["results"][k][0];
            var r = ingredientDictionary["results"][k][1];

            var position = new Vector3(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat);
            var rotation = new Quaternion(r[0].AsFloat, r[1].AsFloat, r[2].AsFloat, r[3].AsFloat);

            var mat = Helper.quaternion_matrix(rotation);
            var euler = Helper.euler_from_matrix(mat);
            rotation = Helper.MayaRotationToUnity(euler);

            // Find centered position
            if (!center) position += Helper.QuaternionTransform(rotation, bounds.center);
            SceneManager.Instance.AddIngredientInstance(pdbName, position, rotation);

            //if (biomt)
            //{
            //    foreach (var matBiomt in biomtTransforms)
            //    {
            //        var rotBiomt = Helper.RotationMatrixToQuaternion(matBiomt);
            //        var posBiomt = new Vector3(matBiomt.m03, matBiomt.m13, matBiomt.m23) + position + bounds.center;

            //        SceneManager.Instance.AddIngredientInstance(pdbName, posBiomt, rotBiomt);
            //    }
            //}
            //else
            //{
            //     SceneManager.Instance.AddIngredientInstance(pdbName, position, rotation);
            //}
        }
    }

    public static void AddCurveIngredients(JSONNode ingredientDictionary)
    {
        //in case there is curveN, grab the data if more than 4 points
        //use the given PDB for the representation.
        var numCurve = ingredientDictionary["nbCurve"].AsInt;
        var curveIngredientName = ingredientDictionary["name"].Value;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");
        
        SceneManager.Instance.AddCurveIngredient(curveIngredientName, pdbName);
        
        for (int i = 0; i < numCurve; i++)
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
    }
	
    public static void DebugMethod()
    {
        //var half = new Half(16.8f);
        Debug.Log("Hello");
    }
}
