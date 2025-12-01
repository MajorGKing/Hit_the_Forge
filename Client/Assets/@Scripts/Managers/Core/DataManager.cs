using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public interface IValidate
{
    bool Validate();
}

public interface ILoader<Key, Value> : IValidate
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    private HashSet<IValidate> _loaders = new HashSet<IValidate>();

    public Dictionary<string, Data.TextData> TextDict { get; private set; } = new Dictionary<string, Data.TextData>();
    public Dictionary<int, Data.WeaponData> WeaponDict { get; private set; } = new Dictionary<int, Data.WeaponData>();
    public Dictionary<int, Data.EnhancementData> EnhancementDict { get; private set; } = new Dictionary<int, Data.EnhancementData>();
    public Dictionary<int, Data.PlayerUpgradeData> PlayerUpgradeDict { get; private set; } = new Dictionary<int, Data.PlayerUpgradeData>();
    public Dictionary<int, Data.ForgeUpgradeData> ForgeUpgradeDict { get; private set; } = new Dictionary<int, Data.ForgeUpgradeData>();
    public Dictionary<int, Data.TownUpgradeData> TownUpgradeDict { get; private set; } = new Dictionary<int, Data.TownUpgradeData>();

    public void Init()
    {
        TextDict = LoadJson<Data.TextDataLoader, string, Data.TextData>("TextData").MakeDict();
        WeaponDict = LoadJson<Data.WeaponDataLoader, int, Data.WeaponData>("WeaponData").MakeDict();
        EnhancementDict = LoadJson<Data.EnhancementDataLoader, int, Data.EnhancementData>("EnhancementData").MakeDict();
        PlayerUpgradeDict = LoadJson<Data.PlayerUpgradeDataLoader, int, Data.PlayerUpgradeData>("PlayerUpgradeData").MakeDict();
        ForgeUpgradeDict = LoadJson<Data.ForgeUpgradeDataLoader, int, Data.ForgeUpgradeData>("ForgeUpgradeData").MakeDict();
        TownUpgradeDict = LoadJson<Data.TownUpgradeDataLoader, int, Data.TownUpgradeData>("TownUpgradeData").MakeDict();
        Validate();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
		TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        Loader loader = JsonConvert.DeserializeObject<Loader>(textAsset.text);
        _loaders.Add(loader);
        return loader;
	}

    private bool Validate()
    {
        bool success = true;

        foreach (var loader in _loaders)
        {
            if (loader.Validate() == false)
                success = false;
        }

        _loaders.Clear();

        return success;
    }

}
