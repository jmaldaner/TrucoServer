namespace CardgameServer
{
    public interface IShuffler<T>
    {
        void Shuffle(List<T> list);
    }
}
