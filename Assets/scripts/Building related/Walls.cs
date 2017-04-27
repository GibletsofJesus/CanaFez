using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walls : MonoBehaviour
{
	[SerializeField]
	Transform roofPoint;
	// Use this for initialization
	void Start ()
	{
		
	}

	void OnColliderEnter (Collision col)
	{
		if (col.gameObject.tag == "Player") {
			//GET OUT OF THE WALLS

			col.transform.position = new Vector3 (
				col.transform.position.x,
				roofPoint.position.y + 2,
				col.transform.position.z
			);
		}
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
