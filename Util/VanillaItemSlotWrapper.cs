﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace ExampleMod.UI {

    // This class wraps the vanilla ItemSlot class into a UIElement. The ItemSlot class was made before the UI system was made, so it can't be used normally with UIState. 
    // By wrapping the vanilla ItemSlot class, we can easily use ItemSlot.
    // ItemSlot isn't very modder friendly and operates based on a "Context" number that dictates how the slot behaves when left, right, or shift clicked and the background used when drawn. 
    // If you want more control, you might need to write your own UIElement.
    // I've added basic functionality for validating the item attempting to be placed in the slot via the validItem Func. 
    // See ExamplePersonUI for usage and use the Awesomify chat option of Example Person to see in action.
    internal class VanillaItemSlotWrapper : UIElement
	{
		internal Item Item;
		private readonly int _context;
		private readonly float _scale;
		internal Func<Item, bool> ValidItemFunc;
        internal Action OnSetItem;
        public bool filterStyle = false;
        public Vector2 relativePos = new Vector2(0, 0);

		public VanillaItemSlotWrapper(int context = ItemSlot.Context.BankItem, float scale = 1f) {
			_context = context;
			_scale = scale;
			Item = new Item();
			Item.SetDefaults(0);

			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);

            OnMouseDown += ButtonClicked;
        }

        private void ButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
            if (!filterStyle) return;
            if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
                Main.PlaySound(SoundID.MenuTick);
                Item = Main.mouseItem.Clone();
                Item.stack = 1;
                Item.prefix = 0;
                Item.favorited = false;
                if (OnSetItem != null) OnSetItem();
            }
        }

        public void DrawSlot(SpriteBatch spriteBatch) {
            DrawSelf(spriteBatch);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

            //if (ContainsPoint(Main.MouseScreen)) Main.NewText("contains");
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
                    // Handle handles all the click and hover actions based on the context.
                    Item it = Item;
                    int st = it.stack;
					if(!filterStyle) ItemSlot.Handle(ref Item, _context);
                    if ((Item != it || Item.stack != st) && OnSetItem != null) OnSetItem();
				}
			}
            // Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
            try {
                ItemSlot.Draw(spriteBatch, ref Item, _context, rectangle.TopLeft() + relativePos);
            }catch(Exception e) {
                Main.NewText(e);
            }
			Main.inventoryScale = oldScale;
		}

        public override bool ContainsPoint(Vector2 point) {
            
            float width = Main.inventoryBack9Texture.Width * _scale;
            float height = Main.inventoryBack9Texture.Height * _scale;
            
            if(relativePos.X != 0 || relativePos.Y != 0) {
                if (point.X > relativePos.X && point.Y > relativePos.Y && point.X < relativePos.X + width) {
                    return point.Y < relativePos.Y + height;
                }
                return false;
            }

            return base.ContainsPoint(point - relativePos);
        }
    }
}