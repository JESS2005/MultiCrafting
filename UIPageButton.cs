using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.UI;
using Terraria.ID;
using MultiCrafting;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI.Chat;

namespace MultiCrafting {
	public class UIPageButton : UIElement
	{
		private Asset<Texture2D> _texture;
		private Color color=new Color(44, 57, 105, 178);
		private UIText text;
		private string hoverText;


		public UIPageButton(string hoverText) {
            this.Height.Set(0f, 1f);
            this._texture = ModContent.Request<Texture2D>("MultiCrafting/Textures/Button");
            //OverflowHidden = true;
            text = new UIText("testtesttest");
            text.DynamicallyScaleDownToWidth = true;
            text.MaxWidth = new StyleDimension(-1f, 1f);

            text.Width.Set(50f, 0f);
            text.Height.Set(-7f, 1f);
            text.HAlign = 0.5f;
            text.VAlign = 0.5f;
            Append(text);

            this.hoverText = hoverText;
        }

        public void RecalculateText() {
		}

		public void SetColor(Color color) {
			this.color = color;
		}

		private void DrawBar(SpriteBatch spriteBatch, Texture2D texture, Rectangle dimensions, Color color)
		{
    		spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, 10, dimensions.Height), new Rectangle(0, 0, 10, texture.Height), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X+10, dimensions.Y, dimensions.Width-20, dimensions.Height), new Rectangle(10, 0, 4, texture.Height), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X + dimensions.Width-10, dimensions.Y, 10, dimensions.Height), new Rectangle(texture.Width - 10, 0, 10, texture.Height), color);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetDimensions();

			this.DrawBar(spriteBatch, _texture.Value, dimensions.ToRectangle(), color);
            if (ContainsPoint(Main.MouseScreen)) {
                DrawTooltipBackground(hoverText, Color.White);
                //Main.hoverItemName = "test"; // Set tooltip text
                //Main.mouseText = true; // Force display tooltip UI
            }
        }

        private void DrawTooltipBackground(string text, Color textColor = default) { //Thank you Javid
            if (text == "")
                return;

            int padd = 10;
            Vector2 stringVec = FontAssets.MouseText.Value.MeasureString(text);
            Rectangle bgPos = new Rectangle(Main.mouseX + 20, Main.mouseY + 20, (int)stringVec.X+ padd+6, (int)stringVec.Y + padd - 5);
            bgPos.X = Utils.Clamp(bgPos.X, 0, Main.screenWidth - bgPos.Width);
            bgPos.Y = Utils.Clamp(bgPos.Y, 0, Main.screenHeight - bgPos.Height);

            Vector2 textPos = new Vector2(bgPos.X +3+ padd / 2, bgPos.Y + padd / 2);
            if (textColor == default) {
                textColor = Main.MouseTextColorReal;
            }

            Utils.DrawInvBG(Main.spriteBatch, bgPos, new Color(23, 25, 81, 255) * 0.925f);
            Utils.DrawBorderString(Main.spriteBatch, text, textPos, textColor);
        }

        /// <summary>
        /// Removes chat tags from the decalred mod's displayname, presenting it in its pure text form.
        /// </summary>
        //public static string RemoveChatTags(Mod mod) => RemoveChatTags(mod.DisplayName);
        //public static string RemoveChatTags(string text) => string.Join("", ChatManager.ParseMessage(text, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text));
    }
}