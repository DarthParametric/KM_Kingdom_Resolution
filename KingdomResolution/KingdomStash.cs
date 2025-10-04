using Kingmaker;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.UI;
using Kingmaker.UI.SettingsUI;
using Kingmaker.View.MapObjects;
using UnityEngine;
using static KingdomResolution.Main;

namespace KingdomResolution;

public class KingdomStash
{
    class DisposeFakeStash : GameAction
    {
        public GameObject FakeStash;
        public DisposeFakeStash(GameObject fakeStash)
        {
            FakeStash = fakeStash;
        }
        public override string GetCaption()
        {
            return "DisposeFakeStash";
        }
        public override void RunAction()
        {
            if (FakeStash != null) UnityEngine.Object.Destroy(FakeStash);
        }
    }

    static string BindingName = "OpenKingdomStash";
    static bool IsLoaded = false;
    public static bool selectingHotkey = false;
    static GameObject fakeKingdomStash;

    static public void Init()
    {
        if (IsLoaded) return;
        IsLoaded = true;
        Game.Instance.Keyboard.Bind(BindingName, new Action(OnOpen));
        if (settings.kingdomStashHotkey != null)
        {
            Game.Instance.Keyboard.RegisterBinding(BindingName, settings.kingdomStashHotkey, KeyboardAccess.GameModesGroup.World, false);
        }
    }

    public static void OnOpen()
    {
        try
        {
            if (fakeKingdomStash != null)
            {
                Game.Instance.UI.LootWindowController.OnButtonClose();
                return;
            }
            fakeKingdomStash = new GameObject("FakeKingdomStash");
            var loot = fakeKingdomStash.AddComponent<LootComponent>();
            Traverse.Create(loot).Field("m_AddMapMarker").SetValue(false);
            ((InteractionComponent)loot).Data = new LootComponent.LootPersistentData(Enumerable.Empty<LootEntry>())
            {
                AlreadyUnlocked = true,
                Enabled = true
            };
            loot.LootContainerType = LootContainerType.PlayerChest;
            var kingdomLoot = fakeKingdomStash.AddComponent<KingdomLootComponent>();
            kingdomLoot.OnAreaDidLoad();

            var resourceType = Traverse.CreateWithType("Kingmaker.View.MapObjects.LootComponent+TriggerData").GetValue<Type>();
            object triggerData = Activator.CreateInstance(resourceType);
            var actionsHolder = ScriptableObject.CreateInstance<ActionsHolder>();
            actionsHolder.Actions.Actions = new GameAction[]
            {
                    new DisposeFakeStash(fakeKingdomStash)
            };
            Traverse.Create(triggerData).Field("Action").SetValue(actionsHolder);
            Traverse.Create(loot).Field("m_OnClosedTrigger").SetValue(triggerData);

            loot.Interact(Game.Instance.Player.MainCharacter.Value);
        } catch(Exception ex)
        {
            Log.Log($"Caught exception:\n{ex}");
        }
    }
    static bool CanBeRegistered()
    {
        var isBound = Game.Instance.Keyboard.GetBindingByName(BindingName) != null;
        if (isBound) Game.Instance.Keyboard.UnregisterBinding(BindingName);
        var result =  Game.Instance.Keyboard.CanBeRegistered(
            BindingName,
            settings.kingdomStashHotkey.Key,
            KeyboardAccess.GameModesGroup.World,
            settings.kingdomStashHotkey.IsCtrlDown,
            settings.kingdomStashHotkey.IsAltDown,
            settings.kingdomStashHotkey.IsShiftDown);
        if (isBound) Game.Instance.Keyboard.RegisterBinding(BindingName, settings.kingdomStashHotkey, KeyboardAccess.GameModesGroup.World, false);
        return result;
    }
    static public void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Kingdom Stash", GUILayout.ExpandWidth(false)))
        {
            OnOpen();
        }
        var buttonStye = new GUIStyle(GUI.skin.button);
        if (selectingHotkey) {
            buttonStye.active.textColor = Color.gray;
            buttonStye.focused = buttonStye.active;
            buttonStye.normal = buttonStye.active;
            buttonStye.hover = buttonStye.active;
            SelectHotkey();
        }         
        if (GUILayout.Button("Set KingdomStash Stash Hotkey", buttonStye, GUILayout.ExpandWidth(false)))
        {
            selectingHotkey = true;
        }
        if (GUILayout.Button("Clear Kingdom Stash Hotkey", GUILayout.ExpandWidth(false)))
        {
            Game.Instance.Keyboard.UnregisterBinding(BindingName);
            settings.kingdomStashHotkey = null;
        }
        var hotkeyText = "";
        if (settings.kingdomStashHotkey == null)
        {
            hotkeyText = "None";
        }
        else
        {
            if (settings.kingdomStashHotkey.IsCtrlDown) hotkeyText += "Ctrl+";
            if (settings.kingdomStashHotkey.IsShiftDown) hotkeyText += "Shift+";
            if (settings.kingdomStashHotkey.IsAltDown) hotkeyText += "Alt+";
            hotkeyText += settings.kingdomStashHotkey.Key.ToString();
        }
        GUILayout.Label($"Kingdom Stash Hotkey: {hotkeyText}", GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        if(settings.kingdomStashHotkey != null && !CanBeRegistered())
        {
            GUILayout.Label($"Keybinding {hotkeyText} is already registered");
        }
    }
    static public void SelectHotkey()
    {
        var keycode = KeyCode.None;
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))){
            if (vKey == KeyCode.None || vKey > KeyCode.PageDown) continue;
             if(Input.GetKey(vKey)){
                keycode = vKey;
                break;
             }
        }
        if(keycode != KeyCode.None)
        {
            settings.kingdomStashHotkey = new BindingKeysData()
            {
                Key = keycode,
                IsCtrlDown = Input.GetKey(KeyCode.LeftControl),
                IsAltDown = Input.GetKey(KeyCode.LeftAlt),
                IsShiftDown = Input.GetKey(KeyCode.LeftShift)
            };
            selectingHotkey = false;
            Game.Instance.Keyboard.UnregisterBinding(BindingName);
            Game.Instance.Keyboard.RegisterBinding(BindingName, settings.kingdomStashHotkey, KeyboardAccess.GameModesGroup.World, false);
        }
    }
}
