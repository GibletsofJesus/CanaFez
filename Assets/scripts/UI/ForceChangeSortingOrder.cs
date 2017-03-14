using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceChangeSortingOrder : MonoBehaviour
{
	public int order;

	void Update ()
	{
		GetComponent<Renderer>().sortingOrder = order;
	}
}
