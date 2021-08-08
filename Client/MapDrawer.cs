/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using Treachery.Shared;
using System.Collections.Generic;

namespace Treachery.Client
{
    public static partial class MapDrawer
    {
        private static Canvas2DContext map = null;
        private static BECanvasComponent canvas = null;
        private static Handler h = null;
        private static readonly float[] BORDER_SOLID = Array.Empty<float>();
        private static readonly float[] BORDER_STRIPED = new float[] { 5, 5 };
        private const double TWOPI = 2 * Math.PI;

        private static Dimensions Dimensions { get; set; } = null;

        public static bool ShowWheelsAndHMS { get; set; } = true;

        public static bool Loading { get; set; } = false;

        private static int nrOfDraws;

        [JSInvokable("MapDrawerDraw")]
        public static async Task Draw()
        {
            Support.LogDuration("Draw() " + nrOfDraws++);

            if (canvas == null) return;

            if (h.Game != null && h.Game.CurrentPhase >= Phase.AwaitingPlayers)
            {
                map = await canvas.CreateCanvas2DAsync();
                await map.ClearRectAsync(0, 0, Skin.Current.MapDimensions.X, Skin.Current.MapDimensions.Y);
                await DrawMap();

                if (!Loading && h.Game.CurrentPhase > Phase.AwaitingPlayers)
                {
                    await DrawDestroyedShieldWall();
                    await DrawSSW();
                    await DrawTurn(h.Game.CurrentTurn);
                    await DrawResourcesOnPlanet();
                    await DrawMonsters();
                    await DrawStormAndStormPrescience();
                    await DrawPhases();
                    await DrawHighlightedTerritories();
                    await DrawPlayersAndTechTokensAndLeaderSkills();
                    await DrawHiddenMobileStronghold();
                    await DrawStrongholdOwnership();
                    await DrawRecentMoves();
                    await DrawForcesOnDune();
                    await DrawBids();
                    await DrawTanks();
                    await DrawCardPiles();
                    await DrawBattlePlans();
                    await DrawDisconnected();
                    await DrawOptions();
                }
            }
        }

        public static void SetGame(BECanvasComponent c, Handler handler)
        {
            canvas = c;
            h = handler;
        }

        #region Map
        private static async Task DrawMap()
        {
            if (Loading)
            {
                await DrawImage(Artwork.Map, 0, 0, 1, 1, Skin.Current.SHADOW_LIGHT, 0, 0, 0);
                await DrawText(Skin.Current.MapDimensions.X / 2, Skin.Current.MapDimensions.Y / 2, "Loading...", Skin.Current.PLAYERNAME_FONT, TextAlign.Center, Skin.Current.PLAYERNAME_FONTCOLOR, Skin.Current.PLAYERNAME_FONT_BORDERWIDTH, Skin.Current.PLAYERNAME_FONT_BORDERCOLOR);
                return;
            }
            else
            {
                await DrawImage(Artwork.Map, 0, 0, Skin.Current.MapDimensions.X, Skin.Current.MapDimensions.Y, Skin.Current.SHADOW_LIGHT, 0, 0, 0);
            }
        }
        #endregion

