using Microsoft.Xna.Framework;
using MultiCrafting;
using System.ComponentModel;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RecipeBrowser
{
    class CraftingConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        /*[DefaultValue(true)]
        public bool AllowWhenTyping { get; set; }*/

        [Header("RecipeSettings")]

        [DefaultValue(false)]
        public bool AllowBannerUse { get; set; }

        [DefaultValue(true)]
        public bool StopWhenOwned { get; set; }

        [DefaultValue(true)]
        public bool VisualizeTree { get; set; }

        [DefaultValue(true)]
        public bool GenerateRecipe { get; set; }

        [DefaultValue(35)]

        [Range(1, 1500)]
        [ReloadRequired]
        public int MaxIngredients { get; set; }

        [DefaultValue(15)]
        [Range(1, 40)]
        public int MaxCraftingSteps { get; set; }

        [DefaultValue(true)]
        public bool ShowOwnedOnTree { get; set; }

        //Add config to disable other pages?


        [Header("LineSettings")]

        [DefaultValue(typeof(Color), "227, 227, 227, 255")]
        [Range(0, 255)]
        public Color LineColor { get; set; }

        [DefaultValue(4)]
        [Range(0f, 20f)]
        public int HorizontalLineThickness { get; set; }

        [DefaultValue(4)]
        [Range(0f, 20f)]
        public int VerticalLineThickness { get; set; }

        [Header("DebugSettings")]
        [DefaultValue(false)]
        public bool EnableLogging { get; set; }

        

        internal static void SaveConfig() //Thank you Javid
        {
            MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
            if (saveMethodInfo != null)
                saveMethodInfo.Invoke(null, new object[] { ModContent.GetInstance<CraftingConfig>() });
            else { }
        }

        public override void OnChanged() {
            //This will be called whenever config is saved
            if (!VisualizeTree && UISystem.instance != null) {
                UISystem.instance.MenuBar.open = false;
                //Main.NewText("Off");
            }
        }

    }
}