using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Sumo
{
    public class Spawns : BaseScript
    {
        public object GetSpawns()
        {
            return Exports["spawnmanager"].getSpawns();
        }
    }

    class Utility : BaseScript
    {

        public static async Task TransitionToCoords(Vector4 targetPosition, uint vehicleModel = 0)
        {
            RequestCollisionAtCoord(targetPosition.X, targetPosition.Y, targetPosition.Z);
            var tempPedHash = (uint)GetHashKey("S_M_Y_Clown_01"); // Thanks, you're a genius Mraes!
            RequestModel(tempPedHash);
            while (!HasModelLoaded(tempPedHash))
            {
                await Delay(0);
            }
            var tempPed = CreatePed(4, tempPedHash, targetPosition.X, targetPosition.Y, targetPosition.Z, targetPosition.W, false, false);
            SetEntityVisible(tempPed, false, false);
            // Teleport into a new vehicle.
            if (vehicleModel != 0)
            {
                if (IsModelInCdimage(vehicleModel))
                {
                    RequestModel(vehicleModel);
                    while (!HasModelLoaded(vehicleModel))
                    {
                        await Delay(0);
                    }
                }
            }
            //StartPlayerSwitch(PlayerPedId(), tempPed, 0, 1);
            StartPlayerSwitch(PlayerPedId(), tempPed, 0, 0);
            await Delay(10);
            DeleteEntity(ref tempPed);
            SetModelAsNoLongerNeeded(tempPedHash);
            var veh = 0;
            if (vehicleModel != 0)
            {
                veh = CreateVehicle(vehicleModel, targetPosition.X, targetPosition.Y, targetPosition.Z, targetPosition.W, true, false);
                SetVehicleHasBeenOwnedByPlayer(veh, true);
                SetModelAsNoLongerNeeded(vehicleModel);
                SetVehicleNeedsToBeHotwired(veh, false);
                var r = 0;
                var g = 0;
                var b = 0;
                var a = 0;
                GetHudColour((PlayerId() < 32 ? PlayerId() : PlayerId() % 30) + 28, ref r, ref g, ref b, ref a);
                SetVehicleCustomPrimaryColour(veh, r, g, b);
                SetVehicleCustomSecondaryColour(veh, r, g, b);
            }
            if (Gfx.LimboActive)
            {
                Gfx.ToggleLimbo();
            }
            while (GetPlayerSwitchState() != 10 && GetPlayerSwitchState() != 8)
            {
                DisplayRadar(false);
                await Delay(0);
            }
            SetEntityCoords(PlayerPedId(), targetPosition.X, targetPosition.Y, targetPosition.Z, false, false, false, false);
            SetEntityHeading(PlayerPedId(), targetPosition.W);

            if (vehicleModel != 0 && DoesEntityExist(veh) && IsEntityAVehicle(veh) && !IsEntityDead(veh))
            {
                SetPedIntoVehicle(PlayerPedId(), veh, -1);
            }

            SetGameplayCamRelativeHeading(0f);

            ClearAreaOfEverything(targetPosition.X, targetPosition.Y, targetPosition.Z, 500f, false, false, false, false);
            while (IsPlayerSwitchInProgress())
            {
                DisplayRadar(false);
                Game.DisableAllControlsThisFrame(0);
                await Delay(0);
            }

            Game.EnableAllControlsThisFrame(0);
            DisplayRadar(IsRadarPreferenceSwitchedOn());
            DisplayHud(IsHudPreferenceSwitchedOn());

            var blip = GetMainPlayerBlipId();
            SetBlipAsFriendly(blip, false);
            SetBlipColour(blip, 6 + PlayerId());
            SetBlipHighDetail(blip, true);
            SetBlipFriend(blip, false);
            SetBlipBright(blip, true);
            PulseBlip(blip);
            ShowHeadingIndicatorOnBlip(blip, false);
            Debug.Write(DoesBlipExist(blip) + "\n");

            SumoClient.currentPhase = SumoClient.GamePhase.WAITING;
        }
    }
}
