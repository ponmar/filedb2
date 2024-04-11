using System.Speech.Synthesis;

namespace FileDBAvalonia.Model;

public interface ISpeeker
{
    void Speek(params string[] texts);
    void CancelSpeek();
}

#if OS_WINDOWS

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
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

#else

public class Speeker : ISpeeker
{
    public void Speek(params string[] texts)
    {
    }

    public void CancelSpeek()
    {
    }
}

#endif
