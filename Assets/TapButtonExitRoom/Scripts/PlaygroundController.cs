using System.Collections;
using UnityEngine;

public class PlaygroundController : MonoBehaviour
{
	private const int kDoorDownDistance = 5;
	private const int kDoorDownTime = 1;

	[SerializeField] private Transform _agent;
	[SerializeField] private Transform _door;
	[SerializeField] private Transform _buttonStand;
	[SerializeField] private Transform _button;
	[SerializeField] private Transform _wife;
	[SerializeField] private MeshRenderer _buttonRenderer;
	[SerializeField] private Material _startBtnMaterial;
	[SerializeField] private Material _pressedButtonMaterial;
	[Space]
	[SerializeField] private Transform _topLeftSpawnPosition;
	[SerializeField] private Transform _botRightSpawnPosition;
	[Space]
	[SerializeField] private Light _light;
	[SerializeField] private Color _lightStartColor = Color.red;
	[SerializeField] private Color _lightPressedColor = Color.green;
	public Vector3 ButtonPosition
	{
		get { return _button.position; }
	}
	public Vector3 WifePosition
	{
		get { return _wife.position; }
	}

	private Vector3 _doorInitialPosition;
	private Vector3 _agentInitialPosition;
	private float _doorTimer;
	private Coroutine _doorCoroutine;

	void Awake()
	{
		_doorInitialPosition = _door.position;
		_agentInitialPosition = _agent.position;
	}

	public void ResetScene()
	{
		TryStopDoor();
		ChangeColor(false);
		
		_agent.position = _agentInitialPosition;

		float saveY = _agent.position.y;
		Vector3 tmpPosition = GetRandomSpawnPosition();
		tmpPosition.y = saveY;
		_agent.position = tmpPosition;

		saveY = _buttonStand.position.y;
		tmpPosition = GetRandomSpawnPosition();
		tmpPosition.y = saveY;
		_buttonStand.position = tmpPosition;

		_door.position = _doorInitialPosition;
	}

	public Vector3 GetRandomSpawnPosition()
	{
		float minX = Mathf.Min(_topLeftSpawnPosition.position.x, _botRightSpawnPosition.position.x);
		float minZ = Mathf.Min(_topLeftSpawnPosition.position.z, _botRightSpawnPosition.position.z);

		float maxX = Mathf.Max(_topLeftSpawnPosition.position.x, _botRightSpawnPosition.position.x);
		float maxZ = Mathf.Max(_topLeftSpawnPosition.position.z, _botRightSpawnPosition.position.z);

		return new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));
	}

	public void OpenDoor()
	{
		TryStopDoor();
		_doorCoroutine = StartCoroutine(MoveDoorCoroutine());
	}

	public void ChangeColor(bool pressing)
	{
		_buttonRenderer.material = pressing ? _pressedButtonMaterial : _startBtnMaterial;
		_light.color = pressing ? _lightPressedColor : _lightStartColor;
	}

	private void TryStopDoor()
	{
		if (_doorCoroutine != null)
		{
			StopCoroutine(_doorCoroutine);
			_doorCoroutine = null;
		}
	}

	private IEnumerator MoveDoorCoroutine()
	{
		_door.position = _doorInitialPosition;
		Vector3 endPosition = _doorInitialPosition;
		endPosition.y -= kDoorDownDistance;
		_doorTimer = 0;

		while (_doorTimer < kDoorDownTime)
		{
			Vector3 doorLerpPos = Vector3.Lerp(_doorInitialPosition, endPosition, _doorTimer / kDoorDownTime);
			_door.transform.position = doorLerpPos;
			_doorTimer += Time.deltaTime;
			yield return 0;
		}
	}
}
