using UnityEngine;
using System.Collections;

public class DestinationProperties : MonoBehaviour {

    public float MoleculeType;
    public float arcLength;
    private float moleculeRadius;
    public int spacer = 5;
    public float PosOnCircle = 0.0f;
    public Vector4 origin;
    public Vector4 ScaledPosition;

    public void Initialize(float ID)
    {
        origin = transform.position;

        MoleculeType = ID;
        moleculeRadius = SceneManager.Instance.ProteinRadii[(int)ID]; // *PersistantSettings.Instance.Scale;

        //todo: scale the spacer too?        
        //if(ID < (SceneManager.Instance.ProteinRadii.Count - 1)) arcLength = radiusScaled + SceneManager.Instance.ProteinRadii[(int)ID + 1] * PersistantSettings.Instance.Scale + spacer;
        //else arcLength = radiusScaled + SceneManager.Instance.ProteinRadii[0] * PersistantSettings.Instance.Scale + spacer;
        if(ID < (SceneManager.Instance.ProteinRadii.Count - 1)) arcLength = moleculeRadius + SceneManager.Instance.ProteinRadii[(int)ID + 1] + spacer;
        else arcLength = moleculeRadius + SceneManager.Instance.ProteinRadii[0] + spacer;

    }

    public float getArcLength()
    {
        return arcLength;
    }

    void setArcLength(float arc)
    {
        arcLength = arc;
    }


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
