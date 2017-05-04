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

	bool loaded;

	void Start ()
	{
		if (!PlayerPrefs.HasKey("Palette")) {
			PlayerPrefs.SetInt("Palette",0);
			paletteIndex = 0;
		}
		loadPalettes();
		instance = this;
		if (PlayerPrefs.HasKey("Palette")) {
			paletteIndex = PlayerPrefs.GetInt("Palette");
			if (paletteIndex > LookupTexture.Count - 1)
				paletteIndex = LookupTexture.Count - 1;
		}
		if (_mat == null)
			_mat = new Material (swappingShader);
	}

	bool reading;

	public void loadPalettes ()
	{
		if (UnlockManager.instance.playerLevel > 9) {	
			LookupTexture = new List<Texture2D> ();
			reading = true;
			GetStylesFromDirectory("/styles/");
			GetStylesFromDirectory("/styles/Custom");
			GetStylesFromDirectory("ERROR PLEASE");
			reading = false;
		}
	}

	void GetStylesFromDirectory (string extraPath)
	{
		if (Directory.Exists(Application.dataPath + extraPath)) {
			DirectoryInfo dirInf = new  DirectoryInfo (Application.dataPath + extraPath);
			FileInfo[] files = dirInf.GetFiles();
			//Directory.GetFiles(Application.dataPath + extraPath).OrderBy(f=>f_
			foreach (FileInfo f in files) {
				//f.FullName.EndsWith(".psd") ||
				if (f.FullName.EndsWith(".png") || f.FullName.EndsWith(".psd")) {				
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

		int max = UnlockManager.instance.playerLevel > 9 ? LookupTexture.Count - 1 : UnlockManager.instance.playerLevel;

		if (paletteIndex > max)
			paletteIndex = 0;

		if (paletteIndex < 0)
			paletteIndex = max;

		PlayerPrefs.SetInt("Palette",paletteIndex);
		if (textComp)
			textComp.text = paletteIndex + ". " + LookupTexture [paletteIndex].name.Remove(0,LookupTexture [paletteIndex].name.IndexOf('#') + 1);
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
		/*
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
			SetPaletteIndex(9);*/
	}

	void OnDisable ()
	{
		if (_mat != null)
			DestroyImmediate(_mat);
	}

	void OnRenderImage (RenderTexture src, RenderTexture dst)
	{
		if (!reading) {
			_mat.SetTexture("_PaletteTex",LookupTexture [paletteIndex]);
			Graphics.Blit(src,dst,_mat);
		}
	}

}