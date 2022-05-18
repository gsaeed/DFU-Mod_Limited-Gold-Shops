using UnityEngine;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
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


        #region UI Rects

        Rect talkButtonRect = new Rect(5, 5, 120, 7);
        Rect investButtonRect = new Rect(5, 14, 120, 7);
        Rect serviceButtonRect = new Rect(5, 23, 120, 7);
        Rect exitButtonRect = new Rect(44, 33, 43, 15);

        #endregion

        #region UI Controls

        Button investButton = new Button();

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

            // Create interface panel
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            mainPanel.BackgroundTexture = baseTexture;
            mainPanel.Position = new Vector2(0, 50);
            mainPanel.Size = new Vector2(130, 51);

            // Talk button
            talkButton = DaggerfallUI.AddButton(talkButtonRect, mainPanel);
            //talkButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            talkButton.OnMouseClick += TalkButton_OnMouseClick;

            // Invest button
            investButton = DaggerfallUI.AddButton(investButtonRect, mainPanel);
            //investButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            investButton.OnMouseClick += InvestButton_OnMouseClick;

            // Service button
            serviceLabel.Position = new Vector2(0, 1);
            serviceLabel.ShadowPosition = Vector2.zero;
            serviceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            serviceLabel.Text = GetServiceLabelText();
            serviceButton = DaggerfallUI.AddButton(serviceButtonRect, mainPanel);
            //serviceButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            serviceButton.Components.Add(serviceLabel);
            serviceButton.OnMouseClick += ServiceButton_OnMouseClick;

            // Exit button
            exitButton = DaggerfallUI.AddButton(exitButtonRect, mainPanel);
            //exitButton.BackgroundColor = new Color(0.9f, 0.1f, 0.5f, 0.75f);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;

            NativePanel.Components.Add(mainPanel);
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
                if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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
            if (LimitedGoldShops.LimitedGoldShopsMain.ShopBuildingData.TryGetValue(currentBuildingID, out sd))
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