using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RailingPlacer : MonoBehaviour
{
	public int sizeX = 0, sizeY = 0;
	public bool combineMeshesOnCompletion, selectiveRemoval, DestoryCollidersOnCompletion, CreateNewMeshCollider;
	[SerializeField]
	float chunkSize = 0.5375f;
	[SerializeField]
	GameObject railingObject;
	[SerializeField]
	Transform associatedBuilding;
	[SerializeField]
	Collider[] noFencesHerePlease;

	void Start ()
	{
		MakeRailings(sizeX,sizeY);
	}

	List<GameObject> rails = new List<GameObject> ();

	public IEnumerator MakeRailings (int _sizeX, int _sizeY)
	{
		//For square sections of railings, add 2 to sizeY
		if (rails.Count > 0) {
			foreach (GameObject g in rails) {
				Destroy(g);
			}
			rails.Clear();
		}
		for (int x = 0; x < _sizeX; x++) {
			for (int y = 0; y < _sizeY; y++) {
				if (y == 0 || y == _sizeY - 1) {
					GameObject newRail = Instantiate(railingObject,transform) as GameObject;
					Vector3 spawnPos = new Vector3 (chunkSize * (y == 0 ? y : y - 1), 0, -chunkSize * (y == 0 ? x : x + 1));
					newRail.transform.localPosition = spawnPos;
					Quaternion spawnRot = Quaternion.Euler(y == 0 ? Vector3.zero : Vector3.up * 180);
					newRail.transform.localRotation = spawnRot;
					rails.Add(newRail);
					//newRail.transform.localPosition += new Vector3 (-(float)_sizeY / 2, 0, (float)_sizeX / 2);
				}
				else if (x == 0 || x == _sizeX - 1) {

						GameObject newRail = Instantiate(railingObject,transform) as GameObject;
						Vector3 spawnPos = new Vector3 (chunkSize * (x == 0 ? y : y - 1), 0, -chunkSize * (x == 0 ? x : x + 1));
						newRail.transform.localPosition = spawnPos;
						Quaternion spawnRot = Quaternion.Euler(x == 0 ? Vector3.up * 90 : Vector3.up * 270);
						newRail.transform.localRotation = spawnRot;
						rails.Add(newRail);
						//newRail.transform.localPosition += new Vector3 (-(float)_sizeY / 2, 0, (float)_sizeX / 2);
					}
			}
		}
		old = _sizeX * _sizeY;
		if (selectiveRemoval)
			SelectivelyRemove();
		if (DestoryCollidersOnCompletion) {
			foreach (GameObject g in rails) {
				Destroy(g.GetComponentInChildren<Collider>());
			}
		}
		yield return new WaitForSeconds (0.1f);

		if (combineMeshesOnCompletion)
			CombineMeshes();
	}

	public void SelectivelyRemove ()
	{
		foreach (Collider c in GetComponentsInChildren<Collider>()) {
			foreach (Collider _c in noFencesHerePlease) {
				if (_c.bounds.Contains(c.bounds.center)) {
					Destroy(c.gameObject);
				}
			}
		}

		foreach (Collider c in noFencesHerePlease) {
			Destroy(c.gameObject);
		}
	}

	public void CombineMeshes ()
	{
		Vector3 prev = transform.position;
		transform.position = Vector3.zero;
		transform.localScale /= 2;
		MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance [meshes.Length];
		int i = 0;
		while (i < meshes.Length) {
			combine [i].mesh = meshes [i].sharedMesh;
			combine [i].transform = meshes [i].transform.localToWorldMatrix;
			meshes [i].gameObject.SetActive(false);
			i++;
		}
		MeshFilter newMesh = gameObject.AddComponent<MeshFilter>();
		newMesh.mesh = new Mesh ();
		newMesh.mesh.CombineMeshes(combine);
		if (CreateNewMeshCollider)
			GetComponent<MeshCollider>().sharedMesh = newMesh.mesh;
		transform.position = prev;
		transform.localScale *= 2;
		transform.localRotation = associatedBuilding.localRotation;
		foreach (GameObject g in rails) {
			Destroy(g);
		}

		GetComponent<MeshRenderer>().sortingOrder = -10;
	}

	int old;
	// Update is called once per frame
	void Update ()
	{
		if (sizeX * sizeY != old) {
			StartCoroutine(MakeRailings(sizeX,sizeY));
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(RailingPlacer))]
public class thingDoer : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();

		RailingPlacer rp = (RailingPlacer)target;
		if (GUILayout.Button("Combine meshes"))
			rp.CombineMeshes();
		if (GUILayout.Button("Selective removal"))
			rp.SelectivelyRemove();
	}
}
#endif