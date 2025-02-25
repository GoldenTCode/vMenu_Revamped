using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using FxEvents;

using Newtonsoft.Json;

using ScaleformUI;
using ScaleformUI.Menu;

using vMenu.Client.Menus;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;
using vMenu.Client.Events;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Functions
{
    public class MenuFunctions
    {
        private static readonly object _padlock = new();
        private static MenuFunctions _instance;

        private MenuFunctions()
        {
            Debug.WriteLine("MenuFunctions Initialized");
        }

        internal static MenuFunctions Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new MenuFunctions();
                }
            }
        }

        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        public static void QuitSession() => NetworkSessionEnd(true, true);

        public void SetBannerTexture()
        {
            if (Main.MenuBanner.TextureUrl != null)
            {
                Main.DuiObject = CreateDui(Main.MenuBanner.TextureUrl, 512, 128);
                string _duihandle = GetDuiHandle(Main.DuiObject);
                long _txdhandle = CreateRuntimeTxd("vmenu_textures_custom");
                CreateRuntimeTextureFromDuiHandle(_txdhandle, "menubanner", _duihandle);
                Main.MenuBanner.TextureDictionary = "vmenu_textures_custom";
            }
        }

        private Type[] GetClassesInMenusNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public void RestartMenu()
        {
            if (MenuHandler.CurrentMenu != null && MenuHandler.CurrentMenu.Visible)
            {
                MenuHandler.CurrentMenu.Visible = false;
                MenuHandler.CloseAndClearHistory();
                InitializeAllMenus();
                MenuHandler.CurrentMenu.Visible = true;
            }
            else
            {
                MenuHandler.CloseAndClearHistory();
                InitializeAllMenus();
            }
        }

        public async void UpdateOnlinePlayers(UIMenu menu, UIMenuItem item)
        {
            try
            {
                string playersSt = await EventDispatcher.Get<string>("RequestPlayersList");

                List<OnlinePlayersCB> playersObj = JsonConvert.DeserializeObject<List<OnlinePlayersCB>>(playersSt);
                int PlayersLeftToAdd = playersObj.Count();

                Main.OnlinePlayers.Clear();

                playersObj.ForEach(async (OnlinePlayersCB playerData) =>
                {
                    CitizenFX.Core.Player ply = Main.PlayerList[playerData.Player.ServerId];

                    if (ply != null && ply.Character != null && ply.Character.Exists())
                    {
                        int mugshotHandle = RegisterPedheadshot(ply.Character.Handle);

                        while (!IsPedheadshotReady(mugshotHandle) || !IsPedheadshotValid(mugshotHandle))
                        {
                            await BaseScript.Delay(0);
                        }

                        string mugtxd = GetPedheadshotTxdString(mugshotHandle);

                        Main.OnlinePlayers.Add(new KeyValuePair<OnlinePlayersCB, string>(playerData, mugtxd));

                        UnregisterPedheadshot(mugshotHandle);

                        PlayersLeftToAdd--;
                    }
                    else
                    {
                        PlayersLeftToAdd--;
                    }
                });

                while (PlayersLeftToAdd > 0)
                {
                    await BaseScript.Delay(0);
                }

                OnlinePlayersMenu.ReplaceMenuItems(menu, item);
            }
            catch(Exception err)
            {
                Debug.WriteLine(err.ToString());
            }
        }

        public PointF GetMenuOffset()
        {
            if (Main.MenuAlign == Shared.Enums.MenuAlign.Left)
            {
                return new PointF(20, 20);
            }
            else
            {
                return new PointF(970, 20);
            }
        }
        /// <summary>
        /// Public function to check if a permission is allowed.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="checkAnyway">Legacy Support useless now.</param>
        /// <returns></returns>
        public static bool IsAllowed(vMenu.Shared.Enums.Permission permission,  bool checkAnyway = false)
        {
            var permStr = permission.ToString();
            bool allvalue = false;
            if (permStr.Substring(0, 2).ToUpper() == permStr.Substring(0, 2))
            {
                if (permStr.Substring(2) is not ("All" or "Menu"))
                {
                    string value = (permStr.Substring(0, 2) + "All").ToString();
                    var enumval = Enum.TryParse(value, false, out vMenu.Shared.Enums.Permission perms);
                    if (enumval)
                    {
                        allvalue = MenuEvents.Permissions[perms];
                    }
                    else
                    {
                        allvalue = false;
                    }
                }
            }
            return MenuEvents.Permissions[permission] || MenuEvents.Permissions[vMenu.Shared.Enums.Permission.Everything] || false || allvalue;
        }
        public static readonly int build = 2699;
        public static bool GameBuildCheck()
        {
           return (GetGameBuildNumber() >= build);
        }
        public async void MiscSettingsTask()
        {

            
            BaseScript.Delay(0);
            return;
        }
        public void InitializeAllMenus()
        {
            // Submenus //
            _ = new Menus.WorldSubmenus.WeatherOptionsMenu();
            _ = new Menus.WorldSubmenus.TimeOptionsMenu();
            _ = new Menus.PlayerSubmenus.WeaponOptionsMenu();
            _ = new Menus.OnlinePlayersSubmenus.OnlinePlayerMenu();
            _ = new Menus.VehicleSubmenus.VehicleSpawnerMenu();

            // Menus //
            _ = new OnlinePlayersMenu();
            _ = new BannedPlayersMenu();
            _ = new PlayerOptionsMenu();
            _ = new VehicleOptionsMenu();
            _ = new WorldOptionsMenu();
            _ = new VoiceChatOptionsMenu();
            _ = new RecordingMenu();
            _ = new MiscOptionsMenu();
            _ = new AboutMenu();
            _ = new MainMenu();
        }
        #region GetUserInput
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public static async Task<string> GetUserInput(string windowTitle = null, string defaultText = null, int maxInputLength = 30)
        {
            // Create the window title string.
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

            // Display the input box.
            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await BaseScript.Delay(0);
            // Wait for a result.
            while (true)
            {
                var keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        return null;
                    case 1: // finished editing
                        return GetOnscreenKeyboardResult();
                    default:
                        await BaseScript.Delay(0);
                        break;
                }
            }
        }
        #endregion
    }
}
