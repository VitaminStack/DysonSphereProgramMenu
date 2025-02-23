using ABN;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Steamworks;




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

                // Patch für SiloComponent.InternalUpdate (Prefix)
                MethodInfo rocketMethod = AccessTools.Method(typeof(SiloComponent), "InternalUpdate", new Type[] { typeof(float), typeof(DysonSphere), typeof(AnimData[]), typeof(int[]) });
                MethodInfo rocketPrefix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.RocketPrefix));

                // Patch für EjectorComponent.Export (Prefix)
                MethodInfo ejectorExportMethod = AccessTools.Method(typeof(EjectorComponent), "Export", new Type[] { typeof(BinaryWriter) });
                MethodInfo ejectorExportPrefix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.EjectorExportPrefix));

                // Patch für SiloComponent.Export (Prefix)
                MethodInfo rocketExportMethod = AccessTools.Method(typeof(SiloComponent), "Export", new Type[] { typeof(BinaryWriter) });
                MethodInfo rocketExportPrefix = typeof(DysonSphereProgramMenuMod.Patches).GetMethod(nameof(DysonSphereProgramMenuMod.Patches.RocketExportPrefix));
                








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



        public static bool EjectorPrefix(ref EjectorComponent __instance, float power, long tick, DysonSwarm swarm, AstroData[] astroPoses, AnimData[] animPool, int[] consumeRegister)
        {
            if (DysonSphereProgramMenu.EjectorModded)
            {
                UnityEngine.Debug.Log("EjectorPrefix: Anpassung des Ejektor-Verhaltens.");
                // Beispielhafte Anpassung: Parameter modifizieren oder Logging einbauen.
            }
            return true;
        }
        public static bool RocketPrefix(ref SiloComponent __instance, float power, DysonSphere sphere, AnimData[] animPool, int[] consumeRegister)
        {
            if (DysonSphereProgramMenu.RocketModded)
            {
                UnityEngine.Debug.Log("RocketPrefix: Anpassung der Silo/Raketen-Komponente.");
                // Beispiel: Modifikationen vor der Ausführung der Originalmethode.
            }
            return true;
        }
        public static bool EjectorExportPrefix(ref EjectorComponent __instance, ref BinaryWriter w)
        {
            if (DysonSphereProgramMenu.EjectorExportModded)
            {
                UnityEngine.Debug.Log("EjectorExportPrefix: Custom Export für EjectorComponent.");
                // Beispielhafte eigene Exportlogik – hier Dummy-Daten schreiben:
                return false; // Originalexport-Methode unterdrücken
            }
            return true;
        }
        public static bool RocketExportPrefix(ref SiloComponent __instance, ref BinaryWriter w)
        {
            if (DysonSphereProgramMenu.RocketExportModded)
            {
                UnityEngine.Debug.Log("RocketExportPrefix: Custom Export für SiloComponent.");
                // Beispiel: Dummy-Daten schreiben
                return false;
            }
            return true;
        }




    }
    public static class DroneComponent_InternalUpdate_Patch
    {
        private static float cachedDronespeed = 0.0f;

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

            if (cachedDronespeed == 0.0f)
            {
                cachedDronespeed = droneSpeed;
            }

            droneSpeed *= DysonSphereProgramMenu.DroneSlider;

            return true; // Originalmethode weiterhin ausführen.
        }
    }


    [HarmonyPatch(typeof(PlayerAction_Mine), "GameTick")]
    public static class PlayerAction_Mine_GameTick_FastMining_Patch
    {
        private static float originalMiningSpeed;

        static void Prefix(PlayerAction_Mine __instance)
        {
            if (!DysonSphereProgramMenu.FastMining) return; // Falls FastMining deaktiviert ist, keine Änderung

            if (__instance.player != null && __instance.player.mecha != null)
            {
                originalMiningSpeed = __instance.player.mecha.miningSpeed;
                __instance.player.mecha.miningSpeed *= 30f; // Geschwindigkeit x30
            }
        }

        static void Postfix(PlayerAction_Mine __instance)
        {
            if (!DysonSphereProgramMenu.FastMining) return; // Falls FastMining deaktiviert ist, keine Änderung

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
            if (!DysonSphereProgramMenu.passiveEnemy) return true; // Falls deaktiviert, Patch ignorieren

            __result = (EAggressiveLevel)(DysonSphereProgramMenu.passiveEnemy ? 10.0f : (__instance.aggressiveness + 1f) * 10f + 0.5f);
            return false; // Originalmethode nicht ausführen, da __result überschrieben wurde
        }
    }


    [HarmonyPatch(typeof(PrefabDesc), "ReadPrefab")]//BeltModifer
    public static class PrefabDesc_ReadPrefab_Patch
    {
        static void Postfix(ref PrefabDesc __instance, GameObject _prefab, GameObject _colliderPrefab)
        {
            if (!DysonSphereProgramMenu.BeltSpeedMod) return; // Falls deaktiviert, Patch ignorieren

            BeltDesc belt = __instance.prefab.GetComponentInChildren<BeltDesc>(true);
            if (belt != null)
            {
                __instance.beltSpeed = belt.speed * DysonSphereProgramMenu.BeltMultiplier;
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

            if (DysonSphereProgramMenu.MechaModded)
            {
                

                // Mecha-Anpassungen
                __instance.coreEnergy = __instance.coreEnergyCap; // Immer volle Energie
                __instance.hp = __instance.hpMax; // Unverwundbar
                __instance.replicateSpeed *= 30f; // Schnellere Replikation
                __instance.bulletEnergyCost = 0; // Keine Energiekosten für Schüsse
                __instance.bulletDamageScale = 100.0f; // 100-facher Schaden
            }
            

            if (DysonSphereProgramMenu.UnlockAll)
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
                DysonSphereProgramMenu.UnlockAll = false;
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
            if (DysonSphereProgramMenu.achievementToggle)
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
}
