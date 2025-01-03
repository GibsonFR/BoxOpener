global using BepInEx;
global using BepInEx.IL2CPP;
global using HarmonyLib;
global using System.Linq;
global using System.Reflection;
global using TMPro;
global using UnhollowerRuntimeLib;
global using UnityEngine;
global using UnityEngine.UI;

namespace BoxOpener
{
    [BepInPlugin("26A0C2CA-5DE9-42D6-845A-9FFB0526E9F0", "BoxOpener", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Log.LogInfo("Mod created by Gibson, discord : gib_son");
        }

        private static float lastBoxOpenTime = 0f;
        private static int currentItemIndex = 0;
        private static GameObject[] items;
        private static bool isProcessingBoxes = false;
        private static float openingDelay = 0.5f;

        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Update))]
        [HarmonyPostfix]
        public static void OnSteamManagerUpdate()
        {
            if (Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.LeftControl) && !isProcessingBoxes)
            {
                items = GameObject.FindObjectsOfType<GameObject>()
                    .Where(go => go.name == "CosmeticItemUi(Clone)")
                    .ToArray();

                isProcessingBoxes = true;
                currentItemIndex = 0;
                lastBoxOpenTime = Time.time;
            }

            if (isProcessingBoxes && currentItemIndex < items.Length)
            {
                if (Time.time - lastBoxOpenTime >= openingDelay)
                {
                    var item = items[currentItemIndex];
                    currentItemIndex++;
                    lastBoxOpenTime = Time.time;

                    if (!item) return;

                    var nameTransform = item.transform.Find("Name");
                    if (!nameTransform) return;

                    var itemText = nameTransform.GetComponent<TextMeshProUGUI>();
                    if (!itemText) return;

                    if (!itemText.text.Contains("Crab Box")) return;

                    var itemButton = item.GetComponent<Button>();
                    if (!itemButton) return;

                    SimulateButtonClick(itemButton);
                    OpenBox();
                }
            }
            else if (isProcessingBoxes)
            {
                isProcessingBoxes = false;
            }

            void OpenBox()
            {
                try
                {
                    var allObjects = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Object>());
                    if (allObjects == null || allObjects.Length == 0) return;

                    var targetScripts = allObjects
                        .Where(obj => obj?.TryCast<GeneralUiInventoryItemClick>() != null)
                        .Select(obj => obj.TryCast<GeneralUiInventoryItemClick>())
                        .ToList();

                    foreach (var script in targetScripts)
                    {
                        var equipMethod = script.GetType().GetMethod("Equip",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        equipMethod?.Invoke(script, null);
                    }
                }
                catch { }
            }

            void SimulateButtonClick(Button button)
            {
                if (!button) return;

                try
                {
                    var eventSystem = UnityEngine.EventSystems.EventSystem.current;
                    eventSystem?.SetSelectedGameObject(button.gameObject);

                    UnityEngine.EventSystems.ExecuteEvents.Execute(
                        button.gameObject,
                        new UnityEngine.EventSystems.PointerEventData(eventSystem),
                        UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler
                    );

                    UnityEngine.EventSystems.ExecuteEvents.Execute(
                        button.gameObject,
                        new UnityEngine.EventSystems.BaseEventData(eventSystem),
                        UnityEngine.EventSystems.ExecuteEvents.submitHandler
                    );
                }
                catch { }
            }
        }

        //Anticheat Bypass 
        [HarmonyPatch(typeof(EffectManager), "Method_Private_Void_GameObject_Boolean_Vector3_Quaternion_0")]
        [HarmonyPatch(typeof(LobbyManager), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(MonoBehaviourPublicVesnUnique), "Method_Private_Void_0")]
        [HarmonyPatch(typeof(LobbySettings), "Method_Public_Void_PDM_2")]
        [HarmonyPatch(typeof(MonoBehaviourPublicTeplUnique), "Method_Private_Void_PDM_32")]
        [HarmonyPrefix]
        public static bool Prefix(System.Reflection.MethodBase __originalMethod)
        {
            return false;
        }
    }
}