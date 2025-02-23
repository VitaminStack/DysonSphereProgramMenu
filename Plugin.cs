using BepInEx;
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
            isMenuVisible = !isMenuVisible;
        }
    }

    void OnGUI()
    {
        MovementMenuUI.Draw();
        MainMenuUI.Draw();
    }

    public static class UIHelper
    {
        private static float referenceWidth = 1920f;
        private static float referenceHeight = 1080f;

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
    }
    public class MovementMenuUI
    {
        private static Rect movementMenuRect = new Rect(Screen.width - 405, 50, 150, 150);
        public static bool IsVisible = false;

        public static float MechaSpeed = 1f;
        public static float SailSpeed = 1f;
        public static float WarpSpeed = 1f;

        public static void Draw()
        {
            if (MainMenuUI.MovementMenu)
            {
                movementMenuRect = UIHelper.CreateWindow(1, movementMenuRect, MovementMenuWindow, "Movement Settings");
            }
        }

        private static void MovementMenuWindow(int windowID)
        {
            if(MainMenuUI.MovementMenu)
            {
                GUI.DragWindow(new Rect(0, 0, 10000, 20));
                GUILayout.Label("WalkSpeed: " + MechaSpeed.ToString("0.00") + "x");
                MechaSpeed = GUILayout.HorizontalSlider(MechaSpeed, 1f, 10f);
                GUILayout.Label("MaxSail: " + SailSpeed.ToString("0.00") + "x");
                SailSpeed = GUILayout.HorizontalSlider(SailSpeed, 1f, 5f);
                GUILayout.Label("MaxWarp: " + WarpSpeed.ToString("0.00") + "x");
                WarpSpeed = GUILayout.HorizontalSlider(WarpSpeed, 1f, 5f);
            }           
        }
    }
    public class MainMenuUI
    {
        private static Rect mainMenuRect = new Rect(Screen.width - 250, 50, 200, 200);
        public static bool IsVisible = true;

        public static bool BeltSpeedMod = false;
        public static int BeltMultiplier = 1;

        public static float DroneSlider = 1.0f;        
        public static bool MechaModded = false;
        public static bool passiveEnemy = false;
        public static bool achievementToggle = true;
        public static bool FastMining = false;
        public static bool FreeCrafting = false;
        public static bool MovementMenu = false;
        public static bool UnlockAll = false;

        public static void Draw()
        {
            if (isMenuVisible)
            {
                mainMenuRect = UIHelper.CreateWindow(0, mainMenuRect, MainMenuWindow, "Main Menu");
            }
        }
        private static void MainMenuWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.Label("DroneSpeed");
            GUILayout.BeginHorizontal();
            DroneSlider = GUILayout.HorizontalSlider(DroneSlider, 0.1f, 10.0f);
            GUILayout.Label(DroneSlider.ToString("0.00") + "x Speed");
            GUILayout.EndHorizontal();

            GUILayout.Label("BeltMultiplier: " + BeltMultiplier.ToString());
            MechaModded = GUILayout.Toggle(MechaModded, "Modded Mech");

            passiveEnemy = GUILayout.Toggle(passiveEnemy, "Passive Enemy");
            achievementToggle = GUILayout.Toggle(achievementToggle, "Get Achievements?");
            FastMining = GUILayout.Toggle(FastMining, "FastMining");
            FreeCrafting = GUILayout.Toggle(FreeCrafting, "FreeCrafting");

            MovementMenu = GUILayout.Toggle(MovementMenu, "MovementMenu");
            if (MovementMenu)
            {
                MovementMenuUI.IsVisible = true;
            }
            if (GUILayout.Button("UnlockAll"))
            {
                UnlockAll = true;
            }

        }

        public static void LoadConfig()
        {
            string configPath = Path.Combine(Application.dataPath, "../BepInEx/plugins/DysonMenu/config.txt");

            // Prüfen, ob die Config-Datei existiert; falls nicht, Standard-Konfiguration erstellen
            if (!File.Exists(configPath))
            {
                // Standardwerte: BeltMod deaktiviert, Beltmultiplier auf 1
                string defaultConfig = "BeltMod:true" + System.Environment.NewLine + "Beltmultiplier:2";
                File.WriteAllText(configPath, defaultConfig);
                VitaminLogger.LogInfo("Config file not found. A default config file has been created.");
            }

            // Konfiguration einlesen
            string customPath = File.ReadAllText(configPath).Trim();
            string[] lines = customPath.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("BeltMod"))
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        BeltSpeedMod = parts[1].Trim() == "1";
                    }
                }
                if (line.StartsWith("Beltmultiplier"))
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        int.TryParse(parts[1].Trim(), out BeltMultiplier);
                    }
                }
            }
        }

    }
}
