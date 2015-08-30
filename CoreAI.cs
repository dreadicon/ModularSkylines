using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModularSkylines
{
    //TODO: rework simulation ticking to only recalculate consumption/use values every few ticks or on state change.
    public class CoreAI : CommonBuildingAI
    {
        public ColorAI colorAI;
        public DecorationAI decorationAI;
        public FireAI fireAI;
        public LevelAI levelAI;
        public bool playerControlled;
        public bool baseCosts;
        private bool isGrowable;

        //Lists of modules for building based on type of resource they alter/interact with.
        private List<BuildingModule> BuildingModules;
        private List<SimulationBuildingModule> SimulationBuildingModules; 

        private List<IImmaterialResourceModule> ImmaterialResourceModules;
        private List<INaturalResourceModule> NaturalResourceModules;
        private List<IEconomyResourceModule> EconomyResourceModules;
        private List<IWaterModule> WaterModules;
        private List<IElectricModule> ElectricModules;
        private List<IGarbageModule> GarbageModules;

        private Dictionary<NaturalResourceManager.Resource, List<INaturalResourceModule>> NatualResourceMapping;
        private Dictionary<ImmaterialResourceManager.Resource, List<IImmaterialResourceModule>> ImmaterialResourceMapping;
        private Dictionary<string, List<BuildingModule>> CustomResourceMapping; 
         

        //Player Building AI properties
        //TODO: remove the Attributes and implement own custom UI system
        
        public MessageInfo m_onCompleteMessage;

        public ManualMilestone m_createPassMilestone;

        public int m_fireSize = 127;

        public int m_maxLevel = 1;

        [CustomizableProperty("Construction Cost", "Gameplay common")]
        public int m_constructionCost = 1000;

        [CustomizableProperty("Maintenance Cost", "Gameplay common")]
        public int m_maintenanceCost = 100;

        [CustomizableProperty("Electricity Consumption", "Electricity")]
        public int m_electricityConsumption = 10;

        [CustomizableProperty("Water Consumption", "Water")]
        public int m_waterConsumption = 10;

        [CustomizableProperty("Sewage Accumulation", "Water")]
        public int m_sewageAccumulation = 10;

        [CustomizableProperty("Garbage Accumulation", "Gameplay common")]
        public int m_garbageAccumulation = 10;

        [CustomizableProperty("Fire Hazard", "Gameplay common")]
        public int m_fireHazard = 1;

        [CustomizableProperty("Fire Tolerance", "Gameplay common")]
        public int m_fireTolerance = 20;

        //Private Building AI properties
        [CustomizableProperty("Construction Time")]
        public int m_constructionTime = 30;

        //The exact execution of GetColor is handled by a separate class, which this class stores a reference to.
        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
        {
            return colorAI.GetColor(buildingID, ref data, infoMode);
        }

        //Added accessor to base color method
        public Color BaseColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
        {
            return base.GetColor(buildingID, ref data, infoMode);
        }

        //Public accessor added
        public bool GetShowConsumption(ushort buildingID, ref Building data)
        {
            return (data.m_flags & (Building.Flags.Completed | Building.Flags.Abandoned | Building.Flags.BurnedDown)) == Building.Flags.Completed;
        }

        //TODO: this needs to be more modular; need to figure something out to have procedural condition checking based on aspects present
        protected override bool ShowConsumption(ushort buildingID, ref Building data)
        {
            //Note: the Player version differs, as ploppables have on/off toggles using the 'Active' flag.
            
            return (data.m_flags & (Building.Flags.Completed | Building.Flags.Abandoned | Building.Flags.BurnedDown)) == Building.Flags.Completed;
        }

        public override string GetLocalizedStatus(ushort buildingID, ref Building data)
        {
            if ((data.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
            {
                return Locale.Get("BUILDING_STATUS_ABANDONED");
            }
            if ((data.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
            {
                return Locale.Get("BUILDING_STATUS_BURNED");
            }
            if ((data.m_flags & Building.Flags.Upgrading) != Building.Flags.None)
            {
                return Locale.Get("BUILDING_STATUS_UPGRADING");
            }
            if ((data.m_flags & Building.Flags.Completed) == Building.Flags.None)
            {
                return Locale.Get("BUILDING_STATUS_UNDER_CONSTRUCTION");
            }
            if ((data.m_flags & Building.Flags.Active) == Building.Flags.None)
            {
                return this.GetLocalizedStatusInactive(buildingID, ref data);
            }
            if (data.m_productionRate == 0 && playerControlled == true) //TODO: better implement growable aspects vs. player-controlled aspects such as opperation toggling.
            {
                return Locale.Get("BUILDING_STATUS_OFF");
            }
            return this.GetLocalizedStatusActive(buildingID, ref data);
        }

        protected virtual string GetLocalizedStatusInactive(ushort buildingID, ref Building data)
        {
            return Locale.Get("BUILDING_STATUS_NOT_OPERATING");
        }

        protected virtual string GetLocalizedStatusActive(ushort buildingID, ref Building data)
        {
            if ((data.m_flags & Building.Flags.RateReduced) != Building.Flags.None)
            {
                return Locale.Get("BUILDING_STATUS_REDUCED");
            }
            return Locale.Get("BUILDING_STATUS_DEFAULT");
        }

        //Set up to iterate over each relevant module for the building, running the appropriate calculations as needed.
        public override int GetResourceRate(ushort buildingID, ref Building data, NaturalResourceManager.Resource resource)
        {
            int rate = base.GetResourceRate(buildingID, ref data, resource);
            foreach (var module in NaturalResourceModules)
            {
                module.GetNaturalResourceRate(ref rate, buildingID, ref data, resource);
            }
            return rate;
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource)
        {
            int rate = base.GetResourceRate(buildingID, ref data, resource);
            foreach (var module in ImmaterialResourceModules)
            {
                module.GetImmaterialResourceRate(ref rate, buildingID, ref data, resource);
            }
            return rate;
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, EconomyManager.Resource resource)
        {
            int rate = base.GetResourceRate(buildingID, ref data, resource);
            foreach (var module in EconomyResourceModules)
            {
                module.GetEconomyResourceRate(ref rate, buildingID, ref data, resource);
            }
            return rate;
        }

        public override int GetElectricityRate(ushort buildingID, ref Building data)
        {
            int rate = base.GetElectricityRate(buildingID, ref data);
            foreach (var module in ElectricModules)
            {
                module.GetElectricRate(ref rate, buildingID, ref data);
            }
            /*
            // PlayerBuildingAI code
            int productionRate = (int)data.m_productionRate;
            int budget = Singleton<EconomyManager>.instance.GetBudget(this.m_info.m_class);
            productionRate = PlayerBuildingAI.GetProductionRate(productionRate, budget);
            return -(productionRate * this.m_electricityConsumption / 100);
            */
            return rate;
        }

        public override int GetWaterRate(ushort buildingID, ref Building data)
        {
            int rate = base.GetWaterRate(buildingID, ref data);
            foreach (var module in WaterModules)
            {
                module.GetWaterRate(ref rate, buildingID, ref data);
            }
            /*
            // PlayerBuildingAI code
            int productionRate = (int)data.m_productionRate;
            int budget = Singleton<EconomyManager>.instance.GetBudget(this.m_info.m_class);
            productionRate = PlayerBuildingAI.GetProductionRate(productionRate, budget);
            return -(productionRate * this.m_waterConsumption / 100);
            */
            return rate;
        }

        public override int GetGarbageRate(ushort buildingID, ref Building data)
        {
            int rate = base.GetGarbageRate(buildingID, ref data);
            foreach (var module in GarbageModules)
            {
                module.GetGarbageRate(ref rate, buildingID, ref data);
            }
            /*
            // PlayerBuildingAI code
            int productionRate = (int)data.m_productionRate;
            int budget = Singleton<EconomyManager>.instance.GetBudget(this.m_info.m_class);
            productionRate = PlayerBuildingAI.GetProductionRate(productionRate, budget);
            return productionRate * this.m_garbageAccumulation / 100;
            */
            return rate;
        }

        // Fire parameters for size/tolerance set on object & updated via modules. hazard obtained via specific Module
        public override bool GetFireParameters(ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance)
        {
            fireAI.GetFireHazard(buildingID, ref buildingData, out fireHazard);
            fireSize = m_fireSize;
            fireTolerance = m_fireTolerance;
            return this.m_fireHazard != 0;
        }

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            if (this.m_createPassMilestone != null)
            {
                this.m_createPassMilestone.SetPrefab(this.m_info);
            }
        }

        // Allow modules to handle actions for building lifecycle.
        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            /*
            // PrivateBuildingAI code
            int num;
            int num2;
            int num3;
            int num4;
            this.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
            int workCount = num + num2 + num3 + num4;
            int homeCount = this.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
            int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount, workCount, visitCount, 0, 0);
            */

            
            foreach (var module in BuildingModules)
            {
                module.OnCreateBuilding(buildingID, ref data);
            }
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            /*
            // PrivateBuildingAI code
            base.BuildingLoaded(buildingID, ref data, version);
            int num;
            int num2;
            int num3;
            int num4;
            this.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
            int workCount = num + num2 + num3 + num4;
            int homeCount = this.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
            int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
            
            */
            BuildingModule.BuildingLoadInfo info = new BuildingModule.BuildingLoadInfo();

            foreach (var module in BuildingModules)
            {
                module.OnBuildingLoaded(buildingID, ref data, version, this);

            }
            base.EnsureCitizenUnits(buildingID, ref data, homeCount, workCount, visitCount, 0);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            base.ReleaseBuilding(buildingID, ref data);
            foreach (var module in BuildingModules)
            {
                module.OnReleaseBuilding(buildingID, ref data);
            }
        }

        protected override void BuildingCompleted(ushort buildingID, ref Building buildingData)
        {
            base.BuildingCompleted(buildingID, ref buildingData);
            foreach (var module in BuildingModules)
            {
                module.OnBuildingCompleted(buildingID, ref buildingData);
            }
            // PlayerBuildingAI code
            //Singleton<MessageManager>.instance.TryCreateMessage(this.m_onCompleteMessage, Singleton<MessageManager>.instance.GetRandomResidentID());
        }

        protected override int GetConstructionTime()
        {
            return this.m_constructionTime;
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);

            foreach (var module in SimulationBuildingModules)
            {
                module.OnSimulationStep(buildingID, ref buildingData, ref frameData);
            }

            //PrivateBuildingAI code
            /*
            if ((buildingData.m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None)
            {
                SimulationManager instance = Singleton<SimulationManager>.instance;
                if (buildingData.m_fireIntensity == 0 && instance.m_randomizer.Int32(10u) == 0 && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
                {
                    buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
                    if (!buildingData.CheckZoning(this.m_info.m_class.GetZone()))
                    {
                        buildingData.m_flags |= Building.Flags.Demolishing;
                        CoreAI.CheckNearbyBuildingZones(buildingData.m_position);
                        instance.m_currentBuildIndex += 1u;
                    }
                }
            }
            else if ((buildingData.m_flags & (Building.Flags.Abandoned | Building.Flags.Downgrading)) != Building.Flags.None && (buildingData.m_majorProblemTimer == 255 || (buildingData.m_flags & Building.Flags.Abandoned) == Building.Flags.None))
            {
                SimulationManager instance2 = Singleton<SimulationManager>.instance;
                ZoneManager instance3 = Singleton<ZoneManager>.instance;
                int num;
                switch (this.m_info.m_class.m_service)
                {
                    case ItemClass.Service.Residential:
                        num = instance3.m_actualResidentialDemand;
                        goto IL_164;
                    case ItemClass.Service.Commercial:
                        num = instance3.m_actualCommercialDemand;
                        goto IL_164;
                    case ItemClass.Service.Industrial:
                        num = instance3.m_actualWorkplaceDemand;
                        goto IL_164;
                    case ItemClass.Service.Office:
                        num = instance3.m_actualWorkplaceDemand;
                        goto IL_164;
                }
                num = 0;
            IL_164:
                if (instance2.m_randomizer.Int32(100u) < num && instance3.m_lastBuildIndex == instance2.m_currentBuildIndex)
                {
                    float num2 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(buildingData.m_position));
                    if (num2 <= buildingData.m_position.y)
                    {
                        ItemClass.SubService subService = this.m_info.m_class.m_subService;
                        ItemClass.Level level = ItemClass.Level.Level1;
                        if (this.m_info.m_class.m_service == ItemClass.Service.Industrial)
                        {
                            ZoneBlock.GetIndustryType(buildingData.m_position, out subService, out level);
                        }
                        int width = buildingData.Width;
                        int length = buildingData.Length;
                        BuildingInfo randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, this.m_info.m_class.m_service, subService, level, width, length, this.m_info.m_zoningMode);
                        if (randomBuildingInfo != null)
                        {
                            buildingData.m_flags |= Building.Flags.Demolishing;
                            float num3 = buildingData.m_angle + 1.57079637f;
                            if (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
                            {
                                num3 -= 1.57079637f;
                                length = width;
                            }
                            else if (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
                            {
                                num3 += 1.57079637f;
                                length = width;
                            }
                            ushort num4;
                            if (Singleton<BuildingManager>.instance.CreateBuilding(out num4, ref Singleton<SimulationManager>.instance.m_randomizer, randomBuildingInfo, buildingData.m_position, buildingData.m_angle, length, Singleton<SimulationManager>.instance.m_currentBuildIndex))
                            {
                                Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
                                switch (this.m_info.m_class.m_service)
                                {
                                    case ItemClass.Service.Residential:
                                        instance3.m_actualResidentialDemand = Mathf.Max(0, instance3.m_actualResidentialDemand - 5);
                                        break;
                                    case ItemClass.Service.Commercial:
                                        instance3.m_actualCommercialDemand = Mathf.Max(0, instance3.m_actualCommercialDemand - 5);
                                        break;
                                    case ItemClass.Service.Industrial:
                                        instance3.m_actualWorkplaceDemand = Mathf.Max(0, instance3.m_actualWorkplaceDemand - 5);
                                        break;
                                    case ItemClass.Service.Office:
                                        instance3.m_actualWorkplaceDemand = Mathf.Max(0, instance3.m_actualWorkplaceDemand - 5);
                                        break;
                                }
                            }
                            instance2.m_currentBuildIndex += 1u;
                        }
                    }
                }
            }
            */
        }

        //Used in PrivateBuildingAI simulation step; move to module later.
        private static void CheckNearbyBuildingZones(Vector3 position)
        {
            int num = Mathf.Max((int)((position.x - 35f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((position.z - 35f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((position.x + 35f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((position.z + 35f) / 64f + 135f), 269);
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = buildingGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        ushort nextGridBuilding = buildings.m_buffer[(int)num5].m_nextGridBuilding;
                        Building.Flags flags = buildings.m_buffer[(int)num5].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Demolishing)) == Building.Flags.Created)
                        {
                            BuildingInfo info = buildings.m_buffer[(int)num5].Info;
                            if (info != null && info.m_placementStyle == ItemClass.Placement.Automatic)
                            {
                                ItemClass.Zone zone = info.m_class.GetZone();
                                if (zone != ItemClass.Zone.None && (buildings.m_buffer[(int)num5].m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None && VectorUtils.LengthSqrXZ(buildings.m_buffer[(int)num5].m_position - position) <= 1225f)
                                {
                                    Building[] expr_198_cp_0 = buildings.m_buffer;
                                    ushort expr_198_cp_1 = num5;
                                    expr_198_cp_0[(int)expr_198_cp_1].m_flags = (expr_198_cp_0[(int)expr_198_cp_1].m_flags & ~Building.Flags.ZonesUpdated);
                                    if (!buildings.m_buffer[(int)num5].CheckZoning(zone))
                                    {
                                        Singleton<BuildingManager>.instance.ReleaseBuilding(num5);
                                    }
                                }
                            }
                        }
                        num5 = nextGridBuilding;
                        if (++num6 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);

            foreach (var module in SimulationBuildingModules)
            {
                module.OnActiveSimulationStep(buildingID, ref buildingData, ref frameData);
            }
            // PrivateBuildingAI code
            /*
            if ((buildingData.m_problems & Notification.Problem.MajorProblem) != Notification.Problem.None)
            {
                if (buildingData.m_fireIntensity == 0)
                {
                    buildingData.m_majorProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_majorProblemTimer + 1));
                    if (buildingData.m_majorProblemTimer >= 64 && !Singleton<BuildingManager>.instance.m_abandonmentDisabled)
                    {
                        buildingData.m_majorProblemTimer = 192;
                        buildingData.m_flags &= ~Building.Flags.Active;
                        buildingData.m_flags |= Building.Flags.Abandoned;
                        buildingData.m_problems = (Notification.Problem.FatalProblem | (buildingData.m_problems & ~Notification.Problem.MajorProblem));
                        base.RemovePeople(buildingID, ref buildingData);
                        this.BuildingDeactivated(buildingID, ref buildingData);
                        Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
                    }
                }
            }
            else
            {
                buildingData.m_majorProblemTimer = 0;
            }
            */
        }

        //TODO: Maybe move this logic into it's own module later?
        public int HandleWorkers(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount)
        {
            int b = 0;
            base.GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            int num;
            int num2;
            int num3;
            int num4;
            this.CalculateWorkplaceCount(new Randomizer((int)buildingID), buildingData.Width, buildingData.Length, out num, out num2, out num3, out num4);
            workPlaceCount = num + num2 + num3 + num4;
            if (buildingData.m_fireIntensity == 0)
            {
                base.HandleWorkPlaces(buildingID, ref buildingData, num, num2, num3, num4, ref behaviour, aliveWorkerCount, totalWorkerCount);
                if (aliveWorkerCount != 0 && workPlaceCount != 0)
                {
                    int num5 = (behaviour.m_efficiencyAccumulation + aliveWorkerCount - 1) / aliveWorkerCount;
                    b = 2 * num5 - 200 * num5 / ((100 * aliveWorkerCount + workPlaceCount - 1) / workPlaceCount + 100);
                }
            }
            Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoWorkers | Notification.Problem.NoEducatedWorkers);
            int num6 = (num4 * 300 + num3 * 200 + num2 * 100) / (workPlaceCount + 1);
            int num7 = (behaviour.m_educated3Count * 300 + behaviour.m_educated2Count * 200 + behaviour.m_educated1Count * 100) / (aliveWorkerCount + 1);
            if (aliveWorkerCount < workPlaceCount >> 1)
            {
                buildingData.m_workerProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_workerProblemTimer + 1));
                if (buildingData.m_workerProblemTimer >= 128)
                {
                    problem = Notification.AddProblems(problem, Notification.Problem.NoWorkers | Notification.Problem.MajorProblem);
                }
                else if (buildingData.m_workerProblemTimer >= 64)
                {
                    problem = Notification.AddProblems(problem, Notification.Problem.NoWorkers);
                }
            }
            else if (num7 < num6 - 50)
            {
                buildingData.m_workerProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_workerProblemTimer + 1));
                if (buildingData.m_workerProblemTimer >= 128)
                {
                    problem = Notification.AddProblems(problem, Notification.Problem.NoEducatedWorkers | Notification.Problem.MajorProblem);
                }
                else if (buildingData.m_workerProblemTimer >= 64)
                {
                    problem = Notification.AddProblems(problem, Notification.Problem.NoEducatedWorkers);
                }
            }
            else
            {
                buildingData.m_workerProblemTimer = 0;
            }
            buildingData.m_problems = problem;
            return Mathf.Max(1, b);
        }

        // Logic for Get method moved to dedicated LevelAI class.
        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {
            if (m_maxLevel != -1)
                return levelAI.GetUpgradeInfo(buildingID, ref data, this.m_info);
            return null;
        }

        public override void BuildingUpgraded(ushort buildingID, ref Building data)
        {
            if (m_maxLevel != -1)
            {
                foreach (var module in BuildingModules)
                {
                    module.OnBuildingUpgraded(buildingID, ref data);
                }
            }
            int num;
            int num2;
            int num3;
            int num4;
            this.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
            int workCount = num + num2 + num3 + num4;
            int homeCount = this.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
            int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
            base.EnsureCitizenUnits(buildingID, ref data, homeCount, workCount, visitCount, 0);
        }

        //as above
        protected void StartUpgrading(ushort buildingID, ref Building buildingData)
        {
            if (levelAI != null && m_maxLevel != -1)
                levelAI.LevelUpStart(buildingID, ref buildingData, this.m_info);
            

        }

        //added conditional so as to confine ranges only for growables.
        public override void GetWidthRange(out int minWidth, out int maxWidth)
        {
            if (isGrowable)
            {
                minWidth = 1;
                maxWidth = 4;
            }
            else
            {
                base.GetWidthRange(out minWidth, out maxWidth);
            }
        }

        public override void GetLengthRange(out int minLength, out int maxLength)
        {
            if (isGrowable)
            {
                minLength = 1;
                maxLength = 4;
            }
            else
            {
                base.GetLengthRange(out minLength, out maxLength);
            }
        }
        
        //TODO: Verify that a simple IF statement actually works and editor assets can cross over from one to the other.
        public override void GetDecorationArea(out int width, out int length, out float offset)
        {
            decorationAI.GetDecorationArea(out width, out length, out offset, this.m_info);
        }

        public override void GetDecorationDirections(out bool negX, out bool posX, out bool negZ, out bool posZ)
        {
            decorationAI.GetDecorationDirections(out negX, out posX, out negZ, out posZ, this.m_info);
        }

        //Not sure I need these....
        public virtual int CalculateHomeCount(Randomizer r, int width, int length)
        {
            return 0;
        }

        public virtual int CalculateVisitplaceCount(Randomizer r, int width, int length)
        {
            return 0;
        }

        public virtual void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            level0 = 0;
            level1 = 0;
            level2 = 0;
            level3 = 0;
        }

        public virtual int CalculateProductionCapacity(Randomizer r, int width, int length)
        {
            return 0;
        }

        public virtual void GetConsumptionRates(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
        {
            electricityConsumption = 0;
            waterConsumption = 0;
            sewageAccumulation = 0;
            garbageAccumulation = 0;
            incomeAccumulation = 0;
        }

        public virtual void GetPollutionRates(int productionRate, out int groundPollution, out int noisePollution)
        {
            groundPollution = 0;
            noisePollution = 0;
        }

    }
}
