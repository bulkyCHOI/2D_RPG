using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Diagnostics;
using System.IO;
using Unity.VisualScripting;





#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{
#if UNITY_EDITOR
    //%Ctrl, #Shift, &Alt
    //아래처럼 해주면 메뉴가 생긴다.
    [MenuItem("Tools/GenerateMap %#&g")]
    private static void GenerateMap()
    {
        GenerateByMap("Assets/Resources/Map/");
        GenerateByMap("../Common/MapData");
    }

    private static void GenerateByMap(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject go in gameObjects)
        {
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);
            Tilemap tmPortal = Util.FindChild<Tilemap>(go, "Tilemap_Portal", true);

            tmBase.CompressBounds();    //min/max 초기화
            tm.CompressBounds();

            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax);

                for (int y = tmBase.cellBounds.yMax; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                            writer.Write("1");
                        else
                        {
                        TileBase checkPortalTile = tmPortal.GetTile(new Vector3Int(x, y, 0));
                        if (checkPortalTile != null)
                        { 
                            Debug.Log(checkPortalTile.name);
                            string w = checkPortalTile.name.ToString();
                            writer.Write(w);
                        }
                        else
                            writer.Write("0");
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
    }

    private static int GetPortalNumber(string objectName)
    {
        switch (objectName)
        {
            case "chest":
                return 1;
        }
        return 1;
    }
#endif
}
