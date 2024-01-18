using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    List<GameObject> _objects = new List<GameObject>();

    public void Add(GameObject go)
    {
        _objects.Add(go);
    }
    public void Remove(GameObject go)
    {
        _objects.Remove(go);
    }

    public GameObject Find(Vector3Int cellPos)
    {
        foreach (GameObject go in _objects)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if(cc == null)
                continue;

            if(cc.CellPos == cellPos)
                return go;
        }
        return null;
    }
    public void Clear()
    {
        _objects.Clear();
    }
}
