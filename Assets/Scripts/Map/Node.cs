using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Map
{
      public class Node
      {
            protected Vector2Int _position;
            protected NodeType _nodeType;

            public int gCost;
            public int hCost;
            public Node parent;

            public int fCost
            {
                  get => gCost + hCost;
            }
            
            protected Node[,] _grid;

            protected NodeController _controller;

            public Node(Vector2Int position, NodeType nodeType)
            {
                  _position = position;
                  _nodeType = nodeType;
            }

            public void SetGrid(Node[,] grid)
            {
                  _grid = grid;
            }

            public Node[,] GetGrid()
            {
                  return _grid;
            }

            public void SetController(NodeController controller)
            {
                  _controller = controller;
            }

            public NodeController GetController()
            {
                  return _controller;
            }

            public List<Node> GetNeighbours()
            {
                  List<Node> neighbours = new List<Node>();
                  NodeType nodeType = GetNodeType();

                  switch (nodeType)
                  {
                        case NodeType.BLANK:
                              break;
                        case NodeType.DATA_BUS:
                        case NodeType.START_NODE:
                        case NodeType.NODE:
                              for (int y = -1; y <= 1; y++)
                              {
                                    for (int x = -1; x <= 1; x++)
                                    {
                                          if (Math.Abs(x) + Math.Abs(y) == 0 || Math.Abs(x) + Math.Abs(y) == 2)
                                                continue;

                                          Node n = _grid[_position.x + x, _position.y + y];
                                          if (n == null)
                                                continue;
                                          
                                          if (NodeReferences.InteractableNodeTypes.Contains(n.GetNodeType()) || n.GetNodeType() == NodeType.BLANK)
                                                continue;
                                          
                                          if (x != 0 && n.GetNodeType() == NodeType.VERT_CONNECTION)
                                                continue;
                                          
                                          if (y != 0 && n.GetNodeType() == NodeType.HOR_CONNECTION)
                                                continue;
                                          
                                          neighbours.Add(n);
                                    }
                              }
                              break;
                        case NodeType.VERT_CONNECTION:
                              for (int y = -1; y <= 1; y += 2)
                              {
                                    Node n = _grid[_position.x, _position.y + y];
                                    if (n == null)
                                          continue;
                                    
                                    if (n.GetNodeType() == NodeType.HOR_CONNECTION || n.GetNodeType() == NodeType.BLANK)
                                          continue;
                                    
                                    neighbours.Add(n);
                              }
                              break;
                        case NodeType.HOR_CONNECTION:
                              for (int x = -1; x <= 1; x += 2)
                              {
                                    Node n = _grid[_position.x + x, _position.y];
                                    if (n == null)
                                          continue;
                                    
                                    if (n.GetNodeType() == NodeType.VERT_CONNECTION || n.GetNodeType() == NodeType.BLANK)
                                          continue;
                                    
                                    neighbours.Add(n);
                              }
                              break;
                        default:
                              throw new ArgumentOutOfRangeException();
                  }

                  return neighbours;
            }

            public List<Node> GetNodeNeighbours()
            {
                  List<Node> neighbours = new List<Node>();
                  
                  for (int y = -1; y <= 1; y += 1)
                  {
                        for (int x = -1; x <= 1; x += 1)
                        {
                              if (Math.Abs(x) + Math.Abs(y) == 0)
                                    continue;
                              
                              if (Math.Abs(x) > 0 && Math.Abs(y) > 0)
                                    continue;

                              Node n = _grid[_position.x + x, _position.y + y];
                              if (n == null)
                                    continue;

                              if (n.GetNodeType() == NodeType.HOR_CONNECTION || n.GetNodeType() == NodeType.VERT_CONNECTION)
                              {
                                    List<Node> conNeigh = n.GetNeighbours();
                                    for (int i = 0; i < conNeigh.Count; i++)
                                    {
                                          if (conNeigh[i].GetController() != GetController())
                                          {
                                                if (NodeReferences.InteractableNodeTypes.Contains(conNeigh[i].GetNodeType()))
                                                      neighbours.Add(conNeigh[i]); 
                                          }
                                    }
                              }
                              
                              if (NodeReferences.InteractableNodeTypes.Contains(n.GetNodeType()))
                                    neighbours.Add(n);
                        }
                  }

                  return neighbours;
            }

            public Vector2Int GetPosition()
            {
                  return _position;
            }

            public void SetPosition(Vector2Int newPosition)
            {
                  _position = newPosition;
            }

            public NodeType GetNodeType()
            {
                  return _nodeType;
            }

            public void SetNodeType(NodeType newNodeType)
            {
                  _nodeType = newNodeType;
            }
      }

      public enum NodeType
      {
            BLANK, NODE, START_NODE, VERT_CONNECTION, HOR_CONNECTION, DATA_BUS
      }
}