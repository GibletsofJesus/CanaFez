using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
	public Vector3 direction;
	public float maxLifetime = 25, lifetime;


	public void SetupCloud (Vector3 _direction, float newMaxLifetime)
	{
		endingLife = false;
		transform.localScale = Vector3.one;
		lifetime = 0;
		maxLifetime = newMaxLifetime;
		direction = transform.InverseTransformDirection(_direction);
	}

	void Update ()
	{
		lifetime += Time.deltaTime;
		if (lifetime >= maxLifetime) {
			if (!endingLife) {
				endingLife = true;
				StartCoroutine(endLife());
			}
		}
		if (transform.localPosition.x > 30) {
			lifetime = maxLifetime;
		}

		transform.Translate(direction * Time.deltaTime,Space.Self);
	}

	bool endingLife;

	[SerializeField]
	AnimationCurve deathScale;

	IEnumerator endLife ()
	{
		float lerpy = 0;
		float deathspin = Random.Range(30,100) * (Random.value > .5 ? 1 : -1);
		while (lerpy < 1) {
			lerpy += Time.deltaTime * 0.2f;
			transform.localScale = Vector3.one * deathScale.Evaluate(lerpy);
			Vector3 rot = transform.rotation.eulerAngles;
			Vector3 addRot = Vector3.one * Time.deltaTime * deathspin * 0.2f;
			//transform.rotation = Quaternion.Euler(rot + addRot);
			//transform.localScale = Vector3.Lerp(Vector3.one,Vector3.zero,lerpy);
			yield return new WaitForEndOfFrame ();
		}

		CloudManager.instance.ActiveClouds.Remove(gameObject);
		CloudManager.instance.DeadClouds.Add(gameObject);
		gameObject.SetActive(false);
	}
}
