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

using Framework.Constants;
using Framework.Network;
using Framework.Utility;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using System;

namespace WorldServer.Game.Spells
{
    public class SpellCastTargets : Cypher
    {
        public SpellCastTargets()
        {
            Elevation = 0;
            Speed = 0;
            strTarget = string.Empty;
            Src = new SpellDestination();
            Dst = new SpellDestination();

            ObjectTarget = null;
            ItemTarget = null;

            ObjectTargetGUID = 0;
            ItemTargetGUID = 0;
            ItemTargetEntry = 0;

            TargetMask = 0;
        }

        void Read(PacketReader data, Unit caster)
        {
            TargetMask = (SpellCastTargetFlags)data.ReadUInt32();

            if (TargetMask == SpellCastTargetFlags.None)
                return;

            if (Convert.ToBoolean(TargetMask & (SpellCastTargetFlags.Unit | SpellCastTargetFlags.UnitMinipet | SpellCastTargetFlags.Gameobject |
                SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.CorpseAlly)))
                ObjectTargetGUID = data.ReadPackedGuid();

            if (Convert.ToBoolean(TargetMask & (SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem)))
                ItemTargetGUID = data.ReadPackedGuid();

            if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.SourceLocation))
            {
                Src.TransportGUID = data.ReadPackedGuid();
                if (Src.TransportGUID != 0)
                    Src.TransportOffset.ReadXYZStream(ref data);
                else
                    Src.Position.ReadXYZStream(ref data);
            }
            else
            {
                Src.TransportGUID = caster.GetTransGUID();
                if (Src.TransportGUID != 0)
                    Src.TransportOffset.Relocate(caster.GetTransOffsetX(), caster.GetTransOffsetY(), caster.GetTransOffsetZ(), caster.GetTransOffsetO());
                else
                    Src.Position.Relocate(caster.Position);
            }

            if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.DestLocation))
            {
                Dst.TransportGUID = data.ReadPackedGuid();
                if (Dst.TransportGUID != 0)
                    Dst.TransportOffset.ReadXYZStream(ref data);
                else
                    Dst.Position.ReadXYZStream(ref data);
            }
            else
            {
                Dst.TransportGUID = caster.GetTransGUID();
                if (Dst.TransportGUID != 0)
                    Dst.TransportOffset.Relocate(caster.GetTransOffsetX(), caster.GetTransOffsetY(), caster.GetTransOffsetZ(), caster.GetTransOffsetO());
                else
                    Dst.Position.Relocate(caster.Position);
            }

            if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.String))
                strTarget = data.ReadString();

            Update(caster);
        }

        public void Write(ref PacketWriter data)
        {
            data.WriteUInt32((uint)TargetMask);

            if (Convert.ToBoolean(TargetMask & (SpellCastTargetFlags.Unit | SpellCastTargetFlags.CorpseAlly | SpellCastTargetFlags.Gameobject |  SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.UnitMinipet)))
                data.WritePackedGuid(ObjectTargetGUID);

            if (Convert.ToBoolean(TargetMask & (SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem)))
            {
                if (ItemTarget != null)
                    data.WriteUInt64(ItemTarget.GetPackGUID());
                else
                    data.WriteUInt8(0);
            }

            if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.SourceLocation))
            {
                data.WritePackedGuid(Src.TransportGUID); // relative position guid here - transport for example
                if (Src.TransportGUID != 0)
                    data.WriteBytes(Src.TransportOffset.WriteXYZStream());
                else
                    data.WriteBytes(Src.Position.WriteXYZStream());
            }

            if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.DestLocation))
            {
                data.WritePackedGuid(Dst.TransportGUID); // relative position guid here - transport for example
                if (Dst.TransportGUID != 0)
                    data.WriteBytes(Dst.TransportOffset.WriteXYZStream());
                else
                    data.WriteBytes(Dst.Position.WriteXYZStream());
            }

            if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.String))
                data.WriteString(strTarget);
        }

        void Update(Unit caster)
        {
            ObjectTarget = ObjectTargetGUID != 0 ? ((ObjectTargetGUID == caster.GetGUIDLow()) ? caster : ObjMgr.GetObject<WorldObject>(caster, ObjectTargetGUID)) : null;

            ItemTarget = null;
            if (caster is Player)
            {
                Player player = caster.ToPlayer();
                if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.Item))
                    ItemTarget = player.GetItemByGuid(ItemTargetGUID);
                else if (Convert.ToBoolean(TargetMask & SpellCastTargetFlags.TradeItem))
                {
                    //if (ItemTargetGUID == (uint)TradeSlots.NONTRADED) // here it is not guid but slot. Also prevents hacking slots
                        //if (TradeData* pTrade = player->GetTradeData())
                            //ItemTarget = pTrade->GetTraderData()->GetItem(TradeSlots.NONTRADED);
                }

                if (ItemTarget != null)
                    ItemTargetEntry = ItemTarget.GetEntry();
            }

            // update positions by transport move
            if (HasSrc() && Src.TransportGUID != 0)
            {
                WorldObject transport = ObjMgr.GetObject<WorldObject>(caster, Src.TransportGUID);
                if (transport != null)
                {
                    Src.Position.Relocate(transport.Position);
                    Src.Position.RelocateOffset(Src.TransportOffset);
                }
            }

            if (HasDst() && Dst.TransportGUID != 0)
            {
                WorldObject transport = ObjMgr.GetObject<WorldObject>(caster, Dst.TransportGUID);
                if (transport != null)
                {
                    Dst.Position.Relocate(transport.Position);
                    Dst.Position.RelocateOffset(Dst.TransportOffset);
                }
            }
        }

        public SpellCastTargetFlags GetTargetMask() { return TargetMask; }
        public void SetTargetMask(uint newMask) { TargetMask = (SpellCastTargetFlags)newMask; }

        public bool HasSrc() { return Convert.ToBoolean(GetTargetMask() & SpellCastTargetFlags.SourceLocation); }
        public bool HasDst() { return Convert.ToBoolean(GetTargetMask() & SpellCastTargetFlags.DestLocation); }
        public bool HasTraj() { return Speed != 0; }

        public Unit GetUnitTarget()
        {
            if (ObjectTarget != null)
                return ObjectTarget.ToUnit();
            return null;
        }
        public void SetUnitTarget(Unit target)
        {
            if (target == null)
                return;

            ObjectTarget = target;
            ObjectTargetGUID = target.GetGUID();
            TargetMask |= SpellCastTargetFlags.Unit;
        }

        #region Fields
        SpellCastTargetFlags TargetMask;

        // objects (can be used at spell creating and after Update at casting)
        WorldObject ObjectTarget;
        Item ItemTarget;

        // object GUID/etc, can be used always
        ulong ObjectTargetGUID;
        ulong ItemTargetGUID;
        uint ItemTargetEntry;

        SpellDestination Src;
        SpellDestination Dst;

        float Elevation;
        float Speed;
        string strTarget;
        #endregion
    }
}
