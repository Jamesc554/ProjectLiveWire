using System;
using System.Collections;
using System.Collections.Generic;
using Lobby;
using Map;
using Mirror;
using UnityEngine;

public static class AStar
{
    private static NetworkManagerVirus room;

    private static NetworkManagerVirus NetManager
    {
        get
        {
            if (room != null)
            {
                return room;
            }

            return room = NetworkManager.singleton as NetworkManagerVirus;
        }
    }
        /// <summary>
        /// Generates a path from A to B, navigating around obstacles.
        /// </summary>
        /// <param name="startNode">Start of the path</param>
        /// <param name="targetNode">Target Position</param>
        /// <returns>A List<Node> of the path, NULL if there is no path.</returns>
        public static List<Node> FindPath(Node startNode, Node targetNode)
        {
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                if (currentNode.GetController().GetOwner().Equals(NetManager.GlobalOwner))
                    continue;
                
                foreach (Node neighbour in currentNode.GetNodeNeighbours())
                {
                    if (neighbour != targetNode)
                    {
                        if ((!neighbour.GetController().GetOwner().Equals(currentNode.GetController().GetOwner()) &&
                             !neighbour.GetController().GetOwner().Equals(NetManager.GlobalOwner)
                            ) && NodeReferences.InteractableNodeTypes.Contains(neighbour.GetNodeType()) || closedSet.Contains(neighbour))
                            continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }
            // NO PATH WAS FOUND - RETURN NULL
            return null;
        }

        static List<Node> RetracePath(Node startNode, Node targetNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            
            path.Reverse();
            
            // foreach (var node in path)
            // { 
            //     node.GetVisuals().GetComponent<MeshRenderer>().material.color = Color.magenta;
            //     Debug.Log("Path: " + node.GetX() + ":" + node.GetY());
            // }
            
            return path;
        }
        
        static int GetDistance(Node a, Node b)
        {
            int distanceX = Math.Abs(a.GetPosition().x - b.GetPosition().x);
            int distanceY = Math.Abs(a.GetPosition().y - b.GetPosition().y);

            if (distanceX > distanceY)
            {
                return 14 * distanceY + 10 * (distanceX - distanceY);
            }
            
            return 14 * distanceX + 10 * (distanceY - distanceX);
        }

        public static List<Node> SimplifyPath(List<Node> path)
        {
            List<Node> waypoints = new List<Node>();

            Vector2 directionOld = Vector2.zero;
            for (int i = 0; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i-1].GetPosition().x - path[i].GetPosition().x, path[i-1].GetPosition().y - path[i].GetPosition().y);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i]);
                }

                directionOld = directionNew;
            }
            
            return waypoints;
        }
}
