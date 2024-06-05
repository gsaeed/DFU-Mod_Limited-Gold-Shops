// Project:         LimitedGoldShops mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.00
// Created On: 	    4/21/2020, 7:35 PM
// Last Edit:		5/28/2020, 11:40 PM
// Modifier:
// Special Thanks:  BadLuckBurt, Hazelnut, Ralzar, LypyL, Pango, TheLacus, Baler

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;


namespace LimitedGoldShops
{
    [FullSerializer.fsObject("v1")]
    public class LGSaveData
    {
        public Dictionary<string, ShopData> ShopBuildingData;
    }

    public class ShopData {
        public ulong CreationTime;
        public bool InvestedIn;
        public uint AmountInvested;
        public int ShopAttitude;
        public int BuildingQuality;
        public int CurrentGoldSupply;
        public int CurrentCreditSupply;

    }

    public class LimitedGoldShopsMain : MonoBehaviour, IHasModSaveData
    {
        static Mod mod;

        static LimitedGoldShopsMain instance;

        public static int BankRangeMultiplier { get; set; }

        public static int MinimumItemQualityAtBestBank { get; set; }

        public static int MinimumItemQualityAtGoodBank { get; set; }

        public static int MinimumItemQualityAtAverageBank { get; set; }

        public static int TradeInValuePercent { get; set; }

        public static float ShopGoldSettingModifier { get; set; }
        public static bool ShopStandardsSetting { get; set; }
        public static bool ShopIgnoresStandardsForMagicalItems { get; set; }
        public static bool CheckWeaponsArmor { get; set; }
        public static bool CheckClothing { get; set; }
        public static bool CheckReligiousItems { get; set; }
        public static bool CheckIngredients { get; set; }


        public static bool CanSellUnidentifiedItems { get; set; }
        public static bool OnlyQualityShopsCanBuyUnidentifiedItems { get; set; }
        public static int ShopQualityNeededToBuyUnidentifiedItems { get; set; }
        public static ulong DaysToReset { get; set; } = 15;
        public static LimitedGoldShopsMain Instance
        {
            get { return instance ?? (instance = FindObjectOfType<LimitedGoldShopsMain>()); }
        }

        public static Dictionary<string, ShopData> ShopBuildingData;

        public static int FlexCurrentGoldSupply { get; set; }

        public Type SaveDataType
        {
            get { return typeof(LGSaveData); }
        }

        public object NewSaveData()
        {
            return new LGSaveData
            {
                ShopBuildingData = new Dictionary<string, ShopData>()
            };
        }

        public object GetSaveData()
        {
            return new LGSaveData
            {
                ShopBuildingData = ShopBuildingData
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var LGSaveData = (LGSaveData)saveData;
            Debug.Log("Restoring save data");
            ShopBuildingData = LGSaveData.ShopBuildingData;
            Debug.Log("Save data restored");
        }    

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            instance = new GameObject("LimitedGoldShops")
                .AddComponent<LimitedGoldShopsMain>(); // Add script to the scene.
            mod.SaveDataInterface = instance;
            if (ShopBuildingData == null)
            {
                ShopBuildingData = new Dictionary<string, ShopData>();
            }

            PlayerEnterExit.OnTransitionInterior += GenerateShop_OnTransitionInterior;

            WorldTime.OnNewDay += RemoveExpiredShops_OnNewDay;

            mod.LoadSettingsCallback =
                LoadSettings; // To enable use of the "live settings changes" feature in-game.

            // initiates mod message handler
            mod.MessageReceiver = DFModMessageReceiver;

            mod.IsReady = true;

        }

        static void DFModMessageReceiver(string message, object data, DFModMessageCallback callBack)
        {
            switch (message)
            {
                case "GetShopGoldSupply":
                    callBack?.Invoke(message, FlexCurrentGoldSupply);
                    break;
                default:
                    callBack?.Invoke(message, "Unknown message");
                    break;
            }
        }

