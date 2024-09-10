using System;
using System.Globalization;
using System.Speech.Synthesis;


namespace FSXVORSim.AppState
{
    internal class AppStateATCVoice
    {
        private readonly SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        public void SpeakInstruction(CultureInfo culture, AppStateExcercise excercise)
        {
            var builder = new PromptBuilder();
            builder.StartVoice(culture);
            builder.AppendText(excercise.ToExerciseString(true));
            builder.EndVoice();

            synthesizer.SpeakAsync(builder);
        }
    }
}
