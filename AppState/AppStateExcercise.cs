using FSXVORSim.Resources;
using System;
using System.Linq;

namespace FSXVORSim.AppState
{
    internal class AppStateExcercise
    {
        private static readonly int[] exercisesRadials = { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330 };

        private static readonly AppStateVorStatePosition[] exercisesStates = { AppStateVorStatePosition.INBOUND, AppStateVorStatePosition.OUTBOUND };

        private readonly AppStateVorState currentExerciseVorState = AppStateVorState.FromPositionAndRadial(AppStateVorStatePosition.OUTBOUND, 0);

        public AppStateExcercise()
        {
            currentExerciseVorState = AppStateVorState.FromPositionAndRadial(
                exercisesStates[new Random().Next(0, exercisesStates.Length)],
                exercisesRadials[new Random().Next(0, exercisesRadials.Length)]
            );
        }

        public bool Equals(AppStateExcercise state)
        {
            return currentExerciseVorState.Equals(state.currentExerciseVorState);
        }

        public bool Equals(AppState state)
        {
            return currentExerciseVorState.Equals(state.VorState);
        }

        public bool Equals(AppStateVorState state)
        {
            return currentExerciseVorState.Equals(state);
        }

        public override string ToString()
        {
            return currentExerciseVorState.ToString();
        }

        public string ToExerciseString(bool voice = false)
        {
            String radialTxt = voice ? String.Join(" ", currentExerciseVorState.Radial.ToString().ToCharArray()) : currentExerciseVorState.Radial.ToString();
            return currentExerciseVorState.Position switch
            {
                AppStateVorStatePosition.INBOUND => String.Format(Strings.ExerciseInboundStr, radialTxt),
                AppStateVorStatePosition.OUTBOUND => String.Format(Strings.ExerciseOutboundStr, radialTxt),
                AppStateVorStatePosition.CROSSING => throw new ArgumentOutOfRangeException("AppStateVorStatePosition Invalid crossing value"),
                _ => throw new ArgumentOutOfRangeException("AppStateVorStatePosition Invalid value")
            };
        }
    }
}
 