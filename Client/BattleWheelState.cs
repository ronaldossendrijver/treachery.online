/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Client;

/// <summary>
/// Client-side visibility of the floating battle wheel, shared between the wheel itself and the action rail.
/// </summary>
public static class BattleWheelState
{
    private static bool _hidden;

    public static bool Hidden
    {
        get => _hidden;
        set
        {
            if (_hidden == value) return;
            _hidden = value;
            Changed?.Invoke();
        }
    }

    public static event Action? Changed;

    public static readonly Type[] ActionTypes =
    [
        typeof(BattleClaimed), typeof(SwitchedSkilledLeader),
        typeof(Voice), typeof(Prescience), typeof(KarmaPrescience), typeof(ResidualPlayed),
        typeof(Battle), typeof(BattleRevision),
        typeof(TreacheryCalled), typeof(PoisonToothCancelled), typeof(PortableAntidoteUsed), typeof(Diplomacy), typeof(KarmaPinkDial),
        typeof(FaceDancerRevealed), typeof(FaceDanced),
        typeof(RockWasMelted), typeof(Retreat), typeof(Captured), typeof(BattleConcluded), typeof(LoserConcluded),
        typeof(AuditCancelled), typeof(Audited)
    ];

    /// <summary>Decisions taken once the battle is resolved; these appear on the right-hand flap of the wheel.</summary>
    public static readonly HashSet<Type> AfterBattleTypes =
    [
        typeof(Captured), typeof(AuditCancelled), typeof(Audited), typeof(Retreat), typeof(BattleConcluded), typeof(LoserConcluded),
        typeof(FaceDancerRevealed), typeof(FaceDanced)
    ];
}

/// <summary>Identity and state of one floating battle wheel.</summary>
public sealed record BattleWheelFrame(string DragKey, Faction Combatant, bool IsAggressor, string? Role, string Territory, bool IsActive, bool Hidden);