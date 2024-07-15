using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unavinar.LegsWalker
{
	public class TriggerEventObject : MonoBehaviour
	{
		[SerializeField] List<string> allowedTags;

		public Action ON_TRIGGER_ENTER;

		void OnTriggerEnter(Collider other)
		{
			if (allowedTags.Contains(other.tag))
			{
				ON_TRIGGER_ENTER?.Invoke();
			}
		}
	}
}