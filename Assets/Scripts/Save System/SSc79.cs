using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SSc79
{
	public static string filePath {get { return Application.persistentDataPath + "/SSc79"; } }
	public static SaveData saveData;
	public const string version = "0.7.9";


	public static void Save(string saveName)
	{
		string path = filePath + "/" + saveName + ".json";
		if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
		File.WriteAllText(path, JsonUtility.ToJson(saveData, true));
	}


	public static void NewSave()
	{
		saveData = new SaveData();
		saveData.version = version;
	}

	public static SaveData Load(string saveName)
	{
		string path = filePath + "/" + saveName + ".json";
		if (!File.Exists(path)) {
			Debug.LogError("Path not valid for load");
			return null;
		}
		string content = File.ReadAllText(path);
		saveData = JsonUtility.FromJson<SaveData>(content);
		return saveData;
	} 


	[System.Serializable]
	public class SaveData {
		public string version;
		public List<WarriorData> warriors;
		public Vector2Int mapSize; 
		public Color[] mapData;

		public SaveData()
		{
			warriors = new List<WarriorData>();
		}
	}

	[System.Serializable]
	public class WarObjectData {

		public Vector3 position;
		public enum ModelType { Car, City, Warrior, Truck, Tank, Ship, Plane, Sprite, Factory, Shop, ContructionSite };
		public ModelType modelType;
		public string nation;
		public float health;
		public float damage;

		public WarObjectData(WarObject obj)
		{
			position = obj.transform.position;
			modelType = (ModelType) obj.modelType;
			nation = obj.nation;
			health = obj.health;
			damage = obj.damage;
		}
	}

	[System.Serializable]
	public class WarriorData : WarObjectData {
		public float speed;

		public WarriorData(Warrior obj) : base(obj)
		{
			speed = obj.speed;
		}
	}
}
