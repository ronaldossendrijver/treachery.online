/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Client
{
    public static partial class MapDrawer
    {
        [JSInvokable("DetermineTooltip")]
        public static string DetermineTooltip(int x, int y, Dimensions dimensions)
        {
            Dimensions = dimensions;
            int mapX = Dimensions.TranslateClientXToRelativeX(x);
            int mapY = Dimensions.TranslateClientYToRelativeY(y); 
            return DetermineIntelligence(mapX, mapY);
        }

        public static Dictionary<Location, string> IntelLocations = new Dictionary<Location, string>();
        public static Dictionary<int, string> IntelPlayers = new Dictionary<int, string>();
        public static Dictionary<int, string> IntelTechtokensAndSkills = new Dictionary<int, string>();
        public static string IntelTreacheryCardPile = "";
        public static string IntelResourceCardPile = "";
        public static string IntelTanks = "";
        public static string IntelOptions = "Click to show/hide Battle Wheels, Bidding Information and the Hidden Mobile Stronghold";
        public static string IntelObservers = "";

        public static void UpdateIntelligence()
        {
            try
            {
                if (h.Game != null)
                {
                    IntelTreacheryCardPile = IntelOfTreacheryCardPile();

                    IntelTanks = IntelOfTanks();
                    IntelResourceCardPile = IntelOfResourceCardPile();
                    IntelObservers = IntelOfIntelObservers();

                    //Location
                    IntelLocations.Clear();
                    foreach (var l in h.Game.Map.Locations)
                    {
                        IntelLocations.Add(l, Intel(l));
                    }

                    //Player token
                    IntelPlayers.Clear();
                    IntelTechtokensAndSkills.Clear();
                    if (h.Game.CurrentPhase > Phase.SelectingFactions)
                    {
                        foreach (var player in h.Game.Players)
                        {
                            IntelPlayers.Add(player.PositionAtTable, Intel(player));
                            IntelTechtokensAndSkills.Add(player.PositionAtTable, IntelOfTechtokensAndSkilledLeaders(player));
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Support.Log(e);
            }
        }

        private static string IntelOfIntelObservers()
        {
            string result = "";

            if (h.Host != null && h.Host.JoinedObservers.Any(o => o != "$RonaldAdmin$"))
            {
                result += Skin.Current.Format("<h4>Observers</h4>");
                result += "<div>" + string.Join(", ", h.Host.JoinedObservers.Where(o => o != "$RonaldAdmin$").Distinct()) + "</div>";
            }
            
            return result;
        }

        private static string IntelOfTanks()
        {
            string result = Skin.Current.Format("<h4>{0}</h4>", Concept.Graveyard);

            foreach (var p in h.Game.Players)
            {
                bool showFaceDown = h.Faction == Faction.Purple || h.Faction == p.Faction;

                result += "<div class=\"text-left row m-0 mt-1\">";

                if (p.ForcesKilled > 0)
                {
                    result += string.Format("<div class=\"col p-0\"><img class=\"m-0 p-0\" width=\"40\" height=\"40\" src=\"{0}\"/><strong>&nbsp;{1}</strong></div>",
                        Skin.Current.GetFactionForceImageURL(p.Faction),
                        p.ForcesKilled);
                }

                if (p.SpecialForcesKilled > 0)
                {
                    result += string.Format("<div class=\"col p-0\"><img class=\"m-0 p-0\" width=\"40\" height=\"40\" src=\"{0}\"/><strong>&nbsp;{1}</strong></div>",
                        Skin.Current.GetFactionSpecialForceImageURL(p.Faction),
                        p.SpecialForcesKilled);
                }

                var corpses = p.Leaders
                    .Where(l => !h.Game.IsAlive(l))
                    .Select(l => new Tuple<IHero, LeaderState>(l, h.Game.LeaderState[l]))
                    .OrderBy(l => l.Item2.TimeOfDeath);

                foreach (var hero in corpses)
                {
                    result += string.Format("<div class=\"col p-0\"><img class=\"m-0 p-0\" width=\"80\" height=\"80\" src=\"{0}\" /></div>",
                                !hero.Item2.IsFaceDownDead || showFaceDown ? Skin.Current.GetImageURL(hero.Item1) : Skin.Current.GetFactionFacedownImageURL(hero.Item1.Faction));
                }

                if (p.Faction == Faction.Green && !h.Game.MessiahIsAlive)
                {
                    result += string.Format("<div class=\"col p-0\"><img class=\"m-0 p-0\" width=\"80\" height=\"80\" src=\"{0}\" title=\"{1}\" /></div>",
                                Skin.Current.Messiah_URL,
                                Skin.Current.Describe(Concept.Messiah));
                }

                result += "</div>";
            }

            return result;
        }

        private static string DetermineIntelligence(int x, int y)
        {
            //Tanks
            if (y > 0 && y < 14 * Skin.Current.FORCETOKEN_RADIUS && x > 0 && x < 16 * Skin.Current.FORCETOKEN_RADIUS)
            {
                return IntelTanks;
            }

            //Spice Card Pile
            if (y > Skin.Current.SpiceDeckLocation.Y && y < Skin.Current.SpiceDeckLocation.Y + 400 && x > Skin.Current.SpiceDeckLocation.X && x < Skin.Current.SpiceDeckLocation.X + 1200)
            {
                return IntelResourceCardPile;
            }

            //Treachery Card Pile
            if (y > Skin.Current.TreacheryDeckLocation.Y && y < Skin.Current.TreacheryDeckLocation.Y + 400 && x > Skin.Current.TreacheryDeckLocation.X - 400 && x < Skin.Current.TreacheryDeckLocation.X + 800)
            {
                return IntelTreacheryCardPile;
            }

            //Location
            var l = h.Game.Map.FindLocation(x, y);
            if (l != null && IntelLocations.ContainsKey(l))
            {
                return IntelLocations[l];
            }

            //Player token
            var player = h.Game.Players.FirstOrDefault(p => Near(PlayerTokenPosition(h.Game, p.PositionAtTable), x, y, (int)(3.5f * Skin.Current.PlayerTokenRadius)));
            if (player != null)
            {
                if (Near(PlayerTokenPosition(h.Game, player.PositionAtTable), x, y, Skin.Current.PlayerTokenRadius) && IntelPlayers.ContainsKey(player.PositionAtTable))
                {
                    return IntelPlayers[player.PositionAtTable];
                }
                else if (IntelTechtokensAndSkills.ContainsKey(player.PositionAtTable))
                {
                    return IntelTechtokensAndSkills[player.PositionAtTable];
                }
            }

            //Options
            if (x >= Skin.Current.MapDimensions.X - 150 &&
                    y >= Skin.Current.MapDimensions.Y / 4 - 300 &&
                    y <= Skin.Current.MapDimensions.Y / 4 - 100)
            {
                return IntelOptions;
            }

            //Observers
            if (x >= Skin.Current.PlanetCenter.X - 500 &&
                x <= Skin.Current.PlanetCenter.X + 500 &&
                y >= Skin.Current.MapDimensions.Y - 300)
            {
                return IntelObservers;
            }

            return "";
        }

        private static bool Near(Point p, int x, int y, int distance)
        {
            return Math.Abs(p.X - x) < distance && Math.Abs(p.Y - y) < distance;
        }

        private static string Intel(Location intelligenceLocation)
        {
            string result = "<div style='width:300px'>";

            var owner = h.Game.StrongholdOwnership.ContainsKey(intelligenceLocation) ? h.Game.StrongholdOwnership[intelligenceLocation] : Faction.None;
            if (owner != Faction.None)
            {
                result += Support.GetOwnedStrongholdHTML(intelligenceLocation, owner);
            }
            else
            {
                result += Skin.Current.Format("<h5>{0}</h5>", intelligenceLocation.Territory.Name);
                if (intelligenceLocation.Name != "")
                {
                    result += "<div class='mt-0 mb-2'><strong>Sector: " + intelligenceLocation.Name + "</strong></div>";
                }
            }

            if (Skin.Current.ShowVerboseToolipsOnMap)
            {
                if (intelligenceLocation.Territory.IsStronghold || h.Game.IsSpecialStronghold(intelligenceLocation.Territory))
                {
                    result += Skin.Current.Format("<p>Yields a victory point at end of turn.</p>");
                }

                if (intelligenceLocation == h.Game.Map.Arrakeen || intelligenceLocation == h.Game.Map.Carthag)
                {
                    result += Skin.Current.Format("<p><strong>Harvesters: </strong>when occupied, grants a {0} collection rate of 3-per-force.</p>", Concept.Resource);
                    result += Skin.Current.Format("<p><strong>Ornithopters: </strong>when occupied, your forces can move up to 3 territories.</p>", Concept.Resource);
                }

                if (h.Game.Applicable(Rule.IncreasedResourceFlow))
                {
                    if (intelligenceLocation == h.Game.Map.Arrakeen || intelligenceLocation == h.Game.Map.Carthag || intelligenceLocation == h.Game.Map.TueksSietch)
                    {
                        int bonus = (intelligenceLocation == h.Game.Map.Arrakeen || intelligenceLocation == h.Game.Map.Carthag) ? 2 : 1;
                        result += Skin.Current.Format("<p>Yields <strong>{0} {1}</strong> at end of turn when occupied.</p>", bonus, Concept.Resource);
                    }
                }

                result += Skin.Current.Format("<p>{0} from {1}.</p>", ProtectedFromMonsterText(intelligenceLocation), Concept.Monster);
                result += Skin.Current.Format("<p>{0} from Storms.</p>", ProtectedFromStormText(intelligenceLocation));

                if (intelligenceLocation.Sector == h.Game.SectorInStorm)
                {
                    result += "<p>Currently in Storm.</p>";
                }

                if (intelligenceLocation.SpiceBlowAmount > 0)
                {
                    result += Skin.Current.Format("<p>May contain <strong>{0} {1}</strong> after {2}.</p>", intelligenceLocation.SpiceBlowAmount, Concept.Resource, MainPhase.Blow);
                }

                if (h.Game.ResourcesOnPlanet.ContainsKey(intelligenceLocation))
                {
                    result += Skin.Current.Format("<p>Current amount of {0}: <strong>{1}</strong>.</p>", Concept.Resource, h.Game.ResourcesOnPlanet[intelligenceLocation]);
                }



            }
            result += "<div class=\"row m-0 bg-dark text-center\">";
            if (h.Game.ForcesOnPlanetExcludingEmptyLocations.ContainsKey(intelligenceLocation))
            {
                foreach (var b in h.Game.ForcesOnPlanet[intelligenceLocation])
                {
                    var p = h.Game.GetPlayer(b.Faction);
                    if (b.AmountOfSpecialForces > 0 || b.AmountOfForces > 0)
                    {
                        if (b.AmountOfForces > 0)
                        {
                            result += string.Format("<div class=\"col p-0\"><img class=\"img-fluid m-0 p-0\" width=\"32\" height=\"32\" src=\"{0}\" /><strong>&nbsp;{1}</strong></div>",
                                Skin.Current.GetFactionForceImageURL(p.Faction),
                                b.AmountOfForces);
                        }

                        if (b.AmountOfSpecialForces > 0)
                        {
                            result += string.Format("<div class=\"col p-0\"><img class=\"img-fluid m-0 p-0\" width=\"32\" height=\"32\" src=\"{0}\" /><strong>&nbsp;{1}</strong></div>",
                                Skin.Current.GetFactionSpecialForceImageURL(p.Faction),
                                b.Faction == Faction.White && h.Faction != Faction.White ? "?" : "" + (b.Faction == Faction.White ? h.Game.CurrentNoFieldValue : b.AmountOfSpecialForces));
                        }
                    }
                }
            }

            foreach (var hero in h.Game.LeaderState.Where(state => state.Key is IHero && state.Value.Alive && state.Value.CurrentTerritory == intelligenceLocation.Territory).Select(state => state.Key))
            {
                result += string.Format("<div class=\"col p-0\"><img class=\"img-fluid m-0 p-0\" width=\"64\" height=\"64\" src=\"{0}\" /></div>",
                            Skin.Current.GetImageURL(hero));
            }

            result += "</div>";

            return result;
        }

        private static string Intel(Player p)
        {
            string result = "";
            result += Skin.Current.Format("<h5>{0} plays <span class=\"badge badge-primary badge-pill\" style=\"{1}\">{2}</span></h5>", p.Name, Support.Color(p.Faction), p.Faction);
            foreach (var v in LeaderManager.Leaders.Where(l => l.Faction == p.Faction))
            {
                result += Support.GetLeaderHTML(v);
            }

            result += "<div class=\"row m-0 mt-1 justify-content-center\">";

            result += "<div class=\"col-2 col-lg-1 p-0\">Reserves:</div>";

            result += string.Format("<div class=\"col-2 col-lg-1 p-0\"><img class=\"img-fluid m-0 p-0\" width=\"24\" height=\"24\" src=\"{0}\" title=\"{1}\" /><strong>&nbsp;{2}</strong></div>",
                Skin.Current.GetFactionForceImageURL(p.Faction),
                p.ForceName,
                p.ForcesInReserve);

            if (p.HasSpecialForces)
            {
                result += string.Format("<div class=\"col-2 col-lg-1 p-0\"><img class=\"img-fluid m-0 p-0\" width=\"24\" height=\"24\" src=\"{0}\" title=\"{1}\" /><strong>&nbsp;{2}</strong></div>",
                    Skin.Current.GetFactionSpecialForceImageURL(p.Faction),
                    p.SpecialForceName,
                    p.SpecialForcesInReserve);
            }

            result += "</div>";

            result += "<div class=\"mt-1 text-left\">" + Skin.Current.GetFactionInfo_HTML(h.Game, p.Faction) + "</div>";
            return result;
        }

        private static string IntelOfTechtokensAndSkilledLeaders(Player p)
        {
            string result = "";
            
            if (p.Faction == Faction.White && h.Game.LatestRevealedNoFieldValue >= 0)
            {
                result += string.Format("<div class=\"mt-1\">Latest revealed No-Field value: <strong>{0}</strong></div>", h.Game.LatestRevealedNoFieldValue);
            }

            foreach (var v in p.TechTokens)
            {
                result += Support.GetTechTokenHTML(v);
            }

            var skilledLeader = h.Game.GetSkilledLeader(p);
            if (skilledLeader != null && h.Game.IsInFrontOfShield(skilledLeader))
            {
                result += Support.GetSkilledLeaderHTML(skilledLeader, h.Game.Skill(skilledLeader));
            }

            return result;
        }

        private static string IntelOfTreacheryCardPile()
        {
            string result = "";
            if (h.Game.WhiteCache.Count > 0)
            {
                result += Skin.Current.Format("<h4>{0} Cards ({1} on deck)</h4>", Faction.White, h.Game.WhiteCache.Count);

                if (h.Faction == Faction.White || h.Game.Applicable(Rule.AssistedNotekeeping))
                {
                    result += "<div class=\"row row-cols-6 ml-0 mr-0 mt-1\">";
                    foreach (var c in h.Game.WhiteCache)
                    {
                        result += string.Format("<img src=\"{0}\" class=\"img-fluid\"/>", Skin.Current.GetImageURL(c));
                    }
                    result += "</div>";
                }
            }

            result += Skin.Current.Format("<h4>Treachery Cards ({0} on deck, {1} discarded)</h4>", h.Game.TreacheryDeck.Items.Count, h.Game.TreacheryDiscardPile.Items.Count);

            if (h.Game.Applicable(Rule.AssistedNotekeeping))
            {
                result += "<div class=\"row row-cols-6 ml-0 mr-0 mt-1\">";
                foreach (var c in h.Game.TreacheryDiscardPile.Items)
                {
                    result += string.Format("<img src=\"{0}\" class=\"img-fluid\"/>", Skin.Current.GetImageURL(c));
                }
                result += "</div>";
            }
            else if (h.Game.TreacheryDiscardPile.Top != null)
            {
                result += "Most recently discarded:";
                result += Support.GetTreacheryCardHoverHTML(h.Game.TreacheryDiscardPile.Top);
            }
            

            return result;
        }

        private static string IntelOfResourceCardPile()
        {
            string result = "";

            result += Skin.Current.Format("<h4>{0} Cards</h4>", Concept.Resource);

            if (h.Game.CurrentMainPhase == MainPhase.ShipmentAndMove && h.Game.HasResourceDeckPrescience(h.Player) && h.Game.ResourceCardDeck.Top != null)
            {
                result += "<div class='row'>";
                result += "<div class='col'>";
                result += Skin.Current.Format("<p>{0} on deck.</p>", h.Game.ResourceCardDeck.Items.Count);
                result += "<p>Top Card:</p>";
                result += Support.GetResourceCardHoverHTML(h.Game.ResourceCardDeck.Top);
                result += "</div>";
            }
            else
            {
                result += Skin.Current.Format("<p>{0} on deck.</p>", h.Game.ResourceCardDeck.Items.Count);
                result += "<div class='row'>";
            }

            if (h.Game.Applicable(Rule.IncreasedResourceFlow))
            {
                result += "<div class='col'>";
                result += Skin.Current.Format("<p>{0} on discard pile A</p>", h.Game.ResourceCardDiscardPileA.Items.Count);
                if (h.Game.LatestSpiceCardA != null)
                {
                    result += "<p>Latest card:</p>";
                    result += Support.GetResourceCardHoverHTML(h.Game.LatestSpiceCardA);
                    if (h.Game.Applicable(Rule.AssistedNotekeeping))
                    {
                        foreach (var c in h.Game.ResourceCardDiscardPileA.Items.Where(c => c != h.Game.LatestSpiceCardA))
                        {
                            result += "<p>" + c.ToString() + "</p>";
                        }
                    }
                }
                result += "</div>";

                result += "<div class='col'>";
                result += Skin.Current.Format("<p>{0} on discard pile B</p>", h.Game.ResourceCardDiscardPileB.Items.Count);
                if (h.Game.LatestSpiceCardB != null)
                {
                    result += "<p>Latest card:</p>";
                    result += Support.GetResourceCardHoverHTML(h.Game.LatestSpiceCardB);
                    if (h.Game.Applicable(Rule.AssistedNotekeeping))
                    {
                        foreach (var c in h.Game.ResourceCardDiscardPileB.Items.Where(c => c != h.Game.LatestSpiceCardB))
                        {
                            result += "<p>" + c.ToString() + "</p>";
                        }
                    }
                }
                result += "</div>";
            }
            else
            {
                result += "<div class='col'>";
                result += Skin.Current.Format("<p>{0} on the discard pile.</p>", h.Game.ResourceCardDiscardPileA.Items.Count);
                if (h.Game.LatestSpiceCardA != null)
                {
                    result += "<p>Latest card:</p>";
                    result += Support.GetResourceCardHoverHTML(h.Game.LatestSpiceCardA);
                    if (h.Game.Applicable(Rule.AssistedNotekeeping))
                    {
                        foreach (var c in h.Game.ResourceCardDiscardPileA.Items.Where(c => c != h.Game.LatestSpiceCardA))
                        {
                            result += "<p>" + c.ToString() + "</p>";
                        }
                    }
                }
                result += "</div>";
            }

            result += "</div>";

            return result;
        }

        private static string ProtectedFromMonsterText(Location l)
        {
            return l.Territory.IsProtectedFromWorm ? "Protected" : "NOT protected";
        }

        private static string ProtectedFromStormText(Location l)
        {
            return h.Game.IsProtectedFromStorm(l) ? "Protected" : "NOT protected";
        }

    }
}
