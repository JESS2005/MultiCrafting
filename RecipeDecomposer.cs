using Humanizer;
using MultiCrafting;
using rail;
using RecipeBrowser;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

internal class RecipeDecomposer
{
    internal static TreeStep fullTree=null;
    public static Dictionary<int,int> DictFromList(List<Item> list)
    {
        Dictionary<int, int> items = new Dictionary<int, int>();
        foreach (Item item in list)
        {
            AddToDictionary(items, item.type, item.stack);
        }
        return items;
    }


    //             Items      Groups    Tiles     Conditions
    public static (List<Item> Items, Dictionary<int, int> Groups,List<int> Tiles,List<Condition> Conditions) DecomposeItem(Item targetItem,Dictionary<int,int>storageUnused,int MaxItems=200)
    {
        fullTree = new TreeStep(targetItem.type,1);
        int MaxSteps = ModContent.GetInstance<CraftingConfig>().MaxCraftingSteps;
        bool AllowBanners= ModContent.GetInstance<CraftingConfig>().AllowBannerUse;
        bool MakingRecipe = ModContent.GetInstance<CraftingConfig>().GenerateRecipe;
        List<TreeStep> items = new List<TreeStep>();
        List<TreeStep> storedOrMissing = new List<TreeStep>();

        Dictionary<int, int> groupsList = new Dictionary<int, int>();
        List<int> tilesList = new List<int>();
        List<Condition> conditionsList = new List<Condition>();
        items.Add(fullTree);


        int V = 0;
        do
        {
            V++;
            List<TreeStep> newItems = new List<TreeStep>();
            //Key=itemId, value=usedAmount
            foreach (TreeStep itemTree in items)
            {
                Recipe recipe = FindFirstValidRecipe(itemTree.itemId, AllowBanners);


                if (recipe != null)
                {
                
                    if (CreatesLoop(recipe))
                    {
                        //Creates loop, don't go further
                        storedOrMissing.Add(itemTree);
                    } else {
                        itemTree.MakeAmount = recipe.createItem.stack;
                        //Main.NewText($"From {recipe.requiredItem[0].stack} [i:{recipe.requiredItem[0].type}] to {recipe.createItem.stack} [i:{recipe.createItem.type}]");
                        foreach (int tile in recipe.requiredTile)
                        {
                            if (!tilesList.Contains(tile)) { tilesList.Add(tile); }
                        }
                        foreach (Condition condition in recipe.Conditions)
                        {
                            if (!conditionsList.Contains(condition)) { conditionsList.Add(condition); }
                        }
                        
                        int LostAm = 0; //are we losing any items?
                        //Every ingredient in the recipe
                        foreach (Item ingredient in recipe.requiredItem)
                        {
                            if (!ingredient.IsAir)
                            {
                                int whichGroup= ItemInRecipeGroup(ingredient.type, recipe.acceptedGroups); //eh, recipe.acceptedGroups not that good
                                if (whichGroup == -1) {whichGroup= ItemInRecipeGroup(ingredient.type, MultiCraftingSystem.MyGroups); } //Trying both
                                Dictionary<int,int> content=new Dictionary<int,int>();
                                //int content = 0;
                                if (whichGroup == -1) {
                                    AddToDictionary(content, ingredient.type, ContainsItem(storageUnused, ingredient.type));
                                    //content= ContainsItem(storageUnused, ingredient.type);
                                } else {
                                    foreach ( int itemID in RecipeGroup.recipeGroups[whichGroup].ValidItems) {
                                        AddToDictionary(content, itemID, ContainsItem(storageUnused, itemID));
                                        //Main.NewText($"You have {content[ingredient.type]} [i:{ingredient.type}]");
                                        //content += ContainsItem(storageUnused, itemID);
                                    }
                                }
                                int contentCount = getDictTotal(content);
                                int resultAmount = recipe.createItem.stack; //how many results does one craft create
                                int ingredientsTot = ingredient.stack *( (itemTree.usedAmount + resultAmount - 1) / resultAmount);//Total amount of ingredient required
                                int lost = (itemTree.usedAmount) % resultAmount;
                                if (lost != 0) {
                                    LostAm = resultAmount - lost;
                                }
                                if (contentCount!=0) {

                                    //AddToDictionary(storedOrMissing, ingredient.type, ingredient.stack * kvp.Value);
                                    if (contentCount < ingredient.stack * itemTree.usedAmount)
                                    {
                                        //some is stored, may go further
                                        TreeStep itemTemp = new TreeStep(ingredient.type, contentCount, whichGroup);
                                        storedOrMissing.Add(itemTemp);
                                        itemTree.Ingredients.Add(itemTemp);
                                        itemTemp = new TreeStep(ingredient.type, ingredientsTot - contentCount, whichGroup);
                                        newItems.Add(itemTemp);
                                        itemTree.Ingredients.Add(itemTemp);
                                        foreach (KeyValuePair<int, int> storedPair in content) {
                                            AddToDictionary(storageUnused, storedPair.Key, -storedPair.Value);
                                        }
                                        //AddToDictionary(storageUnused, ingredient.type, -contentCount);
                                    } else
                                    {
                                        //is already stored, don't go further
                                        TreeStep itemTemp = new TreeStep(ingredient.type, ingredientsTot, whichGroup);
                                        itemTree.Ingredients.Add(itemTemp);
                                        storedOrMissing.Add(itemTemp); //IngredientsTot is 
                                        foreach (KeyValuePair<int, int> storedPair in content) {
                                            if (ingredientsTot>0) {
                                                AddToDictionary(storageUnused, storedPair.Key, -Math.Min(storedPair.Value,ingredientsTot));
                                                ingredientsTot-=storedPair.Value;
                                            }
                                        }
                                        //AddToDictionary(storageUnused, ingredient.type, -ingredientsTot);
                                    }
                                } else
                                {
                                    //none is stored, proceed
                                    TreeStep itemTemp = new TreeStep(ingredient.type, ingredientsTot, whichGroup);
                                    newItems.Add(itemTemp);
                                    itemTree.Ingredients.Add(itemTemp);
                                }
                            }
                        }
                        if (LostAm > 0 && MakingRecipe) {
                            itemTree.extraAmount = LostAm;
                            Main.NewText($"This recipe will lose {LostAm} [i:{recipe.createItem.type}]"); }
                    }
                }
                else
                {
                    //no recipe, don't go further
                    storedOrMissing.Add(itemTree);
                }
            }

            items = newItems;
        } while (items.Count>0 && V< MaxSteps && (storedOrMissing.Count+items.Count)<=MaxItems);
        //Main.NewText(V);
        if (V== MaxSteps&&items.Count>0)
        {
            Main.NewText("That's a long recipe...");
            /*String intact = "";
            foreach (KeyValuePair<int,int> kvp in items) {
                intact += $"[i:{kvp.Key}]";
            }
            Main.NewText("Leaving " + intact + " as is");*/
        }
        if ((storedOrMissing.Count + items.Count) > MaxItems)
        {
            Main.NewText($"Stopped counting at {(storedOrMissing.Count + items.Count)} Components");
            if (MaxItems<1500)
            {
                Main.NewText($"You can increase this in the settings");
            }
        }

        //Main.NewText(V);
        // Convert back to Item list
        List<Item> result = new List<Item>();
        //If recipe didn't complete this may still have stuff
        foreach (TreeStep itemStep in items)
        {
            storedOrMissing.Add(itemStep);
        }
        foreach (TreeStep itemStep in storedOrMissing)
        {
            int GroupIn = ItemInRecipeGroup(itemStep.itemId,MultiCraftingSystem.MyGroups);
            if (GroupIn == -1)
            {
                result.Add(new Item(itemStep.itemId, itemStep.usedAmount));
            } else
            {
                AddToDictionary(groupsList,GroupIn, itemStep.usedAmount);
            }
            
        }
        var (A,B)=CountDistinct(fullTree);
        return (result,groupsList,tilesList, conditionsList);
    }

