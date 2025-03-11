using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public string saveName;

    [ContextMenu("Save")]
    public void Save()
    {
        // Warrior Data
        foreach (var obj in BattleManager.GetAllWarriors()) {
            SSc79.saveData.warriors.Add(new(obj));
        }

        // Map Data
        SSc79.saveData.mapData = GlobalData.ConvertTo1D(GlobalData.mapRenderer.mapData);
        SSc79.saveData.mapSize = GlobalData.Size(GlobalData.mapRenderer.mapData);

        SSc79.Save(saveName);
    }

    [ContextMenu("New Save")]
    public void NewSave()
    {
        SSc79.NewSave();
    }

    [ContextMenu("Load")]
    public void Load()
    {
        var saveData = SSc79.Load(saveName);

        // Load Map Data
        GlobalData.mapRenderer.mapData = GlobalData.ConvertTo2D( saveData.mapData, saveData.mapSize.x, saveData.mapSize.y);
        
        // Load Warriors
        BattleManager.RemoveWarriors();
        foreach (var data in saveData.warriors)
        {
            EconomyManager.SpawnWarrior(data.position, (WarObject.ModelType)data.modelType,
                new(data.nation, data.health, data.damage, data.speed));
        }
        GlobalData.battle.GroupWarObjects();
    }
}
