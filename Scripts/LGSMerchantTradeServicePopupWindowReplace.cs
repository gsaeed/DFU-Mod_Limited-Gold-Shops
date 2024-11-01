using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Utility.AssetInjection;
using LimitedGoldShops;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class LGSMerchantTradeServicePopupWindowReplace : DaggerfallMerchantServicePopupWindow
    {
        TextFile.Token[] tokens = null;
        Entity.PlayerEntity player = GameManager.Instance.PlayerEntity;
        int buildQual = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;
        int currentBuildingID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
        ShopData sd;
        bool investedFlag = false;
        int shopAttitude = 0;
        int investOffer = 0;
        List<DaggerfallUnityItem> validTradeItems = new List<DaggerfallUnityItem>();
        List<DaggerfallUnityItem> orderedValidTradeItems = new List<DaggerfallUnityItem>();
        private DaggerfallUnityItem selectedItem;
        private int selectedItemAdjustedValue;
        private bool buildingSummaryFound = false;
        BuildingSummary buildingSummary = default;
        int[] qualitySpecs = {0, 61, 76, 92, 100};
        struct minMaxRange
        {
            public int Min;
            public int Max;

            public minMaxRange(int x, int y)
            {
                Min = x;
                Max = y;
            }

        }

        private Dictionary<int, minMaxRange> BankRanges = new Dictionary<int, minMaxRange>();


        #region UI Rects

        Rect talkButtonRect = new Rect(5, 5, 120, 7);
        Rect investButtonRect = new Rect(5, 14, 120, 7);
        Rect serviceButtonRect = new Rect(5, 23, 55, 7);
        Rect tradeButtonRect = new Rect(65, 23, 55, 7);
        Rect exitButtonRect = new Rect(44, 33, 43, 15);

        #endregion

        #region UI Controls

        Button investButton = new Button();
        Button tradeButton = new Button();
        TextLabel tradeLabel = new TextLabel();
        #endregion

        #region Fields

        const string baseTextureName = "LGS_Invest_Popup_Services_Replacer";      // Talk / Invest / Sell

        #endregion

        #region Constructors

        public LGSMerchantTradeServicePopupWindowReplace(IUserInterfaceManager uiManager, StaticNPC npc, Services service)
            : base(uiManager, npc, service)
        {

        }

        #endregion

        

        protected override void Setup()
        {
            // Load all textures
            LoadTextures();
            selectedItem = null;
            // Create interface panel
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            mainPanel.BackgroundTexture = baseTexture;
            mainPanel.Position = new Vector2(0, 50);
            mainPanel.Size = new Vector2(130, 51);
            BuildingDirectory buildingDirectory = GameManager.Instance.StreamingWorld.GetCurrentBuildingDirectory();


            buildingSummaryFound = true;
            if (buildingDirectory && !buildingDirectory.GetBuildingSummary(
                    GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey, out buildingSummary))
                buildingSummaryFound = false;

            // Talk button
            talkButton = DaggerfallUI.AddButton(talkButtonRect, mainPanel);
            //talkButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            talkButton.OnMouseClick += TalkButton_OnMouseClick;

            // Invest button
            investButton = DaggerfallUI.AddButton(investButtonRect, mainPanel);
            //investButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            investButton.OnMouseClick += InvestButton_OnMouseClick;

            if (GetServiceLabelText() == TextManager.Instance.GetLocalizedText("serviceBanking")
                && buildingSummaryFound && buildingSummary.Quality >= 8)
            {
                BankRanges.Add(8, new minMaxRange(10, 25));
                BankRanges.Add(9, new minMaxRange(15, 50));
                BankRanges.Add(10, new minMaxRange(20, 75));
                BankRanges.Add(11, new minMaxRange(25, 100));
                BankRanges.Add(12, new minMaxRange(100, 1000));
                BankRanges.Add(13, new minMaxRange(250, 2000));
                BankRanges.Add(14, new minMaxRange(500, 3000));
                BankRanges.Add(15, new minMaxRange(750, 4000));
                BankRanges.Add(16, new minMaxRange(1000, 5000));
                BankRanges.Add(17, new minMaxRange(1500, 7500));
                BankRanges.Add(18, new minMaxRange(2000, 10000));
                BankRanges.Add(19, new minMaxRange(2500, 25000));
                BankRanges.Add(20, new minMaxRange(2500, 1000000));

                serviceButtonRect = new Rect(5, 23, 55, 7);
                serviceLabel.Position = new Vector2(0, 1);
                serviceLabel.ShadowPosition = Vector2.zero;
                serviceLabel.Font = DaggerfallUI.DefaultFont;
                serviceLabel.HorizontalAlignment = HorizontalAlignment.Left;
                serviceLabel.Text = GetServiceLabelText();
                serviceButton = DaggerfallUI.AddButton(serviceButtonRect, mainPanel);
                //serviceButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
                serviceButton.Components.Add(serviceLabel);
                serviceButton.OnMouseClick += ServiceButton_OnMouseClick;
                
                tradeLabel.Position = new Vector2(0, 1);
                tradeLabel.ShadowPosition = Vector2.zero;
                tradeLabel.Font = DaggerfallUI.DefaultFont;
                tradeLabel.HorizontalAlignment = HorizontalAlignment.Right;
                tradeLabel.Text = "Trade";
                tradeButton = DaggerfallUI.AddButton(tradeButtonRect, mainPanel);
                //serviceButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
                tradeButton.Components.Add(tradeLabel);
                tradeButton.OnMouseClick += TradeButton_OnMouseClick;
            }
            else
            {
                // Service button for banking
                serviceButtonRect = new Rect(5, 23, 120, 7);
                serviceLabel.Position = new Vector2(0, 1);
                serviceLabel.ShadowPosition = Vector2.zero;
                serviceLabel.Font = DaggerfallUI.DefaultFont;
                serviceLabel.HorizontalAlignment = HorizontalAlignment.Center;
                serviceLabel.Text = GetServiceLabelText();
                serviceButton = DaggerfallUI.AddButton(serviceButtonRect, mainPanel);
                //serviceButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
                serviceButton.Components.Add(serviceLabel);
                serviceButton.OnMouseClick += ServiceButton_OnMouseClick;
            }

            // Exit button
            exitButton = DaggerfallUI.AddButton(exitButtonRect, mainPanel);
            //exitButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;

            NativePanel.Components.Add(mainPanel);
        }

        int GetBankQuality()
        {
            int n = 0;
            if (buildingSummary.Quality > 17)
                n= LimitedGoldShopsMain.MinimumItemQualityAtBestBank;
            else if (buildingSummary.Quality > 14)
                n= LimitedGoldShopsMain.MinimumItemQualityAtGoodBank;
            else
                n= LimitedGoldShopsMain.MinimumItemQualityAtAverageBank;
            return n;
        }
        string GetItemQualityMinSpec()
        {

            int n = GetBankQuality();
            
            switch (n)
            {
                case 0:
                    return "in any condition";
                case 1:
                    return "no worse than slightly used";
                case 2:
                    return "in almost new condition";
                case 3:
                    return "in new condition only";
                default:
                    return "in perfect condition only";
            }
        }
        
        private void TradeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {

            var playerEntity = GameManager.Instance.PlayerEntity;
            DaggerfallUI.Instance.PopToHUD();
            DaggerfallListPickerWindow validItemPicker = new DaggerfallListPickerWindow(uiManager, uiManager.TopWindow);
            validItemPicker.OnItemPicked += TradeItem_OnItemPicked;
            validItemPicker.OnCancel += TradeItem_OnCancel;
            validTradeItems.Clear(); // Clears the valid item list before every poison apply tool use.

            var minValue = BankRanges[buildingSummary.Quality].Min * LimitedGoldShopsMain.BankRangeMultiplier;
            var maxValue = BankRanges[buildingSummary.Quality].Max * LimitedGoldShopsMain.BankRangeMultiplier;
            //var minValue = BankRanges[buildingSummary.Quality].Min;
            //var maxValue = BankRanges[buildingSummary.Quality].Max;
            
            DaggerfallUI.AddHUDText($"Happy to help you with this, we only accept items of values between {minValue:N0} and {maxValue:N0} that are {GetItemQualityMinSpec()} ");

            
            int itemCount = playerEntity.Items.Count;

            for (int i = 0; i < playerEntity.Items.Count; i++)
            {
                DaggerfallUnityItem item = playerEntity.Items.GetItem(i);
                if (!item.IsIdentified || item.IsQuestItem || item.ItemGroup == ItemGroups.Transportation ||
                    item.value < minValue || item.value > maxValue
                    || item.ConditionPercentage < qualitySpecs[GetBankQuality()])
                    continue;
                
                validTradeItems.Add(item);
            }

            if (validTradeItems.Count > 0)
            {
                orderedValidTradeItems.Clear();
                foreach (var item in validTradeItems.OrderByDescending(i => i.value))
                {
                    orderedValidTradeItems.Add(item);
                    string itemName = $"{item.LongName}   Value = {item.value.ToString("N0").PadLeft(60 - item.LongName.Length)}";
                    validItemPicker.ListBox.AddItem(itemName);
                }
                uiManager.PushWindow(validItemPicker);
            }
            else
            {
                DaggerfallUI.MessageBox("You have no valid items with which to trade.");
            }

        }

        private bool TradeItemQualityPasses(int itemQuality, int minQuality)
        {
            return itemQuality >= minQuality;
        }
        
        private void TradeItem_OnCancel(DaggerfallPopupWindow sender)
        {
            uiManager.PopWindow();
            DaggerfallUI.MessageBox("Thank you.");
        }

        private void TradeItem_OnItemPicked(int index, string itemstring)
        {
            uiManager.PopWindow();
            float TradeValueAdjustment = 1f;
            BuildingDirectory buildingDirectory = GameManager.Instance.StreamingWorld.GetCurrentBuildingDirectory();
            if (!buildingDirectory)
                TradeValueAdjustment = 1f ;

            BuildingSummary buildingSummary = default;
            if (buildingDirectory && !buildingDirectory.GetBuildingSummary(GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey, out buildingSummary))
                TradeValueAdjustment = 1f ;
            else
            {
                if (buildingSummary.Quality < 4)
                    TradeValueAdjustment *= 2f;
                else if (buildingSummary.Quality >= 4 && buildingSummary.Quality <= 7)
                    TradeValueAdjustment = TradeValueAdjustment * 1.25f;
                else if (buildingSummary.Quality >= 14 && buildingSummary.Quality <= 17)
                    TradeValueAdjustment = TradeValueAdjustment * 0.75f;
                else if (buildingSummary.Quality >= 18 )
                    TradeValueAdjustment = TradeValueAdjustment * 0.5f;
                else
                    TradeValueAdjustment = 1f;
            }

            selectedItem = orderedValidTradeItems[index];
            selectedItemAdjustedValue = (int)(selectedItem.value * LimitedGoldShopsMain.TradeInValuePercent / 100f * TradeValueAdjustment);
            DaggerfallMessageBox acceptTradeMessageBox = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
            acceptTradeMessageBox.SetText(
                $"You want to trade this {selectedItem.LongName}, my appraiser offers {selectedItemAdjustedValue:N0}.");
            acceptTradeMessageBox.OnButtonClick += AcceptTradeMessageBox_OnButtonClick;

            acceptTradeMessageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            acceptTradeMessageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No, true);
            acceptTradeMessageBox.Show();
        }

        private void AcceptTradeMessageBox_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageboxbutton)
        {
            uiManager.PopWindow();
            if (messageboxbutton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                GameManager.Instance.PlayerEntity.GoldPieces += selectedItemAdjustedValue;
                DaggerfallUI.MessageBox(
                    $"Here are your {selectedItemAdjustedValue:N0} gold pieces for the trade of this {selectedItem.LongName}, thank you for doing business.");
                GameManager.Instance.PlayerEntity.Items.RemoveOne(selectedItem);
                selectedItem = null;
            }
            else
                DaggerfallUI.MessageBox("Well, maybe next time.");
        }

        protected override void LoadTextures()
        {
            Texture2D tex;
			
            //TextureReplacement.TryImportTextureFromLooseFiles(Path.Combine(TexturesFolder, baseTextureName), false, false, true, out tex);
            TextureReplacement.TryImportTexture(baseTextureName, true, out tex);

            //Debug.Log("Texture is:" + tex.ToString());
            baseTexture = tex;
        }

        #region Event Handlers

        protected void InvestButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            if (currentService == Services.Sell)
            {
                int mercSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile);
                int playerIntell = player.Stats.LiveIntelligence;
                if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
                {
                    investedFlag = sd.InvestedIn;
                    shopAttitude = sd.ShopAttitude;
                }

                if (playerIntell >= 30 && mercSkill >= 40)
                {
                    DaggerfallInputMessageBox investMessageBox = new DaggerfallInputMessageBox(uiManager, this);
                    if (buildQual <= 3) // 01 - 03
                    {
                        if (shopAttitude == 0)
                            tokens = LGSTextTokenHolder.ShopTextTokensNice(10);
                        else
                            tokens = LGSTextTokenHolder.ShopTextTokensMean(13);
                    }
                    else if (buildQual <= 7) // 04 - 07
                    {
                        if (shopAttitude == 0)
                            tokens = LGSTextTokenHolder.ShopTextTokensNice(1);
                        else
                            tokens = LGSTextTokenHolder.ShopTextTokensMean(9);
                    }
                    else if (buildQual <= 17) // 08 - 17
                    {
                        if (shopAttitude == 0)
                            tokens = LGSTextTokenHolder.ShopTextTokensNice(14);
                        else
                            tokens = LGSTextTokenHolder.ShopTextTokensMean(17);
                    }
                    else                      // 18 - 20
                    {
                        if (shopAttitude == 0)
                            tokens = LGSTextTokenHolder.ShopTextTokensNice(18);
                        else
                            tokens = LGSTextTokenHolder.ShopTextTokensMean(21);
                    }

                    if (investedFlag)
                    {
                        if (buildQual <= 3) // 01 - 03
                        {
                            if (shopAttitude == 0)
                                tokens = LGSTextTokenHolder.ShopTextTokensNice(12);
                            else
                                tokens = LGSTextTokenHolder.ShopTextTokensMean(15);
                        }
                        else if (buildQual <= 7) // 04 - 07
                        {
                            if (shopAttitude == 0)
                                tokens = LGSTextTokenHolder.ShopTextTokensNice(2);
                            else
                                tokens = LGSTextTokenHolder.ShopTextTokensMean(11);
                        }
                        else if (buildQual <= 17) // 08 - 17
                        {
                            if (shopAttitude == 0)
                                tokens = LGSTextTokenHolder.ShopTextTokensNice(16);
                            else
                                tokens = LGSTextTokenHolder.ShopTextTokensMean(19);
                        }
                        else                      // 18 - 20
                        {
                            if (shopAttitude == 0)
                                tokens = LGSTextTokenHolder.ShopTextTokensNice(20);
                            else
                                tokens = LGSTextTokenHolder.ShopTextTokensMean(23);
                        }
                    }
                    investMessageBox.SetTextTokens(tokens);
                    investMessageBox.TextPanelDistanceY = 0;
                    investMessageBox.InputDistanceX = 24;
                    investMessageBox.InputDistanceY = 2;
                    investMessageBox.TextBox.Numeric = true;
                    investMessageBox.TextBox.MaxCharacters = 9;
                    investMessageBox.TextBox.Text = "1";
                    investMessageBox.OnGotUserInput += InvestMessageBox_OnGotUserInput;
                    investMessageBox.Show();
                }
                else if (shopAttitude == 1 && buildQual <= 7 && playerIntell < 30)
                {
                    DaggerfallInputMessageBox scamMessageBox = new DaggerfallInputMessageBox(uiManager, this);
                    tokens = LGSTextTokenHolder.ShopTextTokensMean(1);
                    scamMessageBox.InputDistanceY = 2;
                    if (investedFlag)
                    //if (test == 0)
                    {
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(2);
                        scamMessageBox.InputDistanceY = 9;
                    }
                    scamMessageBox.SetTextTokens(tokens);
                    scamMessageBox.TextPanelDistanceY = 0;
                    scamMessageBox.InputDistanceX = 24;
                    scamMessageBox.TextBox.Numeric = true;
                    scamMessageBox.TextBox.MaxCharacters = 9;
                    scamMessageBox.TextBox.Text = "1";
                    scamMessageBox.OnGotUserInput += ScamMessageBox_OnGotUserInput;
                    scamMessageBox.Show();
                }
                else if (playerIntell < 30)
                {
                    tokens = LGSTextTokenHolder.ShopTextTokensNeutral(1);
                    DaggerfallUI.MessageBox(tokens);
                }
                else
                {
                    tokens = LGSTextTokenHolder.ShopTextTokensNeutral(2);
                    DaggerfallUI.MessageBox(tokens);
                }
            }
            else
                DaggerfallUI.MessageBox("We don't accept investment offers here, sorry.");
        }

        protected void InvestMessageBox_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            int playerIntell = player.Stats.LiveIntelligence;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            bool result = int.TryParse(input, out investOffer);
            if (!result || investOffer < 500)
            {
                tokens = LGSTextTokenHolder.ShopTextTokensNeutral(5);
                DaggerfallUI.MessageBox(tokens);
				return;
            }

            DaggerfallMessageBox investConfimBox = new DaggerfallMessageBox(uiManager, this);
            if (buildQual <= 3) // 01 - 03
            {
                if (shopAttitude == 0)
                    tokens = LGSTextTokenHolder.ShopTextTokensNice(11, investOffer);
                else
                    tokens = LGSTextTokenHolder.ShopTextTokensMean(14, investOffer);
            }
            else if (buildQual <= 7) // 04 - 07
            {
                if (shopAttitude == 0)
                    tokens = LGSTextTokenHolder.ShopTextTokensNice(3, investOffer);
                else
                    tokens = LGSTextTokenHolder.ShopTextTokensMean(10, investOffer);
            }
            else if (buildQual <= 17) // 08 - 17
            {
                if (shopAttitude == 0)
                    tokens = LGSTextTokenHolder.ShopTextTokensNice(15, investOffer);
                else
                    tokens = LGSTextTokenHolder.ShopTextTokensMean(18, investOffer);
            }
            else                      // 18 - 20
            {
                if (shopAttitude == 0)
                    tokens = LGSTextTokenHolder.ShopTextTokensNice(19, investOffer);
                else
                    tokens = LGSTextTokenHolder.ShopTextTokensMean(22, investOffer);
            }
            if (investedFlag)
            {
                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(13, investOffer);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(16, investOffer);
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(9, investOffer);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(12, investOffer);
                }
                else if (buildQual <= 17) // 08 - 17
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(17, investOffer);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(20, investOffer);
                }
                else                      // 18 - 20
                {
                    if (shopAttitude == 0)
                        tokens = LGSTextTokenHolder.ShopTextTokensNice(21, investOffer);
                    else
                        tokens = LGSTextTokenHolder.ShopTextTokensMean(24, investOffer);
                }
            }
            investConfimBox.SetTextTokens(tokens);
            investConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            investConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            investConfimBox.OnButtonClick += ConfirmInvestment_OnButtonClick;
            uiManager.PushWindow(investConfimBox);
        }

        protected void ScamMessageBox_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            int playerIntell = player.Stats.LiveIntelligence;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            bool result = int.TryParse(input, out investOffer);
            if (!result || investOffer < 500)
            {
                tokens = LGSTextTokenHolder.ShopTextTokensMean(25);
                DaggerfallUI.MessageBox(tokens);
				return;
            }

            DaggerfallMessageBox scamConfimBox = new DaggerfallMessageBox(uiManager, this);
            tokens = LGSTextTokenHolder.ShopTextTokensMean(3);
            scamConfimBox.SetTextTokens(tokens);
            scamConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            scamConfimBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            scamConfimBox.OnButtonClick += ConfirmGettingScammed_OnButtonClick;
            uiManager.PushWindow(scamConfimBox);
        }

        protected void ConfirmInvestment_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            int playerIntell = player.Stats.LiveIntelligence;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            CloseWindow();
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                Entity.PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                if (playerEntity.GetGoldAmount() >= investOffer)
                {
                    playerEntity.DeductGoldAmount(investOffer);
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                    if (buildQual <= 3) // 01 - 03
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I'll do my best, partner.");
                        else
                            DaggerfallUI.MessageBox("Thanks for the gold, ya nutter.");
                    }
                    else if (buildQual <= 7) // 04 - 07
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Ya won't be disappointed, boss.");
                        else
                            DaggerfallUI.MessageBox("Really care for those fish, huh?");
                    }
                    else if (buildQual <= 17) // 08 - 17
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Not a bad signature, do you practice?");
                        else
                            DaggerfallUI.MessageBox("Your signature is atrocious, by the way.");
                    }
                    else if (buildQual >= 18) // 18 - 20
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I never disappoint when it comes to turning a profit.");
                        else
                            DaggerfallUI.MessageBox("Wipe your feet next time, you're tracking sludge in, lamprey.");
                    }
                    LimitedGoldShops.LimitedGoldShopsMain.UpdateInvestAmount(investOffer);
                }
                else
                {
                    if (buildQual <= 3) // 01 - 03
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I think you counted wrong, you don't have that much.");
                        else
                            DaggerfallUI.MessageBox("I know you don't have that much, stop lyin' ya idiot!");
                    }
                    else if (buildQual <= 7) // 04 - 07
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Might want to recount, boss, cause I don't see that amount.");
                        else
                            DaggerfallUI.MessageBox("Who ya tryna impress, huh? I know you don't have that much.");
                    }
                    else if (buildQual <= 17) // 08 - 17
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("Sorry, but it appears your funds are less than you suggest.");
                        else
                            DaggerfallUI.MessageBox("Quit wasting my damn time, you dolt, you don't have that much!");
                    }
                    else if (buildQual >= 18) // 18 - 20
                    {
                        if (shopAttitude == 0)
                            DaggerfallUI.MessageBox("I think you need to hire a new accountant, because you don't have that.");
                        else
                            DaggerfallUI.MessageBox("I need to charge more for my time, so leeches like you won't waste it so often!");
                    }
                }
            }
            else if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.No)
            {
                if (buildQual <= 3) // 01 - 03
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("I understand your reluctance, just look around, haha!");
                    else
                        DaggerfallUI.MessageBox("Why you gettin' my hopes up, just to dash them down?");
                }
                else if (buildQual <= 7) // 04 - 07
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("That's fine, I would not take such a gamble myself either.");
                    else
                        DaggerfallUI.MessageBox("Why you tryna bait me in just to pull away the line?");
                }
                else if (buildQual <= 17) // 08 - 17
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("Very well, thankfully parchment is cheap around here.");
                    else
                        DaggerfallUI.MessageBox("What's with the indecisive always wasting my time in this town?");
                }
                else if (buildQual >= 18) // 18 - 20
                {
                    if (shopAttitude == 0)
                        DaggerfallUI.MessageBox("So be it, ask again when you are ready to commit.");
                    else
                        DaggerfallUI.MessageBox("The parasite sucks my precious time away once again, a pity.");
                }
            }
        }

        protected void ConfirmGettingScammed_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            int playerIntell = player.Stats.LiveIntelligence;
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue($"{GameManager.Instance.PlayerGPS.CurrentMapID}-{currentBuildingID}", out sd))
            {
                investedFlag = sd.InvestedIn;
                shopAttitude = sd.ShopAttitude;
            }

            CloseWindow();
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                Entity.PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                if (playerEntity.GetGoldAmount() >= investOffer)
                {
                    playerEntity.DeductGoldAmount(investOffer);
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);
                    tokens = LGSTextTokenHolder.ShopTextTokensNeutral(3);
                    if (investedFlag)
                    {
                        tokens = LGSTextTokenHolder.ShopTextTokensNeutral(4);
                    }
                    DaggerfallUI.MessageBox(tokens);
                    investOffer = 0;
                    LimitedGoldShops.LimitedGoldShopsMain.UpdateInvestAmount(investOffer);
                }
                else
                    DaggerfallUI.MessageBox("Good joke there, you really got me there, ya jerk...");
            }
            else if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.No)
                DaggerfallUI.MessageBox("Yeah, I was just joking as well, haha...");
        }
        #endregion
    }
}