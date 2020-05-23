using Map.Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
    public static class TilemapParser
    {
        public static Node[,] ParseMap(Tilemap tileMap)
        {

            BoundsInt bounds = tileMap.cellBounds;
            TileBase[] allTiles = tileMap.GetTilesBlock(bounds);
            
            Node[,] map = new Node[bounds.size.x, bounds.size.y];
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int x = 0; x < bounds.size.x; x++)
                {
                    TileBase tile = allTiles[x + y * bounds.size.x];
                    if (tile != null)
                    {
                        switch (tile.name)
                        {
                            case "ChipTileMap_StartNode":
                                map[x, y] = new BaseNode(new Vector2Int(x, y), NodeType.START_NODE);
                                break;
                            case "ChipTileMap_Node":
                                map[x, y] = new BaseNode(new Vector2Int(x, y), NodeType.NODE);
                                break;
                            case "ChipTileMap_DataBus":
                                map[x, y] = new BaseNode(new Vector2Int(x, y), NodeType.DATA_BUS);
                                break;
                            case "ChipTileMap_HConnection":
                                map[x, y] = new Node(new Vector2Int(x, y), NodeType.HOR_CONNECTION);
                                break;
                            case "ChipTileMap_VConnection":
                                map[x, y] = new Node(new Vector2Int(x, y), NodeType.VERT_CONNECTION);
                                break;
                            default:
                                map[x, y] = new Node(new Vector2Int(x, y), NodeType.BLANK);
                                break;
                        }
                    }
                }
            }

            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int x = 0; x < bounds.size.x; x++)
                {
                    if (map[x, y] != null)
                    {
                        map[x, y].SetGrid(map);
                    }
                }
            }

            return map;
        }
    }
}
