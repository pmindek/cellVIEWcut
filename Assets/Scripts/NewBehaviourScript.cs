using System;
using UnityEngine;
public class NewBehaviourScript : MonoBehaviour
{
    public TreeViewController TreeViewController;

	// Use this for initialization
	void Start ()
    {
	    foreach (var node in PersistantSettings.Instance.hierachy)
	    {
            TreeViewController.AddNodeObject(node.path, new object[] { node.name }, "Text");
        }
        TreeViewController.Init();
    }
}
