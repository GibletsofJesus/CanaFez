using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//using UnityEditor;

public class WorldGen : MonoBehaviour
{
	[System.Serializable]
	class buildingPrefab
	{
		public string name;
		public Building buildingObject;
		public int sizeX, sizeY;
	}

	public static WorldGen instance;

	[SerializeField]
	buildingPrefab[] buildingPrefabs;
	[SerializeField]
	int citySizeX, citySizeY;
	[SerializeField]
	Transform worldObjects;
	public float buildingChunkSize = 7, heightVaraiation, density;
	float roadWidth;
	List<Building> buildings = new List<Building> ();

	bool[,] availableSquares;
	[SerializeField]
	List<Vector2> possiblePlacements = new List<Vector2> ();

	void Start ()
	{
		instance = this;
		StartCoroutine(GenerateCity());
		CityNameUi.text = GenerateSillyName();
	}

	bool checkSpace (int _sizeX, int _sizeY, int _posX, int _posY, int rot)
	{
		for (int x = 0; x < _sizeX; x++) {
			for (int y = 0; y < _sizeY; y++) {
				switch (rot) {
					case 0: //0 degree rotation
						if (!availableSquares [_posX + x, _posY + y]) {
							return false;
						}
						break;
					case 1://90 degree rotation
						if (_posX - y < 0 || _posY + x < citySizeY)
							return false;
						if (!availableSquares [_posX - y, _posY + x]) {
							return false;
						}	
						break;
					case 2://180 degree rotation
						if (_posY - y < 0 || _posX - x < 0)
							return false;
						if (!availableSquares [_posX - x, _posY - y]) {
							return false;
						}	
						break;
					case 3://270 degree rotation
						if (_posX + y < citySizeX || _posY - x < 0)
							return false;
						if (!availableSquares [_posX + y, _posY - x]) {
							return false;
						}	
						break;
						return false;
				}
			}
		}
		return true;
	}

	public IEnumerator GenerateCity ()
	{
		ThirdPersonCharacter.instance.lastSpawner = null;
		worldObjects.rotation = Quaternion.Euler(Vector3.zero);
		foreach (Building b in buildings) {
			Destroy(b.gameObject);
		}
		buildings.Clear();
		ResetSquares();

		//Place me some parks bb
		int parkX = Random.Range(3,8), parkY = Random.Range(3,6);
		int startx = Random.Range(0,citySizeX - parkX - 1), starty = Random.Range(0,citySizeY - parkY - 1);
		for (int i = startx; i < startx + parkX; i++) {
			for (int j = starty; j < starty + parkY; j++) {
				availableSquares [i, j] = false;
			}
		}

		for (int i = 0; i < density; i++) {
			//Which building to place
			//This will eventually be much better system, but it'll do FOR NOW
			int index = Random.Range(1,buildingPrefabs.Length);
			int rotAttempt = 0;

			int tryThisOne = Random.Range(0,possiblePlacements.Count);
			//Whilst we still have available places to put large buildings down
			if (possiblePlacements.Count > 0) {		
				//Find a valid postion to place a building	
				while (possiblePlacements.Count > 0 &&
				       !checkSpace(buildingPrefabs [index].sizeX,buildingPrefabs [index].sizeY, 			//object dimensions
					       (int)possiblePlacements [tryThisOne].x,(int)possiblePlacements [tryThisOne].y, 	//position
					       rotAttempt)) {//orientation
					if (rotAttempt < 3) {
						rotAttempt++;	
					}
					else {	
						//Only do this if we've tried every possible rotation orientation (4)
						possiblePlacements.RemoveAt(tryThisOne);
						rotAttempt = 0;
						tryThisOne = Random.Range(0,possiblePlacements.Count - 1);
					}
				}

				if (possiblePlacements.Count > 0)
					//Place building
					yield return PlaceLargeBuilding(buildingPrefabs [index].buildingObject,
						buildingPrefabs [index].sizeX,buildingPrefabs [index].sizeY,
						(int)possiblePlacements [tryThisOne].x,(int)possiblePlacements [tryThisOne].y,rotAttempt);
			}
			else
				break;
		}
		FillInAvailableSquares();
	}

	[SerializeField]
	Text CityNameUi;

	public string GenerateSillyName ()
	{
		CityNameUi.GetComponentInParent<Animator>().Play("city intro");
		string[] start = { "North", "East", "South", "West", "New" };
		string[] noun = { "Bork", "Snork", "Cleft", "Smog", "Crump", "Biggle", "Fuckshit" };
		string[] suffix = { "ington", "shire", " upon thames", "istan", "ville" };
		string output = "";
		if (Random.value > 0.2f)
			output += start [Random.Range(0,start.Length)] + " ";
		output += noun [Random.Range(0,noun.Length)];
		if (Random.value > 0.2f)
			output += suffix [Random.Range(0,suffix.Length)];
		CityNameUi.GetComponentInParent<Animator>().Play("city intro");
		CityNameUi.text = output;
		return output;
	}

