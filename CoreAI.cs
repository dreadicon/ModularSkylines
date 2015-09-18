using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using ModularSkylines.VanillaModules;
using System.Threading.Tasks;
using UnityEngine;

namespace ModularSkylines
{
    //TODO: rework simulation ticking to only recalculate consumption/use values every few ticks or on state change.
    public sealed class CoreAI : CommonBuildingAI
    {
        // Last known building color
        public Color color;
        // Only one method of rendering decoration placement may be used (vanilla is growable vs ploppable)
        public DecorationAI decorationAI;

        public BuildingConfig behaviorConfig;

        public bool playerControlled;
        public bool isGrowable;
        //Common cached building simulation information & 'dirty' flag for when state has changed.
        public bool commonConsumptionUpdate;
        public CommonConsumption commonConsumption;
        public bool occupantsUpdated;
        public Occupants occupants;

        //All delegate data sets for building
        private List<DelegateData> BuildingModules;

        //Simulation Modules
        private List<SimulationDelegate> OnSimulation;
        private List<SimulationDelegate> OnActiveSimulation;

        //Building lifecycle modules
        private BuildingDelegate OnCreateBuilding;
        private BuildingDelegate OnReleaseBuilding;
        private BuildingDelegate OnBuildingCompleted;
        private BuildingDelegate OnBuildingUpgraded;
        private BuildingVersionDelegate OnBuildingLoaded;
        private BuildingDelegate OnStartUpgrade;
        //Note: might not nees this one. Investigate later.
        //private BuildingDelegate GetUpgradeInfo;

        //Building state update modules
        private BuildingDelegate OnOccupantsChanged; //NOTE: this may not impact enough be useful, but can't hurt
        private BuildingDelegate OnDistrictChanged; 
        private BuildingDelegate OnVisitorsChanged; //NOTE: this may change too often to be a useful.

        // Modules which may return a color for the building.
        private List<BuildingColorDelegate> ColorModules;

        //Bag for custom properties.
        //TODO: reworks this as an array lookup system whose keys are generated at runtime from config data.
        //Note: I used RuntimeTypeHandle as typeof(T).TypeHandle is substantially faster than just typeof(T)
        private Dictionary<RuntimeTypeHandle, DataModule> dataDictionary;

        public T GetData<T>() where T : DataModule<T>, new()
        {
            DataModule data;
            if(dataDictionary.TryGetValue(typeof(T).TypeHandle, out data))
                return (T)data;
            return new T();
        }

        public bool TryGetData<T>(out T module) where T : DataModule<T>
        {
            DataModule data;
            if(dataDictionary.TryGetValue(typeof(T).TypeHandle, out data))
            {
                module = (T)data;
                return true;
            }
            module = null;
            return false;
        }

        public void SetData<T>(T data) where T : DataModule<T>
        {
            data.DataAltered = true;
            dataDictionary[typeof (T).TypeHandle] = data;
        }

        //This is an alternate Get method for the data bag which invokes associated delegates. Useful for lazy computation.
        //Might later rework this somehow to, rather than have fixed invokes per module, allow other modules to mark
        //other modules whose values are dependent on them as such.
        private Dictionary<RuntimeTypeHandle, object> lazyGetInvoke;
        public T GetDataEvent<T>() where T : DataModule<T>, new()
        {
            T data;
            if(TryGetData<T>(out data))
            {
                if(data.DataAltered)
                {
                    object delObj;
                    if (lazyGetInvoke.TryGetValue(typeof(T).TypeHandle, out delObj))
                    {
                        var del = (DataModuleEvent<T>)delObj;
                        data.DataAltered = false;
                        del(this, data);
                    }
                }
                return data;   
            }
            return new T();
        }

        //TODO: remove the Attributes and implement own custom UI system
        public Dictionary<short, short[]> behaviorIDLookup;
        public bool CheckBehavior(short id)
        {
            if (behaviorIDLookup.ContainsKey(id)) return true;
            return false;
        }
        public bool CheckBehavior(short id, short sub)
        {
            short[] subgroup;
            if (behaviorIDLookup.TryGetValue(id, out subgroup))
                if (subgroup.Contains(sub)) return true;
            return false;
        }

        public int m_fireSize = 127;

        [CustomizableProperty("Fire Hazard", "Gameplay common")]
        public int m_fireHazard = 1;

        [CustomizableProperty("Fire Tolerance", "Gameplay common")]
        public int m_fireTolerance = 20;

