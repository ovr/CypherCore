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

using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using System.Collections.Generic;

namespace WorldServer.Network
{
    public static class SocketAsyncEventArgsPool
    {
        private static readonly ObjectPool<SocketAsyncEventArgs> _objectPool =
            new ObjectPool<SocketAsyncEventArgs>(() => new SocketAsyncEventArgs());

        public static SocketAsyncEventArgs Acquire()
        {
            Contract.Ensures(Contract.Result<SocketAsyncEventArgs>() != null);
            
            var obj = _objectPool.GetObject();
            Contract.Assume(obj != null);
            return obj;
        }

        public static void Release(SocketAsyncEventArgs arg)
        {
            arg.AcceptSocket = null;
            arg.SetBuffer(null, 0, 0);
            arg.BufferList = null;
            arg.DisconnectReuseSocket = false;
            arg.RemoteEndPoint = null;
            arg.SendPacketsElements = null;
            arg.SendPacketsFlags = 0;
            arg.SendPacketsSendSize = 0;
            arg.SocketError = 0;
            arg.SocketFlags = 0;
            arg.UserToken = null;

            _objectPool.PutObject(arg);
        }

    }
}
