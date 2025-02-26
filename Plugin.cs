﻿using BepInEx;
using UnityEngine;
using System.IO;
using Steamworks;

[BepInPlugin("com.example.myplugin", "DysonSphereProgramMenu", "2.0.0")]
public class DysonSphereProgramMenu : BaseUnityPlugin
{
    // Plugin-Metadaten
    private string PluginName = "DysonSphereProgramMenu";
    private string PluginVersion = "2.0.0";
    public static bool DebugMode = true;

    // Sichtbarkeit des Menüs
    private static bool isMenuVisible;


    void Awake()
    {
        MainMenuUI.LoadConfig();
        DysonSphereProgramMenuMod.MyPatcher.ApplyPatches();
    }

    void Start()
    {
        VitaminLogger.LogInfo($"{PluginName} wurde gestartet!");
        MainMenuUI.LoadConfig();
        isMenuVisible = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Insert))
        {
            if (MainMenuUI.IsVisible || MiscUI.IsVisible || MovementMenuUI.IsVisible)
            {
                // Alle Menüs ausblenden
                MainMenuUI.IsVisible = false;
                MiscUI.IsVisible = false;
                MovementMenuUI.IsVisible = false;
            }
            else
            {
                // Nur das MainMenu wieder anzeigen (andere bleiben aus)
                MainMenuUI.IsVisible = true;
            }
        }
    }


    void OnGUI()
    {
        MainMenuUI.Draw();
        MiscUI.Draw();
        MovementMenuUI.Draw();
        
        
    }


    public static class UIHelper
    {
        private static float referenceWidth = 1920f;
        private static float referenceHeight = 1080f;

        private static GUIStyle highlightedToggle;
        private static GUIStyle defaultToggle;
        private static GUIStyle sectionStyle;

        public static Rect CreateWindow(int id, Rect windowRect, GUI.WindowFunction windowFunction, string title)
        {
            float scaleX = Screen.width / referenceWidth;
            float scaleY = Screen.height / referenceHeight;
            Matrix4x4 originalMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX, scaleY, 1));

            windowRect.x = Mathf.Clamp(windowRect.x, 0, referenceWidth - windowRect.width);
            windowRect.y = Mathf.Clamp(windowRect.y, 0, referenceHeight - windowRect.height);
            windowRect = GUILayout.Window(id, windowRect, windowFunction, title);

            GUI.matrix = originalMatrix;
            return windowRect;
        }

        public static void InitializeStyles()
        {
            if (highlightedToggle == null)
            {
                highlightedToggle = new GUIStyle(GUI.skin.toggle) { fontStyle = FontStyle.Bold };
                highlightedToggle.normal.textColor = Color.white;
                highlightedToggle.hover.textColor = Color.green;
                highlightedToggle.onNormal.textColor = Color.green;
                highlightedToggle.onHover.textColor = Color.yellow;

                defaultToggle = new GUIStyle(GUI.skin.toggle);
                defaultToggle.normal.textColor = Color.white;

                sectionStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(10, 10, 10, 10)
                };
            }
        }

        public static void DrawUI(ref Rect windowRect, int id, bool isVisible, GUI.WindowFunction windowFunction, string title)
        {
            if (isVisible)
            {
                windowRect = CreateWindow(id, windowRect, windowFunction, title);
            }
        }

        public static GUIStyle GetSectionStyle() => sectionStyle;
        public static GUIStyle GetHighlightedToggle() => highlightedToggle;
        public static GUIStyle GetDefaultToggle() => defaultToggle;
    }
    public class MainMenuUI
    {
        private static Rect mainMenuRect = new Rect(Screen.width - 250, 50, 200, 250);
        public static bool IsVisible = true;
        public static bool MovementMenu = false;
        public static bool UnlockAll = false;

        // Nicht sichtbare Werte (Konfiguration)
        public static bool BeltSpeedMod = false;
        public static int BeltMultiplier = 1;
        private static bool configLoaded = false;

        public static void Draw()
        {
            if (!configLoaded)
            {
                LoadConfig();
                configLoaded = true;
            }

            UIHelper.DrawUI(ref mainMenuRect, 0, IsVisible, MainMenuWindow, "Main Menu");
        }

        private static void MainMenuWindow(int windowID)
        {
            UIHelper.InitializeStyles();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical(UIHelper.GetSectionStyle());

            // Menü-Toggles mit Highlight
            MiscUI.IsVisible = GUILayout.Toggle(MiscUI.IsVisible, "➡ Misc Settings", MiscUI.IsVisible ? UIHelper.GetHighlightedToggle() : UIHelper.GetDefaultToggle());
            MovementMenuUI.IsVisible = GUILayout.Toggle(MovementMenuUI.IsVisible, "➡ Movement Menu", MovementMenuUI.IsVisible ? UIHelper.GetHighlightedToggle() : UIHelper.GetDefaultToggle());

            GUILayout.Space(10);

            if (GUILayout.Button("Unlock All"))
            {
                UnlockAll = true;
                VitaminLogger.LogInfo("UnlockAll activated!");
            }

            GUILayout.EndVertical();
        }

        public static void LoadConfig()
        {
            string configPath = Path.Combine(Application.dataPath, "../BepInEx/plugins/DysonMenu/config.txt");

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, "BeltMod:true\nBeltmultiplier:2");
                VitaminLogger.LogInfo("Config file not found. Created default config.");
            }

            foreach (var line in File.ReadAllLines(configPath))
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    if (line.StartsWith("BeltMod")) BeltSpeedMod = parts[1].Trim().ToLower() == "true";
                    if (line.StartsWith("Beltmultiplier")) int.TryParse(parts[1].Trim(), out BeltMultiplier);
                }
            }

            VitaminLogger.LogInfo($"Config Loaded: BeltSpeedMod={BeltSpeedMod}, BeltMultiplier={BeltMultiplier}");
        }
    }
    public class MiscUI
    {
        private static Rect miscMenuRect = new Rect(Screen.width - 250, 320, 200, 200);
        public static bool IsVisible = false;

        public static float DroneSlider = 1.0f;
        public static bool MechaModded = false;
        public static bool PassiveEnemy = false;
        public static bool AchievementToggle = true;
        public static bool FastMining = false;
        public static bool FreeCrafting = false;
        public static bool NoRequiredDirt = false;

        public static void Draw() => UIHelper.DrawUI(ref miscMenuRect, 1, IsVisible, MiscMenuWindow, "Misc Settings");

        private static void MiscMenuWindow(int windowID)
        {
            UIHelper.InitializeStyles();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.Label("Drone Speed: " + DroneSlider.ToString("0.00") + "x");
            DroneSlider = GUILayout.HorizontalSlider(DroneSlider, 0.1f, 10.0f);

            GUILayout.BeginVertical(UIHelper.GetSectionStyle());
            MechaModded = GUILayout.Toggle(MechaModded, "Modded Mech");
            PassiveEnemy = GUILayout.Toggle(PassiveEnemy, "Passive Enemy");
            AchievementToggle = GUILayout.Toggle(AchievementToggle, "Get Achievements?");
            FastMining = GUILayout.Toggle(FastMining, "Fast Mining");
            FreeCrafting = GUILayout.Toggle(FreeCrafting, "Free Crafting");
            NoRequiredDirt = GUILayout.Toggle(NoRequiredDirt, "No Dirt Required");
            GUILayout.EndVertical();
        }
    }
    public class MovementMenuUI
    {
        private static Rect movementMenuRect = new Rect(Screen.width - 250, 540, 200, 220);
        public static bool IsVisible = false;

        public static float MechaSpeed = 1f;
        public static float SailSpeed = 1f;
        public static float WarpSpeed = 1f;

        public static void Draw() => UIHelper.DrawUI(ref movementMenuRect, 2, IsVisible, MovementMenuWindow, "Movement Settings");
        
        private static void MovementMenuWindow(int windowID)
        {
            UIHelper.InitializeStyles();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.Label("Mecha Speed: " + MechaSpeed.ToString("0.00") + "x");
            MechaSpeed = GUILayout.HorizontalSlider(MechaSpeed, 1f, 10f);

            GUILayout.Label("Max Sail Speed: " + SailSpeed.ToString("0.00") + "x");
            SailSpeed = GUILayout.HorizontalSlider(SailSpeed, 1f, 5f);

            GUILayout.Label("Max Warp Speed: " + WarpSpeed.ToString("0.00") + "x");
            WarpSpeed = GUILayout.HorizontalSlider(WarpSpeed, 1f, 5f);
        }
    }




}
