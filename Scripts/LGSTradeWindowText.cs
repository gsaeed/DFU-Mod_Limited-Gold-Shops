using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Banking;
using DaggerfallWorkshop.Game.Guilds;
using LimitedGoldShops;
using System;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Questing;


namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public partial class LGSTradeWindowText : AsesinoInfoTradeWindow, IMacroContextProvider
    {
        protected static TextLabel localTextLabelOne;
        protected static TextLabel localTextLabelTwo;
        protected static TextLabel localTextLabelThree;
        protected static int createCreditAmt = 0;

        public LGSTradeWindowText(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) // Added to attempt to circumvent DFU v0.10.25 issue, working now.
            : base(uiManager, previous, WindowModes.Sell, null)
        {

        }

        public LGSTradeWindowText(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous, WindowModes windowMode, IGuild guild = null)
            : base(uiManager, previous, windowMode, guild)
        {

        }


        static new Dictionary<DFLocation.BuildingTypes, List<ItemGroups>> storeBuysItemType = new Dictionary<DFLocation.BuildingTypes, List<ItemGroups>>()
        {
            { DFLocation.BuildingTypes.Alchemist, new List<ItemGroups>()
                { ItemGroups.Gems, ItemGroups.CreatureIngredients1, ItemGroups.CreatureIngredients2, ItemGroups.CreatureIngredients3, ItemGroups.PlantIngredients1, ItemGroups.PlantIngredients2, ItemGroups.MiscellaneousIngredients1, ItemGroups.MiscellaneousIngredients2, ItemGroups.MetalIngredients } },
            { DFLocation.BuildingTypes.Armorer, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Weapons } },
            { DFLocation.BuildingTypes.Bookseller, new List<ItemGroups>()
                { ItemGroups.Books } },
            { DFLocation.BuildingTypes.ClothingStore, new List<ItemGroups>()
                { ItemGroups.MensClothing, ItemGroups.WomensClothing } },
            { DFLocation.BuildingTypes.FurnitureStore, new List<ItemGroups>()
                { ItemGroups.Furniture } },
            { DFLocation.BuildingTypes.GemStore, new List<ItemGroups>()
                { ItemGroups.Gems, ItemGroups.Jewellery } },
            { DFLocation.BuildingTypes.GeneralStore, new List<ItemGroups>()
                { ItemGroups.Books, ItemGroups.MensClothing, ItemGroups.WomensClothing, ItemGroups.Transportation, ItemGroups.Jewellery, ItemGroups.Weapons, ItemGroups.UselessItems2 } },
            { DFLocation.BuildingTypes.PawnShop, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Books, ItemGroups.MensClothing, ItemGroups.WomensClothing, ItemGroups.Gems, ItemGroups.Jewellery, ItemGroups.ReligiousItems, ItemGroups.Weapons, ItemGroups.UselessItems2, ItemGroups.Paintings } },
            { DFLocation.BuildingTypes.WeaponSmith, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Weapons } },
        };


        protected override void FilterLocalItems()
        {
            localItemsFiltered.Clear();

            // Add any basket items to filtered list first, if not using wagon
            if (WindowMode == WindowModes.Buy && !UsingWagon && BasketItems != null)
            {
                for (int i = 0; i < BasketItems.Count; i++)
                {
                    DaggerfallUnityItem item = BasketItems.GetItem(i);
                    // Add if not equipped
                    if (!item.IsEquipped)
                    {
                        AddLocalItem(item);
                    }

                }
            }
            // Add local items to filtered list
            if (localItems != null)
            {
                for (int i = 0; i < localItems.Count; i++)
                {
                    // Add if not equipped & accepted for selling
                    DaggerfallUnityItem item = localItems.GetItem(i);
                    if (!item.IsEquipped && (
                            (WindowMode != WindowModes.Sell && WindowMode != WindowModes.SellMagic) ||
                            (WindowMode == WindowModes.Sell && ItemTypesAccepted.Contains(item.ItemGroup)) ||
                            (WindowMode == WindowModes.SellMagic && item.IsEnchanted)))
                    {

                        if (!item.protectedItem && FilterUtilities.ItemPassesFilter(filterString, item) && TabPassesFilter(item) &&
                            (!LimitedGoldShopsMain.ShopStandardsSetting || LimitedGoldShopsMain.ShopStandardsSetting &&
                                (IsItemWithinShopStandards(item) || !IsItemWithinShopStandards(item) &&
                                LimitedGoldShopsMain.ShopIgnoresStandardsForMagicalItems)) &&
                            (WindowMode == WindowModes.Identify ||
                             (!item.IsEnchanted || item.IsEnchanted && item.IsIdentified) ||
                             (LimitedGoldShopsMain.CanSellUnidentifiedItems && item.IsEnchanted && !item.IsIdentified && !LimitedGoldShopsMain.OnlyQualityShopsCanBuyUnidentifiedItems) ||
                             (LimitedGoldShopsMain.CanSellUnidentifiedItems && item.IsEnchanted && !item.IsIdentified && LimitedGoldShopsMain.OnlyQualityShopsCanBuyUnidentifiedItems &&
                              GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality >= LimitedGoldShopsMain.ShopQualityNeededToBuyUnidentifiedItems) ||
                             (!LimitedGoldShopsMain.CanSellUnidentifiedItems && (!item.IsEnchanted || item.IsIdentified))))
                            AddLocalItem(item);
                    }
                    else
                    {
                        if (GameManager.Instance.PlayerEnterExit.BuildingType == DaggerfallConnect.DFLocation.BuildingTypes.Alchemist &&
                           item.LongName.ToLower().Contains("potion"))
                        {
                            if (!item.protectedItem && FilterUtilities.ItemPassesFilter(filterString, item) && TabPassesFilter(item) &&
                                (!LimitedGoldShopsMain.ShopStandardsSetting || LimitedGoldShopsMain.ShopStandardsSetting &&
                                    IsItemWithinShopStandards(item)))
                                AddLocalItem(item);
                        }

                    }
                }

                if (localItemsFiltered.Count > 0)
                    FilterUtilities.SortMe(SortCriteria,ref localItemsFiltered);
            }
        }
        public override void OnPush()
        {
            base.OnPush();
            if (!IsSetup)
                Setup();

            SortCriteria = 3;
            SetLocalSortButton(SortCriteria);
        }


       
        public bool IsItemWithinShopStandards(DaggerfallUnityItem itemChecked)
        {
            int baseGoldValue = FormulaHelper.CalculateBaseCost(itemChecked);

            if (!(GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.GeneralStore ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.PawnShop ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.Armorer ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.WeaponSmith ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.Alchemist ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.Bookseller ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.ClothingStore ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.GemStore ||
                GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType ==
                DFLocation.BuildingTypes.GeneralStore))
            {
                return true;
            }

            if (itemChecked.ItemGroup == ItemGroups.Weapons || itemChecked.ItemGroup == ItemGroups.Armor)
            {
                if (!LimitedGoldShopsMain.CheckWeaponsArmor)
                    return true;

                var material = GetMaterialTypeInt(itemChecked.NativeMaterialValue);

                if (itemChecked.IsArtifact)
                    material = 11;

                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        if (itemChecked.IsEnchanted)
                        {
                            if (material >= 3)
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            if (material >= 4)
                                return false;
                            else
                                return true;                            
                        }


                    case 7:
                    case 8:
                    case 9:
                        if (itemChecked.IsEnchanted)
                        {
                            if (material <= 0)
                                return false;
                            else if (material > 4)
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            if (material <= 2)
                                return false;
                            else if (material > 6)
                                return false;
                            else
                                return true;
                        }
                    case 12:
                    case 13:
                    case 14:
                        if (itemChecked.IsEnchanted)
                        {
                            if (material <= 1)
                                return false;
                            else if (material >= 6)
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            if (material <= 3)
                                return false;
                            else if (material >= 8)
                                return false;
                            else
                                return true;
                        }
                    case 15:
                    case 16:
                    case 17:
                        if (itemChecked.IsEnchanted)
                        {
                            if (material <= 3)
                                return false;
                            else if (material >= 8)
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            if (material <= 5)
                                return false;
                            else if (material >= 10)
                                return false;
                            else
                                return true;
                        }
                    case 18:
                    case 19:
                    case 20:
                        if (itemChecked.IsEnchanted)
                        {
                            if (material <= 4)
                                return false;
                            else
                                return true;
                        }
                        else
                        {
                            if (material <= 6)
                                return false;
                            else
                                return true;
                        }
                    default:
                        return true;
                }
            }
            else if (itemChecked.ItemGroup == ItemGroups.MensClothing || itemChecked.ItemGroup == ItemGroups.WomensClothing)
            {
                if (!LimitedGoldShopsMain.CheckClothing)
                    return true;

                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        if (baseGoldValue <= 9)
                            return false;
                        else
                            return true;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        if (baseGoldValue <= 24)
                            return false;
                        else
                            return true;
                    case 18:
                    case 19:
                    case 20:
                        if (baseGoldValue <= 99)
                            return false;
                        else
                            return true;
                    default:
                        return true;
                }
            }
            else if (itemChecked.ItemGroup == ItemGroups.ReligiousItems)
            {
                if (!LimitedGoldShopsMain.CheckReligiousItems)
                    return true;

                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        if (baseGoldValue <= 15)
                            return false;
                        else
                            return true;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        if (baseGoldValue <= 40)
                            return false;
                        else
                            return true;
                    case 18:
                    case 19:
                    case 20:
                        if (baseGoldValue <= 99)
                            return false;
                        else
                            return true;
                    default:
                        return true;
                }
            }
            else if (itemChecked.IsIngredient)
            {
                if (!LimitedGoldShopsMain.CheckIngredients)
                    return true;

                switch (GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        if (baseGoldValue <= 7)
                            return false;
                        else
                            return true;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        if (baseGoldValue <= 19)
                            return false;
                        else
                            return true;
                    case 18:
                    case 19:
                    case 20:
                        if (baseGoldValue <= 29)
                            return false;
                        else
                            return true;
                    default:
                        return true;
                }
            }
            else
                return true;
        }

        public int GetMaterialTypeInt(int nativeMatValue) // -1 = Unknown, 0 = Leather, 1 = Chain, 2 = Iron, 3 = Steel, 4 = Silver, 5 = Elven, 6 = Dwarven, 7 = Mithril, 8 = Adamantium, 9 = Ebony, 10 = Orcish, 11 = Daedric.
        {
            if (nativeMatValue <= 9 && nativeMatValue >= 0) // Checks if the item material is for weapons, and leather armor.
            {
                switch (nativeMatValue)
                {
                    case (int)WeaponMaterialTypes.Iron:
                        return 2;
                    case (int)WeaponMaterialTypes.Steel:
                        return 3;
                    case (int)WeaponMaterialTypes.Silver:
                        return 4;
                    case (int)WeaponMaterialTypes.Elven:
                        return 5;
                    case (int)WeaponMaterialTypes.Dwarven:
                        return 6;
                    case (int)WeaponMaterialTypes.Mithril:
                        return 7;
                    case (int)WeaponMaterialTypes.Adamantium:
                        return 8;
                    case (int)WeaponMaterialTypes.Ebony:
                        return 9;
                    case (int)WeaponMaterialTypes.Orcish:
                        return 10;
                    case (int)WeaponMaterialTypes.Daedric:
                        return 11;
                    default:
                        return 0; // Leather should default to this.
                }
            }
            else if (nativeMatValue <= 521 && nativeMatValue >= 256) // Checks if the item material is for armors.
            {
                switch (nativeMatValue)
                {
                    case (int)ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                        return 1;
                    case (int)ArmorMaterialTypes.Iron:
                        return 2;
                    case (int)ArmorMaterialTypes.Steel:
                        return 3;
                    case (int)ArmorMaterialTypes.Silver:
                        return 4;
                    case (int)ArmorMaterialTypes.Elven:
                        return 5;
                    case (int)ArmorMaterialTypes.Dwarven:
                        return 6;
                    case (int)ArmorMaterialTypes.Mithril:
                        return 7;
                    case (int)ArmorMaterialTypes.Adamantium:
                        return 8;
                    case (int)ArmorMaterialTypes.Ebony:
                        return 9;
                    case (int)ArmorMaterialTypes.Orcish:
                        return 10;
                    case (int)ArmorMaterialTypes.Daedric:
                        return 11;
                    default:
                        return -1;
                }
            }
            else
                return -1;
        }

        protected override void Setup()
        {
            base.Setup();
            SetupShopGoldInfoText();
        }


        protected override void ShowTradePopup()
        {
            const int tradeMessageBaseId = 260;
            //const int notEnoughGoldId = 454;
            int msgOffset = 0;
            int tradePrice = GetTradePrice();

            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            int goldSupply = 0;
            int shopAttitude = 0; // For now, 0 = Nice Attitude and 1 = Bad Attitude.
            int creditAmt = 0;
            bool ignore = true;
            LimitedGoldShops.ShopData sd;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                shopAttitude = sd.ShopAttitude;
                creditAmt = sd.CurrentCreditSupply;
            }

            if (WindowMode == WindowModes.Sell && goldSupply <= 0 && !ignore)
            {
                int buildQual = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
                TextFile.Token[] tokens = LGSTextTokenHolder.ShopTextTokensNice(4);
                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0) // Possibly could add the "shop type" to the save-data as well, and use that to determine some other things.
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(5);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(4);
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(6);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(5);
                }
                else if (buildQual <= 13) // 08 - 13
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(4);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(6);
                }
                else if (buildQual <= 17) // 14 - 17
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(7);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(7);
                }
                else                      // 18 - 20
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(8);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(8);
                }

                DaggerfallMessageBox noGoldShop = new DaggerfallMessageBox(DaggerfallUI.UIManager, this);
                noGoldShop.SetTextTokens(tokens, this);
                noGoldShop.ClickAnywhereToClose = true;
                uiManager.PushWindow(noGoldShop);
            }
            else if (WindowMode == WindowModes.Sell && goldSupply < tradePrice)
            {
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                    TextFile.Formatting.JustifyCenter,
                    "I can offer " + tradePrice.ToString() +
                    ".  Would you be willing to take " + goldSupply.ToString() + " in gold and take credit",
                    "for " + (tradePrice - goldSupply).ToString() + "?");

                createCreditAmt = (tradePrice - goldSupply);

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetTextTokens(tokens, this);
                messageBox.ClickAnywhereToClose = false;
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Counter);
                messageBox.OnButtonClick += ConfirmPoorTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
            else if (WindowMode != WindowModes.Sell && WindowMode != WindowModes.SellMagic && PlayerEntity.GetGoldAmount() + creditAmt < tradePrice)
            {
                DaggerfallUI.MessageBox($"That will cost you {tradePrice}, you do not have enough gold.");
            }
            else
            {
                if (cost >> 1 <= tradePrice)
                {
                    if (cost - (cost >> 2) <= tradePrice)
                        msgOffset = 2;
                    else
                        msgOffset = 1;
                }
                if (WindowMode == WindowModes.Sell || WindowMode == WindowModes.SellMagic)
                    msgOffset += 3;

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRandomTokens(tradeMessageBaseId + msgOffset);
                messageBox.SetTextTokens(tokens, this);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Counter);
                messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
        }

        protected void SetupShopGoldInfoText()
        {
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            ShopData sd;
            int goldSupply = 0;
            uint investedAmount = 0;
            int creditAmt = 0;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                creditAmt = sd.CurrentCreditSupply;
                investedAmount = sd.AmountInvested;
                
            }

            if (!(playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.Temple) && !(playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.GuildHall))
            {
                localTextLabelOne = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 24), string.Empty, NativePanel);
                localTextLabelOne.TextScale = 0.85f;
                localTextLabelOne.Text = "Invested: " + investedAmount.ToString();

                localTextLabelTwo = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 31), string.Empty, NativePanel);
                localTextLabelTwo.TextScale = 0.85f;
                localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();

                localTextLabelThree = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 38), string.Empty, NativePanel);
                localTextLabelThree.TextScale = 0.85f;
                localTextLabelThree.Text = "Credit: " + creditAmt.ToString();
            }
            UpdateShopGoldDisplay();
        }


        void DeductGoldAmount(int tradePrice)
        {
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            ShopData sd;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                if (sd.CurrentCreditSupply > 0)
                {
                    if (sd.CurrentCreditSupply > tradePrice)
                        return;

                    tradePrice = tradePrice - sd.CurrentCreditSupply;
                }   
            }
            PlayerEntity.DeductGoldAmount(tradePrice);
            return;
        }

        protected override void ConfirmTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            PopWindow();
            if (messageBoxButton != DaggerfallMessageBox.MessageBoxButtons.Counter)
            {
                ConfirmTrade(sender, messageBoxButton, GetTradePrice());
                return;
            }

            // Show message box
            DaggerfallInputMessageBox mb = new DaggerfallInputMessageBox(uiManager, this);
            mb.SetTextBoxLabel("Counter Offer?");
            mb.TextPanelDistanceY = 0;
            mb.InputDistanceX = 15;
            mb.TextBox.Numeric = true;
            mb.ClickAnywhereToClose = false;
            mb.TextBox.MaxCharacters = 8;
            mb.TextBox.Text = GetTradePrice().ToString();
            mb.OnGotUserInput += (inputSender, input) => ConfirmTrade_OnGotUserInput(sender, input, GetTradePrice().ToString());
            mb.Show();

        }

        private void ConfirmTrade_OnGotUserInput(DaggerfallMessageBox sender, string input, string originalPrice, bool poorTrade = false)
        {
            string direction = WindowMode == WindowModes.Sell || WindowMode == WindowModes.SellMagic ? "high" : "low";
            string[] accepts = new string[]
            {
                "I reluctantly accept your offer.",
                "Alright, I'll take it.",
                "Deal. Let's move forward.",
                "I accept, but I'm not thrilled about it.",
                "Fine, you have a deal.",
                "I guess this will do.",
                "Alright, let's do it.",
                "I accept your terms.",
                "Okay, I'll go with that.",
                "Sure, let's make it happen.",
                "I accept, but I hope for better next time.",
                "Alright, I'll make it up on the next one.",
                "Deal. Let's get this done.",
                "I accept, let's proceed.",
                "Okay, I'll take your offer.",
                "I accept, but I'm not happy about it.",
                "Alright, let's finalize this.",
                "Deal. I'm in.",
                "I accept, let's move forward.",
                "Okay, let's do this.",
                "I accept, but I expect better next time.",
                "Alright, let's make it happen.",
                "Deal. Let's get started.",
                "I accept, let's get this over with.",
                "Okay, I'll take it for now."
            };
            string[] declines = new string[]
            {
                "That is an insulting offer.",
                "I'm sorry, but I can't accept that.",
                $"No deal, that's too {direction}.",
                "I can't do it for that price.",
                "That's not going to work for me.",
                "I have to decline.",
                "No, that's not acceptable.",
                "I can't agree to that.",
                "Sorry, but I have to refuse.",
                "That's not enough.",
                "I can't take that offer.",
                "No, I can't do it.",
                $"That's not {direction} enough for me.",
                $"I can't accept such a {direction} offer.",
                "I'm afraid I have to decline.",
                "No, that's not going to happen.",
                $"I can't go that {direction}.",
                "That's not a fair price.",
                "I have to say no.",
                "I can't agree to those terms.",
                "I can't accept that amount.",
                "No, I can't agree to that.",
                "That's not going to work.",
                "I have to turn down your offer."
            };
            if (string.IsNullOrEmpty(input))
                return;
            
            var offer = int.Parse(input);
            var originalcost = int.Parse(originalPrice);
            var difference = 0.0f;
            var accept = accepts[UnityEngine.Random.Range(0,accepts.Length)];
            var decline = declines[UnityEngine.Random.Range(0, declines.Length)];
            var acceptMB = DaggerfallMessageBox.MessageBoxButtons.Yes;
            var declineMB = DaggerfallMessageBox.MessageBoxButtons.No;
            var response = accept;
            var responseMB = acceptMB;
            if (!GameManager.Instance.PlayerEnterExit.IsPlayerInsideOpenShop)
            {
                response = decline;
                responseMB = declineMB;
            }
            else
            {
                var quality = GameManager.Instance.PlayerEnterExit.Interior.BuildingData.Quality;
                var playerMercantile =
                    GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
                var player = GameManager.Instance.PlayerEntity;
                var luck = player.Stats.LiveLuck;

                int merchantMercantileLevel = 5 * (quality - 10) + 50;
                int merchantPersonalityLevel = 5 * (quality - 10) + 50;
                int inventoryAging = 0;
                if (lootTarget != null)
                    inventoryAging = lootTarget.stockedDate - DaggerfallLoot.CreateStockedDate(DaggerfallUnity.Instance.WorldTime.Now);
                int deltaMercantile;
                int deltaPersonality;

                if (WindowMode == WindowModes.Sell || WindowMode == WindowModes.SellMagic)
                {
                    deltaMercantile = (((100 - merchantMercantileLevel) * 256) / 200 + 128) * (((player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile)) * 256) / 200 + 128) / 256;
                    deltaPersonality = (((100 - merchantPersonalityLevel) * 256) / 200 + 128) * ((player.Stats.LivePersonality * 256) / 200 + 128) / 256;

                    difference = (offer - originalcost) / (originalcost * 1f) * 100;
                }
                else // buying
                {
                    deltaMercantile = ((merchantMercantileLevel * 256) / 200 + 128) * (((100 - (player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile))) * 256) / 200 + 128) / 256;
                    deltaPersonality = ((merchantPersonalityLevel * 256) / 200 + 128) * (((100 - player.Stats.LivePersonality) * 256) / 200 + 128) / 256;

                    difference = (originalcost - offer) / (originalcost * 1f) * 100;
                }

                var willing = ((deltaMercantile /
                                Mathf.Clamp(player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile), 5, 200)) +
                               (deltaPersonality / Mathf.Clamp(player.Stats.LivePersonality, 5, 200))) / 2;


                willing += Mathf.Clamp(UnityEngine.Random.Range(-10, 10) +
                                       Mathf.Clamp(GameManager.Instance.PlayerEntity.SGroupReputations[1]/10, -10, 10), 0,100);
                if (Dice100.SuccessRoll(luck / 10))
                    willing += 40;
                else if (Dice100.SuccessRoll(luck / 5))
                    willing += 10;
                else if (Dice100.SuccessRoll(luck / 5))
                    willing -= 10;
                else if (Dice100.SuccessRoll(luck / 10))
                    willing -= 40;

                willing = Mathf.Clamp(willing, 5, 100);
                if (willing - difference > 0)
                {
                    response = accept;
                    responseMB = acceptMB;
                    player.TallySkill(DFCareer.Skills.Mercantile, (short)LimitedGoldShopsMain.SuccessfulCounterOffer);
                }
                else
                {
                    response = decline;
                    responseMB = declineMB;
                    player.TallySkill(DFCareer.Skills.Mercantile, (short)LimitedGoldShopsMain.FailedCounterOffer);

                }


            }

            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
            messageBox.SetText(response, this);
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.OK);
            messageBox.ClickAnywhereToClose = false;
            if (poorTrade)
                messageBox.OnButtonClick += (sender2, button) => ConfirmPoorTrade(sender, responseMB, int.Parse(input));
            else
                messageBox.OnButtonClick += (sender2, button) => ConfirmTrade(sender, responseMB, int.Parse(input));
            uiManager.PushWindow(messageBox);
        }


        protected void ConfirmTrade(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton, int tradePrice)
        {
            bool receivedLetterOfCredit = false;

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                // Proceed with trade.
                switch (WindowMode)
                {
                    case WindowModes.Sell:
                    case WindowModes.SellMagic:
                        float goldWeight = tradePrice * DaggerfallBankManager.goldPieceWeightInKg;
                        if (PlayerEntity.CarriedWeight + goldWeight <= PlayerEntity.MaxEncumbrance)
                        {
                            PlayerEntity.GoldPieces += tradePrice;
                        }
                        else
                        {
                            DaggerfallUnityItem loc = ItemBuilder.CreateItem(ItemGroups.MiscItems, (int)MiscItems.Letter_of_credit);
                            loc.value = tradePrice;
                            GameManager.Instance.PlayerEntity.Items.AddItem(loc, Items.ItemCollection.AddPosition.Front);
                            receivedLetterOfCredit = true;
                        }
                        RaiseOnTradeHandler(remoteItems.GetNumItems(), tradePrice);
                        remoteItems.Clear();
                        break;


                    case WindowModes.Buy:
                        DeductGoldAmount(tradePrice);
                        RaiseOnTradeHandler(basketItems.GetNumItems(), tradePrice);
                        PlayerEntity.Items.TransferAll(basketItems);
                        break;

                    case WindowModes.Repair:

                        DeductGoldAmount(tradePrice);
                        if (DaggerfallUnity.Settings.InstantRepairs)
                        {
                            foreach (DaggerfallUnityItem item in remoteItemsFiltered)
                                item.currentCondition = item.maxCondition;
                        }
                        else
                        {
                            UpdateRepairTimes(true);
                        }
                        RaiseOnTradeHandler(remoteItems.GetNumItems(), tradePrice);
                        break;

                    case WindowModes.Identify:
                        DeductGoldAmount(tradePrice);
                        for (int i = 0; i < remoteItems.Count; i++)
                        {
                            DaggerfallUnityItem item = remoteItems.GetItem(i);
                            item.IdentifyItem();
                        }
                        RaiseOnTradeHandler(remoteItems.GetNumItems(), tradePrice);
                        break;
                }
                if (receivedLetterOfCredit)
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.ParchmentScratching);
                else
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                PlayerEntity.TallySkill(DFCareer.Skills.Mercantile, 1);
                LimitedGoldShops.LimitedGoldShopsMain.TradeUpdateShopGold(WindowMode, tradePrice);
                UpdateShopGoldDisplay();
				Refresh();
            }
            while (uiManager.TopWindow != this)
                CloseWindow();
            if (receivedLetterOfCredit)
                DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("letterOfCredit"));

        }

        protected void ConfirmPoorTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            PopWindow();
            if (messageBoxButton != DaggerfallMessageBox.MessageBoxButtons.Counter)
            {
                ConfirmPoorTrade(sender, messageBoxButton, GetTradePrice());
                return;
            }

            // Show message box
            DaggerfallInputMessageBox mb = new DaggerfallInputMessageBox(uiManager, this);
            mb.SetTextBoxLabel("Counter Offer?");
            mb.TextPanelDistanceY = 0;
            mb.InputDistanceX = 15;
            mb.TextBox.Numeric = true;
            mb.ClickAnywhereToClose = false;
            mb.TextBox.MaxCharacters = 8;
            mb.TextBox.Text = GetTradePrice().ToString();
            mb.OnGotUserInput += (inputSender, input) => ConfirmTrade_OnGotUserInput(sender, input, GetTradePrice().ToString(), true);
            mb.Show();
        }

        protected void ConfirmPoorTrade(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton, int tradePrice)
        {
            bool receivedLetterOfCredit = false;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            LimitedGoldShops.ShopData sd;
            int goldSupply = 0;
            int creditAmt = 0;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                creditAmt = sd.CurrentCreditSupply;

            }

            createCreditAmt = (tradePrice - goldSupply);


            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                float goldWeight = goldSupply * DaggerfallBankManager.goldPieceWeightInKg;
                if (PlayerEntity.CarriedWeight + goldWeight <= PlayerEntity.MaxEncumbrance)
                {
                    PlayerEntity.GoldPieces += goldSupply;
                    
                }
                else
                {
                    DaggerfallUnityItem loc = ItemBuilder.CreateItem(ItemGroups.MiscItems, (int)MiscItems.Letter_of_credit);
                    loc.value = goldSupply;
                    GameManager.Instance.PlayerEntity.Items.AddItem(loc, Items.ItemCollection.AddPosition.Front);
                    receivedLetterOfCredit = true;
                }
                RaiseOnTradeHandler(remoteItems.GetNumItems(), goldSupply);
                remoteItems.Clear();

                if (receivedLetterOfCredit)
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.ParchmentScratching);
                else
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                PlayerEntity.TallySkill(DFCareer.Skills.Mercantile, 1);
                LimitedGoldShops.LimitedGoldShopsMain.TradeUpdateShopGold(WindowMode, goldSupply, createCreditAmt);
                UpdateShopGoldDisplay();
				Refresh();
            }
            CloseWindow();
        //    if (receivedLetterOfCredit)
          //      DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "letterOfCredit"));
        }

        public void UpdateShopGoldDisplay()
        {
            try // This is here to "resolve" whenever a building that does not count as a shop, has the trade-window opened up inside of it if the game has been loaded and no "valid" shop buildings have been entered. Essentially it threw an exception because the dictionary had nothing to give, so this is there to catch that in that situation.
            {
                int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
                LimitedGoldShops.ShopData sd;
                int goldSupply = 0;
                uint investedAmount = 0;
                int creditAmt = 0;
                if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
                {
                    goldSupply = sd.CurrentGoldSupply;
                    investedAmount = sd.AmountInvested;
                    creditAmt = sd.CurrentCreditSupply;
                }

                localTextLabelOne.Text = "Invested: " + investedAmount.ToString();
                localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();
                localTextLabelThree.Text = "Credit: " + creditAmt.ToString();
            }
            catch
            {
                //Debug.Log("You opened the trade menu inside a Temple or a Guild-Hall, as your first entered building since launching the game.");
            }
        }
    }
}
