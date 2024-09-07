using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSXVORSim.AppState
{
    internal class AppStateExcercise
    {
        private static readonly int[] exercisesRadials = { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330 };

        private static readonly AppStateVorStatePosition[] exercisesStates = { AppStateVorStatePosition.INBOUND, AppStateVorStatePosition.OUTBOUND };

        private AppStateVorState currentExerciseVorState = AppStateVorState.FromPositionAndRadial(AppStateVorStatePosition.OUTBOUND, 0);

        public AppStateExcercise()
        {
            this.currentExerciseVorState = AppStateVorState.FromPositionAndRadial(
                exercisesStates[new Random().Next(0, exercisesStates.Length)],
                exercisesRadials[new Random().Next(0, exercisesRadials.Length)]
            );
        }

        public bool Equals(AppStateExcercise state)
        {
            return this.currentExerciseVorState.Equals(state.currentExerciseVorState);
        }

        public bool Equals(AppState state)
        {
            return this.currentExerciseVorState.Equals(state.VorState);
        }

        public bool Equals(AppStateVorState state)
        {
            return this.currentExerciseVorState.Equals(state);
        }

        public override string ToString()
        {
            return currentExerciseVorState.ToString();
        }
    }
}
