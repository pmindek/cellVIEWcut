using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AnimationManager : MonoBehaviour
{
    // Declare the AnimationManager as a singleton
    private static AnimationManager _instance = null;
    public static AnimationManager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<AnimationManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_animationManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_animationManager") {  };
                _instance = go.AddComponent<AnimationManager>();
            }

            //_instance.OnUnityReload();
            return _instance;
        }
    }

    public static bool CheckInstance()
    {
        return _instance != null;
    }

    //--------------------------------------------------------------
         
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
    public int NumberOfSteps = 300;  //# of steps to complete the transition

    //current animation position
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float currentZ = 0.0f;

    public Vector4 Destination; //the origin of the destination volume (= volume to which instances will transition)
    private GameObject destinationCube; //defines the origin of the destination coordinate system
    private List<GameObject> destinationsPerType; //transition destination for each molecule type
    public List<Vector4> destinationsPerInstance; //transition destination for each molecule instance
    private List<Vector4> lastPosPerInstance; //saves the last animation position to support staged animation

    private float umfang = 0.0f; //used for the circular layout

    //###############################################################################
    //ingredient volume relation stuff
    public float AtomUnit = 1.0f; //describes the size of the cube that a single atom occupies -> TODO: editable
    public float XVolume = 10.0f; // XVolume * AtomUnit = length of the volume that houses all ingredient molecules -> TODO: editable
    public float YVolume = 10.0f; // XVolume * AtomUnit = width of the volume -> TODO: editable
    public List<int> VolumeOrder = new List<int>(); //contains the order in which the ingredients should be placed in the volume -> TODO: editable

    public int TotalNumberOfAtoms = 0;

    //animation states:
    public enum AnimState { Paused, Play, Rewind };
    public AnimState AnimationState = AnimState.Play;
    private int anistate = 0;
    public int PausePlayRewind
    {
        get
        {
            return anistate;
        } 
        set
        {
            if (anistate <= 0 && anistate >= 2)
                AnimationState = (AnimState)anistate;
            else
            {
                anistate = 0;
                AnimationState = (AnimState)anistate;
            }
        }
    }

    void OnEnable()
    {
        step_size = 1.0f / NumberOfSteps;
        step_count = 0;
    }



    // Use this for initialization
    void Start()
    {
        //create molecule objects that store the original pos/rot of each instance
        parseMolecules();

        //create destination points for each instance per molecule type
        createMoleculeLayout();

        //now that we have the final layout, we can calculate and store the animation paths
        //bakeAnimations();
    }

    //private void bakeAnimations()
    //{
    //    foreach (MoleculeGroup m in Ingredients)
    //    {
            
    //    }
    //}

    private void createMoleculeLayout()
    {
        destinationsPerType = new List<GameObject>();
        destinationsPerInstance = new List<Vector4>();

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
            //now we can calculate the instance positions
            props.SetInstancePositions();

        }
    }

    // Update is called once per frame
    void Update()
    {
        //debug_frame_counter++;
        new_positions.Clear();

        //current_step = step_size * step_count;

        ////update position buffer...
        //for (int i = 0; i < types.Count; i++)
        //{
        //    //initial proof of concept: manipualtion of original instance positions along an axis
        //    //new_positions.Add(new Vector4(positions[i].x - debug_frame_counter, positions[i].y, positions[i].z, positions[i].w));

        //    //second proof of concept: linear interpolation for all instances towards a user specified gameobject position
        //    //currentX = LinearInterpol(positions[i].x, Destination.x, Mathf.Pow(current_step, 1));
        //    //currentY = LinearInterpol(positions[i].y, Destination.y, Mathf.Pow(current_step, 1));
        //    //currentZ = LinearInterpol(positions[i].z, Destination.z, Mathf.Pow(current_step, 1));

        //    //third proof of concept: interpolate instances towards a point per type
        //    //send each molecule type towards its designated target cube
        //    //Vector4 pos = destinationsPerType[(int)types[i].x].GetComponent<DestinationProperties>().ScaledPosition;
        //    //currentX = LinearInterpol(positions[i].x, pos.x, Mathf.Pow(current_step, 1));
        //    //currentY = LinearInterpol(positions[i].y, pos.y, Mathf.Pow(current_step, 1));
        //    //currentZ = LinearInterpol(positions[i].z, pos.z, Mathf.Pow(current_step, 1));

        //    //fourth proof of concept: individual target point for each instance
        //    //Vector4 pos = destinationsPerInstance[i];
        //    //currentX = LinearInterpol(positions[i].x, pos.x, Mathf.Pow(current_step, 1));
        //    //currentY = LinearInterpol(positions[i].y, pos.y, Mathf.Pow(current_step, 1));
        //    //currentZ = LinearInterpol(positions[i].z, pos.z, Mathf.Pow(current_step, 1));

        //    new_positions.Add(new Vector4(currentX, currentY, currentZ, positions[i].w));
        //    //new_positions.Add() --> current point pos is picked from
        //}

        //fifth proof of concept: load baked animation for each instance
        //read baked animations..

        if (AnimationState == AnimState.Play)
        {
            foreach(MoleculeGroup m in Ingredients)
            {
                //TODO: check whether we should animate the respective molecule type (filtered)
                //TODO: or if we should wait a certain number of steps until we animate the next type (staged)
                //in both cases, we should fill the new_positions vector with the original values / values from the last iteration

                //go through all the instances of the current m-type
                foreach (InstanceControlPoints cpp in m.InstanceAnimationPaths)
                {
                    new_positions.Add(cpp.GetNext());
                }
            }
        }
        else if (AnimationState == AnimState.Rewind)
        {
            foreach (MoleculeGroup m in Ingredients)
            {
                //go through all the instances of the current m-type
                foreach (InstanceControlPoints cpp in m.InstanceAnimationPaths)
                {
                    new_positions.Add(cpp.GetPrev());
                }
            }
        }
        
        //save the last animation position of all instances
        //TODO: initialize with original positions
        lastPosPerInstance = new_positions;
        
        //TODO: new way for stopping the animation... should end when no more "frames" are to be played and not in dependency to NumberOfSteps, since the number can differ between molecule  types
        if(step_count <= NumberOfSteps) step_count++;

        GPUBuffer.Instance.ProteinInstancePositions.SetData(new_positions.ToArray());
    }

    public static float LinearInterpol(float origin, float destination, float step)
    {
        float result = 0.0f;

        result = origin * (1 - step) + destination * step;

        return result;
    }


    private void parseMolecules()
    {
        destinationCube = GameObject.Find("destinationCube");
        //scale the position to bring it to "protein space"
        //Destination = destinationCube.transform.position / 0.065f;

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

    public float moleculeRadius; //bounding sphere radius
    public float cubicVolumeTotal;
    public float cubeMSingle; //m = side of the cube that describes the volume of an instance
    public float cubeMTotal;  //m of the volume that describes the sum of all instances
    public float sphericRTotal;

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

    //for each instance, we store a list of ControlPairPoints that contain the baked animation path between the pair of points
    public List<InstanceControlPoints> InstanceAnimationPaths;

    public MoleculeGroup(float id, List<int> instanceCounts, int atomCount)
    {
        ID = id;
        InstanceCount = instanceCounts[(int)ID];
        AtomsPerInstance = atomCount;
        TotalAtomsOfType = InstanceCount * AtomsPerInstance;
        moleculeRadius = SceneManager.Instance.ProteinRadii[(int)id];
        InstanceAnimationPaths = new List<InstanceControlPoints>();

        for (int i = 0; i < (int)ID; i++)
        {
            StartIndex += instanceCounts[i];
        }

        OriginalPositions = copyOriginalValues(SceneManager.Instance.ProteinInstancePositions, InstanceCount, StartIndex);
        OriginalRotations = copyOriginalValues(SceneManager.Instance.ProteinInstanceRotations, InstanceCount, StartIndex);

        SetVolumes(moleculeRadius);
    }

    //copies original position/rotation values from the composite list based on ID & count (length)
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

    public void SetVolumes(float radius)
    {
        cubeMSingle = radiusToLength(radius); //side of cube that describes the same volume as the sphere
        cubicVolumeTotal = Mathf.Pow(cubeMSingle, 3.0f) * InstanceCount; //total cubic volume of all instances
        float sphereVolumeTotal = (4.0f * Mathf.PI * Mathf.Pow(radius, 3.0f) / 3.0f) * InstanceCount;

        cubeMTotal = Mathf.Pow(cubicVolumeTotal, 1.0f / 3.0f); //side of the cube

        //radius of the sphere
        sphericRTotal = lengthToRadius(cubeMTotal);
        //Debug.Log("bla");

        //calculate plane
        //TODO

        //calculate bar height
        //TODO

    }

    //given the bounding sphere radius, calculate the lenght of the cube that occupies the same volume as the sphere
    private float radiusToLength(float radius)
    {
        float length = 0.0f;

        length = Mathf.Pow(2.0f, 2.0f / 3.0f) * Mathf.Pow(((Mathf.PI) / 3.0f), 1.0f / 3.0f) * radius;

        return length;
    }

    //convert the length of a cube to the radius of a sphere
    private float lengthToRadius(float length)
    {
        float radius = 0.0f;

        radius = Mathf.Pow(((3.0f) / Mathf.PI), 1.0f / 3.0f) * length / Mathf.Pow(2.0f, 2.0f / 3.0f);

        return radius;
    }
}

