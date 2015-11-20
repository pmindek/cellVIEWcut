using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestinationProperties : MonoBehaviour {

    public float MoleculeType;
    public float arcLength;

    public MoleculeGroup MoleculeInfo;

    public int spacer = 5;
    public float PosOnCircle = 0.0f;
    public Vector4 origin; //position of the "marker" cube / center of the instance target location
    public Vector4 ScaledPosition;

    //properties of plane and histogram representations forms
    //TODO: move to moleculegroup class one actually used
    private float planeSurface; //cuboid with depth 1    
    //histogram properties
    public float BarWidth;
    public float BarDepth;
    private float barHeight; //determined by volume, width, and depth

    public void Initialize(MoleculeGroup molecule)
    {
        origin = transform.position;

        MoleculeType = molecule.ID;
        MoleculeInfo = molecule;

        //sphere layout

        //radius = instance bounding sphere radius
        //if (MoleculeType < (SceneManager.Instance.ProteinRadii.Count - 1)) arcLength = MoleculeInfo.moleculeRadius + AnimationManager.Instance.Ingredients[(int)MoleculeType + 1].moleculeRadius + spacer;
        //else arcLength = MoleculeInfo.moleculeRadius + AnimationManager.Instance.Ingredients[0].moleculeRadius + spacer;

        //radius = sphere volume of all instances radius
        if (MoleculeType < (SceneManager.Instance.ProteinRadii.Count - 1)) arcLength = MoleculeInfo.sphericRTotal + AnimationManager.Instance.Ingredients[(int)MoleculeType + 1].sphericRTotal + spacer;
        else arcLength = MoleculeInfo.sphericRTotal + AnimationManager.Instance.Ingredients[0].sphericRTotal + spacer;

        //other layouts
        //TODO...
    }

    public void SetInstancePositions()
    {
        float targetSphereRadius = MoleculeInfo.sphericRTotal - MoleculeInfo.moleculeRadius;

        //sample sphere positions for each instance
        //float stepSize = 1.0f / MoleculeInfo.InstanceCount;
        //float currentStep = 0.0f;

        for (int i = 0; i < MoleculeInfo.InstanceCount; i++)
        {
            Vector3 newPos = new Vector3();

            do
            {
                for (int j = 0; j < 3; j++)
                {
                    newPos[j] = Random.Range(-1.0F, 1.0F);
                }

            } while (newPos.sqrMagnitude > 1.0);

            newPos = newPos * MoleculeInfo.sphericRTotal;

            Vector4 target = new Vector4(ScaledPosition.x + newPos.x, ScaledPosition.y + newPos.y, ScaledPosition.z + newPos.z, ScaledPosition.w);            
            //AnimationManager.Instance.destinationsPerInstance.Add(target); //TODO: remove this as soon as baked animation stuff is working
            Vector4 source = AnimationManager.Instance.Ingredients[(int)MoleculeType].OriginalPositions[i];

            InstanceControlPoints cpList = new InstanceControlPoints();
            cpList.AddCPP(new ControlPointPair(source, target));
            //*****************************
            //*** INFO: at this point additional control points for the motion path can/would be added
            //*****************************
            AnimationManager.Instance.Ingredients[(int)MoleculeType].InstanceAnimationPaths.Add(cpList);
        }
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
