using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace MultiCrafting {
    internal class TreeStep {

        public int itemId;
        public int usedAmount;
        public int extraAmount;
        public int distinctIngredients=1;
        public int depth = 0;
        public int recipeGroup;
        public int MakeAmount=1;
        public UIItemSlot2 slot;
        public UIItemSlot2 slotIngredients;
        public List<TreeStep> Ingredients=new List<TreeStep>();
        public TreeStep(int ItemId,int ItemUsed, int recipeGroup = -1, int ItemExtra = 0) {
            itemId = ItemId;
            usedAmount = ItemUsed;
            extraAmount = ItemExtra;
            this.recipeGroup = recipeGroup;
        }

        internal void UpdateStoredUI(Dictionary<int, int> storageUnused,bool found=false, float Famount=0) {

            // Green=Have DkGreen=passed Blue=need Orange=partially
            float HasAmount = 0;
            if (slot != null) {
                if (found) {
                    slot.BackgroundColor = new Color(81, 105, 44, 178);
                } else {
                    //returns amount of items you are still missing
                    //Val=remaining needed, Famount= percentage already has, Left=amount left to find
                    int Val = ContainsItem(storageUnused, (int)((usedAmount * Famount) + 0.5f));
                    int Left = usedAmount - (int)((usedAmount * Famount) + 0.5f);
                    if (Val == 0) {
                        found = true;
                        if (Famount > 0) {
                            slot.BackgroundColor = new Color(123, 227, 189, 178);
                        } else {
                            slot.BackgroundColor = new Color(121, 171, 44, 178);
                        }

                    } else if (Val == Left) {
                        if (Famount > 0) {
                            slot.BackgroundColor = new Color(247, 175, 42, 178);
                        } else {
                            slot.BackgroundColor = new Color(44, 57, 105, 178);
                        }
                        
                        HasAmount = Famount;
                    } else {
                        slot.BackgroundColor = new Color(247, 175, 42, 178);
                        HasAmount = ((usedAmount - Val) / MakeAmount) / ((float)(usedAmount + extraAmount) / MakeAmount);
                        //Main.NewText(HasAmount);
                    }
                }
            }

            foreach(TreeStep son in Ingredients) {
                son.UpdateStoredUI(storageUnused,found,HasAmount);
            }

        }

        internal void UpdateComponentsUI(Dictionary<int, int> storageUnused, Dictionary<int, (int, int,int)> needOwned, float Famount = 0) {

            int Val = ContainsItem(storageUnused, (int)((usedAmount * Famount) + 0.5f));
            int Left = usedAmount - (int)((usedAmount * Famount) + 0.5f);
            if (Val == 0) { //All were found 
                int V = 0;
                if (recipeGroup==-1) {
                    storageUnused.TryGetValue(itemId, out V);
                } else {
                    foreach(int itemIdG in RecipeGroup.recipeGroups[recipeGroup].ValidItems) {
                        storageUnused.TryGetValue(itemIdG, out int A);
                        V += A;
                    }
                }
                
                AddToTupleDict(needOwned, itemId, Left, V+Left);

            } else if (Val == Left) { //None were found
                if (Ingredients.Count > 0) {
                    foreach (TreeStep son in Ingredients) {
                        son.UpdateComponentsUI(storageUnused, needOwned, Famount);
                    }
                } else {
                    AddToTupleDict(needOwned, itemId, Left, 0);
                }

            } else {
                Famount = ((usedAmount - Val) / MakeAmount) / ((float)(usedAmount + extraAmount) / MakeAmount);

                if (Ingredients.Count > 0) {
                    AddToTupleDict(needOwned, itemId, Left - Val, Left - Val);
                    foreach (TreeStep son in Ingredients) {
                        son.UpdateComponentsUI(storageUnused, needOwned, Famount);
                    }
                } else {
                    AddToTupleDict(needOwned, itemId, Left, Left - Val);
                }
                //Main.NewText(HasAmount);
            }



        }

        private void AddToTupleDict(Dictionary<int, (int, int,int)> dict,int type, int stackN,int stackO) {
            if (dict.ContainsKey(type)) {
                var current = dict[type];
                current.Item1 += stackN;
                //current.Item2 += stackO;

                dict[type] = current;
            } else {
                dict[type] = (stackN, stackO,recipeGroup);
            }
        }

        private void AddToDictionary(Dictionary<int, int> dict, int type, int stack) {
  
            if (dict.ContainsKey(type)) {
                dict[type] += stack;

                if (dict[type] == 0) {
                    dict.Remove(type);
                } else if (dict[type] < 0) {
                    //Main.NewText($"Can't have {dict[type]} items");
                    dict.Remove(type);
                }

            } else {
                dict[type] = stack;
            }

        }
        private int ContainsItem(Dictionary<int, int> items,int Famount) {
            int notHave = usedAmount -Famount;

            if (recipeGroup == -1) {
                int a = 0;
                items.TryGetValue(itemId, out a);
                AddToDictionary(items, itemId, -Math.Min(notHave,a));
                notHave -= Math.Min(notHave, a);
            } else {
                foreach (int itemID in RecipeGroup.recipeGroups[recipeGroup].ValidItems) {
                    int a = 0;
                    items.TryGetValue(itemID, out a);
                    AddToDictionary(items, itemID, -Math.Min(notHave, a));
                    notHave -= Math.Min(notHave, a);
                }
            }

            return notHave;
        }
    }


}