        #region Moves
        private static Dictionary<ArrowId, int> ExistingArrows = new Dictionary<ArrowId, int>();
        private static async Task DrawRecentMoves()
        {
            if (Skin.Current.ShowArrowsForRecentMoves)
            {
                if (h.Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
                {
                    foreach (var move in h.Game.RecentMoves.Where(move => !move.Passed))
                    {
                        var to = move.To.Center;

                        foreach (var from in move.ForceLocations.Keys)
                        {
                            var a = CloserTo(from.Center, to, 0.10f);
                            var b = CloserTo(to, from.Center, 0.15f);

                            var arrowId = new ArrowId() { Faction = move.Initiator, From = from, To = move.To };
                            int arrowNumber = -1;

                            if (!ExistingArrows.ContainsKey(arrowId))
                            {
                                arrowNumber = ExistingArrows.Count;

                                if (arrowNumber < Artwork.Arrows.Length)
                                {
                                    await Browser.CreateArrowImage(ArrowId.CreateArrowId(arrowNumber), Skin.Current.MapDimensions.X / 2 - Skin.Current.PlanetRadius,
                                        Skin.Current.MapDimensions.Y / 2 - Skin.Current.PlanetRadius,
                                        Skin.Current.PlanetRadius,
                                        a,
                                        b,
                                        Skin.Current.FactionColor[move.Initiator],
                                        Skin.Current.FactionColorTransparant[move.Initiator],
                                        0.35f);

                                    ExistingArrows.Add(arrowId, arrowNumber);
                                }
                                else
                                {
                                    arrowNumber = -1;
                                }
                            }
                            else
                            {
                                arrowNumber = ExistingArrows[arrowId];
                            }

                            if (arrowNumber >= 0)
                            {
                                await DrawImageSimple(Artwork.Arrows[arrowNumber].Value, Skin.Current.MapDimensions.X / 2 - Skin.Current.PlanetRadius, Skin.Current.MapDimensions.Y / 2 - Skin.Current.PlanetRadius, 2 * Skin.Current.PlanetRadius, 2 * Skin.Current.PlanetRadius);
                            }
                        }
                    }
                }
            }
        }

        public static Point CloserTo(Point a, Point b, float percentage)
        {
            return new Point()
            {
                X = a.X + (int)(percentage * (b.X - a.X)),
                Y = a.Y + (int)(percentage * (b.Y - a.Y)),
            };
        }

        #endregion

        #region DrawHMS
        private static async Task DrawHiddenMobileStronghold()
        {
            if (h.Game.Map.HiddenMobileStronghold.Visible)
            {
                await DrawImage(
                    Artwork.HiddenMobileStronghold,
                    h.Game.Map.HiddenMobileStronghold.Center.X - HiddenMobileStronghold.RADIUS,
                    h.Game.Map.HiddenMobileStronghold.Center.Y - HiddenMobileStronghold.RADIUS,
                    HiddenMobileStronghold.RADIUS + Math.Abs(HiddenMobileStronghold.DX), 2 * HiddenMobileStronghold.RADIUS, Skin.Current.SHADOW_DARK, 3, 8, 8, ShowWheelsAndHMS ? 1.0f : 0.3f);
            }
        }
        #endregion

        #region Highlights
        private static async Task DrawHighlightedTerritories()
        {
            if (h == null || h.Actions == null) return;

            foreach (var t in h.HighlightedTerritories)
            {
                await DrawSegments(t.Shape, Skin.Current.HIGHLIGHT_OVERLAY_COLOR);
            }
        }

        private static async Task DrawSSW()
        {
            if (h.Game.Applicable(Rule.SSW))
            {
                if (h.Game.IsSpecialStronghold(h.Game.Map.ShieldWall))
                {
                    await DrawSegments(h.Game.Map.ShieldWall.Shape, "#F27F3480");
                }
                else
                {
                    var position = h.Game.Map.ShieldWall.MiddleLocation.Center;

                    for (int i = 0; i < h.Game.NumberOfMonsters; i++)
                    {
                        await DrawImage(Artwork.Monster,
                            i * 0.7f * Skin.Current.MONSTERTOKEN_RADIUS + position.X - 0.35f * Skin.Current.MONSTERTOKEN_RADIUS - 20,
                            position.Y - 50,
                            0.7f * Skin.Current.MONSTERTOKEN_RADIUS,
                            0.7f * Skin.Current.MONSTERTOKEN_RADIUS, "white", 4, 2, 2);
                    }
                }
            }
        }
        #endregion

        #region Battleplans
        private static async Task DrawBattlePlans()
        {
            if (ShowWheelsAndHMS && h.Game.CurrentPhase != Phase.Facedancing && h.Game.CurrentPhase != Phase.BattleReport)
            {
                bool MaySeeAggressorBattlePlan = h.Game.AggressorBattleAction != null && (h.Game.AggressorBattleAction.Initiator == h.Faction || h.Game.GreenKarma && h.Game.CurrentBattle.OpponentOf(Faction.Green) == h.Game.AggressorBattleAction.Player);
                bool MaySeeDefenderBattlePlan = h.Game.DefenderBattleAction != null && (h.Game.DefenderBattleAction.Initiator == h.Faction || h.Game.GreenKarma && h.Game.CurrentBattle.OpponentOf(Faction.Green) == h.Game.DefenderBattleAction.Player);
                bool MaySeeBothBattlePlans = h.Game.AggressorBattleAction != null && h.Game.DefenderBattleAction != null;
                bool AggressorIsAffectedByPartialPrescience = h.Game.CurrentPrescience != null && h.Game.AggressorBattleAction != null && h.Game.CurrentBattle.OpponentOf(Faction.Green) == h.Game.AggressorBattleAction.Player;
                bool DefenderIsAffectedByPartialPrescience = h.Game.CurrentPrescience != null && h.Game.DefenderBattleAction != null && h.Game.CurrentBattle.OpponentOf(Faction.Green) == h.Game.DefenderBattleAction.Player;

                if (MaySeeAggressorBattlePlan || MaySeeBothBattlePlans)
                {
                    //confirmed aggressor wheel
                    await DrawWheel(h.Game.AggressorBattleAction, h.Game.CurrentBattle.Defender, h.Game.DefenderTraitorAction, true, Skin.Current.BattleScreenWidth, Skin.Current.BattleScreenHeight, 500, 200,
                        Skin.Current.BattleScreenHeroX, Skin.Current.BattleScreenHeroY, Skin.Current.BattleWheelForcesY,
                        Skin.Current.BattleWheelForcesX, Skin.Current.BattleWheelCardX, Skin.Current.BattleWheelCardY, Skin.Current.BattleWheelCardWidth, Skin.Current.BattleWheelCardHeight,
                        Skin.Current.BattleWheelHeroWidth, Skin.Current.BattleWheelHeroHeight);
                }
                else if (MaySeeAggressorBattlePlanUnderConstruction)
                {
                    //aggressor wheel under construction
                    await DrawWheel(h._battleUnderConstruction, h.Game.CurrentBattle.Defender, null, true, Skin.Current.BattleScreenWidth, Skin.Current.BattleScreenHeight, 500, 200,
                        Skin.Current.BattleScreenHeroX, Skin.Current.BattleScreenHeroY, Skin.Current.BattleWheelForcesY,
                        Skin.Current.BattleWheelForcesX, Skin.Current.BattleWheelCardX, Skin.Current.BattleWheelCardY, Skin.Current.BattleWheelCardWidth, Skin.Current.BattleWheelCardHeight,
                        Skin.Current.BattleWheelHeroWidth, Skin.Current.BattleWheelHeroHeight);
                }
                else if (AggressorIsAffectedByPartialPrescience)
                {
                    //aggressor prescience
                    await DrawWheel(h.Game.AggressorBattleAction, h.Game.CurrentBattle.Defender, null, true, Skin.Current.BattleScreenWidth, Skin.Current.BattleScreenHeight, 500, 200,
                        Skin.Current.BattleScreenHeroX, Skin.Current.BattleScreenHeroY, Skin.Current.BattleWheelForcesY,
                        Skin.Current.BattleWheelForcesX, Skin.Current.BattleWheelCardX, Skin.Current.BattleWheelCardY, Skin.Current.BattleWheelCardWidth, Skin.Current.BattleWheelCardHeight,
                        Skin.Current.BattleWheelHeroWidth, Skin.Current.BattleWheelHeroHeight, h.Game.CurrentPrescience.Aspect);
                }

                if (MaySeeDefenderBattlePlan || MaySeeBothBattlePlans)
                {
                    //confirmed defender wheel
                    await DrawWheel(h.Game.DefenderBattleAction, h.Game.CurrentBattle.Player, h.Game.AggressorTraitorAction, false, Skin.Current.BattleScreenWidth, Skin.Current.BattleScreenHeight, Skin.Current.MapDimensions.X / 2 - 500, Skin.Current.MapDimensions.Y / 2,
                        Skin.Current.BattleScreenHeroX, Skin.Current.BattleScreenHeroY, Skin.Current.BattleWheelForcesY,
                        Skin.Current.BattleWheelForcesX, Skin.Current.BattleWheelCardX, Skin.Current.BattleWheelCardY, Skin.Current.BattleWheelCardWidth, Skin.Current.BattleWheelCardHeight,
                        Skin.Current.BattleWheelHeroWidth, Skin.Current.BattleWheelHeroHeight);
                }
                else if (MaySeeDefenderBattlePlanUnderConstruction)
                {
                    //defernder wheel under construction
                    await DrawWheel(h._battleUnderConstruction, h.Game.CurrentBattle.Player, null, false, Skin.Current.BattleScreenWidth, Skin.Current.BattleScreenHeight, Skin.Current.MapDimensions.X / 2 - 500, Skin.Current.MapDimensions.Y / 2,
                        Skin.Current.BattleScreenHeroX, Skin.Current.BattleScreenHeroY, Skin.Current.BattleWheelForcesY,
                        Skin.Current.BattleWheelForcesX, Skin.Current.BattleWheelCardX, Skin.Current.BattleWheelCardY, Skin.Current.BattleWheelCardWidth, Skin.Current.BattleWheelCardHeight,
                        Skin.Current.BattleWheelHeroWidth, Skin.Current.BattleWheelHeroHeight);
                }
                else if (DefenderIsAffectedByPartialPrescience)
                {
                    //defender prescience
                    await DrawWheel(h.Game.DefenderBattleAction, h.Game.CurrentBattle.Player, null, false, Skin.Current.BattleScreenWidth, Skin.Current.BattleScreenHeight, Skin.Current.MapDimensions.X / 2 - 500, Skin.Current.MapDimensions.Y / 2,
                        Skin.Current.BattleScreenHeroX, Skin.Current.BattleScreenHeroY, Skin.Current.BattleWheelForcesY,
                        Skin.Current.BattleWheelForcesX, Skin.Current.BattleWheelCardX, Skin.Current.BattleWheelCardY, Skin.Current.BattleWheelCardWidth, Skin.Current.BattleWheelCardHeight,
                        Skin.Current.BattleWheelHeroWidth, Skin.Current.BattleWheelHeroHeight, h.Game.CurrentPrescience.Aspect);
                }
            }
        }

        private static bool MaySeeAggressorBattlePlanUnderConstruction
        {
            get
            {
                return h._battleUnderConstruction != null && h.Game.AggressorBattleAction == null && h.Game.CurrentBattle != null && h.Game.CurrentBattle.Initiator == h.Faction;
            }
        }

        private static bool MaySeeDefenderBattlePlanUnderConstruction
        {
            get
            {
                return h._battleUnderConstruction != null && h.Game.DefenderBattleAction == null && h.Game.CurrentBattle != null && h.Game.CurrentBattle.Target == h.Faction;
            }
        }

        private static async Task DrawWheel(Battle plan, Player opponent, TreacheryCalled t, bool isAggressor, int wheelWidth, int wheelHeight, int leftMargin, int topMargin,
            int leaderLeftMargin, int leaderTopMargin, int forceTopMargin, int forceLeftMargin,
            int cardX, int cardY, int cardWidth, int cardHeight, int leaderSizeWidth, int leaderSizeHeight, PrescienceAspect aspect = PrescienceAspect.None)
        {
            await DrawWheelImage(wheelWidth, wheelHeight, leftMargin, topMargin, plan.Initiator, aspect == PrescienceAspect.None ? 1f : 0.5f);

            if (aspect == PrescienceAspect.None || aspect == PrescienceAspect.Dial) await DrawWheelForces(plan, leftMargin, topMargin, forceTopMargin, forceLeftMargin, isAggressor);
            if (aspect == PrescienceAspect.None) await DrawWheelResourcesPaid(plan, leftMargin, topMargin, forceTopMargin, forceLeftMargin);
            if (aspect == PrescienceAspect.None || aspect == PrescienceAspect.Leader) await DrawWheelLeader(plan, opponent, t, leftMargin, topMargin, leaderLeftMargin, leaderTopMargin, cardWidth, cardHeight, leaderSizeWidth, leaderSizeHeight, aspect == PrescienceAspect.Leader);
            if (aspect == PrescienceAspect.None || aspect == PrescienceAspect.Defense) await DrawWheelDefense(plan, leftMargin, topMargin, cardX, cardY, cardWidth, cardHeight, aspect == PrescienceAspect.Defense);
            if (aspect == PrescienceAspect.None || aspect == PrescienceAspect.Weapon) await DrawWheelWeapon(plan, leftMargin, topMargin, cardX, cardY, cardWidth, cardHeight, aspect == PrescienceAspect.Weapon);
        }

        private static async Task DrawWheelImage(int wheelWidth, int wheelHeight, int leftMargin, int topMargin, Faction f, float alpha = 1f)
        {
            await DrawImage(Artwork.Wheel, leftMargin, topMargin, wheelWidth, wheelHeight, Skin.Current.SHADOW_LIGHT, 5, 10, 10, alpha);
            await DrawImage(Artwork.FactionTableTokens[f].Value, leftMargin, topMargin, wheelWidth, wheelHeight, Skin.Current.SHADOW_LIGHT, 0, 0, 0, alpha * 0.3f);
        }

        private static async Task DrawWheelDefense(Battle b, int leftMargin, int topMargin, int cardX, int cardY, int cardWidth, int cardHeight, bool prescience)
        {
            if (b.Defense != null)
            {
                await DrawImage(Artwork.GetTreacheryCard(b.Defense), leftMargin + cardX + cardWidth + 30, topMargin + cardY, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 1, 5, 5);
            }
            else if (prescience)
            {
                await DrawText(leftMargin + cardX + cardWidth + 30 + cardWidth / 2, topMargin + cardY + 20 + cardHeight / 2, "no defense", Skin.Current.TURN_FONT, TextAlign.Center, "white", 4, "black");
            }
        }

        private static async Task DrawWheelWeapon(Battle b, int leftMargin, int topMargin, int cardX, int cardY, int cardWidth, int cardHeight, bool prescience)
        {
            if (b.Weapon != null)
            {
                await DrawImage(Artwork.GetTreacheryCard(b.Weapon), leftMargin + cardX, topMargin + cardY, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 1, 5, 5);
            }
            else if (prescience)
            {
                await DrawText(leftMargin + cardX + cardWidth / 2, topMargin + cardY + 20 + cardHeight / 2, "no weapon", Skin.Current.TURN_FONT, TextAlign.Center, "white", 4, "black");
            }
        }

        private static async Task DrawWheelForces(Battle b, int leftMargin, int topMargin, int forceTopMargin, int forceLeftMargin, bool aggressor)
        {
            var dial = b.Dial(h.Game, aggressor ? h.Game.CurrentBattle.Target : h.Game.CurrentBattle.Initiator).ToString();

            int forceX = leftMargin + forceLeftMargin;
            int forceY = topMargin + forceTopMargin;

            await DrawText(forceX, forceY, dial, Skin.Current.WHEEL_FONT, TextAlign.Center,
                Skin.Current.WHEEL_FONTCOLOR, Skin.Current.WHEEL_FONT_BORDERWIDTH, aggressor ? Skin.Current.WHEEL_FONT_AGGRESSOR_BORDERCOLOR : Skin.Current.WHEEL_FONT_DEFENDER_BORDERCOLOR);
        }

        private static async Task DrawWheelResourcesPaid(Battle b, int leftMargin, int topMargin, int forceTopMargin, int forceLeftMargin)
        {
            int forceX = leftMargin + forceLeftMargin;
            int forceY = topMargin + forceTopMargin;

            int costsOfBattle = b.Cost(h.Game);
            if (costsOfBattle > 0)
            {
                await DrawImage(Artwork.HarvesterImage, forceX + 250 - Skin.Current.RESOURCETOKEN_RADIUS, forceY + 50 - Skin.Current.RESOURCETOKEN_RADIUS, Skin.Current.RESOURCETOKEN_RADIUS * 2, Skin.Current.RESOURCETOKEN_RADIUS * 2, Skin.Current.SHADOW_LIGHT, 2, 3, 3);
                await DrawText(forceX + 250, forceY + 50 + 25, costsOfBattle.ToString(), Skin.Current.RESOURCETOKEN_FONT, TextAlign.Center, Skin.Current.RESOURCETOKEN_FONTCOLOR, 2, Skin.Current.RESOURCETOKEN_FONT_BORDERCOLOR);
            }
        }

        private static async Task DrawWheelLeader(Battle plan, Player opponent, TreacheryCalled t, int leftMargin, int topMargin, int leaderLeftMargin, int leaderTopMargin, int cardWidth, int cardHeight, int leaderSizeWidth, int leaderSizeHeight, bool prescience)
        {
            if (plan.Hero == null)
            {
                if (prescience)
                {
                    await DrawText(leftMargin + leaderLeftMargin + leaderSizeWidth / 2, topMargin + leaderTopMargin + leaderSizeHeight / 2, "no leader", Skin.Current.CARDPILE_FONT, TextAlign.Center, "white", Skin.Current.WHEEL_FONT_BORDERWIDTH, "black");
                }
            }
            else if (plan.Hero is Leader)
            {
                var skill = h.Game.Skill(plan.Player, plan.Hero);
                if (skill != LeaderSkill.None)
                {
                    int skillLeftMargin = leftMargin + leaderLeftMargin - (int)(0.5f * (cardWidth - leaderSizeWidth));
                    int skillTopMargin = topMargin + leaderTopMargin - cardHeight + 80;

                    await DrawImage(Artwork.GetSkillCard(skill), skillLeftMargin, skillTopMargin, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 1, 5, 5);
                    
                    int bonus = Battle.DetermineSkillBonus(h.Game, plan, out _);
                    if (bonus != 0)
                    {
                        await DrawText(skillLeftMargin + cardWidth - 20, skillTopMargin + 140, "+ " + bonus, Skin.Current.CARDPILE_FONT, TextAlign.Right, "green", Skin.Current.WHEEL_FONT_BORDERWIDTH, "white");
                    }
                    else
                    {
                        int penalty = Battle.DetermineSkillPenalty(h.Game, plan, opponent, out _);
                        if (penalty != 0)
                        {
                            await DrawText(skillLeftMargin + cardWidth - 20, skillTopMargin + 140, "- " + penalty, Skin.Current.CARDPILE_FONT, TextAlign.Right, "red", Skin.Current.WHEEL_FONT_BORDERWIDTH, "white");
                        }
                    }
                }

                //Hero token
                await DrawImage(Artwork.GetLeaderToken(plan.Hero as Leader), leftMargin + leaderLeftMargin, topMargin + leaderTopMargin, leaderSizeWidth, leaderSizeHeight, Skin.Current.SHADOW_DARK, 1, 2, 2);

            }
            else if (plan.Hero is TreacheryCard)
            {
                await DrawImage(Artwork.GetTreacheryCard((plan.Hero as TreacheryCard)), leftMargin + leaderLeftMargin, topMargin + leaderTopMargin, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 1, 5, 5);
            }

            if (t != null && t.TraitorCalled)
            {
                await DrawText(leftMargin + leaderLeftMargin + leaderSizeWidth / 2, topMargin + leaderTopMargin + leaderSizeHeight / 2 + 100, "TRAITOR!", Skin.Current.CARDPILE_FONT, TextAlign.Center, "red", Skin.Current.WHEEL_FONT_BORDERWIDTH, "white");
            }

            if (plan.Initiator == Faction.Green && plan.Messiah)
            {
                await DrawImage(Artwork.Messiah, leftMargin + leaderLeftMargin + leaderSizeWidth / 1.5, topMargin + leaderTopMargin - 50, -1, -1, Skin.Current.SHADOW_LIGHT, 1, 2, 2);
            }
        }
        #endregion

        #region Piles
        private static async Task DrawCardPiles()
        {
            int cardWidth = 371;
            int cardHeight = 515;

            var x = Skin.Current.SpiceDeckLocation.X;
            var y = Skin.Current.SpiceDeckLocation.Y;

            //Temporal code to avoid spice piles to display off screen, in case people use outdated skins

            if (x + 100 + 3 * cardWidth > Skin.Current.MapDimensions.X)
            {
                x = Skin.Dune1979.SpiceDeckLocation.X;
                y = Skin.Dune1979.SpiceDeckLocation.Y;
            }

            //Resource cards

            if (h.Game.CurrentMainPhase == MainPhase.ShipmentAndMove && h.Game.HasResourceDeckPrescience(h.Player) && Artwork.GetResourceCard(h.Game.ResourceCardDeck.Top).Available)
            {
                await DrawImage(Artwork.GetResourceCard(h.Game.ResourceCardDeck.Top).Value, x, y, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 5, 5, 5);
            }
            else
            {
                await DrawImage(Artwork.SpiceCardBack, x, y, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 5, 5, 5);
            }

            await DrawText(x + cardWidth / 2, y + cardHeight / 2 + 20, h.Game.ResourceCardDeck.Items.Count.ToString(), Skin.Current.CARDPILE_FONT, TextAlign.Center, Skin.Current.CARDPILE_FONTCOLOR, Skin.Current.CARDPILE_FONT_BORDERWIDTH, Skin.Current.CARDPILE_FONT_BORDERCOLOR);

            x += cardWidth + 30;
            if (h.Game.LatestSpiceCardA != null && Artwork.GetResourceCard(h.Game.LatestSpiceCardA).Available)
            {
                await DrawImage(Artwork.GetResourceCard(h.Game.LatestSpiceCardA).Value, x, y, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 5, 5, 5);
            }

            if (h.Game.Applicable(Rule.IncreasedResourceFlow))
            {
                //A
                await DrawText(x + cardWidth / 2, y + cardHeight / 2 + 20, "A", Skin.Current.CARDPILE_FONT, TextAlign.Center, Skin.Current.CARDPILE_FONTCOLOR, 3, Skin.Current.CARDPILE_FONT_BORDERCOLOR);

                x += cardWidth + 30;
                if (h.Game.LatestSpiceCardB != null && Artwork.GetResourceCard(h.Game.LatestSpiceCardB).Available)
                {
                    await DrawImage(Artwork.GetResourceCard(h.Game.LatestSpiceCardB).Value, x, y, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 5, 5, 5);
                }

                //B
                await DrawText(x + cardWidth / 2, y + cardHeight / 2 + 20, "B", Skin.Current.CARDPILE_FONT, TextAlign.Center, Skin.Current.CARDPILE_FONTCOLOR, 3, Skin.Current.CARDPILE_FONT_BORDERCOLOR);
            }

            //Treachery cards
            x = Skin.Current.TreacheryDeckLocation.X;

            if (h.Game.TreacheryDeck.Items.Count > 0) {
                await DrawImage(Artwork.TreacheryCardBack, x, Skin.Current.TreacheryDeckLocation.Y, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 5, 5, 5);
            }

            await DrawText(x + cardWidth / 2, Skin.Current.TreacheryDeckLocation.Y + cardHeight / 2 + 20, h.Game.TreacheryDeck.Items.Count.ToString(), Skin.Current.CARDPILE_FONT, TextAlign.Center, Skin.Current.CARDPILE_FONTCOLOR, 3, Skin.Current.CARDPILE_FONT_BORDERCOLOR);

            if (!h.Game.TreacheryDiscardPile.IsEmpty)
            {
                x += cardWidth + 30;
                await DrawImage(Artwork.GetTreacheryCard(h.Game.TreacheryDiscardPile.Top), x, Skin.Current.TreacheryDeckLocation.Y, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 5, 5, 5);
                await DrawText(x + cardWidth / 2, Skin.Current.TreacheryDeckLocation.Y + cardHeight / 2 + 20, h.Game.TreacheryDiscardPile.Items.Count.ToString(), Skin.Current.CARDPILE_FONT, TextAlign.Center, Skin.Current.CARDPILE_FONTCOLOR, 3, Skin.Current.CARDPILE_FONT_BORDERCOLOR);
            }

            if (h.Game.WhiteCache.Count > 0)
            {
                x = Skin.Current.TreacheryDeckLocation.X - cardWidth - 30;
                await DrawImage(Artwork.TreacheryCardBack, x, Skin.Current.TreacheryDeckLocation.Y, cardWidth, cardHeight, Skin.Current.SHADOW_LIGHT, 5, 5, 5);
                await DrawImage(Artwork.FactionTokens[Faction.White].Value, x + cardWidth / 3, Skin.Current.TreacheryDeckLocation.Y + 30, cardWidth / 3, cardWidth / 3, Skin.Current.SHADOW_LIGHT, 1, 2, 2);
                await DrawText(x + cardWidth / 2, Skin.Current.TreacheryDeckLocation.Y + cardHeight / 2 + 20, h.Game.WhiteCache.Count.ToString(), Skin.Current.CARDPILE_FONT, TextAlign.Center, Skin.Current.CARDPILE_FONTCOLOR, 3, Skin.Current.CARDPILE_FONT_BORDERCOLOR);
            }
        }
        #endregion

        #region Spice
        private static async Task DrawResourcesOnPlanet()
        {
            foreach (var l in h.Game.Map.Locations.Where(l => l.SpiceBlowAmount > 0))
            {
                if (h.Game.ResourcesOnPlanet.ContainsKey(l))
                {
                    int amount = h.Game.ResourcesOnPlanet[l];
                    await DrawImage(Artwork.HarvesterImage, l.SpiceLocation.X - Skin.Current.RESOURCETOKEN_RADIUS, l.SpiceLocation.Y - Skin.Current.RESOURCETOKEN_RADIUS, Skin.Current.RESOURCETOKEN_RADIUS * 2, Skin.Current.RESOURCETOKEN_RADIUS * 2, Skin.Current.SHADOW_LIGHT, 2, 3, 3);
                    await DrawText(l.SpiceLocation.X, l.SpiceLocation.Y + 30, amount.ToString(), Skin.Current.RESOURCETOKEN_FONT, TextAlign.Center, Skin.Current.RESOURCETOKEN_FONTCOLOR, Skin.Current.RESOURCETOKEN_FONT_BORDERWIDTH, Skin.Current.RESOURCETOKEN_FONT_BORDERCOLOR);
                }
                else if (Skin.Current.DrawResourceIconsOnMap)
                {
                    await DrawImage(Artwork.ResourceImage, l.SpiceLocation.X - Skin.Current.RESOURCETOKEN_RADIUS, l.SpiceLocation.Y - Skin.Current.RESOURCETOKEN_RADIUS, Skin.Current.RESOURCETOKEN_RADIUS * 2, Skin.Current.RESOURCETOKEN_RADIUS * 2, Skin.Current.SHADOW_LIGHT, 0, 0, 0);
                }
            }
        }
        #endregion

        #region Forces
        private static async Task DrawForcesOnDune()
        {
            var forceLocations = h.Game.ForcesOnPlanetExcludingEmptyLocations;

            foreach (var location in h.Game.Map.Locations)
            {
                int xOffset = 0;
                int yOffset = 0;
                var pos = location.Center;

                //var location = fl.Key;
                if (forceLocations.ContainsKey(location))
                {
                    var battalions = forceLocations[location];
                    int nrOfBattalions = battalions.Count;
                    yOffset = -(nrOfBattalions - 1) * Skin.Current.FORCETOKEN_RADIUS;
                    
                    foreach (var battalion in battalions)
                    {
                        var player = h.Game.GetPlayer(battalion.Faction);
                        var threatened = h.Game.ThreatenedByAllyPresence(player, location.Territory);

                        xOffset = 0;
                        string color = Skin.Current.GetFactionColor(battalion.Faction);
                        if (battalion.AmountOfForces > 0)
                        {
                            await DrawForces(pos.X, yOffset + pos.Y, color, battalion.AmountOfForces, false, battalion.Faction, threatened);
                            xOffset += 2 * Skin.Current.FORCETOKEN_RADIUS;
                        }

                        if (battalion.AmountOfSpecialForces > 0)
                        {
                            await DrawForces(pos.X + xOffset, yOffset + pos.Y, color, battalion.AmountOfSpecialForces, true, battalion.Faction, threatened);
                            xOffset += 2 * Skin.Current.FORCETOKEN_RADIUS;
                        }

                        yOffset += 2 * Skin.Current.FORCETOKEN_RADIUS;
                    }
                }

                if (h.Game.AnyForcesIn(location.Territory) ? forceLocations.ContainsKey(location) : location == location.Territory.MiddleLocation) {

                    xOffset = 0;
                    foreach (var hero in h.Game.LeaderState.Where(state => state.Key is IHero && state.Value.Alive && state.Value.CurrentTerritory == location.Territory).Select(state => state.Key))
                    {
                        if (hero is Leader leader)
                        {
                            await DrawImage(Artwork.GetLeaderToken(leader), pos.X + xOffset - Skin.Current.FORCETOKEN_RADIUS, yOffset + pos.Y - Skin.Current.FORCETOKEN_RADIUS, Skin.Current.FORCETOKEN_RADIUS * 3, Skin.Current.FORCETOKEN_RADIUS * 3, Skin.Current.SHADOW_DARK, 1, 1, 1);
                        }
                        else if (hero is Messiah)
                        {
                            await DrawImage(Artwork.Messiah, pos.X + xOffset - Skin.Current.FORCETOKEN_RADIUS, yOffset + pos.Y - Skin.Current.FORCETOKEN_RADIUS, Skin.Current.FORCETOKEN_RADIUS * 3, Skin.Current.FORCETOKEN_RADIUS * 3, Skin.Current.SHADOW_DARK, 1, 1, 1);
                        }

                        xOffset += 3 * Skin.Current.FORCETOKEN_RADIUS;
                    }
                }
            }
        }

        private static readonly int[] DisplaceX = Displace();

        private static int[] Displace()
        {
            var rnd = new Random();
            int[] result = new int[20];
            for (int i = 0; i < 20; i++)
            {
                result[i] = 20 - rnd.Next(40);
            }
            return result;
        }

        private static async Task DrawForces(Point pos, string color, int AmountOfForces, bool special, Faction faction, bool threatened = false)
        {
            await DrawForces(pos.X, pos.Y, color, AmountOfForces, special, faction, threatened);
        }

        private static async Task DrawForces(int x, int y, string color, int AmountOfForces, bool special, Faction faction, bool threatened = false)
        {
            var img = special ? Artwork.SpecialForceTokens[faction] : Artwork.ForceTokens[faction];
            var fontcolor = threatened ? "red" : Skin.Current.FORCETOKEN_FONTCOLOR;
            var bordercolor = threatened ? "yellow" : Skin.Current.FORCETOKEN_FONT_BORDERCOLOR;

            string numberOfForces = faction == Faction.White && special ? "?" : AmountOfForces.ToString();

            if (img.Available)
            {
                for (int i = 0; i < AmountOfForces; i++)
                {
                    await DrawImageSimple(img.Value, x - i * 3 - Skin.Current.FORCETOKEN_RADIUS, y - i * 5 - Skin.Current.FORCETOKEN_RADIUS, Skin.Current.FORCETOKEN_RADIUS * 2, Skin.Current.FORCETOKEN_RADIUS * 2);
                }
                await DrawText(x + 30, y + 40, numberOfForces, Skin.Current.FORCETOKEN_FONT, TextAlign.Center, fontcolor, Skin.Current.FORCETOKEN_FONT_BORDERWIDTH, bordercolor);
            }
            else
            {
                if (!special)
                {
                    await DrawToken(x, y, Skin.Current.FORCETOKEN_RADIUS, color, Skin.Current.FORCETOKEN_BORDERCOLOR, BORDER_SOLID, Skin.Current.FORCETOKEN_BORDERWIDTH);
                    await DrawText(x, y + 25, numberOfForces, Skin.Current.FORCETOKEN_FONT, TextAlign.Center, fontcolor, Skin.Current.FORCETOKEN_FONT_BORDERWIDTH, bordercolor);
                }
                else
                {
                    if (faction == Faction.Blue)
                    {
                        await DrawToken(x, y, Skin.Current.FORCETOKEN_RADIUS, color, bordercolor, BORDER_STRIPED, Skin.Current.FORCETOKEN_BORDERWIDTH);
                    }
                    else if (faction == Faction.Yellow || faction == Faction.Red)
                    {
                        await DrawToken(x, y, Skin.Current.FORCETOKEN_RADIUS, color, Skin.Current.FORCETOKEN_SPECIAL_BORDERCOLOR, BORDER_SOLID, Skin.Current.FORCETOKEN_SPECIAL_BORDERWIDTH);
                    }

                    await DrawText(x, y + 25, numberOfForces, Skin.Current.FORCETOKEN_FONT, TextAlign.Center, Skin.Current.FORCETOKEN_SPECIAL_FONTCOLOR, Skin.Current.FORCETOKEN_FONT_BORDERWIDTH, bordercolor);
                }
            }
        }
        #endregion

        #region Phases
        private static async Task DrawPhases()
        {
            if (h.Game.CurrentMainPhase >= MainPhase.Setup)
            {
                int posX = Skin.Current.MapDimensions.X;
                int posY = 40;
                foreach (var phase in Enumerations.GetValues<MainPhase>(typeof(MainPhase)).Where(p => p >= MainPhase.Storm && p <= MainPhase.Contemplate))
                {
                    if (phase == h.Game.CurrentMainPhase)
                    {
                        //var text = (h.Game.CurrentMoment == MainPhaseMoment.Start ? "*" : "") + Skin.Current.Describe(phase) + (h.Game.CurrentMoment == MainPhaseMoment.End ? "*" : "") + "(" + h.Game.CurrentMoment + ")";
                        posY += 90;

                        if (h.Game.EconomicsStatus != BrownEconomicsStatus.None && phase == MainPhase.Charity)
                        {
                            var dX = await DrawText(posX, posY, Skin.Current.Describe(phase), Skin.Current.PHASE_ACTIVE_FONT, TextAlign.Right, Skin.Current.PHASE_ACTIVE_FONTCOLOR, Skin.Current.PHASE_ACTIVE_FONT_BORDERWIDTH, Skin.Current.PHASE_FONT_BORDERCOLOR, null, true);
                            await DrawImage(Artwork.FactionTokens[Faction.Brown].Value, posX - dX - 200, posY - 90, 120, 120, Skin.Current.SHADOW_LIGHT, 1, 2, 2);
                            await DrawText(posX - dX - 140, posY, Skin.Current.Describe(h.Game.EconomicsStatus), Skin.Current.SKILL_FONT, TextAlign.Center, "white", 1, "black", 150);
                        }
                        else
                        {
                            await DrawText(posX, posY, Skin.Current.Describe(phase), Skin.Current.PHASE_ACTIVE_FONT, TextAlign.Right, Skin.Current.PHASE_ACTIVE_FONTCOLOR, Skin.Current.PHASE_ACTIVE_FONT_BORDERWIDTH, Skin.Current.PHASE_FONT_BORDERCOLOR);
                        }
                        posY += 40;
                    }
                    else
                    {
                        posY += 60;

                        if (h.Game.EconomicsStatus != BrownEconomicsStatus.None && phase == MainPhase.Charity)
                        {
                            var dX = await DrawText(posX, posY, Skin.Current.Describe(phase), Skin.Current.PHASE_FONT, TextAlign.Right, Skin.Current.PHASE_FONTCOLOR, Skin.Current.PHASE_FONT_BORDERWIDTH, Skin.Current.PHASE_FONT_BORDERCOLOR, null, true);
                            await DrawImage (Artwork.FactionTokens[Faction.Brown].Value, posX - dX - 160, posY - 60, 80, 80, Skin.Current.SHADOW_LIGHT, 1, 2, 2);
                            await DrawText(posX - dX - 120, posY, Skin.Current.Describe(h.Game.EconomicsStatus), Skin.Current.SKILL_FONT, TextAlign.Center, "white", 1, "black", 120);
                        }
                        else
                        {
                            await DrawText(posX, posY, Skin.Current.Describe(phase), Skin.Current.PHASE_FONT, TextAlign.Right, Skin.Current.PHASE_FONTCOLOR, Skin.Current.PHASE_FONT_BORDERWIDTH, Skin.Current.PHASE_FONT_BORDERCOLOR);
                        }

                        posY += 30;
                    }
                }
            }
        }

        private static async Task DrawOptions()
        {
            await DrawImageSimple(ShowWheelsAndHMS ? Artwork.Eye : Artwork.EyeSlash, Skin.Current.MapDimensions.X - 100, Skin.Current.MapDimensions.Y / 4 - 250, 100, 100);
        }

        #endregion

        #region ShieldWall
        private static async Task DrawDestroyedShieldWall()
        {
            if (h.Game.ShieldWallDestroyed)
            {
                await DrawSegments(h.Game.Map.ShieldWall.Shape, Skin.Current.BLOWNSHIELDWALL_OVERLAY_COLOR);
                await DrawSegments(h.Game.Map.Arrakeen.Territory.Shape, Skin.Current.METHEOR_OVERLAY_COLOR);
                await DrawSegments(h.Game.Map.Carthag.Territory.Shape, Skin.Current.METHEOR_OVERLAY_COLOR);
                await DrawSegments(h.Game.Map.ImperialBasin.Shape, Skin.Current.METHEOR_OVERLAY_COLOR);
            }
        }
        #endregion

        #region PreviousSpiceBlow
        private static async Task DrawMonsters()
        {
            foreach (var t in h.Game.Monsters)
            {
                var position = t.MiddleLocation.Center;
                await DrawImage(Artwork.Monster, position.X - Skin.Current.MONSTERTOKEN_RADIUS, position.Y - Skin.Current.MONSTERTOKEN_RADIUS, 2 * Skin.Current.MONSTERTOKEN_RADIUS, 2 * Skin.Current.MONSTERTOKEN_RADIUS, Skin.Current.SHADOW_DARK, 3, 5, 5);
            }
        }
        #endregion

        #region PlayerTokens

        private static Point PlayerTokenPosition(Game game, int positionAtTable)
        {
            var positionSector = Math.Floor((float)positionAtTable * Map.NUMBER_OF_SECTORS / game.MaximumNumberOfPlayers);
            double angle = (4.5 - positionSector) * TWOPI / Map.NUMBER_OF_SECTORS;
            var x = Skin.Current.PlanetCenter.X + (int)(Skin.Current.MapRadius * Math.Cos(angle));
            var y = Skin.Current.PlanetCenter.Y + (int)(Skin.Current.MapRadius * Math.Sin(angle));
            return new Point(x, y);
        }

        private static async Task DrawPlayersAndTechTokensAndLeaderSkills()
        {
            if (h.Game.CurrentPhase > Phase.SelectingFactions)
            {
                var techtokenOrbitRadius = Skin.Current.MapRadius + 30;

                foreach (var p in h.Game.Players)
                {
                    double radiusModifier = 1;
                    var position = PlayerTokenPosition(h.Game, p.PositionAtTable);
                    if (h.HighlightPlayer(p))
                    {
                        radiusModifier = 1.6;
                    }
                    var x = position.X - radiusModifier * Skin.Current.PlayerTokenRadius;
                    var y = position.Y - radiusModifier * Skin.Current.PlayerTokenRadius;

                    await DrawCircle(position.X, position.Y, (int)(1.5 * Skin.Current.PlayerTokenRadius), Skin.Current.TABLEPOSITION_BACKGROUNDCOLOR);
                    await DrawImage(Artwork.FactionTableTokens[p.Faction].Value, x, y, radiusModifier * Skin.Current.PlayerTokenRadius * 2, radiusModifier * Skin.Current.PlayerTokenRadius * 2, Skin.Current.SHADOW_LIGHT, 3, 3, 3);

                    if (p.Ally != Faction.None)
                    {
                        await DrawImage(Artwork.FactionTableTokens[p.Ally].Value, position.X, position.Y + 20, 1.2 * Skin.Current.PlayerTokenRadius, 1.2 * Skin.Current.PlayerTokenRadius, Skin.Current.SHADOW_LIGHT, 1, 1, 1);
                    }

                    var positionSector = Math.Floor((float)p.PositionAtTable * Map.NUMBER_OF_SECTORS / h.Game.MaximumNumberOfPlayers);
                    double ttRad = (TWOPI / 180) + 0.02 + (4.5 - positionSector) * TWOPI / Map.NUMBER_OF_SECTORS;

                    foreach (var tt in p.TechTokens)
                    {
                        ttRad += (TWOPI / 90);
                        await DrawImage(Artwork.TechTokens[tt].Value, Skin.Current.PlanetCenter.X + techtokenOrbitRadius * Math.Cos(ttRad) - Skin.Current.PlayerTokenRadius, Skin.Current.PlanetCenter.Y + techtokenOrbitRadius * Math.Sin(ttRad) - Skin.Current.PlayerTokenRadius, 2 * Skin.Current.PlayerTokenRadius, 2 * Skin.Current.PlayerTokenRadius, Skin.Current.SHADOW_DARK, 1, 1, 1);
                    }

                    var skilledLeader = h.Game.GetSkilledLeader(p);
                    if (skilledLeader != null && h.Game.IsInFrontOfShield(skilledLeader))
                    {
                        ttRad += (TWOPI / 90);
                        var ttx = Skin.Current.PlanetCenter.X + techtokenOrbitRadius * Math.Cos(ttRad) - Skin.Current.PlayerTokenRadius;
                        var tty = Skin.Current.PlanetCenter.Y + techtokenOrbitRadius * Math.Sin(ttRad) - Skin.Current.PlayerTokenRadius;
                        await DrawImage(Artwork.GetLeaderToken(skilledLeader), ttx, tty, 2 * Skin.Current.PlayerTokenRadius, 2 * Skin.Current.PlayerTokenRadius, Skin.Current.SHADOW_DARK, 1, 1, 1);
                        await DrawText(ttx + Skin.Current.PlayerTokenRadius, tty + 2 * Skin.Current.PlayerTokenRadius, Skin.Current.Describe(h.Game.Skill(skilledLeader)), Skin.Current.SKILL_FONT, TextAlign.Center, Skin.Current.SKILL_FONTCOLOR, Skin.Current.SKILL_FONT_BORDERWIDTH, Skin.Current.SKILL_FONT_BORDERCOLOR, 2.2* Skin.Current.PlayerTokenRadius);
                    }

                    if (p.Faction == Faction.White && h.Game.LatestRevealedNoFieldValue >= 0)
                    {
                        ttRad += (TWOPI / 90);
                        var ttx = Skin.Current.PlanetCenter.X + techtokenOrbitRadius * Math.Cos(ttRad) - Skin.Current.PlayerTokenRadius;
                        var tty = Skin.Current.PlanetCenter.Y + techtokenOrbitRadius * Math.Sin(ttRad) - Skin.Current.PlayerTokenRadius;
                        await DrawImage(Artwork.SpecialForceTokens[Faction.White].Value, ttx + Skin.Current.FORCETOKEN_RADIUS, tty + Skin.Current.FORCETOKEN_RADIUS, 2 * Skin.Current.FORCETOKEN_RADIUS, 2 * Skin.Current.FORCETOKEN_RADIUS, Skin.Current.SHADOW_DARK, 1, 1, 1);
                        await DrawText(ttx + 2 * Skin.Current.FORCETOKEN_RADIUS, tty + 2.5f * Skin.Current.FORCETOKEN_RADIUS, h.Game.LatestRevealedNoFieldValue.ToString(), Skin.Current.PLAYERNAME_FONT, TextAlign.Center, Skin.Current.PLAYERNAME_FONTCOLOR, Skin.Current.PLAYERNAME_FONT_BORDERWIDTH, Skin.Current.PLAYERNAME_FONT_BORDERCOLOR);
                    }

                    var align = TextAlign.Center;
                    int textPositionX = position.X;
                    if (position.X < 200)
                    {
                        align = TextAlign.Left;
                        textPositionX = (int)(position.X - 1.6 * Skin.Current.PlayerTokenRadius);
                    }
                    else if (position.X > Skin.Current.MapDimensions.X - 200)
                    {
                        align = TextAlign.Right;
                        textPositionX = (int)(position.X + 1.6 * Skin.Current.PlayerTokenRadius);
                    }

                    await DrawText(textPositionX, y + 40, p.Name, Skin.Current.PLAYERNAME_FONT, align, Skin.Current.PLAYERNAME_FONTCOLOR, Skin.Current.PLAYERNAME_FONT_BORDERWIDTH, Skin.Current.PLAYERNAME_FONT_BORDERCOLOR, 6 * Skin.Current.PlayerTokenRadius);
                }
            }
        }

        private static async Task DrawStrongholdOwnership()
        {
            foreach (var stronghold in h.Game.Map.Strongholds)
            {
                if (h.Game.StrongholdOwnership.ContainsKey(stronghold))
                {
                    var owner = h.Game.StrongholdOwnership[stronghold];
                    await DrawImage(Artwork.FactionTableTokens[owner].Value, stronghold.Center.X - 180, stronghold.Center.Y - 50, Skin.Current.PlayerTokenRadius, Skin.Current.PlayerTokenRadius, Skin.Current.SHADOW_LIGHT, 1, 1, 1, 0.8f);
                }
            }
        }
        #endregion

        #region Bids

        private static async Task DrawBids()
        {
            if (h.Game.CurrentPhase == Phase.Bidding || h.Game.CurrentPhase == Phase.BlackMarketBidding)
            {
                foreach (var p in h.Game.Players)
                {
                    var position = PlayerTokenPosition(h.Game, p.PositionAtTable);
                    if (!h.HighlightPlayer(p))
                    {
                        var y = position.Y + Skin.Current.PlayerTokenRadius;
                        string bidText = "";
                        string bidColor = "white";
                        if (!p.HasRoomForCards)
                        {
                            bidText = "FULL";
                            bidColor = "rgb(200,200,200)";
                        }
                        else if (h.Game.Bids.ContainsKey(p.Faction))
                        {
                            var bid = h.Game.Bids[p.Faction];
                            bidText = DetermineBidText(bid);
                            bidColor = bid.Passed ? "rgb(255,200,200)" : "rgb(200,255,200)";
                        }

                        await DrawText(position.X, y, bidText, Skin.Current.TURN_FONT, TextAlign.Center, bidColor, 3, "black");
                    }
                }
            }
        }

        private static string DetermineBidText(IBid bid)
        {
            if (h.Game.CurrentAuctionType != AuctionType.BlackMarketSilent && h.Game.CurrentAuctionType != AuctionType.WhiteSilent)
            {
                return bid.Passed ? "PASS" : string.Format("BID {0}", bid.TotalAmount);
            }
            else
            {
                return "READY";
            }
        }

        #endregion

        #region Tanks
        private static async Task DrawTanks()
        {
            if (h.Game.CurrentPhase >= Phase.MetheorAndStormSpell)
            {
                float y = 2 * Skin.Current.FORCETOKEN_RADIUS;
                float x = Skin.Current.FORCETOKEN_RADIUS;
                float leaderWidth = 3.8f * Skin.Current.FORCETOKEN_RADIUS;
                float leaderHeight = leaderWidth * Skin.Current.BattleWheelHeroHeight / Skin.Current.BattleWheelHeroWidth;
                float leaderPctHeight = .15f * leaderWidth * Skin.Current.BattleWheelHeroHeight / Skin.Current.BattleWheelHeroWidth;
                float spacing = 0.8f;

                foreach (var p in h.Game.Players)
                {
                    string color = Skin.Current.GetFactionColor(p.Faction);

                    if (p.ForcesKilled > 0 || p.SpecialForcesKilled > 0)
                    {
                        if (p.ForcesKilled > 0)
                        {
                            var l = new Point((int)x + Skin.Current.FORCETOKEN_RADIUS, (int)y + Skin.Current.FORCETOKEN_RADIUS);
                            await DrawForces(l, color, p.ForcesKilled, false, p.Faction);
                        }

                        if (p.SpecialForcesKilled > 0)
                        {
                            var dX = (p.ForcesKilled > 0) ? 2 * Skin.Current.FORCETOKEN_RADIUS : 0;
                            var l = new Point((int)x + Skin.Current.FORCETOKEN_RADIUS + dX, (int)y + Skin.Current.FORCETOKEN_RADIUS);
                            await DrawForces(l, color, p.SpecialForcesKilled, true, p.Faction);
                        }

                        y += spacing * 2 * Skin.Current.FORCETOKEN_RADIUS;
                    }

                    var corpses = p.Leaders
                        .Where(l => !h.Game.IsAlive(l))
                        .Select(l => new Tuple<Leader, LeaderState>(l, h.Game.LeaderState[l]))
                        .OrderBy(l => l.Item2.TimeOfDeath).Select(l => l.Item1);

                    int corpseNr = 0;
                    if (corpses.Any())
                    {
                        y += spacing * (leaderPctHeight * (corpses.Count() - 1));
                        int dy = 0;
                        foreach (var leader in corpses)
                        {
                            var l = new Point((int)x, (int)y - dy);

                            if (h.Game.LeaderState[leader].IsFaceDownDead)
                            {
                                await DrawImage(Artwork.FactionFacedownTokens[p.Faction].Value, l.X + DisplaceX[corpseNr] * 2, l.Y, leaderWidth, leaderHeight, Skin.Current.SHADOW_LIGHT, 2, 3, 3);
                            }
                            else
                            {
                                await DrawImage(Artwork.GetLeaderToken(leader), l.X + DisplaceX[corpseNr] * 2, l.Y, leaderWidth, leaderHeight, Skin.Current.SHADOW_LIGHT, 2, 3, 3);
                            }

                            dy += (int)leaderPctHeight;
                            corpseNr++;
                        }

                        y += spacing * leaderHeight;
                    }

                    if (p.Faction == Faction.Green && !h.Game.MessiahIsAlive)
                    {
                        var l = new Point((int)x, (int)y);
                        await DrawImage(Artwork.Messiah, l.X, l.Y, 0.5f * leaderWidth, 0.5f * leaderHeight, Skin.Current.SHADOW_LIGHT, 2, 3, 3);
                        y += spacing * leaderHeight;
                    }

                    NextColumnIfNecessary(ref y, ref x, spacing);
                }
            }
        }

        private static void NextColumnIfNecessary(ref float y, ref float x, float spacing)
        {
            if (y > 14 * Skin.Current.FORCETOKEN_RADIUS - 0.5f * x)
            {
                y = 2 * Skin.Current.FORCETOKEN_RADIUS;
                x += 4 * Skin.Current.FORCETOKEN_RADIUS;
            }
        }
        #endregion

        #region Storm
        private static async Task DrawStormAndStormPrescience()
        {
            int sector = h.Game.SectorInStorm;
            await DrawStorm(sector, Skin.Current.STORM_OVERLAY_COLOR);

            if (h.Game.HasStormPrescience(h.Player))
            {
                for (int i = 1; i <= h.Game.NextStormMoves; i++)
                {
                    await DrawStorm((sector + i) % Map.NUMBER_OF_SECTORS, Skin.Current.STORM_PRESCIENCE_OVERLAY_COLOR);
                }
            }

        }

        private static async Task DrawStorm(int sector, string style)
        {
            if (sector >= 0)
            {
                foreach (var l in h.Game.Map.Locations.Where(l => l.Sector == sector))
                {
                    await DrawLocation(l, style);
                }
            }
        }

        #endregion

        #region Turn
        private static async Task DrawTurn(int turn)
        {
            if (turn >= 1)
            {
                await DrawText(Skin.Current.PlanetCenter.X, 100, string.Format("Turn {0} of {1}", h.Game.CurrentTurn, h.Game.MaximumNumberOfTurns), Skin.Current.TURN_FONT, TextAlign.Center, Skin.Current.TURN_FONT_COLOR, Skin.Current.TURN_FONT_BORDERWIDTH, Skin.Current.TURN_FONT_BORDERCOLOR);
            }

            if (h.Host != null && h.Host.JoinedObservers.Count(o => o != "$RonaldAdmin$") > 0)
            {
                await DrawText(Skin.Current.PlanetCenter.X, Skin.Current.MapDimensions.Y - 60, string.Format("observers: {0}" , h.Host.JoinedObservers.Count(o => o != "$RonaldAdmin$")), Skin.Current.FORCETOKEN_FONT, TextAlign.Center, Skin.Current.TURN_FONT_COLOR, Skin.Current.TURN_FONT_BORDERWIDTH, Skin.Current.TURN_FONT_BORDERCOLOR);
            }
        }

        private static async Task DrawDisconnected()
        {
            if (h.IsDisconnected)
            {
                await DrawText(Skin.Current.PlanetCenter.X, Skin.Current.PlanetCenter.Y, "You were disconnected from the game.", Skin.Current.TURN_FONT, TextAlign.Center, "darkred", 3, "white");

                string message;
                if ((DateTime.Now - h.Disconnected).TotalSeconds < 60)
                {
                    message = "Please wait while the game is trying to reconnect...";
                }
                else
                {
                    message = "Trying to reconnect, but the host may have ended or restarted the game or your internet connection may be broken...";
                }

                await DrawText(Skin.Current.PlanetCenter.X, Skin.Current.PlanetCenter.Y + 150, message, Skin.Current.PLAYERNAME_FONT, TextAlign.Center, "darkred", 2, "white");
            }
        }
        #endregion

        #region arrows



        #endregion


        #region DrawingMethods
        private static async Task DrawLocation(Location loc, string style)
        {
            await map.SaveAsync();

            double normalizedSector1 = 6 - loc.Sector;
            double normalizedSector2 = 5 - loc.Sector;
            double angle1 = normalizedSector1 * 2 * Math.PI / Map.NUMBER_OF_SECTORS;
            double angle2 = normalizedSector2 * 2 * Math.PI / Map.NUMBER_OF_SECTORS;

            await map.BeginPathAsync();
            await map.MoveToAsync(Skin.Current.PlanetCenter.X, Skin.Current.PlanetCenter.Y);
            await map.ArcAsync(Skin.Current.PlanetCenter.X, Skin.Current.PlanetCenter.Y, Skin.Current.PlanetRadius, angle1, angle2, true);
            await map.LineToAsync(Skin.Current.PlanetCenter.X, Skin.Current.PlanetCenter.Y);
            await map.ClosePathAsync();
            await map.ClipAsync();

            await map.SetFillStyleAsync(style);
            await DrawPath(loc.Territory.Shape);
            await map.FillAsync();

            await map.RestoreAsync();
        }

        private static async Task DrawImageSimple(ElementReference img, double x, double y, double width, double height)
        {
            await map.DrawImageAsync(img, x, y, width, height);
        }

        private static async Task DrawImage(ElementReference img, double x, double y, double width, double height,
            string shadowColor, int blur, int offsetX, int offsetY, float alpha = 1.0f)
        {
            await map.SaveAsync();

            await SetShadow(shadowColor, blur, offsetX, offsetY);
            await SetAlpha(alpha);

            try
            {
                if (width == -1 || height == -1)
                {
                    await map.DrawImageAsync(img, (int)x, (int)y);
                }
                else
                {
                    await map.DrawImageAsync(img, (int)x, (int)y, (int)width, (int)height);
                }
            }
            catch (Exception ex)
            {
                Support.Log(ex.Message);
            }

            await map.RestoreAsync();
        }

        private static async Task<double> DrawText(double x, double y, string text,
            string font, TextAlign alignment, string color, int bordersize, string bordercolor, double? maxWidth = null, bool measureText = false)
        {
            double result = 0;

            await map.SaveAsync();
            await SetShadow("rgb(0,0,0,0)", 0, 0, 0);
            await map.SetFontAsync(font);
            await map.SetTextAlignAsync(alignment);
            await map.SetFillStyleAsync(color);
            await map.FillTextAsync(text, x, y, maxWidth);

            if (bordersize > 0)
            {
                await map.SetStrokeStyleAsync(bordercolor);
                await map.SetLineWidthAsync(bordersize);
                await map.StrokeTextAsync(text, x, y, maxWidth);
            }

            if (measureText)
            {
                var measure = await map.MeasureTextAsync(text);
                result = measure.Width;
            }

            await map.RestoreAsync();

            return result;
        }

        private static async Task DrawSegments(Segment[] shape, string fillStyle)
        {
            await map.SaveAsync();
            await map.SetFillStyleAsync(fillStyle);
            await DrawPath(shape);
            await map.FillAsync();
            await map.RestoreAsync();
        }

        private static async Task DrawToken(int x, int y, int radius, string fillStyle, string borderStyle, float[] borderDash, int borderThickness)
        {
            await map.SaveAsync();

            await SetShadow(Skin.Current.SHADOW_LIGHT, 5, 3, 3);
            await map.SetFillStyleAsync(fillStyle);
            await map.BeginPathAsync();
            await map.ArcAsync(x, y, radius, 0, TWOPI, false);
            await map.FillAsync();

            if (borderThickness > 0)
            {
                await map.SetStrokeStyleAsync(borderStyle);
                await map.SetLineDashAsync(borderDash);
                await map.SetLineWidthAsync(borderThickness);
                await map.BeginPathAsync();
                await map.ArcAsync(x, y, radius, 0, TWOPI, false);
                await map.StrokeAsync();
            }

            await map.RestoreAsync();
        }

        private static async Task DrawRectangle(int x, int y, int width, int height, string fillStyle, string borderStyle, float[] borderDash, int borderThickness)
        {
            await map.SaveAsync();

            await map.SetFillStyleAsync(fillStyle);
            await map.FillRectAsync(x, y, width, height);

            if (borderThickness > 0)
            {
                await map.SetStrokeStyleAsync(borderStyle);
                await map.SetLineDashAsync(borderDash);
                await map.SetLineWidthAsync(borderThickness);
                await map.StrokeRectAsync(x, y, width, height);
            }

            await map.RestoreAsync();
        }

        // Usage: 
        //drawLineWithArrows(50,50,150,50,5,8,true,true);

        // x0,y0: the line's starting point
        // x1,y1: the line's ending point
        // width: the distance the arrowhead perpendicularly extends away from the line
        // height: the distance the arrowhead extends backward from the endpoint
        // arrowStart: true/false directing to draw arrowhead at the line's starting point
        // arrowEnd: true/false directing to draw arrowhead at the line's ending point
        private static async Task DrawLineWithArrows(
            int x0, int y0, int x1, int y1, int aWidth, int aLength, bool arrowStart, bool arrowEnd, 
            string style, float[] dash, int thickness)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;
            var angle = (float)Math.Atan2(dy, dx);
            var length = Math.Sqrt(dx * dx + dy * dy);

            await map.SaveAsync();

            await map.SetStrokeStyleAsync(style);
            await map.SetLineDashAsync(dash);
            await map.SetLineWidthAsync(thickness);

            await map.TranslateAsync(x0, y0);
            await map.RotateAsync(angle);
            await map.BeginPathAsync();
            await map.MoveToAsync(0, 0);
            await map.LineToAsync(length, 0);

            if (arrowStart)
            {
                await map.MoveToAsync(aLength, -aWidth);
                await map.LineToAsync(0, 0);
                await map.LineToAsync(aLength, aWidth);
            }

            if (arrowEnd)
            {
                await map.MoveToAsync(length - aLength, -aWidth);
                await map.LineToAsync(length, 0);
                await map.LineToAsync(length - aLength, aWidth);
            }
            
            await map.StrokeAsync();
            await map.SetTransformAsync(1, 0, 0, 1, 0, 0);

            await map.RestoreAsync();
        }

