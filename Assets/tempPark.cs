using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempPark : MonoBehaviour
{

	[SerializeField]
	GameObject treePrefab;

	// Use this for initialization
	void Start ()
	{
		GameObject newTree = Instantiate(treePrefab,Vector3.zero,transform.rotation);
		newTree.transform.parent = transform;
		newTree.transform.localPosition = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
