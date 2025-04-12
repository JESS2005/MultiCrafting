using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using static System.Net.Mime.MediaTypeNames;

namespace MultiCrafting {

    internal class ComponentsUIPage : UIElement {
        internal DraggableUIPanel mainPanel;
        //internal Dictionary<int,UIItemSlotComponent> slotsList = new Dictionary<int, UIItemSlotComponent>();
        internal List<UIItemSlotComponent> slotsList = new List<UIItemSlotComponent>();
        private ComponentsUIPageInternal grid;
        private FinalUIElement UIHeaderFin;

        public ComponentsUIPage() {
            Height.Set(0, 1f);
            Width.Set(0, 1f);
            grid=new ComponentsUIPageInternal();
            Append(grid);
            var Scrollbar = new UIScrollbar();
            Scrollbar.SetView(100f, 1000f);
            Scrollbar.Top.Set(6, 0);
            Scrollbar.Height.Set(-27, 1f);
            Scrollbar.Left.Set(-20, 1f);
            
            grid.SetScrollbar(Scrollbar);

            UIHeaderFin = new FinalUIElement();
            Append(UIHeaderFin);
            Append(Scrollbar);
            OverflowHidden = true;

        }

        public void SetItem(int item) {
            UIHeaderFin.SetItem(item);
        }

        public void AddAll(Dictionary<int, int> storageUnused) {
            Dictionary<int, (int, int, int )> needOwned = new Dictionary<int, (int, int,int)>();
            foreach (TreeStep son in RecipeDecomposer.fullTree.Ingredients) {
                son.UpdateComponentsUI(storageUnused, needOwned);
            }
            RefreshValues(needOwned);
        }


        //Need to try grid._items
        private void RefreshValues(Dictionary<int, (int, int, int)> needOwned) {
            for (int i=slotsList.Count-1;i>=0;i--) {
                UIItemSlotComponent Value = slotsList[i];
                int Key = Value.item.type;
                if (needOwned.ContainsKey(Key)) {
                    if (needOwned[Key].Item2==0) {
                        Value._stackText.SetText($"[c/E11919:{needOwned[Key].Item2}/{needOwned[Key].Item1}]");
                    } else if (needOwned[Key].Item2< needOwned[Key].Item1) {
                        Value._stackText.SetText($"[c/FFF014:{needOwned[Key].Item2}/{needOwned[Key].Item1}]");
                    } else {
                        Value._stackText.SetText($"{needOwned[Key].Item2}/{needOwned[Key].Item1}");
                    }

                    needOwned.Remove(Key);
                } else {
                    //Main.NewText(grid.Children[0].HasChild(kvp.Value));
                    grid.Remove(Value);
                    slotsList.RemoveAt(i);
                }
            }
            foreach (KeyValuePair<int, (int, int, int)> kvp in needOwned) {
                string str;
                if (kvp.Value.Item2 == 0) {
                    str=$"[c/E11919:{kvp.Value.Item2}/{kvp.Value.Item1}]";
                } else if (kvp.Value.Item2 < kvp.Value.Item1) {
                    str = $"[c/FFF014:{kvp.Value.Item2}/{kvp.Value.Item1}]";
                } else {
                    str = $"{kvp.Value.Item2}/{kvp.Value.Item1}";
                }
                UIItemSlotComponent newSlot = new UIItemSlotComponent(kvp.Key, str);
                grid.Add(newSlot);
                slotsList.Add(newSlot);
                if (kvp.Value.Item3!=-1) {
                    newSlot.ChangeHoverName(kvp.Value.Item3);
                }
            }
        }

        /*public override void Draw(SpriteBatch spriteBatch) {
            if (mainPanel.openPage == 1)
                base.Draw(spriteBatch);
        }*/
    }

    internal class FinalUIElement : UIPanel {
        private Item item;
        private UIText text;
        public FinalUIElement() {

            Height.Set(50, 0f);
            Width.Set(-30, 1f);
            Left.Set(30, 0);
            text = new UIText("",1.5f);
            //text.Left.Set(10, 0);
            text.Top.Set(-2, 0);
            text.Height.Set(0, 1f);
            Append(text);
            this.item = new Item(0);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (ContainsPoint(Main.MouseScreen)) {
                Main.HoverItem = item;
                Main.hoverItemName = item.Name;
            }
            base.DrawSelf(spriteBatch);
        }

        public void SetItem(int item) {
            this.item = new Item(item);
            text.SetText($"[i:{item}] "+ Lang.GetItemNameValue(item));
        }
    }

    internal class ComponentsUIPageInternal : UIGrid {

        public ComponentsUIPageInternal() {
            Height.Set(-70, 1f);
            Width.Set(-40, 1f);
            Left.Set(10, 0f);
            Top.Set(60, 0f);

        }
    }



    public class UIItemSlotComponent : UIPanel {
        internal Item item;
        internal UIText _iconText;
        internal UIText _stackText;

        public UIItemSlotComponent(int itemId,string nums,int stack=1) {
            item = new Item(itemId, stack);
            Width.Set(50f, 0f);
            Height.Set(50f, 0f);

            _iconText = new UIText($"[i:{itemId}]", 1.6f);
            _iconText.Top.Set(-4f, 0f);
            _iconText.HAlign = 0.5f;
            _iconText.VAlign = 0.5f;
            Append(_iconText);

            _stackText = new UIText(nums, 0.7f);
            _stackText.Left.Set(8f , 0f);
            _stackText.Top.Set(22f, 0f);
            _stackText.Width.Set(0f, 1f);
            _stackText.Height.Set(0f, 1f);
            _stackText.HAlign = 1f;
            _stackText.DynamicallyScaleDownToWidth = true;
            Append(_stackText);


        }


        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (ContainsPoint(Main.MouseScreen)) {
                Main.HoverItem = item;
                Main.hoverItemName = item.Name;
            }
            base.DrawSelf(spriteBatch);
        }


        internal void ChangeHoverName(int GroupId) {
            item.SetNameOverride(Language.GetTextValue(RecipeGroup.recipeGroups[GroupId].GetText()));
        }

    }
}
