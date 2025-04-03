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
	public class UIPageButtonCirc : UIElement
	{
		private Asset<Texture2D> _texture;
		private Color color=Color.White;
		private UIText text;
		private string hoverText;


		public UIPageButtonCirc(string hoverText) {
            this.Height.Set(10f, 0f);
            this.Width.Set(10f, 0f);
            this._texture = ModContent.Request<Texture2D>("MultiCrafting/Textures/ButtonCirc");
            this.hoverText = hoverText;
        }

		public void SetColor(Color color) {
			this.color = color;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetDimensions();

			spriteBatch.Draw(_texture.Value, dimensions.ToRectangle(), color);
            if (ContainsPoint(Main.MouseScreen)) {
                DrawTooltipBackground(hoverText, Color.White);
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
    }
}