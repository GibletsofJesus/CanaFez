using UnityEngine;
using System.Collections;

//Create tiny pauses for certain events
//i.e. clashing of beaks, defeat of geese etc.
public class frameHolder : MonoBehaviour
{

	public static frameHolder instance;

	public bool enableFrameholding = true;

	public void holdFrame (float time)
	{
		if (enableFrameholding) {
			holdDuration += time;
			if (Time.timeScale > 0)
				StartCoroutine(hold());
		}
	}

	bool running;
	float holdDuration;

	IEnumerator hold ()
	{
		while (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_PAUSE) {
			Time.timeScale = 0;
			yield return null;
		}

		if (enableFrameholding) {
			Time.timeScale = 0.1f;

			while (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_PAUSE) {
				Time.timeScale = 0;
				yield return null;
			}

			while (holdDuration > 0) {
				holdDuration -= Time.fixedDeltaTime;
				yield return new WaitForEndOfFrame ();
			}
		}
		while (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_PAUSE) {
			Time.timeScale = 0;
			yield return null;
		}
		Time.timeScale = 1;
	}

	// Use this for initialization
	void Start ()
	{
		instance = this;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}