using System;
using System.Collections.Generic;

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
        public long TemplateId;
        public int Stage;
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
    public class CreatureDataLoader : ILoader<long, CreatureData>
    {
        public List<CreatureData> creatures = new List<CreatureData>();
        public Dictionary<long, CreatureData> MakeDict()
        {
            Dictionary<long, CreatureData> dict = new Dictionary<long, CreatureData>();
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
        public string ENG;
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
        public int WeaponNumber;
        public int Stage;
        public string WeaponName;
        public long HP;
        public long Iron;
        public long Coal;
        public long Price;
        public int NextWeaponNumber;
        public string WeaponImage;
    }

    [Serializable]
    public class WeaponDataLoader : ILoader<int, Dictionary<int, WeaponData>>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public Dictionary<int, Dictionary<int, WeaponData>> MakeDict()
        {
            Dictionary<int, Dictionary<int, WeaponData>> dict = new Dictionary<int, Dictionary<int, WeaponData>>();
            foreach (WeaponData weapon in weapons)
            {
                if (!dict.ContainsKey(weapon.Stage))
                    dict.Add(weapon.Stage, new Dictionary<int, WeaponData>());

                dict[weapon.Stage].Add(weapon.WeaponNumber, weapon);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region EnhancementData
    [Serializable]
    public class EnhancementData
    {
        public int EnhancementLevel;
        public int Stage;
        public int EnhancementSucess;
        public int BasicSucess;
        public float Price;
    }

    [Serializable]
    public class EnhancementDataLoader : ILoader<int, Dictionary<int, EnhancementData>>
    {
        public List<EnhancementData> enhancements = new List<EnhancementData>();
        public Dictionary<int, Dictionary<int, EnhancementData>> MakeDict()
        {
            Dictionary<int, Dictionary<int, EnhancementData>> dict = new Dictionary<int, Dictionary<int, EnhancementData>>();
            foreach (EnhancementData enhancement in enhancements)
            {
                if (!dict.ContainsKey(enhancement.Stage))
                    dict.Add(enhancement.Stage, new Dictionary<int, EnhancementData>());

                dict[enhancement.Stage].Add(enhancement.EnhancementLevel, enhancement);
            }
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    [Serializable]
    public class UpgradeData
    {
        public long TemplateId;
        public int Stage;
        public string UpgradeName;
        public long Price;
        public long CurrentValue;
        public long NextValue;
        public long OriginalTemplateId;
        public long NextTempalteId;

        //[ExcludeFieldAttribute]
        //public int StatIndex;
    }

    #region PlayerUpgrade
    [Serializable]
    public class PlayerUpgradeData : UpgradeData
    {
        //public int TemplateId;
        //public string UpgradeName;
        public Define.EPlayerStat StatType;
        //public int Price;
        //public int CurrentValue;
        //public int NextValue;
        //public int OriginalTemplateId;
        //public int NextTempalteId;
    }

    [Serializable]
    public class PlayerUpgradeDataLoader : ILoader<long, PlayerUpgradeData>
    {
        public List<PlayerUpgradeData> upgrades = new List<PlayerUpgradeData>();
        public Dictionary<long, PlayerUpgradeData> MakeDict()
        {
            Dictionary<long, PlayerUpgradeData> dict = new Dictionary<long, PlayerUpgradeData>();
            foreach (PlayerUpgradeData upgrade in upgrades)
                dict.Add(upgrade.TemplateId, upgrade);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region ForgeUpgrade
    [Serializable]
    public class ForgeUpgradeData : UpgradeData
    {
        //public int TemplateId;
        //public string UpgradeName;
        public Define.EPlayerForgeStat StatType;
        //public int Price;
        //public int CurrentValue;
        //public int NextValue;
        //public int OriginalTemplateId;
        //public int NextTempalteId;
    }

    [Serializable]
    public class ForgeUpgradeDataLoader : ILoader<long, ForgeUpgradeData>
    {
        public List<ForgeUpgradeData> upgrades = new List<ForgeUpgradeData>();
        public Dictionary<long, ForgeUpgradeData> MakeDict()
        {
            Dictionary<long, ForgeUpgradeData> dict = new Dictionary<long, ForgeUpgradeData>();
            foreach (ForgeUpgradeData upgrade in upgrades)
                dict.Add(upgrade.TemplateId, upgrade);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region TownUpgrade
    [Serializable]
    public class TownUpgradeData : UpgradeData
    {
        public Define.EPlayerTownStat StatType;
    }

    [Serializable]
    public class TownUpgradeDataLoader : ILoader<long, TownUpgradeData>
    {
        public List<TownUpgradeData> upgrades = new List<TownUpgradeData>();
        public Dictionary<long, TownUpgradeData> MakeDict()
        {
            Dictionary<long, TownUpgradeData> dict = new Dictionary<long, TownUpgradeData>();
            foreach (TownUpgradeData upgrade in upgrades) 
                dict.Add(upgrade.TemplateId, upgrade);
            
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region ShopProduct
    [Serializable]
    public class ShopProductData : UpgradeData
    {
        public Define.EShopProductType StatType;
        public Define.EShopBuyType BuyType;
    }

    [Serializable]
    public class ShopProductDataLoader : ILoader<long, ShopProductData>
    {
        public List<ShopProductData> products = new List<ShopProductData>();
        public Dictionary<long, ShopProductData> MakeDict()
        {
            Dictionary<long, ShopProductData> dict = new Dictionary<long, ShopProductData>();
            foreach (ShopProductData product in products)
                dict.Add(product.TemplateId, product);

            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

}