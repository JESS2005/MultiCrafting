using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser;
using ReLogic.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static System.Net.Mime.MediaTypeNames;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace MultiCrafting {

    [Autoload(Side = ModSide.Client)]
    public class UISystem : ModSystem {
        internal static UISystem instance;
        internal RecipeTreeUI MenuBar;
        internal UserInterface _menuBar;
        
        public override void Load() {
            instance = this;

        }

        public void LoadRest() {
            MenuBar = new RecipeTreeUI();
            MenuBar.Activate();
            _menuBar = new UserInterface();
            _menuBar.SetState(MenuBar);
        }
        public override void UpdateUI(GameTime gameTime) {
            
            if (MenuBar!=null&&MenuBar.open) { }
                _menuBar?.Update(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) { //Thank you Javid
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "RecipeViewer: Recipe Viewer",
                    delegate {
                        if (MenuBar.open)
                            _menuBar.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }

    public class RecipeTreeUI : UIState {
        internal DraggableUIPanel mainPanel;
        private bool dragging;
        public bool open=false;
        //private Vector2 offset;
        private float zoom = 1f;


        public override void OnInitialize() {
            mainPanel = new DraggableUIPanel();
            mainPanel.SetPadding(0);
            mainPanel.Left.Set(840, 0);
            mainPanel.Top.Set(86, 0);
            mainPanel.Width.Set(600, 0);
            mainPanel.Height.Set(400, 0);
            Append(mainPanel);

        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (open&&mainPanel.IsMouseHovering) {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
    }
    public class Canvas : UIElement {
        internal float Scale = 1f;
        internal List<UIItemSlot2> slots=new List<UIItemSlot2>();
        internal List<UIHorizontalLine> lines= new List<UIHorizontalLine>();
        internal List<UIVerticalLine> linesV = new List<UIVerticalLine>();

        public void SetW(int W) {
            Width.Set(W, 0f);
        }

        public void SetH(int H) {
            Height.Set(H, 0f);
        }
    }
    public class DraggableUIPanel : UIPanel {
        private Vector2 _offset; // Mouse click offset from panel's top-left corner
        private Vector2 _dragOffset;
        public bool Dragging;
        public bool DraggingCanvas;
        public bool HorizontalScrolling;
        public bool VerticalScrolling;
        public bool ReSizing;
        public Canvas canvas;

        public int openPage = 0;

        public static float Zoom = 1f;
        public static float MinZoom = 0.5f, MaxZoom = 3f;

        internal UIScrollbar _verticalScrollbar;
        internal UIHorizontalScrollbar _horizontalScrollbar;
        internal UIElement _notCanvas;
        internal UIElement _expandButton;
        internal UIPageButtonCirc _treeButton;
        internal UIPageButtonCirc _itemsButton;
        internal UIPageButtonCirc _recipeButton;

        internal TreeUIPage _treePage;
        internal ComponentsUIPage _componentsPage;
        private UIElement _hideElement;


        public void MakeScrollBars() { //i'm sure i had to make this for some reason
            _horizontalScrollbar = new UIHorizontalScrollbar();
            _horizontalScrollbar.SetView(1f, 1f);
            _horizontalScrollbar.Height.Set(20f, 0f);
            _horizontalScrollbar.Width.Set(-27f, 1f);
            _horizontalScrollbar.Left.Set(6f, 0f);
            _horizontalScrollbar.Top.Set(-20f, 1f);
            _treePage.Append(_horizontalScrollbar);

            _verticalScrollbar = new UIScrollbar();
            _verticalScrollbar.SetView(100f, 100f);
            _verticalScrollbar.Height.Set(-27f, 1f);
            _verticalScrollbar.Width.Set(20f, 0f);
            _verticalScrollbar.Left.Set(-20f, 1f);
            _verticalScrollbar.Top.Set(6f, 0f);
            _treePage.Append(_verticalScrollbar);

            _componentsPage = new ComponentsUIPage();
            _componentsPage.mainPanel = this;
            //Append(_componentsPage);

            _treeButton = new UIPageButtonCirc("View tree");
            _treeButton.Top.Set(10f, 0f);
            _treeButton.Left.Set(10f, 0f);
            Append(_treeButton);

            _itemsButton = new UIPageButtonCirc("Ingredients checklist");
            _itemsButton.Top.Set(26f, 0f);
            _itemsButton.Left.Set(10f, 0f);
            Append(_itemsButton);

            _hideElement=new UIElement();
            _hideElement.Width.Set(0, 0);
            _hideElement.Height.Set(0, 0);
            _hideElement.OverflowHidden = true;
            _hideElement.Append(_componentsPage);

            /*_recipeButton = new UIPageButtonCirc("Refresh recipe");
            _recipeButton.Top.Set(42f, 0f);
            _recipeButton.Left.Set(10f, 0f);
            Append(_recipeButton);*/

        }

        private void ReAppendButtons() {
            Append(_itemsButton);
            Append(_treeButton);
        }

        public DraggableUIPanel() {
            canvas = new Canvas();
        }
        public override void OnInitialize() {
            base.OnInitialize();

            // Canvas area
            _treePage = new TreeUIPage();
            Append(_treePage);
            _treePage.mainPanel = this;
            _notCanvas = new UIElement();
            _notCanvas.Top.Set(0f, 0f);
            _notCanvas.Width.Set(-20f, 1f);
            _notCanvas.Height.Set(-20f, 1f);
            _notCanvas.OverflowHidden = true;
            _treePage.Append(_notCanvas);

            // Canvas
            canvas.Width.Set(600f-20, 0f);
            canvas.Height.Set(400f-20, 0f);
            _notCanvas.Append(canvas);


            // hold to re-size
            _expandButton = new UIElement();
            _expandButton.Left.Set(-14f, 1f);
            _expandButton.Top.Set(-14f, 1f);
            _expandButton.Width.Set(14f, 0f);
            _expandButton.Height.Set(14f, 0f);
            Append(_expandButton);



        }

        public override void ScrollWheel(UIScrollWheelEvent evt) {
            if (UISystem.instance.MenuBar.open && ContainsPoint(evt.MousePosition)&& openPage == 0) {
                float oldZoom = Zoom;
                Zoom += 0.1f * Math.Sign(evt.ScrollWheelValue);
                Zoom = MathHelper.Clamp(Zoom, MinZoom, MaxZoom);
                float zoomCH = (Zoom / oldZoom);
                canvas.Height.Pixels = canvas.Height.Pixels * zoomCH;
                canvas.Width.Pixels = canvas.Width.Pixels * zoomCH;

                float RelX = evt.MousePosition.X - Left.Pixels;
                canvas.Left.Pixels = RelX - (RelX - canvas.Left.Pixels) * zoomCH;
                float RelY = evt.MousePosition.Y - Top.Pixels;
                canvas.Top.Pixels = RelY - (RelY - canvas.Top.Pixels) * zoomCH;

                foreach (UIItemSlot2 sSlot in canvas.slots) {
                    sSlot.Left.Pixels = sSlot.Left.Pixels * zoomCH;
                    sSlot.Top.Pixels = sSlot.Top.Pixels * zoomCH;
                    sSlot.Width.Pixels = sSlot.Width.Pixels * zoomCH;
                    sSlot.Height.Pixels = sSlot.Height.Pixels * zoomCH;
                    sSlot._iconText.Remove();
                    sSlot._iconText = new UIText($"[i:{sSlot._itemId}]", Zoom * 1.6f);
                    sSlot._iconText.Top.Set(-2f - 2f * Zoom, 0f);
                    sSlot._iconText.HAlign = 0.5f;
                    sSlot._iconText.VAlign = 0.5f;
                    sSlot.Append(sSlot._iconText);
                    if (sSlot._stackText != null) {
                        String Text = sSlot._stackText.Text;
                        sSlot._stackText.Remove();
                        sSlot._stackText = new UIText(Text, Zoom);
                        sSlot._stackText.Left.Set(8f * Zoom, 0f);
                        sSlot._stackText.Top.Set(-3 + 18f * (float)Math.Pow(Zoom, 1.5f), 0f);
                        sSlot._stackText.Width.Set(0f, 1f);
                        sSlot._stackText.Height.Set(0f, 1f);
                        sSlot._stackText.HAlign = 1f;
                        sSlot.Append(sSlot._stackText);
                    }
                }
                foreach (UIHorizontalLine sSlot in canvas.lines) {
                    sSlot.Left.Pixels = sSlot.Left.Pixels * zoomCH;
                    sSlot.Top.Pixels = sSlot.Top.Pixels * zoomCH;
                    sSlot.Width.Pixels = sSlot.Width.Pixels * zoomCH;
                    sSlot.Height.Pixels = sSlot.Height.Pixels * zoomCH;
                }
                foreach (UIVerticalLine sSlot in canvas.linesV) {
                    sSlot.Left.Pixels = sSlot.Left.Pixels * zoomCH;
                    sSlot.Top.Pixels = sSlot.Top.Pixels * zoomCH;
                    sSlot.Width.Pixels = sSlot.Width.Pixels * zoomCH;
                    sSlot.Height.Pixels = sSlot.Height.Pixels * zoomCH;
                }

                _horizontalScrollbar.SetView(canvas.Width.Pixels, _notCanvas.GetInnerDimensions().Width);

                float HC = canvas.Height.Pixels;
                float HW = _notCanvas.GetInnerDimensions().Height;
                if (HC > HW) {
                    float A = HC;
                    HC = HW;
                    HW = A;
                }
                _verticalScrollbar.SetView(HC, HW);

                KeepCanvasInside();
                UpdateScrollBars();
            }
            base.ScrollWheel(evt);


        }

        public override void LeftMouseDown(UIMouseEvent evt) {
            base.LeftMouseDown(evt);

            if (UISystem.instance.MenuBar.open) {
                if (_treeButton.ContainsPoint(evt.MousePosition)) {
                    openPage=0;
                    _hideElement.Append(_componentsPage); //i'm sure there's a better way
                    Append(_treePage);
                    ReAppendButtons();
                } else if (_itemsButton.ContainsPoint(evt.MousePosition)) {
                    openPage=1;
                    _hideElement.Append(_treePage);
                    Append(_componentsPage);
                    ReAppendButtons();
                } else if(_expandButton.ContainsPoint(evt.MousePosition)) {
                    _offset = new Vector2(evt.MousePosition.X - Width.Pixels, evt.MousePosition.Y-Height.Pixels);
                    ReSizing = true;
               } else if (_notCanvas.ContainsPoint(evt.MousePosition)) {
                    _offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
                    Dragging = true;
                } else if (openPage == 0) {
                    if (_horizontalScrollbar.ContainsPoint(evt.MousePosition)) {
                        HorizontalScrolling = true;
                    } else if (_verticalScrollbar.ContainsPoint(evt.MousePosition)) {
                        VerticalScrolling = true;
                    }
                }
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt) {
            base.LeftMouseUp(evt);
            Dragging = false;
            ReSizing = false;
            HorizontalScrolling=false;
            VerticalScrolling=false;
        }

        public override void RightMouseDown(UIMouseEvent evt) {
            base.RightMouseDown(evt);
            if (openPage == 0) {
                _dragOffset = new Vector2(evt.MousePosition.X - canvas.Left.Pixels, evt.MousePosition.Y - canvas.Top.Pixels);
                DraggingCanvas = true;
            }
        }

        public override void RightMouseUp(UIMouseEvent evt) {
            base.RightMouseUp(evt);
            DraggingCanvas = false;
        }

        public void UpdateScrollBars() {
            if (canvas.Width.Pixels > _notCanvas.GetInnerDimensions().Width) {
                _horizontalScrollbar.ViewPosition = -canvas.Left.Pixels;
            } else {
                _horizontalScrollbar.ViewPosition = -canvas.Left.Pixels + _notCanvas.GetInnerDimensions().Width - canvas.Width.Pixels;
            }
            if (canvas.Height.Pixels > _notCanvas.GetInnerDimensions().Height) {
                _verticalScrollbar.ViewPosition = -canvas.Top.Pixels;
            } else {
                _verticalScrollbar.ViewPosition = -canvas.Top.Pixels + _notCanvas.GetInnerDimensions().Height - canvas.Height.Pixels;
            }
        }

        public void KeepCanvasInside() {
            if (canvas.Width.Pixels > _notCanvas.GetInnerDimensions().Width) {
                canvas.Left.Pixels = MathHelper.Clamp(canvas.Left.Pixels, _notCanvas.GetInnerDimensions().Width - canvas.Width.Pixels, 0);
            } else {
                canvas.Left.Pixels = MathHelper.Clamp(canvas.Left.Pixels, 0, _notCanvas.GetInnerDimensions().Width - canvas.Width.Pixels);
            }
            if (canvas.Height.Pixels > _notCanvas.GetInnerDimensions().Height) {
                canvas.Top.Pixels = MathHelper.Clamp(canvas.Top.Pixels, _notCanvas.GetInnerDimensions().Height - canvas.Height.Pixels, 0);
            } else {
                canvas.Top.Pixels = MathHelper.Clamp(canvas.Top.Pixels, 0, _notCanvas.GetInnerDimensions().Height - canvas.Height.Pixels);
            }
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            if (Dragging) {
                Left.Set(Main.mouseX - _offset.X, 0f);
                Top.Set(Main.mouseY - _offset.Y, 0f);

                ClampToScreen();
            } else
            if (DraggingCanvas) {
    
                if (canvas.Width.Pixels > _notCanvas.GetInnerDimensions().Width) {
                    canvas.Left.Pixels = MathHelper.Clamp(Main.mouseX - _dragOffset.X, _notCanvas.GetInnerDimensions().Width - canvas.Width.Pixels, 0);
                } else {
                    canvas.Left.Pixels = MathHelper.Clamp(Main.mouseX - _dragOffset.X, 0, _notCanvas.GetInnerDimensions().Width - canvas.Width.Pixels);
                }
                if (canvas.Height.Pixels > _notCanvas.GetInnerDimensions().Height) {
                    canvas.Top.Pixels = MathHelper.Clamp(Main.mouseY - _dragOffset.Y, _notCanvas.GetInnerDimensions().Height - canvas.Height.Pixels, 0);
                } else {
                    canvas.Top.Pixels = MathHelper.Clamp(Main.mouseY - _dragOffset.Y, 0, _notCanvas.GetInnerDimensions().Height - canvas.Height.Pixels);
                }
                UpdateScrollBars();

            } else if (ReSizing) {
                Width.Pixels = MathHelper.Clamp(Main.mouseX - _offset.X,150,2000);
                Height.Pixels = MathHelper.Clamp(Main.mouseY - _offset.Y, 150, 2000);
                KeepCanvasInside();

                _horizontalScrollbar.SetView(canvas.Width.Pixels, _notCanvas.GetInnerDimensions().Width);
                float HC = canvas.Height.Pixels;
                float HW = _notCanvas.GetInnerDimensions().Height;
                if (HC > HW) {
                    float A = HC;
                    HC = HW;
                    HW = A;
                }
                _verticalScrollbar.SetView(HC, HW);
                UpdateScrollBars();

            } else if(HorizontalScrolling) {
                if (canvas.Width.Pixels > _notCanvas.GetInnerDimensions().Width) {
                    canvas.Left.Pixels = -_horizontalScrollbar.ViewPosition;
                } else {
                    canvas.Left.Pixels = -_horizontalScrollbar.ViewPosition + _notCanvas.GetInnerDimensions().Width - canvas.Width.Pixels;
                }
            } else if (VerticalScrolling) {
                if (canvas.Height.Pixels > _notCanvas.GetInnerDimensions().Height) {
                    canvas.Top.Pixels = -_verticalScrollbar.ViewPosition;
                } else {
                    canvas.Top.Pixels = -_verticalScrollbar.ViewPosition + _notCanvas.GetInnerDimensions().Height - canvas.Height.Pixels;
                }
            }
        }

        // Prevents the panel from being dragged off-screen
        private void ClampToScreen() {
            var parentDimensions = Parent.GetDimensions().ToRectangle();
            Left.Pixels = MathHelper.Clamp(Left.Pixels, 0, parentDimensions.Width - Width.Pixels);
            Top.Pixels = MathHelper.Clamp(Top.Pixels, 0, parentDimensions.Height - Height.Pixels);
        }

        private int VLineDim;
        private int HLineDim;
        private Color LineColor;

        internal void updateTree() {
            int GridDist = 70;
            //UISystem.instance.MenuBar.open= !UISystem.instance.MenuBar.open;
            TreeStep mainStep = RecipeDecomposer.fullTree;
            VLineDim = ModContent.GetInstance<CraftingConfig>().VerticalLineThickness;
            HLineDim = ModContent.GetInstance<CraftingConfig>().HorizontalLineThickness;
            LineColor = ModContent.GetInstance<CraftingConfig>().LineColor;

            this.canvas.Width.Pixels = ((GridDist * mainStep.distinctIngredients + 18) * DraggableUIPanel.Zoom);
            this.canvas.Height.Pixels = ((GridDist * (mainStep.depth + 1) + 18) * DraggableUIPanel.Zoom);

            this._horizontalScrollbar.SetView(this.canvas.Width.Pixels, this._notCanvas.GetInnerDimensions().Width);
            this._horizontalScrollbar.ViewPosition = 10000;
            float HC = this.canvas.Height.Pixels;
            float HW = this._notCanvas.GetInnerDimensions().Height;
            if (HC > HW) {
                float A = HC;
                HC = HW;
                HW = A;
            }
            this._verticalScrollbar.SetView(HC, HW);
            this._verticalScrollbar.ViewPosition = 10000;

            foreach (UIItemSlot2 itemSl in this.canvas.slots) {
                this.canvas.RemoveChild(itemSl);
                itemSl.Remove();
            }
            this.canvas.slots = new List<UIItemSlot2>();
            foreach (UIHorizontalLine itemSl in this.canvas.lines) {
                this.canvas.RemoveChild(itemSl);
                itemSl.Remove();
            }
            this.canvas.lines = new List<UIHorizontalLine>();
            foreach (UIVerticalLine itemSl in this.canvas.linesV) {
                this.canvas.RemoveChild(itemSl);
                itemSl.Remove();
            }
            this.canvas.linesV = new List<UIVerticalLine>();
            this.canvas.Left.Pixels = 0;
            this.canvas.Top.Pixels = 0;
            //RViewerLogger.LogMsg("Generating UI");
            buildTree(mainStep, GridDist, 0, 0, false, true);
            this._componentsPage.SetItem(mainStep.itemId);
        }

        internal float buildTree(TreeStep mainStep, int gridSize, int depthNode = 0, int passedItems = 0, bool reversed = false, bool starting = false) {

            float Scale = DraggableUIPanel.Zoom;
            UIItemSlot2 NewSlot = new UIItemSlot2(mainStep.itemId, mainStep.usedAmount, mainStep.extraAmount);
            if (mainStep.recipeGroup != -1) {
                NewSlot.ChangeHoverName(mainStep.recipeGroup);
            }


            //this.canvas.Append(new UIVerticalLine(0, -4, 2, 4, Color.Red));
            //this.canvas.Append(new UIVerticalLine(this.canvas.Height.Pixels, 4, this.canvas.Width.Pixels-2, 4, Color.Red));

            mainStep.slot = NewSlot;
            //Main.NewText((passedItems + (float)Math.Min(mainStep.distinctIngredients - 1, 0) / 2));
            //Main.NewText((depthNode - 1));
            float XPos = gridSize * (passedItems + (float)(mainStep.distinctIngredients - 1) / 2) + 19;
            XPos = XPos * Scale;
            NewSlot.Left.Set(XPos, 0);
            //NewSlot.Left.Set(0,XPos/this.canvas.Width.Pixels);
            float YPos = gridSize * (-depthNode - 1) + 3;
            YPos = YPos * Scale + this.canvas.Height.Pixels;
            NewSlot.Top.Set(YPos, 0);
            //NewSlot.Top.Set(0, YPos / this.canvas.Height.Pixels);
            this.canvas.Append(NewSlot);
            this.canvas.slots.Add(NewSlot);

            if (starting) {
                //RViewerLogger.LogMsg($"Slot1:{XPos},{YPos}");
            }

            if (!starting && VLineDim > 0) {
                UIVerticalLine VLine = new UIVerticalLine(YPos + 50 * Scale, 10 * Scale, XPos + (25 - VLineDim / 2) * Scale, VLineDim * Scale, LineColor);
                this.canvas.Append(VLine);
                this.canvas.linesV.Add(VLine);
            }

            bool sons = false;
            float Left = 0; float Right = 0;
            foreach (TreeStep son in mainStep.Ingredients) {
                float N = buildTree(son, gridSize, depthNode + 1, passedItems);
                if (!sons) {
                    sons = true;
                    Left = N;
                    Right = N;
                }
                Right = N;
                passedItems += son.distinctIngredients;
            }
            if (starting) {
                //RViewerLogger.LogMsg($"Has {mainStep.Ingredients.Count} children");
                //RViewerLogger.LogMsg($"L/R {Left}/{Right}");
            }

            if (Left != Right && HLineDim > 0) {
                UIHorizontalLine NewLine = new UIHorizontalLine(Left + (25 - VLineDim / 2) * Scale, Right + (25 + VLineDim / 2) * Scale, YPos - (10 + HLineDim / 2) * Scale, HLineDim * Scale, LineColor);
                this.canvas.Append(NewLine);
                this.canvas.lines.Add(NewLine);
            }
            //RViewerLogger.LogMsg("Drew finalH");

            if (sons && VLineDim > 0) {
                UIVerticalLine VLine = new UIVerticalLine(YPos, -10 * Scale, XPos + (25 - VLineDim / 2) * Scale, VLineDim * Scale, LineColor);
                this.canvas.Append(VLine);
                this.canvas.linesV.Add(VLine);
            }
            //RViewerLogger.LogMsg("Drew finalV");

            return XPos;
        }

    }

    public class UIHorizontalLine : UIElement {
        private readonly Color _color;

        public UIHorizontalLine(float startX, float endX, float yPos, float thickness, Color color) {
            _color = color;

            // Calculate dimensions
            float width = Math.Abs(endX - startX);
            float left = Math.Min(startX, endX);

            Left.Set(left, 0f);
            Top.Set(yPos, 0f);
            Width.Set(width, 0f);
            Height.Set(thickness, 0f);

            // Create static texture once
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle dimensions = GetDimensions();

            // Draw line using stretched texture
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle((int)dimensions.X, (int)dimensions.Y,
                            (int)(Width.Pixels+0.5f), (int)(Height.Pixels+ 0.5f)),
                _color);
        }
    }

    public class UIVerticalLine : UIElement {
        private readonly Color _color;

        public UIVerticalLine(float startY, float H, float xPos, float thickness, Color color) {
            _color = color;

            Left.Set(xPos, 0f);
            Width.Set(thickness, 0f);
            // Calculate dimensions
            if (H > 0) {
                Top.Set(startY, 0f);
                Height.Set(H, 0f);
            } else {
                Top.Set(startY+H, 0f);
                Height.Set(-H, 0f);
            }


        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle dimensions = GetDimensions();

            // Draw line using stretched texture
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle((int)dimensions.X, (int)dimensions.Y,
                            (int)(Width.Pixels + 0.5f), (int)(Height.Pixels + 0.5f)),
                _color);
        }
    }




    public class UIItemSlot2 : UIPanel {
        internal int _itemId;
        private int _stack;
        private float _scale;
        private Item item;
        internal UIText _iconText;
        internal UIText _stackText;

        public UIItemSlot2(int itemId, int stack,int extra) {
            item = new Item(itemId,stack);
            _scale= DraggableUIPanel.Zoom;
            _itemId = itemId;
            _stack = stack;
            Width.Set(50f*_scale, 0f);
            Height.Set(50f*_scale, 0f);
            //BackgroundColor= Color.Blue;
            _iconText = new UIText($"[i:{_itemId}]", _scale * 1.6f);
            _iconText.Top.Set(-2f - 2f *_scale, 0f);
            _iconText.HAlign = 0.5f;
            _iconText.VAlign = 0.5f;
            Append(_iconText);

            if (_stack > 1||extra>0) {
                String nums = _stack.ToString();
                if (extra!=0) { nums += $"[c/FF0000:+{extra}]"; }
                _stackText = new UIText(nums, _scale);
                _stackText.Left.Set(8f * _scale, 0f);
                _stackText.Top.Set(-3 + 18f * (float)Math.Pow(_scale, 1.5f), 0f);
                _stackText.Width.Set(0f, 1f);
                _stackText.Height.Set(0f, 1f);
                _stackText.HAlign = 1f;
                Append(_stackText);
            }


        }


        protected override void DrawSelf(SpriteBatch spriteBatch) {
            if (ContainsPoint(Main.MouseScreen)) {
                Main.HoverItem = item;
                Main.hoverItemName = item.Name;
            }
            base.DrawSelf(spriteBatch);
        }

        //IDK
        /*public override void LeftMouseDown(UIMouseEvent evt) {
            //if (UISystem.instance.MenuBar.open) {
                if (ContainsPoint(evt.MousePosition)&& UISystem.instance.MenuBar.open) {
                    //Main.NewText("B");
                    MultiCraftingPlayer.instance.NewRecipe();
                }
            base.LeftMouseDown(evt);
            //}
        }*/

        internal void ChangeHoverName(int GroupId) {
            item.SetNameOverride(Language.GetTextValue(RecipeGroup.recipeGroups[GroupId].GetText()));
        }
        
    }
}
