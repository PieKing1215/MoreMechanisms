using ExampleMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MoreMechanisms.Items;
using MoreMechanisms.Tiles;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

/*  -- IDEAS --
    counter
    configurable timer
    forcefields
    pistons
    
    auto crafting tile
    
    way to copy paste item/entity filters
      (maybe like factorio?)

    -- PARTIALLY IMPLEMENTED -- 
    
    delay block

    vacuum (hopper): suck items from world into inventory
      toggleable by wire

    itemducts: take items from inventories and push to others
      TODO: maybe change to separate connector piece and separate filter piece
      filter what items to take or push
      TODO: integrate with magic storage

    smart quarry which can selectively mine blocks or replace blocks
      make it so it has a small range on its own but you can build a frame to make it bigger
      make it have an inventory slot to put a pickaxe in and then it uses the stats of the pickaxe

    turret with inventory for bullets
      configure which entities to shoot
      multiple tiers unlocked as game progresses
      has max "power" and stats can be upgraded up to the power level (like ftl)
        speed, damage, knockback, etc.
        
    auto selling tile
    
    item dropping tile

    grand design upgrade with dedicated inventory slots and the right click menu having extra buttons for mechanisms

     */

namespace MoreMechanisms {
    public class MoreMechanisms : Mod {

        public static MoreMechanisms instance;

        public static HitTile globalHitTile = new HitTile();

        Texture2D checkboxTexture;
        Texture2D checkmarkTexture;
        Texture2D xTexture;

        Texture2D plusTexture;
        Texture2D minusTexture;

        internal UserInterface speakerUI;
        internal SpeakerUIState speakerUIState;

        internal UserInterface entitySensorUI;
        internal EntitySensorUIState entitySensorUIState;

        internal UserInterface itemFilterUI;
        internal FilterUIState itemFilterUIState;

        internal UserInterface quarryUI;
        internal QuarryUIState quarryUIState;

        internal UserInterface turretUI;
        internal TurretUIState turretUIState;

        public class SpeakerUIState : UIState {
            
            UIPanel panel;
            UIImage globalCheckboxMark;
            UIText volumeText;
            UIText pitchText;
            internal int i, j;
            List<UIPanel> buttons;

            public override void OnInitialize() {

                panel = new UIPanel();
                panel.Width.Set(700, 0);
                panel.Height.Set(500, 0);
                panel.HAlign = 0.5f;
                panel.VAlign = 0.25f;
                
                UIText header = new UIText("Speaker", 1.2f);
                header.HAlign = 0.5f; // 1
                header.Top.Set(5, 0); // 2
                panel.Append(header);

                UIElement global = new UIElement();
                global.Top.Set(30, 0);
                global.Left.Set(30, 0);
                global.Width.Set(160, 0);
                global.Height.Set(25, 0);
                global.SetPadding(0);

                UIImage globalCheckbox = new UIImage(MoreMechanisms.instance.checkboxTexture);
                globalCheckbox.Top.Set(0, 0);
                globalCheckbox.Left.Set(5, 0);
                //globalCheckbox.VAlign = 0.5f;
                global.Append(globalCheckbox);

                globalCheckboxMark = new UIImage(MoreMechanisms.instance.checkmarkTexture);
                globalCheckbox.Append(globalCheckboxMark);

                UIText globalCheckboxText = new UIText("Global playback", 1f);
                globalCheckboxText.HAlign = 0f;
                globalCheckboxText.VAlign = 0.5f;
                globalCheckboxText.Left.Set(22, 0);
                globalCheckboxText.Top.Set(4, 0);
                globalCheckbox.Append(globalCheckboxText);

                global.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {

                    Main.PlaySound(SoundID.MenuTick);

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16) - tile.frameX % 36 / 18;
                    int top = (this.j / 16) - tile.frameY / 18;

                    int index = GetInstance<TESpeaker>().Find(left, top);
                    if (index != -1) {
                        TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];

                        speakerEnt.global = !speakerEnt.global;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            speakerEnt.changed = true;
                        } else {
                            speakerEnt.ClientSendServer();
                        }
                        
                        globalCheckboxMark.SetImage(speakerEnt.global ? MoreMechanisms.instance.checkmarkTexture : MoreMechanisms.instance.xTexture);
                    }

                };

                panel.Append(global);

                volumeText = new UIText("Volume: 100%");
                volumeText.Top.Set(65, 0);
                volumeText.Left.Set(30 + 30, 0);
                panel.Append(volumeText);

