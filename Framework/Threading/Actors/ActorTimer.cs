/*
 * Copyright (C) 2012-2013 CypherCore <http://github.com/organizations/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */﻿

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Framework.Threading.Actors
{
    public sealed class ActorTimer : IDisposableResource
    {
        private readonly Timer _timer;

        public IActor TargetActor { get; private set; }

        public Action Callback { get; private set; }

        public bool IsDisposed { get; private set; }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_timer != null);
            Contract.Invariant(TargetActor != null);
            Contract.Invariant(Callback != null);
        }

        public ActorTimer(IActor target, Action callback, TimeSpan delay, int period = Timeout.Infinite)
        {
            Contract.Requires(target != null);
            Contract.Requires(callback != null);
            Contract.Requires(period >= Timeout.Infinite);

            TargetActor = target;
            Callback = callback;
            _timer = new Timer(TimerCallback, null, delay, TimeSpan.FromMilliseconds(period));
        }

        ~ActorTimer()
        {
            InternalDispose();
        }

        private void InternalDispose()
        {
            _timer.Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            InternalDispose();
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        public void Change(TimeSpan delay, int period = Timeout.Infinite)
        {
            Contract.Requires(period >= Timeout.Infinite);

            _timer.Change(delay, TimeSpan.FromMilliseconds(period));
        }

        private void TimerCallback(object state)
        {
            TargetActor.PostAsync(Callback);
        }
    }
}
