using UnityEngine;

namespace Unavinar.LegsWalker
{
	public class RobotWalkerPlaygroundController : MonoBehaviour
	{
		private const int kRespawnTries = 500;

		[SerializeField] Transform topLeftSpawnPosition;
		[SerializeField] Transform botRightSpawnPosition;
		[SerializeField] Transform target;
		[SerializeField] Transform agent;
		[SerializeField] float minLengthAgentTarget = 10;

		public Vector3 TargetPosition
		{
			get => target.transform.position;
		}

		private float _targetY;
		private float _agentY;

		public void Initialize()
		{
			_agentY = agent.position.y;
			_targetY = target.position.y;
		}

		public void ResetScene()
		{
			agent.transform.position = GetRandomSpawnPosition(_agentY);

			for (int i = 0; i < kRespawnTries; i++)
			{
				target.transform.position = GetRandomSpawnPosition(_targetY);
				if (Vector3.Distance(target.transform.position, agent.transform.position) >= minLengthAgentTarget)
					break;
			}
		}

		public Vector3 GetRandomSpawnPosition(float y)
		{
			float minX = Mathf.Min(topLeftSpawnPosition.position.x, botRightSpawnPosition.position.x);
			float minZ = Mathf.Min(topLeftSpawnPosition.position.z, botRightSpawnPosition.position.z);

			float maxX = Mathf.Max(topLeftSpawnPosition.position.x, botRightSpawnPosition.position.x);
			float maxZ = Mathf.Max(topLeftSpawnPosition.position.z, botRightSpawnPosition.position.z);

			return new Vector3(Random.Range(minX, maxX), y, Random.Range(minZ, maxZ));
		}
	}
}