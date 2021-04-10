using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Banking;
using DaggerfallWorkshop.Game.Guilds;
using LimitedGoldShops;
using System;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public partial class LGSTradeWindowText : DaggerfallTradeWindow, IMacroContextProvider
    {
        protected static TextLabel localTextLabelOne;
        protected static TextLabel localTextLabelTwo;
        protected static TextLabel localTextLabelThree;
        protected Button localCloseFilterButton;
        protected bool filterButtonNeedUpdate;
        protected static Button localFilterButton;
        protected static TextBox localFilterTextBox;
        protected static string filterString = null;
        protected static int createCreditAmt = 0;
        protected static string[] itemGroupNames = new string[]
        {
            "drugs",
            "uselessitems1",
            "armor",
            "weapons",
            "magicitems",
            "artifacts",
            "mensclothing",
            "books",
            "furniture",
            "uselessitems2",
            "religiousitems",
            "maps",
            "womensclothing",
            "paintings",
            "gems",
            "plantingredients1",
            "plantingredients2",
            "creatureingredients1",
            "creatureingredients2",
            "creatureingredients3",
            "miscellaneousingredients1",
            "metalingredients",
            "miscellaneousingredients2",
            "transportation",
            "deeds",
            "jewellery",
            "questitems",
            "miscitems",
            "currency"
        };

        public LGSTradeWindowText(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) // Added to attempt to circumvent DFU v0.10.25 issue, working now.
            : base(uiManager, previous, WindowModes.Sell, null)
        {

        }

        public LGSTradeWindowText(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous, WindowModes windowMode, IGuild guild = null)
            : base(uiManager, previous, windowMode, guild)
        {

        }

        protected override void Setup()
        {
            base.Setup();
            SetupShopGoldInfoText();
            SetupTargetIconPanelFilterBox();
        }

        protected void SetupTargetIconPanelFilterBox()
        {
            string toolTipText = string.Empty;
            toolTipText = "Press Filter Button to Open Filter Text Box.\rAnything typed into text box will autofilter.\rFor negative filter, type '-' in front.\rFor example, -steel weapon will find all weapons not made of steel.";


            localFilterTextBox = DaggerfallUI.AddTextBoxWithFocus(new Rect(new Vector2(1, 24), new Vector2(47, 8)), "filter pattern", localTargetIconPanel);
            localFilterTextBox.VerticalAlignment = VerticalAlignment.Bottom;
            localFilterTextBox.OnType += LocalFilterTextBox_OnType;
            localFilterTextBox.OverridesHotkeySequences = true;

            localFilterButton = DaggerfallUI.AddButton(new Rect(40, 25, 15, 8), localTargetIconPanel);
            localFilterButton.Label.Text = "Filter";
            localFilterButton.ToolTip = defaultToolTip;
            localFilterButton.ToolTipText = toolTipText;

            localFilterButton.Label.TextScale = 0.75f;
            localFilterButton.Label.ShadowColor = Color.black;
            localFilterButton.VerticalAlignment = VerticalAlignment.Bottom;
            localFilterButton.HorizontalAlignment = HorizontalAlignment.Left;
            localFilterButton.BackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
            localFilterButton.OnMouseClick += LocalFilterButton_OnMouseClick;

            localCloseFilterButton = DaggerfallUI.AddButton(new Rect(47, 25, 8, 8), localTargetIconPanel);
            localCloseFilterButton.Label.Text = "x";
            localCloseFilterButton.Label.TextScale = 0.75f;
            localCloseFilterButton.Label.ShadowColor = Color.black;
            localCloseFilterButton.VerticalAlignment = VerticalAlignment.Bottom;
            localCloseFilterButton.HorizontalAlignment = HorizontalAlignment.Right;
            localCloseFilterButton.BackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
            localCloseFilterButton.OnMouseClick += LocalCloseFilterButton_OnMouseClick;

            filterButtonNeedUpdate = true;
        }

        public override void OnPop()
        {
            ClearFilterFields();
            base.OnPop();
        }



        public override void Update()
        {
            base.Update();

            if (localFilterTextBox.HasFocus() && Input.GetKeyDown(KeyCode.Return))
                SetFocus(null);

            if (filterButtonNeedUpdate)
            {
                filterButtonNeedUpdate = false;
                UpdateFilterButton();
            }
        }

        protected new void SelectTabPage(TabPages tabPage)
        {
            // Select new tab page
            base.SelectTabPage(tabPage);

            //ClearFilterFields();
            FilterRemoteItems();
            remoteItemListScroller.Items = remoteItemsFiltered;
        }

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
                        if (ItemPassesFilter(item))
                            AddLocalItem(item);
                    }
                    else
                    {
                        if (GameManager.Instance.PlayerEnterExit.BuildingType == DaggerfallConnect.DFLocation.BuildingTypes.Alchemist &&
                           item.LongName.ToLower().Contains("potion"))
                        {
                            if (ItemPassesFilter(item))
                                AddLocalItem(item);
                        }

                    }
                }
            }
        }

        protected override void FilterRemoteItems()
        {
            if (WindowMode == WindowModes.Repair)
            {
                // Clear current references
                remoteItemsFiltered.Clear();

                // Add items to list if they are not being repaired or are being repaired here. 
                if (remoteItems != null)
                {
                    for (int i = 0; i < remoteItems.Count; i++)
                    {
                        DaggerfallUnityItem item = remoteItems.GetItem(i);
                        if (!item.RepairData.IsBeingRepaired() || item.RepairData.IsBeingRepairedHere())
                            remoteItemsFiltered.Add(item);
                        if (item.RepairData.IsRepairFinished())
                            item.currentCondition = item.maxCondition;
                    }
                }
                UpdateRepairTimes(false);
            }
            else
                FilterRemoteItemsWithoutExtraFilter();
        }

        protected void FilterRemoteItemsWithoutExtraFilter()
        {
            // Clear current references
            remoteItemsFiltered.Clear();

            // Add items to list
            if (remoteItems != null)
                for (int i = 0; i < remoteItems.Count; i++)
                    remoteItemsFiltered.Add(remoteItems.GetItem(i));
        }

        protected bool ItemPassesFilter(DaggerfallUnityItem item)
        {
            bool iterationPass = false;

            if (String.IsNullOrEmpty(filterString))
                return true;

            foreach (string word in filterString.Split(' '))
            {
                if (word.Trim().Length > 0)
                {
                    if (word[0] == '-')
                    {
                        string wordLessFirstChar = word.Remove(0, 1);
                        iterationPass = true;
                        if (item.LongName.IndexOf(wordLessFirstChar, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = false;
                        else if (itemGroupNames[(int)item.ItemGroup].IndexOf(wordLessFirstChar, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = false;
                    }
                    else
                    {
                        iterationPass = false;
                        if (item.LongName.IndexOf(word, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = true;
                        else if (itemGroupNames[(int)item.ItemGroup].IndexOf(word, StringComparison.OrdinalIgnoreCase) != -1)
                            iterationPass = true;
                    }

                    if (!iterationPass)
                        return false;
                }
            }

            return true;
        }


        private void UpdateFilterButton()
        {
            if (filterString != null && localFilterButton.Enabled)
            {
                localFilterButton.Enabled = false;
                localFilterTextBox.Enabled = true;
                localCloseFilterButton.Enabled = true;
            }
            else
            {
                localFilterButton.Enabled = true;
                localFilterTextBox.Enabled = false;
                localCloseFilterButton.Enabled = false;
            }
        }
        private void ClearFilterFields()
        {
            filterString = null;
            localFilterTextBox.Text = string.Empty;
            UpdateFilterButton();
        }

        private void LocalFilterButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            filterString = "";
            localFilterTextBox.SetFocus();
            filterButtonNeedUpdate = true;
        }

        private void LocalCloseFilterButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ClearFilterFields();
            Refresh(false);
            filterButtonNeedUpdate = true;
        }


        private void LocalFilterTextBox_OnType()
        {
            filterString = localFilterTextBox.Text.ToLower();
            Refresh(false);
        }



        protected override void ShowTradePopup()
        {
            const int tradeMessageBaseId = 260;
            const int notEnoughGoldId = 454;
            int msgOffset = 0;
            int tradePrice = GetTradePrice();

            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            int goldSupply = 0;
            int shopAttitude = 0; // For now, 0 = Nice Attitude and 1 = Bad Attitude.
            int creditAmt = 0;
            bool ignore = true;
            LimitedGoldShops.ShopData sd;
            if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.OnButtonClick += ConfirmPoorTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
            else if (WindowMode != WindowModes.Sell && WindowMode != WindowModes.SellMagic && PlayerEntity.GetGoldAmount() + creditAmt < tradePrice)
            {
                DaggerfallUI.MessageBox(notEnoughGoldId);
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
                messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
        }

        protected void SetupShopGoldInfoText()
        {
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            LimitedGoldShops.ShopData sd;
            int goldSupply = 0;
            uint investedAmount = 0;
            int creditAmt = 0;
            if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
            LimitedGoldShops.ShopData sd;
            if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
            bool receivedLetterOfCredit = false;

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                // Proceed with trade.
                int tradePrice = GetTradePrice();
                switch (WindowMode)
                {
                    case WindowModes.Sell:
                    case WindowModes.SellMagic:
                        float goldWeight = tradePrice * DaggerfallBankManager.goldUnitWeightInKg;
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
                LimitedGoldShops.LimitedGoldShops.TradeUpdateShopGold(WindowMode, tradePrice);
                UpdateShopGoldDisplay();
				Refresh();
            }
            CloseWindow();
          //  if (receivedLetterOfCredit)
            //    DaggerfallUI.MessageBox(TextManager.Instance.GetText(textDatabase, "letterOfCredit"));
        }

        protected void ConfirmPoorTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            bool receivedLetterOfCredit = false;
            int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            LimitedGoldShops.ShopData sd;
            int goldSupply = 0;
            int creditAmt = 0;
            if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                creditAmt = sd.CurrentCreditSupply;

            }

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                float goldWeight = goldSupply * DaggerfallBankManager.goldUnitWeightInKg;
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
                LimitedGoldShops.LimitedGoldShops.TradeUpdateShopGold(WindowMode, goldSupply, createCreditAmt);
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
                if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
