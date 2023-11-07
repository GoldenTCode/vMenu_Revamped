// System Libraries //
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using vMenu.Client.Menus;
using vMenu.Client.Menus.OnlinePlayersSubmenus;
using static CitizenFX.FiveM.Native.Natives;
using ScaleformUI;

namespace vMenu.Client.Functions
{
    public class Notify : BaseScript
    {
        public static void Error(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Error:~h~ {message}", NotificationColor.Red, blink, SaveToBrief);
        } 
        public static void Success(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Success:~h~ {message}", NotificationColor.GreenLight, blink, SaveToBrief);
        }   
        public static void Info(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Info:~h~ {message}", NotificationColor.Blue, blink, SaveToBrief);
        }   
        public static void Alert(string message, bool blink = true, bool SaveToBrief = true)
        {
            Notifications.ShowNotification($"~h~Alert:~h~ {message}", NotificationColor.Yellow, blink, SaveToBrief);
        }   
        public static void Custom(string message, bool blink = true, bool SaveToBrief = true, NotificationColor color = NotificationColor.Cyan)
        {
            Notifications.ShowNotification(message, color, blink, SaveToBrief);
        }         
    }
}
