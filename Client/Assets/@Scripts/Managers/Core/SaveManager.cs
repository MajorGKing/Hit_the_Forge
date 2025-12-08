using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SaveData
{
    public int Version = 1;
    public int[] currency;
    public int[] playerStatLevel;
    public int[] forgeStatLevel;
    public int[] townStatLevel;
    public int[] shopProducts;
}

public class SaveManager
{
    public void Init()
    {
        Managers.Game.OnDoSave -= SaveGame;
        Managers.Game.OnDoSave += SaveGame;
    }

    public void Clear()
    {
        Managers.Game.OnDoSave -= SaveGame;
    }

    public void SaveGame()
    {
        SaveData data = Managers.Player.GetSaveData();
        data.Version = 1; // Set current version
        string json = JsonConvert.SerializeObject(data);
        string encrypted = AesUtils.Encrypt(json);

        PlayerPrefs.SetString("SaveData", encrypted);
        PlayerPrefs.Save();
        
        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string encrypted = PlayerPrefs.GetString("SaveData");
            try 
            {
                string json = AesUtils.Decrypt(encrypted);
                SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

                if (data.Version < 1)
                {
                    // Handle migration for older versions here
                    // e.g., if (data.Version == 0) CheckOldData(data);
                }

                Managers.Player.RestoreFromSaveData(data);
                Debug.Log("Game Loaded");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load save data: {e.Message}");
                // Handle corruption (e.g., start fresh or warn user)
            }
        }
        else
        {
            Debug.Log("No save data found.");
        }
    }
}
