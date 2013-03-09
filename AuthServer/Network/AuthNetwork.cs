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

namespace AuthServer.Network
{
    public class AuthNetwork
    {
        public volatile bool listenSocket = true;
        TcpListener listener;

        public bool Start(string host, int port)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse(host), port);
                listener.Start();

                return true;
            }
            catch (Exception e)
            {
                Log.outError("{0}", e.Message);
                Log.outInit();

                return false;
            }
        }

        public void AcceptConnectionThread()
        {
            new Thread(AcceptConnection).Start();
        }

        void AcceptConnection()
        {
            while (listenSocket)
            {
                Thread.Sleep(1);
                if (listener.Pending())
                {                    
                    AuthClass realmClient = new AuthClass();
                    realmClient.clientSocket = listener.AcceptSocket();
                    Log.outInfo("Incoming Connection from IP: {0}", realmClient.clientSocket.RemoteEndPoint);
                    new Thread(realmClient.Recieve).Start();
                }
            }
        }
    }
}
