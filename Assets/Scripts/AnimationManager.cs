using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AnimationManager : MonoBehaviour
{
         
    private List<Vector4> positions;        //original positions of instances
    private List<Vector4> new_positions;    //stores position updates of instances
    private List<Vector4> types;            //type info of instances
    public int NumberOfIngredients;         //number of different ingredient types in the data set
    public List<int> InstanceCountPerIngredient = new List<int>();          //molecules per ingedient
    public List<MoleculeGroup> Ingredients = new List<MoleculeGroup>();     //one molecule group stores: ingredient ID, # instances from that ingredient, original pos&rot of each instance

    private int debug_frame_counter = 0;    //DEBUG/DEMO: for updating positions each frame
    private float step_size = 0.0f;     
    private int step_count = 0;         //current step number
    private float current_step = 0.0f;  //current step size
    public int NumberOfSteps = 400;  //# of steps to complete the transition

    //current animation position
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float currentZ = 0.0f;

    public Vector4 Destination; //the origin of the destination volume (= volume to which instances will transition)
    private GameObject destinationCube; //defines the origin of the destination coordinate system
    private List<GameObject> destinationsPerType; //transition destination for each molecule type

    private float umfang = 0.0f; //used for the circular layout

    //###############################################################################
    //ingredient volume relation stuff
    public float AtomUnit = 1.0f; //describes the size of the cube that a single atom occupies -> TODO: editable
    public float XVolume = 10.0f; // XVolume * AtomUnit = length of the volume that houses all ingredient molecules -> TODO: editable
    public float YVolume = 10.0f; // XVolume * AtomUnit = width of the volume -> TODO: editable
    public List<int> VolumeOrder = new List<int>(); //contains the order in which the ingredients should be placed in the volume -> TODO: editable

    public int TotalNumberOfAtoms = 0;

    void OnEnable()
    {
        step_size = 1.0f / NumberOfSteps;
        step_count = 0;
    }



    // Use this for initialization
    void Start()
    {
        //create molecule objects
        parseMolecules();

        //create destination points for each molecule type
        createMoleculeLayout();
    }

    private void createMoleculeLayout()
    {
        destinationsPerType = new List<GameObject>();

        //create game objects for each molecule type
        foreach (MoleculeGroup m in Ingredients)
        {
            GameObject temp = Instantiate(destinationCube);
            DestinationProperties props = temp.GetComponent<DestinationProperties>();
            props.Initialize(m);
            props.PosOnCircle = umfang;

            umfang += props.getArcLength();

            destinationsPerType.Add(temp);
            //temp.transform.parent = destinationCube.transform;
        }

        float radius = umfang / (2 * Mathf.PI);

        //now that the umfang is known, we can calculate the positions of the game objects along the circle
        foreach (GameObject o in destinationsPerType)
        {
            DestinationProperties props = o.GetComponent<DestinationProperties>();
            float alpha = props.PosOnCircle / radius;
            float x = radius * Mathf.Cos(alpha) * PersistantSettings.Instance.Scale;
            float y = radius * Mathf.Sin(alpha) * PersistantSettings.Instance.Scale;

            o.transform.position = new Vector3(props.origin.x + x, props.origin.y + y, props.origin.z);
            props.ScaledPosition = o.transform.position / PersistantSettings.Instance.Scale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //debug_frame_counter++;
        new_positions.Clear();

        current_step = step_size * step_count;

        //update position buffer...
        for (int i = 0; i < types.Count; i++)
        {
            //initial proof of concept: manipualtion of original instance positions along an axis
            //new_positions.Add(new Vector4(positions[i].x - debug_frame_counter, positions[i].y, positions[i].z, positions[i].w));

            //second proof of concept: linear interpolation for all instances towards a user specified gameobject position
            //currentX = LinearInterpol(positions[i].x, Destination.x, Mathf.Pow(current_step, 1));
            //currentY = LinearInterpol(positions[i].y, Destination.y, Mathf.Pow(current_step, 1));
            //currentZ = LinearInterpol(positions[i].z, Destination.z, Mathf.Pow(current_step, 1));

            //send each molecule type towards its designated target cube
            Vector4 pos = destinationsPerType[(int)types[i].x].GetComponent<DestinationProperties>().ScaledPosition;
            //Vector4 pos = destinationsPerType[(int)types[i].x].transform.position;
            currentX = LinearInterpol(positions[i].x, pos.x, Mathf.Pow(current_step, 1));
            currentY = LinearInterpol(positions[i].y, pos.y, Mathf.Pow(current_step, 1));
            currentZ = LinearInterpol(positions[i].z, pos.z, Mathf.Pow(current_step, 1));

            new_positions.Add(new Vector4(currentX, currentY, currentZ, positions[i].w));
        }

        if(step_count <= NumberOfSteps) step_count++;

        GPUBuffer.Instance.ProteinInstancePositions.SetData(new_positions.ToArray());
    }

    float LinearInterpol(float origin, float destination, float step)
    {
        float result = 0.0f;

        result = origin * (1 - step) + destination * step;

        return result;
    }


    private void parseMolecules()
    {
        destinationCube = GameObject.Find("destinationCube");
        //scale the position to bring it to "protein space"
        Destination = destinationCube.transform.position / 0.065f;

        positions = SceneManager.Instance.ProteinInstancePositions;
        types = SceneManager.Instance.ProteinInstanceInfos;
        NumberOfIngredients = SceneManager.Instance.ProteinNames.Count;
        new_positions = new List<Vector4>();

        InstanceCountPerIngredient = calculateInstancesPerIngredient(types);

        Ingredients = setupMolecules(NumberOfIngredients, InstanceCountPerIngredient, SceneManager.Instance.ProteinAtomCount);

        for (int i = 0; i < Ingredients.Count; i++)
        {
            TotalNumberOfAtoms += Ingredients[i].TotalAtomsOfType;
        }

        Debug.Log(TotalNumberOfAtoms);
    }

    private List<MoleculeGroup> setupMolecules(int numIngredients, List<int> instanceCounts, List<int> atomCounts)
    {
        List<MoleculeGroup> molecules = new List<MoleculeGroup>();
        MoleculeGroup molecule;

        for (int i = 0; i < numIngredients; i++)
        {
            molecule = new MoleculeGroup(i, instanceCounts, atomCounts[i]);
            molecules.Add(molecule);
        }

        return molecules;
    }

    private List<int> calculateInstancesPerIngredient(List<Vector4> moleculeTypes)
    {
        List<int> countPerIngredient = new List<int>();
        float currentType = 0.0f;
        int currentInstanceCount = 0;

        for (int i = 0; i < moleculeTypes.Count; ++i)
        {
            currentInstanceCount++;

            if (currentType != moleculeTypes[i].x)
            {
                currentType = moleculeTypes[i].x;

                countPerIngredient.Add(currentInstanceCount);
                currentInstanceCount = 0;
            }
        }
        countPerIngredient.Add(currentInstanceCount);

        return countPerIngredient;
    }

    void OnDisable()
    {
    }

    void OnDestroy()
    {
        foreach (GameObject o in destinationsPerType)
        {
            Destroy(o);
        }

    }
}

//represents all molecule instances of a molecule type
public class MoleculeGroup
{
    public float ID = -1.0f;
    public int InstanceCount = 0;
    public int AtomsPerInstance = 0;
    public int TotalAtomsOfType = 0;
    public int StartIndex = 0;
    public List<Vector4> OriginalPositions;
    public List<Vector4> OriginalRotations;

    //the origin of the container that will house all instances of this ingredient type after the transition
    //-> depends on: atom volume, volume length&width of the complete container, volumes of all prior ingredients
    //(as they fill the complete container) => total height of prev molecule groups
    public Vector4 IngredientDestOrigin;
    public Vector4 MoleculeBoundingVolume; //w/h/l of a single molecule
    public float GroupHeight = 0;
    //molecule lenght, width? space filling alg to determine?
    //molecule group height = how much space do all molecules of this type need? (w&h already defined in total volume)

    //depending on: MoleculeBoundingVolume & the number of previous molecules from this group
    public List<Vector4> MoleculeDestOrigins; //the origin of the destination of each molecule (within the coord system of the ingredient type destination container)

    public MoleculeGroup(float id, List<int> instanceCounts, int atomCount)
    {
        ID = id;
        InstanceCount = instanceCounts[(int)ID];
        AtomsPerInstance = atomCount;
        TotalAtomsOfType = InstanceCount * AtomsPerInstance;

        for (int i = 0; i < (int)ID; i++)
        {
            StartIndex += instanceCounts[i];
        }

        OriginalPositions = copyOriginalValues(SceneManager.Instance.ProteinInstancePositions, InstanceCount, StartIndex);
        OriginalRotations = copyOriginalValues(SceneManager.Instance.ProteinInstanceRotations, InstanceCount, StartIndex);

    }

    /// <summary>
    /// copies original position/rotation values from the composite list based on ID & count (length)
    /// </summary>
    private List<Vector4> copyOriginalValues(List<Vector4> invec, int instcount, int start)
    {
        List<Vector4> outvec = new List<Vector4>();
        Vector4 current;

        for (int i = 0; i < instcount; i++)
        {
            current = invec[i + start];
            outvec.Add(current);
        }

        return outvec;
    }
}
