using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.GameInput;
using RecipeBrowser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

/* TODO:
 * Add option to hide subtrees
 * selection of recipe (+reload recipe button)
 * show who drops/sells
 * stop scrollwheel action
 * fix item list text
 * controller support
 * add page icons
 * merge owned/not if last step
 * 
 * Low Priority:
 * rework recipe search with a dict (?)
 * avoid needless item destruction (check owned)
 * sort ingredients page
 * favourite page
*/

namespace MultiCrafting
{

    public class MultiCrafting : Mod
    {
        internal static List<Condition> OldConditions = new List<Condition>();
        internal static MultiCrafting instance;
        internal ModKeybind SetRecipeHotKey;
        public override void Load()
        {
            Logger.InfoFormat("Loaded");
            instance = this;
            SetRecipeHotKey = KeybindLoader.RegisterKeybind(this, "SetRecipe", "Mouse4");
            UISystem.instance.LoadRest();
            UISystem.instance.MenuBar.mainPanel.MakeScrollBars();
        }

        public override void Unload()
        {
            Logger.InfoFormat("Unoaded");
            instance = null;
            SetRecipeHotKey = null;
        }

    }

    internal class MultiCraftingPlayer : ModPlayer
    {
        private int Time = 0;
        internal static MultiCraftingPlayer instance = null;

        public override void Load() {
            instance = this;
        }

        public override void Unload() {
            instance = null;
        }

        public override void OnEnterWorld() {
            base.OnEnterWorld();
            instance=this;
        }


        public override void PostUpdate() {

            if (Player.whoAmI==Main.myPlayer&&++Time == 20) {
                Time = 0;
                if (UISystem.instance.MenuBar.open && ModContent.GetInstance<CraftingConfig>().ShowOwnedOnTree && RecipeDecomposer.fullTree != null) {

                    if (UISystem.instance.MenuBar.mainPanel.openPage == 0) {
                        RecipeDecomposer.fullTree.UpdateStoredUI(GetStorage());
                    } else {
                        UISystem.instance.MenuBar.mainPanel._componentsPage.AddAll(GetStorage());
                    }
                }
            }
            base.PostUpdate();
        }


        private Dictionary<int, int> GetStorage() {
            Dictionary<int, int> stored = new Dictionary<int, int>();
            if (MagicStorageIntegration.Enabled) { //Magic Storage
                foreach (Item item in MagicStorageIntegration.StorageItems()) {
                    if (!item.IsAir) { RecipeDecomposer.AddToDictionary(stored, item.type, item.stack); }
                }
            }
            foreach (Item item in Player.inventory) { //Inventory
                if (!item.IsAir) { RecipeDecomposer.AddToDictionary(stored, item.type, item.stack); }
            }
            foreach (Item item in Player.armor) { //Armor and accessories
                if (!item.IsAir) { RecipeDecomposer.AddToDictionary(stored, item.type, item.stack); }
            }
            int ChIndex = Player.chest;
            if (ChIndex >= 0) {
                foreach (Item item in Main.chest[ChIndex].item) { //Open chest
                    RecipeDecomposer.AddToDictionary(stored, item.type, item.stack);
                }
            }
            return stored;
        }

        public void NewRecipe() {
            Item itemHover = Main.HoverItem;
            CraftingConfig configs = ModContent.GetInstance<CraftingConfig>();
            RViewerLogger.LogMsg($"Hover:{Lang.GetItemNameValue(itemHover.type)}x{itemHover.stack}");
            RViewerLogger.LogMsg($"Generate:{configs.GenerateRecipe} Visualize:{configs.VisualizeTree} Magic Storage:{MagicStorageIntegration.Enabled} UIOpen:{UISystem.instance.MenuBar.open} Max:{configs.MaxIngredients}");
            if (!itemHover.IsAir && (configs.GenerateRecipe || configs.VisualizeTree)) {

                //RViewerLogger.LogMsg($"{stored.Count} items detected");

                (List<Item> Items, Dictionary<int, int> Groups, List<int> Tiles, List<Condition> Conditions) result;
                if (configs.VisualizeTree) {
                    result = RecipeDecomposer.DecomposeItem(itemHover, GetStorage(), MultiCraftingSystem.maxRecipeLenght);
                } else {
                    result = RecipeDecomposerNoTree.DecomposeItem(itemHover, GetStorage(), MultiCraftingSystem.maxRecipeLenght);
                }
                List<Item> components = result.Items;
                Dictionary<int, int> groups = result.Groups;

                RViewerLogger.LogMsg($"{components.Count}+{groups.Count} items");

                if (components.Count + groups.Count <= MultiCraftingSystem.maxRecipeLenght) {
                    if (configs.GenerateRecipe) {
                        MultiCraftingSystem.AddCustomTiles(result.Tiles);
                        MultiCraftingSystem.AddCustomConditions(result.Conditions);
                        MultiCraftingSystem.AddCustomRecipe(itemHover.type, components);
                        RViewerLogger.LogMsg($"Added {components.Count} components");
                        MultiCraftingSystem.AddCustomGroups(groups);
                        Recipe.FindRecipes(); //Refresh vanilla ui, avoids cheats
                        if (MagicStorageIntegration.Enabled) {
                            MagicStorageIntegration.RefreshStorageUI();
                        }
                    }
                    
                } else {
                    Main.NewText("Recipe requires too many components");
                }
                if (configs.VisualizeTree) {
                    //not really needed
                    if (true||UISystem.instance.MenuBar.mainPanel.openPage==0) {
                        UISystem.instance.MenuBar.mainPanel.updateTree();
                    } else {
                        //UISystem.instance.MenuBar.mainPanel._componentsPage.AddAll();
                    }
 
                    if (!configs.GenerateRecipe) {
                        Main.NewText($"Calculated Tree for [i:{itemHover.type}]");
                    }
                }

            } else {
                if (configs.VisualizeTree) {
                    UISystem.instance.MenuBar.Show();
                }
            }
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (MultiCrafting.instance.SetRecipeHotKey.JustPressed) {
                NewRecipe();
            }
        }
    }

