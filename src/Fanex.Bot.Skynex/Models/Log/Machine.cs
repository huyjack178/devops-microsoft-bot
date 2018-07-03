﻿namespace Fanex.Bot.Skynex.Models.Log
{
    public class Machine
    {
        public int MachineId { get; set; }

        public string MachineName { get; set; }

        public bool IsProduction { get; set; }

        public short TimeZone { get; set; }

        public string MachineIP { get; set; }
    }
}