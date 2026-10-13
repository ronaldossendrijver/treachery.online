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
/// Lets the battle plan that is currently being edited receive leaders and cards clicked in the player panel.
/// </summary>
public static class BattlePlanPicker
{
    public interface ITarget
    {
        bool IsPickable(object item);

        bool IsPicked(object item);

        void Pick(object item);
    }

    private static ITarget? _target;
    private static string _signature = "";

    public static event Action? Changed;

    public static bool Active => _target != null;

    public static bool IsPickable(object item) => _target?.IsPickable(item) ?? false;

    public static bool IsPicked(object item) => _target?.IsPicked(item) ?? false;

    public static void Pick(object item)
    {
        if (_target != null && _target.IsPickable(item)) _target.Pick(item);
    }

    /// <summary>Registers (or refreshes) the active target; listeners are only notified when what is pickable or picked changes.</summary>
    public static void Register(ITarget target, string signature)
    {
        if (ReferenceEquals(_target, target) && _signature == signature) return;
        _target = target;
        _signature = signature;
        Changed?.Invoke();
    }

    public static void Unregister(ITarget target)
    {
        if (!ReferenceEquals(_target, target)) return;
        _target = null;
        _signature = "";
        Changed?.Invoke();
    }
}
