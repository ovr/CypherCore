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

namespace Framework.Threading.Actors
{
    [ContractClass(typeof(SchedulerContracts))]
    internal interface IScheduler : IDisposableResource
    {
        event EventHandler Disposed;

        /// <summary>
        /// Gets the amount of actors in this scheduler.
        /// </summary>
        /// <value>The amount of actors managed by this scheduler.</value>
        int ActorCount { get; }

        void AddActor(Actor actor);
    }

    [ContractClassFor(typeof(IScheduler))]
    internal abstract class SchedulerContracts : IScheduler
    {
        public abstract event EventHandler Disposed;

        public int ActorCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return 0;
            }
        }

        public void AddActor(Actor actor)
        {
            Contract.Requires(actor != null);
        }

        public abstract void Dispose();

        public abstract bool IsDisposed { get; }
    }
}