public class InstanceControlPoints
{
    public List<ControlPointPair> ControlPoints;
    public int CurrentCPP = 0;
    private Vector4 lastPoint;

    public Vector4 GetNext()
    {
        ControlPointPair cpp;
        Vector4 pointOfReturn;

        //clamp the pointer (not very elegant)
        if (CurrentCPP < 0) CurrentCPP = 0;

        while (CurrentCPP < ControlPoints.Count)
        {
            cpp = ControlPoints[CurrentCPP];

            //clamp the pointer (not very elegant)
            if (cpp.CurrentFrame < 0) cpp.CurrentFrame = 0;

            if (cpp.CurrentFrame < cpp.bakedPoints.Count)
            {
                pointOfReturn = cpp.bakedPoints[cpp.CurrentFrame];
                lastPoint = pointOfReturn;
                cpp.CurrentFrame++;
                return pointOfReturn;
            }
            else CurrentCPP++;
        }
        return lastPoint;
    }

    public Vector4 GetPrev()
    {
        ControlPointPair cpp;
        Vector4 pointOfReturn;

        //clamp the pointer (not very elegant)
        if (CurrentCPP >= ControlPoints.Count) CurrentCPP = ControlPoints.Count - 1;

        while (CurrentCPP >= 0)
        {
            cpp = ControlPoints[CurrentCPP];

            //clamp the pointer (not very elegant)
            if (cpp.CurrentFrame >= cpp.bakedPoints.Count) cpp.CurrentFrame = cpp.bakedPoints.Count - 1;

            if (cpp.CurrentFrame >= 0)
            {
                pointOfReturn = cpp.bakedPoints[cpp.CurrentFrame];
                lastPoint = pointOfReturn;
                cpp.CurrentFrame--;
                return pointOfReturn;
            }
            else CurrentCPP--;
        }
        return lastPoint;
    }

