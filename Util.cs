using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MoreMechanisms {
    class Util {

        /// <summary>
        /// <para>Crop the given Texture2D to the given Rectangle</para>
        /// <para>From https://github.com/JavidPack/BossChecklist/blob/4785fbe6b7609bea2fc34fada32dd04af051007d/BossLogUI.cs#L2609</para>
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="crop"></param>
        /// <returns></returns>
        public static Texture2D CropTexture(Texture2D texture, Rectangle crop) {
            Texture2D croppedTexture = new Texture2D(Main.graphics.GraphicsDevice, crop.Width, crop.Height);
            Color[] data = new Color[crop.Width * crop.Height];
            texture.GetData(0, crop, data, 0, data.Length);
            croppedTexture.SetData(data);
            return croppedTexture;
        }

        /// <summary>
        /// <para>Generates a Texture2D of a circle with the given radius.</para>
        /// <para>From <a href="https://stackoverflow.com/a/2984527">https://stackoverflow.com/a/2984527</a></para>
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Texture2D CreateCircle(int radius) {
            int outerRadius = radius * 2 + 2; // So circle doesn't go out of bounds
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            // Color the entire texture transparent first.
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Work out the minimum step necessary using trigonometry + sine approximation.
            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep) {
                // Use the parametric definition of a circle: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * outerRadius + x + 1] = Color.White;
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// <para>Hit tile with pick without needing a player.</para>
        /// <para>Equivalent to if an actual player hit it.</para>
        /// <para>Modified from Terraria's Player.PickTile code.</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pickPower"></param>
        public static void PickTile(int x, int y, int pickPower) {
            int num = 0;
            int tileId = MoreMechanisms.globalHitTile.HitObject(x, y, 1);
            Tile tile = Main.tile[x, y];
            if (Main.tileNoFail[tile.type]) {
                num = 100;
            }
            if (Main.tileDungeon[tile.type] || tile.type == 25 || tile.type == 58 || tile.type == 117 || tile.type == 203) {
                num += pickPower / 2;
            } else if (tile.type == 48 || tile.type == 232) {
                num += pickPower / 4;
            } else if (tile.type == 226) {
                num += pickPower / 4;
            } else if (tile.type == 107 || tile.type == 221) {
                num += pickPower / 2;
            } else if (tile.type == 108 || tile.type == 222) {
                num += pickPower / 3;
            } else if (tile.type == 111 || tile.type == 223) {
                num += pickPower / 4;
            } else if (tile.type == 211) {
                num += pickPower / 5;
            } else {
                TileLoader.MineDamage(pickPower, ref num);
            }
            if (tile.type == 211 && pickPower < 200) {
                num = 0;
            }
            if ((tile.type == 25 || tile.type == 203) && pickPower < 65) {
                num = 0;
                goto IL_02f3;
            }
            if (tile.type == 117 && pickPower < 65) {
                num = 0;
            } else if (tile.type == 37 && pickPower < 50) {
                num = 0;
            } else if (tile.type == 404 && pickPower < 65) {
                num = 0;
            } else {
                if ((tile.type == 22 || tile.type == 204) && (double)y > Main.worldSurface && pickPower < 55) {
                    num = 0;
                    goto IL_02f3;
                }
                if (tile.type == 56 && pickPower < 65) {
                    num = 0;
                } else if (tile.type == 58 && pickPower < 65) {
                    num = 0;
                } else {
                    if ((tile.type == 226 || tile.type == 237) && pickPower < 210) {
                        num = 0;
                        goto IL_02f3;
                    }
                    if (Main.tileDungeon[tile.type] && pickPower < 65) {
                        if ((double)x < (double)Main.maxTilesX * 0.35 || (double)x > (double)Main.maxTilesX * 0.65) {
                            num = 0;
                        }
                    } else if (tile.type == 107 && pickPower < 100) {
                        num = 0;
                    } else if (tile.type == 108 && pickPower < 110) {
                        num = 0;
                    } else if (tile.type == 111 && pickPower < 150) {
                        num = 0;
                    } else if (tile.type == 221 && pickPower < 100) {
                        num = 0;
                    } else if (tile.type == 222 && pickPower < 110) {
                        num = 0;
                    } else if (tile.type == 223 && pickPower < 150) {
                        num = 0;
                    } else {
                        TileLoader.PickPowerCheck(tile, pickPower, ref num);
                    }
                }
            }
            goto IL_02f3;
        IL_02f3:
            if (tile.type == 147 || tile.type == 0 || tile.type == 40 || tile.type == 53 || tile.type == 57 || tile.type == 59 || tile.type == 123 || tile.type == 224 || tile.type == 397) {
                num += pickPower;
            }
            if (tile.type == 165 || Main.tileRope[tile.type] || tile.type == 199 || Main.tileMoss[tile.type]) {
                num = 100;
            }
            if (MoreMechanisms.globalHitTile.AddDamage(tileId, num, false) >= 100 && (tile.type == 2 || tile.type == 23 || tile.type == 60 || tile.type == 70 || tile.type == 109 || tile.type == 199 || Main.tileMoss[tile.type])) {
                num = 0;
            }
            if (tile.type == 128 || tile.type == 269) {
                if (tile.frameX == 18 || tile.frameX == 54) {
                    x--;
                    tile = Main.tile[x, y];
                    MoreMechanisms.globalHitTile.UpdatePosition(tileId, x, y);
                }
                if (tile.frameX >= 100) {
                    num = 0;
                    Main.blockMouse = true;
                }
            }
            if (tile.type == 334) {
                if (tile.frameY == 0) {
                    y++;
                    tile = Main.tile[x, y];
                    MoreMechanisms.globalHitTile.UpdatePosition(tileId, x, y);
                }
                if (tile.frameY == 36) {
                    y--;
                    tile = Main.tile[x, y];
                    MoreMechanisms.globalHitTile.UpdatePosition(tileId, x, y);
                }
                int j = tile.frameX;
                bool flag3 = j >= 5000;
                bool flag2 = false;
                if (!flag3) {
                    int num5 = j / 18;
                    num5 %= 3;
                    x -= num5;
                    tile = Main.tile[x, y];
                    if (tile.frameX >= 5000) {
                        flag3 = true;
                    }
                }
                if (flag3) {
                    j = tile.frameX;
                    int num3 = 0;
                    while (j >= 5000) {
                        j -= 5000;
                        num3++;
                    }
                    if (num3 != 0) {
                        flag2 = true;
                    }
                }
                if (flag2) {
                    num = 0;
                    Main.blockMouse = true;
                }
            }
            if (!WorldGen.CanKillTile(x, y)) {
                num = 0;
            }
            if (MoreMechanisms.globalHitTile.AddDamage(tileId, num, true) >= 100) {
                //AchievementsHelper.CurrentlyMining = true;
                MoreMechanisms.globalHitTile.Clear(tileId);
                if (Main.netMode == 1 && Main.tileContainer[Main.tile[x, y].type]) {
                    WorldGen.KillTile(x, y, true, false, false);
                    NetMessage.SendData(17, -1, -1, null, 0, (float)x, (float)y, 1f, 0, 0, 0);
                    if (Main.tile[x, y].type == 21 || (Main.tile[x, y].type >= 470 && TileID.Sets.BasicChest[Main.tile[x, y].type])) {
                        NetMessage.SendData(34, -1, -1, null, 1, (float)x, (float)y, 0f, 0, 0, 0);
                    }
                    if (Main.tile[x, y].type == 467) {
                        NetMessage.SendData(34, -1, -1, null, 5, (float)x, (float)y, 0f, 0, 0, 0);
                    }
                    if (TileLoader.IsDresser(Main.tile[x, y].type)) {
                        NetMessage.SendData(34, -1, -1, null, 3, (float)x, (float)y, 0f, 0, 0, 0);
                    }
                    if (Main.tile[x, y].type >= 470 && TileID.Sets.BasicChest[Main.tile[x, y].type]) {
                        NetMessage.SendData(34, -1, -1, null, 101, (float)x, (float)y, 0f, 0, Main.tile[x, y].type, 0);
                    }
                    if (Main.tile[x, y].type >= 470 && TileLoader.IsDresser(Main.tile[x, y].type)) {
                        NetMessage.SendData(34, -1, -1, null, 103, (float)x, (float)y, 0f, 0, Main.tile[x, y].type, 0);
                    }
                } else {
                    int num2 = y;
                    bool num6 = Main.tile[x, num2].active();
                    WorldGen.KillTile(x, num2, false, false, false);
                    if (num6 && !Main.tile[x, num2].active()) {
                        //AchievementsHelper.HandleMining();
                    }
                    if (Main.netMode == 1) {
                        NetMessage.SendData(17, -1, -1, null, 0, (float)x, (float)num2, 0f, 0, 0, 0);
                    }
                }
                //AchievementsHelper.CurrentlyMining = false;
            } else {
                WorldGen.KillTile(x, y, true, false, false);
                if (Main.netMode == 1) {
                    NetMessage.SendData(17, -1, -1, null, 0, (float)x, (float)y, 1f, 0, 0, 0);
                }
            }
            if (num != 0) {
                MoreMechanisms.globalHitTile.Prune();
            }
        }

        /// <summary>
        /// <para>Returns true if the pickPower provided could mine the tile.</para>
        /// <para>Modified from Terraria's Player.PickTile code.</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pickPower"></param>
        public static bool CanPickTile(int x, int y, int pickPower) {
            int num = 0;
            int tileId = MoreMechanisms.globalHitTile.HitObject(x, y, 1);
            Tile tile = Main.tile[x, y];
            if (Main.tileNoFail[tile.type]) {
                num = 100;
            }
            if (Main.tileDungeon[tile.type] || tile.type == 25 || tile.type == 58 || tile.type == 117 || tile.type == 203) {
                num += pickPower / 2;
            } else if (tile.type == 48 || tile.type == 232) {
                num += pickPower / 4;
            } else if (tile.type == 226) {
                num += pickPower / 4;
            } else if (tile.type == 107 || tile.type == 221) {
                num += pickPower / 2;
            } else if (tile.type == 108 || tile.type == 222) {
                num += pickPower / 3;
            } else if (tile.type == 111 || tile.type == 223) {
                num += pickPower / 4;
            } else if (tile.type == 211) {
                num += pickPower / 5;
            } else {
                TileLoader.MineDamage(pickPower, ref num);
            }
            if (tile.type == 211 && pickPower < 200) {
                num = 0;
            }
            if ((tile.type == 25 || tile.type == 203) && pickPower < 65) {
                num = 0;
                goto IL_02f3;
            }
            if (tile.type == 117 && pickPower < 65) {
                num = 0;
            } else if (tile.type == 37 && pickPower < 50) {
                num = 0;
            } else if (tile.type == 404 && pickPower < 65) {
                num = 0;
            } else {
                if ((tile.type == 22 || tile.type == 204) && (double)y > Main.worldSurface && pickPower < 55) {
                    num = 0;
                    goto IL_02f3;
                }
                if (tile.type == 56 && pickPower < 65) {
                    num = 0;
                } else if (tile.type == 58 && pickPower < 65) {
                    num = 0;
                } else {
                    if ((tile.type == 226 || tile.type == 237) && pickPower < 210) {
                        num = 0;
                        goto IL_02f3;
                    }
                    if (Main.tileDungeon[tile.type] && pickPower < 65) {
                        if ((double)x < (double)Main.maxTilesX * 0.35 || (double)x > (double)Main.maxTilesX * 0.65) {
                            num = 0;
                        }
                    } else if (tile.type == 107 && pickPower < 100) {
                        num = 0;
                    } else if (tile.type == 108 && pickPower < 110) {
                        num = 0;
                    } else if (tile.type == 111 && pickPower < 150) {
                        num = 0;
                    } else if (tile.type == 221 && pickPower < 100) {
                        num = 0;
                    } else if (tile.type == 222 && pickPower < 110) {
                        num = 0;
                    } else if (tile.type == 223 && pickPower < 150) {
                        num = 0;
                    } else {
                        TileLoader.PickPowerCheck(tile, pickPower, ref num);
                    }
                }
            }
            goto IL_02f3;
        IL_02f3:
            if (tile.type == 147 || tile.type == 0 || tile.type == 40 || tile.type == 53 || tile.type == 57 || tile.type == 59 || tile.type == 123 || tile.type == 224 || tile.type == 397) {
                num += pickPower;
            }
            if (tile.type == 165 || Main.tileRope[tile.type] || tile.type == 199 || Main.tileMoss[tile.type]) {
                num = 100;
            }
            if (/*MoreMechanisms.hitTile.AddDamage(tileId, num, false) >= 100 && */(tile.type == 2 || tile.type == 23 || tile.type == 60 || tile.type == 70 || tile.type == 109 || tile.type == 199 || Main.tileMoss[tile.type])) {
                num = 0;
            }
            if (tile.type == 128 || tile.type == 269) {
                if (tile.frameX == 18 || tile.frameX == 54) {
                    x--;
                    tile = Main.tile[x, y];
                    //MoreMechanisms.hitTile.UpdatePosition(tileId, x, y);
                }
                if (tile.frameX >= 100) {
                    num = 0;
                    Main.blockMouse = true;
                }
            }
            if (tile.type == 334) {
                if (tile.frameY == 0) {
                    y++;
                    tile = Main.tile[x, y];
                    //MoreMechanisms.hitTile.UpdatePosition(tileId, x, y);
                }
                if (tile.frameY == 36) {
                    y--;
                    tile = Main.tile[x, y];
                    //MoreMechanisms.hitTile.UpdatePosition(tileId, x, y);
                }
                int j = tile.frameX;
                bool flag3 = j >= 5000;
                bool flag2 = false;
                if (!flag3) {
                    int num5 = j / 18;
                    num5 %= 3;
                    x -= num5;
                    tile = Main.tile[x, y];
                    if (tile.frameX >= 5000) {
                        flag3 = true;
                    }
                }
                if (flag3) {
                    j = tile.frameX;
                    int num3 = 0;
                    while (j >= 5000) {
                        j -= 5000;
                        num3++;
                    }
                    if (num3 != 0) {
                        flag2 = true;
                    }
                }
                if (flag2) {
                    num = 0;
                    Main.blockMouse = true;
                }
            }
            if (!WorldGen.CanKillTile(x, y)) {
                num = 0;
            }
            return num != 0;
        }

        /// <summary>
        /// Gets the Texture2D for a specific tile.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public static Texture2D GetTileTexture(Tile tile) {
            Texture2D texture = (!Main.canDrawColorTile(tile.type, tile.color())) ? Main.tileTexture[tile.type] : Main.tileAltTexture[tile.type, tile.color()];
            return CropTexture(texture, new Rectangle(9 * 18, 3 * 18, 16, 16));
        }

        /// <summary>
        /// <para>Draws the Tile onto the provided SpriteBatch at screen position (sx, sy).</para>
        /// <para>Based off of some Terraria code but I don't remember where.</para>
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="tile"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        public static void DrawTile(SpriteBatch spriteBatch, Tile tile, int sx, int sy) {
            Texture2D texture = (!Main.canDrawColorTile(tile.type, tile.color())) ? Main.tileTexture[tile.type] : Main.tileAltTexture[tile.type, tile.color()];
            Vector2 vector = new Vector2(8f);
            Vector2 value2 = new Vector2(1f);
            Vector2 scale = value2;
            Vector2 position = (new Vector2((float)(sx), (float)(sy))).Floor();
            spriteBatch.Draw(texture, position, new Rectangle(9 * 18, 3 * 18, 16, 16), Color.White, 0, vector, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// <para>Draws the Tile onto the provided SpriteBatch at the Tile's position in the world.</para>
        /// <para>Based off of some Terraria code but I don't remember where.</para>
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        public static void DrawWorldTile(SpriteBatch spriteBatch, int tx, int ty) {
            Color color = Lighting.GetColor(tx, ty);
            //color = Color.Red;
            float rotation = 0f;
            Vector2 zero2 = Vector2.Zero;
            Tile tileSafely = Framing.GetTileSafely(tx, ty);
            Tile tile = tileSafely;
            Texture2D texture = (!Main.canDrawColorTile(tileSafely.type, tileSafely.color())) ? Main.tileTexture[tileSafely.type] : Main.tileAltTexture[tileSafely.type, tileSafely.color()];
            Vector2 vector = new Vector2(8f);
            Vector2 value2 = new Vector2(1f);
            float scaleFactor = 1f;
            float num10 = 1f;
            num10 = 1f;
            color *= num10 * num10 * 0.8f;
            Vector2 scale = scaleFactor * value2;
            Vector2 position = (new Vector2((float)(tx * 16 - (int)Main.screenPosition.X), (float)(ty * 16 - (int)Main.screenPosition.Y)) + Vector2.Zero + vector + zero2).Floor();
            spriteBatch.Draw(texture, position, new Rectangle(tile.frameX, tile.frameY, 16, 16), color, rotation, vector, scale, SpriteEffects.None, 0f);
        }
    }
}
