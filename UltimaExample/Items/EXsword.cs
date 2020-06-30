using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace UltimaExample.Items
{
	public class EXsword : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("EXsword");
			Tooltip.SetDefault("测试用剑.");
		}
		public override void SetDefaults()
		{
			item.damage = 500000; //伤害
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 1;
			item.knockBack = 6;
			item.value = 10000;
			item.rare = 2;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            target.active = false;
            target.life = target.life - 9999999;
            /*
             * 强行代码杀
             * 一般不会掉落物品
             * 可以直接杀神长直，终灾
             */
        }
    }
}
