using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace UltimaExample.ModTool
{
    public static class BaseExtensions
    {
        public static void AddRecipeGroup(this ModRecipe recipe, Mod mod, string groupName, int count)
        {
            Mod i = (mod == null) ? recipe.mod : mod;
            recipe.AddRecipeGroup(i.Name + ":" + groupName, count);
        }

        public static void AddItem(this ModRecipe recipe, int itemID, int count)
        {
            recipe.AddIngredient(itemID, count);
        }

        public static void AddItem(this ModRecipe recipe, Mod mod, string itemName, int count)
        {
            recipe.AddIngredient((mod == null) ? recipe.mod : mod, itemName, count);
        }

        public static void ClearBuff(this Player player, Mod mod, string name)
        {
            player.ClearBuff(mod.BuffType(name));
        }

        public static void AddBuff(this Player player, Mod mod, string name, int time, bool sync = true)
        {
            player.AddBuff(mod.BuffType(name), time, sync);
        }

        public static int FindBuffIndex(this Player player, Mod mod, string name)
        {
            return player.FindBuffIndex(mod.BuffType(name));
        }

        public static int MusicType(this Mod mod, string name, string prefix = "Sounds/Music/")
        {
            return mod.GetSoundSlot(Terraria.ModLoader.SoundType.Music, prefix + name);
        }

        public static LegacySoundStyle SoundCustom(this Mod mod, string name, string prefix = "Sounds/Custom/")
        {
            return mod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.Custom, prefix + name);
        }

        public static LegacySoundStyle SoundItem(this Mod mod, string name, string prefix = "Sounds/Item/")
        {
            return mod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.Item, prefix + name);
        }
 
        public static LegacySoundStyle SoundNPCHit(this Mod mod, string name, string prefix = "Sounds/NPCHit/")
        {
            return mod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.NPCHit, prefix + name);
        }

        public static LegacySoundStyle SoundNPCKilled(this Mod mod, string name, string prefix = "Sounds/NPCKilled/")
        {
            return mod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.NPCKilled, prefix + name);
        }

        public static int ProjType(this Mod mod, string name)
        {
            return mod.ProjectileType(name);
        }

        public static bool ReadBool(this BinaryReader w)
        {
            return w.ReadBoolean();
        }

        public static int ReadInt(this BinaryReader w)
        {
            return w.ReadInt32();
        }

        public static short ReadShort(this BinaryReader w)
        {
            return w.ReadInt16();
        }

        public static ushort ReadUShort(this BinaryReader w)
        {
            return w.ReadUInt16();
        }

        public static float ReadFloat(this BinaryReader w)
        {
            return w.ReadSingle();
        }

        public static bool IsBlank(this Item item)
        {
            return item.type <= 0 || item.stack <= 0 || string.IsNullOrEmpty(item.Name);
        }

        public static bool water(this Tile tile)
        {
            return tile.liquidType() == 0;
        }

        public static bool lava(this Tile tile)
        {
            return tile.liquidType() == 1;
        }

        public static bool honey(this Tile tile)
        {
            return tile.liquidType() == 2;
        }
    }
}
