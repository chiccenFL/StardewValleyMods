﻿using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WaterYourCrops
{
    public partial class ModEntry
    {
        // HoeDirt.dry = 0;
        // HoeDirt.watered = 1;
        // both apply to HoeDirt.state
        // so if HoeDirt.state == 1, then that dirt is watered

        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.DrawOptimized))]
        public class HoeDirt_DrawOptimized_Patch
        {
            public static void Postfix(HoeDirt __instance, SpriteBatch dirt_batch)
            {
                if (!Config.EnableMod || __instance.state.Value != 0 || __instance.crop is null || (!HasCan() && Config.OnlyWaterCan))
                    return;
                
                Vector2 tile = __instance.Tile;
                Vector2 drawPos = Game1.GlobalToLocal(Game1.viewport, tile * 64f);
                
                byte drawSum = 0;
                Vector2 tileLocation = tile;
                tileLocation.X += 1f;
                GameLocation location = Game1.player.currentLocation;
                
                if (location.terrainFeatures.TryGetValue(tileLocation, out var rightFeature) && rightFeature is HoeDirt)
                    drawSum += 2;
                tileLocation.X -= 2f;
                if (location.terrainFeatures.TryGetValue(tileLocation, out var leftFeature) && leftFeature is HoeDirt)
                    drawSum += 8;
                tileLocation.X += 1f;
                tileLocation.Y += 1f;
                if (location.terrainFeatures.TryGetValue(tileLocation, out var downFeature) && downFeature is HoeDirt)
                    drawSum += 4;
                tileLocation.Y -= 2f;
                if (location.terrainFeatures.TryGetValue(tileLocation, out var upFeature) && upFeature is HoeDirt)
                    drawSum++;

                int sourceRectPosition = HoeDirt.drawGuide[drawSum];

                float depthLayer = __instance.crop.layerDepth - 0.01f;

                dirt_batch?.Draw(
                    waterTexture,
                    drawPos,
                    new Rectangle(sourceRectPosition % 4 * 16, sourceRectPosition / 4 * 16, 16, 16),
                    Config.IndicatorColor * Config.IndicatorOpacity,
                    0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    depthLayer
                    //(tile.Y * 64f + 32f + (tile.Y * 11f + tile.X * 7f) % 10f - 5f) / 10000f -- this was based on player position draw logic iirc. might still be useful in the future
                );
            }
        }

    }
}
