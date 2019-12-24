using System;
using SpamFilter;

namespace DesktopTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var filter = new Filter();
            var domains = new []{ "www.microsoft.com" };

            //var result = filter.IsSpam("spam spam http://bit.ly/2ZhSxkh", domains, 1);
            var result2 = filter.IsSpam("spam spam http://google.com", domains, 1);
            //var result2 = filter.IsSpam("spam spam shorturl.at/cePZ5 ", domains, 2);

            Console.WriteLine("This is a test spam tool");
            //Console.WriteLine("Result 1" + result);
            Console.WriteLine("Result 2" + result2);

        }
    }
}
