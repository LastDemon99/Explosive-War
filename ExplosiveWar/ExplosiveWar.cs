using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;

namespace ExplosiveWar
{
    public class ExplosiveWar : BaseScript
    {
        public static int fx1 = GSCFunctions.LoadFX("fire/ballistic_vest_death");
        public static int fx2 = GSCFunctions.LoadFX("smoke/smoke_trail_white_heli");

        public ExplosiveWar()
        {
            GSCFunctions.PreCacheItem("at4_mp");
            GSCFunctions.PreCacheItem("uav_strike_marker_mp");
            ChangeSpeed(250);
            ChangeJump(75);

            GSCFunctions.MakeDvarServerInfo("didyouknow", "^2Explosive War script by LastDemon99");
            GSCFunctions.MakeDvarServerInfo("g_motd", "^2Explosive War script by LastDemon99");
            GSCFunctions.MakeDvarServerInfo("motd", "^2Explosive War script by LastDemon99");

            InfiniteStock();
            PlayerConnected += new Action<Entity>((player) =>
            {
                ServerWelcomeTittle(player, "Explosive War", new float[] { 0, 0, 1 });
                player.SetClientDvar("ui_mapname", "Explosive War");
                player.SetClientDvar("ui_gametype", "Explosive War");

                PlayerFunc(player);
                OnSpawn(player); 
            });
        }

