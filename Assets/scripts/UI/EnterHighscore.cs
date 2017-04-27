using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterHighscore : MonoBehaviour
{
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
	AudioClip[] blips;

	public IEnumerator openWindows (bool highscore)
	{
		yield return new WaitForSeconds (1);

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

		#region type out all the highscore info
		char[] spliiterChar = { '\r', '\n' };

		UIranks.enabled = true;
		UIscores.enabled = true;
		UInames.enabled = true;
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

		for (int i = 0; i < _ranks.Length; i++) {
			UIranks.text += _ranks [i];
			yield return new WaitForEndOfFrame ();
		}
		for (int i = 0; i < _names.Length; i++) {
			UInames.text += _names [i];
			if (i < _names.Length - 1)
				UInames.text += _names [i + 1];
			i++;
			yield return new WaitForEndOfFrame ();
		}
		for (int i = 0; i < _scores.Length; i++) {
			UIscores.text += _scores [i];
			if (i < _scores.Length - 1)
				UIscores.text += _scores [i + 1];
			i++;
			yield return new WaitForEndOfFrame ();
		}
		#endregion

		if (highscore) {
			headerText.gameObject.SetActive(true);
			EnterNameGO.SetActive(true);
		}

		arrowParent.gameObject.SetActive(true);

		while (!Input.GetButtonDown("Jump"))
			yield return null;
		
		headerText.gameObject.SetActive(false);
		EnterNameGO.SetActive(false);
		
		//Disable everything
		UIranks.enabled = false;
		UIscores.enabled = false;
		UInames.enabled = false;

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
	}

}
