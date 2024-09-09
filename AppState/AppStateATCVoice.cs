using System;
using System.Globalization;
using System.Speech.Synthesis;


namespace FSXVORSim.AppState
{
    internal class AppStateATCVoice
    {
        private SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        public void SpeakInstruction(CultureInfo culture)
        {
            var builder = new PromptBuilder();
            builder.StartVoice(culture);
            builder.AppendText("Prueba prueba 2 8 0");
            builder.EndVoice();

            synthesizer.Speak(builder);
        }
    }
}
