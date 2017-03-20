﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FractalTree : MonoBehaviour
{
	[SerializeField]
	Material[] basicColours;
	[SerializeField]
	Sprite leafSprite;
	public float scale, height, startingWidth, leafSize;
	LineRenderer lr;
	[SerializeField]
	Color leafColour;

	// Use this for initialization
	void Start ()
	{
		oldCol = leafColour;
		oldLeafScale = leafSize;
		MakeTree();
	}

	public void MakeTree ()
	{
		//Begin by making the trunk
		if (!lr)
			lr = gameObject.AddComponent<LineRenderer>();
		else {
			foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>()) {
				Destroy(s.gameObject);
			}
			foreach (LineRenderer l in GetComponentsInChildren<LineRenderer>()) {
				if (l != lr)
					Destroy(l.gameObject);
			}
		}
		lr.material = basicColours [3];
		lr.startWidth = startingWidth;
		lr.useWorldSpace = false;
		lr.SetPositions(new Vector3[] { 
			new Vector3 (0, 0, 0),
			new Vector3 (0, height, 0)
		});

		List<LineRenderer> branches = new List<LineRenderer> ();
		List<LineRenderer> NewBranches = new List<LineRenderer> ();
		branches.Add(lr);

		for (int iteration = 0; iteration < 3; iteration++) {
			for (int p = 0; p < branches.Count; p++) {
				for (int i = 0; i < 3; i++) {
					//draw a linearooney
					LineRenderer l = NewLine(branches [p].GetPosition(1),branches [p].transform);
					l.material = basicColours [3];
					l.useWorldSpace = false;
					l.startWidth = startingWidth / Mathf.Pow(2,iteration);
					l.SetPositions(new Vector3[] { 
						new Vector3 (0, 0, 0),
						new Vector3 (Random.Range(1f,-1f) / Mathf.Pow(2f,iteration), Random.Range(1.25f,.75f) / Mathf.Pow(1.25f,iteration), Random.Range(1f,-1f) / Mathf.Pow(2f,iteration))
					});
					NewBranches.Add(l);
				}
			}
			branches.Clear();
			foreach (LineRenderer lrlr in NewBranches)
				branches.Add(lrlr);
			NewBranches.Clear();
		}

		foreach (LineRenderer lr in branches) {
			GameObject sprite = new GameObject ("Leaf");
			sprite.AddComponent<SpriteRenderer>().sprite = leafSprite;
			sprite.GetComponent<SpriteRenderer>().color = leafColour;
			sprite.transform.localScale = Vector3.one * Random.Range(2.75f,3.25f);
			sprite.transform.parent = lr.transform;
			sprite.transform.localPosition = lr.GetPosition(1);
			sprite.AddComponent<LookAt>().target = Camera.main.transform;
		}

	}

	LineRenderer NewLine (Vector3 pos, Transform _parent)
	{
		GameObject line = new GameObject ("" + pos);
		line.transform.SetParent(_parent);
		line.transform.localPosition = pos;
		line.transform.localScale = Vector3.one * scale;
		return line.AddComponent<LineRenderer>();
	}

	Color oldCol;
	float oldLeafScale;

	void Update ()
	{
		if (lr) {			
			if (lr.startWidth != startingWidth) {

				float herRatios = startingWidth / lr.startWidth;

				foreach (LineRenderer lr in GetComponentsInChildren<LineRenderer>()) {
					lr.startWidth *= herRatios;
				}
			}
		}

		if (leafColour != oldCol) {
			foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>())
				s.color = leafColour;
			oldCol = leafColour;
		}
		if (leafSize != oldLeafScale) {
			foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>())
				s.transform.localScale = Vector3.one * leafSize;
			oldCol = leafColour;
		}
	}
}

[CustomEditor(typeof(FractalTree))]
public class FractualUI : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();

		FractalTree rp = (FractalTree)target;
		if (GUILayout.Button("Make tree"))
			rp.MakeTree();
	}
}