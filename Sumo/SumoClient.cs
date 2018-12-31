using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace Sumo
{
    public static class SumoSound
    {
        public enum sound { SUMO_COUNTDOWN_ONE, SUMO_ROUND_START, SUMO_ROUND_END, PLAYER_DIED, SHARD_ROUND_OVER, SHARD_FADE_OUT, SHARD_RESULT, };
        public static void Play(sound s)
        {
            switch (s)
            {
                case sound.SUMO_COUNTDOWN_ONE:
                    PlaySoundFrontend(-1, "MP_AWARD", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    break;

                case sound.SUMO_ROUND_START:
                    PlaySoundFrontend(-1, "Round_Start", "DLC_LOW2_Sumo_Soundset", true);
                    break;

                case sound.PLAYER_DIED:
                    PlaySoundFrontend(-1, "Vehicle_Destroyed", "DLC_LOW2_Sumo_Soundset", false);
                    break;

                case sound.SHARD_ROUND_OVER:
                    PlaySoundFrontend(-1, "MP_WAVE_COMPLETE", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    break;

                case sound.SHARD_FADE_OUT:
                    PlaySoundFrontend(-1, "ARM_WRESTLING_WHOOSH_MASTER", "", false);
                    break;

                case sound.SHARD_RESULT:
                    PlaySoundFrontend(-1, "GO", "HUD_MINI_GAME_SOUNDSET", false);
                    break;

                case sound.SUMO_ROUND_END:
                    PlaySoundFrontend(-1, "Round_End", "DLC_LOW2_Sumo_Soundset", false);
                    break;

                default:
                    break;
            }
        }
    }


    public class SumoClient : BaseScript
    {
        private bool firstTick = true;
        public enum GamePhase { WAITING, READY, STARTING, STARTED, RESET, DEAD };
        public static GamePhase currentPhase = GamePhase.WAITING;
        public int currentRound = 0;
        public int timeMin = 3;
        public int timeSec = 0;

        public SumoClient()
        {
            //EventHandlers.Add("onClientMapLoad", new Action(FirstJoin));
            Tick += OnTick;
            Tick += ManageBlips;
            Tick += GeneralSetup;
            Tick += RunSpectate;
            Tick += ManageDeathPhase;
            RequestScriptAudioBank("DLC_STUNT/STUNT_RACE_01", false);
            RequestScriptAudioBank("DLC_STUNT/STUNT_RACE_02", false);
            RequestScriptAudioBank("DLC_STUNT/STUNT_RACE_03", false);
            RequestScriptAudioBank("DLC_LOW2/SUMO_01", false);
            EventHandlers.Add("Sumo:Welcome", new Action<int, int, int, int>(FirstJoinSetup));
            EventHandlers.Add("Sumo:PlayerDied", new Action<string>(PlayerDied));
        }

        private async Task ManageDeathPhase()
        {
            if (currentPhase == GamePhase.DEAD)
            {
                SetEntityVisible(PlayerPedId(), false, false);
                SetEntityInvincible(PlayerPedId(), true);
                FreezeEntityPosition(PlayerPedId(), true);
            }
            else if (!Gfx.LimboActive)
            {
                SetEntityInvincible(PlayerPedId(), false);
                SetEntityVisible(PlayerPedId(), true, true);
                if (currentPhase == GamePhase.STARTED)
                {
                    FreezeEntityPosition(PlayerPedId(), false);
                    FreezeEntityPosition(GetVehiclePedIsIn(PlayerPedId(), false), false);
                }
            }
        }

        private async void PlayerDied(string playerName)
        {
            SumoSound.Play(SumoSound.sound.PLAYER_DIED);
            await Delay(3000);
            var allout = true;
            var playersAlive = NetworkGetNumConnectedPlayers();
            foreach (Player p in new PlayerList())
            {
                if (!(p.IsDead || !IsPedInAnyVehicle(GetPlayerPed(p.Handle), false) || !IsEntityVisible(GetPlayerPed(p.Handle))))
                {

                    allout = false;
                }
                else
                {
                    playersAlive--;
                }
            }
            if (allout || playersAlive < 2)
            {
                currentPhase = GamePhase.RESET;
            }
        }

        private void FirstJoin()
        {
            TriggerServerEvent("Sumo:Hello");
            //Exports["spawnmanager"].setAutoSpawn(false);
            SpawnPlayer(false);
        }

        private async void FirstJoinSetup(int phase, int round, int timeM, int timeS)
        {
            currentPhase = (GamePhase)phase;
            currentRound = round;
            timeMin = timeM;
            timeSec = timeS;

            if ((currentPhase == GamePhase.WAITING || currentPhase == GamePhase.RESET))
            {
                SpawnPlayer(true);
            }
            else
            {
                SetEntityHealth(PlayerPedId(), 200);
                SetEntityCoordsNoOffset(PlayerPedId(), 0f, 0f, 500f, false, false, false);
                //ResurrectPed(PlayerPedId());
                NetworkResurrectLocalPlayer(0f, 0f, 500f, 0f, true, false);
                ClearPedTasksImmediately(PlayerPedId());
                SetEntityHealth(PlayerPedId(), 300);
                RemoveAllPedWeapons(PlayerPedId(), true);
                ClearPlayerWantedLevel(PlayerId());
                SetEntityCoordsNoOffset(PlayerPedId(), 0f, 0f, 500f, false, false, false);
                //SetEntityHealth(PlayerPedId(), 200);
                FreezeEntityPosition(PlayerPedId(), true);
                ShutdownLoadingScreen();
                await Delay(1000);
                DoScreenFadeIn(1000);
                EnableAllControlActions(0);
                //RunSpectate();
            }
        }

        private async Task RunSpectate()
        {
            if (currentPhase == GamePhase.DEAD || ((currentPhase == GamePhase.STARTED || currentPhase == GamePhase.STARTING) && (!IsEntityVisible(PlayerPedId()))))
            {
                foreach (Player p in new PlayerList())
                {
                    var ped = GetPlayerPed(p.Handle);
                    if (!IsPedInAnyVehicle(PlayerPedId(), false) && (IsPedInAnyVehicle(ped, false) && IsEntityVisible(ped) && !IsPlayerDead(p.Handle)))
                    {
                        NetworkSetInSpectatorMode(true, ped);
                    }
                    else
                    {
                        NetworkSetInSpectatorMode(false, PlayerPedId());
                    }

                }
            }
            else
            {
                NetworkSetInSpectatorMode(false, PlayerPedId());
            }
        }

        private async Task GeneralSetup()
        {
            Gfx.DrawStatBar("TIME", "00:00", 0);
            var i = 1;
            foreach (Player p in new PlayerList())
            {
                if (IsEntityVisible(GetPlayerPed(p.Handle)))
                {
                    Gfx.DrawStatBar($"~HUD_COLOUR_NET_PLAYER{p.Handle + 1}~{p.Name}", "(player)", i);
                    i++;
                }
            }

            //ClearPedTasksImmediately(PlayerPedId());
            //SetEntityHealth(PlayerPedId(), 300);
            RemoveAllPedWeapons(PlayerPedId(), true);
            ClearPlayerWantedLevel(PlayerId());
            SetParkedVehicleDensityMultiplierThisFrame(0f);
            SetPedDensityMultiplierThisFrame(0f);
            SetRandomVehicleDensityMultiplierThisFrame(0f);
            SetScenarioPedDensityMultiplierThisFrame(0f, 0f);
            SetSomeVehicleDensityMultiplierThisFrame(0f);
            SetVehicleDensityMultiplierThisFrame(0f);
        }

        private async Task ManageBlips()
        {
            foreach (Player p in new PlayerList())
            {
                if (p.Handle != PlayerId())
                {
                    var ped = GetPlayerPed(p.Handle);
                    if (DoesEntityExist(ped))
                    {
                        var blip = GetBlipFromEntity(ped);
                        if (!DoesBlipExist(blip))
                        {
                            blip = AddBlipForEntity(ped);
                        }
                        //if (p.Handle == PlayerId())
                        //{
                        //SetBlipSprite(blip, 425);
                        //SetBlipScale(blip, 0.8f);
                        //ShowHeadingIndicatorOnBlip(blip, false);
                        //SetBlipRotation(blip, (int)GetEntityHeading(ped));
                        //}
                        //else
                        //{
                        SetBlipSprite(blip, 1);
                        ShowHeadingIndicatorOnBlip(blip, true);
                        //}

                        SetBlipColour(blip, p.Handle + 6);
                        if (IsEntityVisible(ped))
                        {
                            SetBlipDisplay(blip, 8);
                        }
                        else
                        {
                            SetBlipDisplay(blip, 4);
                        }
                    }
                }
            }
        }

        private async Task OnTick()
        {
            if (firstTick)
            {
                //Exports["spawnmanager"].setAutoSpawn(false);
                firstTick = false;
                FirstJoin();
            }

            if (IsEntityDead(PlayerPedId()) && currentPhase == GamePhase.STARTED)
            {
                Gfx.ShowShard(Gfx.MessageType.KNOCKED_OUT, "~r~KNOCKED OUT", "", 0, 0, 3000);
                DoScreenFadeOut(3000);
                await Delay(3000);
                currentPhase = GamePhase.DEAD;
                SpawnPlayer();
                await Delay(2000);
                DoScreenFadeIn(3000);
            }
            else
            {
                if (IsEntityInWater(PlayerPedId()) || IsEntityInWater(GetVehiclePedIsIn(PlayerPedId(), false)))
                {
                    NetworkExplodeVehicle(GetVehiclePedIsIn(PlayerPedId(), false), true, false, false);
                    SetEntityHealth(PlayerPedId(), 0);
                    TriggerServerEvent("Sumo:ImOut");
                }
            }
            if (Gfx.LimboActive && !IsPauseMenuActive() && !IsPauseMenuRestarting())
            {
                //DisableAllControlActions(0);
                //Game.DisableAllControlsThisFrame(0);
                //Game.EnableControlThisFrame(0, Control.MpTextChatAll);
                //Game.EnableControlThisFrame(0, Control.FrontendPause);
                //Game.EnableControlThisFrame(0, Control.FrontendPauseAlternate);
            }
            if (currentPhase == GamePhase.RESET)
            {
                SpawnPlayer(true);
                while (currentPhase == GamePhase.RESET && !IsPedInAnyVehicle(PlayerPedId(), false))
                {
                    await Delay(0);
                }
            }
            //else
            //{
            //    Game.EnableAllControlsThisFrame(0);
            //    if (Game.IsDisabledControlJustPressed(0, Control.FrontendPause) ||
            //        Game.IsDisabledControlJustPressed(0, Control.FrontendPauseAlternate) ||
            //        Game.IsControlJustPressed(0, Control.FrontendPause) ||
            //        Game.IsControlJustPressed(0, Control.FrontendPauseAlternate))
            //    {
            //        SetPauseMenuActive(true);
            //    }
            //}
        }

        private void FadeScreenIn()
        {
            if (IsScreenFadingOut() || IsScreenFadedOut())
                DoScreenFadeIn(250);
        }

        private async void SpawnPlayer(bool inVehicle = false)
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                var veh = GetVehiclePedIsIn(PlayerPedId(), true);
                SetEntityAsMissionEntity(veh, true, true);
                DeleteVehicle(ref veh);
                var pos = Game.PlayerPed.Position;
                ClearAreaOfEverything(pos.X, pos.Y, pos.Z, 500f, false, false, false, false);
            }
            //await Delay(500);
            //dynamic spawns = new Spawns().GetSpawns();
            //var spawnPoint = spawns[PlayerId()];

            uint model = (uint)GetHashKey("csb_mweather");
            RequestModel(model);
            if (IsModelInCdimage(model))
            {
                while (!HasModelLoaded(model))
                {
                    await Delay(0);
                }
                SetPlayerModel(PlayerId(), model);
                SetModelAsNoLongerNeeded(model);
                SetPedDefaultComponentVariation(PlayerPedId());
            }

            //if (spawns.Count() > PlayerId())
            //{
            //    var spawn = spawns[PlayerId()];
            //    Vector3 spawnCoords = new Vector3(float.Parse(spawn["x"].ToString()), float.Parse(spawn["y"].ToString()), float.Parse(spawn["z"].ToString()));
            //    float spawnHeading = float.Parse(spawn["heading"].ToString());

            //    NetworkResurrectLocalPlayer(0f, 0f, 0f, 0f, false, false);
            //}
            //else
            //{
            //}


            //NetworkResurrectLocalPlayer(spawnCoords.X, spawnCoords.Y, spawnCoords.Z, spawnHeading, false, false);

            //Newtonsoft.Json.Linq.JArray spawns = JsonConvert.DeserializeObject(Exports["spawnmanager"].getSpawns());
            //if (spawns.Count() > 0)
            //{
            //    Debug.Write($"{spawns.ToString()}\n");
            //    var spawn = spawns[0];
            //    Vector3 spawnCoords = new Vector3(float.Parse(spawn["x"].ToString()), float.Parse(spawn["y"].ToString()), float.Parse(spawn["z"].ToString()));
            //    float spawnHeading = float.Parse(spawn["heading"].ToString());
            //    NetworkResurrectLocalPlayer(spawnCoords.X, spawnCoords.Y, spawnCoords.Z, spawnHeading, false, false);
            //}
            //else
            //{
            //    Exports["spawnmanager"].spawnPlayer(1);
            //    //NetworkResurrectLocalPlayer(0f, 0f, 0f, 0f, false, false);
            //}
            //SetEntityHealth(PlayerPedId(), 0);

            SetEntityHealth(PlayerPedId(), 200);
            SetEntityCoordsNoOffset(PlayerPedId(), 0f, 0f, 500f, false, false, false);
            //ResurrectPed(PlayerPedId());
            NetworkResurrectLocalPlayer(0f, 0f, 500f, 0f, true, false);
            ClearPedTasksImmediately(PlayerPedId());
            SetEntityHealth(PlayerPedId(), 300);
            RemoveAllPedWeapons(PlayerPedId(), true);
            ClearPlayerWantedLevel(PlayerId());
            SetEntityCoordsNoOffset(PlayerPedId(), 0f, 0f, 500f, false, false, false);
            //SetEntityHealth(PlayerPedId(), 200);
            FreezeEntityPosition(PlayerPedId(), true);
            ShutdownLoadingScreen();
            SetEntityVisible(PlayerPedId(), false, false);
            //if (currentPhase == GamePhase.WAITING || currentPhase == GamePhase.RESET)
            if (inVehicle)
            {
                Gfx.ToggleLimbo();
            }
            await Delay(1000);
            DoScreenFadeIn(1000);
            EnableAllControlActions(0);
        }
    }
}