    public class MultiCraftingSystem : ModSystem {

        public static int maxRecipeLenght= ModContent.GetInstance<CraftingConfig>().MaxIngredients;
        public static Recipe mainRecipe;
        internal static List<int> MyGroups= new List<int>();
        internal static float HLineDim=0;
        internal static float VLineDim = 0;
        internal static Color LineColor= Color.White;

        public static void AddCustomRecipe(int Result, List<Item> Components)
        {
            mainRecipe.requiredItem.Clear();
            mainRecipe.ReplaceResult(Result);

            foreach (Item item in Components)
            {
                mainRecipe.AddIngredient(item.type,item.stack);
            }
            Main.NewText($"Added recipe for [i:{Result}]");
            if (MagicStorageIntegration.Enabled&&MagicStorageIntegration.recCrafingDepth()!=0) {
                Main.NewText("Magic storage's recursion crafting can cause conflict, it's suggested to either disable it or disable \"Generate Recipe\" in this mod's settings", Color.Red);
            }
        }


        public static void AddCustomGroups(Dictionary<int, int> Groups)
        {
            mainRecipe.acceptedGroups.Clear();

            foreach (KeyValuePair<int, int> group in Groups)
            {
                mainRecipe.AddRecipeGroup(group.Key,group.Value);
            }
        }
        public static void AddCustomTiles(List<int> Tiles)
        {
            mainRecipe.requiredTile.Clear();

            foreach (int tile in Tiles)
            {
                mainRecipe.AddTile(tile);
            }
        }
        public static void AddCustomConditions(List<Condition> Conditions)
        {
            //this one is read only !?
            foreach (Condition condition in MultiCrafting.OldConditions)
            {
                mainRecipe.RemoveCondition(condition);
            }

            MultiCrafting.OldConditions = Conditions;
            foreach (Condition condition in Conditions)
            {
                mainRecipe.AddCondition(condition);
            }
        }




        public override void AddRecipeGroups() //Just some handy groups
        {
            RecipeGroup group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CopperOre)}", ItemID.CopperOre, ItemID.TinOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("CopperOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.IronOre)}", ItemID.IronOre, ItemID.LeadOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("IronOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverOre)}", ItemID.SilverOre, ItemID.TungstenOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("SilverOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.GoldOre)}", ItemID.GoldOre, ItemID.PlatinumOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("GoldOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.DemoniteOre)}", ItemID.DemoniteOre, ItemID.CrimtaneOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("DemoniteOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CobaltOre)}", ItemID.CobaltOre, ItemID.PalladiumOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("CobaltOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MythrilOre)}", ItemID.MythrilOre, ItemID.OrichalcumOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("MythrilOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.AdamantiteOre)}", ItemID.AdamantiteOre, ItemID.TitaniumOre);
            MyGroups.Add(RecipeGroup.RegisterGroup("AdamantiteOreCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CopperBar)}", ItemID.CopperBar, ItemID.TinBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("CopperBarCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.IronBar)}", ItemID.IronBar, ItemID.LeadBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("IronBarCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}", ItemID.SilverBar, ItemID.TungstenBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("SilverBarCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.GoldBar)}", ItemID.GoldBar, ItemID.PlatinumBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("GoldBarCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.DemoniteBar)}", ItemID.DemoniteBar, ItemID.CrimtaneBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("DemoniteBarCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.CobaltBar)}", ItemID.CobaltBar, ItemID.PalladiumBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("CobaltBarCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MythrilBar)}", ItemID.MythrilBar, ItemID.OrichalcumBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("MythrilBarCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.AdamantiteBar)}", ItemID.AdamantiteBar, ItemID.TitaniumBar);
            MyGroups.Add(RecipeGroup.RegisterGroup("AdamantiteBarCR", group));

            //Need to fix these names:
            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.ShadowScale)}", ItemID.ShadowScale, ItemID.TissueSample);
            MyGroups.Add(RecipeGroup.RegisterGroup("EvilMaterialCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Evil Sword", ItemID.LightsBane, ItemID.BloodButcherer);
            MyGroups.Add(RecipeGroup.RegisterGroup("EvilSwordsCR", group));

            group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Evil Seed", ItemID.CorruptSeeds, ItemID.CrimsonSeeds);
            MyGroups.Add(RecipeGroup.RegisterGroup("EvilSeedsCR", group));

        }


        public override void AddRecipes() //Adds recipe with the max items chose
        {
            mainRecipe = Recipe.Create(ItemID.VoidMonolith);
            for (int i = 0; i < maxRecipeLenght; i++)
            {
                mainRecipe.AddIngredient(ItemID.VoidMonolith, 10);
            }
            mainRecipe.Register();
        }

       
    }
}
