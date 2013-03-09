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

namespace Framework.Constants.Authentication
{
    public enum ClientLink : byte
    {
        AuthLogonChallenge     = 0x00,
        AuthLogonProof         = 0x01,
        AuthReconnectChallenge = 0x02,
        AuthReconnectProof     = 0x03,
        RealmList               = 0x10,
        XferInitiate            = 0x30,
        XferData                = 0x31
    }

    public enum ServerLink : byte
    {
        GruntAuthChallenge = 0x00,
        GruntAuthVerify    = 0x02,
        GruntConnPing      = 0x10,
        GruntConnPong      = 0x11,
        GruntHello          = 0x20,
        GruntProvesession   = 0x21,
        GruntKick           = 0x24,
        GruntPcwarning      = 0x29,
        GruntStrings        = 0x41,
        GruntSunkenupdate   = 0x44,
        GruntSunkenOnline  = 0x46,
        GruntCaistimeupdate = 0x2C
    }
}
