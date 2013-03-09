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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Framework.Logging;
using System.Collections.Generic;
using WorldServer.Game.Managers;

namespace WorldServer.Network
{
    public class WorldNetwork
    {
        protected Socket Listener;
        public bool IsListening { get; private set; }

        public bool Start(string host, int port)
        {            
            if (IsListening) 
                throw new InvalidOperationException("Server is already listening.");

            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            try
            {
                Listener.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            }
            catch (SocketException)
            {
                Log.outError("{0} can not bind on {1}", this.GetType().Name, host);
                return false;
            }

            // Start listening for incoming connections.
            Listener.Listen(10);
            IsListening = true;

            Accept(null);

            return true;
        }
        private void Accept(SocketAsyncEventArgs args)
        {
            if (args == null)
            {                
                args = SocketAsyncEventArgsPool.Acquire();
                args.Completed += OnAccept;
            }
            else
                args.AcceptSocket = null;

            try
            {
                if (!Listener.AcceptAsync(args))
                    OnAccept(this, args);
            }
            catch (Exception ex)
            {
                args.Completed -= OnAccept;
                SocketAsyncEventArgsPool.Release(args);

                if (ex is ObjectDisposedException)
                    return;

                if (ex is SocketException)
                {
                    Log.outException(ex);
                    return;
                }

                throw;
            }
        }
        private void OnAccept(object sender, SocketAsyncEventArgs args)
        {
            var sock = args.AcceptSocket;
            if (!sock.Connected)
                return;

            try
            {
                var accept = true;
                if (!accept)
                {
                    Log.outWarn("Disconnecting client from {0}; already connected.", sock.RemoteEndPoint);

                    sock.Shutdown(SocketShutdown.Both);
                    sock.Disconnect(false);
                    sock.Close();
                }
                else
                {
                    // Add the client and thus start receiving.
                    var client = new WorldSession();//(sock, this, _propagator);
                    client.OnConnect(sock);
                }
            }
            catch (Exception ex)
            {
                Log.outException(ex);
            }

            // Continue accepting with the event args we were using before.
            Accept(args);
        }
    }
}
