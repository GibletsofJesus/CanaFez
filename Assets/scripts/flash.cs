using UnityEngine;
using UnityEngine.UI;

public class flash : MonoBehaviour
{

	public SpriteRenderer sr;
	public Image img;
	public Text t;
	bool flashOn;
	int i = 0;
	public int flashSpeed;

	void FixedUpdate ()
	{
		if (flashOn) {
			if (sr)
				sr.enabled = !sr.enabled;
			if (img)
				img.enabled = !img.enabled;
			if (t)
				t.enabled = !t.enabled;
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
