using System;
using UnityEngine;


namespace UnityStandardAssets.Utility
{
	public class FollowTarget : MonoBehaviour
	{
		public Transform target;
		public Vector3 offset = new Vector3 (0f, 7.5f, 0f);
		public int snap = 4;
		public bool mimicRot, specialRot;

		private void Update ()
		{
			transform.position = target.position + offset;
			if (snap > 0) {
				transform.position = new Vector3 (
					((int)transform.position.x / snap) * snap,
					((int)transform.position.y / snap) * snap,
					((int)transform.position.z / snap) * snap
				);
			}
			if (mimicRot)
				transform.rotation = target.rotation;

			if (specialRot)
				transform.rotation = Quaternion.Euler(0,target.transform.localRotation.eulerAngles.y,0);
		}
	}
}
