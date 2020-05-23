using System;
using System.Collections.Generic;
using Lobby;
using Map;
using Mirror;
using UnityEngine;

namespace Interface
{
    public class SelectionSquare : NetworkBehaviour
    {
        public GameObject[] nodes;

        private bool isSelecting = false;
        private Vector3 mousePosition1;
        
        private NetworkManagerVirus _netManager;

        private NetworkManagerVirus NetManager
        {
            get
            {
                if (_netManager != null)
                {
                    return _netManager;
                }

                return _netManager = NetworkManager.singleton as NetworkManagerVirus;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isSelecting = true;
                mousePosition1 = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                NetworkIdentity ni = NetworkClient.connection.identity;
                NetworkGamePlayerVirus player = ni.GetComponent<NetworkGamePlayerVirus>();
                player.CmdUnselectNodes();
                
                foreach (var node in FindObjectsOfType<SelectableNode>())
                {
                    if (IsWithinSelectionBounds(node.gameObject))
                    {
                        Debug.Log("Selected Node");
                        player.CmdSelectNode(node.GetComponent<NetworkIdentity>());
                    }
                }
                
                isSelecting = false;
            }
        }

        void OnGUI()
        {
            if (isSelecting)
            {
                var rect = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
                Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        public bool IsWithinSelectionBounds(GameObject gameObject)
        {
            if (!isSelecting)
                return false;

            var camera = Camera.main;
            var viewportBounds = Utils.GetViewportBounds(camera, mousePosition1, Input.mousePosition);

            return viewportBounds.Contains(camera.WorldToViewportPoint(gameObject.transform.position));
        }
    }
}