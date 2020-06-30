using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using UltimaExample.ModTool;

namespace UltimaExample.NPCs
{
    public class Ofire : ModNPC
    {
        private Player player;

        private float speed;

        public float[] customAI = new float[2];

        private bool start;

        private bool charging;

        private int chargeFrame;

        private bool shooting;

        private int shootFrame;

        private int chargeCounter;

        private int shootCounter;

        private bool idleStart;

        private bool charging2;

        private int deadTimer;

        private bool phase2;

        private bool missileLaunch1;

        private bool missileDone1;

        private bool laser1;

        private bool laserDone1;

        private bool laserFiring1;

        private int laserFrame;

        private int laser1Frame;

        private bool laserFiring2;

        private int laser2Frame;

        private int laser1Counter;

        private int laser2Counter;

        private int laserSpamTimer;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("演示BOSS");
        }
        /*
         * 这是一个演示BOSS。
         * 它使用NPC代码的最标准技术ai数组来制作。
         * 运用大量的高级ai。
         * 可能用状态机这种更简单的方法表达出来。
         */
        public override void SetDefaults()
        {
            base.npc.aiStyle = -1;
            this.aiType = 0;
            base.npc.lifeMax = 115500;
            base.npc.damage = 280;
            base.npc.defense = 80;
            base.npc.knockBackResist = 0f;
            base.npc.width = 112;
            base.npc.height = 178;
            base.npc.value = (float)Item.buyPrice(0, 0, 0, 0);
            base.npc.npcSlots = 2f;
            base.npc.boss = true;
            base.npc.buffImmune[20] = true;
            base.npc.buffImmune[31] = true;
            base.npc.buffImmune[39] = true;
            base.npc.buffImmune[24] = true;
            Main.npcFrameCount[base.npc.type] = 5;
            base.npc.netAlways = true;
            base.npc.lavaImmune = true;
            base.npc.noGravity = true;
            base.npc.noTileCollide = true;
            base.npc.HitSound = SoundID.NPCHit42;
            base.npc.DeathSound = SoundID.NPCDeath14;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            Dust.NewDust(base.npc.position + base.npc.velocity, base.npc.width, base.npc.height, 226, base.npc.velocity.X * 0.5f, base.npc.velocity.Y * 0.5f, 0, default(Color), 1f);
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            base.npc.lifeMax = (int)((float)base.npc.lifeMax * 0.5f * bossLifeScale);
            base.npc.damage = (int)((float)base.npc.damage * 0.2f);
            base.npc.defense = base.npc.defense + numPlayers;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == 2 || Main.dedServ)
            {
                writer.Write(this.customAI[0]);
                writer.Write(this.customAI[1]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == 1)
            {
                this.customAI[0] = reader.ReadFloat();
                this.customAI[1] = reader.ReadFloat();
            }
        }
        /*
         * 给自定义ai值。
         * 有时候ai数组不够用时可以添加。
         */

        private void Target()
        {
            this.player = Main.player[base.npc.target];
        }

        private void Move(Vector2 offset)
        {
            this.speed = 20f;
            Vector2 move = this.player.Center + offset - base.npc.Center;
            float magnitude = this.Magnitude(move);
            if (magnitude > this.speed)
            {
                move *= this.speed / magnitude;
            }
            float turnResistance = 10f;
            move = (base.npc.velocity * turnResistance + move) / (turnResistance + 1f);
            magnitude = this.Magnitude(move);
            if (magnitude > this.speed)
            {
                move *= this.speed / magnitude;
            }
            base.npc.velocity = move;
        }

        private float Magnitude(Vector2 mag)
        {
            return (float)Math.Sqrt((double)(mag.X * mag.X + mag.Y * mag.Y));
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
        } //原版血条大小。

