/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;

namespace Treachery.Client;

public static class DefaultSkin
{
    public const string DEFAULT_ART_LOCATION = ".";
    
    public static Skin Default { get; } = new()
    {
        Description = "1979 Art",
        Version = Skin.LatestVersion,

        Map_URL = DEFAULT_ART_LOCATION + "/art/map.svg",
        Eye_URL = DEFAULT_ART_LOCATION + "/art/eye.svg",
        EyeSlash_URL = DEFAULT_ART_LOCATION + "/art/eyeslash.svg",
        HighThreshold_URL = DEFAULT_ART_LOCATION + "/art/arrow-up-circle-fill.svg",
        LowThreshold_URL = DEFAULT_ART_LOCATION + "/art/arrow-down-circle-fill.svg",
        CardBack_ResourceCard_URL = DEFAULT_ART_LOCATION + "/art/SpiceBack.gif",
        CardBack_TreacheryCard_URL = DEFAULT_ART_LOCATION + "/art/TreacheryBack.gif",
        BattleScreen_URL = DEFAULT_ART_LOCATION + "/art/wheel.png",
        Messiah_URL = DEFAULT_ART_LOCATION + "/art/messiah.svg",
        Monster_URL = DEFAULT_ART_LOCATION + "/art/monster.svg",
        DrawResourceIconsOnMap = true,
        ShowVerboseToolipsOnMap = true,
        ShowArrowsForRecentMoves = true,
        Resource_URL = DEFAULT_ART_LOCATION + "/art/PassiveSpice.svg",
        Harvester_URL = DEFAULT_ART_LOCATION + "/art/ActiveSpice.svg",
        HMS_URL = DEFAULT_ART_LOCATION + "/art/hms.svg",

        Concept_STR = new Dictionary<Concept, string>
        {
            [Concept.Messiah] = "Kwisatz Haderach",
            [Concept.Monster] = "Shai Hulud",
            [Concept.Resource] = "Spice",
            [Concept.Graveyard] = "Tleilaxu Tanks"

        }.Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Concept, string>>() : new Dictionary<Concept, string>
        {
            [Concept.BabyMonster] = "Sandtrout"

        }).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Concept, string>>() : new Dictionary<Concept, string>
        {
            [Concept.GreatMonster] = "Great Maker"

        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        MainPhase_STR = new Dictionary<MainPhase, string>
        {
            [MainPhase.None] = "None",
            [MainPhase.Started] = "Awaiting Players",
            [MainPhase.Setup] = "Setting Up",
            [MainPhase.Storm] = "Storm",
            [MainPhase.Charity] = "CHOAM Charity",
            [MainPhase.Blow] = "Spice Blow",
            [MainPhase.Bidding] = "Bidding",
            [MainPhase.Resurrection] = "Revival",
            [MainPhase.ShipmentAndMove] = "Ship & Move",
            [MainPhase.Battle] = "Battle",
            [MainPhase.Collection] = "Collection",
            [MainPhase.Contemplate] = "Mentat",
            [MainPhase.Ended] = "Game Ended"
        },

        TreacheryCardType_STR = new Dictionary<TreacheryCardType, string>
        {
            [TreacheryCardType.None] = "None",
            [TreacheryCardType.Laser] = "Lasegun",
            [TreacheryCardType.ProjectileDefense] = "Projectile Defense",
            [TreacheryCardType.Projectile] = "Projectile Weapon",
            [TreacheryCardType.WeirdingWay] = "Weirding Way",
            [TreacheryCardType.ProjectileAndPoison] = "Poison Blade",
            [TreacheryCardType.PoisonDefense] = "Poison Defense",
            [TreacheryCardType.Poison] = "Poison Weapon",
            [TreacheryCardType.Chemistry] = "Chemistry",
            [TreacheryCardType.PoisonTooth] = "Poison Tooth",
            [TreacheryCardType.Shield] = "Shield",
            [TreacheryCardType.Antidote] = "Snooper",
            [TreacheryCardType.ShieldAndAntidote] = "Shield Snooper",
            [TreacheryCardType.Mercenary] = "Cheap Hero",
            [TreacheryCardType.Karma] = "Karama",
            [TreacheryCardType.Useless] = "Worthless",
            [TreacheryCardType.StormSpell] = "Weather Control",
            [TreacheryCardType.RaiseDead] = "Tleilaxu Ghola",
            [TreacheryCardType.Metheor] = "Family Atomics",
            [TreacheryCardType.Caravan] = "Hajr",
            [TreacheryCardType.Clairvoyance] = "Truthtrance"

        }.Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<TreacheryCardType, string>>() : new Dictionary<TreacheryCardType, string>
        {
            [TreacheryCardType.ArtilleryStrike] = "Artillery Strike",
            [TreacheryCardType.Harvester] = "Harvester",
            [TreacheryCardType.Thumper] = "Thumper",
            [TreacheryCardType.Amal] = "Amal"

        }).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<TreacheryCardType, string>>() : new Dictionary<TreacheryCardType, string>
        {
            [TreacheryCardType.Distrans] = "Distrans",
            [TreacheryCardType.Juice] = "Juice Of Sapho",
            [TreacheryCardType.MirrorWeapon] = "Mirror Weapon",
            [TreacheryCardType.PortableAntidote] = "Portable Snooper",
            [TreacheryCardType.Flight] = "Ornithopter",
            [TreacheryCardType.SearchDiscarded] = "Nullentropy",
            [TreacheryCardType.TakeDiscarded] = "Semuta Drug",
            [TreacheryCardType.Residual] = "Residual Poison",
            [TreacheryCardType.Rockmelter] = "Stone Burner"

        }).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<TreacheryCardType, string>>() : new Dictionary<TreacheryCardType, string>
        {
            [TreacheryCardType.Recruits] = "Recruits",
            [TreacheryCardType.Reinforcements] = "Reinforcements",
            [TreacheryCardType.HarassAndWithdraw] = "Harass and Withdraw"

        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TreacheryCardName_STR = new Dictionary<int, string>
            {
                [0] = "Lasegun",
                [1] = "Crysknife",
                [2] = "Maula Pistol",
                [3] = "Slip Tip",
                [4] = "Stunner",
                [5] = "Chaumas",
                [6] = "Chaumurky",
                [7] = "Ellaca Drug",
                [8] = "Gom Jabbar",
                [9] = "Shield",
                [13] = "Snooper",
                [17] = "Cheap Hero",
                [18] = "Cheap Hero",
                [19] = "Cheap Hero",
                [20] = "Tleilaxu Ghola",
                [21] = "Family Atomics",
                [22] = "Hajr",
                [23] = "Karama",
                [25] = "Truthtrance",
                [27] = "Weather Control",
                [28] = "Baliset",
                [29] = "Jubba Cloak",
                [30] = "Kulon",
                [31] = "La La La",
                [32] = "Trip to Gamont"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [33] = "Poison Blade",
                    [34] = "Hunter-Seeker",
                    [35] = "Basilia Weapon",
                    [36] = "Weirding Way",
                    [37] = "Poison Tooth",
                    [38] = "Shield Snooper",
                    [39] = "Chemistry",
                    [40] = "Artillery Strike",
                    [41] = "Harvester",
                    [42] = "Thumper",
                    [43] = "Amal",
                    [44] = "Kull Wahad"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [45] = "Distrans",
                    [46] = "Juice Of Sapho",
                    [47] = "Mirror Weapon",
                    [48] = "Portable Snooper",
                    [49] = "Ornithopter",
                    [50] = "Nullentropy Box",
                    [51] = "Semuta Drug",
                    [52] = "Residual Poison",
                    [53] = "Stone Burner",
                    [54] = "Karama"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [55] = "Recruits",
                    [56] = "Reinforcements",
                    [57] = "Harass and Withdraw"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TreacheryCardDescription_STR = new Dictionary<int, string>
            {
                [0] = "Weapon - Special - Play as part of your Battle Plan. Automatically kills your opponent's leader. Causes an explosion when a Shield is used in the same battle, killing both leaders and all forces in the territory, cause both factions to loose the battle.",
                [1] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [2] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [3] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [4] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                [5] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [6] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [7] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [8] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                [9] = "Defense - Projectile - Play as part of your Battle Plan. Protects your leader from a projectile weapon in this battle. You may keep this card if you win this battle.",
                [13] = "Defense - Poison - Play as part of your Battle Plan. Protects your leader from a poison weapon in this battle. You may keep this card if you win this battle.",
                [17] = "Play as a leader with zero strength on your Battle Plan and discard after the battle.",
                [18] = "Play as a leader with zero strength on your Battle Plan and discard after the battle.",
                [19] = "Play as a leader with zero strength on your Battle Plan and discard after the battle.",
                [20] = "Play at any time to revive up to 5 forces or 1 leader.",
                [21] = "Can be played after turn 1 just before the storm moves if you have forces on the Shield Wall or an adjacent territory. Destroys the Shield Wall and all forces on it, causing Arrakeen, Carthag and the Imperial Basin to be vulnerable to storms for the rest of the game. This card is then removed from play.",
                [22] = "Play during your turn at the Movement phase to perform an additional move.",
                [23] = "Allows you to prevent use of a Faction Advantage. Allows you to bid any amount of spice on a card or immediately win a card on bid. Allows you to ship at half price. In the advanced game, allows use of your Special Karama Power once during the game. Discard after use.",
                [25] = "Publicly ask one player a yes or no question about the game. That question must be answered truthfully.",
                [27] = "Can be played after turn 1 just before the storm moves. Instead of normal storm moved, you can move the storm 0 to 10 sectors.",
                [28] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [29] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [30] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [31] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan.",
                [32] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan."
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [33] = "Weapon - Special - Play as part of your Battle Plan. This weapon counts as both projectile and poison. You may keep this card if you win this battle.",
                    [34] = "Weapon - Projectile - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Shield. You may keep this card if you win the battle.",
                    [35] = "Weapon - Poison - Play as part of your Battle Plan. Kills opponent's leader before battle is resolved. Opponent may protect leader with a Snooper. You may keep this card if you win the battle.",
                    [36] = "Weapon - Special - Play as part of your Battle Plan. Counts as a projectile weapon but has the same effect as a projectile defense when played as a defense with another weapon. You may keep this card if you win this battle.",
                    [37] = "Weapon - Special - Play as part of your Battle Plan. Kills both leaders, and is not stopped by a Snooper. After seeing the battle results, you may choose not to use this weapon in which case you don't need to discard it if you win the battle.",
                    [38] = "Defense - Special - Play as part of your Battle Plan. Counts as both a Shield (projectile defense) and Snooper (poison defense). You may keep this card if you win this battle.",
                    [39] = "Defense - Special - Play as part of your Battle Plan. Counts as a poison defense but has the same effect as a poison weapon when played as a weapon with another defense. You may keep this card if you win this battle.",
                    [40] = "Weapon - Special - Play as part of your Battle Plan. Kills both leaders (no spice is paid for them). Both players may use Shields to protect their leader against the Artillery Strike. Surviving (shielded) leaders do not count towards the battle total, the side that dialed higher wins the battle. Discard after use.",
                    [41] = "Play just after a spice blow comes up. Doubles the Spice blow. Place double the amount of spice in the territory.",
                    [42] = "Play at beginning of Spice Blow Phase instead of revealing the Spice Blow card. Causes a Shai-Hulud to appear. Play proceeds as though Shai-Hulud has been revealed.",
                    [43] = "At the beginning of any phase, cause all players to discard half of the spice behind their shields, rounded up, to the Spice Bank.",
                    [44] = "Worthless Card - Play as part of your Battle Plan, in place of a weapon, defense, or both. This card has no value in play, and you can discard it only by playing it in your Battle plan."
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [45] = "Distrans - Give another player a treachery card from your hand at any time except during a bid and if their hand size permits. Discard after use.",
                    [46] = "Choose one: (1) be considered aggressor in a battle or (2) play at the beginning of a phase or action that requires turn order to go first or (3) go last in a phase or action that requires turn order. Discard after use.",
                    [47] = "Weapon - Special - Play as part of your Battle Plan. Becomes a copy of your opponent's weapon. Discard after use.",
                    [48] = "Defense - Poison - You may play this after revealing your battle plan if you did not play a defense and if Voice permits. Discard after use.",
                    [49] = "Ornithopter - As part of your movement you may move one group of forces up to 3 territories or two groups of forces up to your normal move distance. Discard after use.",
                    [50] = "Nullentropy Box - At any time, pay 2 spice to secretly search and take one card from the treachery discard pile. Then shuffle the discard pile, discarding this card on top.",
                    [51] = "Semuta Drug - Add a treachery card to your hand immediately after another player discards it. You choose if multiple cards are discarded at the same time. Discard after use.",
                    [52] = "Residual Poison - Play against your opponent in battle before making battle plans. Kills one of their available leaders at random. No spice is collected for it. Discard after use.",
                    [53] = "Weapon - Special - Play as part of your Battle Plan. You choose after pland are revealed to either kill both leaders or reduce the strength of both leaders to 0. The player with the highest number of undialed forces wins the battle. Dialed forces are lost normally. Discard after use.",
                    [54] = "Allows you to prevent use of a Faction Advantage. Allows you to bid any amount of spice on a card or immediately win a card on bid. Allows you to ship at half price. In the advanced game, allows use of your Special Karama Power once during the game. Discard after use."
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [55] = "Play during Revival. All factions double their Free Revival rates. The revival limits is increased to 7. Discard after use.",
                    [56] = "Play as part of your battle plan in place of a weapon or defense if you have at least 3 forces in reserves. Add 2 to your dialed number, then send 3 forces from reserves to the Tanks. Discard after use.",
                    [57] = "Play as part of your battle plan in place of a weapon or defense when not on your own Home World. Your undialed forces return to your reserves. Your leader may be killed as normal. If your opponent calls traitor, this effect is cancelled. Discard after use."
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TechTokenDescription_STR = new Dictionary<TechToken, string>
        {
            [TechToken.Graveyard] = "At the end of the Revival phase, yields 1 spice per tech token you own if any player except Tleilaxu used free revival.",
            [TechToken.Resources] = "At the end of the Charity phase, yields 1 spice per tech token you own if any player except Bene Gesserit claimed charity.",
            [TechToken.Ships] = "At the end of the Shipment & Move phase, yields 1 spice per tech token you own if any player except Guild shipped forces from off-planet."
        },

        TreacheryCardImage_URL = new Dictionary<int, string>
            {
                [0] = DEFAULT_ART_LOCATION + "/art/Lasegun.gif",
                [1] = DEFAULT_ART_LOCATION + "/art/Chrysknife.gif",
                [2] = DEFAULT_ART_LOCATION + "/art/MaulaPistol.gif",
                [3] = DEFAULT_ART_LOCATION + "/art/Slip-Tip.gif",
                [4] = DEFAULT_ART_LOCATION + "/art/Stunner.gif",
                [5] = DEFAULT_ART_LOCATION + "/art/Chaumas.gif",
                [6] = DEFAULT_ART_LOCATION + "/art/Chaumurky.gif",
                [7] = DEFAULT_ART_LOCATION + "/art/EllacaDrug.gif",
                [8] = DEFAULT_ART_LOCATION + "/art/GomJabbar.gif",
                [9] = DEFAULT_ART_LOCATION + "/art/Shield.gif",
                [13] = DEFAULT_ART_LOCATION + "/art/Snooper.gif",
                [17] = DEFAULT_ART_LOCATION + "/art/CheapHero.gif",
                [19] = DEFAULT_ART_LOCATION + "/art/CheapHeroine.gif",
                [20] = DEFAULT_ART_LOCATION + "/art/TleilaxuGhola.gif",
                [21] = DEFAULT_ART_LOCATION + "/art/FamilyAtomics.gif",
                [22] = DEFAULT_ART_LOCATION + "/art/Hajr.gif",
                [23] = DEFAULT_ART_LOCATION + "/art/Karama.gif",
                [25] = DEFAULT_ART_LOCATION + "/art/Truthtrance.gif",
                [27] = DEFAULT_ART_LOCATION + "/art/WeatherControl.gif",
                [28] = DEFAULT_ART_LOCATION + "/art/Baliset.gif",
                [29] = DEFAULT_ART_LOCATION + "/art/JubbaCloak.gif",
                [30] = DEFAULT_ART_LOCATION + "/art/Kulon.gif",
                [31] = DEFAULT_ART_LOCATION + "/art/LaLaLa.gif",
                [32] = DEFAULT_ART_LOCATION + "/art/TripToGamont.gif"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [33] = DEFAULT_ART_LOCATION + "/art/PoisonBlade.gif",
                    [34] = DEFAULT_ART_LOCATION + "/art/Hunter-Seeker.gif",
                    [35] = DEFAULT_ART_LOCATION + "/art/BasiliaWeapon.gif",
                    [36] = DEFAULT_ART_LOCATION + "/art/WeirdingWay.gif",
                    [37] = DEFAULT_ART_LOCATION + "/art/PoisonTooth.gif",
                    [38] = DEFAULT_ART_LOCATION + "/art/ShieldSnooper.gif",
                    [39] = DEFAULT_ART_LOCATION + "/art/Chemistry.gif",
                    [40] = DEFAULT_ART_LOCATION + "/art/ArtilleryStrike.gif",
                    [41] = DEFAULT_ART_LOCATION + "/art/Harvester.gif",
                    [42] = DEFAULT_ART_LOCATION + "/art/Thumper.gif",
                    [43] = DEFAULT_ART_LOCATION + "/art/Amal.gif",
                    [44] = DEFAULT_ART_LOCATION + "/art/KullWahad.gif"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [45] = DEFAULT_ART_LOCATION + "/art/Distrans.gif",
                    [46] = DEFAULT_ART_LOCATION + "/art/JuiceOfSapho.gif",
                    [47] = DEFAULT_ART_LOCATION + "/art/MirrorWeapon.gif",
                    [48] = DEFAULT_ART_LOCATION + "/art/PortableSnooper.gif",
                    [49] = DEFAULT_ART_LOCATION + "/art/Ornithopter.gif",
                    [50] = DEFAULT_ART_LOCATION + "/art/Nullentropy.gif",
                    [51] = DEFAULT_ART_LOCATION + "/art/SemutaDrug.gif",
                    [52] = DEFAULT_ART_LOCATION + "/art/ResidualPoison.gif",
                    [53] = DEFAULT_ART_LOCATION + "/art/StoneBurner.gif",
                    [54] = DEFAULT_ART_LOCATION + "/art/WhiteKarama.gif"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [55] = DEFAULT_ART_LOCATION + "/art/Recruits.gif",
                    [56] = DEFAULT_ART_LOCATION + "/art/Reinforcements.gif",
                    [57] = DEFAULT_ART_LOCATION + "/art/HarassAndWithdraw.gif"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        ResourceCardImage_URL = new Dictionary<int, string>
            {
                [7] = DEFAULT_ART_LOCATION + "/art/CielagoNorth.gif",
                [10] = DEFAULT_ART_LOCATION + "/art/CielagoSouth.gif",
                [15] = DEFAULT_ART_LOCATION + "/art/TheMinorErg.gif",
                [17] = DEFAULT_ART_LOCATION + "/art/RedChasm.gif",
                [18] = DEFAULT_ART_LOCATION + "/art/SouthMesa.gif",
                [22] = DEFAULT_ART_LOCATION + "/art/SihayaRidge.gif",
                [25] = DEFAULT_ART_LOCATION + "/art/OHGap.gif",
                [26] = DEFAULT_ART_LOCATION + "/art/BrokenLand.gif",
                [29] = DEFAULT_ART_LOCATION + "/art/RockOutcroppings.gif",
                [31] = DEFAULT_ART_LOCATION + "/art/HaggaBasin.gif",
                [33] = DEFAULT_ART_LOCATION + "/art/FuneralPlain.gif",
                [34] = DEFAULT_ART_LOCATION + "/art/TheGreatFlat.gif",
                [37] = DEFAULT_ART_LOCATION + "/art/HabbanyaErg.gif",
                [39] = DEFAULT_ART_LOCATION + "/art/WindPassNorth.gif",
                [40] = DEFAULT_ART_LOCATION + "/art/HabbanyaRidgeFlat.gif",
                [98] = DEFAULT_ART_LOCATION + "/art/Shai-Hulud.gif"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [99] = DEFAULT_ART_LOCATION + "/art/Sandtrout.gif"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [41] = DEFAULT_ART_LOCATION + "/art/SihayaRidgeDiscovery.gif",
                    [42] = DEFAULT_ART_LOCATION + "/art/RockOutcroppingsDiscovery.gif",
                    [43] = DEFAULT_ART_LOCATION + "/art/HaggaBasinDiscovery.gif",
                    [44] = DEFAULT_ART_LOCATION + "/art/FuneralPlainDiscovery.gif",
                    [45] = DEFAULT_ART_LOCATION + "/art/WindPassNorthDiscovery.gif",
                    [46] = DEFAULT_ART_LOCATION + "/art/OHGapDiscovery.gif",
                    [100] = DEFAULT_ART_LOCATION + "/art/GreatMaker.gif"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        PersonName_STR = new Dictionary<int, string>
            {
                [1001] = "Thufir Hawat",
                [1002] = "Lady Jessica",
                [1003] = "Gurney Halleck",
                [1004] = "Duncan Idaho",
                [1005] = "Dr. Wellington Yueh",
                [1006] = "Alia",
                [1007] = "Margot Lady Fenring",
                [1008] = "Mother Ramallo",
                [1009] = "Princess Irulan",
                [1010] = "Wanna Yueh",
                [1011] = "Hasimir Fenring",
                [1012] = "Captain Aramsham",
                [1013] = "Caid",
                [1014] = "Burseg",
                [1015] = "Bashar",
                [1016] = "Stilgar",
                [1017] = "Chani",
                [1018] = "Otheym",
                [1019] = "Shadout Mapes",
                [1020] = "Jamis",
                [1021] = "Staban Tuek",
                [1022] = "Master Bewt",
                [1023] = "Esmar Tuek",
                [1024] = "Soo-Soo Sook",
                [1025] = "Guild Rep.",
                [1026] = "Feyd Rautha",
                [1027] = "Beast Rabban",
                [1028] = "Piter de Vries",
                [1029] = "Captain Iakin Nefud",
                [1030] = "Umman Kudu"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1031] = "Dominic Vernius",
                    [1032] = "C'Tair Pilru",
                    [1033] = "Tessia Vernius",
                    [1034] = "Kailea Vernius",
                    [1035] = "Cammar Pilru",
                    [1036] = "Zoal",
                    [1037] = "Hidar Fen Ajidica",
                    [1038] = "Master Zaaf",
                    [1039] = "Wykk",
                    [1040] = "Blin"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1041] = "Viscount Tull",
                    [1042] = "Duke Verdun",
                    [1043] = "Rajiv Londine",
                    [1044] = "Lady Jalma",
                    [1045] = "Frankos Aru",
                    [1046] = "Auditor",
                    [1047] = "Talis Balt",
                    [1048] = "Haloa Rund",
                    [1049] = "Flinto Kinnis",
                    [1050] = "Lady Helena",
                    [1051] = "Premier Ein Calimar"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1052] = "Sanya Ecaz",
                    [1053] = "Rivvy Dinari",
                    [1054] = "Ilesa Ecaz",
                    [1055] = "Bindikk Narvi",
                    [1056] = "Whitmore Bludd",
                    [1057] = "Duke Prad Vidal",
                    [1058] = "Lupino Ord",
                    [1059] = "Hiih Resser",
                    [1060] = "Trin Kronos",
                    [1061] = "Grieu Kronos",
                    [1062] = "Vando Terboli"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        PersonImage_URL = new Dictionary<int, string>
            {
                [1001] = DEFAULT_ART_LOCATION + "/art/person0.png",
                [1002] = DEFAULT_ART_LOCATION + "/art/person1.png",
                [1003] = DEFAULT_ART_LOCATION + "/art/person2.png",
                [1004] = DEFAULT_ART_LOCATION + "/art/person3.png",
                [1005] = DEFAULT_ART_LOCATION + "/art/person4.png",
                [1006] = DEFAULT_ART_LOCATION + "/art/person20.png",
                [1007] = DEFAULT_ART_LOCATION + "/art/person21.png",
                [1008] = DEFAULT_ART_LOCATION + "/art/person22.png",
                [1009] = DEFAULT_ART_LOCATION + "/art/person23.png",
                [1010] = DEFAULT_ART_LOCATION + "/art/person24.png",
                [1011] = DEFAULT_ART_LOCATION + "/art/person15.png",
                [1012] = DEFAULT_ART_LOCATION + "/art/person16.png",
                [1013] = DEFAULT_ART_LOCATION + "/art/person17.png",
                [1014] = DEFAULT_ART_LOCATION + "/art/person18.png",
                [1015] = DEFAULT_ART_LOCATION + "/art/person19.png",
                [1016] = DEFAULT_ART_LOCATION + "/art/person10.png",
                [1017] = DEFAULT_ART_LOCATION + "/art/person11.png",
                [1018] = DEFAULT_ART_LOCATION + "/art/person12.png",
                [1019] = DEFAULT_ART_LOCATION + "/art/person13.png",
                [1020] = DEFAULT_ART_LOCATION + "/art/person14.png",
                [1021] = DEFAULT_ART_LOCATION + "/art/person25.png",
                [1022] = DEFAULT_ART_LOCATION + "/art/person26.png",
                [1023] = DEFAULT_ART_LOCATION + "/art/person27.png",
                [1024] = DEFAULT_ART_LOCATION + "/art/person28.png",
                [1025] = DEFAULT_ART_LOCATION + "/art/person29.png",
                [1026] = DEFAULT_ART_LOCATION + "/art/person5.png",
                [1027] = DEFAULT_ART_LOCATION + "/art/person6.png",
                [1028] = DEFAULT_ART_LOCATION + "/art/person7.png",
                [1029] = DEFAULT_ART_LOCATION + "/art/person8.png",
                [1030] = DEFAULT_ART_LOCATION + "/art/person9.png"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1031] = DEFAULT_ART_LOCATION + "/art/person30.png",
                    [1032] = DEFAULT_ART_LOCATION + "/art/person31.png",
                    [1033] = DEFAULT_ART_LOCATION + "/art/person32.png",
                    [1034] = DEFAULT_ART_LOCATION + "/art/person33.png",
                    [1035] = DEFAULT_ART_LOCATION + "/art/person34.png",
                    [1036] = DEFAULT_ART_LOCATION + "/art/person35.png",
                    [1037] = DEFAULT_ART_LOCATION + "/art/person36.png",
                    [1038] = DEFAULT_ART_LOCATION + "/art/person37.png",
                    [1039] = DEFAULT_ART_LOCATION + "/art/person38.png",
                    [1040] = DEFAULT_ART_LOCATION + "/art/person39.png"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1041] = DEFAULT_ART_LOCATION + "/art/person1041.gif",
                    [1042] = DEFAULT_ART_LOCATION + "/art/person1042.gif",
                    [1043] = DEFAULT_ART_LOCATION + "/art/person1043.gif",
                    [1044] = DEFAULT_ART_LOCATION + "/art/person1044.gif",
                    [1045] = DEFAULT_ART_LOCATION + "/art/person1045.gif",
                    [1046] = DEFAULT_ART_LOCATION + "/art/person1046.gif",
                    [1047] = DEFAULT_ART_LOCATION + "/art/person1047.gif",
                    [1048] = DEFAULT_ART_LOCATION + "/art/person1048.gif",
                    [1049] = DEFAULT_ART_LOCATION + "/art/person1049.gif",
                    [1050] = DEFAULT_ART_LOCATION + "/art/person1050.gif",
                    [1051] = DEFAULT_ART_LOCATION + "/art/person1051.gif"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [1052] = DEFAULT_ART_LOCATION + "/art/person1052.png",
                    [1053] = DEFAULT_ART_LOCATION + "/art/person1053.png",
                    [1054] = DEFAULT_ART_LOCATION + "/art/person1054.png",
                    [1055] = DEFAULT_ART_LOCATION + "/art/person1055.png",
                    [1056] = DEFAULT_ART_LOCATION + "/art/person1056.png",
                    [1057] = DEFAULT_ART_LOCATION + "/art/person1057.png",
                    [1058] = DEFAULT_ART_LOCATION + "/art/person1058.png",
                    [1059] = DEFAULT_ART_LOCATION + "/art/person1059.png",
                    [1060] = DEFAULT_ART_LOCATION + "/art/person1060.png",
                    [1061] = DEFAULT_ART_LOCATION + "/art/person1061.png",
                    [1062] = DEFAULT_ART_LOCATION + "/art/person1062.png"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TerritoryName_STR = new Dictionary<int, string>
            {
                [0] = "Polar Sink",
                [1] = "Imperial Basin",
                [2] = "Carthag",
                [3] = "Arrakeen",
                [4] = "Tuek's Sietch",
                [5] = "Sietch Tabr",
                [6] = "Habbanya Sietch",
                [7] = "Cielago North",
                [8] = "Cielago Depression",
                [9] = "Meridian",
                [10] = "Cielago South",
                [11] = "Cielago East",
                [12] = "Harg Pass",
                [13] = "False Wall South",
                [14] = "False Wall East",
                [15] = "The Minor Erg",
                [16] = "Pasty Mesa",
                [17] = "Red Chasm",
                [18] = "South Mesa",
                [19] = "Basin",
                [20] = "Rim Wall West",
                [21] = "Hole In The Rock",
                [22] = "Sihaya Ridge",
                [23] = "Shield Wall",
                [24] = "Gara Kulon",
                [25] = "Old Gap",
                [26] = "Broken Land",
                [27] = "Tsimpo",
                [28] = "Arsunt",
                [29] = "Rock Outcroppings",
                [30] = "Plastic Basin",
                [31] = "Hagga Basin",
                [32] = "Bight Of The Cliff",
                [33] = "Funeral Plain",
                [34] = "The Great Flat",
                [35] = "Wind Pass",
                [36] = "The Greater Flat",
                [37] = "Habbanya Erg",
                [38] = "False Wall West",
                [39] = "Wind Pass North",
                [40] = "Habbanya Ridge Flat",
                [41] = "Cielago West"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
                {
                    [42] = "Hidden Mobile Stronghold"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<int, string>>() : new Dictionary<int, string>
            {
                [43] = "Caladan",
                [44] = "Giedi Prime",
                [45] = "Southern Hemisphere",
                [46] = "Kaitain",
                [47] = "Salusa Secundus",
                [48] = "Junction",
                [49] = "Wallach IX",
                [50] = "Ix",
                [51] = "Tleilax",
                [52] = "Tupile",
                [53] = "Richese",
                [54] = "Ecaz",
                [55] = "Grumman",
                [56] = "Jacurutu Sietch",
                [57] = "Cistern",
                [58] = "Ecological Testing Station",
                [59] = "Shrine",
                [60] = "Orgiz Processing Station"

            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        TerritoryBorder_SVG = new Dictionary<int, string>
        {
            [0] = "M243.4 297.6 L236.2 311.3 L233.7 326.2 L238.9 332 L242.4 332.3 L248.8 338.5 L256.8 341.8 L264.1 340.8 L268.6 348.9 L273.6 353.9 L288.7 354.5 L301 345.8 L301.4 337.3 L305.7 332 L310.1 321.9 L317.7 311.3 L320.2 297.1 L312 285.4 L302.7 285.5 L298.3 287.3 L291.4 287.1 L286.2 281.9 L281 281.9 L276.5 285.3 L267.7 288.3 L254.9 289.4 L243.4 297.6 z",
            [1] = "M345.8 221.8 L355.3 195.8 L353.8 193.3 L355.8 190.1 L354.6 183.9 L361.1 172.7 L354.7 170.7 L347.7 171.9 L343.9 162.8 L330.4 152.6 L329.5 145 L336.4 137.1 L342.3 126.1 L343.4 121 L340.8 110.7 L341.9 106.9 L336.6 99.3 L330.8 98.6 L320 90.4 L306.4 107.4 L297.6 127.7 L292.9 132.2 L297.6 140.5 L297.5 159 L287.6 182.5 L283.8 207.7 L286.4 217 L286.2 235.3 L288.3 237.8 L287.8 241.9 L287.9 260.2 L289.3 264.5 L286.2 281.9 L291.4 287.1 L298.3 287.3 L302.7 285.5 L302.7 280.5 L312.5 271.2 L317.2 262 L333.1 243.9 L331.6 240.5 L342.5 222.4 L345.8 221.8 z",
            [2] = "M287.6 182.5 L297.5 159 L297.6 140.5 L292.9 132.2 L277.5 129.2 L269.4 133.8 L250.7 139.1 L257.5 178 L261.6 178.3 L267.9 185.3 L275.2 182.6 L287.6 182.5 z",
            [3] = "M341.9 106.9 L340.8 110.7 L343.4 121 L342.3 126.1 L336.4 137.1 L329.5 145 L330.4 152.6 L343.9 162.8 L347.7 171.9 L354.7 170.7 L361.1 172.7 L383.3 134.2 L383.1 124.6 L370.6 110.3 L358 106.3 L341.9 106.9",
            [4] = "M451.4 447.4 L470.7 430.2 L479.3 414.8 L479.1 402.4 L482.6 390.3 L469.5 386.4 L464.4 381.1 L453.8 379.9 L443.5 377.4 L434.9 392.5 L433.7 404 L437.2 420.2 L435.7 433.8 L437.4 441 L451.4 447.4",
            [5] = "M128.8 234 L133.9 216.7 L131 213.6 L130.5 208.7 L127.9 206.7 L127.7 200.5 L122 191 L120 181.5 L111.1 177.2 L91.1 195.1 L90.4 200 L86.2 207.7 L81 209.4 L79.5 218 L89.4 228.9 L94.9 229.7 L100.2 226.6 L112.8 232 L120.6 231.9 L128.8 234 z",
            [6] = "M139.6 430 L129.1 409 L128.6 401.4 L125.9 395 L112.3 391.7 L97.8 396.8 L91.2 407.5 L96.8 427.1 L117 449 L139.6 430",
            [7] = "M224.1 410 L219.8 417.5 L221.1 424.6 L233.7 436.2 L263.8 439.1 L268.6 435.7 L280 435.6 L305.8 451.9 L314 446.6 L323.8 431.9 L330.4 424.6 L336 406.5 L301 345.8 L288.7 354.5 L273.6 353.9 L268.6 348.9 L264.1 340.8 L224.1 410 z",
            [8] = "M336.2 473.2 L324.9 474.3 L311 480.9 L301.1 481.1 L277.8 486.5 L245.5 486.1 L228.7 478.4 L218 479.5 L208.8 474 L210.1 462.8 L217.3 453.4 L219.7 438.3 L218 432.7 L224.5 427.7 L233.7 436.2 L263.8 439.1 L268.6 435.7 L280 435.6 L305.8 451.9 L314 446.6 L323.8 431.9 L327.9 446.4 L340.9 463.4 L336.2 473.2 z",
            [9] = "M259.7 552.8 L259.8 536.1 L264.3 529.4 L259.8 515.6 L260.2 494.4 L267 486.4 L245.5 486.1 L228.7 478.4 L218 479.5 L208.8 474 L195.1 486.8 L179.5 487.3 L159.8 521.3 C191.7 539.8 226.7 550 259.7 552.8 z",
            [10] = "M336.2 473.2 L324.9 474.3 L311 480.9 L301.1 481.1 L277.8 486.5 L267 486.4 L260.2 494.4 L259.8 515.6 L264.3 529.4 L259.8 536.1 L259.7 552.8 C268.3 553.6 276.8 553.9 285.2 553.7 C304.8 553.4 325.5 550.6 346.3 544.8 L342.6 539.1 L342.5 520.6 L346.1 510.3 L344.6 494.6 L337 481.3 L336.2 473.2 z",
            [11] = "M346.3 544.8 C372.2 537.6 398.2 525.6 421.6 508.9 L422 487.5 L420.8 485.6 L414 486 L403.3 483.9 L398.7 477 L396.1 465.4 L387.2 455.7 L373.2 449 L357.5 434 L336 406.5 L330.4 424.6 L323.8 431.9 L327.9 446.4 L340.9 463.4 L336.2 473.2 L337 481.3 L344.6 494.6 L346.1 510.3 L342.5 520.6 L342.6 539.1 L346.3 544.8 z",
            [12] = "M321.6 381.5 L325.3 369.6 L325 357.5 L327.5 357.4 L330.9 360.1 L339.7 360.6 L344.6 342 L334 336.9 L319.2 343.4 L305.7 332 L301.4 337.3 L301 345.8 L321.6 381.5 z",
            [13] = "M451.4 447.4 L437.4 441 L435.7 433.8 L437.2 420.2 L433.7 404 L434.9 392.5 L420.3 381.2 L413.4 381.6 L400 372.9 L393 374 L379.5 362.5 L372.4 376.6 L368.4 376.5 L361.8 366.5 L356.7 347.4 L349.8 341.5 L344.6 342 L339.7 360.6 L330.9 360.1 L327.5 357.4 L325 357.5 L325.3 369.6 L321.6 381.5 L336 406.5 L357.5 434 L373.2 449 L387.2 455.7 L396.1 465.4 L398.7 477 L403.3 483.9 L414 486 L420.8 485.6 L426 483.3 L427.2 480 L431.8 473.4 L433.4 462.8 L440.2 455.2 L451.4 447.4 z",
            [14] = "M334 336.9 L341 319.9 L341 314 L343 311.3 L342.2 289.1 L329.5 270.7 L317.2 262 L312.5 271.2 L302.7 280.5 L302.7 285.5 L312 285.4 L320.2 297.1 L317.7 311.3 L310.1 321.9 L305.7 332 L319.2 343.4 L334 336.9 z",
            [15] = "M344.6 342 L349.8 341.5 L356.7 347.4 L361.8 366.5 L368.4 376.5 L372.4 376.6 L379.5 362.5 L376.1 345.9 L388.7 311.3 L392.5 295.2 L391.9 273.8 L385.7 260.7 L385.6 245.2 L378.7 248.4 L375.5 252.5 L371.6 254.4 L368.3 261.3 L362 260.9 L349.1 265.7 L345.8 269.8 L333.3 276.3 L342.2 289.1 L343 311.3 L341 314 L341 319.9 L334 336.9 L344.6 342 z",
            [16] = "M482.6 390.3 L491.5 364.8 L495.8 356.2 L496.5 342.4 L494.6 336 L495.7 334.1 L493.6 328.8 L495.6 319.7 L501.6 311.3 L502.1 303.7 L495.9 303.7 L486.8 292.2 L484.5 279.6 L488.2 269.5 L497 262.8 L497.9 258.8 L513.4 242.2 C510 230.6 505.6 219.1 500.3 207.8 L490.8 208.1 L487.8 210.3 L484.4 208.6 L475.4 215 L468.8 210.9 L453.9 215.4 L449 210 L437 210.5 L433.3 207.7 L430.8 199 L412.5 213.9 L407.2 227.3 L400.3 233.9 L396 235.2 L395.6 238.6 L392.6 242.1 L385.6 245.2 L385.7 260.7 L391.9 273.8 L392.5 295.2 L388.7 311.3 L376.1 345.9 L379.5 362.5 L393 374 L400 372.9 L413.4 381.6 L420.3 381.2 L434.9 392.5 L443.5 377.4 L453.8 379.9 L464.4 381.1 L469.5 386.4 L482.6 390.3 z",
            [17] = "M523.5 307.3 C523.1 286.3 520 264.2 513.4 242.2 L497.9 258.8 L497 262.8 L488.2 269.5 L484.5 279.6 L486.8 292.2 L495.9 303.7 L502.1 303.7 L501.6 311.3 L523.5 311.3 L523.5 307.3 z",
            [18] = "M421.6 508.9 C454.2 486 481.1 453.8 498.4 418.7 C515.8 383.7 523.6 345.7 523.5 311.3 L501.6 311.3 L495.6 319.7 L493.6 328.8 L495.7 334.1 L494.6 336 L496.5 342.4 L495.8 356.2 L491.5 364.8 L482.6 390.3 L479.1 402.4 L479.3 414.8 L470.7 430.2 L451.4 447.4 L440.2 455.2 L433.4 462.8 L431.8 473.4 L427.2 480 L426 483.3 L420.8 485.6 L422 487.5 L421.6 508.9 z",
            [19] = "M383.7 163.9 L388.7 164.5 L397.6 160.1 L405 158.9 L415 153 L424 152 L428.3 143.7 L442.7 130.6 C434.1 123 425 115.9 415.5 109.6 L404.1 126.1 L393.5 132.1 L393.5 142.1 L389.1 147.4 L386.9 158.4 L383.7 163.9 z",
            [20] = "M355.3 195.8 L367.4 187.2 L367.3 183.9 L374.5 179.6 L375 176.7 L378.2 176.5 L378.9 172.2 L386.9 158.4 L389.1 147.4 L393.5 142.1 L393.5 132.1 L390.2 130.6 L386.5 134.2 L383.3 134.2 L361.1 172.7 L354.6 183.9 L355.8 190.1 L353.8 193.3 L355.3 195.8 z",
            [21] = "M418.9 168.3 L424.2 161.4 L424 152 L415 153 L405 158.9 L397.6 160.1 L388.7 164.5 L383.7 163.9 L378.9 172.2 L378.2 176.5 L375 176.7 L374.5 179.6 L367.3 183.9 L367.4 187.2 L355.3 195.8 L345.8 221.8 L350.4 220.9 L384.3 201.4 L391 200.7 L407.4 188.2 L407.8 182.8 L418.9 168.3 z",
            [22] = "M418.9 168.3 L435.6 165.8 L449 170.4 L466.8 155.5 C459.4 146.7 451.3 138.3 442.7 130.6 L428.3 143.7 L424 152 L424.2 161.4 L418.9 168.3 z",
            [23] = "M333.3 276.3 L345.8 269.8 L349.1 265.7 L362 260.9 L368.3 261.3 L371.6 254.4 L375.5 252.5 L378.7 248.4 L392.6 242.1 L395.6 238.6 L396 235.2 L400.3 233.9 L407.2 227.3 L412.5 213.9 L439.7 191.8 L448.9 174.6 L449 170.4 L435.6 165.8 L418.9 168.3 L407.8 182.8 L407.4 188.2 L391 200.7 L384.3 201.4 L350.4 220.9 L342.5 222.4 L331.6 240.5 L333.1 243.9 L317.2 262 L329.5 270.7 L333.3 276.3 z",
            [24] = "M449 170.4 L448.9 174.6 L439.7 191.8 L430.8 199 L433.3 207.7 L437 210.5 L449 210 L453.9 215.4 L468.8 210.9 L475.4 215 L484.4 208.6 L487.8 210.3 L490.8 208.1 L500.3 207.8 C491.6 189.4 480.3 171.6 466.8 155.5 L449 170.4 z",
            [25] = "M320 90.4 L330.8 98.6 L336.6 99.3 L341.9 106.9 L358 106.3 L370.6 110.3 L383.1 124.6 L383.3 134.2 L386.5 134.2 L390.2 130.6 L393.5 132.1 L404.1 126.1 L415.5 109.6 C380.6 86.2 340.6 73.2 303.3 69.9 L303.3 79.1 L297.8 88.1 L305 86.7 L320 90.4 z",
            [26] = "M166 112.1 L179 116.4 L198.1 116.4 L207.8 108.2 L223.8 105.9 L243.5 98.2 L266.5 93.1 L286.1 94.3 L290.7 89.5 L297.8 88.1 L303.3 79.1 L303.3 69.9 C294.4 69.1 285.6 68.8 277 68.9 C239.3 69.5 197.5 79.5 159.8 101.4 L166 112.1 z",
            [27] = "M320 90.4 L305 86.7 L290.7 89.5 L286.1 94.3 L266.5 93.1 L243.5 98.2 L223.8 105.9 L222.6 109.6 L222.1 120.2 L220 121 L212.6 136.5 L210.3 136.6 L208.3 141.4 L202.9 145.1 L199.9 149.3 L195.4 151.5 L193.4 159.5 L186.6 170.4 L181.8 180.9 L183.2 184.6 L185.6 187.3 L194.8 184.1 L201.2 173 L218.1 159.2 L233.5 156.5 L237.7 159.1 L252.9 151.8 L250.7 139.1 L269.4 133.8 L277.5 129.2 L292.9 132.2 L297.6 127.7 L306.4 107.4 z",
            [28] = "M257.5 178 L261.6 178.3 L267.9 185.3 L275.2 182.6 L287.6 182.5 L283.8 207.7 L286.4 217 L286.2 235.3 L288.3 237.8 L287.8 241.9 L287.9 260.2 L289.3 264.5 L286.2 281.9 L281 281.9 L276.5 285.3 L267.7 288.3 L258.4 272.1 L259.4 259.7 L257.7 254.1 L253.7 249.6 L253 232.3 L254.8 215.2 L261.7 201.4 L257.5 178 z",
            [29] = "M159.8 101.4 C126 120.8 96 149.6 75.3 183.1 L84.6 192.7 L89.3 193 L91.1 195.1 L111.1 177.2 L120 181.5 L130.2 170.5 L139.8 141.9 L148 129.2 L157.9 123.1 L166 112.1 L159.8 101.4 z",
            [30] = "M235.5 273.1 L188.3 233.5 L188.4 227 L184.2 222.9 L182 212.2 L179.1 204.6 L180.7 201.7 L181.2 192.2 L183.2 184.6 L181.8 180.9 L186.6 170.4 L193.4 159.5 L195.4 151.5 L199.9 149.3 L202.9 145.1 L208.3 141.4 L210.3 136.6 L212.6 136.5 L220 121 L222.1 120.2 L222.6 109.6 L223.8 105.9 L207.8 108.2 L198.1 116.4 L179 116.4 L166 112.1 L157.9 123.1 L148 129.2 L139.8 141.9 L130.2 170.5 L120 181.5 L122 191 L127.7 200.5 L127.9 206.7 L130.5 208.7 L131 213.6 L133.9 216.7 L128.8 234 L126.2 242.8 L123.1 243.1 L121 247.5 L120.5 252.9 L185 276.4 L220 289.1 L235.5 273.1 z",
            [31] = "M267.7 288.3 L258.4 272.1 L259.4 259.7 L257.7 254.1 L253.7 249.6 L253 232.3 L254.8 215.2 L261.7 201.4 L252.9 151.8 L237.7 159.1 L233.5 156.5 L218.1 159.2 L201.2 173 L194.8 184.1 L185.6 187.3 L183.2 184.6 L181.2 192.2 L180.7 201.7 L179.1 204.6 L182 212.2 L184.2 222.9 L188.4 227 L188.3 233.5 L235.5 273.1 L254.9 289.4 L267.7 288.3 z",
            [32] = "M47 248.2 L55.4 249.7 L60.3 255 L71.3 254 L85.8 249.4 L88.5 251.2 L95.9 254.8 L99.6 258.6 L110.9 257.8 L120.5 252.9 L121 247.5 L123.1 243.1 L126.2 242.8 L128.8 234 L120.6 231.9 L112.8 232 L100.2 226.6 L94.9 229.7 L89.4 228.9 L79.5 218 L81 209.4 L86.2 207.7 L90.4 200 L91.1 195.1 L89.3 193 L84.6 192.7 L75.3 183.1 C62.4 203.7 53 225.9 47 248.2 z",
            [33] = "M42.1 270.3 L53.5 278.2 L81.4 279.1 L96.3 283.5 L115.1 282.7 L142.5 277.2 L161 279.3 L167.4 283.5 L185.2 280 L185 276.4 L120.5 252.9 L110.9 257.8 L99.6 258.6 L95.9 254.8 L88.5 251.2 L85.8 249.4 L71.3 254 L60.3 255 L55.4 249.7 L47 248.2 C45 255.5 43.4 262.9 42.1 270.3 z",
            [34] = "M185 276.4 L185.2 280 L167.4 283.5 L161 279.3 L142.5 277.2 L115.1 282.7 L96.3 283.5 L81.4 279.1 L53.5 278.2 L42.1 270.3 C39.7 284.2 38.6 298 38.6 311.3 L205.6 311.3 L216.8 300.7 L220 289.1 L185 276.4 z",
            [35] = "M212.5 368.9 L225.7 331.5 L235.3 328 L233.7 326.2 L236.2 311.3 L243.4 297.6 L254.9 289.4 L235.5 273.1 L220 289.1 L216.8 300.7 L205.6 311.3 L203 321.8 L202.8 339.8 L193.6 384.7 L212.5 368.9 z",
            [36] = "M41.8 350.3 L48.7 347.8 L71 351.3 L90 352.2 L106.7 348 L128.5 346.4 L138.2 344.2 L146.1 347 L157.5 346.2 L168.8 352.2 L169 338.4 L170.7 336.9 L171.4 334.5 L181.7 329.7 L184 323.6 L203 321.8 L205.6 311.3 L38.6 311.3 L38.7 315.4 C38.9 326.8 39.9 338.5 41.8 350.3 z",
            [37] = "M168.8 352.2 L157.5 346.2 L146.1 347 L138.2 344.2 L128.5 346.4 L106.7 348 L90 352.2 L71 351.3 L48.7 347.8 L41.8 350.3 C44.2 364.9 47.9 379.7 53.2 394.3 L106.3 375 L117.3 375.9 L123.7 381.8 L129.2 381.5 L140.1 387.4 L149.2 389.6 L153.5 379 L169.2 362 L169.5 354.8 L168.8 352.2 z",
            [38] = "M203 321.8 L184 323.6 L181.7 329.7 L171.4 334.5 L170.7 336.9 L169 338.4 L168.8 352.2 L169.5 354.8 L169.2 362 L153.5 379 L149.2 389.6 L151.8 392.5 L154.6 397.9 L154.7 405.1 L149.7 409.5 L150.7 420.7 L159 438.5 L160.5 453.3 L169.6 459.3 L178.4 459.3 L176.5 428.8 L186.5 417.4 L193.6 384.7 L202.8 339.8 L203 321.8 z",
            [39] = "M235.3 328 L225.7 331.5 L212.5 368.9 L213.3 388.3 L224.1 410 L264.1 340.8 L256.8 341.8 L248.8 338.5 L242.4 332.3 L238.9 332 L235.3 328 z",
            [40] = "M178.419 459.259L169.587 459.259L160.505 453.319L158.971 438.467L150.68 420.751L149.705 409.503L154.737 405.115L154.583 397.874L151.821 392.536L149.214 389.651L140.11 387.414L129.214 381.509L123.695 381.768L117.344 375.877L106.295 374.956L53.249 394.264C71.319 445.451 110.68 493.501 159.847 521.302L179.48 487.295L183.096 466.835L178.419 459.259ZM139.629 430.025L139.629 430.025L117.001 449.011L96.807 427.145L91.241 407.55L97.789 396.841L112.334 391.744L125.942 395.017L128.561 401.424L129.122 408.954Z",
            [41] = "M193.6 384.7 L186.5 417.4 L176.5 428.8 L178.4 459.3 L183.1 466.8 L179.5 487.3 L195.1 486.8 L208.8 474 L210.1 462.8 L217.3 453.4 L219.7 438.3 L218 432.7 L224.5 427.7 L221.1 424.6 L219.8 417.5 L224.1 410 L213.3 388.3 L212.5 368.9 L193.6 384.7 z"
        },

        LocationCenter_Point = new Dictionary<int, PointD>
        {
            [0] = new(277, 314), //Polar Sink
            [1] = new(327, 243), //Imperial Basin (East Sector)
            [2] = new(323, 204), //Imperial Basin (Center Sector)
            [3] = new(291, 204), //Imperial Basin (West Sector)
            [4] = new(274, 169), //Carthag
            [5] = new PointD(348, 146), //Arrakeen
            [6] = new PointD(451, 407), //Tuek's Sietch
            [7] = new PointD(110, 201), //Sietch Tabr
            [8] = new PointD(109, 408), //Habbanya Sietch
            [9] = new PointD(241, 417), //Cielago North (West Sector)
            [10] = new PointD(279, 421), //Cielago North (Center Sector)
            [11] = new PointD(316, 416), //Cielago North (East Sector)
            [12] = new PointD(236, 457), //Cielago Depression (West Sector)
            [13] = new PointD(277, 454), //Cielago Depression (Center Sector)
            [14] = new PointD(324, 461), //Cielago Depression (East Sector)
            [15] = new PointD(217, 511), //Meridian (West Sector)
            [16] = new PointD(253, 523), //Meridian (East Sector)
            [17] = new PointD(296, 498), //Cielago South (West Sector)
            [18] = new PointD(326, 497), //Cielago South (East Sector)
            [19] = new PointD(358, 462), //Cielago East (West Sector)
            [20] = new PointD(380, 468), //Cielago East (East Sector)
            [21] = new PointD(312, 355), //Harg Pass (West Sector)
            [22] = new PointD(333, 346), //Harg Pass (East Sector)
            [23] = new PointD(388, 434), //False Wall South (West Sector)
            [24] = new PointD(394, 393), //False Wall South (East Sector)
            [25] = new PointD(319, 333), //False Wall East (Far South Sector)
            [26] = new PointD(326, 320), //False Wall East (South Sector)
            [27] = new PointD(332, 303), //False Wall East (Middle Sector)
            [28] = new PointD(325, 284), //False Wall East (North Sector)
            [29] = new PointD(316, 276), //False Wall East (Far North Sector)
            [30] = new PointD(367, 355), //The Minor Erg (Far South Sector)
            [31] = new PointD(361, 326), //The Minor Erg (South Sector)
            [32] = new PointD(379, 295), //The Minor Erg (North Sector)
            [33] = new PointD(371, 268), //The Minor Erg (Far North Sector)
            [34] = new PointD(408, 365), //Pasty Mesa (Far South Sector)
            [35] = new PointD(433, 336), //Pasty Mesa (South Sector)
            [36] = new PointD(439, 288), //Pasty Mesa (North Sector)
            [37] = new PointD(431, 234), //Pasty Mesa (Far North Sector)
            [38] = new PointD(489, 279), //Red Chasm
            [39] = new PointD(446, 473), //South Mesa (South Sector)
            [40] = new PointD(460, 449), //South Mesa (Middle Sector)
            [41] = new PointD(507, 346), //South Mesa (North Sector)
            [42] = new PointD(428, 129), //Basin
            [43] = new PointD(367, 179), //Rim Wall West
            [44] = new PointD(393, 187), //Hole In The Rock
            [45] = new PointD(428, 159), //Sihaya Ridge
            [46] = new PointD(383, 235), //Shield Wall (South Sector)
            [47] = new PointD(355, 235), //Shield Wall (North Sector)
            [48] = new PointD(477, 190), //Gara Kulon
            [49] = new PointD(400, 118), //OH Gap (East Sector)
            [50] = new PointD(357, 93), //OH Gap (Middle Sector)
            [51] = new PointD(312, 80), //OH Gap (West Sector)
            [52] = new PointD(278, 82), //Broken Land (East Sector)
            [53] = new PointD(219, 97), //Broken Land (West Sector)
            [54] = new PointD(272, 114), //Tsimpo (East Sector)
            [55] = new PointD(230, 137), //Tsimpo (Middle Sector)
            [56] = new PointD(192, 178), //Tsimpo (West Sector)
            [57] = new PointD(275, 223), //Arsunt (East Sector)
            [58] = new PointD(266, 268), //Arsunt (West Sector)
            [59] = new PointD(124, 156), //Rock Outcroppings (North Sector)
            [60] = new PointD(81, 180), //Rock Outcroppings (South Sector)
            [61] = new PointD(198, 132), //Plastic Basin (North Sector)
            [62] = new PointD(165, 165), //Plastic Basin (Middle Sector)
            [63] = new PointD(172, 254), //Plastic Basin (South Sector)
            [64] = new PointD(235, 188), //Hagga Basin (East Sector)
            [65] = new PointD(199, 203), //Hagga Basin (West Sector)
            [66] = new PointD(71, 214), //Bight Of The Cliff (North Sector)
            [67] = new PointD(67, 244), //Bight Of The Cliff (South Sector)
            [68] = new PointD(85, 261), //Funeral Plain
            [69] = new PointD(143, 295), //The Great Flat
            [70] = new PointD(236, 287), //Wind Pass (Far North Sector)
            [71] = new PointD(227, 303), //Wind Pass (North Sector)
            [72] = new PointD(213, 327), //Wind Pass (South Sector)
            [73] = new PointD(207, 357), //Wind Pass (Far South Sector)
            [74] = new PointD(155, 327), //The Greater Flat
            [75] = new PointD(84, 371), //Habbanya Erg (West Sector)
            [76] = new PointD(145, 375), //Habbanya Erg (East Sector)
            [77] = new PointD(192, 334), //False Wall West (North Sector)
            [78] = new PointD(196, 374), //False Wall West (Middle Sector)
            [79] = new PointD(163, 449), //False Wall West (South Sector)
            [80] = new PointD(241, 336), //Wind Pass North (North Sector)
            [81] = new PointD(247, 355), //Wind Pass North (South Sector)
            [82] = new PointD(79, 408), //Habbanya Ridge Flat (West Sector)
            [83] = new PointD(152, 485), //Habbanya Ridge Flat (East Sector)
            [84] = new PointD(205, 402), //Cielago West (North Sector)
            [85] = new PointD(201, 463) //Cielago West (South Sector)
        },

        LocationSpice_Point = new Dictionary<int, PointD>
        {
            [11] = new PointD(309, 393), //Cielago North (East Sector)
            [17] = new PointD(290, 539), //Cielago South (West Sector)
            [33] = new PointD(362, 272), //The Minor Erg (Far North Sector)
            [38] = new PointD(509, 292), //Red Chasm
            [40] = new PointD(491, 401), //South Mesa (Middle Sector)
            [45] = new PointD(442, 144), //Sihaya Ridge
            [50] = new PointD(335, 86), //OH Gap (Middle Sector)
            [53] = new PointD(196, 99), //Broken Land (West Sector)
            [60] = new PointD(94, 176), //Rock Outcroppings (South Sector)
            [65] = new PointD(216, 233), //Hagga Basin (West Sector)
            [68] = new PointD(59, 266), //Funeral Plain
            [69] = new PointD(55, 293), //The Great Flat
            [75] = new PointD(60, 367), //Habbanya Erg (West Sector)
            [80] = new PointD(233, 340), //Wind Pass North (North Sector)
            [83] = new PointD(133, 483) //Habbanya Ridge Flat (East Sector)
        },

        FactionName_STR = new Dictionary<Faction, string>
            {
                [Faction.None] = "None",
                [Faction.Green] = "Atreides",
                [Faction.Black] = "Harkonnen",
                [Faction.Yellow] = "Fremen",
                [Faction.Red] = "Emperor",
                [Faction.Orange] = "Guild",
                [Faction.Blue] = "Bene Gesserit"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "Ixian",
                    [Faction.Purple] = "Tleilaxu"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "CHOAM",
                    [Faction.White] = "Richese"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "Ecaz",
                    [Faction.Cyan] = "Moritani"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionTableImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionFacedownImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionForceImage_URL = new Dictionary<Faction, string>
            {
                [Faction.Green] = DEFAULT_ART_LOCATION + "/art/faction1force.svg",
                [Faction.Black] = DEFAULT_ART_LOCATION + "/art/faction2force.svg",
                [Faction.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3force.svg",
                [Faction.Red] = DEFAULT_ART_LOCATION + "/art/faction4force.svg",
                [Faction.Orange] = DEFAULT_ART_LOCATION + "/art/faction5force.svg",
                [Faction.Blue] = DEFAULT_ART_LOCATION + "/art/faction6force.svg"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = DEFAULT_ART_LOCATION + "/art/faction7force.svg",
                    [Faction.Purple] = DEFAULT_ART_LOCATION + "/art/faction8force.svg"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = DEFAULT_ART_LOCATION + "/art/faction9force.svg",
                    [Faction.White] = DEFAULT_ART_LOCATION + "/art/faction10force.svg"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = DEFAULT_ART_LOCATION + "/art/faction11force.svg",
                    [Faction.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12force.svg"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionSpecialForceImage_URL = new Dictionary<Faction, string>
            {
                { Faction.Yellow, DEFAULT_ART_LOCATION + "/art/faction3specialforce.svg" },
                { Faction.Red, DEFAULT_ART_LOCATION + "/art/faction4specialforce.svg" },
                { Faction.Blue, DEFAULT_ART_LOCATION + "/art/faction6specialforce.svg" }
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    { Faction.Grey, DEFAULT_ART_LOCATION + "/art/faction7specialforce.svg" }
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    { Faction.White, DEFAULT_ART_LOCATION + "/art/faction10specialforce.svg" }
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        FactionColor = new Dictionary<Faction, string>
            {
                [Faction.None] = "#646464",
                [Faction.Green] = "#63842e",
                [Faction.Black] = "#2c2c2c",
                [Faction.Yellow] = "#d29422",
                [Faction.Red] = "#b33715",
                [Faction.Orange] = "#c85b20",
                [Faction.Blue] = "#385884"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "#b0b079",
                    [Faction.Purple] = "#602d8b"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "#582d1b",
                    [Faction.White] = "#b3afa4"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "#a85f9c",
                    [Faction.Cyan] = "#289caa"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        ForceName_STR = new Dictionary<Faction, string>
            {
                [Faction.None] = "-",
                [Faction.Green] = "forces",
                [Faction.Black] = "forces",
                [Faction.Yellow] = "forces",
                [Faction.Red] = "forces",
                [Faction.Orange] = "forces",
                [Faction.Blue] = "fighters"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "suboids",
                    [Faction.Purple] = "forces"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "forces",
                    [Faction.White] = "forces"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "forces",
                    [Faction.Cyan] = "forces"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        SpecialForceName_STR = new Dictionary<Faction, string>
            {
                [Faction.None] = "-",
                [Faction.Green] = "-",
                [Faction.Black] = "-",
                [Faction.Yellow] = "Fedaykin",
                [Faction.Red] = "Sardaukar",
                [Faction.Orange] = "-",
                [Faction.Blue] = "advisors"
            }
            .Concat(Game.ExpansionLevel < 1 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Grey] = "cyborgs",
                    [Faction.Purple] = "-"
                }
            ).Concat(Game.ExpansionLevel < 2 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Brown] = "-",
                    [Faction.White] = "No-Field"
                }
            ).Concat(Game.ExpansionLevel < 3 ? Array.Empty<KeyValuePair<Faction, string>>() : new Dictionary<Faction, string>
                {
                    [Faction.Pink] = "-",
                    [Faction.Cyan] = "-"
                }
            ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

        AmbassadorName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<Ambassador, string>
        {
            [Ambassador.None] = "None",
            [Ambassador.Green] = "Atreides",
            [Ambassador.Black] = "Harkonnen",
            [Ambassador.Yellow] = "Fremen",
            [Ambassador.Red] = "Emperor",
            [Ambassador.Orange] = "Guild",
            [Ambassador.Blue] = "Bene Gesserit",
            [Ambassador.Grey] = "Ixian",
            [Ambassador.Purple] = "Tleilaxu",
            [Ambassador.Brown] = "CHOAM",
            [Ambassador.White] = "Richese",
            [Ambassador.Pink] = "Ecaz",
            [Ambassador.Cyan] = "Moritani"
        },

        AmbassadorImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<Ambassador, string>
        {
            [Ambassador.Green] = DEFAULT_ART_LOCATION + "/art/faction1ambassador.svg",
            [Ambassador.Black] = DEFAULT_ART_LOCATION + "/art/faction2ambassador.svg",
            [Ambassador.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3ambassador.svg",
            [Ambassador.Red] = DEFAULT_ART_LOCATION + "/art/faction4ambassador.svg",
            [Ambassador.Orange] = DEFAULT_ART_LOCATION + "/art/faction5ambassador.svg",
            [Ambassador.Blue] = DEFAULT_ART_LOCATION + "/art/faction6ambassador.svg",
            [Ambassador.Grey] = DEFAULT_ART_LOCATION + "/art/faction7ambassador.svg",
            [Ambassador.Purple] = DEFAULT_ART_LOCATION + "/art/faction8ambassador.svg",

            [Ambassador.Brown] = DEFAULT_ART_LOCATION + "/art/faction9ambassador.svg",
            [Ambassador.White] = DEFAULT_ART_LOCATION + "/art/faction10ambassador.svg",

            [Ambassador.Pink] = DEFAULT_ART_LOCATION + "/art/faction11ambassador.svg",
            [Ambassador.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12ambassador.svg"
        },

        AmbassadorDescription_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<Ambassador, string>
        {
            [Ambassador.None] = "None",

            [Ambassador.Green] = "Atreides - See visitor's hand",
            [Ambassador.Black] = "Harkonnen - Look at a random Traitor Card the visiting faction holds",
            [Ambassador.Yellow] = "Fremen - Move a group of your forces on the board to any territory (subject to storm and occupancy rules)",
            [Ambassador.Red] = "Emperor - Gain 5 spice",
            [Ambassador.Orange] = "Guild - Send up to 4 of your forces in reserves to any territory not in storm for free",
            [Ambassador.Blue] = "Bene Gesserit - Trigger the effect of any Ambassador that was not part of your supply",

            [Ambassador.Grey] = "Ixian - Discard a Treachery Card and draw from the deck",
            [Ambassador.Purple] = "Tleilaxu - Revive one of your leaders or up to 4 of your forces for free",

            [Ambassador.Brown] = "CHOAM - Discard any of your Treachery Cards and gain 3 spice for each one",
            [Ambassador.White] = "Richese - Pay 3 spice to the Bank for a Treachery Card",

            [Ambassador.Pink] = "Ecaz - Gain Vidal if he is not in the Tanks, captured, or a ghola until used in a battle, or form an alliance with the visiting faction (if neither of you are allied); they may take control of Vidal instead. This token returns to your available supply"
        },

        TechTokenName_STR = Game.ExpansionLevel < 1 ? new() : new Dictionary<TechToken, string>
        {
            { TechToken.Graveyard, "Axlotl Tanks" },
            { TechToken.Ships, "Heighliners" },
            { TechToken.Resources, "Spice Production" }
        },

        TechTokenImage_URL = Game.ExpansionLevel < 1 ? new() : new Dictionary<TechToken, string>
        {
            { TechToken.Graveyard, DEFAULT_ART_LOCATION + "/art/techtoken0.svg" },
            { TechToken.Ships, DEFAULT_ART_LOCATION + "/art/techtoken1.svg" },
            { TechToken.Resources, DEFAULT_ART_LOCATION + "/art/techtoken2.svg" }
        },

        LeaderSkillCardName_STR = Game.ExpansionLevel < 2 ? new() : new Dictionary<LeaderSkill, string>
        {
            [LeaderSkill.Bureaucrat] = "Bureaucrat",
            [LeaderSkill.Diplomat] = "Diplomat",
            [LeaderSkill.Decipherer] = "Rihani Decipherer",
            [LeaderSkill.Smuggler] = "Smuggler",
            [LeaderSkill.Graduate] = "Suk Graduate",
            [LeaderSkill.Planetologist] = "Planetologist",
            [LeaderSkill.Warmaster] = "Warmaster",
            [LeaderSkill.Adept] = "Prana Bindu Adept",
            [LeaderSkill.Swordmaster] = "Swordmaster of Ginaz",
            [LeaderSkill.KillerMedic] = "Killer Medic",
            [LeaderSkill.MasterOfAssassins] = "Master of Assassins",
            [LeaderSkill.Sandmaster] = "Sandmaster",
            [LeaderSkill.Thinker] = "Mentat",
            [LeaderSkill.Banker] = "Spice Banker"
        },

        LeaderSkillCardImage_URL = Game.ExpansionLevel < 2 ? new() : new Dictionary<LeaderSkill, string>
        {
            [LeaderSkill.Bureaucrat] = DEFAULT_ART_LOCATION + "/art/Bureaucrat.gif",
            [LeaderSkill.Diplomat] = DEFAULT_ART_LOCATION + "/art/Diplomat.gif",
            [LeaderSkill.Decipherer] = DEFAULT_ART_LOCATION + "/art/Decipherer.gif",
            [LeaderSkill.Smuggler] = DEFAULT_ART_LOCATION + "/art/Smuggler.gif",
            [LeaderSkill.Graduate] = DEFAULT_ART_LOCATION + "/art/Graduate.gif",
            [LeaderSkill.Planetologist] = DEFAULT_ART_LOCATION + "/art/Planetologist.gif",
            [LeaderSkill.Warmaster] = DEFAULT_ART_LOCATION + "/art/Warmaster.gif",
            [LeaderSkill.Adept] = DEFAULT_ART_LOCATION + "/art/Adept.gif",
            [LeaderSkill.Swordmaster] = DEFAULT_ART_LOCATION + "/art/Swordmaster.gif",
            [LeaderSkill.KillerMedic] = DEFAULT_ART_LOCATION + "/art/KillerMedic.gif",
            [LeaderSkill.MasterOfAssassins] = DEFAULT_ART_LOCATION + "/art/MasterOfAssassins.gif",
            [LeaderSkill.Sandmaster] = DEFAULT_ART_LOCATION + "/art/Sandmaster.gif",
            [LeaderSkill.Thinker] = DEFAULT_ART_LOCATION + "/art/Mentat.gif",
            [LeaderSkill.Banker] = DEFAULT_ART_LOCATION + "/art/Banker.gif"
        },

        HomeWorldImage_URL = new Dictionary<World, string>
        {
            [World.Green] = DEFAULT_ART_LOCATION + "/art/faction1planet.svg",
            [World.Black] = DEFAULT_ART_LOCATION + "/art/faction2planet.svg",
            [World.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3planet.svg",
            [World.Red] = DEFAULT_ART_LOCATION + "/art/faction4planet.svg",
            [World.RedStar] = DEFAULT_ART_LOCATION + "/art/faction4planet2.svg",
            [World.Orange] = DEFAULT_ART_LOCATION + "/art/faction5planet.svg",
            [World.Blue] = DEFAULT_ART_LOCATION + "/art/faction6planet.svg",

            [World.Grey] = DEFAULT_ART_LOCATION + "/art/faction7planet.svg",
            [World.Purple] = DEFAULT_ART_LOCATION + "/art/faction8planet.svg",

            [World.Brown] = DEFAULT_ART_LOCATION + "/art/faction9planet.svg",
            [World.White] = DEFAULT_ART_LOCATION + "/art/faction10planet.svg",

            [World.Pink] = DEFAULT_ART_LOCATION + "/art/faction11planet.svg",
            [World.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12planet.svg"
        },

        HomeWorldCardImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<World, string>
        {
            [World.Green] = DEFAULT_ART_LOCATION + "/art/CaladanCard.gif",
            [World.Black] = DEFAULT_ART_LOCATION + "/art/GiediPrimeCard.gif",
            [World.Yellow] = DEFAULT_ART_LOCATION + "/art/ArrakisCard.gif",
            [World.Red] = DEFAULT_ART_LOCATION + "/art/KaitainCard.gif",
            [World.RedStar] = DEFAULT_ART_LOCATION + "/art/SalusaSecundusCard.gif",
            [World.Orange] = DEFAULT_ART_LOCATION + "/art/JunctionCard.gif",
            [World.Blue] = DEFAULT_ART_LOCATION + "/art/WallachIXCard.gif",

            [World.Grey] = DEFAULT_ART_LOCATION + "/art/IxCard.gif",
            [World.Purple] = DEFAULT_ART_LOCATION + "/art/TleilaxCard.gif",

            [World.Brown] = DEFAULT_ART_LOCATION + "/art/TupileCard.gif",
            [World.White] = DEFAULT_ART_LOCATION + "/art/RicheseCard.gif",

            [World.Pink] = DEFAULT_ART_LOCATION + "/art/EcazCard.gif",
            [World.Cyan] = DEFAULT_ART_LOCATION + "/art/GrummanCard.gif"
        },

        NexusCardImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<Nexus, string>
        {
            [Nexus.Green] = DEFAULT_ART_LOCATION + "/art/faction1nexus.gif",
            [Nexus.Black] = DEFAULT_ART_LOCATION + "/art/faction2nexus.gif",
            [Nexus.Yellow] = DEFAULT_ART_LOCATION + "/art/faction3nexus.gif",
            [Nexus.Red] = DEFAULT_ART_LOCATION + "/art/faction4nexus.gif",
            [Nexus.Orange] = DEFAULT_ART_LOCATION + "/art/faction5nexus.gif",
            [Nexus.Blue] = DEFAULT_ART_LOCATION + "/art/faction6nexus.gif",

            [Nexus.Grey] = DEFAULT_ART_LOCATION + "/art/faction7nexus.gif",
            [Nexus.Purple] = DEFAULT_ART_LOCATION + "/art/faction8nexus.gif",

            [Nexus.Brown] = DEFAULT_ART_LOCATION + "/art/faction9nexus.gif",
            [Nexus.White] = DEFAULT_ART_LOCATION + "/art/faction10nexus.gif",

            [Nexus.Pink] = DEFAULT_ART_LOCATION + "/art/faction11nexus.gif",
            [Nexus.Cyan] = DEFAULT_ART_LOCATION + "/art/faction12nexus.gif"
        },

        TerrorTokenName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<TerrorType, string>
        {
            [TerrorType.None] = "None",
            [TerrorType.Assassination] = "Assassination",
            [TerrorType.Atomics] = "Atomics",
            [TerrorType.Extortion] = "Extortion",
            [TerrorType.Robbery] = "Robbery",
            [TerrorType.Sabotage] = "Sabotage",
            [TerrorType.SneakAttack] = "Sneak Attack"
        },

        TerrorTokenDescription_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<TerrorType, string>
        {
            [TerrorType.None] = "None",
            [TerrorType.Assassination] = "Choose a random leader from that player, send it to the Tanks and collect spice for it",
            [TerrorType.Atomics] = "All forces in the territory go to the Tanks. Place the Atomics Aftermath token in the territory. No forces may ever ship into this territory (even Fremen). From this turn forward, your hand limit is reduced by 1 (as well as your ally’s), discarding a random card if a hand exceeds the limit",
            [TerrorType.Extortion] = "Gain 5 spice from the Spice Bank, placed in front of your shield. Collect it in the Mentat Pause, then regain this Terror token unless any one player in storm order pays you 3 spice",
            [TerrorType.Robbery] = "Steal half the spice (rounded up) from that player or take the top card of the Treachery Deck (then discarding a card of your choice if you exceed your hand size)",
            [TerrorType.Sabotage] = "Draw a random Treachery Card from that player and discard it if possible. Then you may give that player a Treachery Card of your choice from your hand",
            [TerrorType.SneakAttack] = "Send up to 5 of your forces in reserves into that territory at no cost (subject to storm and occupancy rules), even if the Atomics Aftermath token is there"
        },

        DiscoveryTokenName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryToken, string>
        {
            [DiscoveryToken.Jacurutu] = "Jacurutu Sietch",
            [DiscoveryToken.Shrine] = "Shrine",
            [DiscoveryToken.TestingStation] = "Ecological Testing Station",
            [DiscoveryToken.Cistern] = "Cistern",
            [DiscoveryToken.ProcessingStation] = "Orgiz Processing Station",
            [DiscoveryToken.CardStash] = "Treachery Card Stash",
            [DiscoveryToken.ResourceStash] = "Spice Stash",
            [DiscoveryToken.Flight] = "Ornithopter"
        },

        DiscoveryTokenDescription_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryToken, string>
        {
            [DiscoveryToken.Jacurutu] = "Counts as a normal stronghold.  If you win a battle here, gain 1 spice for each of your opponent’s undialed forces that go to the Tanks",
            [DiscoveryToken.Shrine] = "If occupied, you may play Truthtrance as a Karama card, and vice versa",
            [DiscoveryToken.TestingStation] = "If occupied during Storm, you may add or subtract the movement of the storm by 1, not affecting Weather Control",
            [DiscoveryToken.Cistern] = "If occupied during Collection, gain 2 spice from the bank",
            [DiscoveryToken.ProcessingStation] = "If occupied during Collection, steal 1 spice of each spice blow collected",
            [DiscoveryToken.CardStash] = "Gain a treachery card, remove this token from the game and discard a card if you exceed your maximum hand size",
            [DiscoveryToken.ResourceStash] = "Gain 7 spice from the bank and remove this token from the game",
            [DiscoveryToken.Flight] = "Gain the token and remove it from the game to gain 3 movement for one movement action."
        },

        DiscoveryTokenTypeName_STR = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryTokenType, string>
        {
            [DiscoveryTokenType.Yellow] = "Hiereg",
            [DiscoveryTokenType.Orange] = "Smuggler"
        },

        DiscoveryTokenTypeImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryTokenType, string>
        {
            [DiscoveryTokenType.Yellow] = "/art/discoverytype1.png",
            [DiscoveryTokenType.Orange] = "/art/discoverytype2.png"
        },

        DiscoveryTokenImage_URL = Game.ExpansionLevel < 3 ? new() : new Dictionary<DiscoveryToken, string>
        {
            [DiscoveryToken.Jacurutu] = "/art/discovery1.png",
            [DiscoveryToken.Cistern] = "/art/discovery2.png",
            [DiscoveryToken.TestingStation] = "/art/discovery3.png",
            [DiscoveryToken.Shrine] = "/art/discovery4.png",
            [DiscoveryToken.ProcessingStation] = "/art/discovery5.png"
        },

        StrongholdCardName_STR = Game.ExpansionLevel < 2 ? new() : new Dictionary<int, string>
        {
            [2] = "Carthag",
            [3] = "Arrakeen",
            [4] = "Tuek's Sietch",
            [5] = "Sietch Tabr",
            [6] = "Habbanya Sietch",
            [42] = "Hidden Mobile Stronghold"
        },

        StrongholdCardImage_URL = Game.ExpansionLevel < 2 ? new() : new Dictionary<StrongholdAdvantage, string>
        {
            [StrongholdAdvantage.CountDefensesAsAntidote] = DEFAULT_ART_LOCATION + "/art/Carthag.gif",
            [StrongholdAdvantage.FreeResourcesForBattles] = DEFAULT_ART_LOCATION + "/art/Arrakeen.gif",
            [StrongholdAdvantage.CollectResourcesForUseless] = DEFAULT_ART_LOCATION + "/art/TueksSietch.gif",
            [StrongholdAdvantage.CollectResourcesForDial] = DEFAULT_ART_LOCATION + "/art/SietchTabr.gif",
            [StrongholdAdvantage.WinTies] = DEFAULT_ART_LOCATION + "/art/HabbanyaSietch.gif",
            [StrongholdAdvantage.AnyOtherAdvantage] = DEFAULT_ART_LOCATION + "/art/HMS.gif"
        },

        MusicGeneral_URL = DEFAULT_ART_LOCATION + "/art/101_-_Dune_-_DOS_-_Arrakis.mp3",
        MusicResourceBlow_URL = DEFAULT_ART_LOCATION + "/art/104_-_Dune_-_DOS_-_Sequence.mp3",
        MusicSetup_URL = DEFAULT_ART_LOCATION + "/art/109_-_Dune_-_DOS_-_Water.mp3",
        MusicBidding_URL = DEFAULT_ART_LOCATION + "/art/103_-_Dune_-_DOS_-_Baghdad.mp3",
        MusicShipmentAndMove_URL = DEFAULT_ART_LOCATION + "/art/106_-_Dune_-_DOS_-_Wormsuit.mp3",
        MusicBattle_URL = DEFAULT_ART_LOCATION + "/art/105_-_Dune_-_DOS_-_Worm_Intro.mp3",
        MusicBattleClimax_URL = DEFAULT_ART_LOCATION + "/art/108_-_Dune_-_DOS_-_War_Song.mp3",
        MusicMentat_URL = DEFAULT_ART_LOCATION + "/art/102_-_Dune_-_DOS_-_Morning.mp3",

        Sound_YourTurn_URL = DEFAULT_ART_LOCATION + "/art/yourturn.mp3",
        Sound_Chatmessage_URL = DEFAULT_ART_LOCATION + "/art/whisper.mp3",

        Sound = new Dictionary<Milestone, string>
        {
            [Milestone.GameStarted] = DEFAULT_ART_LOCATION + "/art/intro.mp3",
            [Milestone.Shuffled] = DEFAULT_ART_LOCATION + "/art/shuffleanddeal.mp3",
            [Milestone.BabyMonster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
            [Milestone.Monster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
            [Milestone.GreatMonster] = DEFAULT_ART_LOCATION + "/art/monster.mp3",
            [Milestone.Resource] = DEFAULT_ART_LOCATION + "/art/resource.mp3",
            [Milestone.MetheorUsed] = DEFAULT_ART_LOCATION + "/art/explosion.mp3",
            [Milestone.CharityClaimed] = DEFAULT_ART_LOCATION + "/art/bid.mp3",
            [Milestone.CardOnBidSwapped] = DEFAULT_ART_LOCATION + "/art/cardflip.mp3",
            [Milestone.Bid] = DEFAULT_ART_LOCATION + "/art/bid.mp3",
            [Milestone.CardWonSwapped] = DEFAULT_ART_LOCATION + "/art/cardflip.mp3",
            [Milestone.AuctionWon] = DEFAULT_ART_LOCATION + "/art/bell.mp3",
            [Milestone.Revival] = DEFAULT_ART_LOCATION + "/art/revival.mp3",
            [Milestone.Shipment] = DEFAULT_ART_LOCATION + "/art/shipment.mp3",
            [Milestone.Move] = DEFAULT_ART_LOCATION + "/art/move.mp3",
            [Milestone.Explosion] = DEFAULT_ART_LOCATION + "/art/explosion.mp3",
            [Milestone.LeaderKilled] = DEFAULT_ART_LOCATION + "/art/scream.mp3",
            [Milestone.Messiah] = "",
            [Milestone.TreacheryCalled] = DEFAULT_ART_LOCATION + "/art/laughter.mp3",
            [Milestone.FaceDanced] = DEFAULT_ART_LOCATION + "/art/laughter.mp3",
            [Milestone.Clairvoyance] = DEFAULT_ART_LOCATION + "/art/clairvoyance.mp3",
            [Milestone.Karma] = DEFAULT_ART_LOCATION + "/art/karma.mp3",
            [Milestone.Bribe] = DEFAULT_ART_LOCATION + "/art/bribe.mp3",
            [Milestone.GameWon] = DEFAULT_ART_LOCATION + "/art/win.mp3",
            [Milestone.Amal] = DEFAULT_ART_LOCATION + "/art/flute.mp3",
            [Milestone.Thumper] = DEFAULT_ART_LOCATION + "/art/thumping.mp3",
            [Milestone.Harvester] = DEFAULT_ART_LOCATION + "/art/engine.mp3",
            [Milestone.HmsMovement] = DEFAULT_ART_LOCATION + "/art/hms.mp3",
            [Milestone.RaiseDead] = DEFAULT_ART_LOCATION + "/art/bubbles.mp3",
            [Milestone.WeatherControlled] = DEFAULT_ART_LOCATION + "/art/thunder.mp3",
            [Milestone.Storm] = DEFAULT_ART_LOCATION + "/art/thunder.mp3",
            [Milestone.Voice] = DEFAULT_ART_LOCATION + "/art/voice.mp3",
            [Milestone.Prescience] = DEFAULT_ART_LOCATION + "/art/clairvoyance.mp3",
            [Milestone.ResourcesReceived] = DEFAULT_ART_LOCATION + "/art/bid.mp3",
            [Milestone.Economics] = DEFAULT_ART_LOCATION + "/art/bribe.mp3",
            [Milestone.CardTraded] = DEFAULT_ART_LOCATION + "/art/cardflip.mp3",
            [Milestone.Discard] = DEFAULT_ART_LOCATION + "/art/crumple.mp3",
            [Milestone.SpecialUselessPlayed] = DEFAULT_ART_LOCATION + "/art/karma.mp3",
            [Milestone.Bureaucracy] = DEFAULT_ART_LOCATION + "/art/typewriter.mp3",
            [Milestone.Audited] = DEFAULT_ART_LOCATION + "/art/typewriter.mp3",
            [Milestone.TerrorPlanted] = DEFAULT_ART_LOCATION + "/art/terror.mp3",
            [Milestone.TerrorRevealed] = DEFAULT_ART_LOCATION + "/art/terror.mp3",
            [Milestone.AmbassadorPlaced] = DEFAULT_ART_LOCATION + "/art/ambassador.mp3",
            [Milestone.AmbassadorActivated] = DEFAULT_ART_LOCATION + "/art/ambassador.mp3",
            [Milestone.NexusPlayed] = DEFAULT_ART_LOCATION + "/art/fairybell.mp3",
            [Milestone.DiscoveryAppeared] = DEFAULT_ART_LOCATION + "/art/discoveryhorn.mp3",
            [Milestone.DiscoveryRevealed] = DEFAULT_ART_LOCATION + "/art/discoveryhorn.mp3",
            [Milestone.Assassination] = DEFAULT_ART_LOCATION + "/art/scream.mp3"
        },

        MapDimensions = new PointD(563, 626),
        PlanetRadius = 242,
        MapRadius = 260,
        PlanetCenter = new PointD(281, 311),
        PlayerTokenRadius = 11,

        SpiceDeckLocation = new PointD(0, 540),
        TreacheryDeckLocation = new PointD(475, 540),
        CardSize = new PointD(40, 56),

        BattleScreenWidth = 273,

        BattleScreenHeroX = 47,
        BattleScreenHeroY = 150,
        BattleWheelHeroWidth = 86,
        BattleWheelHeroHeight = 86,

        BattleWheelForcesX = 137,
        BattleWheelForcesY = 22,

        BattleWheelCardX = 148,
        BattleWheelCardY = 102,
        BattleWheelCardWidth = 86,
        BattleWheelCardHeight = 120,

        //Monster token
        MONSTERTOKEN_RADIUS = 13,

        //Force tokens
        FORCETOKEN_FONT = "normal normal bold 8px Verdana, Open Sans, Calibri, Tahoma, sans-serif",
        FORCETOKEN_FONTCOLOR = "white",
        FORCETOKEN_FONT_BORDERCOLOR = "black",
        FORCETOKEN_FONT_BORDERWIDTH = 1,
        FORCETOKEN_RADIUS = 8,

        //Spice tokens
        RESOURCETOKEN_FONT = "normal normal bold 8px Verdana, Open Sans, Calibri, Tahoma, sans-serif",
        RESOURCETOKEN_FONTCOLOR = "white",
        RESOURCETOKEN_FONT_BORDERCOLOR = "black",
        RESOURCETOKEN_FONT_BORDERWIDTH = 1,
        RESOURCETOKEN_RADIUS = 8,

        //Other highlights
        HIGHLIGHT_OVERLAY_COLOR = "rgba(255,255,255,0.5)",
        METHEOR_OVERLAY_COLOR = "rgba(209,247,137,0.5)",
        BLOWNSHIELDWALL_OVERLAY_COLOR = "rgba(137,238,247,0.5)",
        STORM_OVERLAY_COLOR = "rgba(255,100,100,0.5)",
        STORM_PRESCIENCE_OVERLAY_COLOR = "rgba(255,100,100,0.2)",

        //Card piles
        CARDPILE_FONT = "normal normal normal 20px Advokat, Calibri, Tahoma, sans-serif",
        CARDPILE_FONTCOLOR = "white",
        CARDPILE_FONT_BORDERCOLOR = "black",
        CARDPILE_FONT_BORDERWIDTH = 1,

        //Phases
        PHASE_FONT = "normal normal normal 10px Advokat, Calibri, Tahoma, sans-serif",
        PHASE_ACTIVE_FONT = "normal normal normal 18px Advokat, Calibri, Tahoma, sans-serif",
        PHASE_FONTCOLOR = "white",
        PHASE_ACTIVE_FONTCOLOR = "rgb(231,191,60)",
        PHASE_FONT_BORDERCOLOR = "black",
        PHASE_FONT_BORDERWIDTH = 1,
        PHASE_ACTIVE_FONT_BORDERWIDTH = 1,

        //Player names
        PLAYERNAME_FONT = "normal normal normal 10px Advokat, Calibri, Tahoma, sans-serif",
        PLAYERNAME_FONTCOLOR = "white",
        PLAYERNAME_FONT_BORDERCOLOR = "black",
        PLAYERNAME_FONT_BORDERWIDTH = 1,

        SKILL_FONT = "normal normal normal 7px Advokat, Calibri, Tahoma, sans-serif",
        SKILL_FONTCOLOR = "white",
        SKILL_FONT_BORDERCOLOR = "black",
        SKILL_FONT_BORDERWIDTH = 1,

        TABLEPOSITION_BACKGROUNDCOLOR = "rgb(231,191,60)",

        //Turns
        TURN_FONT = "normal normal normal 18px Advokat, Calibri, Tahoma, sans-serif",
        TURN_FONT_COLOR = "white",
        TURN_FONT_BORDERCOLOR = "black",
        TURN_FONT_BORDERWIDTH = 1,

        //Wheel
        WHEEL_FONT = "normal normal normal 24px Advokat, Calibri, Tahoma, sans-serif",
        WHEEL_FONTCOLOR = "black",
        WHEEL_FONT_AGGRESSOR_BORDERCOLOR = "white",
        WHEEL_FONT_DEFENDER_BORDERCOLOR = "white",
        WHEEL_FONT_BORDERWIDTH = 2,

        //Shadows
        SHADOW = "#000000AA",

        //General
        FACTION_INFORMATIONCARDSTYLE = "font: normal normal normal 14px Calibri, Tahoma, sans-serif; color: white; padding: 5px 5px 5px 5px; overflow: auto; background-color: rgba(32,32,32,0.95); border-color: grey; border-style: solid; border-width: 1px; border-radius: 3px;"
    };
}