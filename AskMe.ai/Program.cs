using AskMe.ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFIDFExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Some example documents.
            //string[] documents =
            //{
            //    //"The sun in the Stars sky is bright.",
            //    //"We can see the shining sun, the bright sun."
            //    "How much is my balance",
            //    "I need my balance",
            //    "balance for account",
            //    "payment for my account",
            //    "hello need my balance"
            //};

            //// Apply TF*IDF to the documents and get the resulting vectors.
            //double[][] inputs = TFIDF.Transform(documents, 0);
            //inputs = TFIDF.Normalize(inputs);

            //double[] result = inputs[inputs.Length - 1];

            //for (int index = 0; index < inputs.Length-1; index++)
            //{
            //    Console.WriteLine(documents[index]);
            //    int counter = 0;
            //    double cosineValue = 0;
            //    foreach (double value in inputs[index])
            //    {
            //        cosineValue = cosineValue + (value * result[counter]);
            //        counter++;
            //    }
            //    Console.WriteLine(cosineValue);
            //    Console.WriteLine("\n");
            //}

            //Console.WriteLine("------------------------------");

            //for (int index = 0; index < inputs.Length; index++)
            //{
            //    Console.WriteLine(documents[index]);

            //    foreach (double value in inputs[index])
            //    {
            //        Console.Write(value + ", ");
            //    }

            //    Console.WriteLine("\n");
            //}

            Console.WriteLine("Levinstine Algorithm");
            Console.WriteLine("----------------------------------");

            //Console.WriteLine(LevenshteinDistance.Compute("Payments for Account", "Pyment for Account"));
            //Console.WriteLine(LevenshteinDistance.Compute("Payments for Account", "Balance for Account"));

            Console.WriteLine("TF IDF Cosine Similarity");
            Console.WriteLine("----------------------------------");
            int threshold = 0;

            try
            {
                // Create instance of calculator
                SimilarityCalculator sc = new SimilarityCalculator();

                sc.CompareString("Payments for Account", "Payment Balance for Account", vocabularyThreshold: threshold);
                sc.CompareString("Payments for Account", "Balance for Account", vocabularyThreshold: threshold);

                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();
        }
    }
}
