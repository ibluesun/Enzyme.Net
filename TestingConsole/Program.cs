using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enzyme;
using System.IO;

namespace TestingConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            IEnzymeContext dfp = new TestingContext();

            var rv = new EnzymeEngine(dfp);

            StreamReader sr = new StreamReader("test.txt");

            while(!sr.EndOfStream)
            {
                string pp = sr.ReadLine().Trim();
                if (!string.IsNullOrEmpty(pp))
                    Console.WriteLine(rv.Evaluate(pp));
            }
        }

        public static void perf()
        {
            EnzymeDynamicContext dfp = new EnzymeDynamicContext();

            var rv = new EnzymeEngine(dfp);

            rv.Evaluate("$total = 0");

            double total = 0.0;

            double d = Environment.TickCount;

            for (int i = 0; i < 1000; i++)
            {
                total = total + 1;
                Console.WriteLine(total);
            }
            d = Environment.TickCount - d;

            double e = Environment.TickCount;
            for (int i = 0; i < 1000; i++)
            {
                rv.Evaluate("$total=$total+1");
                Console.WriteLine(dfp.GetFieldValue("total"));
            }
            e = Environment.TickCount - e;

            double pf = ((e - d) / e) * 100.0;
            Console.WriteLine("Performance loss: {0}", pf);
            Console.ReadLine();

        }
    }
}
