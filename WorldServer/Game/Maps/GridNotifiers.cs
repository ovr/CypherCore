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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Game.WorldEntities;
using Framework.Network;
using Framework.Constants;
using Framework.Threading.Actors;

namespace WorldServer.Game.Maps
{
    public abstract class Notifier : Actor<Notifier>
    {
        public abstract void Visit(ref List<WorldObject> objs, NotifierObjectType type);
        public void CreatureUnitRelocationWorker(Creature c, Unit u)
        {
            if (!u.isAlive() || !c.isAlive() || c == u)//|| u.isInFlight())
                return;

            if (c.HasReactState(ReactStates.AGGRESSIVE) && !c.HasUnitState(UnitState.Sightless))
                if (c.IsAIEnabled && c.canSeeOrDetect(u, false, true))
                    c.AI().MoveInLineOfSight_Safe(u);
        }
    }

    public class Visitor<T> where T : Notifier
    {
        public Visitor(T notifier, NotifierObjectType _type)
        {
            m_notifier = notifier;
            SetNotifierType(_type);
        }
        public T GetNotifier() { return m_notifier; }
        T m_notifier;
        NotifierObjectType m_type;
        public NotifierObjectType GetNotifierType() { return m_type; }
        public void SetNotifierType(NotifierObjectType _type) { m_type = _type; }
    }

    public class VisibleNotifier : Notifier
    {
        public VisibleNotifier(Player pl)
        {
            i_player = pl;
            i_data = new UpdateData(pl.GetMapId());
            vis_guids = new List<ulong>(pl.m_clientGUIDs);
            i_visibleNow = new List<Unit>();
        }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
            {
                vis_guids.Remove(obj.GetGUID());
                i_player.UpdateVisibilityOf(obj, ref i_data, ref i_visibleNow);
            }
        }
        public void SendToSelf()
        {
            // at this moment i_clientGUIDs have guids that not iterate at grid level checks
            // but exist one case when this possible and object not out of range: transports
            /*
            if (Transport* transport = i_player.GetTransport())
                for (Transport::PlayerSet::const_iterator itr = transport->GetPassengers().begin();itr != transport->GetPassengers().end();++itr)
                {
                    if (vis_guids.find((*itr)->GetGUID()) != vis_guids.end())
                    {
                        vis_guids.erase((*itr)->GetGUID());

                        i_player.UpdateVisibilityOf((*itr), i_data, i_visibleNow);

                        if (!(*itr)->isNeedNotify(NOTIFY_VISIBILITY_CHANGED))
                            (*itr)->UpdateVisibilityOf(&i_player);
                    }
                }
            */
            foreach (var guid in vis_guids)
            {
                i_player.m_clientGUIDs.Remove(guid);
                i_data.AddOutOfRangeGUID(guid);

                if (WorldObject.IS_PLAYER_GUID(guid))
                {
                    Player pl = Cypher.ObjMgr.FindPlayer(guid);
                    if (pl != null && pl.IsInWorld && !pl.isNeedNotify(NotifyFlags.VISIBILITY_CHANGED))
                        pl.UpdateVisibilityOf(i_player);
                }
            }

            if (!i_data.HasData())
                return;

            PacketWriter packet = null;
            i_data.BuildPacket(ref packet);
            i_player.GetSession().Send(packet);

            //for (std::set<Unit*>::const_iterator it = i_visibleNow.begin(); it != i_visibleNow.end(); ++it)
            //i_player.SendInitialVisiblePackets(*it);
        }

