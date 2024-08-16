namespace CardgameServer.cards
{
    public class Shuffler<T> : IShuffler<T>
    {
        private Random random = new Random(System.DateTime.Now.Millisecond);

        public void Shuffle(List<T> list)
        {
            lock (list)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    int shufflePosition = random.Next(i + 1);
                    if (shufflePosition != i)
                    {
                        (list[shufflePosition], list[i]) = (list[i], list[shufflePosition]);
                    }
                }
            }
        }
    }
}
