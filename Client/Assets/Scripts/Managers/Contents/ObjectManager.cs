﻿using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

	public static GameObjectType GetObjectTypeById(int id)
	{
		int type = (id >> 24) & 0x7F;
		return (GameObjectType)type;
    }

	public void Add(ObjectInfo info, bool myPlayer = false)
	{
		GameObjectType type = GetObjectTypeById(info.ObjectId);
		if (type == GameObjectType.Player)
		{
			if (myPlayer)
			{
				GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
				go.name = info.Name;
				_objects.Add(info.ObjectId, go);

				MyPlayer = go.GetComponent<MyPlayerController>();
				MyPlayer.Id = info.ObjectId;
				MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.Stat = info.StatInfo;
				MyPlayer.SyncPos();
			}
			else
			{
				GameObject go = Managers.Resource.Instantiate("Creature/Player");
				go.name = info.Name;
				_objects.Add(info.ObjectId, go);
				PlayerController pc = go.GetComponent<PlayerController>();
				pc.Id = info.ObjectId;
				pc.PosInfo = info.PosInfo;
				pc.Stat = info.StatInfo;
				pc.SyncPos();
			}
		}
		else if (type == GameObjectType.Monster)
		{

		}
		else if (type == GameObjectType.Projectile)
		{
            GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
			go.name = "Arrow";
			_objects.Add(info.ObjectId, go);

			ArrowController ac = go.GetComponent<ArrowController>();
			ac.PosInfo = info.PosInfo;
			ac.Stat = info.StatInfo;
			ac.SyncPos();
        }
    }

	public void Remove(int id)
	{
		GameObject go = FindById(id);
		if (go == null)
            return;

		_objects.Remove(id);
		Managers.Resource.Destroy(go);
	}

	public void RemoveMyPlayer()
	{
		if (MyPlayer != null)
		{
			Remove(MyPlayer.Id);
			MyPlayer = null;
		}
		else
			return;
	}

	public GameObject FindById(int id)
	{
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear() //모든 오브젝트를 삭제
	{
		foreach (GameObject obj in _objects.Values)	
		{
            Managers.Resource.Destroy(obj);
        }
		_objects.Clear();
	}
}
