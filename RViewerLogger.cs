
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser;
using System.Drawing.Printing;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.IL_Actions.NPCs;

namespace MultiCrafting
{
    
    public static class RViewerLogger {
        private static log4net.ILog Logger => MultiCrafting.instance.Logger;
        private static bool Enabled => ModContent.GetInstance<CraftingConfig>().EnableLogging;
        public static void LogMsg(string Messagae) {
            if (Enabled)
                Logger.DebugFormat(Messagae);
        }

    }

}
