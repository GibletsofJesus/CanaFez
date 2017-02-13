using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(toggleShortcut))]
public class toggleShortcut : Editor
{
	public MeshRenderer mr;

	void OnSceneGUI ()
	{
		Event e = Event.current;
		switch (e.type) {
			case EventType.KeyDown:
				if (e.keyCode == KeyCode.G)
					mr.enabled = !mr.enabled;
				break;
		}
	}
}

[ExecuteInEditMode]
public class EditorToggle : MonoBehaviour
{
	[ExecuteInEditMode]
	void OnRenderObject ()
	{
		if (Input.GetKeyDown(KeyCode.G)) {
			Debug.Log("button");
		}
	}
}