        private void OnSpawn(Entity player)
        {
            player.SpawnedPlayer += new Action(() =>
            {
                GSCFunctions.DisableWeaponPickup(player);

                if (player.SessionTeam == "allies")
                {
                    player.SetModel("mp_fullbody_ally_juggernaut");
                    player.SetViewModel("viewhands_juggernaut_ally");
                }
                else if (player.SessionState != "spectator")
                {
                    player.SetModel("mp_fullbody_opforce_juggernaut");
                    player.SetViewModel("viewhands_juggernaut_opforce");
                }

                player.TakeAllWeapons();
                player.GiveWeapon("at4_mp");
                player.GiveWeapon("uav_strike_marker_mp");
                AfterDelay(400, () => { player.SwitchToWeaponImmediate("at4_mp"); });
                GiveAllPerks(player);
                player.SetPerk("specialty_falldamage", false, true);
            });
        }
        private void PlayerFunc(Entity player)
        {
            player.SetField("readydash", true);
            player.SetField("jump", true);
            player.SetField("fuel", 2);
            player.NotifyOnPlayerCommand("jump", "+gostand");
            player.NotifyOnPlayerCommand("dash", "+breath_sprint");

            player.OnNotify("jump", ent =>
            {
                if (player.IsAlive && IsReady(player) && player.GetField<bool>("jump") == true)
                {
                    player.SetField("jump", false);
                    AfterDelay(1500, () => { player.SetField("fuel", 2); player.SetField("jump", true); });
                }

                if (player.IsAlive && IsReady(player) && player.GetField<int>("fuel") > 0)
                {
                    player.SetField("fuel", player.GetField<int>("fuel") - 1);
                    var vel = GSCFunctions.GetVelocity(player);
                    GSCFunctions.SetVelocity(player, new Vector3(vel.X, vel.Y, 400));
                    GSCFunctions.PlayFX(player, fx1, player.Origin + new Vector3(0, 0, -5));
                }
            });

            player.OnNotify("dash", ent =>
                {
                    if (player.IsAlive && IsReady(player) && player.GetField<bool>("readydash") == true)
                    {
                        player.SetField("readydash", false);

                        var vel = GSCFunctions.GetVelocity(player);
                        var newvel = GSCFunctions.AnglesToForward(player.GetPlayerAngles()) * 500;
                        var len = newvel.Length() / new Vector3(newvel.X, newvel.Y, 0).Length();

                        GSCFunctions.SetVelocity(player, new Vector3(newvel.X * len, newvel.Y * len, vel.Z));
                        GSCFunctions.PlayFX(player, fx2, player.Origin + new Vector3(0, 0, -5));

                        AfterDelay(1500, () => { player.SetField("readydash", true); });
                    }
                });

            player.OnNotify("weapon_fired", (self, weapon) =>
            {
                if (weapon.As<string>() == "uav_strike_marker_mp")
                {
                    Vector3 asd = GSCFunctions.AnglesToForward(GSCFunctions.GetPlayerAngles(player));
                    Vector3 dsa = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
                    GSCFunctions.MagicBullet("ims_projectile_mp", GSCFunctions.GetTagOrigin(player, "tag_weapon_left"), dsa, self);
                    GSCFunctions.MagicBullet("ims_projectile_mp", GSCFunctions.GetTagOrigin(player, "tag_weapon_left"), dsa + new Vector3(50, 50, 50), self);
                }

            });
        }
        private void InfiniteStock()
        {
            OnInterval(50, () =>
            {
                foreach (Entity player in BaseScript.Players)
                {
                    if (player.CurrentWeapon == "at4_mp")
                    {
                        GSCFunctions.SetWeaponAmmoClip(player, player.CurrentWeapon, 5);
                        GSCFunctions.SetWeaponAmmoStock(player, player.CurrentWeapon, 45);
                    }
                    else
                        GSCFunctions.SetWeaponAmmoStock(player, player.CurrentWeapon, 45);
                }
                return true;
            });
        }
        private bool IsReady(Entity player)
        {
            if (!GSCFunctions.IsOnGround(player) && !GSCFunctions.IsOnLadder(player) && !GSCFunctions.IsMantling(player))
                return true;
            else
                return false;
        }
        private void GiveAllPerks(Entity player)
        {
            player.SetPerk("specialty_longersprint", true, false);
            player.SetPerk("specialty_fastreload", true, false);
            player.SetPerk("specialty_scavenger", true, false);
            player.SetPerk("specialty_blindeye", true, false);
            player.SetPerk("specialty_paint", true, false);
            player.SetPerk("specialty_hardline", true, false);
            player.SetPerk("specialty_coldblooded", true, false);
            player.SetPerk("specialty_quickdraw", true, false);
            player.SetPerk("specialty_twoprimaries", true, false);
            player.SetPerk("specialty_assists", true, false);
            player.SetPerk("_specialty_blastshield", true, false);
            player.SetPerk("specialty_detectexplosive", true, false);
            player.SetPerk("specialty_autospot", true, false);
            player.SetPerk("specialty_bulletaccuracy", true, false);
            player.SetPerk("specialty_quieter", true, false);
            player.SetPerk("specialty_stalker", true, false);
        }
        private void DisableSelectClass(Entity player)
        {
            GSCFunctions.ClosePopUpMenu(player, "");
            GSCFunctions.CloseInGameMenu(player);
            player.Notify("menuresponse", "team_marinesopfor", "axis");
            player.OnNotify("joined_team", ent =>
            {
                AfterDelay(500, () => { ent.Notify("menuresponse", "changeclass", "class1"); });
            });
            player.OnNotify("menuresponse", (player2, menu, response) =>
            {
                if (menu.ToString().Equals("class") && response.ToString().Equals("changeclass_marines"))
                {
                    AfterDelay(100, () => { player.Notify("menuresponse", "changeclass", "back"); });
                }
            });
        }
        private void ServerWelcomeTittle(Entity player, string tittle, float[] rgb)
        {
            player.SetField("welcome", 0);
            player.SpawnedPlayer += new Action(() =>
            {
                if (player.GetField<int>("welcome") == 0)
                {
                    HudElem serverWelcome = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, 1f);
                    serverWelcome.SetPoint("TOPCENTER", "TOPCENTER", 0, 165);
                    serverWelcome.SetText(tittle);
                    serverWelcome.GlowColor = (new Vector3(rgb[0], rgb[1], rgb[2]));
                    serverWelcome.GlowAlpha = 1f;
                    serverWelcome.SetPulseFX(150, 4700, 700);
                    player.SetField("welcome", 1);

                    AfterDelay(5000, () => { serverWelcome.Destroy(); });
                }
            });
        }          
        private unsafe void ChangeSpeed(int value)
        {
            *(int*)new IntPtr(4677866) = value;
        }
        private unsafe void ChangeJump(float value)
        {
            *(float*)new IntPtr(7186184) = (float)value;
        }
    }
}
