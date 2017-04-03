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
		public float placementProbability;
	}

	[System.Serializable]
	class roadIntersections
	{
		public bool up, down, left, right;
	}

	public static WorldGen instance;
	[HideInInspector]
	public bool generating;

	[Header("Building settings")]
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
	private int resMultiplier = 1;

	[Header("Roads")]
	public bool generateRoads;
	[SerializeField]
	float m_GroundLevel;
	/*roadIntersections[,] roadPoints;
	[SerializeField]
	GameObject[] intersectionPrefabs;
	[SerializeField]
	GameObject roadPrefab;*/

	[Header("Parks")]
	[SerializeField]
	GameObject[] parkPrefabs;

	[Header("UI reference(s)")]
	[SerializeField]
	Text CityNameUi;

	public int GetCompletionRating ()
	{
		return Mathf.RoundToInt(((float)minimapCapture.instance.captures.Count / (float)buildings.Count) * 100f);
	}

	void Start ()
	{
		instance = this;
		StartCoroutine(GenerateCity());
	}

	void Update ()
	{
		//Change screen resolution
		if (Input.GetKeyDown(KeyCode.X)) {
			resMultiplier++;
			if (resMultiplier > 4)
				resMultiplier = 1;
			Screen.SetResolution(240 * resMultiplier,160 * resMultiplier,false);
		}
	}

	public IEnumerator GenerateCity ()
	{
		generating = true;
		GameObject allBuildings = new GameObject ("Buildings");
		allBuildings.transform.parent = transform;
		allBuildings.transform.localScale = Vector3.one;
		allBuildings.transform.localPosition = Vector3.zero;

		PlayerCharacter.instance.lastSpawner = null;
		worldObjects.rotation = Quaternion.Euler(Vector3.zero);
		foreach (Building b in buildings) {
			Destroy(b.gameObject);
		}
		buildings.Clear();
		ResetSquares();

		/*roadPoints = new roadIntersections[citySizeX + 1,citySizeY + 1];

		for (int y = 0; y < citySizeY + 1; y++) {
			for (int x = 0; x < citySizeX + 1; x++) {
				roadPoints [x, y] = new roadIntersections ();
			}
		}*/

		#region parks
		//Place me some parks bb
		int parkX = Random.Range(3,8), parkY = Random.Range(3,6);
		int startx = Random.Range(0,citySizeX - parkX - 1), starty = Random.Range(0,citySizeY - parkY - 1);
		for (int i = startx; i < startx + parkX; i++) {
			for (int j = starty; j < starty + parkY; j++) {
				availableSquares [i, j] = false;
			}
		}

		//Now create a cube where we want the park
		Vector3 _spawnPos = new Vector3 (
			                    (startx + (((float)parkX - .25f) / 2f)) * (buildingChunkSize * 3f), 
			                    -32.5f, 
			                    -(starty + (((float)parkY - .25f) / 2f)) * (buildingChunkSize * 3f));
		GameObject g = Instantiate(parkPrefabs [0],transform.position + _spawnPos,Quaternion.Euler(Vector3.zero),transform);

		_spawnPos = new Vector3 (startx * (buildingChunkSize * 3f), 
			-33, 
			-(starty * (buildingChunkSize * 3f)));

		
		g.name = "park norm + spawnpos";
		//g.transform.position = transform.position + _spawnPos + OOFset;
		g.transform.localScale = new Vector3 ((parkX - 0.25f) * buildingChunkSize * 1.5f, .5f, (parkY - .25f) * buildingChunkSize * 1.5f);
		//Then fill that place with trees


		#endregion

		#region place large buildings
		for (int i = 0; i < density; i++) {
			//Which building to place
			//This will eventually be much better system, but it'll do FOR NOW
			int index = Random.Range(1,buildingPrefabs.Length);

			while (Random.value > buildingPrefabs [index].placementProbability)
				index = Random.Range(1,buildingPrefabs.Length);

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
					yield return PlaceBuilding(buildingPrefabs [index].buildingObject,
						buildingPrefabs [index].sizeX,
						buildingPrefabs [index].sizeY,
						(int)possiblePlacements [tryThisOne].x,
						(int)possiblePlacements [tryThisOne].y,
						rotAttempt,allBuildings.transform);
			}
			else
				break;
		}
		#endregion

		//Fill in the rest of the city
		yield return StartCoroutine(FillInAvailableSquares(allBuildings.transform));
		if (generateRoads)
			StartCoroutine(RoadPlacement());
		foreach (Building b in buildings) {
			//b.gameObject.SetActive(false);
		}
		generating = false;
	}

	IEnumerator RoadPlacement ()
	{
		yield return null;
		/*
		GameObject allRoads = new GameObject ("Roads");
		allRoads.transform.parent = transform;
		allRoads.transform.localScale = Vector3.one;
		allRoads.transform.localPosition = Vector3.zero;

		for (int x = 0; x < citySizeX + 1; x++) {
			for (int y = 0; y < citySizeY + 1; y++) {
				Vector3 _spawnPos = new Vector3 (x * (buildingChunkSize * 3),
					                    -32.5f,
					                    -y * (buildingChunkSize * 3));
				_spawnPos += Vector3.forward * 3.5f;
				_spawnPos += Vector3.left * 3.5f;
				_spawnPos += transform.position;

				//If an intersection only has 2 directions and they're the opposite of one another it's just a straight line
				//So don't put a thing down, just make an extra long road.

				int prefabToUse = (roadPoints [x, y].up ? 1 : 0) +
				                  (roadPoints [x, y].down ? 1 : 0) +
				                  (roadPoints [x, y].left ? 1 : 0) +
				                  (roadPoints [x, y].right ? 1 : 0);
				prefabToUse -= 2;

				Quaternion RoadRot = Quaternion.Euler(Vector3.zero);

				switch (prefabToUse) {
					case 0:

						if ((roadPoints [x, y].left && roadPoints [x, y].right) || (roadPoints [x, y].up && roadPoints [x, y].down)) {
							prefabToUse = -5;
							break;
						}

						if (roadPoints [x, y].left && roadPoints [x, y].up)
							RoadRot = Quaternion.Euler(Vector3.up * 90);
						if (roadPoints [x, y].down && roadPoints [x, y].left)
							RoadRot = Quaternion.Euler(Vector3.up * 180);
						if (roadPoints [x, y].right && roadPoints [x, y].down)
							RoadRot = Quaternion.Euler(Vector3.up * 270);
						break;
					case 1: 
						if (!roadPoints [x, y].down)
							RoadRot = Quaternion.Euler(Vector3.up * 90);
						if (!roadPoints [x, y].right)
							RoadRot = Quaternion.Euler(Vector3.up * 180);
						if (!roadPoints [x, y].up)
							RoadRot = Quaternion.Euler(Vector3.up * 270);
						break;
				}

				if (prefabToUse >= 0) {
					GameObject _newIntersection = Instantiate(intersectionPrefabs [prefabToUse],_spawnPos,RoadRot,allRoads.transform) as GameObject;
				}

				if (roadPoints [x, y].right) {
					GameObject _newSegment = Instantiate(roadPrefab,_spawnPos,Quaternion.Euler(Vector3.up * 90),allRoads.transform) as GameObject;
					_newSegment.transform.localScale = Vector3.right * 0.75f + Vector3.one;
					_newSegment.transform.localPosition += (Vector3.back * 5);
				}
				if (roadPoints [x, y].down) {
					GameObject _newSegment = Instantiate(roadPrefab,_spawnPos,Quaternion.Euler(Vector3.zero),allRoads.transform) as GameObject;
					_newSegment.transform.localScale = Vector3.right * 0.75f + Vector3.one;
					_newSegment.transform.localPosition += (Vector3.right * 5);
				}
			}
		}*/
	}


	#region Building placement

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

	Building PlaceBuilding (Building prefab, int _sizeX, int _sizeY, int _posX, int _posY, int rot, Transform _parent)
	{
		#region Determine spawn position of building
		Vector3 _spawnPos = new Vector3 (_posX * (buildingChunkSize * 3), 
			                    ((int)Mathf.Round(Random.Range(-heightVaraiation,heightVaraiation) / 4) * 4), 
			                    -_posY * (buildingChunkSize * 3));


		_spawnPos += transform.position;
		Vector3 offset = Vector3.zero;//Offset needed when rotating buildings so the end up in the same position as previously
		#endregion

		Building _newBuilding = (Building)Instantiate(prefab,_spawnPos,Quaternion.Euler(Vector3.up * 90 * rot),_parent);
		_newBuilding.standardPos = _newBuilding.transform.position;

		#region rotate building
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
		#endregion

		_newBuilding.transform.position = _newBuilding.standardPos + _newBuilding.rotOffset;
		buildings.Add(_newBuilding);

		#region do road things
		/*for (int x = 0; x < _sizeX + 1; x++) {
			for (int y = 0; y < _sizeY + 1; y++) {

				int xRotOffset = 0, yRotOffset = 0;
				switch (rot) {
					case 0:
						xRotOffset = _posX + x;
						yRotOffset = _posY + y;
						break;
					case 1:
						xRotOffset = _posX - y + 1;
						yRotOffset = _posY + x;
						break;
					case 2:
						xRotOffset = _posX - x + 1;
						yRotOffset = _posY - y + 1;
						break;
					case 3:
						xRotOffset = _posX + y;
						yRotOffset = _posY - x + 1;
						break;
				}

				roadPoints [xRotOffset, yRotOffset].left = ((x == 0 || x == _sizeX) && y != 0
					? true : roadPoints [xRotOffset, yRotOffset].left);
				
				roadPoints [xRotOffset, yRotOffset].right = ((x == 0 || x == _sizeX) && y != _sizeY
					? true : roadPoints [xRotOffset, yRotOffset].right);
				
				roadPoints [xRotOffset, yRotOffset].down = ((y == 0 || y == _sizeY) && x != _sizeX
					? true : roadPoints [xRotOffset, yRotOffset].down);

				roadPoints [xRotOffset, yRotOffset].up = ((y == 0 || y == _sizeY) && x != 0
					? true : roadPoints [xRotOffset, yRotOffset].up);
			}
		}*/
		_newBuilding.SetPavementHeight(m_GroundLevel);
		#endregion

		Vector3 centre = transform.position
		                 + new Vector3 (buildingChunkSize * citySizeX * 1.5f, 0, -buildingChunkSize * citySizeY * 1.5f);
		//See if this is outside a circle thing
		float requiredDistance = buildingChunkSize * citySizeX * (1.5f + Random.Range(-0.2f,0.2f) - ((_sizeX * _sizeY) / 15));

		if (Vector3.Distance(centre,_newBuilding.transform.position) > requiredDistance)
			_newBuilding.gameObject.SetActive(false);
		else {
			#region Make sure we don't place a building here
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
			#endregion

		}

		//Doing this to setup the upgrade zone animator to manager
		_newBuilding.SetupSpawner(false);

		return _newBuilding;
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

	IEnumerator FillInAvailableSquares (Transform _parent)
	{
		Vector3 prevPos = Vector3.zero;
		List<Animator> spawners = new List<Animator> ();

		//Generate a 10x10 grid of 1x1 buildings, not very interesting at all.
		for (int y = 0; y < citySizeY; y++) {
			for (int x = 0; x < citySizeX; x++) {
				if (availableSquares [x, y]) {

					Building b = PlaceBuilding(buildingPrefabs [0].buildingObject,1,1,x,y,0,_parent);
					bool spawn = true;

					foreach (Animator a in  spawners) {
						if (Vector3.Distance(a.transform.position,b.transform.position) < 60) {
							spawn = false;
							break;
						}
					}
					if (spawn) {
						spawners.Add(b.SetupSpawner(true));
					}
					else {
						b.SetupSpawner(false);
					}
				}
			}
		}

		if (!PlayerCharacter.instance.lastSpawner) {
			PlayerCharacter.instance.lastSpawner = spawners [spawners.Count / 2];
		}
		yield return null;
	}

	#endregion

	#region Personisation generation

	public string GenerateSillyName ()
	{
		string output = "";
		CityNameUi.text = "YOU SHOULDN'T BE SEEING THIS";
		while (CityNameUi.text.Length > 13) {
			output = "";
			CityNameUi.GetComponentInParent<Animator>().Play("city intro");
			string[] start = { "North", "East", "South", "West", "New" };
			string[] noun = { "Bork", "Snork", "Cleft", "Smog", "Crump", "Biggle", "Fuckshit" };
			string[] suffix = { "ington", "shire", " upon thames", "istan", "ville" };
			if (Random.value > 0.2f)
				output += start [Random.Range(0,start.Length)] + " ";
			output += noun [Random.Range(0,noun.Length)];
			if (Random.value > 0.2f)
				output += suffix [Random.Range(0,suffix.Length)];
			CityNameUi.GetComponentInParent<Animator>().Play("city intro");
			CityNameUi.text = output;
		}
		sillyname = output;
		return output;
	}

	string sillyname;

	public string GetCurrentCityName ()
	{
		return sillyname;
	}

	#endregion
}