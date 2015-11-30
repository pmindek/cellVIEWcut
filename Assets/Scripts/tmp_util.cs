using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

public class tmp_util : MonoBehaviour {

    private XmlSerializer serializer;
    public string path = "C:\\proteins.xml";
    private FileStream stream;

	// Use this for initialization
	void Start ()
    {
        ////read
        //serializer = new XmlSerializer(typeof(CutParametersContainer));
        //stream = new FileStream(path, FileMode.Open);
        //var container = serializer.Deserialize(stream) as MonsterContainer;
        //stream.Close();


        ////write
        // serializer = new XmlSerializer(typeof(CutParametersContainer));
        // stream = new FileStream(path, FileMode.Create);
        // serializer.Serialize(stream, this);
        // stream.Close();
	}
	

    public void ExportProteinSettings()
    {
        CutParametersContainer exportParams = new CutParametersContainer();

        foreach (CutObject cuto in SceneManager.Instance.CutObjects)
        {
            exportParams.ProteinTypeParameters.Add(cuto.ProteinTypeParameters);
        }

        ////write
        serializer = new XmlSerializer(typeof(CutParametersContainer));
        stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, exportParams);
        stream.Close();
    }

    public void ImportProteinSettings()
    {
        ////read
        serializer = new XmlSerializer(typeof(CutParametersContainer));
        stream = new FileStream(path, FileMode.Open);
        CutParametersContainer importParams = serializer.Deserialize(stream) as CutParametersContainer;
        stream.Close();

        for (int i = 0; i < importParams.ProteinTypeParameters.Count && i < SceneManager.Instance.CutObjects.Count; i++)
        {
            SceneManager.Instance.CutObjects[i].ProteinTypeParameters = importParams.ProteinTypeParameters[i]; 
        }
    }

    [XmlRoot("CutParametersContainer")]
    public class CutParametersContainer
    {
        [XmlArray("List of ParamSets")]
        [XmlArrayItem("ParamSet")]
        public List<List<CutParameters>> ProteinTypeParameters = new List<List<CutParameters>>();
    }
}