        private static async Task DrawCircle(int x, int y, int radius, string fillStyle)
        {
            await map.SaveAsync();
            await map.SetFillStyleAsync(fillStyle);
            await map.BeginPathAsync();
            await map.ArcAsync(x, y, radius, 0, TWOPI, false);
            await map.FillAsync();
            await map.RestoreAsync();
        }

        private static async Task SetShadow(string color, float blur, float offsetX, float offsetY)
        {
            await map.SetShadowColorAsync(color);
            await map.SetShadowBlurAsync(blur);
            await map.SetShadowOffsetXAsync(offsetX);
            await map.SetShadowOffsetYAsync(offsetY);
        }

        private static async Task SetAlpha(float alpha)
        {
            await map.SetGlobalAlphaAsync(alpha);
        }

        public static async Task DrawPath(Segment[] path)
        {
            if (path.Length > 0)
            {
                await map.BeginPathAsync();

                for (int i = 0; i < path.Length; i++)
                {
                    if (path[i] is LineTo lineto)
                    {
                        await map.LineToAsync(lineto.End.X, lineto.End.Y);
                    }
                    else if (path[i] is BezierTo bezierto)
                    {
                        await map.BezierCurveToAsync(
                            bezierto.Control1.X, bezierto.Control1.Y,
                            bezierto.Control2.X, bezierto.Control2.Y,
                            bezierto.End.X, bezierto.End.Y);
                    }
                    else if (path[i] is MoveTo moveto)
                    {
                        await map.MoveToAsync(moveto.Start.X, moveto.Start.Y);
                    }
                    else if (path[i] is Close)
                    {
                        await map.ClosePathAsync();
                    }
                    else if (path[i] is Arc arc)
                    {
                        await map.ArcAsync(arc.Start.X, arc.Start.Y, arc.Radius, arc.StartAngle, arc.EndAngle, arc.CounterClockwise);
                    }
                }
            }
        }