                UIImageButton volumePlus = new UISilentImageButton(MoreMechanisms.instance.plusTexture);
                volumePlus.Top.Set(65, 0);
                volumePlus.Left.Set(30 - 10 + 20, 0);
                volumePlus.Width.Set(25, 0);
                volumePlus.Height.Set(25, 0);
                volumePlus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 10;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16) - tile.frameX % 36 / 18;
                    int top = (this.j / 16) - tile.frameY / 18;

                    int index = GetInstance<TESpeaker>().Find(left, top);
                    if (index != -1) {
                        TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];

                        speakerEnt.volume += amt;
                        if (speakerEnt.volume > 200) speakerEnt.volume = 200;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            speakerEnt.changed = true;
                        } else {
                            speakerEnt.ClientSendServer();
                        }
                        volumeText.SetText("Volume: " + speakerEnt.volume + "%");
                    }

                };
                panel.Append(volumePlus);
                
                UIImageButton volumeMinus = new UISilentImageButton(MoreMechanisms.instance.minusTexture);
                volumeMinus.Top.Set(65, 0);
                volumeMinus.Left.Set(30 - 5, 0);
                volumeMinus.Width.Set(25, 0);
                volumeMinus.Height.Set(25, 0);
                volumeMinus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 10;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16) - tile.frameX % 36 / 18;
                    int top = (this.j / 16) - tile.frameY / 18;

                    int index = GetInstance<TESpeaker>().Find(left, top);
                    if (index != -1) {
                        TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];

                        speakerEnt.volume -= amt;
                        if (speakerEnt.volume < 0) speakerEnt.volume = 0;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            speakerEnt.changed = true;
                        } else {
                            speakerEnt.ClientSendServer();
                        }
                        volumeText.SetText("Volume: " + speakerEnt.volume + "%");
                    }

                };
                panel.Append(volumeMinus);

                pitchText = new UIText("pitch: 100%");
                pitchText.Top.Set(90, 0);
                pitchText.Left.Set(30 + 30, 0);
                panel.Append(pitchText);

                UIImageButton pitchPlus = new UISilentImageButton(MoreMechanisms.instance.plusTexture);
                pitchPlus.Top.Set(90, 0);
                pitchPlus.Left.Set(30 - 10 + 20, 0);
                pitchPlus.Width.Set(25, 0);
                pitchPlus.Height.Set(25, 0);
                pitchPlus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 10;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16) - tile.frameX % 36 / 18;
                    int top = (this.j / 16) - tile.frameY / 18;

                    int index = GetInstance<TESpeaker>().Find(left, top);
                    if (index != -1) {
                        TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];

                        speakerEnt.pitch += amt;
                        if (speakerEnt.pitch > 200) speakerEnt.pitch = 200;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            speakerEnt.changed = true;
                        } else {
                            speakerEnt.ClientSendServer();
                        }
                        pitchText.SetText("Pitch: " + (speakerEnt.pitch >= 100 ? "+" : "") + (speakerEnt.pitch - 100) + "%");
                    }

                };
                panel.Append(pitchPlus);
                
                UIImageButton pitchMinus = new UISilentImageButton(MoreMechanisms.instance.minusTexture);
                pitchMinus.Top.Set(90, 0);
                pitchMinus.Left.Set(30 - 5, 0);
                pitchMinus.Width.Set(25, 0);
                pitchMinus.Height.Set(25, 0);
                pitchMinus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 10;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16) - tile.frameX % 36 / 18;
                    int top = (this.j / 16) - tile.frameY / 18;

                    int index = GetInstance<TESpeaker>().Find(left, top);
                    if (index != -1) {
                        TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];

                        speakerEnt.pitch -= amt;
                        if (speakerEnt.pitch < 0) speakerEnt.pitch = 0;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            speakerEnt.changed = true;
                        } else {
                            speakerEnt.ClientSendServer();
                        }
                        pitchText.SetText("Pitch: " + (speakerEnt.pitch >= 100 ? "+" : "") + (speakerEnt.pitch - 100) + "%");
                    }

                };
                panel.Append(pitchMinus);

                UIList list = new UIList();
                list.Top.Pixels = 128f;
                list.Width.Set(-32f, 1f);
                list.Height.Set(-128f - 4, 1f);
                list.ListPadding = 2f;
                // temporary workaround until https://github.com/tModLoader/tModLoader/pull/749 gets merged
                list.OnScrollWheel += OnScrollWheel_FixHotbarScroll;
                panel.Append(list);

                FixedUIScrollbar scroll = new FixedUIScrollbar();
                scroll.SetView(100f, 1000f);
                scroll.Top.Pixels = 128f;
                scroll.Height.Set(-128f - 4, 1f);
                scroll.HAlign = 1f;
                panel.Append(scroll);
                list.SetScrollbar(scroll);

                var listOfFieldNames = typeof(SoundID).GetFields();
                //Main.NewText(listOfFieldNames.Length);
                //for (int ii = 0; ii < listOfFieldNames.Length; ii++) {
                //    Main.NewText(ii + " " + listOfFieldNames[ii].Name);
                //}

                buttons = new List<UIPanel>();
                int cols = 4;
                for (int i = 0; i < listOfFieldNames.Length; i += cols) {
                    UIElement container = new UISortableElement(i);
                    container.Width.Set(0f, 1f);
                    container.Height.Set(23f, 0f);
                    container.HAlign = 0.5f;
                    list.Add(container);

                    for (int j = 0; j < cols; j++) {
                        int e = i + j;
                        if (e >= listOfFieldNames.Length) break;

                        UIPanel button = new UIPanel();
                        button.Width.Set(0, 1f / cols - 0.005f);
                        button.Height.Set(23, 0);
                        button.HAlign = (j / (float)(cols - 1));

                        buttons.Add(button);

                        var name = listOfFieldNames[e].Name;
                        if (name.StartsWith("DD2_")) name = name.Substring(4);

                        UIText text = new UIText(name, name.Length > 19 ? 0.65f : 0.75f);
                        text.HAlign = text.VAlign = 0.5f;
                        button.Append(text);
                        
                        button.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                            ButtonClicked(e, evt, listeningElement);
                        };
                        container.Append(button);
                    }
                }

                list.Recalculate();
                Append(panel);
            }
            
            public override void OnActivate() {
                base.OnActivate();
                Tile tile = Main.tile[this.i / 16, this.j / 16];
                if (tile == null) return;
                int left = (this.i / 16) - tile.frameX % 36 / 18;
                int top = (this.j / 16) - tile.frameY / 18;

                int index = GetInstance<TESpeaker>().Find(left, top);
                if (index != -1) {
                    TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];

                    globalCheckboxMark.SetImage(speakerEnt.global ? MoreMechanisms.instance.checkmarkTexture : MoreMechanisms.instance.xTexture);
                    volumeText.SetText("Volume: " + speakerEnt.volume + "%");
                    pitchText.SetText("Pitch: " + (speakerEnt.pitch >= 100 ? "+" : "") + (speakerEnt.pitch - 100) + "%");
                    for (int ii = 0; ii < buttons.Count; ii++) {
                        buttons[ii].BackgroundColor = (ii == speakerEnt.soundId) ? new Color(128, 255, 128, 178) : new Color(44, 57, 105, 178);
                    }
                }

                
            }

            private void ButtonClicked(int i, UIMouseEvent evt, UIElement listeningElement) {

                for(int ii = 0; ii < buttons.Count; ii++) {
                    buttons[ii].BackgroundColor = (ii == i) ? new Color(128, 255, 128, 178) : new Color(44, 57, 105, 178);
                }
                //Main.NewText(((UIPanel)listeningElement).BackgroundColor.ToString());

                var listOfFieldNames = typeof(SoundID).GetFields();
                var sound = listOfFieldNames[i].GetValue(null);
                if(sound is LegacySoundStyle) {
                    LegacySoundStyle sst = sound as LegacySoundStyle;
                    Main.PlaySound(sst);
                } else if(sound is int) {
                    int snd = (int)sound;
                    Main.PlaySound(snd);
                }
                
                Tile tile = Main.tile[this.i / 16, this.j / 16];
                int left = (this.i / 16) - tile.frameX % 36 / 18;
                int top = (this.j / 16) - tile.frameY / 18;

                int index = GetInstance<TESpeaker>().Find(left, top);
                if (index != -1) {
                    TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];
                    speakerEnt.soundId = i;
                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        speakerEnt.changed = true;
                    } else {
                        speakerEnt.ClientSendServer();
                    }
                }
            }

            internal static void OnScrollWheel_FixHotbarScroll(UIScrollWheelEvent evt, UIElement listeningElement) {
                Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 120);
            }

            public override void Update(GameTime gameTime) {
                base.Update(gameTime);

                if (Main.LocalPlayer.talkNPC != -1) {
                    // When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
                    MoreMechanisms.instance.HideSpeakerUI();
                }

                if (panel.ContainsPoint(Main.MouseScreen)) {
                    Main.LocalPlayer.mouseInterface = true;
                }

            }

            protected override void DrawSelf(SpriteBatch spriteBatch) {
                base.DrawSelf(spriteBatch);

                // This will hide the crafting menu similar to the reforge menu. For best results this UI is placed before "Vanilla: Inventory" to prevent 1 frame of the craft menu showing.
                Main.HidePlayerCraftingMenu = true;
            }
            
        }

        public class EntitySensorUIState : UIState {

            UIText leftText;
            UIText rightText;
            UIText topText;
            UIText bottomText;
            UIPanel panel;

            List<Tuple<string, UIImage>> triggerMarks;

            internal int i, j;

            public override void OnInitialize() {

                panel = new UIPanel();
                panel.Width.Set(320, 0);
                panel.Height.Set(300, 0);
                panel.HAlign = 0.5f;
                panel.VAlign = 0.125f;

                UIText header = new UIText("Entity Sensor", 1.2f);
                header.HAlign = 0.5f;
                header.Top.Set(5, 0);
                panel.Append(header);

                int maxRange = 24;

                leftText = new UIText("Left: 0 tiles");
                leftText.Top.Set(65, 0);
                leftText.Left.Set(10 + 30, 0);
                panel.Append(leftText);

                UIImageButton leftPlus = new UISilentImageButton(MoreMechanisms.instance.plusTexture);
                leftPlus.Top.Set(65, 0);
                leftPlus.Left.Set(10 - 10 + 20, 0);
                leftPlus.Width.Set(25, 0);
                leftPlus.Height.Set(25, 0);
                leftPlus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.left += amt;
                        if (es.left > maxRange) es.left = maxRange;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        leftText.SetText("Left: " + es.left + " tiles");
                    }

                };
                panel.Append(leftPlus);

                UIImageButton leftMinus = new UISilentImageButton(MoreMechanisms.instance.minusTexture);
                leftMinus.Top.Set(65, 0);
                leftMinus.Left.Set(10 - 5, 0);
                leftMinus.Width.Set(25, 0);
                leftMinus.Height.Set(25, 0);
                leftMinus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.left -= amt;
                        if (es.left < -maxRange) es.left = -maxRange;
                        if (es.left < -es.right) es.left = -es.right;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        leftText.SetText("Left: " + es.left + " tiles");
                    }

                };
                panel.Append(leftMinus);
                
                rightText = new UIText("Right: 0 tiles");
                rightText.Top.Set(90, 0);
                rightText.Left.Set(10 + 30, 0);
                panel.Append(rightText);

                UIImageButton rightPlus = new UISilentImageButton(MoreMechanisms.instance.plusTexture);
                rightPlus.Top.Set(90, 0);
                rightPlus.Left.Set(10 - 10 + 20, 0);
                rightPlus.Width.Set(25, 0);
                rightPlus.Height.Set(25, 0);
                rightPlus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.right += amt;
                        if (es.right > maxRange) es.right = maxRange;
                        //if (es.right < -es.right) es.right = -es.right;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        rightText.SetText("Right: " + es.right + " tiles");
                    }

                };
                panel.Append(rightPlus);

                UIImageButton rightMinus = new UISilentImageButton(MoreMechanisms.instance.minusTexture);
                rightMinus.Top.Set(90, 0);
                rightMinus.Left.Set(10 - 5, 0);
                rightMinus.Width.Set(25, 0);
                rightMinus.Height.Set(25, 0);
                rightMinus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.right -= amt;
                        if (es.right < -maxRange) es.right = -maxRange;
                        if (es.right < -es.left) es.right = -es.left;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        rightText.SetText("Right: " + es.right + " tiles");
                    }

                };
                panel.Append(rightMinus);

                topText = new UIText("Top: 0 tiles");
                topText.Top.Set(115, 0);
                topText.Left.Set(10 + 30, 0);
                panel.Append(topText);

                UIImageButton topPlus = new UISilentImageButton(MoreMechanisms.instance.plusTexture);
                topPlus.Top.Set(115, 0);
                topPlus.Left.Set(10 - 10 + 20, 0);
                topPlus.Width.Set(25, 0);
                topPlus.Height.Set(25, 0);
                topPlus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.top += amt;
                        if (es.top > maxRange) es.top = maxRange;

                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        topText.SetText("Top: " + es.top + " tiles");
                    }

                };
                panel.Append(topPlus);

                UIImageButton topMinus = new UISilentImageButton(MoreMechanisms.instance.minusTexture);
                topMinus.Top.Set(115, 0);
                topMinus.Left.Set(10 - 5, 0);
                topMinus.Width.Set(25, 0);
                topMinus.Height.Set(25, 0);
                topMinus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.top -= amt;
                        if (es.top < -maxRange) es.top = -maxRange;
                        if (es.top < -es.bottom) es.top = -es.bottom;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        topText.SetText("Top: " + es.top + " tiles");
                    }

                };
                panel.Append(topMinus);

                bottomText = new UIText("Bottom: 0 tiles");
                bottomText.Top.Set(140, 0);
                bottomText.Left.Set(10 + 30, 0);
                panel.Append(bottomText);

                UIImageButton bottomPlus = new UISilentImageButton(MoreMechanisms.instance.plusTexture);
                bottomPlus.Top.Set(140, 0);
                bottomPlus.Left.Set(10 - 10 + 20, 0);
                bottomPlus.Width.Set(25, 0);
                bottomPlus.Height.Set(25, 0);
                bottomPlus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.bottom += amt;
                        if (es.bottom > maxRange) es.bottom = maxRange;

                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        bottomText.SetText("Bottom: " + es.bottom + " tiles");
                    }

                };
                panel.Append(bottomPlus);

                UIImageButton bottomMinus = new UISilentImageButton(MoreMechanisms.instance.minusTexture);
                bottomMinus.Top.Set(140, 0);
                bottomMinus.Left.Set(10 - 5, 0);
                bottomMinus.Width.Set(25, 0);
                bottomMinus.Height.Set(25, 0);
                bottomMinus.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);

                    int amt = 1;
                    KeyboardState kS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                    if (kS.IsKeyDown(Keys.LeftShift)) {
                        amt = 1;
                    }

                    Tile tile = Main.tile[this.i / 16, this.j / 16];
                    int left = (this.i / 16);
                    int top = (this.j / 16);

                    int index = GetInstance<TEEntitySensor>().Find(left, top);
                    if (index != -1) {
                        TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                        es.bottom -= amt;
                        if (es.bottom < -maxRange) es.bottom = -maxRange;
                        if (es.bottom < -es.top) es.bottom = -es.top;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            es.changed = true;
                        } else {
                            es.ClientSendServer();
                        }
                        bottomText.SetText("Bottom: " + es.bottom + " tiles");
                    }

                };
                panel.Append(bottomMinus);

                // trigger checkboxes

                UIText triggerLabel = new UIText("Trigger On:", 1f);
                triggerLabel.Top.Set(65 - 25, 0);
                triggerLabel.Left.Set(-140f, 1f);
                triggerLabel.Width.Set(160, 0);
                triggerLabel.Height.Set(25, 0);
                panel.Append(triggerLabel);
                
                Tuple<string, string>[] triggers = {
                    Tuple.Create("Players"    , "triggerPlayers"),
                    Tuple.Create("NPCs"       , "triggerNPCs"),
                    Tuple.Create("Items"      , "triggerItems"),
                    Tuple.Create("Coins"      , "triggerCoins"),
                    Tuple.Create("Enemies"    , "triggerEnemies"),
                    Tuple.Create("Projectiles", "triggerProjectiles"),
                    Tuple.Create("Pets"       , "triggerPets"),
                    Tuple.Create("LightPets"  , "triggerLightPets"),
                    Tuple.Create("Minions"    , "triggerMinions"),
                    Tuple.Create("Sentries"   , "triggerSentries")
                };

                triggerMarks = new List<Tuple<string, UIImage>>();

                int ind = 0;
                foreach(var tr in triggers) {
                    UIElement trPlayer = new UIElement();
                    trPlayer.Top.Set(60 + 22 * ind++, 0);
                    trPlayer.Left.Set(-110f, 1f);
                    trPlayer.Width.Set(160, 0);
                    trPlayer.Height.Set(25, 0);
                    trPlayer.SetPadding(0);

                    UIImage trPlayerCheckbox = new UIImage(MoreMechanisms.instance.checkboxTexture);
                    trPlayerCheckbox.Top.Set(0, 0);
                    trPlayerCheckbox.Left.Set(5, 0);
                    //globalCheckbox.VAlign = 0.5f;
                    trPlayer.Append(trPlayerCheckbox);

                    var trPlayerCheckboxMark = new UIImage(MoreMechanisms.instance.checkmarkTexture);
                    trPlayerCheckbox.Append(trPlayerCheckboxMark);

                    triggerMarks.Add(Tuple.Create(tr.Item2, trPlayerCheckboxMark));

                    UIText trPlayerCheckboxText = new UIText(tr.Item1, 1f);
                    trPlayerCheckboxText.HAlign = 0f;
                    trPlayerCheckboxText.VAlign = 0.5f;
                    trPlayerCheckboxText.Left.Set(22, 0);
                    trPlayerCheckboxText.Top.Set(4, 0);
                    trPlayerCheckbox.Append(trPlayerCheckboxText);

                    trPlayer.OnClick += (UIMouseEvent evt, UIElement listeningElement) => {

                        Main.PlaySound(SoundID.MenuTick);

                        Tile tile = Main.tile[this.i / 16, this.j / 16];
                        int left = (this.i / 16);
                        int top = (this.j / 16);

                        int index = GetInstance<TEEntitySensor>().Find(left, top);
                        if (index != -1) {
                            TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];
                            
                            FieldInfo field = typeof(EntityFilter).GetField(tr.Item2);
                            field.SetValue(es.filter, !(bool)field.GetValue(es.filter));

                            if (Main.netMode != NetmodeID.MultiplayerClient) {
                                es.changed = true;
                            } else {
                                es.ClientSendServer();
                            }
                            trPlayerCheckboxMark.SetImage((bool)field.GetValue(es.filter) ? MoreMechanisms.instance.checkmarkTexture : MoreMechanisms.instance.xTexture);
                        }

                    };

                    panel.Append(trPlayer);
                }
                
                Append(panel);
            }

            public override void OnActivate() {
                base.OnActivate();
                Tile tile = Main.tile[this.i / 16, this.j / 16];
                if (tile == null) return;
                int left = (this.i / 16);
                int top = (this.j / 16);

                int index = GetInstance<TEEntitySensor>().Find(left, top);
                if (index != -1) {
                    TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];

                    leftText.SetText("Left: " + es.left + " tiles");
                    rightText.SetText("Right: " + es.right + " tiles");
                    topText.SetText("Top: " + es.top + " tiles");
                    bottomText.SetText("Bottom: " + es.bottom + " tiles");

                    foreach(var tr in triggerMarks) {
                        FieldInfo field = typeof(EntityFilter).GetField(tr.Item1);
                        tr.Item2.SetImage((bool)field.GetValue(es.filter) ? MoreMechanisms.instance.checkmarkTexture : MoreMechanisms.instance.xTexture);
                    }
                }


            }

            private void ButtonClicked(int i, UIMouseEvent evt, UIElement listeningElement) {
                
            }

            internal static void OnScrollWheel_FixHotbarScroll(UIScrollWheelEvent evt, UIElement listeningElement) {
                Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 120);
            }

            public override void Update(GameTime gameTime) {
                base.Update(gameTime);

                if (Main.LocalPlayer.talkNPC != -1) {
                    // When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
                    MoreMechanisms.instance.HideSpeakerUI();
                }

                if (panel.ContainsPoint(Main.MouseScreen)) {
                    Main.LocalPlayer.mouseInterface = true;
                }

            }

            protected override void DrawSelf(SpriteBatch spriteBatch) {
                base.DrawSelf(spriteBatch);

                // This will hide the crafting menu similar to the reforge menu. For best results this UI is placed before "Vanilla: Inventory" to prevent 1 frame of the craft menu showing.
                Main.HidePlayerCraftingMenu = true;
            }

        }

        public class FilterUIState : UIState {
            
            UIPanel panel;
            
            internal int i, j;

            Item[] items;
            bool whitelist;
            Action<Item[], bool> callback;

            public void Load(Item[] items, bool whitelist, Action<Item[], bool> callback) {
                this.callback = callback;
                this.items = items;
                this.whitelist = whitelist;

                RemoveAllChildren();
                OnInitialize();
            }

            public override void OnInitialize() {

                if (items == null) return;

                panel = new UIPanel();
                panel.Width.Set(420, 0);
                panel.Height.Set(140, 0);
                panel.HAlign = 0.5f;
                panel.VAlign = 0.125f;

                UIText header = new UIText("Filter", 1.2f);
                header.HAlign = 0.5f;
                header.Top.Set(3, 0);
                panel.Append(header);

                UIPanel whiteBlackList = new UIPanel();
                whiteBlackList.Width.Set(80, 0);
                whiteBlackList.Height.Set(25, 0);
                whiteBlackList.Left.Set(-whiteBlackList.Width.Pixels, 1f);
                whiteBlackList.Top.Set(0, 0f);
                whiteBlackList.BackgroundColor = whitelist ? Color.White : Color.Black;

                UIText whiteBlackListText = new UIText(whitelist ? "whitelist" : "blacklist", 0.8f);
                whiteBlackListText.HAlign = 0.5f;
                whiteBlackListText.VAlign = 0.5f;
                whiteBlackListText.TextColor = Color.White;
                whiteBlackList.Append(whiteBlackListText);

                whiteBlackList.OnMouseDown += (UIMouseEvent evt, UIElement listeningElement) => {
                    Main.PlaySound(SoundID.MenuTick);
                    if(whiteBlackListText.Text == "whitelist") {
                        whiteBlackListText.SetText("blacklist");
                        whiteBlackList.BackgroundColor = Color.Black;
                    } else {
                        whiteBlackListText.SetText("whitelist");
                        whiteBlackList.BackgroundColor = Color.White;
                    }

                    whitelist = whiteBlackListText.Text == "whitelist";

                    this.callback(this.items, this.whitelist);
                };

                panel.Append(whiteBlackList);

                int nSlots = items.Length;
                for (int si = 0; si < nSlots; si++) {
                    float left = (10 / 400f) + (((1f - ((Main.inventoryBack9Texture.Width + 8 + 10) / 400f)) / (nSlots - 1)) * si);

                    VanillaItemSlotWrapper slot = new VanillaItemSlotWrapper(ItemSlot.Context.BankItem);
                    slot.filterStyle = true;
                    slot.Item = items[si];

                    if(slot.Item == null) {
                        slot.Item = new Item();
                        slot.Item.SetDefaults(0);
                    }

                    slot.Left.Set(0, left);
                    slot.Top.Pixels = 32;

                    UIText slotTitle = new UIText("", 0.65f);
                    slotTitle.HAlign = 0.5f;
                    slotTitle.Left.Set(Main.inventoryBack9Texture.Width / 2f, -0.5f + left);
                    slotTitle.Top.Pixels = 90;

                    UIText slotTitle2 = new UIText("", 0.65f);
                    slotTitle2.HAlign = 0.5f;
                    slotTitle2.Left.Set(Main.inventoryBack9Texture.Width / 2f, -0.5f + left);
                    slotTitle2.Top.Pixels = 105;

                    int sii = si;
                    slot.OnSetItem = () => {

                        string str = "";
                        string str2 = "";

                        bool br = false;
                        for (int i = 0; i < slot.Item.HoverName.Length; i++) {
                            char ch = slot.Item.HoverName[i];
                            if (!br && ch == ' ' && i >= 9) {
                                br = true;
                                str += '\n';
                            } else {
                                if (br) {
                                    str2 += ch;
                                } else {
                                    str += ch;
                                }
                            }
                        }

                        slotTitle.SetText(str);
                        slotTitle2.SetText(str2);

                        this.items[sii] = slot.Item;
                        this.callback(this.items, this.whitelist);
                    };
                    slot.OnSetItem();

                    panel.Append(slotTitle);
                    panel.Append(slotTitle2);
                    panel.Append(slot);
                }

                Append(panel);
            }

            public override void OnActivate() {
                base.OnActivate();
                
            }

            private void ButtonClicked(int i, UIMouseEvent evt, UIElement listeningElement) {

            }

            internal static void OnScrollWheel_FixHotbarScroll(UIScrollWheelEvent evt, UIElement listeningElement) {
                Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 120);
            }

            public override void Update(GameTime gameTime) {
                base.Update(gameTime);

                if (Main.LocalPlayer.talkNPC != -1) {
                    // When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
                    MoreMechanisms.instance.HideItemFilterUI();
                }

                if (panel.ContainsPoint(Main.MouseScreen)) {
                    Main.LocalPlayer.mouseInterface = true;
                }

            }

            protected override void DrawSelf(SpriteBatch spriteBatch) {
                base.DrawSelf(spriteBatch);

                // This will hide the crafting menu similar to the reforge menu. For best results this UI is placed before "Vanilla: Inventory" to prevent 1 frame of the craft menu showing.
                Main.HidePlayerCraftingMenu = true;
            }

        }

        public class QuarryUIState : UIState {

            UIPanel panel;

            internal int i, j;

            Item item;
            Action<Item> callback;
            ItemFilter filter;

            public void Load(Item item, ItemFilter filter, Action<Item> callback) {
                this.callback = callback;
                this.item = item;
                this.filter = filter;

                RemoveAllChildren();
                OnInitialize();
            }

            public override void OnInitialize() {
                if (item == null) return;

                panel = new UIPanel();
                panel.Width.Set(160, 0);
                panel.Height.Set(200, 0);
                panel.HAlign = 0.5f;
                panel.VAlign = 0.125f;

                UIText header = new UIText("Quarry", 1.2f);
                header.HAlign = 0.5f;
                header.Top.Set(3, 0);
                panel.Append(header);

                UISortableElement filterContainer = new UISortableElement(0);

                filterContainer.Width.Set(80, 0);
                filterContainer.Height.Set(20, 0);
                filterContainer.HAlign = 0.5f;
                filterContainer.Top.Set(44, 0);

                UIText filterText = new UIText("Edit filter", 1f);
                filterText.HAlign = 0.5f;
                filterText.VAlign = 0.5f;

                filterContainer.OnMouseOver += (UIMouseEvent evt, UIElement listeningElement) => {
                    filterText.TextColor = Color.Yellow;
                    filterText.SetText("Edit filter", 1.1f, false);
                };

                filterContainer.OnMouseOut += (UIMouseEvent evt, UIElement listeningElement) => {
                    filterText.TextColor = Color.White;
                    filterText.SetText("Edit filter", 1f, false);
                };

                filterContainer.OnMouseDown += (UIMouseEvent evt, UIElement listeningElement) => {
                    if (!MoreMechanisms.instance.ItemFilterUIVisible()) {
                        MoreMechanisms.instance.itemFilterUIState.i = i;
                        MoreMechanisms.instance.itemFilterUIState.j = j;

                        MoreMechanisms.instance.HideQuarryUI();
                        
                        MoreMechanisms.instance.ShowItemFilterUI(filter.filterItems, filter.filterWhitelist, (Item[] items, bool whitelist) => {
                            filter.filterItems = items;
                            filter.filterWhitelist = whitelist;
                        });
                    }
                };

                filterContainer.Append(filterText);
                panel.Append(filterContainer);

                VanillaItemSlotWrapper slot = new VanillaItemSlotWrapper(ItemSlot.Context.BankItem);
                slot.ValidItemFunc = (Item it) => {
                    return it.IsAir || it.pick > 0;
                };
                slot.Item = item;

                if (slot.Item == null) {
                    slot.Item = new Item();
                    slot.Item.SetDefaults(0);
                }

                float left = 0.5f;
                slot.Left.Set(-Main.inventoryBack9Texture.Width / 2f, left);
                slot.Top.Pixels = 32 + 48;

                UIText slotTitle = new UIText("", 0.75f);
                slotTitle.HAlign = 0.5f;
                slotTitle.Left.Set(0, -0.5f + left);
                slotTitle.Top.Pixels = 90 + 48;

                UIText slotTitle2 = new UIText("", 0.75f);
                slotTitle2.HAlign = 0.5f;
                slotTitle2.Left.Set(0, -0.5f + left);
                slotTitle2.Top.Pixels = 105 + 48;

                slot.OnSetItem = () => {

                    string str = "";
                    string str2 = "";

                    bool br = false;
                    for (int i = 0; i < slot.Item.HoverName.Length; i++) {
                        char ch = slot.Item.HoverName[i];
                        if (!br && ch == ' ' && i >= 9) {
                            br = true;
                            str += '\n';
                        } else {
                            if (br) {
                                str2 += ch;
                            } else {
                                str += ch;
                            }
                        }
                    }

                    slotTitle.SetText(str);
                    slotTitle2.SetText(str2);

                    this.item = slot.Item;
                    this.callback(this.item);
                };

                slot.OnSetItem();
                panel.Append(slotTitle);
                panel.Append(slotTitle2);
                panel.Append(slot);

                Append(panel);
            }

            public override void OnActivate() {
                base.OnActivate();
            }

            private void ButtonClicked(int i, UIMouseEvent evt, UIElement listeningElement) {

            }

            internal static void OnScrollWheel_FixHotbarScroll(UIScrollWheelEvent evt, UIElement listeningElement) {
                Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 120);
            }

            public override void Update(GameTime gameTime) {
                base.Update(gameTime);

                if (Main.LocalPlayer.talkNPC != -1) {
                    // When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
                    MoreMechanisms.instance.HideQuarryUI();
                }

                if (panel.ContainsPoint(Main.MouseScreen)) {
                    Main.LocalPlayer.mouseInterface = true;
                }

            }

            protected override void DrawSelf(SpriteBatch spriteBatch) {
                base.DrawSelf(spriteBatch);
                
                for (int y = -1; y >= -10; y--) {
                    for (int x = -5; x <= 5 + 2; x++) {
                        int tx = i/16 + x;
                        int ty = j/16 + y;
                        Tile t = Framing.GetTileSafely(tx, ty);
                        //Tile t = Main.tile[Player.tileTargetX, Player.tileTargetY];
                        if (t != null && t.active() && Main.tileSolid[t.type]) {
                            Util.DrawTile(spriteBatch, t, 500 + 16 * x, 500 + 16 * y);
                        }

                    }
                }


                // This will hide the crafting menu similar to the reforge menu. For best results this UI is placed before "Vanilla: Inventory" to prevent 1 frame of the craft menu showing.
                Main.HidePlayerCraftingMenu = true;
            }

        }

        public class TurretUIState : UIState {

            UIPanel panel;

            internal int i, j;

            Item item;
            Action<Item> callback;

            public void Load(Item item, Action<Item> callback) {
                this.callback = callback;
                this.item = item;

                RemoveAllChildren();
                OnInitialize();
            }

            public override void OnInitialize() {
                if (item == null) return;

                panel = new UIPanel();
                panel.Width.Set(160, 0);
                panel.Height.Set(200, 0);
                panel.HAlign = 0.5f;
                panel.VAlign = 0.125f;

                UIText header = new UIText("Turret", 1.2f);
                header.HAlign = 0.5f;
                header.Top.Set(3, 0);
                panel.Append(header);

                UISortableElement filterContainer = new UISortableElement(0);

                filterContainer.Width.Set(80, 0);
                filterContainer.Height.Set(20, 0);
                filterContainer.HAlign = 0.5f;
                filterContainer.Top.Set(44, 0);

                UIText filterText = new UIText("Edit filter", 1f);
                filterText.HAlign = 0.5f;
                filterText.VAlign = 0.5f;

                filterContainer.OnMouseOver += (UIMouseEvent evt, UIElement listeningElement) => {
                    filterText.TextColor = Color.Yellow;
                    filterText.SetText("Edit filter", 1.1f, false);
                };

                filterContainer.OnMouseOut += (UIMouseEvent evt, UIElement listeningElement) => {
                    filterText.TextColor = Color.White;
                    filterText.SetText("Edit filter", 1f, false);
                };

                filterContainer.Append(filterText);
                panel.Append(filterContainer);

                VanillaItemSlotWrapper slot = new VanillaItemSlotWrapper(ItemSlot.Context.BankItem);
                slot.ValidItemFunc = (Item it) => {
                    return it.IsAir || (!it.notAmmo && it.ammo == AmmoID.Bullet);
                };
                slot.Item = item;

                if (slot.Item == null) {
                    slot.Item = new Item();
                    slot.Item.SetDefaults(0);
                }

                float left = 0.5f;
                slot.Left.Set(-Main.inventoryBack9Texture.Width / 2f, left);
                slot.Top.Pixels = 32 + 48;

                UIText slotTitle = new UIText("", 0.75f);
                slotTitle.HAlign = 0.5f;
                slotTitle.Left.Set(0, -0.5f + left);
                slotTitle.Top.Pixels = 90 + 48;

                UIText slotTitle2 = new UIText("", 0.75f);
                slotTitle2.HAlign = 0.5f;
                slotTitle2.Left.Set(0, -0.5f + left);
                slotTitle2.Top.Pixels = 105 + 48;

                slot.OnSetItem = () => {

                    string str = "";
                    string str2 = "";

                    bool br = false;
                    for (int i = 0; i < slot.Item.HoverName.Length; i++) {
                        char ch = slot.Item.HoverName[i];
                        if (!br && ch == ' ' && i >= 9) {
                            br = true;
                            str += '\n';
                        } else {
                            if (br) {
                                str2 += ch;
                            } else {
                                str += ch;
                            }
                        }
                    }

                    slotTitle.SetText(str);
                    slotTitle2.SetText(str2);

                    this.item = slot.Item;
                    this.callback(this.item);
                };

                slot.OnSetItem();
                panel.Append(slotTitle);
                panel.Append(slotTitle2);
                panel.Append(slot);

                Append(panel);
            }

            public override void OnActivate() {
                base.OnActivate();
            }

            private void ButtonClicked(int i, UIMouseEvent evt, UIElement listeningElement) {

            }

            internal static void OnScrollWheel_FixHotbarScroll(UIScrollWheelEvent evt, UIElement listeningElement) {
                Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 120);
            }

            public override void Update(GameTime gameTime) {
                base.Update(gameTime);

                if (Main.LocalPlayer.talkNPC != -1) {
                    // When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
                    MoreMechanisms.instance.HideTurretUI();
                }

                if (panel.ContainsPoint(Main.MouseScreen)) {
                    Main.LocalPlayer.mouseInterface = true;
                }

            }

            protected override void DrawSelf(SpriteBatch spriteBatch) {
                base.DrawSelf(spriteBatch);

                // This will hide the crafting menu similar to the reforge menu. For best results this UI is placed before "Vanilla: Inventory" to prevent 1 frame of the craft menu showing.
                Main.HidePlayerCraftingMenu = true;
            }

        }
        
        public override void Load() {
            instance = this;
            if (!Main.dedServ) {
                checkboxTexture  = Util.CropTexture(MoreMechanisms.instance.GetTexture("Resources/UICheckbox"), new Rectangle(0, 0, 20, 20));
                checkmarkTexture = Util.CropTexture(MoreMechanisms.instance.GetTexture("Resources/UICheckbox"), new Rectangle(22, 0, 20, 20));
                xTexture         = Util.CropTexture(MoreMechanisms.instance.GetTexture("Resources/UICheckbox"), new Rectangle(44, 0, 20, 20));

                plusTexture  = Util.CropTexture(MoreMechanisms.instance.GetTexture("Resources/UIButtons"), new Rectangle(0, 0, 16, 16));
                minusTexture = Util.CropTexture(MoreMechanisms.instance.GetTexture("Resources/UIButtons"), new Rectangle(18, 0, 16, 16));

                speakerUI = new UserInterface();

                speakerUIState = new SpeakerUIState();
                speakerUIState.Activate();

                entitySensorUI = new UserInterface();

                entitySensorUIState = new EntitySensorUIState();
                entitySensorUIState.Activate();

                itemFilterUI = new UserInterface();

                itemFilterUIState = new FilterUIState();
                itemFilterUIState.Activate();


                quarryUI = new UserInterface();

                quarryUIState = new QuarryUIState();
                quarryUIState.Activate();


                turretUI = new UserInterface();

                turretUIState = new TurretUIState();
                turretUIState.Activate();
            }
        }

        public override void AddRecipes() {
            base.AddRecipes();

            ModRecipe entitySensorRecipe = new ModRecipe(this);
            entitySensorRecipe.AddIngredient(ItemID.LogicSensor_Above, 1);
            entitySensorRecipe.AddIngredient(ItemID.SoulofSight, 10);
            entitySensorRecipe.AddIngredient(ItemID.AdamantiteBar, 5);
            entitySensorRecipe.AddIngredient(ItemID.Wire, 10);
            entitySensorRecipe.AddTile(TileID.Anvils);
            entitySensorRecipe.SetResult(ModContent.ItemType<EntitySensorItem>());
            entitySensorRecipe.AddRecipe();

            ModRecipe entitySensorRecipe2 = new ModRecipe(this);
            entitySensorRecipe2.AddIngredient(ItemID.LogicSensor_Above, 1);
            entitySensorRecipe2.AddIngredient(ItemID.SoulofSight, 10);
            entitySensorRecipe2.AddIngredient(ItemID.TitaniumBar, 5);
            entitySensorRecipe2.AddIngredient(ItemID.Wire, 10);
            entitySensorRecipe2.AddTile(TileID.Anvils);
            entitySensorRecipe2.SetResult(ModContent.ItemType<EntitySensorItem>());
            entitySensorRecipe2.AddRecipe();
        }
        
        public override void Unload() {
            speakerUIState = null;
            entitySensorUIState = null;
            itemFilterUIState = null;
            quarryUIState = null;
            turretUIState = null;

            checkboxTexture = null;
            checkmarkTexture = null;
            xTexture = null;

            plusTexture = null;
            minusTexture = null;

            globalHitTile = null;

            TEItemDuct.needUpdate.Clear();
            TEItemDuct.needUpdate = null;

            instance = null;
        }
        
        public override void UpdateUI(GameTime gameTime) {

            if (checkmarkTexture == null || checkmarkTexture.Width == 0) {
                Main.NewText("had to reload textures");
            }

            if (speakerUI?.CurrentState != null) {
                speakerUI.Update(gameTime);
            }

            if (entitySensorUI?.CurrentState != null) {
                entitySensorUI.Update(gameTime);
            }

            if (itemFilterUI?.CurrentState != null) {
                itemFilterUI.Update(gameTime);
            }

            if (quarryUI?.CurrentState != null) {
                quarryUI.Update(gameTime);
            }

            if (turretUI?.CurrentState != null) {
                turretUI.Update(gameTime);
            }
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            

            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: SpeakerUI",
                    delegate {
                        if (SpeakerUIVisible()) {
                            speakerUI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: EntitySensorUI",
                    delegate {
                        if (EntitySensorUIVisible()) {
                            entitySensorUI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: FilterUI",
                    delegate {
                        if (ItemFilterUIVisible()) {
                            itemFilterUI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: QuarryUI",
                    delegate {
                        if (QuarryUIVisible()) {
                            quarryUI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: TurretUI",
                    delegate {
                        if (TurretUIVisible()) {
                            turretUI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }

            int wireIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
            if (wireIndex != -1) {
                layers.Insert(wireIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: EntitySensorWire",
                    delegate {
                        Vector2 zero2 = Vector2.Zero;
                        int num21 = (int)((Main.screenPosition.X - zero2.X) / 16f - 1f);
                        int num20 = (int)((Main.screenPosition.X + (float)Main.screenWidth + zero2.X) / 16f) + 2;
                        int num19 = (int)((Main.screenPosition.Y - zero2.Y) / 16f - 1f);
                        int num18 = (int)((Main.screenPosition.Y + (float)Main.screenHeight + zero2.Y) / 16f) + 5;
                        if (num21 < 0) {
                            num21 = 0;
                        }
                        if (num20 > Main.maxTilesX) {
                            num20 = Main.maxTilesX;
                        }
                        if (num19 < 0) {
                            num19 = 0;
                        }
                        if (num18 > Main.maxTilesY) {
                            num18 = Main.maxTilesY;
                        }
                        for (int k = num19; k < num18; k++) {
                            for (int i = num21; i < num20; i++) {
                                Tile tile = Main.tile[i, k];
                                if ((WiresUI.Settings.DrawWires || EntitySensorUIVisible()) && tile.active() && tile.type == TileType("EntitySensorTile") && tile.frameY == 0) {
                                    TileEntity tileEntity = default(TileEntity);
                                    
                                    if (TileEntity.ByPosition.TryGetValue(new Point16(i, k), out tileEntity)) {
                                        TEEntitySensor es = tileEntity as TEEntitySensor;
                                        Vector2 start = new Vector2((float)(i * 16 - 16 * es.left - 1), (float)(k * 16 - (16 * (es.top)) - 1)) + zero2;
                                        Vector2 end = new Vector2((float)(i * 16 + 16 * (es.right + 1) + 1), (float)(k * 16 + 16 * (es.bottom + 1) + 1)) + zero2;
                                        Utils.DrawRectangle(Main.spriteBatch, start, end, Microsoft.Xna.Framework.Color.LightSeaGreen, Microsoft.Xna.Framework.Color.LightSeaGreen, 2f);
                                    }
                                }
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.Game)
                );
                layers.Insert(wireIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: QuarryWire",
                    delegate {
                        Vector2 zero2 = Vector2.Zero;
                        int num21 = (int)((Main.screenPosition.X - zero2.X) / 16f - 1f);
                        int num20 = (int)((Main.screenPosition.X + (float)Main.screenWidth + zero2.X) / 16f) + 2;
                        int num19 = (int)((Main.screenPosition.Y - zero2.Y) / 16f - 1f);
                        int num18 = (int)((Main.screenPosition.Y + (float)Main.screenHeight + zero2.Y) / 16f) + 5;
                        if (num21 < 0) {
                            num21 = 0;
                        }
                        if (num20 > Main.maxTilesX) {
                            num20 = Main.maxTilesX;
                        }
                        if (num19 < 0) {
                            num19 = 0;
                        }
                        if (num18 > Main.maxTilesY) {
                            num18 = Main.maxTilesY;
                        }
                        for (int k = num19; k < num18; k++) {
                            for (int i = num21; i < num20; i++) {
                                Tile tile = Main.tile[i, k];
                                if ((WiresUI.Settings.DrawWires || QuarryUIVisible()) && tile.active() && tile.type == TileType("QuarryTile") && tile.frameX == 0 && tile.frameY == 0) {
                                    TileEntity tileEntity = default(TileEntity);
                                    
                                    if (TileEntity.ByPosition.TryGetValue(new Point16(i, k), out tileEntity)) {
                                        TEQuarry es = tileEntity as TEQuarry;
                                        if (es.hasFrame) {
                                            Vector2 start = new Vector2((float)(i * 16 - 16 * (es.left - 1) - 8), (float)(k * 16 - (16 * (es.top - 1))) - 8) + zero2;
                                            Vector2 end = new Vector2((float)(i * 16 + 16 * (es.right + 1) - 8), (float)(k * 16 + 16 * (es.bottom) + 1 + 8)) + zero2;
                                            Color col = Microsoft.Xna.Framework.Color.Yellow;
                                            Utils.DrawRectangle(Main.spriteBatch, start, end, col, col, 2f);
                                        }
                                    }
                                }
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.Game)
                );
                layers.Insert(wireIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: TurretWire",
                    delegate {
                        Vector2 zero2 = Vector2.Zero;
                        int num21 = (int)((Main.screenPosition.X - zero2.X) / 16f - 1f);
                        int num20 = (int)((Main.screenPosition.X + (float)Main.screenWidth + zero2.X) / 16f) + 2;
                        int num19 = (int)((Main.screenPosition.Y - zero2.Y) / 16f - 1f);
                        int num18 = (int)((Main.screenPosition.Y + (float)Main.screenHeight + zero2.Y) / 16f) + 5;
                        if (num21 < 0) {
                            num21 = 0;
                        }
                        if (num20 > Main.maxTilesX) {
                            num20 = Main.maxTilesX;
                        }
                        if (num19 < 0) {
                            num19 = 0;
                        }
                        if (num18 > Main.maxTilesY) {
                            num18 = Main.maxTilesY;
                        }

                        for (int k = num19; k < num18; k++) {
                            for (int i = num21; i < num20; i++) {
                                Tile tile = Main.tile[i, k];
                                if ((WiresUI.Settings.DrawWires || TurretUIVisible()) && tile.active() && tile.type == TileType("TurretTile") && (tile.frameX % 36) == 0 && (tile.frameY % 36) == 0) {
                                    TileEntity tileEntity = default(TileEntity);

                                    if (TileEntity.ByPosition.TryGetValue(new Point16(i, k), out tileEntity)) {
                                        TETurret es = tileEntity as TETurret;

                                        if(es.circle == null) es.circle = Util.CreateCircle(es.range);
                                        Main.spriteBatch.Draw(es.circle, new Vector2(i * 16 + 16, k * 16 + 16) - Main.screenPosition - es.circle.Size() / 2, Color.Red);
                                    }
                                }
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.Game)
                );
                layers.Insert(wireIndex, new LegacyGameInterfaceLayer(
                    "MoreMechanisms: VacuumWire",
                    delegate {
                        Vector2 zero2 = Vector2.Zero;
                        int num21 = (int)((Main.screenPosition.X - zero2.X) / 16f - 1f);
                        int num20 = (int)((Main.screenPosition.X + (float)Main.screenWidth + zero2.X) / 16f) + 2;
                        int num19 = (int)((Main.screenPosition.Y - zero2.Y) / 16f - 1f);
                        int num18 = (int)((Main.screenPosition.Y + (float)Main.screenHeight + zero2.Y) / 16f) + 5;
                        if (num21 < 0) {
                            num21 = 0;
                        }
                        if (num20 > Main.maxTilesX) {
                            num20 = Main.maxTilesX;
                        }
                        if (num19 < 0) {
                            num19 = 0;
                        }
                        if (num18 > Main.maxTilesY) {
                            num18 = Main.maxTilesY;
                        }

                        for (int k = num19; k < num18; k++) {
                            for (int i = num21; i < num20; i++) {
                                Tile tile = Main.tile[i, k];
                                if (WiresUI.Settings.DrawWires && tile.active() && tile.type == TileType("VacuumTile") && (tile.frameX % 36) == 0 && (tile.frameY % 36) == 0) {
                                    TileEntity tileEntity = default(TileEntity);

                                    if (TileEntity.ByPosition.TryGetValue(new Point16(i, k), out tileEntity)) {
                                        TEVacuum es = tileEntity as TEVacuum;

                                        if (es.circle == null) es.circle = Util.CreateCircle(es.range);
                                        Main.spriteBatch.Draw(es.circle, new Vector2(i * 16 + 8, k * 16 + 8) - Main.screenPosition - es.circle.Size() / 2, Color.Yellow);
                                    }
                                }
                            }
                        }
                        return true;
                    },
                    InterfaceScaleType.Game)
                );


            }
        }
        
        internal void ShowSpeakerUI() {
            Main.PlaySound(SoundID.MenuOpen);
            speakerUI?.SetState(speakerUIState);
            Main.playerInventory = false;
        }

        internal void HideSpeakerUI() {
            Main.PlaySound(SoundID.MenuClose);
            speakerUI?.SetState(null);
            Main.playerInventory = false;
        }

        internal bool SpeakerUIVisible() {
            return speakerUI?.CurrentState == speakerUIState;
        }

        
        internal void ShowItemFilterUI(Item[] items, bool whitelist, Action<Item[], bool> callback) {
            Main.PlaySound(SoundID.MenuOpen);
            itemFilterUIState.Load(items, whitelist, callback);
            itemFilterUI?.SetState(itemFilterUIState);
            Main.playerInventory = true;
        }

        internal void HideItemFilterUI() {
            Main.PlaySound(SoundID.MenuClose);
            itemFilterUI?.SetState(null);
            Main.playerInventory = false;
        }

        internal bool ItemFilterUIVisible() {
            return itemFilterUI?.CurrentState == itemFilterUIState;
        }


        internal void ShowQuarryUI(Item item, ItemFilter filter, Action<Item> callback) {
            Main.PlaySound(SoundID.MenuOpen);
            quarryUIState.Load(item, filter, callback);
            quarryUI?.SetState(quarryUIState);
            Main.playerInventory = true;
        }

        internal void HideQuarryUI() {
            Main.PlaySound(SoundID.MenuClose);
            quarryUI?.SetState(null);
            Main.playerInventory = false;
        }

        internal bool QuarryUIVisible() {
            return quarryUI?.CurrentState == quarryUIState;
        }


        internal void ShowTurretUI(Item item, Action<Item> callback) {
            Main.PlaySound(SoundID.MenuOpen);
            turretUIState.Load(item, callback);
            turretUI?.SetState(turretUIState);
            Main.playerInventory = true;
        }

        internal void HideTurretUI() {
            Main.PlaySound(SoundID.MenuClose);
            turretUI?.SetState(null);
            Main.playerInventory = false;
        }

        internal bool TurretUIVisible() {
            return turretUI?.CurrentState == turretUIState;
        }

        internal void ShowEntitySensorUI() {
            Main.PlaySound(SoundID.MenuOpen);
            entitySensorUI?.SetState(entitySensorUIState);
            Main.playerInventory = false;
        }

        internal void HideEntitySensorUI() {
            Main.PlaySound(SoundID.MenuClose);
            entitySensorUI?.SetState(null);
            Main.playerInventory = false;
        }

        internal bool EntitySensorUIVisible() {
            return entitySensorUI?.CurrentState == entitySensorUIState;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            byte msgType = reader.ReadByte();
            switch (msgType) {
                case 1: // TESpeaker update
                    {
                        short x = reader.ReadInt16();
                        short y = reader.ReadInt16();

                        int index = GetInstance<TESpeaker>().Find(x, y);
                        if (index != -1) {
                            TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];
                            speakerEnt.NetReceive(reader, false);

                            if (Main.netMode == NetmodeID.Server) {
                                ModPacket myPacket = MoreMechanisms.instance.GetPacket();
                                myPacket.Write((byte)1); // id
                                myPacket.Write((short)speakerEnt.Position.X);
                                myPacket.Write((short)speakerEnt.Position.Y);
                                speakerEnt.NetSend(myPacket, false);
                                myPacket.Send();
                            }

                        }
                    }
                    break;
                case 2: // TESpeaker play
                    {
                        short x = reader.ReadInt16();
                        short y = reader.ReadInt16();

                        int index = GetInstance<TESpeaker>().Find(x, y);
                        if (index != -1) {
                            TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];
                            speakerEnt.PlaySound();
                        }
                    }
                    break;
                case 3: // TEEntitySensor update
                    {
                        short x = reader.ReadInt16();
                        short y = reader.ReadInt16();

                        int index = GetInstance<TEEntitySensor>().Find(x, y);
                        if (index != -1) {
                            TEEntitySensor es = (TEEntitySensor)TileEntity.ByID[index];
                            es.NetReceive(reader, false);

                            if (Main.netMode == NetmodeID.Server) {
                                ModPacket myPacket = MoreMechanisms.instance.GetPacket();
                                myPacket.Write((byte)3); // id
                                myPacket.Write((short)es.Position.X);
                                myPacket.Write((short)es.Position.Y);
                                es.NetSend(myPacket, false);
                                myPacket.Send();
                            }

                        }
                    }
                    break;
                default:
                    Logger.WarnFormat("MoreMechanisms: Unknown Message type: {0}", msgType);
                    break;
            }
        }
        
        public override void PostDrawInterface(SpriteBatch spriteBatch) {
            SpeakersPlayer sp = Main.LocalPlayer.GetModPlayer<SpeakersPlayer>();
            sp.Draw(spriteBatch);
        }

    }

    class SpeakersPlayer : ModPlayer {
        
        VanillaItemSlotWrapper[] mechanismSlots = new VanillaItemSlotWrapper[5];

        public void ResetMechanismSlots() {
            mechanismSlots = new VanillaItemSlotWrapper[12];
            for(int i = 0; i < mechanismSlots.Length; i++) {
                mechanismSlots[i] = new VanillaItemSlotWrapper(ItemSlot.Context.BankItem, 0.6f);
                mechanismSlots[i].ValidItemFunc = (it) => {
                    return (it.mech || it.IsAir) && it.type != ItemID.WireKite;
                };
                mechanismSlots[i].Item = new Item();
                mechanismSlots[i].Item.SetDefaults(0);
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (!Main.playerInventory) return;
            
            if (HasMechanismSlots()) {

                // adapted from Terraria's code for drawing the ammo slots

                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, Main.fontMouseText, "Mechanisms", new Vector2(532f + 36f + 36f / 2, 84f), new Microsoft.Xna.Framework.Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, default(Vector2), 0.75f, SpriteEffects.None, 0f);
                for (int i = 0; i < mechanismSlots.Length; i++) {
                    int num20 = 534 + 36;
                    int num19 = (int)(85f + (float)((i % 4) * 56) * 0.6f + 20f);
                    mechanismSlots[i].relativePos = new Vector2(num20 + (int)(i / 4) * 36, num19);
                    mechanismSlots[i].DrawSlot(spriteBatch);
                }
            }
        }

        public override void SetControls() {
            if ((MoreMechanisms.instance.SpeakerUIVisible() || MoreMechanisms.instance.EntitySensorUIVisible()) && Main.LocalPlayer.controlInv) {
                if(MoreMechanisms.instance.SpeakerUIVisible()) MoreMechanisms.instance.HideSpeakerUI();
                if(MoreMechanisms.instance.EntitySensorUIVisible()) MoreMechanisms.instance.HideEntitySensorUI();
                Main.LocalPlayer.releaseInventory = false;
            }

            if(MoreMechanisms.instance.ItemFilterUIVisible() && !Main.playerInventory) {
                MoreMechanisms.instance.HideItemFilterUI();
            }
            
            if (MoreMechanisms.instance.QuarryUIVisible() && !Main.playerInventory) {
                MoreMechanisms.instance.HideQuarryUI();
            }

            if (MoreMechanisms.instance.TurretUIVisible() && !Main.playerInventory) {
                MoreMechanisms.instance.HideTurretUI();
            }
        }

        public override void PostUpdate() {
            if (Main.netMode != NetmodeID.Server) {
                if (Main.LocalPlayer.dead) {
                    if (MoreMechanisms.instance.SpeakerUIVisible()) MoreMechanisms.instance.HideSpeakerUI();
                    if (MoreMechanisms.instance.EntitySensorUIVisible()) MoreMechanisms.instance.HideEntitySensorUI();
                    if (MoreMechanisms.instance.ItemFilterUIVisible()) MoreMechanisms.instance.HideItemFilterUI();
                    if (MoreMechanisms.instance.QuarryUIVisible()) MoreMechanisms.instance.HideQuarryUI();
                    if (MoreMechanisms.instance.TurretUIVisible()) MoreMechanisms.instance.HideTurretUI();
                }

                int uiRange = 7 * 16;
                if (MoreMechanisms.instance.SpeakerUIVisible()) {
                    //Main.NewText("WithinRange " + Speakers.instance.speakerUIState.i + " " + Speakers.instance.speakerUIState.j + " " + 8 * 16 + " = " + Main.LocalPlayer.WithinRange(new Vector2(Speakers.instance.speakerUIState.i, Speakers.instance.speakerUIState.j), 8 * 16));
                    if (!Main.LocalPlayer.WithinRange(new Vector2(MoreMechanisms.instance.speakerUIState.i, MoreMechanisms.instance.speakerUIState.j), uiRange)) {
                        MoreMechanisms.instance.HideSpeakerUI();
                    }
                }

                if (MoreMechanisms.instance.EntitySensorUIVisible()) {
                    //Main.NewText("WithinRange " + Speakers.instance.speakerUIState.i + " " + Speakers.instance.speakerUIState.j + " " + 8 * 16 + " = " + Main.LocalPlayer.WithinRange(new Vector2(Speakers.instance.speakerUIState.i, Speakers.instance.speakerUIState.j), 8 * 16));
                    if (!Main.LocalPlayer.WithinRange(new Vector2(MoreMechanisms.instance.entitySensorUIState.i, MoreMechanisms.instance.entitySensorUIState.j), uiRange)) {
                        MoreMechanisms.instance.HideEntitySensorUI();
                    }
                }

                if (MoreMechanisms.instance.ItemFilterUIVisible()) {
                    //Main.NewText("WithinRange " + Speakers.instance.speakerUIState.i + " " + Speakers.instance.speakerUIState.j + " " + 8 * 16 + " = " + Main.LocalPlayer.WithinRange(new Vector2(Speakers.instance.speakerUIState.i, Speakers.instance.speakerUIState.j), 8 * 16));
                    if (!Main.LocalPlayer.WithinRange(new Vector2(MoreMechanisms.instance.itemFilterUIState.i, MoreMechanisms.instance.itemFilterUIState.j), uiRange)) {
                        MoreMechanisms.instance.HideItemFilterUI();
                    }
                }

                if (MoreMechanisms.instance.QuarryUIVisible()) {
                    //Main.NewText("WithinRange " + Speakers.instance.speakerUIState.i + " " + Speakers.instance.speakerUIState.j + " " + 8 * 16 + " = " + Main.LocalPlayer.WithinRange(new Vector2(Speakers.instance.speakerUIState.i, Speakers.instance.speakerUIState.j), 8 * 16));
                    if (!Main.LocalPlayer.WithinRange(new Vector2(MoreMechanisms.instance.quarryUIState.i, MoreMechanisms.instance.quarryUIState.j), uiRange)) {
                        MoreMechanisms.instance.HideQuarryUI();
                    }
                }
                
                if (MoreMechanisms.instance.TurretUIVisible()) {
                    //Main.NewText("WithinRange " + Speakers.instance.speakerUIState.i + " " + Speakers.instance.speakerUIState.j + " " + 8 * 16 + " = " + Main.LocalPlayer.WithinRange(new Vector2(Speakers.instance.speakerUIState.i, Speakers.instance.speakerUIState.j), 8 * 16));
                    if (!Main.LocalPlayer.WithinRange(new Vector2(MoreMechanisms.instance.turretUIState.i, MoreMechanisms.instance.turretUIState.j), uiRange)) {
                        MoreMechanisms.instance.HideTurretUI();
                    }
                }
            }
        }

        public bool HasMechanismSlots() {

            // has the grand design in inventory
            if (player.HasItem(ItemID.WireKite) || Main.mouseItem.type == ItemID.WireKite) {
                return true;
            }

            return false;
        }

        public override void OnEnterWorld(Player player) {
            MoreMechanisms.instance.HideSpeakerUI();
            MoreMechanisms.instance.HideEntitySensorUI();
            MoreMechanisms.instance.HideItemFilterUI();
            MoreMechanisms.instance.HideQuarryUI();
            MoreMechanisms.instance.HideTurretUI();
        }

        public override bool PreItemCheck() {

            // hack so that a non solid tile can be hammered
            Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
            if (item.hammer > 0 && Main.mouseItem.IsAir) {
                Main.tileSolid[mod.TileType("ItemDuctTile")] = true;
            }

            return base.PreItemCheck();
        }

        public override void PostItemCheck() {

            Main.tileSolid[mod.TileType("ItemDuctTile")] = false;

            base.PostItemCheck();
        }

        public override void Load(TagCompound tag) {
            ResetMechanismSlots();
            if (tag.ContainsKey("mechItems")) {
                List<Item> items = tag.Get<List<Item>>("mechItems");
                for (int i = 0; i < Math.Min(mechanismSlots.Length, items.Count); i++) {
                    if(items[i].type != Main.maxItemTypes) mechanismSlots[i].Item = items[i];
                }
            }
        }

        public override TagCompound Save() {
            TagCompound tags = new TagCompound();

            List<Item> items = new List<Item>();
            foreach (VanillaItemSlotWrapper s in mechanismSlots) {
                items.Add(s.Item);
            }
            tags.Add("mechItems", items);

            return tags;
        }
    }

    internal class FixedUIScrollbar : UIScrollbar {
        protected override void DrawSelf(SpriteBatch spriteBatch) {
            UserInterface temp = UserInterface.ActiveInstance;
            UserInterface.ActiveInstance = MoreMechanisms.instance.speakerUI;
            base.DrawSelf(spriteBatch);
            UserInterface.ActiveInstance = temp;
        }

        public override void MouseDown(UIMouseEvent evt) {
            UserInterface temp = UserInterface.ActiveInstance;
            UserInterface.ActiveInstance = MoreMechanisms.instance.speakerUI;
            base.MouseDown(evt);
            UserInterface.ActiveInstance = temp;
        }
    }
}