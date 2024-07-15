using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Unavinar.LegsWalker
{
	public class JointsAgentController : MonoBehaviour
	{
		[SerializeField] float maxJointForceLimit = 20000;
		[SerializeField] float maxJointSpring = 40000;
		[SerializeField] float jointDampen = 5000;
		[SerializeField] float maxAngularVelocity = 50.0f;
		[SerializeField] float maxLinearVelocity = 50.0f;

		public float MaxJointForceLimit
		{
			get { return maxJointForceLimit; }
		}

		public float MaxJointSpring
		{
			get { return maxJointSpring; }
		}

		public float JointDampen
		{
			get { return jointDampen; }
		}

		Dictionary<Transform, JointPart> _transformToJointPart = new Dictionary<Transform, JointPart>();
		List<JointGroundContact> _groundContacts = new List<JointGroundContact>();
		Agent _agent;

		public void SetAgent(Agent agent)
		{
			_agent = agent;
		}

		public void SetupBodyPart(Transform transform)
		{
			if (transform.TryGetComponent(out ConfigurableJoint configurableJoint))
			{
				JointPart jointPart = new JointPart();
				jointPart.Initialize(this, configurableJoint, transform.GetComponent<Rigidbody>(),
					transform.localPosition, transform.localRotation);
				jointPart.Rigidbody.maxAngularVelocity = maxAngularVelocity;
				jointPart.Rigidbody.maxLinearVelocity = maxLinearVelocity;
				_transformToJointPart.Add(transform, jointPart);
			}

			if (transform.TryGetComponent(out JointGroundContact jointGroundContact))
			{
				jointGroundContact.SetAgent(_agent);
				_groundContacts.Add(jointGroundContact);
			}

		}

		public void ResetJoints()
		{
			foreach (var jointPart in _transformToJointPart.Values)
			{
				jointPart.Reset();
			}
		}

		public void CollectOnGroundJointsObservations(VectorSensor sensor)
		{
			for (int i = 0; i < _groundContacts.Count; i++)
			{
				sensor.AddObservation(_groundContacts[i].TouchingGround);
			}
		}

		public void CollectJointsForceObservations(VectorSensor sensor)
		{
			foreach (var joint in _transformToJointPart.Values)
			{
				sensor.AddObservation(joint.CurrentStrength / maxJointForceLimit);
			}
		}

		public Vector3 GetAverageJointsVelocity()
		{
			Vector3 velSum = Vector3.zero;
			Vector3 avgVel = Vector3.zero;

			int numOfRb = 0;
			foreach (var item in _transformToJointPart.Values)
			{
				numOfRb++;
				velSum += item.Rigidbody.velocity;
			}

			if (numOfRb != 0)
				avgVel = velSum / numOfRb;
			return avgVel;
		}

		public bool HasJointOfTransform(Transform joint) => _transformToJointPart.ContainsKey(joint);

		public void SetJointTargetRotation(Transform joint, float x, float y, float z)
		{
			_transformToJointPart[joint].SetJointTargetRotation(x, y, z);
		}

		public void SetJointStrength(Transform joint, float strength)
		{
			_transformToJointPart[joint].SetJointStrength(strength);
		}
	}
}