        public static async Task DrawDots(Segment[] path)
        {
            if (path.Length > 0)
            {
                await map.BeginPathAsync();

                for (int i = 0; i < path.Length; i++)
                {
                    await DrawText(path[i].Start.X, path[i].Start.Y, path[i].GetType().Name + ":" + i, "normal normal normal 30px sans-serif", TextAlign.Center, "black", 2, "white");
                }
            }
        }
        #endregion

        #region Map

        public static event EventHandler<Location> OnLocationSelected;
        public static event EventHandler<Location> OnLocationSelectedWithCtrlOrAlt;
        public static event EventHandler<Location> OnLocationSelectedWithShift;
        public static event EventHandler<Location> OnLocationSelectedWithShiftAndWithCtrlOrAlt;

        public static async Task MapClick(MouseEventArgs e)
        {
            Dimensions = await Browser.GetMapDimensions();

            if (Dimensions != null)
            {
                int x = Dimensions.TranslateClientXToRelativeX(e.ClientX);
                int y = Dimensions.TranslateClientYToRelativeY(e.ClientY);

                if (x >= Skin.Current.MapDimensions.X - 150 &&
                    y >= Skin.Current.MapDimensions.Y / 4 - 300 &&
                    y <= Skin.Current.MapDimensions.Y / 4 - 100)
                {
                    ShowWheelsAndHMS = !ShowWheelsAndHMS;
                    await Draw();
                }
                else
                {
                    var location = h.Game.Map.FindLocation(x, y);

                    if (location != null)
                    {
                        if (e.ShiftKey)
                        {
                            if (e.CtrlKey || e.AltKey)
                            {
                                OnLocationSelectedWithShiftAndWithCtrlOrAlt?.Invoke(h, location);
                            }
                            else
                            {
                                OnLocationSelectedWithShift?.Invoke(h, location);
                            }
                        }
                        else
                        {
                            if (e.CtrlKey || e.AltKey)
                            {
                                OnLocationSelectedWithCtrlOrAlt?.Invoke(h, location);
                            }
                            else
                            {
                                OnLocationSelected?.Invoke(h, location);
                            }
                        }
                    }
                }
            }
        }

        #endregion Map
    }

    public class ArrowId
    {
        public Location From;
        public Location To;
        public Faction Faction;

        public override bool Equals(object obj)
        {
            return obj is ArrowId a && Faction == a.Faction && From.Equals(a.From) && To.Equals(a.To);
        }

        public override int GetHashCode()
        {
            return (Faction.GetHashCode() + From.GetHashCode() + To.GetHashCode()) % int.MaxValue;
        }
        public static string CreateArrowId(int i)
        {
            return "arrow" + i;
        }
    }
}
