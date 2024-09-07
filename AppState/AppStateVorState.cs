﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSXVORSim.AppState
{
    enum AppStateVorStatePosition
    {
        INBOUND,
        OUTBOUND,
        CROSSING
    }

    internal class AppStateVorState
    {
        AppStateVorStatePosition Position { get; set; }

        int Radial { get; set; }

        public bool Equals(AppStateVorState state)
        {
            return state.Position == this.Position && state.Radial == this.Radial;
        }

        public static AppStateVorState FromHeadingAndRadial(int heading, int radial, int sensitivity = 5)
        {
            var oppositeHeading = (heading + 180) % 360;

            var headingRange = Enumerable
                .Range(heading - sensitivity, sensitivity * 2 + 1)
                .Select(x => x % 360);

            var oppositeHeadingRange = Enumerable
                .Range(oppositeHeading - sensitivity, sensitivity * 2 + 1)
                .Select(x => x % 360);

            if (headingRange.Contains(radial))
                return FromPositionAndRadial(AppStateVorStatePosition.OUTBOUND, radial);

            if (oppositeHeadingRange.Contains(radial))
                return FromPositionAndRadial(AppStateVorStatePosition.INBOUND, radial);

            return FromPositionAndRadial(AppStateVorStatePosition.CROSSING, radial);
        }

        public static AppStateVorState FromPositionAndRadial(AppStateVorStatePosition position, int radial)
        {
            return new AppStateVorState { Radial = radial, Position = position };
        }

        public override string ToString()
        {
            return $"{Position} {Radial}";
        }
    }
}