        //The exact execution of GetColor is handled by a separate class, which this class stores a reference to.
        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
        {
            foreach (var module in ColorModules)
            {
                module(buildingID, ref data, infoMode, ref color);
            }
            return color;
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

        private string GetLocalizedStatusInactive(ushort buildingID, ref Building data)
        {
            return Locale.Get("BUILDING_STATUS_NOT_OPERATING");
        }

        private string GetLocalizedStatusActive(ushort buildingID, ref Building data)
        {
            if ((data.m_flags & Building.Flags.RateReduced) != Building.Flags.None)
            {
                return Locale.Get("BUILDING_STATUS_REDUCED");
            }
            return Locale.Get("BUILDING_STATUS_DEFAULT");
        }

        public override int GetResourceRate(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource)
        {
            if (resource == ImmaterialResourceManager.Resource.NoisePollution) return GetData<Noise>().m_noiseAccumulation;
            return base.GetResourceRate(buildingID, ref data, resource);
        }

        //Add module to fiddle with these every time they are referenced (even though I don't think it's beneficial)
        public void GetConsumptionRates(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
        {
            electricityConsumption = commonConsumption.electricityConsumption;
            waterConsumption = commonConsumption.waterConsumption;
            sewageAccumulation = commonConsumption.sewageAccumulation;
            garbageAccumulation = commonConsumption.garbageAccumulation;
            incomeAccumulation = commonConsumption.netIncome;
        }

        // TODO: add an event to this
        public override bool GetFireParameters(ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance)
        {
            fireHazard = m_fireHazard;
            fireSize = m_fireSize;
            fireTolerance = m_fireTolerance;
            return this.m_fireHazard != 0;
        }

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            if(playerControlled)
            {
                //TODO: implement this properly using modules.
                GetData<PublicBuilding>().m_createPassMilestone?.SetPrefab(this.m_info);
            }
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            OnCreateBuilding(buildingID, ref data, this);

            //WARNING: this could break things if a non-standard service type is used, as the manager initializes the array to 12 elements with no safe expansion.
            //TODO: either fix the manager, or handle this outside vanilla manager.
            foreach (var service in behaviorConfig.GetUniqueTypes())
                Singleton<BuildingManager>.instance.AddServiceBuilding(buildingID, (ItemClass.Service)service);
            //Note: for now, I am not allowing buildings to have 'passengers' or be vehicles :P
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, occupants.maxHomeCount, occupants.totalWorkers, GetData<Tourists>().maxTourists, 0, GetData<Education>().m_studentCount);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            
            OnBuildingLoaded(buildingID, ref data, version, this);
            
            EnsureCitizenUnits(buildingID, ref data, occupants.maxHomeCount, occupants.totalWorkers, GetData<Tourists>().maxTourists, GetData<Education>().m_studentCount);
            
            //WARNING: see CreateBuilding comments for details on potential problems with this
            foreach(var service in behaviorConfig.GetUniqueTypes())
                Singleton<BuildingManager>.instance.AddServiceBuilding(buildingID, (ItemClass.Service)service);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            OnReleaseBuilding(buildingID, ref data, this);
            //WARNING: see CreateBuilding comments for details on potential problems with this
            foreach (var service in behaviorConfig.GetUniqueTypes())
                Singleton<BuildingManager>.instance.RemoveServiceBuilding(buildingID, (ItemClass.Service)service);
            base.ReleaseBuilding(buildingID, ref data);
        }

        protected override void BuildingCompleted(ushort buildingID, ref Building buildingData)
        {
            base.BuildingCompleted(buildingID, ref buildingData);
            OnBuildingCompleted(buildingID, ref buildingData, this);
            
            // TODO: replace onCompleteMessage with module, allow for extension maybe. 
            PublicBuilding publicData;
            if(TryGetData<PublicBuilding>(out publicData))
                Singleton<MessageManager>.instance.TryCreateMessage(publicData.m_onCompleteMessage, Singleton<MessageManager>.instance.GetRandomResidentID());
        }



        public override void BuildingUpgraded(ushort buildingID, ref Building data)
        {
            if (isGrowable)
            {
                OnBuildingUpgraded(buildingID, ref data, this);
            }

            base.EnsureCitizenUnits(buildingID, ref data, occupants.maxHomeCount, occupants.totalWorkers, GetData<Tourists>().maxTourists, GetData<Education>().m_studentCount);
        }

        protected override int GetConstructionTime()
        {
            return GetData<Growth>().m_constructionTime;
        }

        // TODO: add extensibility
        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {
            if (isGrowable)
            {
                Randomizer randomizer = new Randomizer((int)buildingID);
                ItemClass.Level level = this.m_info.m_class.m_level + 1;
                return Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref randomizer, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, level, data.Width, data.Length, this.m_info.m_zoningMode);
            }
                
            return null;
        }

        // TODO: this needs a module and/or some code.
        protected void StartUpgrading(ushort buildingID, ref Building buildingData)
        {
            if (OnStartUpgrade != null && isGrowable)
                OnStartUpgrade(buildingID, ref buildingData, this);
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);

            foreach (var module in OnSimulation)
            {
                module(buildingID, ref buildingData, ref frameData, this);
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

            foreach (var module in OnActiveSimulation)
            {
                module(buildingID, ref buildingData, ref frameData, this);
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
            //this.CalculateWorkplaceCount(new Randomizer((int)buildingID), buildingData.Width, buildingData.Length, out num, out num2, out num3, out num4);
            num = this.occupants.m_workPlaceCount0;
            num2 = this.occupants.m_workPlaceCount1;
            num3 = this.occupants.m_workPlaceCount2;
            num4 = this.occupants.m_workPlaceCount3;
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

        //added conditional so as to confine ranges only for growables.
        //TODO: make this configurable in some fashion, esp. for growables.
        public override void GetWidthRange(out int minWidth, out int maxWidth)
        {
            if (!playerControlled)
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
            if (!playerControlled)
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

        //TODO: set this up as a lazy get.
        public void GetPollutionRates(int productionRate, out int groundPollution, out int noisePollution)
        {
            groundPollution = 0;
            noisePollution = 0;
        }




        //New simulation events

        //This will be called each time the permanent occupants/workers of a location have changed.
        public void OnOccupantChanged ()
        {

        }
        //This will be called each time the current citizens and/or tourists at a location change (workers, shoppers, tourists, residents, etc.)
        public void OnVisitorChanged ()
        {

        }

    }
}
