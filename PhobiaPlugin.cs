﻿using BepInEx;
using KeepThatAwayFromMe;
using MoreSlugcats;
using System.Collections.Generic;
using System.Reflection;
using Watcher;

#region Assembly attributes

[assembly: AssemblyVersion(PhobiaPlugin.PLUGIN_VERSION)]
[assembly: AssemblyFileVersion(PhobiaPlugin.PLUGIN_VERSION)]
[assembly: AssemblyTitle(PhobiaPlugin.PLUGIN_NAME + " (" + PhobiaPlugin.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(PhobiaPlugin.PLUGIN_NAME)]

#endregion Assembly attributes

namespace KeepThatAwayFromMe
{
    [BepInPlugin(PhobiaPlugin.PLUGIN_ID, PhobiaPlugin.PLUGIN_NAME, PhobiaPlugin.PLUGIN_VERSION)]
    [BepInProcess("RainWorld.exe")]
    public class PhobiaPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "com.rainworldgame.keepthatawayfromme.plugin";
        public const string PLUGIN_NAME = "KeepThatAwayFromMe";
        public const string PLUGIN_VERSION = "1.1.0.5";

        public void Awake()
        {
            instance = this;
            On.RainWorld.OnModsInit += Init;
            //On.RainWorld.PostModsDisabledEnabled += AtOnModsSwitched;
        }

        private static PhobiaOption poi;

        private static void Init(On.RainWorld.orig_OnModsInit orig, RainWorld rw) // On.RainWorld.orig_OnModsInit orig, RainWorld rw
        {
            orig(rw);
            if (init) return;
            init = true;
            poi = new PhobiaOption();
            InitializeConfig(poi);
            MachineConnector.SetRegisteredOI("keepthatawayfromme", poi);
            PhobiaScript.Patch();
            instance.Logger.LogMessage("KeepThatAwayFromMe is Intilaized.");
            //foreach (KeyValuePair<string, ConfigurableBase> p in poi.config.configurables)
            //    instance.Logger.LogInfo($"{p.Value.key}: {ValueConverter.ConvertToString(p.Value.BoxedValue, p.Value.settingType)} {p.Value.defaultValue}");
        }

        /*
        private static void AtOnModsSwitched(On.RainWorld.orig_PostModsDisabledEnabled orig, RainWorld rw)
        {
            orig(rw);
            InitializeConfig(poi);
        }
        */

        private static void InitializeConfig(PhobiaOption oi)
        {
            // Initialize Creature Types
            string[] allNames = ExtEnumBase.GetNames(typeof(CreatureTemplate.Type));
            List<string> okayNames = new List<string>();
            for (int i = 0; i < allNames.Length; i++)
            { if (IsValidCritType(allNames[i])) { okayNames.Add(allNames[i]); } }
            allCritTypes = new CreatureTemplate.Type[okayNames.Count];
            critTypesBan = new Configurable<bool>[okayNames.Count];
            for (int j = 0; j < okayNames.Count; j++)
            {
                allCritTypes[j] = new CreatureTemplate.Type(okayNames[j], false);
                critTypesBan[j] = oi.config.Bind(PhobiaOption.GenerateCritKey(allCritTypes[j]), false);
            }
            instance.Logger.LogInfo($"Crit Types: {string.Join(", ", okayNames)}");

            // Initialize Object Types
            allNames = ExtEnumBase.GetNames(typeof(AbstractPhysicalObject.AbstractObjectType));
            okayNames.Clear();
            for (int i = 0; i < allNames.Length; i++)
            { if (IsValidObjType(allNames[i])) { okayNames.Add(allNames[i]); } }
            allObjTypes = new AbstractPhysicalObject.AbstractObjectType[okayNames.Count];
            objTypesBan = new Configurable<bool>[okayNames.Count];
            for (int j = 0; j < okayNames.Count; j++)
            {
                allObjTypes[j] = new AbstractPhysicalObject.AbstractObjectType(okayNames[j], false);
                objTypesBan[j] = oi.config.Bind(PhobiaOption.GenerateObjKey(allObjTypes[j]), false);
            }
            instance.Logger.LogInfo($"Item Types: {string.Join(", ", okayNames)}");
        }

        private static bool init = false;

        public static PhobiaPlugin instance;

        #region Creatures

        public static CreatureTemplate.Type[] allCritTypes = new CreatureTemplate.Type[0];
        public static HashSet<CreatureTemplate.Type> bannedCritTypes = new HashSet<CreatureTemplate.Type>();
        public static Configurable<bool>[] critTypesBan = new Configurable<bool>[0];

        public static bool IsCritBanned(CreatureTemplate ct) => bannedCritTypes.Contains(ct.type);

        public static bool IsValidCritType(CreatureTemplate.Type type)
        {
            if (type.Index < 0) return false;
            if (type == CreatureTemplate.Type.StandardGroundCreature) return false;
            // if (type == CreatureTemplate.Type.LizardTemplate) return false;
            if (type == CreatureTemplate.Type.Slugcat) return false;
            // if (type == CreatureTemplate.Type.Overseer) return false;
            // if (type == CreatureTemplate.Type.TempleGuard) return false;
            if (type.value.ToLower().Contains("template")) return false;
            if (!ModManager.MSC) return true;
            if (type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) return false;
            return true;
        }

        public static bool IsValidCritType(string name) => IsValidCritType(new CreatureTemplate.Type(name, false));

        #endregion Creatures

        #region Objects

        public static AbstractPhysicalObject.AbstractObjectType[] allObjTypes = new AbstractPhysicalObject.AbstractObjectType[0];
        public static HashSet<AbstractPhysicalObject.AbstractObjectType> bannedObjTypes = new HashSet<AbstractPhysicalObject.AbstractObjectType>();
        public static Configurable<bool>[] objTypesBan = new Configurable<bool>[0];

        public static bool IsObjBanned(AbstractPhysicalObject obj) => bannedObjTypes.Contains(obj.type);

        public static bool IsValidObjType(AbstractPhysicalObject.AbstractObjectType type)
        {
            if (type.Index < 0) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.Creature) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.Oracle) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.AttachedBee) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.DartMaggot) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.CollisionField) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.BlinkingFlower) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.LobeTree) return false;
            if (type == AbstractPhysicalObject.AbstractObjectType.Pomegranate) return ModManager.Watcher;
            //if (type == AbstractPhysicalObject.AbstractObjectType.VoidSpawn) return false;
            if (ModManager.MSC)
            {
                if (type == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl) return false;
                if (type == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl) return false;
                if (type == MoreSlugcatsEnums.AbstractObjectType.EnergyCell) return false;
                if (type == MoreSlugcatsEnums.AbstractObjectType.Bullet) return false;
            }
            if (ModManager.Watcher)
            {
                if (type == WatcherEnums.AbstractObjectType.BallToy) return false;
                if (type == WatcherEnums.AbstractObjectType.SoftToy) return false;
                if (type == WatcherEnums.AbstractObjectType.SpinToy) return false;
                if (type == WatcherEnums.AbstractObjectType.WeirdToy) return false;
                if (type == WatcherEnums.AbstractObjectType.Prince) return false;
                if (type == WatcherEnums.AbstractObjectType.PrinceBulb) return false;
                if (type == WatcherEnums.AbstractObjectType.RippleSpawn) return false;
            }
            return true;
        }

        public static bool IsValidObjType(string name) => IsValidObjType(new AbstractPhysicalObject.AbstractObjectType(name, false));

        #endregion Objects
    }
}