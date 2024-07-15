using Unity.MLAgents;
using UnityEngine;

namespace Unavinar.LegsWalker
{
	public class JointGroundContact : MonoBehaviour
	{
		private const string kGroundTag = "Ground";

		[SerializeField] bool penalizeGroundContact;
		[SerializeField] bool endEpizodeOnGroundContact;
		[SerializeField] float groundContactPenalty;

		bool _touchingGround;
		Agent _agent;

		public bool TouchingGround
		{
			get => _touchingGround;
		}

		public void SetAgent(Agent agent)
		{
			_agent = agent;
		}

		void OnCollisionEnter(Collision collision)
		{
			if (collision.transform.CompareTag(kGroundTag))
				_touchingGround = true;
			if (penalizeGroundContact)
				_agent.SetReward(groundContactPenalty);
			if (endEpizodeOnGroundContact)
				_agent.EndEpisode();
		}

		void OnCollisionExit(Collision collision)
		{
			if (collision.transform.CompareTag(kGroundTag))
				_touchingGround = false;
		}
	}
}