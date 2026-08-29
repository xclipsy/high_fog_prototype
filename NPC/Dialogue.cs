namespace HighFog;

public enum Personality
{
    Kind,
    Aggressive,
    Nervous,
    Mysterious,
    Sad,
    Suspicious
}

public sealed class DialogueLine
{
    public string Speaker { get; set; }
    public string Text { get; set; }
    public Action<HighFogGame>? OnCompleted { get; set; }

    public DialogueLine(string speaker, string text, Action<HighFogGame>? onCompleted = null)
    {
        Speaker = speaker;
        Text = text;
        OnCompleted = onCompleted;
    }
}

public sealed class DialogueSequence
{
    private readonly List<DialogueLine> _lines = new();
    private int _currentIndex;

    public IReadOnlyList<DialogueLine> Lines => _lines;
    public bool IsActive { get; private set; }
    public DialogueLine? CurrentLine => (IsActive && _currentIndex < _lines.Count) ? _lines[_currentIndex] : null;

    public void Start(IEnumerable<DialogueLine> lines)
    {
        _lines.Clear();
        _lines.AddRange(lines);
        _currentIndex = 0;
        IsActive = _lines.Count > 0;
    }

    public bool Advance(HighFogGame game)
    {
        if (!IsActive) return false;

        CurrentLine?.OnCompleted?.Invoke(game);

        _currentIndex++;
        if (_currentIndex >= _lines.Count)
        {
            IsActive = false;
            return false;
        }

        return true;
    }

    public void Close()
    {
        IsActive = false;
        _lines.Clear();
    }
}
