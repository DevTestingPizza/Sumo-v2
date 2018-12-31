using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace SumoServer
{
    public class SumoServer : BaseScript
    {
        public enum GamePhase { WAITING, READY, STARTING, STARTED, RESET, DEAD };
        public GamePhase currentPhase = GamePhase.WAITING;
        public int round = 0;
        public int timeMin = 3;
        public int timeSec = 0;

        public SumoServer()
        {
            EventHandlers.Add("Sumo:Hello", new Action<Player>(PlayerJoined));
            EventHandlers.Add("Sumo:ImOut", new Action<Player>(PlayerDied));
            EventHandlers.Add("playerDropped", new Action<Player>(PlayerDied));
        }

        private void PlayerJoined([FromSource] Player source)
        {
            source.TriggerEvent("Sumo:Welcome", (int)currentPhase, round, timeMin, timeSec);
        }
        private void PlayerDied([FromSource] Player source)
        {
            TriggerClientEvent("Sumo:PlayerDied", source.Name);
        }
    }
}
