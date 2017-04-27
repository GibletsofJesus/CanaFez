using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class CustomPaletteCreator : MonoBehaviour
{
	//Lines around the outside of UI elements to indicate selection
	[SerializeField]
	Image[] selectionIndicators;
	[SerializeField]
	Text saveText, PaletteName;
	[SerializeField]
	Image saveLock, bigBox;
	[SerializeField]
	GameObject everything;
	//4 outlines for each example pixel
	[SerializeField]
	Outline[] selectionOutlines;
	//Hue, Sat and value sliders
	[SerializeField]
	Slider[] colourSliders;
	[SerializeField]
	int UiIndex = 0, pixelIndex;

	//UI Index cheat sheet
	//0. Hue
	//1. Sat
	//2. Lightness
	//3. Palette colours
	//4. Save button
	//5. Palette selection

	[SerializeField]
	AudioClip[] blips;

	Texture2D ActiveTexture;

	// Use this for initialization
	void Start ()
	{
		ActiveTexture = new Texture2D (4, 1, TextureFormat.RGB24, false);
		ActiveTexture.filterMode = FilterMode.Point;
	}

	float moveCD;

	void Update ()
	{
		if (input)
			HandleInput();
	}

	void HandleInput ()
	{
		foreach (Outline o in selectionOutlines) {
			o.effectColor = Color.black;
		}
		moveCD -= moveCD > 0 ? Time.deltaTime : moveCD;
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		//If selection button is pressed
		if (Input.GetButtonDown("Jump")) {

			//Depending on what UI element we have selected
			switch (UiIndex) {
				case 4:
					//The save button
					if (saveLock.enabled) {
						File.WriteAllBytes(Application.dataPath + "/shaders/PaletteSwapping/PaletteTextures/styles/Custom/newTex_" + PaletteSwapLookup.instance.LookupTexture.Count + ".png",ActiveTexture.EncodeToPNG());
						//Don't do shit
					}
					else {
						File.WriteAllBytes(Application.dataPath + "/shaders/PaletteSwapping/PaletteTextures/styles/Custom/newTex_" + PaletteSwapLookup.instance.LookupTexture.Count + ".png",ActiveTexture.EncodeToPNG());
						//save
					}
					break;					
				case 5:					
					//Use this palette and return to the options menu
					//If the user has any unsaved changes to a new or existing palette, prompt them for a save
					//Then ya know, get them to name and save the thing
					StartCoroutine(OpenCloseWindow(false));
					break;
			}
		}

		//If we're trying to move up or down
		if (Mathf.Abs(v) > 0.5f && moveCD <= 0) {
			if (UiIndex != 5) {
				if (UiIndex < 3)
					selectionIndicators [UiIndex].enabled = false;
				else if (UiIndex == 4)
						selectionIndicators [UiIndex].color = Color.white;
			
				moveCD = 0.25f;
				UiIndex += v < 0 ? 1 : -1;
				if (UiIndex > 4)
					UiIndex = 4;
				else if (UiIndex < 0)
						UiIndex = 0;
					else
						SoundManager.instance.playSound(blips [0]);
			
				if (UiIndex < 3)
					selectionIndicators [UiIndex].enabled = true;
				else if (UiIndex == 4)
						selectionIndicators [UiIndex].color = Color.grey;

				ChangeSaveButtonState();
			}
			else if (moveCD <= 0) {
					SoundManager.instance.playSound(blips [0]);
					moveCD = 0.25f;
					PaletteSwapLookup.instance.SetPaletteIndex(v > 0 ? 1 : -1,PaletteName);
					GetTextureSetSliders();
				}
		}
		if (Mathf.Abs(h) > 0.15f) {

			switch (UiIndex) {
				case 0:
					colourSliders [0].value += h * Time.deltaTime / 3;
					UpdateActiveTexture();
					break;
				case 1:
					colourSliders [1].value += h * Time.deltaTime / 3;
					UpdateActiveTexture();
					break;
				case 2:
					colourSliders [2].value += h * Time.deltaTime / 3;
					UpdateActiveTexture();
					break;
				case 3:
					if (moveCD <= 0) {
						moveCD = 0.25f;
						pixelIndex += h > 0 ? 1 : -1;

						SoundManager.instance.playSound(blips [0]);

						if (pixelIndex > 3)
							pixelIndex = 0;
						if (pixelIndex < 0)
							pixelIndex = 3;

						GetTextureSetSliders();
					}
					break;
				case 4:
					if (h < 0) {
						selectionIndicators [UiIndex].color = Color.white;
						moveCD = 0.25f;
						SoundManager.instance.playSound(blips [0]);
						UiIndex++;
						selectionIndicators [UiIndex].enabled = true;
					}
					break;
				case 5:
					if (h > 0) {
						selectionIndicators [UiIndex].enabled = false;
						moveCD = 0.25f;
						SoundManager.instance.playSound(blips [0]);
						UiIndex--;
						selectionIndicators [UiIndex].color = Color.grey;
					}
					break;
			}
			//If we've selected a slider, change slide value
			//If not, if we're selecting the botton 2 UI items, change UI index
		}

		if (UiIndex < 4) {
			if (UiIndex == 3) {
				selectionOutlines [UiIndex + pixelIndex].effectColor = selectionFlash < 6 ? Color.black : new Color (0.9f, 0.9f, 0.9f, 1);
			}
			else {
				selectionOutlines [UiIndex].effectColor = selectionFlash < 6 ? Color.black : new Color (0.9f, 0.9f, 0.9f, 1);
				selectionOutlines [3 + pixelIndex].effectColor = new Color (0.9f, 0.9f, 0.9f, 1);
			}
		}
		else if (UiIndex == 5) {
				selectionOutlines [8].effectColor = selectionFlash < 6 ? Color.black : new Color (0.9f, 0.9f, 0.9f, 1);
			}
			else
				selectionIndicators [UiIndex].color = selectionFlash < 6 ? Color.grey : Color.white;

		if (selectionFlash < 12)
			selectionFlash++;
		else
			selectionFlash = 0;
	}

	int selectionFlash;

	void GetTextureSetSliders ()
	{
		ActiveTexture.SetPixels(PaletteSwapLookup.instance.LookupTexture [PlayerPrefs.GetInt("Palette")].GetPixels());

		float hue, sat, val;
		//Setup the sliders
		Color.RGBToHSV(ActiveTexture.GetPixel(pixelIndex,0),out hue,out sat,out val);
		colourSliders [0].value = hue;
		colourSliders [1].value = sat;
		colourSliders [2].value = val;
	}

	bool input;

	void ChangeSaveButtonState ()
	{
		if (PlayerPrefs.GetInt("Palette") < -10) {
			saveText.enabled = false;
			saveLock.enabled = true;
		}
		else {
			saveText.enabled = true;
			saveLock.enabled = false;
		}
	}

	public IEnumerator OpenCloseWindow (bool open)
	{
		//Disable everything
		everything.SetActive(false);
		input = false;
		UiIndex = 5;
		pixelIndex = 0;
		//Play opening/closing sound
		SoundManager.instance.playSound(blips [open ? 1 : 2]);
		ChangeSaveButtonState();

		//Expand out the big box everything is stored in;
		float lerpy = 0;
		while (lerpy < 1) {
			lerpy += Time.deltaTime;
			bigBox.rectTransform.sizeDelta = new Vector2 (
				Mathf.Lerp(0,238,(open ? lerpy : 1 - lerpy) * 2),
				Mathf.Lerp(4,156,((open ? lerpy : 1 - lerpy) * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}

		if (!open) {
			splashScreenControls.instance.ChangeUIState(splashScreenControls.MenuState.options);
		}
		else {

			GetTextureSetSliders();
			PaletteName.text = PlayerPrefs.GetInt("Palette") + ". " + PaletteSwapLookup.instance.LookupTexture [PlayerPrefs.GetInt("Palette")].name;
			input = true;
			everything.SetActive(true);
		}
	}

	void UpdateActiveTexture ()
	{
		ActiveTexture.SetPixel(pixelIndex,0,Color.HSVToRGB(colourSliders [0].value,colourSliders [1].value,colourSliders [2].value));
		ActiveTexture.Apply();
		PaletteSwapLookup.instance.LookupTexture [PlayerPrefs.GetInt("Palette")] = ActiveTexture;
	}
}
