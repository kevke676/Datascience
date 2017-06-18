using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datascience2
{
    class Program
    {
        public static Dictionary<string, Dictionary<string, Tuple<double, int>>> dataItems = new Dictionary<string, Dictionary<string, Tuple<double, int>>>();
        public static Dictionary<string, Dictionary<string, double>> dataUsers = new Dictionary<string, Dictionary<string, double>>();

        static void Main(string[] args)
        {
            int file = 2;
            if (file == 1)
            {
                processData("C:\\Users\\kevke6\\Desktop\\Datamining\\Datascience\\Datascience2\\Datascience2\\Datasource\\userItem.data");
                List<Tuple<string, string>> listUserItem = new List<Tuple<string, string>>();
                listUserItem.Add(new Tuple<string, string>("7", "101"));
                listUserItem.Add(new Tuple<string, string>("7", "103"));
                listUserItem.Add(new Tuple<string, string>("7", "106"));
                listUserItem.Add(new Tuple<string, string>("3", "103"));
                listUserItem.Add(new Tuple<string, string>("3", "105"));
                calculateRating(listUserItem);
                List<Tuple<string, string, double>> itemRatedList = new List<Tuple<string, string, double>>();
                itemRatedList.Add(new Tuple<string, string, double>("3", "105", 4.0));
                readjustDeviation(itemRatedList);
                listUserItem.Remove(new Tuple<string, string>("3", "103"));
                listUserItem.Remove(new Tuple<string, string>("3", "105"));
                calculateRating(listUserItem);
            }
            if (file == 2)
            {
                processData("C:\\Users\\kevke6\\Desktop\\Datamining\\Datascience\\Datascience2\\Datascience2\\Datasource\\ratings.csv");
                Dictionary<string, double> person = dataUsers.Single(p => p.Key.Equals("186")).Value;
                List<string> listItems = dataUsers.SelectMany(p => p.Value.Keys).Distinct().Except(person.Keys).ToList();
                List<Tuple<string, string>> listUserItem = new List<Tuple<string, string>>();
                foreach (var item in listItems)
                {
                    listUserItem.Add(new Tuple<string, string>("186", item));
                }
                List<Tuple<string, string, double>> listRecommended = calculateRating(listUserItem);
                listRecommended = listRecommended.OrderByDescending(r => r.Item3).ToList();
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("Person : " + listRecommended[i].Item1 + " Item : " + listRecommended[i].Item2 + " Rating : " + listRecommended[i].Item3);
                }
                Console.ReadLine();
            }
        }

        static void readjustDeviation(List<Tuple<string, string, double>> itemRatedList)
        {
            foreach (var itemRated in itemRatedList)
            {
                dataUsers.Single(p => p.Key.Equals(itemRated.Item1)).Value.Add(itemRated.Item2, itemRated.Item3);
                Dictionary<string, double> person = dataUsers.Single(p => p.Key.Equals(itemRated.Item1)).Value;
                KeyValuePair<string, Dictionary<string, Tuple<double, int>>> items = dataItems.Single(i => i.Key.Equals(itemRated.Item2));
                Dictionary<string, Dictionary<string, Tuple<double, int>>> itemsUpdated = new Dictionary<string, Dictionary<string, Tuple<double, int>>>();
                itemsUpdated.Add(items.Key, new Dictionary<string, Tuple<double, int>>());

                foreach (var item in items.Value)
                {
                    if (person.Keys.Contains(item.Key)) {
                        double newDiviation = (item.Value.Item1 * item.Value.Item2) + (itemRated.Item3 - person.Single(p => p.Key.Equals(item.Key)).Value) / (item.Value.Item2 + 1);
                        Tuple<double, int> newTemp = new Tuple<double, int>(newDiviation, item.Value.Item2 + 1);

                        itemsUpdated.Single(i => i.Key.Equals(itemRated.Item2)).Value.Add(item.Key, newTemp);

                        newTemp = new Tuple<double, int>(-newDiviation, item.Value.Item2 + 1);
                        dataItems.Single(i => i.Key.Equals(item.Key)).Value.Remove(itemRated.Item2);
                        dataItems.Single(i => i.Key.Equals(item.Key)).Value.Add(itemRated.Item2, newTemp);
                    }
                }
                dataItems.Remove(itemRated.Item2);

                foreach (var itemUpdated in itemsUpdated.Values) {
                    dataItems.Add(itemRated.Item2, itemUpdated);
                }
            }
        }

        static List<Tuple<string, string, double>> calculateRating(List<Tuple<string, string>> listUserItem)
        {
            List<Tuple<string, string, double>> listRecommended = new List<Tuple<string, string, double>>();
            foreach (var userItem in listUserItem)
            {
                KeyValuePair<string, Dictionary<string, double>> person = dataUsers.Single(p => p.Key.Equals(userItem.Item1));
                KeyValuePair<string, Dictionary<string, Tuple<double, int>>> items = dataItems.Single(i => i.Key.Equals(userItem.Item2));

                double rating = 0;
                double totalUserCounter = 0;
                double totalRating = 0;
                foreach (var item in person.Value)
                {
                    var singleItemInfo = items.Value.Single(i => i.Key.Equals(item.Key)).Value;
                    totalRating += (item.Value + singleItemInfo.Item1) * singleItemInfo.Item2;
                    totalUserCounter += singleItemInfo.Item2;
                }
                rating = totalRating / totalUserCounter;
                listRecommended.Add(new Tuple<string, string, double>(person.Key, userItem.Item2, rating));
                //Console.WriteLine("Person : " + person.Key + " Item : " + userItem.Item2 + " Rated : " + rating);
                //Console.ReadLine();
            }
            return listRecommended;
        }

        static void processData(string path)
        {
            StreamReader reader = new StreamReader(path);
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
                string[] fileContent = reader.ReadLine().Split(',');
                string userID = fileContent[0];
                string productID = fileContent[1];
                double rating = double.Parse(fileContent[2], CultureInfo.InvariantCulture);
                if (!dataUsers.ContainsKey(userID))
                {
                    Dictionary<string, double> productDictionary = new Dictionary<string, double>();
                    productDictionary.Add(productID, rating);
                    dataUsers.Add(userID, productDictionary);
                }
                else
                {
                    dataUsers.Single(p => p.Key.Equals(userID)).Value.Add(productID, rating);
                }
            }
            Console.WriteLine("Done reading the file");
            List<string> itemList = dataUsers.SelectMany(d => d.Value.Keys).Distinct().ToList();

            for (int i = 0; i < itemList.Count; i++)
            {
                for (int j = i + 1; j < itemList.Count; j++)
                {
                    if (itemList[j] != null)
                    {
                        var deviationValues = (from item in dataUsers
                                               where item.Value.ContainsKey(itemList[i]) && item.Value.ContainsKey(itemList[j])
                                            let x = item.Value.Single(pv => pv.Key.Equals(itemList[i])).Value
                                            let y = item.Value.Single(pv => pv.Key.Equals(itemList[j])).Value
                                            select new { Count = x,
                                                         Sum = x - y}.Sum);

                        if (!dataItems.ContainsKey(itemList[i]))
                        {
                            Dictionary<string, Tuple<double, int>> tempNewItem = new Dictionary<string, Tuple<double, int>>();
                            tempNewItem.Add(itemList[j], new Tuple<double, int>((deviationValues.Sum() / deviationValues.Count()), deviationValues.Count()));
                            tempNewItem.Add(itemList[i], new Tuple<double, int>(0, 0));
                            dataItems.Add(itemList[i], tempNewItem);
                        }
                        else {
                            dataItems.Single(item => item.Key.Equals(itemList[i])).Value.Add(itemList[j], new Tuple<double, int>((deviationValues.Sum() / deviationValues.Count()), deviationValues.Count()));
                        }
                        if (!dataItems.ContainsKey(itemList[j]))
                        {
                            Dictionary<string, Tuple<double, int>> tempNewItem = new Dictionary<string, Tuple<double, int>>();
                            tempNewItem.Add(itemList[i], new Tuple<double, int>(-(deviationValues.Sum() / deviationValues.Count()), deviationValues.Count()));
                            dataItems.Add(itemList[j], tempNewItem);
                        }
                        else {
                            dataItems.Single(item => item.Key.Equals(itemList[j])).Value.Add(itemList[i], new Tuple<double, int>(-(deviationValues.Sum() / deviationValues.Count()), deviationValues.Count()));
                        }
                    }
                }
            }
            Console.WriteLine("Done with the setup");
        }
    }
}
