/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Shared;

namespace Treachery.Shared.Test;

[TestClass]
public class ManagedGameTests
{
    [TestMethod]
    public async Task ProcessEventAsyncSerializesConcurrentEvents()
    {
        var managedGame = new ManagedGame();
        using var firstEventEntered = new ManualResetEventSlim();
        using var releaseFirstEvent = new ManualResetEventSlim();
        using var secondEventEntered = new ManualResetEventSlim();
        var activeEvents = 0;
        var maximumActiveEvents = 0;

        var firstTask = managedGame.ProcessEventAsync(async () =>
        {
            var active = Interlocked.Increment(ref activeEvents);
            InterlockedExtensions.Max(ref maximumActiveEvents, active);
            firstEventEntered.Set();
            await Task.Run(releaseFirstEvent.Wait);
            Interlocked.Decrement(ref activeEvents);
            return true;
        });
        Assert.IsTrue(firstEventEntered.Wait(1000));

        var secondTask = managedGame.ProcessEventAsync(() =>
        {
            var active = Interlocked.Increment(ref activeEvents);
            InterlockedExtensions.Max(ref maximumActiveEvents, active);
            secondEventEntered.Set();
            Interlocked.Decrement(ref activeEvents);
            return Task.FromResult(true);
        });
        Assert.IsFalse(secondEventEntered.Wait(100));

        releaseFirstEvent.Set();
        await Task.WhenAll(firstTask, secondTask);

        Assert.AreEqual(1, maximumActiveEvents);
        Assert.IsTrue(secondEventEntered.IsSet);
    }

    [TestMethod]
    public async Task ProcessEventAsyncReleasesSemaphoreAfterFailure()
    {
        var managedGame = new ManagedGame();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => managedGame.ProcessEventAsync<bool>(
                () => throw new InvalidOperationException()));

        var result = await managedGame.ProcessEventAsync(
            () => Task.FromResult(true));

        Assert.IsTrue(result);
    }

    private static class InterlockedExtensions
    {
        public static void Max(ref int location, int value)
        {
            var current = Volatile.Read(ref location);
            while (current < value)
            {
                var previous = Interlocked.CompareExchange(ref location, value, current);
                if (previous == current)
                    return;

                current = previous;
            }
        }
    }
}
