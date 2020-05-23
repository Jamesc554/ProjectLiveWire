using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public class EnergyPathfinder : NetworkBehaviour
{
    private List<Node> _path;
    private int _targetIndex;
    private float _speed = 10f;
    private int _energyAmount;

    private Action<NodeController, int> _onDestReached;

    [Server]
    public void Spawn(List<Node> path, NodeController destinationNode, int amount, NetworkIdentity sender)
    {
        _path = path;

        StopCoroutine(FollowPath(destinationNode, amount, sender));
        StartCoroutine(FollowPath(destinationNode, amount, sender));
    }

    private IEnumerator FollowPath(NodeController destinationNode, int amount, NetworkIdentity sender)
    {
        Node currentWaypoint = _path[0];

        while (true)
        {
            if (Math.Abs(transform.position.x - (currentWaypoint.GetPosition().x - 5.5f)) < 0.1 &&
                Math.Abs(transform.position.y - (currentWaypoint.GetPosition().y - 9.5f)) < 0.1)
            {
                _targetIndex++;
                if (_targetIndex >= _path.Count)
                {
                    FinishedPath(destinationNode, amount, sender);
                    yield break;
                }

                Debug.Log($"{_targetIndex}:{_path.Count}");
                currentWaypoint = _path[_targetIndex];
            }

            Vector2Int newTarget = currentWaypoint.GetPosition();
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(newTarget.x - 5.5f, newTarget.y - 9.5f, 0), _speed * Time.deltaTime);

            yield return null;
        }
    }

    [Server]
    private void FinishedPath(NodeController destinationNode, int amount, NetworkIdentity sender)
    {
        destinationNode.AddEnergy(amount, sender);
        NetworkServer.Destroy(gameObject);
    }
}