    public void AddCPP(ControlPointPair cpp)
    {
        ControlPoints.Add(cpp);
        //initialize lastPoint with the very first baked point
        if (lastPoint == null) lastPoint = cpp.bakedPoints[0];
    }


    public InstanceControlPoints()
    {
        ControlPoints = new List<ControlPointPair>();
    }
}

public class ControlPointPair
{
    public Vector4 StartPoint;
    public Vector4 EndPoint;
    public int NumSteps;
    public int CurrentFrame = 0;
    //TODO: control points for non-linear interpol?
    public int InterpolationMethod;
    public List<Vector4> bakedPoints; //baked interpolation in #NumSteps samples between StartPoint and EndPoint

    //current animation position
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float currentZ = 0.0f;

    public ControlPointPair(Vector4 start, Vector4 end, int steps = 300, int method = 0)
    {
        StartPoint = start;
        EndPoint = end;
        NumSteps = steps;
        float stepsize = 1.0f / NumSteps;
        float currentStep = 0.0f;
        InterpolationMethod = method;
        bakedPoints = new List<Vector4>();

        switch (InterpolationMethod)
        {
            case 0: //linear
                for (int i = 1; i <= NumSteps; i++)
                {
                    currentStep = stepsize * i;
                    currentX = AnimationManager.LinearInterpol(start.x, end.x, currentStep);
                    currentY = AnimationManager.LinearInterpol(start.y, end.y, currentStep);
                    currentZ = AnimationManager.LinearInterpol(start.z, end.z, currentStep);

                    bakedPoints.Add(new Vector4(currentX, currentY, currentZ, 0.0f));
                }
                break;
        }
    }
}
