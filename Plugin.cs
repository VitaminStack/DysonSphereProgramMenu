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
    private bool isMenuVisible;

    // UI-Parameter
    public static float DroneSlider = 1.0f;
    public static int BeltMultiplier;
    public static int beltSlider = 1;
    // Startposition in der Referenzauflösung (oben links)
    private Rect menuRect = new Rect(Screen.width - 250, 10, 200, 75);

    // Feature-Toggles
    public static bool MechaModded = false;
    public static bool achievementToggle = true;
    public static bool passiveEnemy = false;
    public static bool SaveGameLoaded = false;
    public static bool MinerModded = false;
    public static bool EjectorModded = false;
    public static bool RocketModded = false;
    public static bool EjectorExportModded = false;
    public static bool RocketExportModded = false;
    public static bool BeltSpeedMod;

    public static bool FreeCrafting = false;
    public static bool FastMining = false;

    public static bool UnlockAll;
    void Awake()
    {
        LoadConfig();
        DysonSphereProgramMenuMod.MyPatcher.ApplyPatches();
    }

    void Start()
    {
        VitaminLogger.LogInfo($"{PluginName} wurde gestartet!");
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
        // Dynamische Skalierung der UI basierend auf einer Referenzauflösung (hier 1920x1080)
        float referenceWidth = 1920f;
        float referenceHeight = 1080f;
        float scaleX = Screen.width / referenceWidth;
        float scaleY = Screen.height / referenceHeight;
        Matrix4x4 originalMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX, scaleY, 1));

        if (isMenuVisible)
        {
            // Sicherstellen, dass das Fenster innerhalb der Referenzauflösung liegt
            menuRect.x = Mathf.Clamp(menuRect.x, 0, referenceWidth - menuRect.width);
            menuRect.y = Mathf.Clamp(menuRect.y, 0, referenceHeight - menuRect.height);
            menuRect = GUILayout.Window(0, menuRect, MenuWindowIngame, "Menu");
        }

        // Wiederherstellen der ursprünglichen Matrix
        GUI.matrix = originalMatrix;
    }

    void MenuWindowIngame(int windowID)
    {        
        // Ermöglicht das Verschieben des Fensters
        GUI.DragWindow(new Rect(0, 0, 10000, 20));

        GUILayout.Label("DroneSpeed");
        GUILayout.BeginHorizontal();
        DroneSlider = GUILayout.HorizontalSlider(DroneSlider, 0.1f, 10.0f);
        // Formatierung korrigiert: getrennte Formatierung und Text
        GUILayout.Label(DroneSlider.ToString("0.00") + "x Speed");
        GUILayout.EndHorizontal();

        GUILayout.Label("BeltMultiplier: " + BeltMultiplier.ToString());
        MechaModded = GUILayout.Toggle(MechaModded, "Modded Mech");
                
        passiveEnemy = GUILayout.Toggle(passiveEnemy, "Passive Enemy");
        achievementToggle = GUILayout.Toggle(achievementToggle, "Get Achievements?");
        FastMining = GUILayout.Toggle(FastMining, "FastMining");
        FreeCrafting = GUILayout.Toggle(FreeCrafting, "FreeCrafting");
        if (GUILayout.Button("UnlockAll"))
        {
            UnlockAll = true;
        }
    }


    public static void LoadConfig()
    {
        string configPath = Path.Combine(Application.dataPath, "../BepInEx/plugins/VitaminMenu/config.txt");
        if (File.Exists(configPath))
        {
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
        else
        {
            VitaminLogger.LogInfo("Could not find Config File!");
        }
    }
}
