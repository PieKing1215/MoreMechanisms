using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms.Tiles {

    public class TESeller : ModTileEntity {
        internal int soundId = 0;
        internal bool global = false;
        internal int volume = 100; // 0-200 default=100
        internal int pitch = 100; // 0-200 default=100
        internal bool changed = false;
        
        public void ClientSendServer() {
            ModPacket myPacket = MoreMechanisms.instance.GetPacket();
            myPacket.Write((byte)1); // id
            myPacket.Write((short)Position.X);
            myPacket.Write((short)Position.Y);
            NetSend(myPacket, false);
            myPacket.Send();
        }

        public override void Update() {
            if (changed) {
                //Main.NewText("changed");
                // Sending 86 aka, TileEntitySharing, triggers NetSend. Think of it like manually calling sync.
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                changed = false;
            } else {
               //Main.NewText("no changed");
            }
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive) {
            soundId = reader.ReadInt32();
            global = reader.ReadBoolean();
            volume = reader.ReadInt32();
            pitch = reader.ReadInt32();
            //Main.NewText("NetReceive " + soundId + " " + global + " " + volume + " " + pitch);
        }

        public override void NetSend(BinaryWriter writer, bool lightSend) {
            writer.Write(soundId);
            writer.Write(global);
            writer.Write(volume);
            writer.Write(pitch);
            //Main.NewText("NetSend " + soundId + " " + global + " " + volume + " " + pitch);
        }

        public override TagCompound Save() {
            return new TagCompound
            {
                {"soundId", soundId},
                {"global", global},
                {"volume", volume},
                {"pitch", pitch}
            };
        }

        public override void Load(TagCompound tag) {
            soundId = tag.Get<int>("soundId");
            global = tag.Get<bool>("global");
            volume = tag.Get<int>("volume");
            pitch = tag.Get<int>("pitch");
        }

        public override bool ValidTile(int i, int j) {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == mod.TileType("SellerTile") && tile.frameX == 0 && tile.frameY == 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction) {
            //Main.NewText("i " + i + " j " + j + " t " + type + " s " + style + " d " + direction);
            if (Main.netMode == 1) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
                NetMessage.SendData(87, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }

        public void PlaySound() {
            var listOfFieldNames = typeof(SoundID).GetFields();
            var sound = listOfFieldNames[soundId].GetValue(null);

            float vol = (volume / 100f);
            if (vol < 0) vol = 0;
            if (vol > 1) vol = 1;
            float pitch = (this.pitch - 100) / 100f * 0.9f;

            if (sound is LegacySoundStyle) {
                LegacySoundStyle snd = sound as LegacySoundStyle;
                vol *= snd.Volume;

                if (global) {
                    Main.PlaySound(snd.SoundId, -1, -1, snd.Style, vol, pitch);
                } else {
                    Main.PlaySound(snd.SoundId, Position.X * 16, Position.Y * 16, snd.Style, vol, pitch);
                }
            } else if (sound is int) {
                int snd = (int)sound;
                if (global) {
                    Main.PlaySound(snd, -1, -1, 1, vol, pitch);
                } else {
                    Main.PlaySound(snd, Position.X * 16, Position.Y * 16, 1, vol, pitch);
                }
            }
        }
    }

    public class SellerTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileContainer[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            //drop = mod.ItemType("SellerItem");

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            //TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TESeller>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.newTile.HookCheck = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
            TileObjectData.addTile(Type);

            //disableSmartCursor = true;
            //disableSmartInteract = true;

            disableSmartCursor = true;
            chest = "Seller";
            chestDrop = mod.ItemType("SellerItem");

            AddMapEntry(new Color(200, 200, 200));
            // Set other values here
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(i * 16, j * 16, 32, 32, chestDrop);
            Chest.DestroyChest(i, j);
        }

        public override bool HasSmartInteract() {
            return true;
        }

        public override bool NewRightClick(int i, int j) {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            Main.mouseRightRelease = false;
            int left = i;
            int top = j;
            while (tile.frameX > 0) {
                left--;
                tile = Main.tile[left, top];
            }
            while (tile.frameY > 0) {
                top--;
                tile = Main.tile[left, top];
            }

            if (player.sign >= 0) {
                Main.PlaySound(SoundID.MenuClose);
                player.sign = -1;
                Main.editSign = false;
                Main.npcChatText = "";
            }
            if (Main.editChest) {
                Main.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                Main.npcChatText = "";
            }
            if (player.editedChestName) {
                NetMessage.SendData(33, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
                player.editedChestName = false;
            }
            if (Main.netMode == 1) {
                if (left == player.chestX && top == player.chestY && player.chest >= 0) {
                    player.chest = -1;
                    Recipe.FindRecipes();
                    Main.PlaySound(SoundID.MenuClose);
                } else {
                    NetMessage.SendData(31, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
                    Main.stackSplit = 600;
                }
            } else {
                int chest = Chest.FindChest(left, top);
                if (chest >= 0) {
                    Main.stackSplit = 600;
                    if (chest == player.chest) {
                        player.chest = -1;
                        Main.PlaySound(SoundID.MenuClose);
                    } else {
                        player.chest = chest;
                        Main.playerInventory = true;
                        Main.recBigList = false;
                        player.chestX = left;
                        player.chestY = top;
                        Main.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                    }
                    Recipe.FindRecipes();
                }
            }
            return true;
        }

        public override void MouseOver(int i, int j) {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            while (tile.frameX > 0) {
                left--;
                tile = Main.tile[left, top];
            }
            while (tile.frameY > 0) {
                top--;
                tile = Main.tile[left, top];
            }
            int chest = Chest.FindChest(left, top);
            player.showItemIcon2 = -1;
            if (chest < 0) {
                player.showItemIconText = Language.GetTextValue("LegacyChestType.0");
            } else {
                player.showItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : "Seller";
                if (player.showItemIconText == "Seller") {
                    player.showItemIcon2 = mod.ItemType("SellerTile");
                    player.showItemIconText = "";
                }
            }
            player.noThrow = 2;
            player.showItemIcon = true;
        }

        public override void MouseOverFar(int i, int j) {
            MouseOver(i, j);
            Player player = Main.LocalPlayer;
            if (player.showItemIconText == "") {
                player.showItemIcon = false;
                player.showItemIcon2 = 0;
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            base.PostDraw(i, j, spriteBatch);

            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;

            while (tile.frameX > 0) {
                left--;
                tile = Main.tile[left, top];
            }

            while (tile.frameY > 0) {
                top--;
                tile = Main.tile[left, top];
            }

            int chest = Chest.FindChest(left, top);
            if (chest >= 0) {
                Chest ch = Main.chest[chest];
                for(int ii = 0; ii < ch.item.Length; ii++) {
                    Item it = ch.item[ii];

                    if(it.active && !it.IsAir) {
                        int val = (it.value / 5) * it.stack;

                        int nPlat = 0;
                        while(val >= Item.platinum) {
                            nPlat++;
                            val -= Item.platinum;
                        }
                        if(nPlat > 0) Item.NewItem(i * 16, j * 16, 32, 32, ItemID.PlatinumCoin, nPlat);

                        int nGold = 0;
                        while (val >= Item.gold) {
                            nGold++;
                            val -= Item.gold;
                        }
                        if (nGold > 0) Item.NewItem(i * 16, j * 16, 32, 32, ItemID.GoldCoin, nGold);

                        int nSilv = 0;
                        while (val >= Item.silver) {
                            nSilv++;
                            val -= Item.silver;
                        }
                        if (nSilv > 0) Item.NewItem(i * 16, j * 16, 32, 32, ItemID.SilverCoin, nSilv);

                        int nCopp = 0;
                        while (val >= Item.copper) {
                            nCopp++;
                            val -= Item.copper;
                        }
                        if (nCopp > 0) Item.NewItem(i * 16, j * 16, 32, 32, ItemID.CopperCoin, nCopp);

                        //Item.NewItem(i * 16, j * 16, 32, 32, it.type, it.stack, false, it.prefix, false, false);
                        ch.item[ii] = new Item();
                    }
                }
            }

        }

    }
}