	Building PlaceLargeBuilding (Building prefab, int _sizeX, int _sizeY, int _posX, int _posY, int rot)
	{
		Vector3 _spawnPos = new Vector3 (_posX * (buildingChunkSize * 3), 
			                    Random.Range(-heightVaraiation,heightVaraiation), 
			                    -_posY * (buildingChunkSize * 3));
		
		_spawnPos += transform.position;

		Vector3 offset = Vector3.zero;//Offset needed when rotating buildings so the end up in the same position as previously

		Building _newBuilding = (Building)Instantiate(prefab,_spawnPos,Quaternion.Euler(Vector3.up * 90 * rot),transform);
		_newBuilding.standardPos = _newBuilding.transform.position;
		//Rotate new building
		switch (rot) {
			case 1:
				_newBuilding.rotOffset = Vector3.right * buildingChunkSize;
				break;
			case 2:
				_newBuilding.rotOffset = Vector3.right * (buildingChunkSize * 2);
				_newBuilding.rotOffset += Vector3.back * (buildingChunkSize * 2);
				break;
			case 3:
				_newBuilding.rotOffset = Vector3.back * buildingChunkSize;
				break;
		}
		_newBuilding.name = _posX + "," + _posY + ((prefab == buildingPrefabs [0].buildingObject) ? "" : "  " + prefab.name);
		_newBuilding.transform.position = _newBuilding.standardPos + _newBuilding.rotOffset;
		buildings.Add(_newBuilding);

		//Make sure we don't place a building here
		for (int x = 0; x < _sizeX; x++) {
			for (int y = 0; y < _sizeY; y++) {
				switch (rot) {
					case 0:
						availableSquares [_posX + x, _posY + y] = false;
						break;
					case 1:
						availableSquares [_posX - y, _posY + x] = false;
						break;
					case 2:
						availableSquares [_posX - x, _posY - y] = false;
						break;
					case 3:
						availableSquares [_posX + y, _posY - x] = false;
						break;
					default:
						Debug.Log(rot);
						break;
				}
			}
		}
		return _newBuilding;
	}

	int resMultiplier = 1;

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.X)) {
			resMultiplier++;
			if (resMultiplier > 4)
				resMultiplier = 1;
			Screen.SetResolution(240 * resMultiplier,160 * resMultiplier,false);
		}
		if (Input.GetKeyDown(KeyCode.Joystick1Button1))
			GenerateSillyName();

		if (Input.GetKeyDown(KeyCode.Joystick1Button6)) {
			StopCoroutine(GenerateCity());
			StartCoroutine(GenerateCity());
		}
	}

	void ResetSquares ()
	{
		int edgeBuffer = 3;

		availableSquares = new bool[citySizeX,citySizeY];
		possiblePlacements.Clear();
		for (int y = 0; y < citySizeY; y++) {
			for (int x = 0; x < citySizeX; x++) {
				availableSquares [x, y] = true;
				if (x + edgeBuffer < citySizeX && y + edgeBuffer < citySizeY && x - edgeBuffer > 0 && y - edgeBuffer > 0)
					possiblePlacements.Add(new Vector2 (x, y));
			}
		}
	}

	void FillInAvailableSquares ()
	{
		Vector3 prevPos = Vector3.zero;
		List<Vector3> spawnLocations = new List<Vector3> ();
		//Generate a 10x10 grid of 1x1 buildings, not very interesting at all.
		for (int y = 0; y < citySizeY; y++) {
			for (int x = 0; x < citySizeX; x++) {
				if (availableSquares [x, y]) {

					Building b = PlaceLargeBuilding(buildingPrefabs [0].buildingObject,1,1,x,y,0);
					bool spawn = true;

					foreach (Vector3 v in  spawnLocations) {
						if (Vector3.Distance(v,b.transform.localPosition) < 30) {
							spawn = false;
							break;
						}
					}
					if (spawn) {
						spawnLocations.Add(b.transform.localPosition);
						b.SetupSpawner(true);
						if (!ThirdPersonCharacter.instance.lastSpawner)
							ThirdPersonCharacter.instance.lastSpawner = b.spawner;
					}
					else {
						b.SetupSpawner(false);
					}
				}
			}
		}
	}
}

/*[CustomEditor(typeof(WorldGen))]
public class worldGenButtons : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		WorldGen wg = (WorldGen)target;
		if (GUILayout.Button("Generate city")) {
			wg.StartCoroutine(wg.GenerateCity());
		}
		if (GUILayout.Button("Silly name")) {
			Debug.Log(wg.GenerateSillyName());
		}
	}
}*/