        private void Start()
        {
            Debug.Log("Begin mod init: LimitedGoldShops");
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Trade, typeof(LGSTradeWindowText));
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantRepairPopup, typeof(LGSMerchantTradeRepairPopupWindowReplace));
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.MerchantServicePopup, typeof(LGSMerchantTradeServicePopupWindowReplace));
            Debug.Log("LimitedGoldShops Registered It's Windows");

            mod.LoadSettings();

            PlayerEnterExit.OnTransitionInterior += GenerateShop_OnTransitionInterior;

            WorldTime.OnNewDay += RemoveExpiredShops_OnNewDay;

            Debug.Log("Finished mod init: LimitedGoldShops");
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            ShopGoldSettingModifier = mod.GetSettings().GetValue<float>("Options", "ShopGoldModifier");
            ShopStandardsSetting = mod.GetSettings().GetValue<bool>("Options", "ShopStandards");
            ShopIgnoresStandardsForMagicalItems = mod.GetSettings().GetValue<bool>("Options", "ShopIgnoresStandardsForMagicalItems");
            CanSellUnidentifiedItems = mod.GetSettings().GetValue<bool>("Options", "CanSellUnidentifiedItems");
            OnlyQualityShopsCanBuyUnidentifiedItems = mod.GetSettings().GetValue<bool>("Options", "OnlyQualityShopsCanBuyUnidentifiedItems");
            var ShopQuality = mod.GetSettings().GetValue<int>("Options", "ShopQualityNeededToBuyUnidentifiedItems");
            
            if (ShopQuality == 0)
                ShopQualityNeededToBuyUnidentifiedItems = 0;
            else if (ShopQuality == 1)
                ShopQualityNeededToBuyUnidentifiedItems = 4;
            else if (ShopQuality == 2)
                ShopQualityNeededToBuyUnidentifiedItems = 8;
            else if (ShopQuality == 3)
                ShopQualityNeededToBuyUnidentifiedItems = 14;
            else
                ShopQualityNeededToBuyUnidentifiedItems = 18;
            DaysToReset = (ulong)mod.GetSettings().GetValue<int>("Options", "DaysToReset");
            DaggerfallWorkshop.Game.Banking.DaggerfallBankManager.housePriceMult =
                (float) (mod.GetSettings().GetValue<int>("Options", "HousePriceMultiplier") * 10);

            TradeInValuePercent = mod.GetSettings().GetValue<int>("TradeItem", "TradeInValuePercent");
            MinimumItemQualityAtAverageBank = mod.GetSettings().GetValue<int>("TradeItem", "MinimumItemQualityAtAverageBank");
            MinimumItemQualityAtGoodBank = mod.GetSettings().GetValue<int>("TradeItem", "MinimumItemQualityAtGoodBank");
            MinimumItemQualityAtBestBank = mod.GetSettings().GetValue<int>("TradeItem", "MinimumItemQualityAtBestBank");
            BankRangeMultiplier = mod.GetSettings().GetValue<int>("TradeItem", "BankRangeMultiplier");

            CheckWeaponsArmor = mod.GetSettings().GetValue<bool>("CheckItemFilter", "CheckWeaponsArmor");
            CheckClothing = mod.GetSettings().GetValue<bool>("CheckItemFilter", "CheckClothing");
            CheckReligiousItems = mod.GetSettings().GetValue<bool>("CheckItemFilter", "CheckReligiousItems");
            CheckIngredients = mod.GetSettings().GetValue<bool>("CheckItemFilter", "CheckIngredients");


            AsesinoTradeWindow.CheckGeneralStore = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckGeneralStore");
            AsesinoTradeWindow.CheckPawnShops = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckPawnShops");
            AsesinoTradeWindow.CheckArmorer = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckArmorer");
            AsesinoTradeWindow.CheckWeaponShop = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckWeaponShop");
            AsesinoTradeWindow.CheckAlchemist = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckAlchemist");
            AsesinoTradeWindow.CheckClothingStore = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckClothingStore");
            AsesinoTradeWindow.CheckBookStore = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckBookStore");
            AsesinoTradeWindow.CheckGemStore = mod.GetSettings().GetValue<bool>("CheckStoreSplitForTabs", "CheckGemStore");
        }

        public static float GetShopGoldSettingModifier()
        {
            return ShopGoldSettingModifier;
        }

        public static bool GetShopStandardsSetting()
        {
            return ShopStandardsSetting;
        }

        public static void TradeUpdateShopGold(DaggerfallTradeWindow.WindowModes mode, int value, int creditAmt = 0)
        {
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            string buildingKey = "";
            int currentGoldSupply = 0;
            int currentCreditAmt = 0;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;

            try
            {
                ShopData sd;
                if (ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
                {
                    buildingKey = $"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}";
                    creationTime = sd.CreationTime;
                    investedIn = sd.InvestedIn;
                    amountInvested = sd.AmountInvested;
                    shopAttitude = sd.ShopAttitude;
                    buildingQuality = sd.BuildingQuality;
                    currentCreditAmt = sd.CurrentCreditSupply;
                    currentGoldSupply = sd.CurrentGoldSupply;

                }
                //Debug.LogFormat("Sale Value = {0}", value);
                if (mode == DaggerfallTradeWindow.WindowModes.Buy && (playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.Temple || playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.GuildHall))
                {
                    //Debug.Log("You are buying inside a Temple or a Guild-Hall.");
                }
                else if (mode == DaggerfallTradeWindow.WindowModes.Buy || mode == DaggerfallTradeWindow.WindowModes.Repair)
                {
                    if (currentCreditAmt > 0)
                    {
                        if (currentCreditAmt >= value)
                        {
                            currentCreditAmt -= value;
                            value = 0;
                        }
                        else
                        {
                            value -= currentCreditAmt;
                            currentCreditAmt = 0;
                        }

                    }

                    currentGoldSupply += value;

                    ShopBuildingData.Remove(buildingKey);

                    ShopData currentBuildKey = new ShopData
                    {
                        CreationTime = creationTime,
                        InvestedIn = investedIn,
                        AmountInvested = amountInvested,
                        ShopAttitude = shopAttitude,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply,
                        CurrentCreditSupply = currentCreditAmt,
                    };
                    ShopBuildingData.Add(buildingKey, currentBuildKey);
                }
                else
                {

                    currentCreditAmt += creditAmt;


                    if (creditAmt > 0) 
                        currentGoldSupply = 0;
                    else
                        currentGoldSupply = sd.CurrentGoldSupply - value;

                        ShopBuildingData.Remove(buildingKey);

                    ShopData currentBuildKey = new ShopData
                    {
                        CreationTime = creationTime,
                        InvestedIn = investedIn,
                        AmountInvested = amountInvested,
                        ShopAttitude = shopAttitude,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply,
                        CurrentCreditSupply = currentCreditAmt,
                    };
                    ShopBuildingData.Add(buildingKey, currentBuildKey);
                }
                //Debug.LogFormat("Shop Gold Supply After Sale = {0}", currentGoldSupply);
                FlexCurrentGoldSupply = currentGoldSupply;
            }
            catch
            {
                //Debug.Log("You are buying inside a Temple or a Guild-Hall, as your first entered building since launching the game!");
            }
        }

        public static void UpdateInvestAmount(int investOffer = 0) // This is responsible for updating investment data in the save-data for a building etc.
        {
            uint investedOffer = Convert.ToUInt32(investOffer);
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            string buildingKey = "";
            int currentGoldSupply = 0;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;

            ShopData sd;
            if (ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                buildingKey = $"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}";
                creationTime = sd.CreationTime;
                investedIn = sd.InvestedIn;
                amountInvested = sd.AmountInvested;
                shopAttitude = sd.ShopAttitude;
                buildingQuality = sd.BuildingQuality;
                currentGoldSupply = sd.CurrentGoldSupply;
            }
            //Debug.LogFormat("Offered Investment Amount = {0}", investedOffer);

            investedIn = true;
            amountInvested = sd.AmountInvested + investedOffer;
            ShopBuildingData.Remove(buildingKey);

            ShopData currentBuildKey = new ShopData
            {
                CreationTime = creationTime,
                InvestedIn = investedIn,
                AmountInvested = amountInvested,
                ShopAttitude = shopAttitude,
                BuildingQuality = buildingQuality,
                CurrentGoldSupply = currentGoldSupply
            };
            ShopBuildingData.Add(buildingKey, currentBuildKey);
            //Debug.LogFormat("Total Investment Amount After This Addition = {0}", amountInvested);
        }

        protected static void RemoveExpiredShops_OnNewDay()
        {
            List<string> expiredKeys = new List<string>();
            ulong creationTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToSeconds();

            foreach (KeyValuePair<string, ShopData> kvp in ShopBuildingData.ToArray())
            {
                ulong timeLastVisited = creationTime - kvp.Value.CreationTime;
                if (!kvp.Value.InvestedIn && kvp.Value.CurrentCreditSupply <=0) // Only deletes expired entries with "OnNewDay" if the "InvestedIn" value inside ShopBuildingData is "false", if the shop has been invested in though, it will not be deleted in this way, only will it be deleted/refreshed when the player enters the expired shop in question, which will generate the gold value based on the amount invested in store, etc.
                {
                    if (DaysToReset != 0 && timeLastVisited >= DaysToReset * 86400) // 15 * 86400 = Number of seconds in Requested days.
                    {
                        //Debug.Log("Expired Building Key " + kvp.Key.ToString() + " Detected, Adding to List");
                        expiredKeys.Add(kvp.Key);
                    }
                }
            }
            
            foreach (string expired in expiredKeys)
            {
                //Debug.Log("Removed Expired Building Key " + expired.ToString());
                ShopBuildingData.Remove(expired);
            }
            ModifyShopGold_OnNewDay();
        }

        protected static void ModifyShopGold_OnNewDay()
        {
            ulong creationTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToSeconds();
            var level = GameManager.Instance.PlayerEntity.Level;
            int buildingQualityMult;
            foreach (KeyValuePair<string, ShopData> kvp in ShopBuildingData.ToArray())
            {
                if (kvp.Value.BuildingQuality >= 16)
                    buildingQualityMult = 5;
                else if (kvp.Value.BuildingQuality >= 12)
                    buildingQualityMult = 3;
                else if (kvp.Value.BuildingQuality >= 8)
                    buildingQualityMult = 2;
                else
                    buildingQualityMult = 1;

                var gold = kvp.Value.CurrentGoldSupply;
                var ranGoldAdjustment = UnityEngine.Random.Range(-100 * level * buildingQualityMult, 100 * level * buildingQualityMult);
                gold += ranGoldAdjustment;
                if (gold <= 25 * level * buildingQualityMult)
                    gold = Mathf.Max(Mathf.Abs(ranGoldAdjustment), 25 * level * buildingQualityMult);

                kvp.Value.CurrentGoldSupply = gold;
                ShopBuildingData[kvp.Key] = kvp.Value;
            }
        }

        protected static void GenerateShop_OnTransitionInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            ulong creationTime = 0;
            int buildingQuality = 0;
            bool investedIn = false;
            uint amountInvested = 0;
            int shopAttitude = 0;
            string buildingKey = "";
            int currentGoldSupply = 0;
            int creditAmt = 0;

            creationTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToSeconds();
            buildingQuality = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
            buildingKey = $"{GameManager.Instance.PlayerGPS.CurrentMapID}-{GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey}";

            if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideOpenShop)
            {
                //Debug.Log("You Just Entered An Open Shop, Good Job!...");
                ShopData sd;
                currentGoldSupply = GoldSupplyAmountGenerator(buildingQuality);
               shopAttitude = UnityEngine.Random.Range(0, 2); // Mostly have it as an int instead of a bool, just incase I want to add more "attitude types" to this later on.
                //Debug.Log("Building Attitude Rolled: " + shopAttitude.ToString());
                FlexCurrentGoldSupply = currentGoldSupply; // On transition, sets the shops gold supply to this "global variable" for later uses.

                if (!ShopBuildingData.ContainsKey(buildingKey))
                {
                    ShopData currentBuildKey = new ShopData
                    {
                        CreationTime = creationTime,
                        InvestedIn = investedIn,
                        AmountInvested = amountInvested,
                        ShopAttitude = shopAttitude,
                        BuildingQuality = buildingQuality,
                        CurrentGoldSupply = currentGoldSupply,
                        CurrentCreditSupply = 0,
                    };
                    //Debug.Log("Adding building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                    ShopBuildingData.Add(buildingKey, currentBuildKey);
                }
                else
                {
                    if (ShopBuildingData.TryGetValue(buildingKey, out sd))
                    {
                        ulong timeLastVisited = creationTime - sd.CreationTime;
                        investedIn = sd.InvestedIn;
                        if (timeLastVisited >= DaysToReset * 86400)
                        {
                            if (investedIn )
                            {
                                investedIn = sd.InvestedIn;
                                amountInvested = sd.AmountInvested;
                                shopAttitude = sd.ShopAttitude;
                                buildingQuality = sd.BuildingQuality;
                                currentGoldSupply = InvestedGoldSupplyAmountGenerator(buildingQuality, amountInvested, shopAttitude);
                                creditAmt = sd.CurrentCreditSupply;
                                

                                ShopBuildingData.Remove(buildingKey);
                                //Debug.Log("Removing Expired Invested Shop Gold & Time: " + buildingKey.ToString());
                                ShopData currentBuildKey = new ShopData
                                {
                                    CreationTime = creationTime,
                                    InvestedIn = investedIn,
                                    AmountInvested = amountInvested,
                                    ShopAttitude = shopAttitude,
                                    BuildingQuality = buildingQuality,
                                    CurrentGoldSupply = currentGoldSupply,
                                    CurrentCreditSupply = creditAmt,
                                };
                                //Debug.Log("Refreshing building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                                ShopBuildingData.Add(buildingKey, currentBuildKey);
                            }
                            else
                            {
                                creditAmt = sd.CurrentCreditSupply;
                                ShopBuildingData.Remove(buildingKey);
                                //Debug.Log("Removed Expired Building Key " + buildingKey.ToString() + " Generating New Properties");
                                ShopData currentBuildKey = new ShopData
                                {
                                    CreationTime = creationTime,
                                    InvestedIn = investedIn,
                                    AmountInvested = amountInvested,
                                    ShopAttitude = shopAttitude,
                                    BuildingQuality = buildingQuality,
                                    CurrentGoldSupply = currentGoldSupply,
                                    CurrentCreditSupply = creditAmt,

                                };
                                //Debug.Log("Adding building " + buildingKey.ToString() + " - quality: " + buildingQuality.ToString());
                                ShopBuildingData.Add(buildingKey, currentBuildKey);
                            }
                        }
                        //Debug.Log("Building " + buildingKey.ToString() + " is present - quality = " + sd.BuildingQuality.ToString());
                    }
                }
            }
            else
                return;
                //Debug.Log("You Just Entered Something Other Than An Open Shop...");
        }

        public static int GoldSupplyAmountGenerator(int buildingQuality)
        {
            //Debug.Log("A shop without investment was just generated.");
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int playerLuck = player.Stats.LiveLuck;
            int currentRegionIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            int regionCostAdjust = GameManager.Instance.PlayerEntity.RegionData[currentRegionIndex].PriceAdjustment / 100;
            float varianceMaker = (UnityEngine.Random.Range(1.001f, 2.751f));
            int buildingQualityMult;
            if (buildingQuality >= 20)
                buildingQualityMult = 5;
            else if (buildingQuality >= 10)
                buildingQualityMult = 3;
            else
                buildingQualityMult = 1;

            return ((int)Mathf.Ceil(((Mathf.Ceil(buildingQuality / 10f) * buildingQualityMult) + (regionCostAdjust + 1)) * (Mathf.Ceil(playerLuck / 3f) * varianceMaker)))*
                   GameManager.Instance.PlayerEntity.Level;
        }

        public static int InvestedGoldSupplyAmountGenerator(int buildingQuality, uint amountInvested, int shopAttitude)
        {
            //Debug.Log("A shop that was invested in refreshed its gold supply, investment taken into account.");
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int playerLuck = player.Stats.LiveLuck - 50;

            float attitudeMulti = 1.00f;
            if (shopAttitude == 1)
                attitudeMulti = .90f;
            else
                attitudeMulti = 1.00f;
            int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
            float investmentMulti = (mercSkill * .01f) + 1;
            uint investmentPostMulti = (uint)Mathf.Floor(amountInvested * investmentMulti);
            float qualityVarianceMod = (UnityEngine.Random.Range(0.2f, 1.1f));
            float qualityInvestMulti = (buildingQuality * (0.03f * qualityVarianceMod) * attitudeMulti);
            uint investmentPostQuality = (uint)Mathf.Floor(investmentPostMulti * qualityInvestMulti);
            //Debug.Log("Player Has: " + amountInvested.ToString() + " Gold Invested Here.");
            //Debug.Log("Invested Amount Has Added: " + investmentPostQuality.ToString() + " Gold To Shop Stock This Cycle.");

            int currentRegionIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            int regionCostAdjust = GameManager.Instance.PlayerEntity.RegionData[currentRegionIndex].PriceAdjustment / 100;
            float varianceMaker = (UnityEngine.Random.Range(1.201f, 2.751f));

            return ((int) Mathf.Ceil(((buildingQuality * 6) + (regionCostAdjust + 1)) *
                       ((UnityEngine.Random.Range(50f, 100f) + playerLuck) * varianceMaker) + investmentPostQuality)) *
                   GameManager.Instance.PlayerEntity.Level;
        }
    }
}