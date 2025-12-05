using ABN;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Steamworks;
using static DysonSphereProgramMenu;




namespace DysonSphereProgramMenuMod
{

    public class MyPatcher
    {
        public static void ApplyPatches()
        {
            var harmony = new Harmony("com.DysonSphereProgramMenu.patch");

            try
            {
                // Patch für EjectorComponent.InternalUpdate (Prefix)
                MethodInfo ejectorMethod = AccessTools.Method(typeof(EjectorComponent), "InternalUpdate", new Type[] { typeof(float), typeof(long), typeof(DysonSwarm), typeof(AstroData[]), typeof(AnimData[]), typeof(int[]) });
                MethodInfo ejectorPrefix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.EjectorPrefix));
                MethodInfo ejectorPostfix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.EjectorPostfix));
                harmony.Patch(ejectorMethod, prefix: new HarmonyMethod(ejectorPrefix), postfix: new HarmonyMethod(ejectorPostfix));

                // Patch für SiloComponent.InternalUpdate (Prefix)
                MethodInfo rocketMethod = AccessTools.Method(typeof(SiloComponent), "InternalUpdate", new Type[] { typeof(float), typeof(DysonSphere), typeof(AnimData[]), typeof(int[]) });
                MethodInfo rocketPrefix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.RocketPrefix));
                MethodInfo rocketPostfix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.RocketPostfix));
                harmony.Patch(rocketMethod, prefix: new HarmonyMethod(rocketPrefix), postfix: new HarmonyMethod(rocketPostfix));

                // Patch für EjectorComponent.Export (Prefix)
                MethodInfo ejectorExportMethod = AccessTools.Method(typeof(EjectorComponent), "Export", new Type[] { typeof(BinaryWriter) });
                MethodInfo ejectorExportPrefix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.EjectorExportPrefix));
                MethodInfo ejectorExportPostfix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.EjectorExportPostfix));
                harmony.Patch(ejectorExportMethod, prefix: new HarmonyMethod(ejectorExportPrefix), postfix: new HarmonyMethod(ejectorExportPostfix));

                // Patch für SiloComponent.Export (Prefix)
                MethodInfo rocketExportMethod = AccessTools.Method(typeof(SiloComponent), "Export", new Type[] { typeof(BinaryWriter) });
                MethodInfo rocketExportPrefix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.RocketExportPrefix));
                MethodInfo rocketExportPostfix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.RocketExportPostfix));
                harmony.Patch(rocketExportMethod, prefix: new HarmonyMethod(rocketExportPrefix), postfix: new HarmonyMethod(rocketExportPostfix));
                DroneComponent_InternalUpdate_Patch.ApplyPatch(harmony);
                harmony.PatchAll();

            }
            catch (Exception ex)
            {
                Debug.LogError("Fehler beim Anwenden der Patches: " + ex.Message);
            }
        }
    }
    public static class Patches
    {
        static bool DebugMode = true;

        private static readonly Dictionary<int, int> EjectorOriginalTimes = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> EjectorUnmodifiedTimes = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> SiloOriginalTimes = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> SiloUnmodifiedTimes = new Dictionary<int, int>();

        public static void EjectorPrefix(ref EjectorComponent __instance, ref float power, long tick, DysonSwarm swarm, AstroData[] astroPoses, AnimData[] animPool, int[] consumeRegister)
        {
            EjectorOriginalTimes[__instance.id] = __instance.time;
        }

        public static void EjectorPostfix(ref EjectorComponent __instance, ref float power)
        {
            int multiplier = Math.Max(1, DysonSphereProgramMenu.MachineSettingsUI.EjectorSpeed);
            int originalTime = EjectorOriginalTimes.TryGetValue(__instance.id, out int cachedTime) ? cachedTime : __instance.time;
            int unmodifiedTime = __instance.time;

            EjectorUnmodifiedTimes[__instance.id] = unmodifiedTime;

            if (multiplier <= 1)
            {
                return;
            }

            int delta = unmodifiedTime - originalTime;
            if (delta > 0)
            {
                __instance.time = originalTime + delta * multiplier;
            }
        }

        public static void RocketPrefix(ref SiloComponent __instance, ref float power, DysonSphere sphere, AnimData[] animPool, int[] consumeRegister)
        {
            SiloOriginalTimes[__instance.id] = __instance.time;
        }

        public static void RocketPostfix(ref SiloComponent __instance, ref float power)
        {
            int multiplier = Math.Max(1, DysonSphereProgramMenu.MachineSettingsUI.SiloSpeed);
            int originalTime = SiloOriginalTimes.TryGetValue(__instance.id, out int cachedTime) ? cachedTime : __instance.time;
            int unmodifiedTime = __instance.time;

            SiloUnmodifiedTimes[__instance.id] = unmodifiedTime;

            if (multiplier <= 1)
            {
                return;
            }

            int delta = unmodifiedTime - originalTime;
            if (delta > 0)
            {
                __instance.time = originalTime + delta * multiplier;
            }
        }

        public static bool EjectorExportPrefix(ref EjectorComponent __instance, ref int __state)
        {
            __state = __instance.time;
            if (EjectorUnmodifiedTimes.TryGetValue(__instance.id, out int cachedTime))
            {
                __instance.time = cachedTime;
            }

            return true;
        }

        public static void EjectorExportPostfix(ref EjectorComponent __instance, int __state)
        {
            __instance.time = __state;
        }

        public static bool RocketExportPrefix(ref SiloComponent __instance, ref int __state)
        {
            __state = __instance.time;
            if (SiloUnmodifiedTimes.TryGetValue(__instance.id, out int cachedTime))
            {
                __instance.time = cachedTime;
            }

            return true;
        }

        public static void RocketExportPostfix(ref SiloComponent __instance, int __state)
        {
            __instance.time = __state;
        }
    }
    public static class DroneComponent_InternalUpdate_Patch
    {

        public static void ApplyPatch(Harmony harmony)
        {
            MethodInfo targetMethod = AccessTools.Method(typeof(DroneComponent), "InternalUpdate",
                new Type[] {
                    typeof(CraftData).MakeByRefType(),
                    typeof(PlanetFactory),
                    typeof(Vector3).MakeByRefType(),
                    typeof(float),
                    typeof(float),
                    typeof(double).MakeByRefType(),
                    typeof(double).MakeByRefType(),
                    typeof(double),
                    typeof(double),
                    typeof(float).MakeByRefType()
                });

            if (targetMethod == null)
            {
                VitaminLogger.LogError("Patch konnte nicht angewendet werden: Methode 'InternalUpdate' nicht gefunden.");
                return;
            }

            MethodInfo prefixMethod = AccessTools.Method(typeof(DroneComponent_InternalUpdate_Patch), nameof(Prefix));
            harmony.Patch(targetMethod, new HarmonyMethod(prefixMethod));

        }

        static bool Prefix(ref DroneComponent __instance, ref float droneSpeed)
        {
            
            droneSpeed = droneSpeed * DysonSphereProgramMenu.MiscUI.DroneSlider;

            return true; // Originalmethode weiterhin ausführen.
        }
        
    }


    [HarmonyPatch(typeof(PlayerAction_Mine), "GameTick")]
    public static class PlayerAction_Mine_GameTick_FastMining_Patch
    {
        private static float originalMiningSpeed;

        static void Prefix(PlayerAction_Mine __instance)
        {
            if (!DysonSphereProgramMenu.MiscUI.FastMining) return; // Falls FastMining deaktiviert ist, keine Änderung

            if (__instance.player != null && __instance.player.mecha != null)
            {
                originalMiningSpeed = __instance.player.mecha.miningSpeed;
                __instance.player.mecha.miningSpeed *= 30f; // Geschwindigkeit x30
            }
        }

        static void Postfix(PlayerAction_Mine __instance)
        {
            if (!DysonSphereProgramMenu.MiscUI.FastMining) return; // Falls FastMining deaktiviert ist, keine Änderung

            if (__instance.player != null && __instance.player.mecha != null)
            {
                __instance.player.mecha.miningSpeed = originalMiningSpeed;
            }
        }
    }


    [HarmonyPatch(typeof(CombatSettings), "get_aggressiveLevel")]
    public static class CombatSettings_GetAggressiveLevel_Patch
    {
        static bool Prefix(ref EAggressiveLevel __result, CombatSettings __instance)
        {
            if (!DysonSphereProgramMenu.MiscUI.PassiveEnemy) return true; // Falls deaktiviert, Patch ignorieren

            __result = (EAggressiveLevel)(DysonSphereProgramMenu.MiscUI.PassiveEnemy ? 10.0f : (__instance.aggressiveness + 1f) * 10f + 0.5f);
            return false; // Originalmethode nicht ausführen, da __result überschrieben wurde
        }
    }


    [HarmonyPatch(typeof(PrefabDesc), "ReadPrefab")]//BeltModifer
    public static class PrefabDesc_ReadPrefab_Patch
    {
        static void Postfix(ref PrefabDesc __instance, GameObject _prefab, GameObject _colliderPrefab)
        {
            if (!DysonSphereProgramMenu.MainMenuUI.BeltSpeedMod) return; // Falls deaktiviert, Patch ignorieren

            BeltDesc belt = __instance.prefab.GetComponentInChildren<BeltDesc>(true);
            if (belt != null)
            {
                __instance.beltSpeed = belt.speed * DysonSphereProgramMenu.MainMenuUI.BeltMultiplier;
            }
        }
        // Wird beim Laden des Patches ausgeführt, um zu prüfen, ob der Patch erfolgreich war.
        static PrefabDesc_ReadPrefab_Patch()
        {
            if (DysonSphereProgramMenu.DebugMode)
            {
                VitaminLogger.LogInfo("[Patch Applied] PrefabDesc.ReadPrefab erfolgreich gepatcht.");
            }
        }
    }

    [HarmonyPatch(typeof(Mecha), "GameTick")]
    public static class Mecha_GameTick_Patch
    {
        private static float originalWalkSpeed;
        private static float originalReplicateSpeed;
        private static double originalCoreEnergy;
        private static int originalHP;
        private static int originalbulletEnergyCost;
        private static float originalbulletDamageScale;

        static void Prefix(Mecha __instance)
        {
            originalReplicateSpeed = __instance.replicateSpeed;
            originalCoreEnergy = __instance.coreEnergy;
            originalHP = __instance.hp;
            originalbulletEnergyCost = __instance.bulletEnergyCost;
            originalbulletDamageScale = __instance.bulletDamageScale;

            if (DysonSphereProgramMenu.MiscUI.MechaModded)
            {


                // Mecha-Anpassungen
                __instance.coreEnergy = __instance.coreEnergyCap; // Immer volle Energie
                __instance.hp = __instance.hpMax; // Unverwundbar
                __instance.replicateSpeed *= 30f; // Schnellere Replikation
                __instance.bulletEnergyCost = 0; // Keine Energiekosten für Schüsse
                __instance.bulletDamageScale = 100.0f; // 100-facher Schaden
            }


            if (DysonSphereProgramMenu.MainMenuUI.UnlockAll)
            {
                if (DysonSphereProgramMenu.DebugMode)
                    VitaminLogger.LogInfo("UnlockAll enabled: processing tech unlocks.");

                // Vorabprüfungen, ob alle nötigen Objekte existieren
                if (__instance.lab == null)
                {
                    VitaminLogger.LogError("UnlockAll error: __instance.lab is null.");
                }
                else if (__instance.lab.gameHistory == null)
                {
                    VitaminLogger.LogError("UnlockAll error: __instance.lab.gameHistory is null.");
                }
                else if (__instance.lab.gameHistory.techStates == null)
                {
                    VitaminLogger.LogError("UnlockAll error: techStates is null.");
                }
                else if (LDB.techs == null)
                {
                    VitaminLogger.LogError("UnlockAll error: LDB.techs is null.");
                }
                else
                {
                    // Iteriere über alle Tech-IDs in techStates
                    foreach (var techId in new System.Collections.Generic.List<int>(__instance.lab.gameHistory.techStates.Keys))
                    {
                        try
                        {
                            TechState techState = __instance.lab.gameHistory.techStates[techId];

                            // Check: Überspringe, falls tech bereits freigeschaltet ist
                            if (techState.unlocked)
                            {
                                if (DysonSphereProgramMenu.DebugMode)
                                    VitaminLogger.LogInfo("TechId " + techId + " is already unlocked, skipping.");
                                continue;
                            }

                            // Setze den Tech auf unlocked und aktualisiere die Werte
                            techState.unlocked = true;
                            techState.unlockTick = GameMain.gameTick;
                            techState.curLevel = techState.maxLevel;
                            techState.hashUploaded = techState.hashNeeded;
                            __instance.lab.gameHistory.techStates[techId] = techState;

                            TechProto techProto = LDB.techs.Select(techId);
                            if (techProto == null)
                            {
                                if (DysonSphereProgramMenu.DebugMode)
                                    VitaminLogger.LogError("UnlockAll error: TechProto for techId " + techId + " is null. Skipping.");
                                continue;
                            }

                            for (int i = 0; i < techProto.UnlockRecipes.Length; i++)
                            {
                                __instance.lab.gameHistory.UnlockRecipe(techProto.UnlockRecipes[i]);
                            }
                            for (int j = 0; j < techProto.UnlockFunctions.Length; j++)
                            {
                                __instance.lab.gameHistory.UnlockTechFunction(techProto.UnlockFunctions[j], techProto.UnlockValues[j], techState.maxLevel);
                            }
                            for (int k = 0; k < techProto.AddItems.Length; k++)
                            {
                                __instance.lab.gameHistory.GainTechAwards(techProto.AddItems[k], techProto.AddItemCounts[k]);
                            }
                            if (techId > 1)
                            {
                                __instance.lab.gameHistory.RegFeatureKey(1000100);
                            }
                            __instance.lab.gameHistory.NotifyTechUnlock(techId, techState.maxLevel, false);
                        }
                        catch (System.Exception ex)
                        {
                            VitaminLogger.LogError("Exception during UnlockAll processing for techId " + techId + ": " + ex.Message);
                        }
                    }
                }
                DysonSphereProgramMenu.MainMenuUI.UnlockAll = false;
                if (DysonSphereProgramMenu.DebugMode)
                    VitaminLogger.LogInfo("All techs unlocked.");
            }

        }

        static void Postfix(Mecha __instance)
        {
            __instance.replicateSpeed = originalReplicateSpeed;
            __instance.bulletEnergyCost = originalbulletEnergyCost;
        }

        static Mecha_GameTick_Patch()
        {
            if (DysonSphereProgramMenu.DebugMode)
            {
                VitaminLogger.LogInfo("[Patch Applied] Mecha.GameTick erfolgreich gepatcht.");
            }
        }
    }

    [HarmonyPatch(typeof(GameAbnormalityData_0925), "TriggerAbnormality")]
    public static class GameAbnormalityData_0925_Patch
    {
        static bool Prefix()
        {
            if (DysonSphereProgramMenu.MiscUI.PassiveEnemy)
            {
                // Wenn aktiviert, wird der Code blockiert und das Achievement nicht gespeichert.
                return false;
            }
            return true; // Standard-Verhalten ausführen
        }

        static GameAbnormalityData_0925_Patch()
        {
            if (DysonSphereProgramMenu.DebugMode)
            {
                VitaminLogger.LogInfo("[Patch Applied] GameAbnormalityData_0925.CheckAbnormality erfolgreich gepatcht.");
            }
        }
    }


    [HarmonyPatch(typeof(UIReplicatorWindow), "OnOkButtonClick")]
    public static class UIReplicator_OnOkButtonClick_Patch
    {
        // Felder für Reflection
        private static FieldInfo selectedRecipeField = AccessTools.Field(typeof(UIReplicatorWindow), "selectedRecipe");
        private static FieldInfo multipliersField = AccessTools.Field(typeof(UIReplicatorWindow), "multipliers");
        private static FieldInfo mechaForgeField = AccessTools.Field(typeof(UIReplicatorWindow), "mechaForge");
        private static FieldInfo isBatchField = AccessTools.Field(typeof(UIReplicatorWindow), "isBatch");

        static bool Prefix(UIReplicatorWindow __instance, int whatever, bool button_enable)
        {
            if (!DysonSphereProgramMenu.MiscUI.FreeCrafting) return true; // Falls deaktiviert, Patch ignorieren.

            RecipeProto selectedRecipe = selectedRecipeField.GetValue(__instance) as RecipeProto;
            if (selectedRecipe == null || GameMain.isFullscreenPaused)
            {
                return false; // Crafting stoppen
            }

            int id = selectedRecipe.ID;
            int num = 1;

            Dictionary<int, int> multipliers = multipliersField.GetValue(__instance) as Dictionary<int, int>;
            if (multipliers != null && multipliers.ContainsKey(id))
            {
                num = multipliers[id];
            }

            num = Mathf.Clamp(num, 1, 10); // Sicherstellen, dass num zwischen 1 und 10 bleibt

            Player mainPlayer = GameMain.mainPlayer;
            if (mainPlayer == null || mainPlayer.mecha == null)
            {
                return false;
            }

            RecipeProto recipeProto = LDB.recipes.Select(id);
            bool isBatch = (bool)isBatchField.GetValue(__instance);

            // 1️⃣ **Unbegrenztes Instant-Crafting ohne Materialverbrauch**
            for (int i = 0; i < recipeProto.Results.Length; i++)
            {
                int itemId = recipeProto.Results[i];
                int stackSize = LDB.items.Select(itemId).StackSize;
                int craftAmount = isBatch ? (num * stackSize) : num;
                int remaining = mainPlayer.TryAddItemToPackage(itemId, craftAmount, 0, true, 0, false);
                int successfullyAdded = craftAmount - remaining;

                if (successfullyAdded > 0)
                {
                    UIItemup.Up(itemId, successfullyAdded);
                    mainPlayer.mecha.AddProductionStat(itemId, successfullyAdded, mainPlayer.nearestFactory);
                }
            }

            // 2️⃣ **Erzwinge das Craften auch ohne freigeschaltete Rezepte oder Ressourcen**
            object mechaForge = mechaForgeField.GetValue(__instance);
            if (mechaForge != null)
            {
                MethodInfo addTaskMethod = AccessTools.Method(mechaForge.GetType(), "AddTask");
                addTaskMethod?.Invoke(mechaForge, new object[] { id, num });
            }

            // 3️⃣ **Registriere das Crafting für Achievements oder Statistiken**
            GameMain.history.RegFeatureKey(1000104);

            return false; // Original-Methode NICHT ausführen
        }

        static UIReplicator_OnOkButtonClick_Patch()
        {
            if (DysonSphereProgramMenu.DebugMode)
            {
                VitaminLogger.LogInfo("[Patch Applied] UIReplicator.OnOkButtonClick erfolgreich gepatcht.");
            }
        }
    }

    [HarmonyPatch(typeof(PlayerController), "GameTick")]
    public static class PlayerMove_Walk_GameTick_Patch
    {
        private static float originalWalkSpeed;
        private static float originalmaxSailSpeed;
        private static float originalmaxWarpSpeed;

        static void Prefix(PlayerController __instance)
        {
            
            // Original WalkSpeed speichern
            originalWalkSpeed = __instance.player.mecha.walkSpeed;
            originalmaxSailSpeed = __instance.player.mecha.maxSailSpeed;
            originalmaxWarpSpeed = __instance.player.mecha.maxWarpSpeed;

            __instance.player.mecha.walkSpeed = originalWalkSpeed * MovementMenuUI.MechaSpeed;
            __instance.player.mecha.maxSailSpeed = originalmaxSailSpeed * MovementMenuUI.SailSpeed;
            __instance.player.mecha.maxWarpSpeed = originalmaxWarpSpeed * MovementMenuUI.WarpSpeed;
        }

        static void Postfix(PlayerController __instance)
        {
            
            __instance.player.mecha.walkSpeed = originalWalkSpeed;
            __instance.player.mecha.maxSailSpeed = originalmaxSailSpeed;
            __instance.player.mecha.maxWarpSpeed = originalmaxWarpSpeed;
        }

        static PlayerMove_Walk_GameTick_Patch()
        {
            if (DysonSphereProgramMenu.DebugMode)
            {
                VitaminLogger.LogInfo("[Patch Applied] PlayerMove_Walk.GameTick erfolgreich gepatcht.");
            }
        }
    }
    //[HarmonyPatch(typeof(EjectorComponent), "InternalUpdate")]
    //public static class EjectorComponent_InternalUpdate_Patch
    //{

    //    static void Prefix(ref int __state, ref int num3)
    //    {
    //        // Original-Wert speichern
    //        __state = num3;

    //        // Schnelligkeit des Ejectors mit EjectorSpeed multiplizieren
    //        num3 = (int)(num3 * DysonSphereProgramMenu.MachineSettingsUI.EjectorSpeed);
    //    }

    //    //static void Postfix(ref int __state, ref int num3)
    //    //{
            
    //    //    // Originalwert nach Berechnung wiederherstellen
    //    //    num3 = __state;
    //    //}
    //}

}
