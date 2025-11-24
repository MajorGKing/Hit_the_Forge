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

    public Dictionary<string, Data.WeaponData> WeaponDict { get; private set; } = new Dictionary<string, Data.WeaponData>();

    public void Init()
    {
        TextDict = LoadJson<Data.TextDataLoader, string, Data.TextData>("TextData").MakeDict();
        WeaponDict = LoadJson<Data.WeaponDataLoader, string, Data.WeaponData>("WeaponData").MakeDict();
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
