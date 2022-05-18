using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;

namespace LimitedGoldShops
{
    public class LGSTextTokenHolder
    {
        public static string GetHonoric()
        {
            int buildQual = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.quality;

            if (GameManager.Instance.PlayerEntity.Gender == Genders.Male)
            {
                if (buildQual <= 7)       // 01 - 07
                    return "%ra";
                else if (buildQual <= 17) // 08 - 17
                    return "sir";
                else                      // 18 - 20
                    return "m'lord";
            }
            else
            {
                if (buildQual <= 7)       // 01 - 07
                    return "%ra";
                else if (buildQual <= 17) // 08 - 17
                    return "ma'am";
                else                      // 18 - 20
                    return "madam";
            }
        }

        public static TextFile.Token[] ShopTextTokensNice(int tokenID, int offerAmount = 0)
        {
            switch (tokenID)
            {
                case 1:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "It's very rare to have anyone suggesting",
                        "investing into my little shop. Tell me,",
                        "how much were you considering?");
                case 2:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Hello again my generous business",
                        "partner! Oh, you wish to have more",
                        "skin in the game? How much more?");
                case 3:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Really? Are you sure about",
                        "investing " + offerAmount + " gold in my",
                        "humble shop?");
                case 4:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Sorry " + GetHonoric() + ", I'm all out of gold.",
                        "You emptied my coffers clean.",
                        "You'll have to buy something,",
                        "or come back in a few days when",
                        "I've been able to visit the bank.");
                case 5:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "I may not be the best with",
                        "finances, " + GetHonoric() + ". But, I do know",
                        "when my purse is so light I can't",
                        "even buy a watered down ale.",
                        "Come back some other time.");
                case 6:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "I'll level with you, " + GetHonoric() + ".",
                        "I don't have a pot to piss in.",
                        "Return in a few days and maybe",
                        "my situation will be different.");
                case 7:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Terribly sorry " + GetHonoric() + ", it appears",
                        "that my funds have run dry.",
                        "Return in about a fortnight and",
                        "I should have a sack of newly",
                        "minted coin to barter with.");
                case 8:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "My most sincere apologies " + GetHonoric() + ",",
                        "I have foolishly not stocked",
                        "myself with ample enough coin",
                        "to satisfy your needs. Give",
                        "me fifteen days at most and",
                        "we can continue our transaction.");
                case 9:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Great! another " + offerAmount + " gold",
                        "onto the existing amount.",
                        "Did I catch that right, boss?");
                case 10:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Investment? You're gonna have to be more straight",
                        "with me " + GetHonoric() + ", cause ah' ain't smellin'",
                        "what you're cooking right now.");
                case 11:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "What's that? " + offerAmount + " gold? And that's it,",
                        "you jus' handin' that to me and that's all?",
                        "You sure about this " + GetHonoric() + "?");
                case 12:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Hey! Hey! My good friend is back! Hope ya don't",
                        "mind me spendin' some of that \"investment\" gold",
                        "on my favorite lager. What's that, you givin' more?");
                case 13:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        offerAmount + " gold! You are the most generous friend",
                        "I've ever had, you know that? If it weren't for you",
                        "I'd be drinking cheap wine tomorrow, insteah' top shelf!");
                case 14:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Yes, I would be willing to take your seed funds and grow",
                        "further profits for the both of us in the end. My other",
                        "clients have not been disappointed with the outcome.");
                case 15:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Very well " + GetHonoric() + ", I will add your name and",
                        "the amount of " + offerAmount + " gold into my ledger. Now",
                        "can you confirm that is amount is correct and sign here?");
                case 16:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Welcome back " + GetHonoric() + ", I would assume that",
                        "you are pleased so far with the direction your investment",
                        "has went thus far? Would you like to stake more in today?");
                case 17:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Of course, another " + offerAmount + " into the account, is that",
                        "correct? If so, just cross that number out and sign...",
                        "There, and there, will that be all for now?");
                case 18:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Absolutely " + GetHonoric() + ", my boutique is the most",
                        "profitable of its kind within this region of the Iliac Bay.",
                        "How much would you like to take part in this business?");
                case 19:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        offerAmount + " gold? A modest sum, but i'm certain I can make",
                        "a good profit with that amount, such that you could",
                        "be nothing but satisfied " + GetHonoric() + ", would you agree?");
                case 20:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Back so soon " + GetHonoric() + "? Well I can tell you that your",
                        "investment has been doing very well thus far. As I told you from",
                        "the start, i'm the best in this region, and that has not changed");
                case 21:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        offerAmount + " more you say? Very good " + GetHonoric() + ", i'll",
                        "be sure to take that into consideration when the next trade caravan",
                        "is scheduled to come by. Will that be all for today " + GetHonoric() + "?");
                default:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Text Token Not Found");
            }
        }

        public static TextFile.Token[] ShopTextTokensMean(int tokenID, int offerAmount = 0)
        {
            switch (tokenID)
            {
                case 1:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "There is a slight glint in the merchants eyes, and a subtle flash of a smirk.",
                        "",
                        "\"Ah my wise friend, of course someone as intelligent as yourself",
                        "would know the benefits of planting your seeds to bare fruit for the",
                        "future! Now then, wise nobel before me, how much would you ",
                        "like to invest in my humble shop? ehehehe.\"");
                case 2:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "There is a small indication of regret on the merchants expression as he",
                        "recognizes your face. When he hears you are suggesting another \"investment\",",
                        "his expression instantly changes to a big toothy grin.",
                        "",
                        "\"Ah my good business partner, good to see you again! Oh of course you would be",
                        "interested in giving, oh pardon me, I mean \"investing\" more gold into my",
                        "shop. Now, how much more am I, uh I mean, \"we\" going to be getting today?\"");
                case 3:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Rest assured, my wise friend.",
                        "Your funds will be in good hands",
                        "with me. hehe.");
                case 4:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Are ya blind or just dumb, " + GetHonoric() + "?",
                        "Can't ya see my purse is as empty",
                        "as that space between your ears?",
                        "Now unless you're gonna buy somethin',",
                        "stop wastin' my damn time!");
                case 5:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Look, " + GetHonoric() + ", I think it's",
                        "pretty clear that I don't have a rat's",
                        "turd left to offer you. So why do you",
                        "mock me with a pointless offer?");
                case 6:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Well, \"" + GetHonoric() + "\" do you plan on",
                        "giving this as a gift? Because if you",
                        "opened your damn eyes, I think it's",
                        "pretty obvious I can't afford your junk.");
                case 7:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "My father did not build this shop",
                        "to serve mouth-breathers that can't",
                        "even tell when a respectable merchant",
                        "is out of coin. NOW SOD OFF!");
                case 8:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "I did not get to where I am",
                        "by wasting my time with peasants",
                        "that don't know the difference between",
                        "an empty coin purse and a gelded stallion's",
                        "scrotum. Now, leave my sight, plebeian.");
                case 9:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "I'm not a big fan of jokes " + GetHonoric() + ". Only",
                        "a fool could suggest investment here without",
                        "cracking a smile, and I don't see you smiling.");
                case 10:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        offerAmount + " gold? So you are not pulling",
                        "my leg? Either that or you are just plain",
                        "out of your mind. You sure about this?");
                case 11:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Ah, my insane business partner. Looking",
                        "to throw more of your gold down the well?",
                        "Tell me, how much will go down this time?");
                case 12:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Another " + offerAmount + "? Very well, as you wish.",
                        "I suppose the Dreugh and Slaughterfish need",
                        "something shiny to stare at, do you agree?");
                case 13:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "\"Invest\" in my shop? HAHAHAHAHA, you must be mad,",
                        GetHonoric() + "! Even my ancestors ain't heard that word in",
                        "generations, what madness brought you to mention it today?");
                case 14:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        offerAmount + " gold! You must be off your rocker or badly lost",
                        "a bar fight recently, " + GetHonoric() + ". I ain't against",
                        "parting a lunatic of their coin though, so what do ya say?");
                case 15:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Hey it's my meal ticket again! How ya doin' ya mad nutter?",
                        "Oh! You wantin' to throw me some more of your coin? Well i'm",
                        "glad you mention it, cause ah' been needin' some new socks.");
                case 16:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Yes yes, another " + offerAmount + " gold you say? Well that's",
                        "good to hear, cause I hear the local waterin' hole callin'",
                        "me right now. Will that be all today my fine meal ticket?");
                case 17:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Yeah yeah, my other clients at least look like they have",
                        "taken a shower this month. Anyway, how much you going to",
                        "throw onto the pile? Spit it out, I ain't got all year!");
                case 18:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Alright fine, " + offerAmount + " gold it is then. Now",
                        "are you going to keep breathing all over my book,",
                        "or do you plan on actually signing the damn thing?");
                case 19:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Oh wonderful, you're back. Yes yes, it's all written",
                        "down, your investment is in good hands with me. With",
                        "that in mind, were you planning on putting more in?");
                case 20:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Another " + offerAmount + " gold huh? Alright,",
                        "i'll update my book and your gold is in",
                        "good company with me, just sign here.");
                case 21:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "HA! A vagrant wishes to leech off of my work ethic and",
                        "superior business aptitude in order to enrich themselves.",
                        "Not a surprise, leech, now how much am I to work with?");
                case 22:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Hmph, " + offerAmount + " gold? Such a paltry amount for a master",
                        "merchant such as myself. But no matter, i'll have this",
                        "insulting sum of gold doubled in no time, little lamprey.");
                case 23:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Perfect, the blood-sucker returns and wants to know how their",
                        "pittance of gold is doing. I'll let you know, it's doing just",
                        "fine with me, don't you worry that proboscis of yours one bit.");
                case 24:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "You wish to give me more to work with? Hmph, " + offerAmount + " this",
                        "time, even added to your previous amount, still insulting never",
                        "the less. Alright, my little parasite, is that all?");
                case 25:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "See, I knew you were just pulling my leg",
                        "there's no way someone could be that dense");
                default:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Text Token Not Found");
            }
        }

        public static TextFile.Token[] ShopTextTokensNeutral(int tokenID, int offerAmount = 0)
        {
            switch (tokenID)
            {
                case 1:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "I've worked with enough idiots in",
                        "my years to know not to take you up",
                        "on that offer, sorry friend.",
                        "",
                        "(Minimum: 30 Intelligence and 40 Mercantile)");
                case 2:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Sorry friend, I only work with those",
                        "that have some degree of business accumane,",
                        "and you don't meet that standard.",
                        "",
                        "(Minimum: 30 Intelligence and 40 Mercantile)");
                case 3:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "The merchant chuckles as he counts your \"investment\".",
                        "",
                        "You may not be the sharpest knife in the cabinet, but you have",
                        "a gut feeling that you won't be seeing that gold again");
                case 4:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "The merchant is clearly trying his hardest to suppress a",
                        "fit of laughter as you hand him your gold, yet again.",
                        "",
                        "\"It is said, \"Fool me once, shame on you. Fool me twice, shame",
                        "on me.\" It's pretty clear you were never taught this proverb",
                        "as a child. Possibly too many paint-chips for breakfast.\"");
                case 5:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Be serious here, how much are you actually wanting",
                        "to invest here? I don't accept such small",
                        "amounts, there is no profit potential there.");
                default:
                    return DaggerfallUnity.Instance.TextProvider.CreateTokens(
                        TextFile.Formatting.JustifyCenter,
                        "Text Token Not Found");
            }
        }
    }
}