        internal Player i_player;
        internal UpdateData i_data;
        internal List<UInt64> vis_guids;
        internal List<Unit> i_visibleNow;
    }

    public class VisibleChangesNotifier : Notifier
    {
        public VisibleChangesNotifier(WorldObject obj)
        {
            i_object = obj;
        }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
            {
                if (obj.GetTypeId() == ObjectType.Player)
                    Visit((Player)obj);
                else if (obj.GetTypeId() == ObjectType.DynamicObject)
                    Visit((DynamicObject)obj);
                else
                    Visit((Creature)obj);
            }
        }
        void Visit(Player pl)
        {
            if (pl == i_object)
                return;

            pl.UpdateVisibilityOf(i_object);

            //if (pl.HasSharedVision())
            {
                //for (SharedVisionList::const_iterator i = pl.GetSharedVisionList().begin();
                //i != iter->getSource()->GetSharedVisionList().end(); ++i)
                {
                    //if ((*i)->m_seer == iter->getSource())
                    //(*i)->UpdateVisibilityOf(&i_object);
                }
            }

        }
        void Visit(Creature creature)
        {
            //if (creature.HasSharedVision())
            //for (SharedVisionList::const_iterator i = creature.GetSharedVisionList().begin();
            //i != iter->getSource()->GetSharedVisionList().end(); ++i)
            //if ((*i)->m_seer == iter->getSource())
            //(*i)->UpdateVisibilityOf(&i_object);
        }
        void Visit(DynamicObject dynamic)
        {
            //if (IS_PLAYER_GUID(dynamic.GetCasterGUID()))
            //if (Player* caster = (Player*)iter->getSource()->GetCaster())
            //if (caster->m_seer == iter->getSource())
            //caster->UpdateVisibilityOf(&i_object);
        }
        WorldObject i_object;
    }

    public class UpdaterNotifier : Notifier
    {
        uint i_timeDiff;
        public UpdaterNotifier(uint diff)
        {
            i_timeDiff = diff;
        }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs.ToList())
            {
                if (obj.IsInWorld)
                    obj.Update(i_timeDiff);
            }
        }
        void Visit(Player player)
        {
        }
        void Visit(Corpse corpse)
        {
        }
        void Visit(Creature creature)
        {
        }
        //template<class T> void Visit(GridRefManager<T> &m);
        //void Visit(PlayerMapType &) {}
        //void Visit(CorpseMapType &) {}
        //void Visit(CreatureMapType &);
    }

    public class DelayedUnitRelocation : Notifier
    {
        Map i_map;
        Cell cell;
        CellCoord p;
        float i_radius;
        public DelayedUnitRelocation(Cell c, CellCoord pair, Map map, float radius)
        {
            i_map = map;
            cell = c;
            p = pair;
            i_radius = radius;
        }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs.ToList())
            {
                if (obj.GetTypeId() == ObjectType.Player)
                    Visit((Player)obj);
                else if (obj.GetTypeId() == ObjectType.Unit)
                    Visit((Creature)obj);
            }
        }

        void Visit(Creature unit)
        {
            if (!unit.isNeedNotify(NotifyFlags.VISIBILITY_CHANGED))
                return;

            CreatureRelocationNotifier relocate = new CreatureRelocationNotifier(unit);

            var c2world_relocation = new Visitor<CreatureRelocationNotifier>(relocate, NotifierObjectType.Object);
            var c2grid_relocation = new Visitor<CreatureRelocationNotifier>(relocate, NotifierObjectType.Grid);

            cell.Visit(p, c2world_relocation, i_map, unit, i_radius);
            cell.Visit(p, c2grid_relocation, i_map, unit, i_radius);

        }
        void Visit(Player player)
        {
            WorldObject viewPoint = player.m_seer;

            if (!viewPoint.isNeedNotify(NotifyFlags.VISIBILITY_CHANGED))
                return;

            if (player != viewPoint && !viewPoint.IsPositionValid())
                return;

            CellCoord pair2 = GridDefines.ComputeCellCoord(viewPoint.GetPositionX(), viewPoint.GetPositionY());
            Cell cell2 = new Cell(pair2);
            //cell.SetNoCreate(); need load cells around viewPoint or player, that's why its commented

            PlayerRelocationNotifier relocate = new PlayerRelocationNotifier(player);
            var c2world_relocation = new Visitor<PlayerRelocationNotifier>(relocate, NotifierObjectType.Object);
            var c2grid_relocation = new Visitor<PlayerRelocationNotifier>(relocate, NotifierObjectType.Grid);

            cell2.Visit(pair2, c2world_relocation, i_map, viewPoint, i_radius);
            cell2.Visit(pair2, c2grid_relocation, i_map, viewPoint, i_radius);

            relocate.SendToSelf();
        }
    }

    public class PlayerRelocationNotifier : VisibleNotifier
    {
        public PlayerRelocationNotifier(Player player) : base(player) { }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            //base.Visit(ref objs);

            foreach (var obj in objs)
            {
                if (obj.GetTypeId() == ObjectType.Player)
                    Visit((Player)obj);
                else if (obj.GetTypeId() == ObjectType.Unit)
                    Visit((Creature)obj);
            }
        }

        void Visit(Player player)
        {
            vis_guids.Remove(player.GetGUID());

            i_player.UpdateVisibilityOf(player, ref i_data, ref i_visibleNow);

            if (!player.m_seer.isNeedNotify(NotifyFlags.VISIBILITY_CHANGED))
                return;

            player.UpdateVisibilityOf(i_player);
        }

        void Visit(Creature c)
        {
            bool relocated_for_ai = (i_player == i_player.m_seer);

            vis_guids.Remove(c.GetGUID());

            i_player.UpdateVisibilityOf(c, ref i_data, ref i_visibleNow);

            if (relocated_for_ai && !c.isNeedNotify(NotifyFlags.VISIBILITY_CHANGED))
                CreatureUnitRelocationWorker(c, i_player);

        }
    }

    public class CreatureRelocationNotifier : Notifier
    {
        Creature i_creature;
        public CreatureRelocationNotifier(Creature c)
        {
            i_creature = c;
        }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
            {
                if (obj.GetTypeId() == ObjectType.Player)
                    Visit((Player)obj);
                else if (obj is Creature)
                    Visit((Creature)obj);
            }
        }
        void Visit(Player player)
        {
            if (!player.m_seer.isNeedNotify(NotifyFlags.VISIBILITY_CHANGED))
                player.UpdateVisibilityOf(i_creature);

            CreatureUnitRelocationWorker(i_creature, player);
        }
        void Visit(Creature c)
        {
            if (!i_creature.isAlive())
                return;

            CreatureUnitRelocationWorker(i_creature, c);

            if (!c.isNeedNotify(NotifyFlags.VISIBILITY_CHANGED))
                CreatureUnitRelocationWorker(c, i_creature);
        }
    }

    public class ResetNotifier : Notifier
    {
        public void resetNotify(List<WorldObject> objs)
        {
            foreach (var obj in objs)
                obj.ResetAllNotifies();
        }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            resetNotify(objs);
        }
    }

    public class MessageDistDeliverer : Notifier
    {
        WorldObject i_source;
        PacketWriter i_message;
        uint i_phaseMask;
        float i_distSq;
        uint team;
        Player skipped_receiver;
        public MessageDistDeliverer(WorldObject src, PacketWriter msg, float dist, bool own_team_only = false, Player skipped = null)
        {
            i_source = src;
            i_message = msg;
            i_phaseMask = src.GetPhaseMask();
            i_distSq = dist * dist;
            team = (uint)((own_team_only && src.GetTypeId() == ObjectType.Player) ? ((Player)src).GetTeam() : 0);
            skipped_receiver = skipped;

        }
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
            {
                if (obj is Player)
                    Visit((Player)obj);
                else if (obj is Creature)
                    Visit((Creature)obj);
                else if (obj is DynamicObject)
                    Visit((DynamicObject)obj);
            }
        }

        void Visit(Player target)
        {
            if (!target.InSamePhase(i_phaseMask))
                return;

            if (target.GetExactDist2dSq(i_source.Position) > i_distSq)
                return;

            // Send packet to all who are sharing the player's vision
            //if (target.HasSharedVision())
            {
                //SharedVisionList::const_iterator i = target->GetSharedVisionList().begin();
                //for (; i != target->GetSharedVisionList().end(); ++i)
                //if ((*i)->m_seer == target)
                //SendPacket(*i);
            }

            if (target.m_seer == target)// || target.GetVehicle())
                SendPacket(target);

        }
        void Visit(Creature target)
        {
            if (!target.InSamePhase(i_phaseMask))
                return;

            if (target.GetExactDist2dSq(i_source.Position) > i_distSq)
                return;

            // Send packet to all who are sharing the creature's vision
            //if (target.HasSharedVision())
            {
                //SharedVisionList::const_iterator i = target->GetSharedVisionList().begin();
                //for (; i != target->GetSharedVisionList().end(); ++i)
                //if ((*i)->m_seer == target)
                //SendPacket(*i);
            }

        }
        void Visit(DynamicObject target)
        {
            if (!target.InSamePhase(i_phaseMask))
                return;

            if (target.GetExactDist2dSq(i_source.Position) > i_distSq)
                return;

            //if (IS_PLAYER_GUID(target.GetCasterGUID()))
            {
                // Send packet back to the caster if the caster has vision of dynamic object
                //Player caster = (Player*)target->GetCaster();
                //if (caster && caster->m_seer == target)
                // SendPacket(caster);
            }

        }

        void SendPacket(Player player)
        {
            // never send packet to self
            if (player == i_source || (team != 0 && (uint)player.GetTeam() != team) || skipped_receiver == player)
                return;

            if (!player.HaveAtClient(i_source))
                return;

            player.GetSession().Send(i_message);
        }
    }

    public enum NotifierObjectType
    {
        Grid,
        Object
    }
}
