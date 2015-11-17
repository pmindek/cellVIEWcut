using UnityEngine;
using System.Collections;

public class DestinationProperties : MonoBehaviour {

    public float MoleculeType;
    public float arcLength;
    private float moleculeRadius;

    public MoleculeGroup MoleculeInfo;

    public int spacer = 5;
    public float PosOnCircle = 0.0f;
    public Vector4 origin;
    public Vector4 ScaledPosition;

    private float cubicVolumeTotal;
    private float cubeMSingle; //m = side of the cube that describes the volume of an instance
    private float cubeMTotal;  //m of the volume that describes the sum of all instances
    private float sphericRTotal;
    private float planeSurface; //cuboid with depth 1
    
    //histogram properties
    public float BarWidth;
    public float BarDepth;
    private float barHeight; //determined by volume, width, and depth

    public void Initialize(MoleculeGroup molecule)
    {
        origin = transform.position;

        MoleculeType = molecule.ID;
        moleculeRadius = SceneManager.Instance.ProteinRadii[(int)MoleculeType]; // *PersistantSettings.Instance.Scale;
        MoleculeInfo = molecule;

        SetVolumes(moleculeRadius);


        //todo: scale the spacer too?        
        //if(ID < (SceneManager.Instance.ProteinRadii.Count - 1)) arcLength = radiusScaled + SceneManager.Instance.ProteinRadii[(int)ID + 1] * PersistantSettings.Instance.Scale + spacer;
        //else arcLength = radiusScaled + SceneManager.Instance.ProteinRadii[0] * PersistantSettings.Instance.Scale + spacer;
        if (MoleculeType < (SceneManager.Instance.ProteinRadii.Count - 1)) arcLength = moleculeRadius + SceneManager.Instance.ProteinRadii[(int)MoleculeType + 1] + spacer;
        else arcLength = moleculeRadius + SceneManager.Instance.ProteinRadii[0] + spacer;
    }

    public void SetVolumes(float radius)
    {
        cubeMSingle = radiusToLength(radius); //side of cube that describes the same volume as the sphere
        cubicVolumeTotal = Mathf.Pow(cubeMSingle, 3.0f) * MoleculeInfo.InstanceCount; //total cubic volume of all instances
        float sphereVolumeTotal = (4.0f * Mathf.PI * Mathf.Pow(radius, 3.0f) / 3.0f) * MoleculeInfo.InstanceCount;

        cubeMTotal = Mathf.Pow(cubicVolumeTotal, 1.0f / 3.0f); //side of the cube
        
        //radius of the sphere
        sphericRTotal = lengthToRadius(cubeMTotal);

        //calculate plane
        //TODO

        //calculate bar height
        //TODO

    }

    //given the bounding sphere radius, calculate the lenght of the cube that occupies the same volume as the sphere
    private float radiusToLength(float radius)
    {
        float length = 0.0f;

        length = Mathf.Pow(2.0f, 2.0f / 3.0f) * Mathf.Pow(((Mathf.PI * radius) / 3.0f), 1.0f / 3.0f);

        return length;
    }

    //convert the length of a cube to the radius of a sphere
    private float lengthToRadius(float length)
    {
        float radius = 0.0f;

        radius = Mathf.Pow(((3.0f * length) / Mathf.PI), 1.0f / 3.0f) / Mathf.Pow(2.0f, 2.0f / 3.0f);

        return radius;
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
