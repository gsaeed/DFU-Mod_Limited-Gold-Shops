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

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public partial class LGSTradeWindowText : DaggerfallTradeWindow, IMacroContextProvider
    {
        protected static TextLabel localTextLabelOne;
        protected static TextLabel localTextLabelTwo;

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
            LimitedGoldShops.ShopData sd;
            if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                shopAttitude = sd.ShopAttitude;
            }

            if (WindowMode == WindowModes.Sell && goldSupply <= 0)
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
                    "I can't afford your offer.",
                    "Would you be willing to settle",
                    "for the " + goldSupply.ToString() + " gold that I have left?");

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetTextTokens(tokens, this);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.OnButtonClick += ConfirmPoorTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
            else if (WindowMode != WindowModes.Sell && WindowMode != WindowModes.SellMagic && PlayerEntity.GetGoldAmount() < tradePrice)
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
            if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
            {
                goldSupply = sd.CurrentGoldSupply;
                investedAmount = sd.AmountInvested;
            }

            if (!(playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.Temple) && !(playerEnterExit.BuildingDiscoveryData.buildingType == DFLocation.BuildingTypes.GuildHall))
            {
                localTextLabelOne = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 34), string.Empty, NativePanel);
                localTextLabelOne.TextScale = 0.85f;
                localTextLabelOne.Text = "Invested: " + investedAmount.ToString();

                localTextLabelTwo = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(263, 41), string.Empty, NativePanel);
                localTextLabelTwo.TextScale = 0.85f;
                localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();
            }
            UpdateShopGoldDisplay();
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
                        PlayerEntity.DeductGoldAmount(tradePrice);
                        RaiseOnTradeHandler(basketItems.GetNumItems(), tradePrice);
                        PlayerEntity.Items.TransferAll(basketItems);
                        break;

                    case WindowModes.Repair:
                        PlayerEntity.DeductGoldAmount(tradePrice);
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
                        PlayerEntity.DeductGoldAmount(tradePrice);
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
            if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
                goldSupply = sd.CurrentGoldSupply;

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
                LimitedGoldShops.LimitedGoldShops.TradeUpdateShopGold(WindowMode, goldSupply);
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
                if (LimitedGoldShops.LimitedGoldShops.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
                {
                    goldSupply = sd.CurrentGoldSupply;
                    investedAmount = sd.AmountInvested;
                }

                localTextLabelOne.Text = "Invested: " + investedAmount.ToString();
                localTextLabelTwo.Text = "Shop Gold: " + goldSupply.ToString();
            }
            catch
            {
                //Debug.Log("You opened the trade menu inside a Temple or a Guild-Hall, as your first entered building since launching the game.");
            }
        }
    }
}
