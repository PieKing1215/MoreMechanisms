using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace MoreMechanisms.Tiles {

    public class TESpeaker : ModTileEntity {
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
            return tile.active() && tile.type == mod.TileType("SpeakerTile") && tile.frameX == 0 && tile.frameY == 0;
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

    public class SpeakerTile : ModTile {
        public override void SetDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            dustType = mod.DustType("Sparkle");
            //drop = mod.ItemType("SpeakerItem");

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TESpeaker>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);

            //disableSmartCursor = true;
            //disableSmartInteract = true;

            AddMapEntry(new Color(200, 200, 200));
            // Set other values here
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            Item.NewItem(i * 16, j * 16, 32, 32, mod.ItemType("SpeakerItem"));
            GetInstance<TESpeaker>().Kill(i, j);
        }

        public override bool HasSmartInteract() {
            return true;
        }

        public override void HitWire(int i, int j) {
            Tile tile = Main.tile[i, j];
            int left = i - tile.frameX % 36 / 18;
            int top = j - tile.frameY / 18;

            int index = GetInstance<TESpeaker>().Find(left, top);
            if (index != -1) {
                TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];

                if (Main.netMode == NetmodeID.Server) {
                    ModPacket myPacket = MoreMechanisms.instance.GetPacket();
                    myPacket.Write((byte)2); // id
                    myPacket.Write((short)speakerEnt.Position.X);
                    myPacket.Write((short)speakerEnt.Position.Y);
                    myPacket.Send();
                }

                speakerEnt.PlaySound();
            }
        }

        public override bool NewRightClick(int i, int j) {

            Tile tile = Main.tile[i, j];
            int left = i - tile.frameX % 36 / 18;
            int top = j - tile.frameY / 18;

            int index = GetInstance<TESpeaker>().Find(left, top);
            if (index != -1) {
                TESpeaker speakerEnt = (TESpeaker)TileEntity.ByID[index];
                //Main.NewText(speakerEnt.soundId + " " + speakerEnt.global + " " + speakerEnt.volume + " " + speakerEnt.pitch);

                //Main.NewText("Scores:");

                //Random rnd = new Random();
                //int snd = rnd.Next(0, 41);
                //speakerEnt.soundId = snd;
                //speakerEnt.changed = true;
                //Main.PlaySound(speakerEnt.soundId, i * 16, j * 16);

                MoreMechanisms.instance.speakerUIState.i = i * 16;
                MoreMechanisms.instance.speakerUIState.j = j * 16;
                if(!MoreMechanisms.instance.SpeakerUIVisible()) MoreMechanisms.instance.ShowSpeakerUI();

            }
            
            //Activate(i, j);

            //Tile tile = Main.tile[i, j];
            //int left = i - tile.frameX % 36 / 18;
            //int top = j - tile.frameY / 18;

            //int index = GetInstance<TESpeaker>().Find(left, top);
            //if (index == -1) {
            //    return false;
            //}
            //Main.NewText("Scores:");
            //TEScoreBoard tEScoreBoard = (TEScoreBoard)TileEntity.ByID[index];
            //foreach (var item in tEScoreBoard.scores) {
            //    Main.NewText(item.Key + ": " + item.Value);
            //}

            return true;
        }

        public override void MouseOver(int i, int j) {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = mod.ItemType("SpeakerItem");
        }
    }
}