        private void DespawnHandler()
        {
            if (!this.player.active || this.player.dead)
            {
                base.npc.TargetClosest(false);
                this.player = Main.player[base.npc.target];
                if (!this.player.active || this.player.dead)
                {
                    this.deadTimer++;
                    if (this.deadTimer == 2)
                    {
                        CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "?????", true, false);
                    }
                    base.npc.velocity = new Vector2(0f, -10f);
                    if (base.npc.timeLeft > 10)
                    {
                        base.npc.timeLeft = 10;
                    }
                    return;
                }
            }
        }

        public override void AI()
        {
            this.Target();
            this.DespawnHandler();
            if (Main.rand.Next(2) == 0)
            {
                Dust.NewDust(new Vector2(base.npc.position.X + 36f, base.npc.position.Y + 54f), 4, 4, 235, 0f, 0f, 0, default(Color), 1f);
            }
            if (!this.charging)
            {
                this.Move(new Vector2(-340f, 0f));
            }
            if (this.customAI[0] == 0f)
            {
                base.npc.ai[0] += 1f;
                base.npc.dontTakeDamage = true;
                this.idleStart = true;
            }
            if (this.customAI[0] == 1f)
            {
                if (Main.rand.Next(250) == 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        int dustIndex = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int ProjID = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(0f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[ProjID].netUpdate = true;
                }
                base.npc.ai[1] += 1f;
                if (base.npc.life <= 71625)
                {
                    if (base.npc.ai[1] == 300f)
                    {
                        this.idleStart = false;
                        this.charging = true;
                        Vector2 vector8 = new Vector2(base.npc.position.X + (float)base.npc.width * 0.5f, base.npc.position.Y + (float)base.npc.height * 0.5f);
                        float rotation = (float)Math.Atan2((double)(vector8.Y - (Main.player[base.npc.target].position.Y + (float)Main.player[base.npc.target].height * 0.5f)), (double)(vector8.X - (Main.player[base.npc.target].position.X + (float)Main.player[base.npc.target].width * 0.5f)));
                        base.npc.velocity.X = (float)(Math.Cos((double)rotation) * 26.0) * -1f;
                        base.npc.velocity.Y = (float)(Math.Sin((double)rotation) * 26.0) * -1f;
                        base.npc.ai[0] %= 6.2831855f;
                        new Vector2((float)Math.Cos((double)base.npc.ai[0]), (float)Math.Sin((double)base.npc.ai[0]));
                        Main.PlaySound(2, (int)base.npc.position.X, (int)base.npc.position.Y, 20, 1f, 0f);
                        Color color = default(Color);
                        Rectangle rectangle = new Rectangle((int)base.npc.position.X, (int)(base.npc.position.Y + (float)((base.npc.height - base.npc.width) / 2)), base.npc.width, base.npc.width);
                        int count = 30;
                        for (int j = 1; j <= count; j++)
                        {
                            int dust = Dust.NewDust(base.npc.position, rectangle.Width, rectangle.Height, 235, 0f, 0f, 100, color, 2.5f);
                            Main.dust[dust].noGravity = false;
                        }
                        base.npc.netUpdate = true;
                        return;
                    }
                    if (base.npc.ai[1] == 330f)
                    {
                        this.charging = false;
                        this.charging2 = true;
                        Vector2 vector9 = new Vector2(base.npc.position.X + (float)base.npc.width * 0.5f, base.npc.position.Y + (float)base.npc.height * 0.5f);
                        float rotation2 = (float)Math.Atan2((double)(vector9.Y - (Main.player[base.npc.target].position.Y + (float)Main.player[base.npc.target].height * 0.5f)), (double)(vector9.X - (Main.player[base.npc.target].position.X + (float)Main.player[base.npc.target].width * 0.5f)));
                        base.npc.velocity.X = (float)(Math.Cos((double)rotation2) * 26.0) * -1f;
                        base.npc.velocity.Y = (float)(Math.Sin((double)rotation2) * 26.0) * -1f;
                        base.npc.ai[0] %= 6.2831855f;
                        new Vector2((float)Math.Cos((double)base.npc.ai[0]), (float)Math.Sin((double)base.npc.ai[0]));
                        Main.PlaySound(2, (int)base.npc.position.X, (int)base.npc.position.Y, 20, 1f, 0f);
                        Color color2 = default(Color);
                        Rectangle rectangle2 = new Rectangle((int)base.npc.position.X, (int)(base.npc.position.Y + (float)((base.npc.height - base.npc.width) / 2)), base.npc.width, base.npc.width);
                        int count2 = 30;
                        for (int k = 1; k <= count2; k++)
                        {
                            int dust2 = Dust.NewDust(base.npc.position, rectangle2.Width, rectangle2.Height, 235, 0f, 0f, 100, color2, 2.5f);
                            Main.dust[dust2].noGravity = false;
                        }
                        base.npc.netUpdate = true;
                        return;
                    }
                }
                if (base.npc.life > 71625)
                {
                    if (base.npc.ai[1] == 300f)
                    {
                        this.idleStart = false;
                        this.charging = true;
                        Vector2 vector10 = new Vector2(base.npc.position.X + (float)base.npc.width * 0.5f, base.npc.position.Y + (float)base.npc.height * 0.5f);
                        float rotation3 = (float)Math.Atan2((double)(vector10.Y - (Main.player[base.npc.target].position.Y + (float)Main.player[base.npc.target].height * 0.5f)), (double)(vector10.X - (Main.player[base.npc.target].position.X + (float)Main.player[base.npc.target].width * 0.5f)));
                        base.npc.velocity.X = (float)(Math.Cos((double)rotation3) * 24.0) * -1f;
                        base.npc.velocity.Y = (float)(Math.Sin((double)rotation3) * 24.0) * -1f;
                        base.npc.ai[0] %= 6.2831855f;
                        new Vector2((float)Math.Cos((double)base.npc.ai[0]), (float)Math.Sin((double)base.npc.ai[0]));
                        Main.PlaySound(2, (int)base.npc.position.X, (int)base.npc.position.Y, 20, 1f, 0f);
                        Color color3 = default(Color);
                        Rectangle rectangle3 = new Rectangle((int)base.npc.position.X, (int)(base.npc.position.Y + (float)((base.npc.height - base.npc.width) / 2)), base.npc.width, base.npc.width);
                        int count3 = 30;
                        for (int l = 1; l <= count3; l++)
                        {
                            int dust3 = Dust.NewDust(base.npc.position, rectangle3.Width, rectangle3.Height, 235, 0f, 0f, 100, color3, 2.5f);
                            Main.dust[dust3].noGravity = false;
                        }
                        base.npc.netUpdate = true;
                        return;
                    }
                    if (base.npc.ai[1] == 330f)
                    {
                        this.charging = false;
                        this.charging2 = true;
                        Vector2 vector11 = new Vector2(base.npc.position.X + (float)base.npc.width * 0.5f, base.npc.position.Y + (float)base.npc.height * 0.5f);
                        float rotation4 = (float)Math.Atan2((double)(vector11.Y - (Main.player[base.npc.target].position.Y + (float)Main.player[base.npc.target].height * 0.5f)), (double)(vector11.X - (Main.player[base.npc.target].position.X + (float)Main.player[base.npc.target].width * 0.5f)));
                        base.npc.velocity.X = (float)(Math.Cos((double)rotation4) * 24.0) * -1f;
                        base.npc.velocity.Y = (float)(Math.Sin((double)rotation4) * 24.0) * -1f;
                        base.npc.ai[0] %= 6.2831855f;
                        new Vector2((float)Math.Cos((double)base.npc.ai[0]), (float)Math.Sin((double)base.npc.ai[0]));
                        Main.PlaySound(2, (int)base.npc.position.X, (int)base.npc.position.Y, 20, 1f, 0f);
                        Color color4 = default(Color);
                        Rectangle rectangle4 = new Rectangle((int)base.npc.position.X, (int)(base.npc.position.Y + (float)((base.npc.height - base.npc.width) / 2)), base.npc.width, base.npc.width);
                        int count4 = 30;
                        for (int m = 1; m <= count4; m++)
                        {
                            int dust4 = Dust.NewDust(base.npc.position, rectangle4.Width, rectangle4.Height, 235, 0f, 0f, 100, color4, 2.5f);
                            Main.dust[dust4].noGravity = false;
                        }
                        base.npc.netUpdate = true;
                        return;
                    }
                }
                if (base.npc.ai[1] >= 360f)
                {
                    this.customAI[0] = 2f;
                    this.charging2 = false;
                    base.npc.netUpdate = true;
                }
            }
            if (this.customAI[0] == 2f)
            {
                if (Main.rand.Next(300) == 0)
                {
                    for (int n = 0; n < 20; n++)
                    {
                        int dustIndex2 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex2].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(0f, -20f), base.mod.ProjectileType("Missile"), 70, 3f, 255, 0f, 0f);
                    Main.projectile[p].netUpdate = true;
                }
                this.customAI[1] += 1f;
                if (base.npc.life > 71625)
                {
                    if (this.customAI[1] <= 89f)
                    {
                        this.idleStart = true;
                    }
                    if (this.customAI[1] >= 90f && this.customAI[1] <= 209f)
                    {
                        this.shooting = true;
                        this.idleStart = false;
                    }
                    if (this.customAI[1] == 145f)
                    {
                        Main.PlaySound(SoundID.Item92, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p2 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(4f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p2].netUpdate = true;
                    }
                    if (this.customAI[1] == 150f)
                    {
                        Main.PlaySound(SoundID.Item92, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p3 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(8f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p3].netUpdate = true;
                    }
                    if (this.customAI[1] == 155f)
                    {
                        Main.PlaySound(SoundID.Item92, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p4 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(12f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p4].netUpdate = true;
                    }
                    if (this.customAI[1] >= 210f)
                    {
                        this.shooting = false;
                        this.idleStart = true;
                        this.customAI[0] = 1f;
                        base.npc.netUpdate = true;
                    }
                    if (this.customAI[1] == 210f)
                    {
                        base.npc.ai[1] = (float)Main.rand.Next(-200, 100);
                        this.customAI[1] = 0f;
                        this.shootCounter = 0;
                        this.shootFrame = 0;
                        base.npc.netUpdate = true;
                    }
                }
                if (base.npc.life <= 71625 && base.npc.life > 55000)
                {
                    if (this.customAI[1] <= 69f)
                    {
                        this.idleStart = true;
                    }
                    if (this.customAI[1] >= 70f && this.customAI[1] <= 189f)
                    {
                        this.shooting = true;
                        this.idleStart = false;
                    }
                    if (this.customAI[1] == 125f)
                    {
                        Main.PlaySound(SoundID.Item92, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p5 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(8f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        int p6 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(6f, 3f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        int p7 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(6f, -3f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p5].netUpdate = true;
                        Main.projectile[p6].netUpdate = true;
                        Main.projectile[p7].netUpdate = true;
                    }
                    if (this.customAI[1] == 122f)
                    {
                        Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p8 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(8f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p8].netUpdate = true;
                    }
                    if (this.customAI[1] == 126f)
                    {
                        Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p9 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(10f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p9].netUpdate = true;
                    }
                    if (this.customAI[1] == 130f)
                    {
                        Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p10 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(12f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p10].netUpdate = true;
                    }
                    if (this.customAI[1] == 134f)
                    {
                        Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p11 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(14f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p11].netUpdate = true;
                    }
                    if (this.customAI[1] == 138f)
                    {
                        Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p12 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(16f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p12].netUpdate = true;
                    }
                    if (this.customAI[1] == 135f)
                    {
                        Main.PlaySound(SoundID.Item92, (int)base.npc.position.X, (int)base.npc.position.Y);
                        int p13 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(8f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        int p14 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(6f, 3f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        int p15 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(6f, -3f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                        Main.projectile[p13].netUpdate = true;
                        Main.projectile[p14].netUpdate = true;
                        Main.projectile[p15].netUpdate = true;
                    }
                    if (this.customAI[1] >= 190f)
                    {
                        this.shooting = false;
                        this.idleStart = true;
                        this.customAI[0] = 1f;
                        base.npc.netUpdate = true;
                    }
                    if (this.customAI[1] == 190f)
                    {
                        base.npc.ai[1] = (float)Main.rand.Next(-150, 150);
                        this.customAI[1] = 0f;
                        this.shootCounter = 0;
                        this.shootFrame = 0;
                        base.npc.netUpdate = true;
                    }
                }
            }
            if (base.npc.life <= 55000)
            {
                if (this.customAI[1] <= 69f)
                {
                    this.idleStart = true;
                }
                if (this.customAI[1] >= 70f && this.customAI[1] <= 189f)
                {
                    this.shooting = true;
                    this.idleStart = false;
                }
                if (this.customAI[1] == 125f)
                {
                    Main.PlaySound(SoundID.Item92, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p16 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(0f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p16].netUpdate = true;
                }
                if (this.customAI[1] == 122f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p17 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(4f, 4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p18 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(6f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p19 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(4f, -4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p17].netUpdate = true;
                    Main.projectile[p18].netUpdate = true;
                    Main.projectile[p19].netUpdate = true;
                }
                if (this.customAI[1] == 124f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p20 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(7f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p20].netUpdate = true;
                }
                if (this.customAI[1] == 126f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p21 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(6f, 4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p22 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(8f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p23 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(6f, -4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p21].netUpdate = true;
                    Main.projectile[p22].netUpdate = true;
                    Main.projectile[p23].netUpdate = true;
                }
                if (this.customAI[1] == 128f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p24 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(9f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p24].netUpdate = true;
                }
                if (this.customAI[1] == 130f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p25 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(8f, 4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p26 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(10f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p27 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(8f, -4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p28 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(0f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p25].netUpdate = true;
                    Main.projectile[p26].netUpdate = true;
                    Main.projectile[p27].netUpdate = true;
                    Main.projectile[p28].netUpdate = true;
                }
                if (this.customAI[1] == 132f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p29 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(11f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p29].netUpdate = true;
                }
                if (this.customAI[1] == 134f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p30 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(10f, 4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p31 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(12f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p32 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(10f, -4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p30].netUpdate = true;
                    Main.projectile[p31].netUpdate = true;
                    Main.projectile[p32].netUpdate = true;
                }
                if (this.customAI[1] == 136f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p33 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(13f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p33].netUpdate = true;
                }
                if (this.customAI[1] == 138f)
                {
                    Main.PlaySound(SoundID.Item125, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p34 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(12f, 4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p35 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(14f, 0f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    int p36 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(12f, -4f), base.mod.ProjectileType("Blast"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p34].netUpdate = true;
                    Main.projectile[p35].netUpdate = true;
                    Main.projectile[p36].netUpdate = true;
                }
                if (this.customAI[1] == 135f)
                {
                    Main.PlaySound(SoundID.Item92, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p37 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(0f, 0f), base.mod.ProjectileType("PlasmaBall"), 50, 3f, 255, 0f, 0f);
                    Main.projectile[p37].netUpdate = true;
                }
                if (this.customAI[1] >= 190f)
                {
                    this.shooting = false;
                    this.idleStart = true;
                    this.customAI[0] = 1f;
                    base.npc.netUpdate = true;
                }
                if (this.customAI[1] == 190f)
                {
                    base.npc.ai[1] = (float)Main.rand.Next(-50, 150);
                    this.customAI[1] = 0f;
                    this.shootCounter = 0;
                    this.shootFrame = 0;
                    base.npc.netUpdate = true;
                }
            }
            if (base.npc.life <= 50000 && !this.missileDone1)
            {
                this.missileLaunch1 = true;
                base.npc.netUpdate = true;
            }
            if (this.missileLaunch1)
            {
                this.shooting = false;
                this.charging = false;
                this.charging2 = false;
                this.idleStart = true;
                base.npc.ai[1] = 0f;
                this.customAI[1] = 0f;
                this.shootCounter = 0;
                this.shootFrame = 0;
                base.npc.ai[2] += 1f;
                if (base.npc.ai[2] == 30f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "准备发射...", true, false);
                }
                if (base.npc.ai[2] == 120f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "在5秒后发射...", true, false);
                    Main.PlaySound(SoundID.Item47, (int)base.npc.position.X, (int)base.npc.position.Y);
                    base.npc.netUpdate = true;
                }
                if (base.npc.ai[2] == 180f)
                {
                    Main.PlaySound(SoundID.Item47, (int)base.npc.position.X, (int)base.npc.position.Y);
                }
                if (base.npc.ai[2] == 240f)
                {
                    Main.PlaySound(SoundID.Item47, (int)base.npc.position.X, (int)base.npc.position.Y);
                }
                if (base.npc.ai[2] == 300f)
                {
                    Main.PlaySound(SoundID.Item47, (int)base.npc.position.X, (int)base.npc.position.Y);
                }
                if (base.npc.ai[2] == 360f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "...", true, false);
                    Main.PlaySound(SoundID.Item47, (int)base.npc.position.X, (int)base.npc.position.Y);
                    base.npc.netUpdate = true;
                }
                if (base.npc.ai[2] == 420f)
                {
                    for (int i2 = 0; i2 < 20; i2++)
                    {
                        int dustIndex3 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex3].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p38 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(0f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p38].netUpdate = true;
                }
                if (base.npc.ai[2] == 422f)
                {
                    for (int i3 = 0; i3 < 20; i3++)
                    {
                        int dustIndex4 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex4].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p39 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(5f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p39].netUpdate = true;
                }
                if (base.npc.ai[2] == 425f)
                {
                    for (int i4 = 0; i4 < 20; i4++)
                    {
                        int dustIndex5 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex5].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p40 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(-5f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p40].netUpdate = true;
                }
                if (base.npc.ai[2] == 426f)
                {
                    for (int i5 = 0; i5 < 20; i5++)
                    {
                        int dustIndex6 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex6].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p41 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(-2f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p41].netUpdate = true;
                }
                if (base.npc.ai[2] == 428f)
                {
                    for (int i6 = 0; i6 < 20; i6++)
                    {
                        int dustIndex7 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex7].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p42 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(7f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p42].netUpdate = true;
                }
                if (base.npc.ai[2] == 431f)
                {
                    for (int i7 = 0; i7 < 20; i7++)
                    {
                        int dustIndex8 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex8].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p43 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(1f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p43].netUpdate = true;
                }
                if (base.npc.ai[2] == 432f)
                {
                    for (int i8 = 0; i8 < 20; i8++)
                    {
                        int dustIndex9 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex9].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p44 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(-8f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p44].netUpdate = true;
                }
                if (base.npc.ai[2] == 435f)
                {
                    for (int i9 = 0; i9 < 20; i9++)
                    {
                        int dustIndex10 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex10].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p45 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(6f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p45].netUpdate = true;
                }
                if (base.npc.ai[2] == 436f)
                {
                    for (int i10 = 0; i10 < 20; i10++)
                    {
                        int dustIndex11 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex11].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p46 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(2f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p46].netUpdate = true;
                }
                if (base.npc.ai[2] == 440f)
                {
                    for (int i11 = 0; i11 < 20; i11++)
                    {
                        int dustIndex12 = Dust.NewDust(new Vector2(base.npc.position.X + 48f, base.npc.position.Y + 26f), 6, 6, 235, 0f, 0f, 100, default(Color), 1.2f);
                        Main.dust[dustIndex12].velocity *= 1.9f;
                    }
                    Main.PlaySound(SoundID.Item74, (int)base.npc.position.X, (int)base.npc.position.Y);
                    int p47 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 54f, base.npc.position.Y + 32f), new Vector2(0f, -20f), base.mod.ProjectileType("Missile"), 60, 3f, 255, 0f, 0f);
                    Main.projectile[p47].netUpdate = true;
                }
                if (base.npc.ai[2] == 500f)
                {
                    base.npc.ai[1] = (float)Main.rand.Next(-50, 150);
                    this.customAI[1] = 0f;
                    this.shootCounter = 0;
                    this.shootFrame = 0;
                    this.missileDone1 = true;
                    this.missileLaunch1 = false;
                    base.npc.netUpdate = true;
                }
            }
            if (base.npc.life <= 10000 && !this.laserDone1)
            {
                this.laser1 = true;
                base.npc.netUpdate = true;
            }
            if (this.laser1)
            {
                this.shooting = false;
                this.charging = false;
                this.charging2 = false;
                base.npc.ai[1] = 0f;
                this.customAI[1] = 0f;
                this.shootCounter = 0;
                this.shootFrame = 0;
                base.npc.ai[3] += 1f;
                if (base.npc.ai[3] < 60f)
                {
                    this.idleStart = true;
                }
                if (base.npc.ai[3] == 10f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "准备发射终极武器...", true, false);
                    base.npc.netUpdate = true;
                }
                if (base.npc.ai[3] == 60f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "10...", true, false);
                }
                if (base.npc.ai[3] == 120f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "9...", true, false);
                }
                if (base.npc.ai[3] == 180f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "8...", true, false);
                }
                if (base.npc.ai[3] == 240f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "7...", true, false);
                }
                if (base.npc.ai[3] == 300f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "6...", true, false);
                }
                if (base.npc.ai[3] == 360f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "5...", true, false);
                }
                if (base.npc.ai[3] == 420f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "4...", true, false);
                }
                if (base.npc.ai[3] == 480f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "3...", true, false);
                }
                if (base.npc.ai[3] == 540f)
                {
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "再见...", true, false);
                    base.npc.netUpdate = true;
                }
                if (base.npc.ai[3] >= 660f && base.npc.ai[3] < 940f)
                {
                    this.idleStart = false;
                    this.laserFiring1 = true;
                }
                if (base.npc.ai[3] == 650f && !Main.dedServ)
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/MegaLaser1").WithVolume(0.9f).WithPitchVariance(0f), -1, -1);
                }
                if (base.npc.ai[3] >= 735f && base.npc.ai[3] <= 925f)
                {
                    this.laserSpamTimer++;
                    if (this.laserSpamTimer >= 2)
                    {
                        Main.PlaySound(SoundID.Item33, (int)base.npc.position.X, (int)base.npc.position.Y);
                        Dust.NewDust(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), 4, 4, 235, 0f, 0f, 0, default(Color), 1f);
                        if (Main.netMode != 1)
                        {
                            int p48 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(20f, 0f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            int p49 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(10f, 10f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            int p50 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(10f, -10f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            int p51 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(-10f, 10f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            int p52 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(-10f, -10f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            int p53 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(-20f, 0f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            int p54 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(0f, 20f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            int p55 = Projectile.NewProjectile(new Vector2(base.npc.position.X + 30f, base.npc.position.Y + 62f), new Vector2(0f, -20f), base.mod.ProjectileType("Missile"), 200, 3f, 255, 0f, 0f);
                            Main.projectile[p48].netUpdate = true;
                            Main.projectile[p49].netUpdate = true;
                            Main.projectile[p50].netUpdate = true;
                            Main.projectile[p51].netUpdate = true;
                            Main.projectile[p52].netUpdate = true;
                            Main.projectile[p53].netUpdate = true;
                            Main.projectile[p54].netUpdate = true;
                            Main.projectile[p55].netUpdate = true;
                        }
                        this.laserSpamTimer = 0;
                    }
                }
                if (base.npc.ai[3] >= 940f && base.npc.ai[3] < 970f)
                {
                    this.laserFiring1 = false;
                    this.laserFiring2 = true;
                }
                if (base.npc.ai[3] >= 970f)
                {
                    base.npc.ai[1] = (float)Main.rand.Next(-50, 150);
                    this.customAI[1] = 0f;
                    this.shootCounter = 0;
                    this.shootFrame = 0;
                    this.laserFiring1 = false;
                    this.laserFiring2 = false;
                    this.idleStart = true;
                    this.laserDone1 = true;
                    this.laser1 = false;
                    CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "...", true, false);
                    base.npc.netUpdate = true;
                }
            }
            if (base.npc.ai[0] == 1f)
            {
                for (int i12 = 0; i12 < 100; i12++)
                {
                    int dustIndex13 = Dust.NewDust(new Vector2(base.npc.position.X, base.npc.position.Y), base.npc.width, base.npc.height, 235, 0f, 0f, 100, default(Color), 1.5f);
                    Main.dust[dustIndex13].velocity *= 1.9f;
                }
            }
            if (base.npc.ai[0] == 60f)
            {
                CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "...", true, false);
            }
            if (base.npc.ai[0] == 120f)
            {
                CombatText.NewText(base.npc.getRect(), Colors.RarityRed, "...", true, false);
            }
            if (base.npc.ai[0] == 260f)
            {
                this.customAI[0] = 1f;
                base.npc.dontTakeDamage = false;
                base.npc.netUpdate = true;
            }
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            if (damage > 15000)
            {
                damage = 0;
            }
            return true;
            /*
             * 限伤。
             */
        }
    }
}
