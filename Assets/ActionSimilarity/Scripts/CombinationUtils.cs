using System.Collections.Generic;
using System.Linq;
using UXF;

namespace ActionSimilarity
{
    public class CombinationUtils
    {
        public static List<List<int>> WithinBlockShuffling(int numberOfItems, int repeats)
        {
            List<int> values = Enumerable.Range(0, numberOfItems).ToList();
            List<List<int>> product = CartesianProduct(values, values.Count);
            
            List<List<int>> repeated = Enumerable
                .Repeat(product, repeats) 
                .SelectMany(x => x)      
                .ToList();  
            
            return repeated;
        }

        public static List<List<T>> CartesianProduct<T>(List<T> list, int length)
        {
            IEnumerable<List<T>> result = new List<List<T>> { new List<T>() };

            for (int i = 0; i < length; i++)
            {
                result = result.SelectMany(seq => list, (seq, val) => new List<T>(seq) { val });
            }

            List<List<T>> product = result.ToList();
            return product;
        }
        
        public static List<List<int>> GetNPermutations(int numberOfItems, int n)
        {
            
            var result = new List<List<int>>();
            Permute(Enumerable.Range(0, numberOfItems).ToList(), 0, result);

            int repeats = n / result.Count;
            List<List<int>> repeated = Enumerable
                .Repeat(result, repeats) 
                .SelectMany(x => x)      
                .ToList();  
            return repeated;
        }

        private static void Permute<T>(List<T> list, int start, List<List<T>> result)
        {
            if (start == list.Count - 1)
            {
                result.Add(new List<T>(list));
                return;
            }

            for (int i = start; i < list.Count; i++)
            {
                (list[start], list[i]) = (list[i], list[start]);  // swap
                Permute(list, start + 1, result);
                (list[start], list[i]) = (list[i], list[start]);  // backtrack
            }
        }
    }
}