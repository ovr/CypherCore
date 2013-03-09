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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Framework.Threading.Actors
{
    public abstract class ChildActor : IActor
    {
        protected ChildActor(IActor parent)
        {
            Contract.Requires(parent != null);

            _parent = parent;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_parent != null);
        }

        private readonly IActor _parent;

        ~ChildActor()
        {
            Dispose(false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1816", Justification = "Behavior intended.")]
        [SuppressMessage("Microsoft.Design", "CA1063", Justification = "Behavior intended.")]
        public void Dispose()
        {
            PostAsync(InternalDispose);
        }

        [SuppressMessage("Microsoft.Usage", "CA1816", Justification = "Behavior intended.")]
        private void InternalDispose()
        {
            if (IsDisposed)
                return;

            Dispose(true);
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public bool IsDisposed { get; private set; }

        public void Join()
        {
            _parent.Join();
        }

        public void PostAsync(Action msg)
        {
            _parent.PostAsync(msg);
        }

        public IWaitable PostWait(Action msg)
        {
            return _parent.PostWait(msg);
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1063", Justification = "IDisposable is part of IActor.")]
    public abstract class ChildActor<TThis> : ChildActor, IActor<TThis>
        where TThis : ChildActor<TThis>
    {
        protected ChildActor(IActor parent)
            : base(parent)
        {
            Contract.Requires(parent != null);
        }

        public void PostAsync(Action<TThis> msg)
        {
            PostAsync(() => msg((TThis)this));
        }

        public IWaitable PostWait(Action<TThis> msg)
        {
            return PostWait(() => msg((TThis)this));
        }
    }
}
