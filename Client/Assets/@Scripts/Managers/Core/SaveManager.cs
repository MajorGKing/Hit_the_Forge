using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SaveData
{
    public int Version = 1;
    public long[] currency;
    public long[] playerStatLevel;
    public long[] forgeStatLevel;
    public long[] townStatLevel;
    public List<long> shopProducts;
    public List<int> ownedWeapons;
    public int clearedWeaponCount;
    public int stage;
    public Define.ELanguage language;
}

public class SaveManager
{
    public void Init()
    {
        Managers.Game.OnDoSave -= SaveGame;
        Managers.Game.OnDoSave += SaveGame;
        Managers.Player.OnPlayerSave -= SaveGame;
        Managers.Player.OnPlayerSave += SaveGame;
    }

    public void Clear()
    {
        Managers.Game.OnDoSave -= SaveGame;
        Managers.Player.OnPlayerSave -= SaveGame;
    }

    public void SaveGame()
    {
        SaveData data = Managers.Player.GetSaveData();
        data.Version = 1; // Set current version
        string json = JsonConvert.SerializeObject(data);
        string encrypted = AesUtils.Encrypt(json);

        PlayerPrefs.SetString("SaveData", encrypted);
        PlayerPrefs.Save();
        
        //Log.Log("Game Saved");
    }

    public bool LoadGame()
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
                LogMessage.Log("Game Loaded");

                return true;
            }
            catch (System.Exception e)
            {
                LogMessage.LogError($"Failed to load save data: {e.Message}");
                // Handle corruption (e.g., start fresh or warn user)
                return false;
            }
        }
        else
        {
            LogMessage.Log("No save data found.");
            return false;
        }
    }
}
