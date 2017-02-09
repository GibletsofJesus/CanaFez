using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class PaletteSwapLookup : MonoBehaviour
{
	public Texture[] LookupTexture;
	//public Sprite[] paletteSprites;
	[SerializeField]
	int paletteIndex = 6;
	Material _mat;
	[SerializeField]
	Shader swappingShader;

	void OnEnable ()
	{
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

		/*if (paletteIndex == 0)
            textComp.text = "0. Original";
        else*/
		textComp.text = '\n' + "" + paletteIndex + ". " + LookupTexture [paletteIndex].name;

		PlayerPrefs.SetInt("Palette",paletteIndex);
	}

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Keypad0))
			paletteIndex = 0;
		if (Input.GetKeyDown(KeyCode.Keypad1))
			paletteIndex = 1;
		if (Input.GetKeyDown(KeyCode.Keypad2))
			paletteIndex = 2;
		if (Input.GetKeyDown(KeyCode.Keypad3))
			paletteIndex = 3;
		if (Input.GetKeyDown(KeyCode.Keypad4))
			paletteIndex = 4;
		if (Input.GetKeyDown(KeyCode.Keypad5))
			paletteIndex = 5;
		if (Input.GetKeyDown(KeyCode.Keypad6))
			paletteIndex = 6;
		if (Input.GetKeyDown(KeyCode.Keypad7))
			paletteIndex = 7;
		if (Input.GetKeyDown(KeyCode.Keypad8))
			paletteIndex = 8;
		if (Input.GetKeyDown(KeyCode.Keypad9))
			paletteIndex = 9;

		/*num += Time.deltaTime;
		if (num > 2) {
			StartCoroutine(WorldGen.instance.GenerateCity());
			paletteIndex++;
			if (paletteIndex > LookupTexture.Length - 1)
				paletteIndex = 0;
			num = 0;
		}*/
	}

	//float num = 0;

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
