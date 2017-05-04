using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class CustomPaletteCreator : MonoBehaviour
{
	public static CustomPaletteCreator instance;
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

	[Header("New shit")]
	[SerializeField]
	GameObject theFuckYouBox;
	[SerializeField]
	Text theFuckYouBoxMessage;

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
		instance = this;
		ActiveTexture = new Texture2D (4, 1, TextureFormat.RGB24, false);
		ActiveTexture.filterMode = FilterMode.Point;
	}

	float moveCD;
	string inputString;

	void Update ()
	{
		theFuckYouBox.SetActive(UnlockManager.instance.playerLevel < 10 ? true : false);
		theFuckYouBoxMessage.text = "Unlock <color=grey>" + (10 - UnlockManager.instance.playerLevel) + "</color>" + '\n' +
		"more colour" + '\n' +
		"palettes to" + '\n' +
		"access" + '\n' + '\n' +
		"CUSTOM" + '\n' +
		"PALETTE" + '\n' +
		"MAKER";
		if (!instance)
			instance = this;
		if (input)
			HandleInput();
		
		if (Input.inputString.Length == 1) {
			if (char.IsLetterOrDigit(Input.inputString [0]) && Input.inputString != inputString)
				inputString = Input.inputString;
			else
				inputString = "#";
		}
		else
			inputString = "#";
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

					input = false;
					StartCoroutine(SavePalette());
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

				//ChangeSaveButtonState();
			}
			else if (moveCD <= 0) {
					SoundManager.instance.playSound(blips [0]);
					moveCD = 0.25f;
					PaletteSwapLookup.instance.SetPaletteIndex(v < 0 ? 1 : -1,PaletteName);
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
					if (h > 0 && UnlockManager.instance.playerLevel > 9) {
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
		if (!PlayerPrefs.HasKey("Palette"))
			ActiveTexture.SetPixels(PaletteSwapLookup.instance.LookupTexture [0].GetPixels());
		else {
			Debug.Log(PlayerPrefs.GetInt("Palette"));
			ActiveTexture.SetPixels(PaletteSwapLookup.instance.LookupTexture [PlayerPrefs.GetInt("Palette")].GetPixels());
		}
		float hue, sat, val;
		//Setup the sliders
		Color.RGBToHSV(ActiveTexture.GetPixel(pixelIndex,0),out hue,out sat,out val);
		colourSliders [0].value = hue;
		colourSliders [1].value = sat;
		colourSliders [2].value = val;
	}

	bool input;
	[SerializeField]
	RectTransform SaveBox, arrowParent;
	[SerializeField]
	Image charArrowA, charArrowB;
	[SerializeField]
	Text[] paletteNameInput;
	[SerializeField]
	GameObject EnterNameGO;
	[SerializeField]
	Text headerText;
	string paletteString;

	IEnumerator SavePalette ()
	{
		bool naming = true;
		int charIndex = 0, flash = 0;
		float lerpy = 0, backspaceCD = 0, upCD = 0, sideCD = 0, holdingVert = 0;


		//open a window
		while (lerpy < 1) {
			lerpy += Time.fixedUnscaledDeltaTime;
			SaveBox.sizeDelta = new Vector2 (
				Mathf.Lerp(0,160,lerpy * 2),
				Mathf.Lerp(4,80,(lerpy * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}

		headerText.gameObject.SetActive(true);
		headerText.text = "Enter name for" + '\n' + "new palette";
		EnterNameGO.SetActive(true);
		arrowParent.gameObject.SetActive(true);

		while (naming) {
			backspaceCD = backspaceCD > 0 ? backspaceCD - Time.deltaTime : 0;
			upCD = upCD > 0 ? upCD - Time.deltaTime : 0;
			sideCD = sideCD > 0 ? sideCD - Time.deltaTime : 0;

			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");

			#region name input
			#region Backspace || B button
			if ((Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Joystick1Button1)) && backspaceCD <= 0) {
				paletteNameInput [charIndex].text = "_";	
				backspaceCD = 0.2f;
				paletteNameInput [charIndex].color = Color.grey;
				if (charIndex > 0)
					charIndex--;
			}
			#endregion
			else if (Mathf.Abs(h) > 0.4f && sideCD <= 0) {
					#region horizontal		
					if (inputString != "a" && inputString != "A" || inputString != "D" || inputString != "d") {
						if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {
							if (h > 0 && charIndex < paletteNameInput.Length - 1) {
								paletteNameInput [charIndex].color = Color.grey;	
								charIndex++;
							}
							if (h < 0 && charIndex > 0) {
								paletteNameInput [charIndex].color = Color.grey;								
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
							if (paletteNameInput [charIndex].text == "_")
								paletteNameInput [charIndex].text = "A";
							else {
								char charVal = paletteNameInput [charIndex].text [0];

								switch (charVal) {
									case '9':
										paletteNameInput [charIndex].text = "A";
										break;
									case 'Z':
										paletteNameInput [charIndex].text = "a";
										break;
									case 'z':
										paletteNameInput [charIndex].text = "0";
										break;
									default:
										paletteNameInput [charIndex].text = "" + (char)System.Convert.ToChar(charVal + 1);
										break;
								}
							}
							charArrowB.color = new Color (.55f, .55f, .55f, 1);
							upCD = holdingVert < 1 ? 0.25f : 0.1f;
						}
						else if (v < 0) {
								if (paletteNameInput [charIndex].text == "_")
									paletteNameInput [charIndex].text = "z";
								else {
									char charVal = paletteNameInput [charIndex].text [0];
									switch (charVal) {
										case '0'://48:
											paletteNameInput [charIndex].text = "z";
											break;
										case 'A'://65:
											paletteNameInput [charIndex].text = "9";
											break;
										case 'a'://97:
											paletteNameInput [charIndex].text = "Z";
											break;
										default:
											paletteNameInput [charIndex].text = "" + (char)System.Convert.ToChar(charVal - 1);
											break;
									}
								}
								charArrowA.color = new Color (.55f, .55f, .55f, 1);
								upCD = holdingVert < 1 ? 0.25f : 0.1f;
							}
						#endregion
					}

					#region keyboard typing
					if (inputString != "#") {
						paletteNameInput [charIndex].text = inputString;	
						paletteNameInput [charIndex].color = Color.grey;
						if (charIndex < paletteNameInput.Length - 1)
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
					paletteNameInput [charIndex].color = flash < 4 ? Color.grey : new Color (.9f, .9f, .9f, 1);

					//Also flash the new name in the scoreboard
					arrowParent.anchoredPosition = new Vector2 (-809.5f + (200 * charIndex), -400);

					charArrowA.color = upCD <= 0 ? new Color (.9f, .9f, .9f, 1) : charArrowA.color;
					charArrowB.color = upCD <= 0 ? new Color (.9f, .9f, .9f, 1) : charArrowB.color;

					paletteString = "";
					foreach (Text t in paletteNameInput) {
						if (t.text != "_")
							paletteString += t.text;
					}
					if (Input.GetButton("Jump") && paletteString.Length > 1)
						naming = false;
					else if (Input.GetButton("Jump"))
							SoundManager.instance.playSound(blips [4]);

					yield return new WaitForEndOfFrame ();
				}
			#endregion
		}
		File.WriteAllBytes(Application.dataPath + "/styles/Custom/" +
		PaletteSwapLookup.instance.LookupTexture.Count.ToString("0000")
		+ '#' + paletteString + ".png",ActiveTexture.EncodeToPNG());
		
		PaletteSwapLookup.instance.loadPalettes();
		PaletteSwapLookup.instance.SetPaletteIndex(PaletteSwapLookup.instance.LookupTexture.Count - 1);
		PaletteSwapLookup.instance.SetPaletteIndex(0,PaletteName);


		headerText.text = "Saved";
		arrowParent.gameObject.SetActive(false);

		while (!Input.GetButtonDown("Jump"))
			yield return null;

		headerText.gameObject.SetActive(false);
		EnterNameGO.SetActive(false);

		//close window
		while (lerpy > 0) {
			lerpy -= Time.fixedUnscaledDeltaTime;
			SaveBox.sizeDelta = new Vector2 (
				Mathf.Lerp(0,150,lerpy * 2),
				Mathf.Lerp(4,98,(lerpy * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}
		input = true;
	}

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
		//ChangeSaveButtonState();
		if (open)
			PaletteSwapLookup.instance.loadPalettes();
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
			if (splashScreenControls.instance != null)
				splashScreenControls.instance.ChangeUIState(splashScreenControls.MenuState.options);
			else
				EndGameUI.instance.currentState = EndGameUI.state.Ready;
		}
		else {
			GetTextureSetSliders();
			PaletteSwapLookup.instance.SetPaletteIndex(0,PaletteName);
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
