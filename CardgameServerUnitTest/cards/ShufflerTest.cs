using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardgameModel;
using CardgameModel.Truco;
using CardgameServer;

namespace CardgameServerUnitTest.cards
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void shuffleRandomIntSet()
        {
            Random random = new Random();
            List<int> ints = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                ints.Add(random.Next(50));
            }

            List<int> shuffled = new List<int>(ints);
            var shuffler = new Shuffler<int>();
            shuffler.Shuffle(ints);

            AssertSameElements(ints, shuffled);
            bool atLeastOneShuffled = false;
            for (int i = 0; i < ints.Count; i++)
            {
                if (ints[i].Equals(shuffled[i]))
                {
                    atLeastOneShuffled = true; break;
                }
            }
            Assert.IsTrue(atLeastOneShuffled);
        }

        [TestMethod]
        public void shuffleCards()
        {
            List<Card> cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                for (int i = 1; i <= 13; i++)
                {
                    cards.Add(new Card(suit, i));
                }
            }

            List<Card> shuffled = new List<Card>(cards);
            var shuffler = new Shuffler<Card>();
            shuffler.Shuffle(shuffled);

            Write(cards);
            Write(shuffled);

            AssertSameElements(cards, shuffled);
            bool atLeastOneShuffled = false;
            for (int i = 0; i < cards.Count; i++)
            {
                if (!cards[i].Equals(shuffled[i]))
                {
                    atLeastOneShuffled = true; break;
                }
            }
            Assert.IsTrue(atLeastOneShuffled);
        }

        private void AssertSameElements<T>(ICollection<T> actual, ICollection<T> expected)
        {
            Assert.AreEqual(actual.Count, expected.Count);
            var actualList = new List<T>(actual);
            var expectedList = new List<T>(expected);
            actualList.Sort();
            expectedList.Sort();
            for (int i = 0; i < actualList.Count; i++)
            {
                Assert.AreEqual(actualList[i], expectedList[i]);
            }
        }

        private static void Write<T>(ICollection<T> collection)
        {
            Console.WriteLine("{ " + string.Join(", ", collection) + " }");
        }

    }



}
