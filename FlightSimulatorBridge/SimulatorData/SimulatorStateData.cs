using System;

namespace FSXVORSim.SimulatorData
{
    enum SimulatorStateDataStatus
    {
        STOPPED,
        RUNNING,
        ERRORED
    }

    internal struct SimulatorStateData
    {
        public SimulatorStateDataStatus Status { get; set; }

        public String Error { get; set; }
    }
}
