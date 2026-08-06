public class Playlist
{
    private readonly List<string> _songs = new();

    public int Count => _songs.Count;

    public void Add(string song) => _songs.Add(song);

    // Integer indexer — lets consumers write playlist[i] like an array,
    // but with our own bounds-checking instead of relying on List<T>'s.
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= _songs.Count)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range. Valid range: 0-{_songs.Count - 1}.");
            }
            return _songs[index];
        }
        set
        {
            if (index < 0 || index >= _songs.Count)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range. Valid range: 0-{_songs.Count - 1}.");
            }
            _songs[index] = value;
        }
    }

    // A second indexer, overloaded by parameter TYPE (string instead of
    // int) — indexers can be overloaded just like methods. This one looks
    // up a song's position by title instead of by numeric position.
    public int this[string title]
    {
        get
        {
            int index = _songs.FindIndex(s => string.Equals(s, title, StringComparison.OrdinalIgnoreCase));
            if (index == -1)
            {
                throw new KeyNotFoundException($"No song titled '{title}' was found.");
            }
            return index;
        }
    }
}
