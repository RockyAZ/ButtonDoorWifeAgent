using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ButtonEscaperAgent : Agent
{
	private const string kButtonTag = "button";
	private const string kWifeTag= "wife";
	private const float kDistToCastFallCheck = 20f;
	private const float kJumpDelay = 2f;

	[SerializeField] private PlaygroundController _playgroundController;
	[SerializeField] private float _jumpPower;
	[SerializeField] private float _walkSpeed;
	[SerializeField] private TrailRenderer _trail;
	[SerializeField] private ParticleSystem _jumpParticle;
	[SerializeField] private ParticleSystem _wifeParticle;
	[Space] 
	[SerializeField] private bool _additionalButtonDirectionReward;
	[SerializeField] private bool _additionalWifeDirectionReward;
	[SerializeField] private bool _additionalEachStepNegativeReward;

	private bool _pressedButtonInEpisod = false;
	private Rigidbody _rb;
	private float _currentJumpDelayTimer = 0;

	public override void Initialize()
	{
		_rb = GetComponent<Rigidbody>();
	}

	public override void OnEpisodeBegin()
	{
		_rb.velocity = Vector3.zero;
		_playgroundController.ResetScene();
		_pressedButtonInEpisod = false;
		_trail.Clear();
	}

	//total 7 observations
	public override void CollectObservations(VectorSensor sensor)
	{
		//is button pressed
		//1
		sensor.AddObservation(_pressedButtonInEpisod);

		//direction to button
		//3
		Vector3 dirToButton = _playgroundController.ButtonPosition - transform.position;
		sensor.AddObservation(dirToButton.normalized);

		//direction to finish
		//3
		Vector3 dirToWife = _playgroundController.WifePosition - transform.position;
		sensor.AddObservation(dirToWife.normalized);
	}

	void Update()
	{
		Debug.DrawRay(transform.position, transform.forward * 5, Color.yellow);
	}

	//branches size
	//      0         1         2
	//0{3}-not move, forward, backward
	//1{3}-not rotate, rotate right, rotate left
	//2{2}-stay, jump
	public override void OnActionReceived(ActionBuffers actions)
	{
		float moving = 0;
		Vector3 rotateDir = Vector3.zero;

		//forward backward moving
		switch (actions.DiscreteActions[0])
		{
			case 1: moving = 1; break;
			case 2: moving = -1; break;
		}

		//left right rotation
		switch (actions.DiscreteActions[1])
		{
			case 1: rotateDir = transform.up; break;
			case 2: rotateDir = -transform.up; break;
		}

		transform.Rotate(rotateDir, Time.fixedDeltaTime * 300f);
		Vector3 movingForce = transform.forward * moving;
		movingForce.Normalize();
		_rb.AddForce(movingForce * _walkSpeed, ForceMode.VelocityChange);

		//jump
		Vector3 jumpPower = Vector3.zero;
		if (actions.DiscreteActions[2] == 1 && _currentJumpDelayTimer <= 0)
		{
			_currentJumpDelayTimer = kJumpDelay;
			jumpPower.y = _jumpPower;
			_jumpParticle.Play();

			if (!_pressedButtonInEpisod && _additionalButtonDirectionReward)
			{
				Vector3 dirToButton = _playgroundController.ButtonPosition - transform.position;
				AddReward(0.1f * Vector3.Dot(dirToButton.normalized, transform.forward.normalized));
			}
		}
		_rb.AddForce(jumpPower, ForceMode.Impulse);

		if (_pressedButtonInEpisod && _additionalWifeDirectionReward)
		{
			Vector3 vectorToWife = (_playgroundController.WifePosition - transform.position).normalized;
			AddReward(0.001f * Vector3.Dot(_rb.velocity.normalized, vectorToWife));
		}

		//check for failing
		if (!Physics.Raycast(transform.position, Vector3.down, kDistToCastFallCheck))
		{
			Debug.DrawRay(transform.position, Vector3.down * kDistToCastFallCheck, Color.red, 10);
			SetReward(-1);
			EndEpisode();
		}
		else
		{
			Debug.DrawRay(transform.position, Vector3.down * kDistToCastFallCheck, Color.green);
		}

		if (_currentJumpDelayTimer > 0)
			_currentJumpDelayTimer -= Time.fixedDeltaTime;

		//additional negative reward after N iteration to make agent find his wife faster
		if(_additionalEachStepNegativeReward)
			AddReward(-1/MaxStep);
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		Vector3Int move = Vector3Int.zero;
		//forward
		if (Input.GetKey(KeyCode.W)) move.z = 1;
		//backward
		else if (Input.GetKey(KeyCode.S)) move.z = 2;

		//right
		if (Input.GetKey(KeyCode.D)) move.x = 1;
		//left
		else if (Input.GetKey(KeyCode.A)) move.x = 2;

		actionsOut.DiscreteActions.Array[0] = move.z;
		actionsOut.DiscreteActions.Array[1] = move.x;

		//jump
		int jump = 0;
		if (Input.GetKey(KeyCode.Space))
		{
			jump = 1;
		}
		actionsOut.DiscreteActions.Array[2] = jump;
	}

	void OnTriggerEnter(Collider collider)
	{
		if (!_pressedButtonInEpisod && collider.tag == kButtonTag)
		{
			_pressedButtonInEpisod = true;
			_playgroundController.OpenDoor();
			_playgroundController.ChangeColor(true);
			AddReward(0.7f);
		}
		else if (collider.tag == kWifeTag)
		{
			_wifeParticle.Play();
			AddReward(1f);
			EndEpisode();
		}
	}
}
