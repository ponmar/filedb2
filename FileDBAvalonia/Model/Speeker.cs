using System.Speech.Synthesis;

namespace FileDBAvalonia.Model;

public interface ISpeeker
{
    void Speek(params string[] texts);
    void CancelSpeek();
}

public class Speeker : ISpeeker
{
    private readonly SpeechSynthesizer synth = new();

    public void Speek(params string[] texts)
    {
        CancelSpeek();
        foreach (var text in texts)
        {
            synth.SpeakAsync(text);
        }
    }

    public void CancelSpeek()
    {
        synth.SpeakAsyncCancelAll();
    }
}
