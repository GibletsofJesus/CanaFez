using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class PaletteSwapLookup : MonoBehaviour
{
	public static PaletteSwapLookup instance;
	public Texture[] LookupTexture;
	//public Sprite[] paletteSprites;
	[SerializeField]
	int paletteIndex = 6;
	Material _mat;
	[SerializeField]
	Shader swappingShader;

	void OnEnable ()
	{
		instance = this;
		if (PlayerPrefs.HasKey("Palette"))
			paletteIndex = PlayerPrefs.GetInt("Palette");
		if (_mat == null)
			_mat = new Material (swappingShader);
	}

	public void SetPaletteIndex (int upDown, Text textComp)
	{
		paletteIndex -= upDown;
		if (paletteIndex > LookupTexture.Length - 1)
			paletteIndex = 0;

		if (paletteIndex < 0)
			paletteIndex = LookupTexture.Length - 1;

		PlayerPrefs.SetInt("Palette",paletteIndex);
		textComp.text = "" + paletteIndex;
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
