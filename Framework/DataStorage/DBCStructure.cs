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
using System.Runtime.InteropServices;
using Framework.Constants;
using Framework.Utility;

namespace Framework.DataStorage
{
    [StructLayout(LayoutKind.Sequential)]
    public class AreaTableEntry
    {
        public uint ID;              // 0
        public uint mapid;           // 1
        public uint zone;            // 2 if 0 then it's zone, else it's zone id of this area
        public uint exploreFlag;     // 3 main index
        public uint flags;           // 4
        //uint                       // 5
        //uint                       // 6,
        //uint                       // 7,
        //uint                       // 8,
        //uint                       // 9,
        public uint _internalName;   // 10
        //unk                        // 11
        public int area_level;       // 12
        //uint Name                  // 13
        public uint team;            // 14
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] LiquidTypeOverride;                          // 15-18 liquid override by type ???????
        //float                      // 19
        //float                      // 20
        //uint                       // 21
        //uint                       // 22
        //uint                       // 23
        //uint                       // 24
        //uint                       // 25
        //uint                       // 26 
        //uint                       // 27
        //uint                       // 28
        //uint                       // 29

        public string AreaName
        {
            get { return DBCStorage.AreaTableStrings.LookupByKey(_internalName); }
        }

        // helpers
        public bool IsSanctuary()
        {
            if (mapid == 609)
                return true;
            return Convert.ToBoolean(flags & (uint)AreaFlags.Sanctuary);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ArmorLocationEntry
    {
        public uint InventoryType;                                  // 0
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public float[] Value;                                       // 1-5 multiplier for armor types (cloth...plate, no armor?)
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CharClasses
    {
        public uint ClassID;                                        // 0        m_ID
        public uint powerType;                                      // 1        m_DisplayPower
        // 2        m_petNameToken
        public uint _name;                                         // 3        m_name_lang
        //char*       nameFemale;                               // 4        m_name_female_lang
        //char*       nameNeutralGender;                        // 5        m_name_male_lang
        //char*       capitalizedName                           // 6,       m_filename
        public uint spellfamily;                                    // 7        m_spellClassSet
        //uint32 flags2;                                        // 8        m_flags (0x08 HasRelicSlot)
        public uint CinematicSequence;                              // 9        m_cinematicSequenceID
        //uint;                                               // 10 
        //uint                                                // 11
        //uint                                                // 12
        //uint                                                // 13
        //uint                                                // 14
        //uint                                                // 15
        //uint                                                // 16
        //uint                                                // 17

        /// <summary>
        /// Return current Race Name
        /// </summary>
        public string ClassName
        {
            get { return DBCStorage.ClassStrings[_name]; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CharPowerTypesEntry
    {
        public uint entry;                                               // 0
        public uint classId;                                             // 1
        public uint power;                                               // 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CharRaces
    {
        public uint RaceID;                                     // 0
        // 1 unused
        public uint FactionID;                                  // 2 faction template id
        // 3 unused
        public uint Model_m;                                    // 4
        public uint Model_f;                                    // 5
        // 6 unused
        public uint TeamID;                                     // 7 (7-Alliance 1-Horde)
        // 8-11 unused
        public uint CinematicSequence;                          // 12 id from CinematicSequences.dbc
        // 13 unused
        public uint _name;                                      // 14
        // 17
        // 16 
        // 17-18    m_facialHairCustomization[2]
        // 19       m_hairCustomization
        //uint                                                  // 20 (23 for worgens)
        //uint32                                                // 21 4.0.0
        //uint32                                                // 22 4.0.0

        /// <summary>
        /// Return current Race Name
        /// </summary>
        public string RaceName
        {
            get { return DBCStorage.RaceStrings[_name]; }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CharStartOutfit
    {
        //public uint Id;                                            // 0
        public uint Mask;      // Race, Class, Gender, ?
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public int[] ItemId;
        //int32 ItemDisplayId[MAX_OUTFIT_ITEMS];                // 14-25 not required at server side
        //int32 ItemInventorySlot[MAX_OUTFIT_ITEMS];            // 26-37 not required at server side
        //uint32 Unknown1;                                      // 38, unique values (index-like with gaps ordered in other way as ids)
        //uint32 Unknown2;                                      // 39
        //uint32 Unknown3;                                      // 40
        //uint32 Unknown4;                                      // 41
        //uint32 Unknown5;                                      // 42
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CreatureDisplayInfoEntry
    {
        public uint Displayid;                                  // 0        m_ID
        public uint ModelId;                                    // 1        m_modelID
        // 2        m_soundID
        // 3        m_extendedDisplayInfoID
        public float scale;                                      // 4        m_creatureModelScale
        // 5        m_creatureModelAlpha
        // 6-8      m_textureVariation[3]
        // 9        m_portraitTextureName
        // 10       m_sizeClass
        // 11       m_bloodID
        // 12       m_NPCSoundID
        // 13       m_particleColorID
        // 14       m_creatureGeosetData
        // 15       m_objectEffectPackageID
        // 16
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CreatureModelDataEntry
    {
        public uint Id;
        //uint32 Flags;
        //char* ModelPath
        //uint32 Unk1;
        //float Scale;                                             // Used in calculation of unit collision data
        //int32 Unk2
        //int32 Unk3
        //uint32 Unk4
        //uint32 Unk5
        //float Unk6
        //uint32 Unk7
        //float Unk8
        //uint32 Unk9
        //uint32 Unk10
        //float CollisionWidth;
        public float CollisionHeight;
        public float MountHeight;                                       // Used in calculation of unit collision data when mounted
        //float Unks[11]
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CurrencyTypesEntry
    {
        public uint ID;                                           // 0        not used
        //uint32    Category;                                   // 1        may be category
        //char* name;                                           // 2
        //char* iconName;                                       // 3
        //uint32 unk4;                                          // 4        all 0
        //uint32 unk5;                                          // 5        archaeology-related (?)
        public uint SubstitutionId;                                  // 6
        public uint TotalCap;                                        // 7
        public uint WeekCap;                                         // 8
        public uint Flags;                                           // 9
        //char* description;                                    // 10
    }

    [StructLayout(LayoutKind.Sequential)]
    public class FactionEntry
    {
        public uint ID;                                         // 0        m_ID
        public int reputationListID;                           // 1        m_reputationIndex
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] BaseRepRaceMask;                         // 2-5      m_reputationRaceMask
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] BaseRepClassMask;                        // 6-9      m_reputationClassMask
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] BaseRepValue;                            // 10-13    m_reputationBase
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] ReputationFlags;                         // 14-17    m_reputationFlags
        public uint team;                                       // 18       m_parentFactionID
        public float spilloverRateIn;                            // 19       Faction gains incoming rep * spilloverRateIn
        public float spilloverRateOut;                           // 20       Faction outputs rep * spilloverRateOut as spillover reputation
        public uint spilloverMaxRankIn;                         // 21       The highest rank the faction will profit from incoming spillover
        //uint32    spilloverRank_unk;                          // 22       It does not seem to be the max standing at which a faction outputs spillover ...so no idea
        public uint _name;                                             // 23       m_name_lang
        //char*     description;                                // 24       m_description_lang
        //uint32                                                // 25

        // helpers
        public bool CanHaveReputation()
        {
            return reputationListID >= 0;
        }

        public string Name
        {
            get { return DBCStorage.FactionStrings[_name]; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class FactionTemplateEntry
    {
        public uint ID;                                         // 0        m_ID
        public uint faction;                                    // 1        m_faction
        public uint factionFlags;                               // 2        m_flags
        public uint ourMask;                                    // 3        m_factionGroup
        public uint friendlyMask;                               // 4        m_friendGroup
        public uint hostileMask;                                // 5        m_enemyGroup
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] enemyFaction;        // 6        m_enemies[MAX_FACTION_RELATIONS]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] friendFaction;       // 10       m_friend[MAX_FACTION_RELATIONS]
        //-------------------------------------------------------  end structure

        // helpers
        public bool IsFriendlyTo(FactionTemplateEntry entry)
        {
            if (ID == entry.ID)
                return true;
            if (entry.faction != 0)
            {
                for (int i = 0; i < 4; ++i)
                    if (enemyFaction[i] == entry.faction)
                        return false;
                for (int i = 0; i < 4; ++i)
                    if (friendFaction[i] == entry.faction)
                        return true;
            }
            return Convert.ToBoolean(friendlyMask & entry.ourMask) || Convert.ToBoolean(ourMask & entry.friendlyMask);
        }
        public bool IsHostileTo(FactionTemplateEntry entry)
        {
            if (ID == entry.ID)
                return false;
            if (entry.faction != 0)
            {
                for (int i = 0; i < 4; ++i)
                    if (enemyFaction[i] == entry.faction)
                        return true;
                for (int i = 0; i < 4; ++i)
                    if (friendFaction[i] == entry.faction)
                        return false;
            }
            return (hostileMask & entry.ourMask) != 0;
        }
        public bool IsHostileToPlayers() { return (hostileMask & (uint)FactionMasks.Player) != 0; }
        public bool IsNeutralToAll()
        {
            for (int i = 0; i < 4; ++i)
                if (enemyFaction[i] != 0)
                    return false;
            return hostileMask == 0 && friendlyMask == 0;
        }
        public bool IsContestedGuardFaction() { return (factionFlags & (uint)FactionTemplateFlags.ContestedGuard) != 0; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtBarberShopCostBaseEntry
    {
        //uint32 level;
        public float cost;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtCombatRatingsEntry
    {
        //uint32 level;
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtChanceToMeleeCritBaseEntry
    {
        //uint32 level;
        public float Base;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtChanceToMeleeCritEntry
    {
        //uint32 level;
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtChanceToSpellCritBaseEntry
    {
        public float Base;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtChanceToSpellCritEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtOCTClassCombatRatingScalarEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtOCTRegenHPEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtOCTRegenMPEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtOCTHpPerStaminaEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtRegenHPPerSptEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtRegenMPPerSptEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtSpellScalingEntry
    {
        public float value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtOCTBaseHPByClassEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GtOCTBaseMPByClassEntry
    {
        public float ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GuildPerkSpellsEntry
    {
        //uint32 Id;
        public uint Level;
        public uint SpellId;
    }

    // common struct for:
    // ItemDamageAmmo.dbc
    // ItemDamageOneHand.dbc
    // ItemDamageOneHandCaster.dbc
    // ItemDamageRanged.dbc
    // ItemDamageThrown.dbc
    // ItemDamageTwoHand.dbc
    // ItemDamageTwoHandCaster.dbc
    // ItemDamageWand.dbc
    [StructLayout(LayoutKind.Sequential)]
    public class ItemArmorQualityEntry
    {
        public uint Id;                                             // 0 item level
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public float[] Value;                                       // 1-7 multiplier for item quality
        public uint Id2;                                            // 8 item level
    }
    [StructLayout(LayoutKind.Sequential)]
    public class ItemArmorShieldEntry
    {
        public uint Id;                                             // 0 item level
        public uint Id2;                                            // 1 item level
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public float[] Value;                                       // 2-8 multiplier for item quality
    }
    [StructLayout(LayoutKind.Sequential)]
    public class ItemArmorTotalEntry
    {
        public uint Id;                                             // 0 item level
        public uint Id2;                                            // 1 item level
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] Value;                                       // 2-5 multiplier for armor types (cloth...plate)
    }
    [StructLayout(LayoutKind.Sequential)]
    public class ItemDamageEntry
    {
        public uint Id;                                             // 0 item level
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public float[] DPS;                                         // 1-7 multiplier for item quality
        public uint Id2;                                            // 8 item level
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ItemDisenchantLootEntry
    {
        public uint Id;
        public uint ItemClass;
        public int ItemSubClass;
        public uint ItemQuality;
        public uint MinItemLevel;
        public uint MaxItemLevel;
        public uint RequiredDisenchantSkill;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ItemRandomPropertiesEntry
    {
        public uint ID;                                           // 0        m_ID
        //char* internalName                                    // 1        m_Name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxEnchantmentEffects)]
        public uint[] enchant_id;     // 2-4      m_Enchantment
        // 5-6      unused
        public uint _nameSuffix;                                       // 7        m_name_lang

        public string nameSuffix
        {
            get { return DBCStorage.ItemRandomPropertiesStrings.LookupByKey(_nameSuffix); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ItemRandomSuffixEntry
    {
        public uint ID;                                           // 0        m_ID
        public uint _nameSuffix;                                       // 1        m_name_lang
        // 2        m_internalName
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] enchant_id;                                // 3-7      m_enchantment
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] prefix;                                    // 8-12     m_allocationPct

        public string nameSuffix
        {
            get { return DBCStorage.ItemRandomSuffixStrings.LookupByKey(_nameSuffix); }
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public class LiquidTypeEntry
    {
        public uint Id;
        //char*  Name;
        //uint32 Flags;
        public uint Type;
        //uint32 SoundId;
        public uint SpellId;
        //float MaxDarkenDepth;
        //float FogDarkenIntensity;
        //float AmbDarkenIntensity;
        //float DirDarkenIntensity;
        //uint32 LightID;
        //float ParticleScale;
        //uint32 ParticleMovement;
        //uint32 ParticleTexSlots;
        //uint32 LiquidMaterialID;
        //char* Texture[6];
        //uint32 Color[2];
        //float Unk1[18];
        //uint32 Unk2[4];
    }

    [StructLayout(LayoutKind.Sequential)]
    public class NameGen
    {
        public uint Id;
        public uint _name;
        public uint Race;
        public uint Gender;

        public string Name
        {
            get { return DBCStorage.NameGenStrings.LookupByKey(_name); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MapEntry
    {
        public uint MapID;                                          // 0        m_ID
        public uint _internalname;                                   // 1        m_Directory
        public uint map_type;                                       // 2        m_InstanceType
        public uint mapFlags;                                       // 3        m_Flags (0x100 - CAN_CHANGE_PLAYER_DIFFICULTY)
        public uint unk4;                                           // 4 4.0.1
        //public uint isPvP;                                          // 5        m_PVP 0 or 1 for battlegrounds (not arenas)
        public uint _name;                                           // 6        m_MapName_lang
        public uint linked_zone;                                    // 7        m_areaTableID
        public uint _hordeIntro;                                     // 8        m_MapDescription0_lang
        public uint _allianceIntro;                                  // 9        m_MapDescription1_lang
        public uint multimap_id;                                    // 10       m_LoadingScreenID (LoadingScreens.dbc)
        public float BattlefieldMapIconScale;                        // 11       m_minimapIconScale
        public int ghost_entrance_map;                             // 12       m_corpseMapID map_id of entrance map in ghost mode (continent always and in most cases = normal entrance)
        public float ghost_entrance_x;                               // 13       m_corpseX entrance x coordinate in ghost mode  (in most cases = normal entrance)
        public float ghost_entrance_y;                               // 14       m_corpseY entrance y coordinate in ghost mode  (in most cases = normal entrance)
        public uint timeOfDayOverride;                              // 15       m_timeOfDayOverride
        public uint addon;                                          // 16       m_expansionID
        public uint unkTime;                                        // 17       m_raidOffset
        public uint maxPlayers;                                     // 18       m_maxPlayers
        public uint NextPhaseMap;                                   // 19 - MapId for next phase.

        public uint Expansion() { return addon; }
        public bool IsDungeon() { return map_type == (uint)MapTypes.Instance || map_type == (uint)MapTypes.Raid; }
        public bool IsNonRaidDungeon() { return map_type == (uint)MapTypes.Instance; }
        public bool Instanceable() { return map_type == (uint)MapTypes.Instance || map_type == (uint)MapTypes.Raid || map_type == (uint)MapTypes.Battleground || map_type == (uint)MapTypes.Arena; }
        public bool IsRaid() { return map_type == (uint)MapTypes.Raid; }
        public bool IsBattleGround() { return map_type == (uint)MapTypes.Battleground; }
        public bool IsBattleArena() { return map_type == (uint)MapTypes.Arena; }
        public bool IsBattleGroundOrArena() { return map_type == (uint)MapTypes.Battleground || map_type == (uint)MapTypes.Arena; }

        public bool IsMountAllowed()
        {
            return !IsDungeon() ||
                MapID == 209 || MapID == 269 || MapID == 309 ||       // TanarisInstance, CavernsOfTime, Zul'gurub
                MapID == 509 || MapID == 534 || MapID == 560 ||       // AhnQiraj, HyjalPast, HillsbradPast
                MapID == 568 || MapID == 580 || MapID == 595 ||       // ZulAman, Sunwell Plateau, Culling of Stratholme
                MapID == 603 || MapID == 615 || MapID == 616;         // Ulduar, The Obsidian Sanctum, The Eye Of Eternity
        }

        public bool IsContinent()
        {
            return MapID == 0 || MapID == 1 || MapID == 530 || MapID == 571;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public class MapDifficultyEntry
    {
        //uint32      Id;                                       // 0
        public uint MapId;                                      // 1
        public uint Difficulty;                                 // 2 (for arenas: arena slot)
        public uint _areaTriggerText;                                // 3        m_message_lang (text showed when transfer to map failed)
        public uint resetTime;                                  // 4,       m_raidDuration in secs, 0 if no fixed reset time
        public uint maxPlayers;                                 // 5,       m_maxPlayers some heroic versions have 0 when expected same amount as in normal version
        //char*       difficultyString;                         // 6        m_difficultystring

        public string AreaTriggerText
        {
            get { return DBCStorage.MapDifficultyStrings[_areaTriggerText]; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class RandomPropertiesPointsEntry
    {
        //uint32  Id;                                           // 0 hidden key
        public uint itemLevel;                                    // 1
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] EpicPropertiesPoints;                      // 2-6
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] RarePropertiesPoints;                      // 7-11
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] UncommonPropertiesPoints;                  // 12-16
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SkillLineEntry
    {
        public uint id;                // 0  m_ID
        public int categoryId;         // 1  m_categoryID
        public uint _name;             // 2  m_displayName_lang
        //uint description;              // 3  m_description_lang
        public uint spellIcon;         // 4  m_spellIconID
        public uint canLink;           // 5  m_canLink (prof. with recipes)
        //uint skillCostID;              // 6  m_skillCostsID
        //uint alternateVerb;            // 7  m_alternateVerb_lang
        //uint                           // 8

        public string Name
        {
            get { return DBCStorage.SkillLineStrings.LookupByKey(_name); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SkillLineAbilityEntry
    {
        public uint id;                     // 0   m_ID
        public uint skillId;                // 1   m_skillLine
        public uint spellId;                // 2   m_spell
        public uint racemask;               // 3   m_raceMask
        public uint classmask;              // 4   m_classMask
        public uint req_skill_value;        // 5   m_minSkillLineRank
        public uint forward_spellid;        // 6   m_supercededBySpell 
        public uint learnOnGetSkill;        // 7   m_acquireMethod
        public uint max_value;              // 8   m_trivialSkillLineRankHigh
        public uint min_value;              // 9   m_trivialSkillLineRankLow
        //uint32;                           // 5
        //uint32    classmaskNot;             // 11
        //unk                                 // 12
    }

    #region Spell
    [StructLayout(LayoutKind.Sequential)]
    public class SpellAuraOptionsEntry
    {
        public uint Id;                                           // 0        m_ID
        public uint spellId;                                      // 1      m_spellId;
        //unk                                                     // 2
        public uint StackAmount;                                  // 3       m_cumulativeAura
        public uint procChance;                                   // 4       m_procChance
        public uint procCharges;                                  // 5       m_procCharges
        public uint procFlags;                                    // 6       m_procTypeMask
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellAuraRestrictionsEntry
    {
        public uint Id;                         // 0    m_ID
        public uint spellId;                    // 1    m_spellId;
        //unk                                   // 2    unk
        public uint CasterAuraState;            // 3    m_casterAuraState
        public uint TargetAuraState;            // 4    m_targetAuraState
        public uint CasterAuraStateNot;         // 5    m_excludeCasterAuraState
        public uint TargetAuraStateNot;         // 6    m_excludeTargetAuraState
        public uint casterAuraSpell;            // 7    m_casterAuraSpell
        public uint targetAuraSpell;            // 8    m_targetAuraSpell
        public uint excludeCasterAuraSpell;     // 9    m_excludeCasterAuraSpell
        public uint excludeTargetAuraSpell;     // 10   m_excludeTargetAuraSpell
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellCastingRequirementsEntry
    {
        public uint Id;                        // 0    m_ID
        public uint FacingCasterFlags;         // 1    m_facingCasterFlags
        //public uint MinFactionId;            // 2    m_minFactionID not used
        //public uint MinReputation;           // 3    m_minReputation not used
        public int AreaGroupId;                // 4    m_requiredAreaGroupId
        //public uint    RequiredAuraVision;   // 5    m_requiredAuraVision not used
        public uint RequiresSpellFocus;        // 6    m_requiresSpellFocus
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellCastTimesEntry
    {
        public uint ID;                        // 0
        public int CastTime;                   // 1
        //public float CastTimePerLevel;       // 2    unsure / per skill?
        //public int MinCastTime;              // 3    unsure
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellCategoriesEntry
    {
        public uint Id;                        // 0    m_ID
        public uint spellId;                   // 1    m_spellId;
        //unk                                  // 2
        public uint Category;                  // 3    m_category
        public uint DmgClass;                  // 4    m_defenseType
        public uint Dispel;                    // 5    m_dispelType
        public uint Mechanic;                  // 6    m_mechanic
        public uint PreventionType;            // 7    m_preventionType
        public uint StartRecoveryCategory;     // 8    m_startRecoveryCategory
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellClassOptionsEntry
    {
        public uint Id;                                         // 0        m_ID
        //public uint    modalNextSpell;                             // 1       m_modalNextSpell not used
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] SpellFamilyFlags;                             // 2-4
        public uint SpellFamilyName;                              // 5       m_spellClassSet
        //char*   Description;                                  // 6 4.0.0
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellCooldownsEntry
    {
        public uint Id;                                           // 0        m_ID
        public uint spellId; //      m_spellId;
        //unk
        public uint CategoryRecoveryTime;                         // 31       m_categoryRecoveryTime
        public uint RecoveryTime;                                 // 30       m_recoveryTime
        public uint StartRecoveryTime;                            // 146      m_startRecoveryTime
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellDifficultyEntry
    {
        public uint ID;                           // 0
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] SpellID;                     // 1-4 instance modes: 10N, 25N, 10H, 25H or Normal/Heroic if only 1-2 is set, if 3-4 is 0 then Mode-2
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellDurationEntry
    {
        public uint ID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] Duration;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellEffectEntry
    {
        public uint Id;                                           // 0         m_ID
        //public uint spellId; //      m_spellId;
        //unk
        public uint Effect;                                       // 1         m_effect
        public float EffectValueMultiplier;                        // 2         m_effectAmplitude
        public uint EffectApplyAuraName;                          // 3         m_effectAura
        public uint EffectAmplitude;                              // 4         m_effectAuraPeriod
        public int EffectBasePoints;                             // 5         m_effectBasePoints (don't must be used in spell/auras explicitly, must be used cached Spell::m_currentBasePoints)
        public float EffectBonusMultiplier;                        // 6         m_effectBonus
        public float EffectDamageMultiplier;                       // 7         m_effectChainAmplitude
        public uint EffectChainTarget;                            // 8         m_effectChainTargets
        public int EffectDieSides;                               // 9         m_effectDieSides
        public uint EffectItemType;                               // 10        m_effectItemType
        public uint EffectMechanic;                               // 11        m_effectMechanic
        public int EffectMiscValue;                              // 12        m_effectMiscValue
        public int EffectMiscValueB;                             // 13        m_effectMiscValueB
        public float EffectPointsPerComboPoint;                    // 14        m_effectPointsPerCombo
        public uint EffectRadiusIndex;                            // 15        m_effectRadiusIndex - spellradius.dbc
        public uint EffectRadiusMaxIndex;                         // 16        4.0.0
        public float EffectRealPointsPerLevel;                     // 17        m_effectRealPointsPerLevel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] EffectSpellClassMask;                         // 18 19 20  m_effectSpellClassMask1(2/3), effect 0
        //unk
        public uint EffectTriggerSpell;                           // 21        m_effectTriggerSpell
        //unk                                      // 
        public uint EffectImplicitTargetA;                        // 22        m_implicitTargetA
        public uint EffectImplicitTargetB;                        // 23        m_implicitTargetB
        public uint EffectSpellId;                                // 24        new 4.0.0
        public uint EffectIndex;                                  // 25        new 4.0.0
        //unk
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellEntry
    {
        public uint Id;                                  // 0        m_ID
        public uint _SpellName;                          // 1        m_name_lang
        public uint _Rank;                               // 2        m_nameSubtext_lang
        //public uint Description;                         // 3       m_description_lang not used
        //public uint ToolTip;                             // 4       m_auraDescription_lang not used
        public uint RuneCostID;                          // 5        m_runeCostID
        //public uint spellMissileID;                      // 6       m_spellMissileID not used
        //public uint spellDescriptionVariableID;          // 7       m_spellDescriptionVariableID, 3.2.0
        public float unk;                                // 8
        public uint SpellScalingId;                      // 9        SpellScaling.dbc
        public uint SpellAuraOptionsId;                  // 10       SpellAuraOptions.dbc
        public uint SpellAuraRestrictionsId;             // 11
        public uint SpellCastingRequirementsId;          // 12       SpellCastingRequirements.dbc
        public uint SpellCategoriesId;                   // 13       SpellCategories.dbc
        public uint SpellClassOptionsId;                 // 14       SpellClassOptions.dbc
        public uint SpellCooldownsId;                    // 15       SpellCooldowns.dbc
        public uint SpellEquippedItemsId;                // 16       SpellEquippedItems.dbc
        public uint SpellInterruptsId;                   // 17       SpellInterrupts.dbc
        public uint SpellLevelsId;                       // 18       SpellLevels.dbc
        public uint SpellPowerId;			 // 19       SpellPower.dbc
        public uint SpellReagentsId;                     // 20       SpellReagents.dbc
        public uint SpellShapeshiftId;                   // 21       SpellShapeshift.dbc
        public uint SpellTargetRestrictionsId;           // 22       SpellTargetRestrictions.dbc
        public uint SpellTotemsId;                       // 23       SpellTotems.dbc
        public uint _spellMisc;                          // 24 	     SpellMisc.dbc

        public string Name
        {
            get { return DBCStorage.SpellStrings[_SpellName]; }
        }
        public string Rank
        {
            get { return DBCStorage.SpellStrings[_Rank]; }
        }

        public SpellMisc Misc
        {
            get { return DBCStorage.SpellMiscStorage.LookupByKey(_spellMisc); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellEquippedItemsEntry
    {
        public uint Id;                                          // 0    m_ID
        public uint spellId; //      m_spellId;
        //unk
        public int EquippedItemClass;                            // 70   m_equippedItemClass (value)
        public int EquippedItemInventoryTypeMask;                // 72   m_equippedItemInvTypes (mask)
        public int EquippedItemSubClassMask;                     // 71   m_equippedItemSubclass (mask)
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellFocusObjectEntry
    {
        public uint ID;                                           // 0
        //char*     Name;                                       // 1        m_name_lang
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellInterruptsEntry
    {
        public uint Id;                                           // 0        m_ID
        public uint spellId; //      m_spellId;
        //unk
        public uint AuraInterruptFlags;                           // 1       m_auraInterruptFlags
        //public uint                                                // 2       4.0.0
        public uint ChannelInterruptFlags;                        // 3       m_channelInterruptFlags
        //public uint                                                // 4       4.0.0
        public uint InterruptFlags;                               // 5       m_interruptFlags
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SpellItemEnchantmentConditionEntry
    {
        public uint ID;                                             // 0        m_ID
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] Color;                                       // 1-5      m_lt_operandType
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        //public uint  LT_Operand[5];         // 6-10     m_lt_operand
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] Comparator;                                  // 11-15    m_operator
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] CompareColor;                               // 15-20    m_rt_operandType
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] Value;                                       // 21-25    m_rt_operand
        //uint8   Logic[5]                                      // 25-30    m_logic
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellItemEnchantmentEntry
    {
        public uint ID;                                         // 0        m_ID
        //public uint      charges;                                  // 1        m_charges
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] type;        // 2-4      m_effect[MAX_ITEM_ENCHANTMENT_EFFECTS]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] amount;      // 5-7      m_effectPointsMin[MAX_ITEM_ENCHANTMENT_EFFECTS]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] spellid;    // 8-10    m_effectArg[MAX_ITEM_ENCHANTMENT_EFFECTS]
        public uint description;                                // 11       m_name_lang
        public uint aura_id;                                    // 12       m_itemVisual
        public uint slot;                                       // 13       m_flags
        public uint GemID;                                      // 14       m_src_itemID
        public uint EnchantmentCondition;                       // 15       m_condition_id
        public uint requiredSkill;                              // 16       m_requiredSkillID
        public uint requiredSkillValue;                         // 17       m_requiredSkillRank
        public uint requiredLevel;                              // 18       new in 3.1
        //unk
        //unk
        //unk
        // 22       new in 3.1
        //unk
        //unk
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellLevelsEntry
    {
        public uint Id;                                           // 0        m_ID
        public uint spellId; //      m_spellId;
        //unk
        public uint baseLevel;                                    // 1       m_baseLevel
        public uint maxLevel;                                     // 2       m_maxLevel
        public uint spellLevel;                                   // 3       m_spellLevel
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellMisc
    {
        //public uint Id;//0
        public uint spellId;                                      // 1      m_spellID
        public uint unk;                                          // 2
        public uint Attributes;                                   // 3      m_attribute
        public uint AttributesEx;                                 // 4      m_attributesEx
        public uint AttributesEx2;                                // 5      m_attributesExB
        public uint AttributesEx3;                                // 6      m_attributesExC
        public uint AttributesEx4;                                // 7      m_attributesExD
        public uint AttributesEx5;                                // 8      m_attributesExE
        public uint AttributesEx6;                                // 9      m_attributesExF
        public uint AttributesEx7;                                // 10      m_attributesExG
        public uint AttributesEx8;                                // 11     m_attributesExH
        public uint AttributesEx9;                                // 12     m_attributesExI
        public uint AttributesEx10;                               // 13     m_attributesExJ
        public uint AttributesEx11;                               // 14     m_attributesExK
        public uint CastingTimeIndex;                             // 15     m_castingTimeIndex
        public uint DurationIndex;                                // 16     m_durationIndex  
        public uint RangeIndex;                                   // 17     m_rangeIndex
        public float Speed;                                       // 18     m_speed
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] SpellVisual;                                // 19-20  m_spellVisualID
        public uint SpellIconID;                                  // 21     m_spellIconID
        public uint ActiveIconID;                                 // 22     m_activeIconID
        public uint SchoolMask;                                   // 23     m_schoolMask
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellPowerEntry
    {
        public uint Id;                                           // 0        m_ID
        public uint spellId; //      m_spellId;
        //unk
        public uint manaCost;                                     // 1       m_manaCost
        public uint manaCostPerlevel;                             // 2       m_manaCostPerLevel
        public uint ManaCostPercentage;                           // 3       m_manaCostPct
        public uint manaPerSecond;                                // 4       m_manaPerSecond
        //public uint  PowerDisplayId;                               // 5       m_powerDisplayID - id from PowerDisplay.dbc, new in 3.1
        //public uint  unk1;                                         // 6       4.0.0
        //public float   unk2;                                         // 7       4.3.0
        //unk
        //unk
        //unk          //12
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellRadiusEntry
    {
        public uint ID;
        public float radius;
        //public uint    Unk    //always 0
        //public float radiusFriend;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellRangeEntry
    {
        public uint ID;
        public float minRangeHostile;
        public float minRangeFriend;
        public float maxRangeHostile;
        public float maxRangeFriend;                               //friend means unattackable unit here
        public uint type;
        //char*   Name;                                         // 6-21     m_displayName_lang
        //char*   ShortName;                                    // 23-38    m_displayNameShort_lang
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellReagentsEntry
    {
        public uint Id;                   // 0        m_ID
        public uint spellId; //      m_spellId;
        //unk
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedConst.MaxSpellReagents)]
        public int[] Reagent;                  // 54-61    m_reagent
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedConst.MaxSpellReagents)]
        public uint[] ReagentCount;            // 62-69    m_reagentCount
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellRuneCostEntry
    {
        public uint ID;                                             // 0
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] RuneCost;                                    // 1-3 (0=blood, 1=frost, 2=unholy)
        //unk
        public uint runePowerGain;                                  // 4

        bool NoRuneCost() { return RuneCost[0] == 0 && RuneCost[1] == 0 && RuneCost[2] == 0; }
        bool NoRunicPowerGain() { return runePowerGain == 0; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellScalingEntry
    {
        public uint Id;                                         // 0        m_ID
        public int CastTimeMin;                                  // 1
        public int CastTimeMax;                                  // 2
        public int CastTimeMaxLevel;                             // 3
        public int ScalingClass;                                 // 4        (index * 100) + charLevel - 1 => gtSpellScaling.dbc
        public float unkLevel;
        //unk
        //unk
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellShapeshiftEntry
    {
        public uint Id;                                           // 0 - m_ID
        public uint Stances;                                   // 3 - m_shapeshiftMask
        // public uint unk_320_2;                                    // 2 - 3.2.0
        public uint StancesNot;                                      // 1 - m_shapeshiftExclude
        // public uint unk_320_3;                                    // 4 - 3.2.0
        // public uint    StanceBarOrder;                            // 5 - m_stanceBarOrder not used
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellShapeshiftFormEntry
    {
        public uint ID;                                              // 0
        //public uint buttonPosition;                                // 1 unused
        //char* Name;                                           // 2 unused
        public uint flags1;                                          // 3
        public int creatureType;                                    // 4 <=0 humanoid, other normal creature types
        //public uint unk1;                                          // 5 unused, related to next field
        public uint attackSpeed;                                     // 6
        public uint modelID_A;                                       // 7 alliance modelid (0 means no model)
        public uint modelID_H;                                       // 8 horde modelid (but only for one form)
        //public uint unk3;                                          // 9 unused always 0
        //public uint unk4;                                          // 10 unused always 0
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedConst.MaxShapeshiftSpells)]
        public uint[] stanceSpell;//[MAX_SHAPESHIFT_SPELLS];              // 11-18 spells which appear in the bar after shapeshifting
        //public uint unk5;                                          // 19
        //public uint unk6;                                          // 20
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellTargetRestrictionsEntry
    {
        public uint Id;                                           // 0        m_ID
        public uint spellId; //      m_spellId;
        //unk
        public float MaxTargetRadius;
        //unk
        public uint MaxAffectedTargets;                           // 1        m_maxTargets
        public uint MaxTargetLevel;                               // 2        m_maxTargetLevel
        public uint TargetCreatureType;                           // 3       m_targetCreatureType
        public uint Targets;                                      // 4       m_targets
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SpellTotemsEntry
    {
        public uint Id;                           // 0        m_ID
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedConst.MaxSpellTotems)]
        public uint[] TotemCategory;              // 1        m_requiredTotemCategoryID
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedConst.MaxSpellTotems)]
        public uint[] Totem;                      // 2        m_totem
    }
    #endregion
}
