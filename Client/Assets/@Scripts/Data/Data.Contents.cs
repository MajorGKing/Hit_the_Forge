using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

namespace Data
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcludeFieldAttribute : Attribute
    {
    }
    #region CreatureData
    [Serializable]
    public class CreatureData
    {
        public int TemplateId;
        public string NameTextID;
        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float ColliderRadius;
        public float MaxHp;
        public float UpMaxHpBonus;
        public float Atk;
        public float MissChance;
        public float AtkBonus;
        public float MoveSpeed;
        public float CriRate;
        public float CriDamage;
        public string IconImage;
        public string SkeletonDataID;
        public int DefaultSkillId;
        public int EnvSkillId;
        public int SkillAId;
        public int SkillBId;
       
    }

    [Serializable]
    public class CreatureDataLoader : ILoader<int, CreatureData>
    {
        public List<CreatureData> creatures = new List<CreatureData>();
        public Dictionary<int, CreatureData> MakeDict()
        {
            Dictionary<int, CreatureData> dict = new Dictionary<int, CreatureData>();
            foreach (CreatureData creature in creatures)
                dict.Add(creature.TemplateId, creature);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region TextData
    public class TextData
    {
        public string DataId;
        public string KOR;
    }

    [Serializable]
    public class TextDataLoader : ILoader<string, TextData>
    {
        public List<TextData> texts = new List<TextData>();

        public Dictionary<string, TextData> MakeDict()
        {
            Dictionary<string, TextData> dict = new Dictionary<string, TextData>();
            foreach (TextData text in texts)
                dict.Add(text.DataId, text);

            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region WeaponData
    [Serializable]
    public class WeaponData
    {
        public int TemplateId;
        public string WeaponName;
        public int HP;
        public int Iron;
        public int Coal;
        public int Price;
        public int NextTemplateId;
    }

    [Serializable]
    public class WeaponDataLoader : ILoader<int, WeaponData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public Dictionary<int, WeaponData> MakeDict()
        {
            Dictionary<int, WeaponData> dict = new Dictionary<int, WeaponData>();
            foreach (WeaponData weapon in weapons)
                dict.Add(weapon.TemplateId, weapon);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

}