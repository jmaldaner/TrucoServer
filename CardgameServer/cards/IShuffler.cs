namespace CardgameServer.cards
{
    public interface IShuffler<T>
    {
        void Shuffle(List<T> list);
    }
}
