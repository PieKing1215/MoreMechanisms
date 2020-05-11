using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace MoreMechanisms {
    /// <summary>
    /// UIImageButton but it doesn't make the hover sound
    /// </summary>
    class UISilentImageButton : UIImageButton {

        public UISilentImageButton(Texture2D tex) : base(tex) {}

        public override void MouseOver(UIMouseEvent evt) {
            // call UIElement.MouseOver instead of UIImageButton.MouseOver
            var ptr = typeof(UIElement).GetMethod("MouseOver").MethodHandle.GetFunctionPointer();
            var baseSay = (Action<UIMouseEvent>)Activator.CreateInstance(typeof(Action<UIMouseEvent>), this, ptr);
            baseSay(evt);
        }
    }
}
