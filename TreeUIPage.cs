using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace MultiCrafting {
    internal class TreeUIPage : UIElement {
        internal DraggableUIPanel mainPanel;
        public TreeUIPage() {
            Width.Set(0, 1f);
            Height.Set(0, 1f);
        }

        /*public override void Draw(SpriteBatch spriteBatch) {
            if (mainPanel.openPage==0)
                base.Draw(spriteBatch);
        }*/
    }
}