    private static (int,int) CountDistinct(TreeStep mainStep) {
        int count = 0;
        int depth = -1;
        foreach (TreeStep son in mainStep.Ingredients) {
            var (tempCount,tempDepth) = CountDistinct(son);
            depth=Math.Max(depth,tempDepth);
            count += tempCount;
        }
        depth++;
        if (count == 0) count = 1;
        mainStep.distinctIngredients = count;
        mainStep.depth = depth;
        return (count,depth);
    }

    public static int ItemInRecipeGroup(int itemID,List<int>groupList)
    {
        //Uses only groups from this mod
        foreach (int groupid in groupList) {
            if (RecipeGroup.recipeGroups[groupid].ValidItems.Contains(itemID))
                return groupid;
        }
        return -1;
    }

    internal static int getDictTotal(Dictionary<int, int> items) {
        int a = 0;
        foreach (KeyValuePair<int, int> kvp in items) {
            a += kvp.Value;
        }

        return a;
    }

    internal static int ContainsItem(Dictionary<int,int> items, int item)
    {
        /*foreach (Item item2 in items) {
            if (item2.type==item.type) return item2;
        }
        return null;*/
        int a=0;
        items.TryGetValue(item, out a);
        return a;
    }
    internal static Recipe FindFirstValidRecipe(int itemType,bool AllowBanners)
    {
        foreach (Recipe recipe in Main.recipe) {
            if (recipe.requiredItem.Count != 0) {
                //Recipe recipe = Main.recipe[i];
                String EngName = ItemID.Search.GetName(recipe.requiredItem[0].type);
                //                                             if true the found recipe is skipped, skips all the useless ones
                if (recipe.createItem.type == itemType && !(EngName.Contains("Fence") || EngName.Contains("Wall") || EngName.Contains("Platform") || (!AllowBanners && EngName.Contains("Banner")) || recipe == MultiCraftingSystem.mainRecipe)) {
                    return recipe;
                }
            }
        }
        return null;
    }

    private static bool CreatesLoop(Recipe recipe)
    {
        // Check if any ingredient is already in our decomposition chain
        foreach (Item ingredient in recipe.requiredItem)
        {
            if (ingredient.type==recipe.createItem.type)
            {
                return true;
            }
            Recipe recip2 = FindFirstValidRecipe(ingredient.type, false);
            if (recip2 == null) return false;
            foreach (Item ingredient2 in recip2.requiredItem)
            {
                if (ingredient2.type == recipe.createItem.type) return true;
            }

        }
        return false;
    }

    internal static void AddToDictionary(Dictionary<int, int> dict, int type, int stack)
    {
        if (dict.ContainsKey(type))
        {
            dict[type] += stack;

            if (dict[type] == 0) { 
                dict.Remove(type); 
            }else if (dict[type] < 0)
            {
                Main.NewText($"Can't have {dict[type]} items");
                dict.Remove(type);
            }

        }
        else
        {
            dict[type] = stack;
        }

    }

}