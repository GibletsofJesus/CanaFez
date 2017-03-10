using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crater : MonoBehaviour
{
	public float maxLifetime = 25, lifetime;
	public Animator anim;
	[SerializeField]
	Projector proj;

	public void Setup (float newMaxLifetime)
	{
		transform.rotation = Quaternion.Euler(new Vector3 (90, Random.Range(-180,180), 0));
		if (anim)
			anim.Play("crater");
		else
			proj.material.SetFloat("_SliceAmount",0);
		lifetime = 0;
		maxLifetime = newMaxLifetime;
	}

	void Update ()
	{
		lifetime += Time.deltaTime;
		if (lifetime >= maxLifetime) {
			CraterManager.instance.ActiveCraters.Remove(gameObject);
			CraterManager.instance.DeadCraters.Add(gameObject);
			if (anim)
				anim.Play("crater_idle");
			gameObject.SetActive(false);
		}

		if (!anim)
			proj.material.SetFloat("_SliceAmount",1.02f - Mathf.Clamp01(lifetime * 3.5f));
	}
}
