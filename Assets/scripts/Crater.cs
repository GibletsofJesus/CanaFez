using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crater : MonoBehaviour
{
	public float maxLifetime = 25, lifetime;
	public Animator anim;

	public void Setup (float newMaxLifetime)
	{
		transform.rotation = Quaternion.Euler(new Vector3 (-90, Random.Range(-180,180), 0));
		anim.Play("crater");
		lifetime = 0;
		maxLifetime = newMaxLifetime;
	}

	void Update ()
	{
		lifetime += Time.deltaTime;
		if (lifetime >= maxLifetime) {
			CraterManager.instance.ActiveCraters.Remove(gameObject);
			CraterManager.instance.DeadCraters.Add(gameObject);
			anim.Play("crater_idle");
			gameObject.SetActive(false);
		}
	}
}
