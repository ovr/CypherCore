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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using Framework.Logging;

namespace Framework.Threading.Actors
{
    public class Actor : IActor
    {
        private readonly IEnumerator<Operation> _msgIterator;

        private readonly IEnumerator<Operation> _mainIterator;

        private readonly ConcurrentQueue<Action> _msgQueue = new ConcurrentQueue<Action>();

        private readonly AutoResetEvent _disposeEvent;

        public bool IsDisposed { get; private set; }

        internal bool IsActive { get; set; }

        internal IScheduler Scheduler { get; set; }

        public ActorContext Context { get; private set; }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(Context != null);
            Contract.Invariant(_msgQueue != null);
            Contract.Invariant(_disposeEvent != null);

            // Don't add IsDisposed here. It would cause major cancellation issues in an asynchronous environment.
        }

        public Actor()
            : this(ActorContext.Global)
        {
        }

        public Actor(ActorContext context)
        {
            Contract.Requires(context != null);

            Context = context;
            _disposeEvent = new AutoResetEvent(false);

            _msgIterator = EnumerateMessages();
            _mainIterator = Main();

            Scheduler = Context.RegisterActor(this);
            Scheduler.Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs args)
        {
            Scheduler.Disposed -= OnDisposed;

            // No guarantee is made about where an actor is disposed!
            InternalDispose();
        }

        ~Actor()
        {
            Dispose(false);
        }

        public void Join()
        {
            _disposeEvent.WaitOne();
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
            _disposeEvent.Set();

            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA2213", Justification = "_disposeEvent must not be disposed.")]
        protected virtual void Dispose(bool disposing)
        {
        }

        internal bool ProcessMessages()
        {
            _msgIterator.MoveNext();

            return _msgQueue.Count > 0;
        }

        internal bool ProcessMain()
        {
            var result = _mainIterator.MoveNext();

            // Happens if a yield break occurs.
            if (!result)
                return false;

            var operation = _mainIterator.Current;

            if (operation == Operation.Dispose)
                Dispose();

            return operation == Operation.Continue;
        }

        public void PostAsync(Action msg)
        {
            _msgQueue.Enqueue(msg);

            Action tmp;
            while (!_msgQueue.TryPeek(out tmp))
                if (_msgQueue.Count == 0)
                    return; // The message was processed immediately, and we can just return.

            if (msg == tmp)
                Scheduler.AddActor(this); // The message was sent while the actor was idle; restart it to continue processing.
        }

        public IWaitable PostWait(Action msg)
        {
            var eventHandle = new AutoResetEvent(false);

            PostAsync(() =>
            {
                msg();

                // Signal that message processing has happened.
                eventHandle.Set();
            });

            return new ActorWaitHandle(eventHandle);
        }

        protected virtual IEnumerator<Operation> Main()
        {
            Contract.Ensures(Contract.Result<IEnumerator<Operation>>() != null);

            yield break; // No main by default.
        }

        private IEnumerator<Operation> EnumerateMessages()
        {
            Contract.Ensures(Contract.Result<IEnumerator<Operation>>() != null);

            while (true)
            {
                Action msg;
                if (_msgQueue.TryDequeue(out msg))
                {
                    var op = OnMessage(msg);
                    if (op != null)
                        yield return (Operation)op;
                }

                yield return Operation.Continue;
            }
        }

        protected virtual Operation? OnMessage(Action msg)
        {
            try
            {
                msg();
            }
            catch (Exception ex)
            {
                Log.outError(ex.Message);
                return Operation.Dispose;
            }

            return null;
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1063", Justification = "IDisposable is part of IActor.")]
    public abstract class Actor<TThis> : Actor, IActor<TThis>
        where TThis : Actor<TThis>
    {
        protected Actor()
        {
        }

        protected Actor(ActorContext context)
            : base(context)
        {
            Contract.Requires(context != null);
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
