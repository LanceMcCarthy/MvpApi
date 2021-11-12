using System;
using System.Collections.Generic;
using System.Linq;

namespace MvpCompanion.UI.WinUI.Models
{
    public static class ProductsGenerator
    {
        static string[] names = new string[]
        {
            "Côte de Blaye",
            "Boston Crab Meat",
            "Singaporean Hokkien Fried Mee",
            "Gula Malacca", "Rogede sild",
            "Spegesild",
            "Zaanse koeken",
            "Chocolade",
            "Maxilaku",
            "Valkoinen suklaa"
        };

        static double[] prices = new double[] { 23.2500, 9.0000, 45.6000, 32.0000, 14.0000, 19.0000, 26.5000, 18.4000, 13.0000, 14.0000 };

        static DateTime[] dates = new DateTime[]
        {
            new DateTime(DateTime.Now.Year, 5, 10),
            new DateTime(DateTime.Now.Year, 9, 13),
            new DateTime(DateTime.Now.Year, 2, 22),
            new DateTime(DateTime.Now.Year, 1, 2),
            new DateTime(DateTime.Now.Year, 4, 13),
            new DateTime(DateTime.Now.Year, 5, 12),
            new DateTime(DateTime.Now.Year, 1, 19),
            new DateTime(DateTime.Now.Year, 8, 26),
            new DateTime(DateTime.Now.Year, 7, 31),
            new DateTime(DateTime.Now.Year, 7, 16)
        };

        static bool[] bools = new bool[] { true, false, true, false, true, false, true, false, true, false };

        public static IEnumerable<Product> GetData(int maxItems)
        {
            Random rnd = new Random();

            return from i in Enumerable.Range(1, maxItems)
                   select new Product(i, names[rnd.Next(9)], prices[rnd.Next(9)],
                       dates[rnd.Next(9)], bools[rnd.Next(9)]);
        }

        public static Product GenerateRandomBusinessObject(Random random)
        {
            return new Product(random.Next(0, 100), names[random.Next(9)], prices[random.Next(9)], dates[random.Next(9)], bools[random.Next(9)]);
        }

        public static void SetRandomPropertyValues(Product obj, Random random)
        {
            obj.Name = names[random.Next(9)];
            obj.UnitPrice = prices[random.Next(9)];
            obj.Date = dates[random.Next(9)];
            obj.Discontinued = bools[random.Next(9)];
        }
    }
}
