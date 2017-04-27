using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[ExecuteInEditMode]
public class PaletteSwapLookup : MonoBehaviour
{
	public static PaletteSwapLookup instance;
	public List<Texture2D> LookupTexture = new List<Texture2D> ();
	//public Sprite[] paletteSprites;
	[SerializeField]
	int paletteIndex = 6;
	Material _mat;
	[SerializeField]
	Shader swappingShader;

	void OnEnable ()
	{
		LookupTexture = new List<Texture2D> ();
		GetStylesFromDirectory("/shaders/PaletteSwapping/PaletteTextures/styles/");
		GetStylesFromDirectory("/shaders/PaletteSwapping/PaletteTextures/styles/New batch");
		GetStylesFromDirectory("/shaders/PaletteSwapping/PaletteTextures/styles/Custom");
		GetStylesFromDirectory("ERROR PLEASE");
		instance = this;
		if (PlayerPrefs.HasKey("Palette"))
			paletteIndex = PlayerPrefs.GetInt("Palette");
		if (_mat == null)
			_mat = new Material (swappingShader);
	}

	void GetStylesFromDirectory (string extraPath)
	{
		if (Directory.Exists(Application.dataPath + extraPath)) {
			DirectoryInfo dirInf = new  DirectoryInfo (Application.dataPath + extraPath);
			FileInfo[] files = dirInf.GetFiles();
			foreach (FileInfo f in files) {
				//f.FullName.EndsWith(".psd") ||
				if (f.FullName.EndsWith(".png")) {				
					Texture2D newTex = new  Texture2D (4, 1, TextureFormat.RGB24, false);
					newTex.filterMode = FilterMode.Point;
					newTex.LoadImage(File.ReadAllBytes(f.FullName));
					newTex.name = f.Name.Remove(f.Name.IndexOf('.'),4);
					LookupTexture.Add(newTex);
				}
			}
		}
	}

	public void SetPaletteIndex (int upDown, Text textComp)
	{
		paletteIndex -= upDown;
		if (paletteIndex > LookupTexture.Count - 1)
			paletteIndex = 0;

		if (paletteIndex < 0)
			paletteIndex = LookupTexture.Count - 1;

		PlayerPrefs.SetInt("Palette",paletteIndex);
		if (textComp)
			textComp.text = paletteIndex + ". " + LookupTexture [paletteIndex].name;
	}

	public void SetPaletteIndex (int newIndex)
	{
		paletteIndex = newIndex;
		PlayerPrefs.SetInt("Palette",paletteIndex);
	}

	void Update ()
	{
		if (!instance)
			instance = this;

		if (Input.GetKeyDown(KeyCode.KeypadPlus))
			SetPaletteIndex(1,null);
		if (Input.GetKeyDown(KeyCode.KeypadMinus))
			SetPaletteIndex(-1,null);

		if (Input.GetKeyDown(KeyCode.Keypad0))
			SetPaletteIndex(0);
		if (Input.GetKeyDown(KeyCode.Keypad1))
			SetPaletteIndex(1);
		if (Input.GetKeyDown(KeyCode.Keypad2))
			SetPaletteIndex(2);
		if (Input.GetKeyDown(KeyCode.Keypad3))
			SetPaletteIndex(3);
		if (Input.GetKeyDown(KeyCode.Keypad4))
			SetPaletteIndex(4);
		if (Input.GetKeyDown(KeyCode.Keypad5))
			SetPaletteIndex(5);
		if (Input.GetKeyDown(KeyCode.Keypad6))
			SetPaletteIndex(6);
		if (Input.GetKeyDown(KeyCode.Keypad7))
			SetPaletteIndex(7);
		if (Input.GetKeyDown(KeyCode.Keypad8))
			SetPaletteIndex(8);
		if (Input.GetKeyDown(KeyCode.Keypad9))
			SetPaletteIndex(9);
	}

	void OnDisable ()
	{
		if (_mat != null)
			DestroyImmediate(_mat);
	}

	void OnRenderImage (RenderTexture src, RenderTexture dst)
	{
		_mat.SetTexture("_PaletteTex",LookupTexture [paletteIndex]);
		Graphics.Blit(src,dst,_mat);
	}

}
