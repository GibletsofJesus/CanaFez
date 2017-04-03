using UnityEngine;
using UnityEngine.UI;

public class flash : MonoBehaviour
{

	public SpriteRenderer sr;
	public Image img;
	public Text t;
	public TextMesh tm;
	public LineRenderer[] lrs;
	bool flashOn;
	int i = 0;
	public int flashSpeed;

	[Header("TextMesh Colours")]
	public Color[] meshCols;

	void FixedUpdate ()
	{
		if (flashOn) {
			if (tm)
				tm.color = tm.color == meshCols [0] ? meshCols [1] : meshCols [0];
			if (sr)
				sr.enabled = !sr.enabled;
			if (img)
				img.enabled = !img.enabled;
			if (t)
				t.enabled = !t.enabled;
			foreach (LineRenderer lr  in lrs) {
				lr.startColor = lr.startColor == meshCols [0] ? meshCols [1] : meshCols [0];
				lr.endColor = lr.endColor == meshCols [0] ? meshCols [1] : meshCols [0];
			}
			flashOn = false;
		}
		else {
			i++;
			if (i > flashSpeed) {
				flashOn = true;
				i = 0;
			}
		}        
	}
}
