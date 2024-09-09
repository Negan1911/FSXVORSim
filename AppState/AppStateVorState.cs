using FSXVORSim.Resources;
using System;
using System.Linq;

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
        public AppStateVorStatePosition Position { get; private set; }

        public int Radial { get; private set; }

        public bool Equals(AppStateVorState state)
        {
            return state.Position == Position && state.Radial == Radial;
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

        public override string ToString() => Position switch
        {
            AppStateVorStatePosition.INBOUND => String.Format(Strings.VorStateInboundStr, Radial),
            AppStateVorStatePosition.OUTBOUND => String.Format(Strings.VorStateOutboundStr, Radial),
            AppStateVorStatePosition.CROSSING => String.Format(Strings.VorStateCrossingStr, Radial),
            _ => throw new ArgumentOutOfRangeException("AppStateVorStatePosition Invalid value")
        };
    }
}
