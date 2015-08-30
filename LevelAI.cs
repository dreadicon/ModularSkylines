using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace ModularSkylines
{
    // A general AI for handling level up data and process. inherit and override this to customize. Selected per-asset or default to this.
    public class LevelAI
    {
        public BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data, BuildingInfo info)
        {
            Randomizer randomizer = new Randomizer((int)buildingID);
            ItemClass.Level level = info.m_class.m_level + 1;
            return Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref randomizer, info.m_class.m_service, info.m_class.m_subService, level, data.Width, data.Length, info.m_zoningMode);
        }
        public void LevelUpStart(ushort buildingID, ref Building buildingData, BuildingInfo m_info)
        {
            buildingData.m_frame0.m_constructState = 0;
            buildingData.m_frame1.m_constructState = 0;
            buildingData.m_frame2.m_constructState = 0;
            buildingData.m_frame3.m_constructState = 0;
            Building.Flags flags = buildingData.m_flags;
            flags |= Building.Flags.Upgrading;
            flags &= ~Building.Flags.Completed;
            flags &= ~Building.Flags.LevelUpEducation;
            flags &= ~Building.Flags.LevelUpLandValue;
            buildingData.m_flags = flags;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            instance.UpdateBuildingRenderer(buildingID, true);
            EffectInfo levelupEffect = instance.m_properties.m_levelupEffect;
            if (levelupEffect != null)
            {
                InstanceID instance2 = default(InstanceID);
                instance2.Building = buildingID;
                Vector3 pos;
                Quaternion q;
                buildingData.CalculateMeshPosition(out pos, out q);
                Matrix4x4 matrix = Matrix4x4.TRS(pos, q, Vector3.one);
                EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(matrix, m_info.m_lodMeshData);
                Singleton<EffectManager>.instance.DispatchEffect(levelupEffect, instance2, spawnArea, Vector3.zero, 0f, 1f, instance.m_audioGroup);
            }
            Vector3 position = buildingData.m_position;
            position.y += m_info.m_size.y;
            Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LevelUp, position, 1f);
            Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
        }
    }
}
