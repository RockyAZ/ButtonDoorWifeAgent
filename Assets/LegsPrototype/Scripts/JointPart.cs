using UnityEngine;

namespace Unavinar.LegsWalker
{
	public class JointPart
	{
		private ConfigurableJoint _joint;
		private Rigidbody _rigidBody;
		private JointsAgentController _jointsController;

		private Vector3 _startingPos;
		private Quaternion _startingRot;

		private float _currentStrength;

		public float CurrentStrength
		{
			get { return _currentStrength; }
		}

		public Rigidbody Rigidbody
		{
			get => _rigidBody;
		}

		public void Initialize(JointsAgentController jointsController, ConfigurableJoint joint, Rigidbody rigidBody,
			Vector3 startingPos, Quaternion startingRot)
		{
			_joint = joint;
			_rigidBody = rigidBody;
			_startingPos = startingPos;
			_startingRot = startingRot;
			_jointsController = jointsController;
		}

		public void Reset()
		{
			_rigidBody.transform.localPosition = _startingPos;
			_rigidBody.transform.localRotation = _startingRot;

			_rigidBody.velocity = Vector3.zero;
			_rigidBody.angularVelocity = Vector3.zero;
		}

		public void SetJointTargetRotation(float x, float y, float z)
		{
			x = (x + 1f) * 0.5f;
			y = (y + 1f) * 0.5f;
			z = (z + 1f) * 0.5f;

			var xRot = Mathf.Lerp(_joint.lowAngularXLimit.limit, _joint.highAngularXLimit.limit, x);
			var yRot = Mathf.Lerp(-_joint.angularYLimit.limit, _joint.angularYLimit.limit, y);
			var zRot = Mathf.Lerp(-_joint.angularZLimit.limit, _joint.angularZLimit.limit, z);

			_joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
		}

		public void SetJointStrength(float strength)
		{
			var rawVal = (strength + 1f) * 0.5f * _jointsController.MaxJointForceLimit;
			var jd = new JointDrive
			{
				positionSpring = _jointsController.MaxJointSpring,
				positionDamper = _jointsController.JointDampen,
				maximumForce = rawVal
			};
			_joint.slerpDrive = jd;
			_currentStrength = jd.maximumForce;
		}
	}
}