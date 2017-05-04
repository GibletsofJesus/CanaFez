using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EnterHighscore : MonoBehaviour
{
	public static EnterHighscore instance;
	//Move arrows over whichever character is selected
	[SerializeField]
	RectTransform arrowParent;

	[SerializeField]
	Image mainWindow;
	//Flash whichever one is selected
	[SerializeField]
	Text[] nameInput;
	//Flash this
	[SerializeField]
	Text headerText, UIranks, UInames, UIscores;
	[SerializeField]
	GameObject EnterNameGO;
	[SerializeField]
	public List<KeyValuePair<string, string>> scores = new List<KeyValuePair<string, string>> ();

	[SerializeField]
	AudioClip[] blips;

	public IEnumerator openWindows (bool highscore)
	{
		//Open window sound
		SoundManager.instance.playSound(blips [0]);

		//Extend the main window open
		float lerpy = 0;
		while (lerpy < 1) {
			lerpy += Time.fixedUnscaledDeltaTime;
			mainWindow.rectTransform.sizeDelta = new Vector2 (
				Mathf.Lerp(0,150,lerpy * 2),
				Mathf.Lerp(4,highscore ? 156 : 98,(lerpy * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}

		DisplayScores();

		#region type out all the highscore info
		char[] spliiterChar = { '\r', '\n' };

		//Do some mad shit with typing things out
		string[] ranks = UIranks.text.Split(spliiterChar,10);
		string[] names = UInames.text.Split(spliiterChar,10);
		string[] scores = UIscores.text.Split(spliiterChar,10);

		string _ranks = UIranks.text;
		string _names = UInames.text;
		string _scores = UIscores.text;

		UIscores.text = "";
		UInames.text = "";
		UIranks.text = "";

		bool skip = false;

		for (int i = 0; i < _ranks.Length; i++) {
			UIranks.text += _ranks [i];
			if (Input.GetButtonDown("Jump"))
				skip = true;
			if (!skip)
				yield return new WaitForEndOfFrame ();
		}
		for (int i = 0; i < _names.Length; i++) {
			UInames.text += _names [i];
			if (i < _names.Length - 1)
				UInames.text += _names [i + 1];
			i++;
			if (Input.GetButton("Jump"))
				skip = true;
			if (!skip)
				yield return new WaitForEndOfFrame ();
		}
		for (int i = 0; i < _scores.Length; i++) {
			UIscores.text += _scores [i];
			if (Input.GetButton("Jump"))
				skip = true;
			if (i < _scores.Length - 1)
				UIscores.text += _scores [i + 1];
			i++;
			if (Input.GetButton("Jump"))
				skip = true;
			if (!skip)
				yield return new WaitForEndOfFrame ();
		}
		#endregion

		if (highscore) {
			headerText.gameObject.SetActive(true);
			EnterNameGO.SetActive(true);

			arrowParent.gameObject.SetActive(true);
			yield return StartCoroutine(GetName());
		}

		while (!Input.GetButtonDown("Jump"))
			yield return null;
		
		headerText.gameObject.SetActive(false);
		EnterNameGO.SetActive(false);
		
		//Disable everything
		UIranks.text = "";
		UIscores.text = "";
		UInames.text = "";

		//Close window sound
		SoundManager.instance.playSound(blips [1]);
		//Close the main window 
		lerpy = 0;
		while (lerpy < 1) {
			lerpy += Time.fixedUnscaledDeltaTime;
			mainWindow.rectTransform.sizeDelta = new Vector2 (
				Mathf.Lerp(150,0,(lerpy * 2) - 1),
				Mathf.Lerp(highscore ? 156 : 98,4,lerpy * 2));
			yield return new WaitForEndOfFrame ();
		}
		if (!highscore)
			EndGameUI.instance.currentState = EndGameUI.state.Ready;
	}

	IEnumerator GetName ()
	{
		Image[] arrows = arrowParent.GetComponentsInChildren<Image>();

		string scoreName = "";
		bool naming = true;
		int charIndex = 0;
		float backspaceCD = 0, upCD = 0, sideCD = 0, holdingVert = 0, flash = 0;

		#region sort new score into position
		int scorePlacement = 0;

		for (int i = scores.Count - 1; i > 0; i--) {
			if (UIScoreManager.instance.points > int.Parse(scores [i].Value)) {
				scorePlacement = i;
			}
		}
		#endregion

		while (naming) {
			backspaceCD = backspaceCD > 0 ? backspaceCD - Time.deltaTime : 0;
			upCD = upCD > 0 ? upCD - Time.deltaTime : 0;
			sideCD = sideCD > 0 ? sideCD - Time.deltaTime : 0;

			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");

			#region flash place in leaderboards

			UInames.text = "";
			UIranks.text = "";
			UIscores.text = "";
			for (int i = 0; i < scores.Count; i++) {
				if (i < scorePlacement) {
					UIranks.text += i + 1 + "." + '\n';
					UInames.text += scores [i].Key + '\n';
					UIscores.text += scores [i].Value + '\n';
				}
				else if (i > scorePlacement) {
						UIranks.text += i + 1 + "." + '\n';
						UInames.text += scores [i - 1].Key + '\n';
						UIscores.text += scores [i - 1].Value + '\n';
					}
					else {
						UIranks.text += "<color=" + (flash < 4 ? "#8E8432ff" : "#F9F9F9FF") + ">" + (i + 1) + "." + "</color>" + '\n';
						UInames.text += "<color=" + (flash < 4 ? "#8E8432ff" : "#F9F9F9FF") + ">" + scoreName + "</color>" + '\n';		
						UIscores.text += "<color=" + (flash < 4 ? "#8E8432ff" : "#F9F9F9FF") + ">" + UIScoreManager.instance.points + "</color>" + '\n';	
					}
			}
			#endregion

			#region name input
			#region Backspace || B button
			if ((Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Joystick1Button1)) && backspaceCD <= 0) {
				nameInput [charIndex].text = "_";	
				backspaceCD = 0.2f;
				nameInput [charIndex].color = Color.grey;
				if (charIndex > 0)
					charIndex--;
			}
			#endregion
			else if (Mathf.Abs(h) > 0.4f && sideCD <= 0) {
					#region horizontal
					if (inputString != "a" && inputString != "A" || inputString != "D" || inputString != "d") {
						if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {					
							if (h > 0 && charIndex < nameInput.Length - 1) {
								nameInput [charIndex].color = Color.grey;
								charIndex++;
							}
							if (h < 0 && charIndex > 0) {
								nameInput [charIndex].color = Color.grey;
								charIndex--;
							}
						}
					}
					sideCD = .25f;
					#endregion
				}
				else {
					#region Vertical controller input
					if (Mathf.Abs(v) > 0.35f && upCD <= 0) {
						if (v > 0) {
							if (nameInput [charIndex].text == "_")
								nameInput [charIndex].text = "A";
							else {
								char charVal = nameInput [charIndex].text [0];

								switch (charVal) {
									case '9':
										nameInput [charIndex].text = "A";
										break;
									case 'Z':
										nameInput [charIndex].text = "a";
										break;
									case 'z':
										nameInput [charIndex].text = "0";
										break;
									default:
										nameInput [charIndex].text = "" + (char)System.Convert.ToChar(charVal + 1);
										break;
								}
							}
							arrows [1].color = new Color (.55f, .55f, .55f, 1);
							upCD = holdingVert < 1 ? 0.25f : 0.1f;
						}
						else if (v < 0) {
								if (nameInput [charIndex].text == "_")
									nameInput [charIndex].text = "z";
								else {
									char charVal = nameInput [charIndex].text [0];
									switch (charVal) {
										case '0'://48:
											nameInput [charIndex].text = "z";
											break;
										case 'A'://65:
											nameInput [charIndex].text = "9";
											break;
										case 'a'://97:
											nameInput [charIndex].text = "Z";
											break;
										default:
											nameInput [charIndex].text = "" + (char)System.Convert.ToChar(charVal - 1);
											break;
									}
								}
								arrows [0].color = new Color (.55f, .55f, .55f, 1);
								upCD = holdingVert < 1 ? 0.25f : 0.1f;
							}
						#endregion
					}

					#region keyboard typing
					if (inputString != "#") {
						nameInput [charIndex].text = inputString;	
						nameInput [charIndex].color = Color.grey;
						if (charIndex < nameInput.Length - 1)
							charIndex++;
					}
					#endregion
					if (Mathf.Abs(v) > 0.5f)
						holdingVert += Time.deltaTime;
					else
						holdingVert = 0;

					flash++;
					if (flash > 8)
						flash = 0;

					//Flash character being input
					nameInput [charIndex].color = flash < 4 ? Color.grey : new Color (.9f, .9f, .9f, 1);

					//Also flash the new name in the scoreboard
					arrowParent.anchoredPosition = new Vector2 (-809.5f + (200 * charIndex), -400);

					arrows [0].color = upCD <= 0 ? new Color (.9f, .9f, .9f, 1) : arrows [0].color;
					arrows [1].color = upCD <= 0 ? new Color (.9f, .9f, .9f, 1) : arrows [1].color;

					scoreName = "";
					foreach (Text t in nameInput) {
						if (t.text != "_")
							scoreName += t.text;
					}
					if (Input.GetButton("Jump") && scoreName.Length > 1)
						naming = false;
				
					yield return new WaitForEndOfFrame ();
				}
			#endregion
		}
		arrowParent.gameObject.SetActive(false);

		AddNewScoreToLB("" + UIScoreManager.instance.points,scoreName);

		foreach (Text t in nameInput) {
			if (t.text == "_")
				t.text = " ";
		}
	}

	void Start ()
	{
		instance = this;
		if (CheckScoreFile()) {
			ReadScoreFile();
			SortScores();
		}
		else {
			CreateScoreFile();
		}
	}

	string inputString;

	void Update ()
	{
		if (!instance)
			instance = this;

		if (Input.inputString.Length == 1) {
			if (char.IsLetterOrDigit(Input.inputString [0]) && Input.inputString != inputString)
				inputString = Input.inputString;
			else
				inputString = "#";
		}
		else
			inputString = "#";
	}

	public void DisplayScores ()
	{
		UInames.text = "";
		UIranks.text = "";
		UIscores.text = "";

		for (int i = 0; i < scores.Count; i++) {
			UIranks.text += i + 1 + "." + '\n';
			UInames.text += scores [i].Key + '\n';
			UIscores.text += string.Format("{0:n0}",scores [i].Value) + '\n';
		}
	}

	// Creates a Score file if there isn't one
	void CreateScoreFile ()
	{
		StreamWriter makeFile = File.CreateText("Scores.dat");
		foreach (KeyValuePair<string, string> k in scores) {
			makeFile.WriteLine(k.Key + " " + k.Value);
		}
		makeFile.Close();
	}

	void WriteToFile ()
	{
		StreamWriter write = new StreamWriter ("Scores.dat", false);
		foreach (KeyValuePair<string, string> k in scores) {
			write.WriteLine(k.Key + "#" + k.Value);
		}
		write.Close();
	}

	// Sorts the scores in descending order
	void SortScores ()
	{
		for (int j = 0; j < scores.Count; j++) {
			for (int i = 0; i < scores.Count - 1; i++) {
				if (int.Parse(scores [i].Value) < int.Parse(scores [i + 1].Value)) {
					KeyValuePair<string, string> temp = scores [i];
					scores [i] = scores [i + 1];
					scores [i + 1] = temp;
				}
			}
		}
	}

	// read in the high Score file
	void ReadScoreFile ()
	{
		List<string> fileInput = new List<string> ();
		scores.Clear();
		StreamReader highScores = new StreamReader ("Scores.dat");
		while (!highScores.EndOfStream) {
			fileInput.Add(highScores.ReadLine());
		}

		for (int i = 0; i < fileInput.Count; i++) {
			char delim = '#';
			string entry = fileInput [i].Split(delim) [0];
			string _score = fileInput [i].Split(delim) [1];
			AddToList(_score,entry);
		}
		highScores.Close();
	}

	public void AddNewScoreToLB (string _playerScore, string _playerName)
	{
		AddToList(_playerScore,_playerName);
		SortScores();
		TrimList();
		WriteToFile();
	}

	void TrimList ()
	{
		for (int i = scores.Count - 1; i > 9; i--) {
			scores.RemoveAt(i);
		}
		scores.TrimExcess();
	}

	// Check a high score file exists
	bool CheckScoreFile ()
	{
		if (File.Exists("Scores.dat")) {
			return true;
		}
		else {
			//make one
			return false;
		}
	}

	public bool CheckIfHighScore (int _score)
	{
		for (int i = scores.Count - 1; i > 0; i--) {

			if (int.Parse(scores [i].Value) < _score) {
				return true;
			}
		}
		if (scores.Count < 10) {
			return true;
		}

		return false;
	}

	void AddToList (string _score, string _name)
	{
		KeyValuePair<string, string> newPlayer = new KeyValuePair<string, string> (_name, _score);
		scores.Add(newPlayer);
	}
}