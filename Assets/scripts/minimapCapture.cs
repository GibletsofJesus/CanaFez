﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class minimapCapture : MonoBehaviour
{
	public static minimapCapture instance;

	[SerializeField]
	Camera thisCamera;
	[SerializeField]
	GameObject[] thingsToDisable;
	public List<Sprite> captures = new List<Sprite> ();
	[SerializeField]
	Image renderToMe;

	void Start ()
	{
		instance = this;
	}

	void Update ()
	{
		if (!instance)
			instance = this;
	}

	public void Capture ()
	{
		foreach (GameObject g in thingsToDisable) {
			g.SetActive(false);
		}

		//targetTextureAsQuad.SetActive(true);
		RenderTexture.active = thisCamera.targetTexture;
		Texture2D tex = new Texture2D (120, 80, TextureFormat.RGB24, true, true);
		tex.filterMode = FilterMode.Point;
		//thisCamera.Render();
		tex.ReadPixels(new Rect (0, 0, 120, 80),0,0);
		tex.Apply();

		//targetTextureAsQuad.SetActive(false);
		foreach (GameObject g in thingsToDisable) {
			g.SetActive(true);
		}

		captures.Add(Sprite.Create(tex,(new Rect (20, 0, 80, 80)),new Vector2 (0.5f, 0.5f)));
		renderToMe.sprite = captures [captures.Count - 1];
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(minimapCapture))]
public class capturererer : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();

		minimapCapture mc = (minimapCapture)target;
		if (GUILayout.Button("Capture"))
			mc.Capture();
	}
}
#endif