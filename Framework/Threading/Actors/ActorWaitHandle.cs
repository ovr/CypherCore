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
    internal sealed class ActorWaitHandle : IWaitable
    {
        private readonly AutoResetEvent _event;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_event != null);
        }

        public ActorWaitHandle(AutoResetEvent eventHandle)
        {
            Contract.Requires(eventHandle != null);

            _event = eventHandle;
        }

        public bool Wait()
        {
            return _event.WaitOne();
        }

        public bool Wait(TimeSpan timeout)
        {
            return _event.WaitOne(timeout);
        }

        public bool WaitExitContext(TimeSpan timeout)
        {
            return _event.WaitOne(timeout, true);
        }
    }
}
