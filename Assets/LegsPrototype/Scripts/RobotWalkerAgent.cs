using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Unavinar.LegsWalker
{
	public class RobotWalkerAgent : Agent
	{
		[SerializeField] Transform bodyTransform;
		[SerializeField] Transform orientationCube;
		[SerializeField] Transform[] joints;
		[SerializeField] JointsAgentController jointsAgentController;
		[SerializeField] RobotWalkerPlaygroundController playgroundController;
		[SerializeField] float targetSpeed;
		[SerializeField] float downRayCastDistance = 10;
		[SerializeField] TriggerEventObject triggerEventObject;
		[Space] [SerializeField] [Range(0, 1)] private float velocityAlignmentRewardMultiplier;
		[Space] [SerializeField] [Range(0, 1)] private float upwardAlignmentRewardMultiplier;
		[Space] [SerializeField] [Range(0, 1)] private float upwardAlignmentNegativeRewardMultiplier;

		private Rigidbody _bodyRB;
		private Vector3 _startBodyRotation;

		protected override void OnEnable()
		{
			base.OnEnable();
			triggerEventObject.ON_TRIGGER_ENTER += OnTargetTouch;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			triggerEventObject.ON_TRIGGER_ENTER -= OnTargetTouch;
		}

		public override void Initialize()
		{
			playgroundController.Initialize();
			jointsAgentController.SetAgent(this);

			foreach (var jointTransform in joints)
			{
				jointsAgentController.SetupBodyPart(jointTransform);
			}

			_bodyRB = bodyTransform.GetComponent<Rigidbody>();
			_startBodyRotation = bodyTransform.eulerAngles;
		}

		public override void OnEpisodeBegin()
		{
			_bodyRB.angularVelocity = Vector3.zero;
			_bodyRB.velocity = Vector3.zero;
			bodyTransform.rotation =
				Quaternion.Euler(_startBodyRotation.x, Random.Range(0, 180f), _startBodyRotation.z);

			jointsAgentController.ResetJoints();
			playgroundController.ResetScene();
		}

		//TODO add button to calculate observation amount and action amount

		// amount of observations without legs: 12
		// total amount of observations formula: 12 + [number of GroundContact] + [number of JointPart]
		public override void CollectObservations(VectorSensor sensor)
		{
			jointsAgentController.CollectOnGroundJointsObservations(sensor); // [number of GroundContact]

			jointsAgentController.CollectJointsForceObservations(sensor); // [number of JointPart]

			Vector3 averageVelocity = jointsAgentController.GetAverageJointsVelocity();
			Vector3 velocityToMatch = orientationCube.forward * targetSpeed;
			sensor.AddObservation(Vector3.Distance(averageVelocity, velocityToMatch)); // 1
			sensor.AddObservation(Vector3.Dot(bodyTransform.forward, orientationCube.forward)); // 1

			sensor.AddObservation(orientationCube.transform.InverseTransformDirection(averageVelocity)); // 3
			sensor.AddObservation(orientationCube.transform.InverseTransformDirection(velocityToMatch)); // 3
			sensor.AddObservation(
				orientationCube.transform.InverseTransformPoint(playgroundController.TargetPosition)); // 3

			RaycastHit hit;
			if (Physics.Raycast(bodyTransform.position, Vector3.down, out hit, downRayCastDistance))
			{
				sensor.AddObservation(hit.distance / downRayCastDistance); // 1
			}
			else
			{
				sensor.AddObservation(1);
			}
		}

		// continuous actions required => jointsAmount * 3
		public override void OnActionReceived(ActionBuffers actionBuffers)
		{
			var continuousActions = actionBuffers.ContinuousActions;
			var i = -1;

			foreach (var joint in joints)
			{
				if (!jointsAgentController.HasJointOfTransform(joint))
					continue;
				jointsAgentController.SetJointTargetRotation(joint, continuousActions[++i], continuousActions[++i], 0);
				jointsAgentController.SetJointStrength(joint, continuousActions[++i]);
			}

			float upwardReward = Vector3.Dot(bodyTransform.up, Vector3.up);
			if (upwardReward < 0)
				AddReward(upwardReward * upwardAlignmentNegativeRewardMultiplier);
			else
				AddReward(upwardReward * upwardAlignmentRewardMultiplier);
			CalculateReward();
		}

		void FixedUpdate()
		{
			UpdateOrientationCube();
		}

		void CalculateReward()
		{
			var cubeForward = orientationCube.transform.forward;

			Vector3 averageVelocity = jointsAgentController.GetAverageJointsVelocity();
			var matchSpeedReward = GetMatchingVelocityReward(cubeForward * targetSpeed, averageVelocity);

			//float reward = velocityAlignmentRewardMultiplier * matchSpeedReward * lookAtTargetReward;
			float dotDeviation = Vector3.Dot(cubeForward, averageVelocity.normalized);
			AddReward(dotDeviation);
			float newValue = (matchSpeedReward * 2) - 1; // in range -1 1
			AddReward(newValue);
		}

		// Return value will approach 1 if it matches perfectly and approach zero as it deviates
		public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
		{
			var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, targetSpeed);
			return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / targetSpeed, 2), 2);
		}

		void Update()
		{
			Debug.DrawRay(orientationCube.position, orientationCube.forward * 10, Color.cyan);
			Debug.DrawRay(bodyTransform.position, bodyTransform.up * 10, Color.cyan);
		}

		public void UpdateOrientationCube()
		{
			var dirVector = playgroundController.TargetPosition - orientationCube.position;
			dirVector.y = 0;
			var lookRot =
				dirVector == Vector3.zero
					? Quaternion.identity
					: Quaternion.LookRotation(dirVector);

			orientationCube.SetPositionAndRotation(bodyTransform.position, lookRot);
		}

		void OnTargetTouch()
		{
			AddReward(1);
			EndEpisode();
			Debug.Log("OnTargetTouch");
		}
	}
}