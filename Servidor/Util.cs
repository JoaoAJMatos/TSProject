using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Servidor
{
    public class Util
    {
        /// Calculates the Levenshtein distance between two strings.
        /// Used for fuzzy string matching in the command handler.
        ///
        /// This is a modified implementation of the pseudocode found on Wikipedia:
        /// https://en.wikipedia.org/wiki/Levenshtein_distance#Iterative_with_two_matrix_rows
        ///
        /// Maybe there's a better algorithm with better space complexity?
        static public int LevenshteinDistance(string str1, string str2)
        {
            int[,] d = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                d[0, j] = j;

            for (int j = 1; j <= str2.Length; j++)
            {
                for (int i = 1; i <= str1.Length; i++)
                {
                    if (str1[i - 1] == str2[j - 1])
                        d[i, j] = d[i - 1, j - 1];  // No operation
                    else
                        d[i, j] = Math.Min(Math.Min(
                              d[i - 1, j] + 1,      // A deletion
                              d[i, j - 1] + 1),     // An insertion
                              d[i - 1, j - 1] + 1   // A substitution
                        );
                }
            }

            return d[str1.Length, str2.Length];
        }

        static public void DisplayLicenseNotice()
        {
            Console.WriteLine("\n                              MIT LICENSE\n");
            Console.WriteLine("          Copyright 2023 Joao Matos, Joao Fernandes, Ruben Lisboa\n");
            Console.WriteLine(" Permission is hereby granted, free of charge, to any person obtaining a copy");
            Console.WriteLine(" of this software and associated documentation files (the \"Software\"), to deal");
            Console.WriteLine(" in the Software without restriction, including without limitation the rights");
            Console.WriteLine(" to use, copy, modify, merge, publish, distribute, sublicense, and/or sell");
            Console.WriteLine(" copies of the Software, and to permit persons to whom the Software is");
            Console.WriteLine(" furnished to do so, subject to the following conditions:\n");
            Console.WriteLine(" The above notice and this permission notice shall be included in all copies or");
            Console.WriteLine(" substantial portions of the Software.\n");
            Console.WriteLine(" THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR");
            Console.WriteLine(" IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,");
            Console.WriteLine(" FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE");
            Console.WriteLine(" AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER");
            Console.WriteLine(" LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,");
            Console.WriteLine(" OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE");
            Console.WriteLine(" SOFTWARE.\n");
        }
    }
}
