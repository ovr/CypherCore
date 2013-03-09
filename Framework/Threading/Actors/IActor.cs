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
    [ContractClass(typeof(ActorContracts))]
    public interface IActor : IDisposableResource
    {
        void Join();

        void PostAsync(Action msg);

        IWaitable PostWait(Action msg);
    }

    [ContractClass(typeof(ActorContracts<>))]
    public interface IActor<out TThis> : IActor
        where TThis : IActor<TThis>
    {
        void PostAsync(Action<TThis> msg);

        IWaitable PostWait(Action<TThis> msg);
    }

    [ContractClassFor(typeof(IActor))]
    public abstract class ActorContracts : IActor
    {
        public void PostAsync(Action msg)
        {
            Contract.Requires(msg != null);
        }

        public IWaitable PostWait(Action msg)
        {
            Contract.Requires(msg != null);
            Contract.Ensures(Contract.Result<IWaitable>() != null);

            return null;
        }

        public abstract void Join();

        public abstract void Dispose();

        public abstract bool IsDisposed { get; }

        //public abstract void AddPermission(Permission perm);

        public abstract void RemovePermission(Type permType);

        public abstract bool HasPermission(Type permType);
    }

    [ContractClassFor(typeof(IActor<>))]
    public abstract class ActorContracts<TThis> : IActor<TThis>
        where TThis : IActor<TThis>
    {
        public void PostAsync(Action<TThis> msg)
        {
            Contract.Requires(msg != null);
        }

        public IWaitable PostWait(Action<TThis> msg)
        {
            Contract.Requires(msg != null);
            Contract.Ensures(Contract.Result<IWaitable>() != null);

            return null;
        }

        public abstract void Dispose();

        public abstract bool IsDisposed { get; }

        //public abstract void AddPermission(Permission perm);

        public abstract void RemovePermission(Type permType);

        public abstract bool HasPermission(Type permType);

        public abstract void Join();

        public abstract void PostAsync(Action msg);

        public abstract IWaitable PostWait(Action msg);
    }
}
