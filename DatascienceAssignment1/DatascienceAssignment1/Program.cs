using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DatascienceAssignment1
{
    class Program
    {
        static Dictionary<string, Dictionary<string, double>> data = new Dictionary<string, Dictionary<string, double>>();
        static void Main(string[] args)
        {
            int part = 2;
            if (part == 1)
            {
                //remove the reader.readline in processdata this is only for header clearance. There is no header in this file
                processData("C:\\Users\\kevke6\\Desktop\\Datamining\\Datascience\\DatascienceAssignment1\\DatascienceAssignment1\\DataSource\\userItem.data");
                TestMain();
            }
            if (part == 2)
            {
                //add the reader.readline in processdata this is only for header clearance. There is a header in this file
                processData("C:\\Users\\kevke6\\Desktop\\Datamining\\Datascience\\DatascienceAssignment1\\DatascienceAssignment1\\DataSource\\ratings.csv");
                MoviePart();
            }
        }

        static void MoviePart()
        {
            KeyValuePair<string, Dictionary<string, double>> person = data.Single(p => p.Key.Equals("186"));
            List<KeyValuePair<string, Dictionary<string, double>>> personsForMeasure =
                data.Select(p => p).Where(p => !p.Key.Contains("186")).ToList();
            Dictionary<string, double> pearsonOutcome = calculatePearson(person, personsForMeasure);
            Dictionary<string, double> nearestNeighborP = showNearestNeighbors(pearsonOutcome, 0.35, 25);
            List<string> items = data.Where(d => nearestNeighborP.Any(n => n.Key.Equals(d.Key))).SelectMany(d => d.Value.Keys).Distinct().Except(person.Value.Keys).ToList();
            Dictionary<string, double> predictedRating = calculatePredictedRating(person, nearestNeighborP, items);
            List<KeyValuePair<string, double>> orderedRatings = predictedRating.OrderByDescending(p => p.Value).ToList();
            for (int i = 0; i< 8; i++)
            {
                Console.WriteLine("Top " + i + ": product " + orderedRatings[i].Key + " with rating: " + orderedRatings[i].Value);
            }
            Console.ReadLine();
            items = data.Where(d => nearestNeighborP.Any(n => n.Key.Equals(d.Key)) && d.Value.Count() >= 3).SelectMany(d => d.Value.Keys).Distinct().Except(person.Value.Keys).ToList();
            predictedRating = calculatePredictedRating(person, nearestNeighborP, items);
            orderedRatings = predictedRating.OrderByDescending(p => p.Value).ToList();
            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine("Top " + i + ": product " + orderedRatings[i].Key + " with rating: " + orderedRatings[i].Value);
            }
            Console.ReadLine();
        }

        static void TestMain()
        {
            KeyValuePair<string, Dictionary<string, double>> person = data.Single(p => p.Key.Equals("7"));
            List<KeyValuePair<string, Dictionary<string, double>>> personsForMeasure =
                data.Select(p => p).Where(p => !p.Key.Contains("7")).ToList();
            Dictionary<string, double> pearsonOutcome = calculatePearson(person, personsForMeasure);
            Dictionary<string, double> euclideanOutcome = calculateEuclidean(person, personsForMeasure);
            Dictionary<string, double> cosineOutcome = calculateCosine(person, personsForMeasure);
            Dictionary<string, double> nearestNeighborP = showNearestNeighbors(pearsonOutcome, 0.35, 3);
            Dictionary<string, double> nearestNeighborE = showNearestNeighbors(euclideanOutcome, 0.35, 3);
            Dictionary<string, double> nearestNeighborC = showNearestNeighbors(cosineOutcome, 0.35, 3);

            List<string> itemForPredictList = new List<string>();
            itemForPredictList.Add("101");
            itemForPredictList.Add("103");
            itemForPredictList.Add("106");

            calculatePredictedRating(person, nearestNeighborP, itemForPredictList);

            person = data.Single(p => p.Key.Equals("4"));
            personsForMeasure = data.Select(p => p).Where(p => !p.Key.Contains("4")).ToList();

            pearsonOutcome = calculatePearson(person, personsForMeasure);
            nearestNeighborP = showNearestNeighbors(pearsonOutcome, 0.35, 3);
            itemForPredictList.Clear();
            itemForPredictList.Add("101");
            calculatePredictedRating(person, nearestNeighborP, itemForPredictList);

            data.Single(p => p.Key.Equals("7")).Value.Add("106", 2.8);

            person = data.Single(p => p.Key.Equals("7"));
            personsForMeasure = data.Select(p => p).Where(p => !p.Key.Contains("7")).ToList();

            pearsonOutcome = calculatePearson(person, personsForMeasure);
            nearestNeighborP = showNearestNeighbors(pearsonOutcome, 0.35, 3);
            itemForPredictList.Clear();
            itemForPredictList.Add("101");
            itemForPredictList.Add("103");
            calculatePredictedRating(person, nearestNeighborP, itemForPredictList);

        }

        static Dictionary<string, double> calculatePredictedRating(KeyValuePair<string, Dictionary<string, double>> person, Dictionary<string, double> nearestNeigbors, List<string> itemForPredictList)
        {
            Dictionary<string, double> preditedRating = new Dictionary<string, double>();
            if (nearestNeigbors != null) {
                foreach (string item in itemForPredictList) {
                    double totalCoefficient = 0;
                    double sumRatingUsers = 0;
                    foreach (var neighbor in nearestNeigbors) {
                        if (!data.Single(p => p.Key.Equals(neighbor.Key)).Value.ContainsKey(item)) continue;
                        sumRatingUsers += data.Single(p => p.Key.Equals(neighbor.Key)).Value.Single(v => v.Key.Equals(item)).Value * neighbor.Value;
                        totalCoefficient += neighbor.Value;
                    }
                    double newRating = sumRatingUsers / totalCoefficient;
                    preditedRating.Add(item, newRating);
                    //Console.WriteLine("Rating " + newRating + " item:" + item);
                }
            }
            return preditedRating;
        }

        static Dictionary<string, double> showNearestNeighbors(Dictionary<string, double> simDictionary, double threshold, int neighbors)
        {
            Dictionary<string, double> nearestNeighbors = new Dictionary<string, double>();
            List<KeyValuePair<string, double>> simList = simDictionary.OrderByDescending(s => s.Value).ToList();
            for (int i = 0; i < neighbors; i++)
            {
                if (simList[i].Value >= threshold)
                {
                    Console.WriteLine(simList[i].Key + " similarity: " + simList[i].Value);
                    nearestNeighbors.Add(simList[i].Key, simList[i].Value);
                }
            }
            Console.ReadLine();
            return nearestNeighbors;
        }

        static Dictionary<string, double> calculateCosine(KeyValuePair<string, Dictionary<string, double>> person,
                                                            List<KeyValuePair<string, Dictionary<string, double>>> personsForMeasure)
        {
            Dictionary<string, double> cosineOutcome = new Dictionary<string, double>();
            foreach (var person2 in personsForMeasure)
            {
                double cosineSim = 0;
                double sumXY = 0;
                double sumXP2 = 0;
                double sumYP2 = 0;

                foreach (var item in person2.Value) {
                    if (person.Value.ContainsKey(item.Key)) {
                        double x = person.Value.Single(p => p.Key.Equals(item.Key)).Value;
                        sumXY += item.Value * x;
                        sumXP2 += x * x;
                        sumYP2 += item.Value * item.Value;
                    } else {
                        sumYP2 += item.Value * item.Value;
                    }
                }

                sumXP2 += person.Value.Where(item => !person2.Value.ContainsKey(item.Key)).Sum(item => item.Value * item.Value); 

                cosineSim = sumXY / (Math.Sqrt(sumXP2) * Math.Sqrt(sumYP2));
                cosineOutcome.Add(person2.Key, cosineSim);
            }
            return cosineOutcome;
        }

        static Dictionary<string,double> calculatePearson(KeyValuePair<string, Dictionary<string, double>> person,
                                                            List<KeyValuePair<string, Dictionary<string, double>>> personsForMeasure)
        {
            Dictionary<string, double> personOutcome = new Dictionary<string, double>();
            foreach (var person2 in personsForMeasure) {
                double sumX = 0;
                double sumY = 0;
                double n = 0;
                double sumXY = 0;
                double avrSumXSumY = 0;
                double sumXP2 = 0;
                double sumYP2 = 0;
                double avrSumXP2 = 0;
                double avrSumYP2 = 0;
                double correlation = 0;

                foreach (var item in person2.Value)
                {
                    if (!person.Value.ContainsKey(item.Key)) continue;

                    double x = person.Value.Single(pv => pv.Key.Equals(item.Key)).Value;
                    sumX += x;
                    sumY += item.Value;
                    sumXY += x * item.Value;
                    sumXP2 += x * x;
                    sumYP2 += item.Value * item.Value;
                    n++;

                }
                if (n > 0)
                {
                    avrSumXSumY = (sumX * sumY) / n;
                    avrSumXP2 = (sumX * sumX) / n;
                    avrSumYP2 = (sumY * sumY) / n;
                }
                if (sumXP2 - avrSumXP2 <= 0 || sumYP2 - avrSumYP2 <= 0)
                {
                    correlation = 0;
                }
                else
                {
                    correlation = (sumXY - avrSumXSumY) / (Math.Sqrt(sumXP2 - avrSumXP2) * Math.Sqrt(sumYP2 - avrSumYP2));
                }
                personOutcome.Add(person2.Key, correlation);
            }

            return personOutcome;
        }

        static Dictionary<string, double> calculateEuclidean(KeyValuePair<string, Dictionary<string, double>> person, 
                                                                List<KeyValuePair<string, Dictionary<string, double>>> personsForMeasure)
        {
            Dictionary<string, double> euclideanOutcome = new Dictionary<string, double>();
            foreach (var person2 in personsForMeasure)
            {
                double euclideanSim = 0;
                double sumXminusYP2 = (from item in person.Value
                                       where person2.Value.ContainsKey(item.Key)
                                       let x = person2.Value.Single(pv => pv.Key.Contains(item.Key)).Value
                                       select Math.Pow(x - item.Value, 2)).Sum();
                euclideanSim = 1 / (1 + Math.Sqrt(sumXminusYP2));


                euclideanOutcome.Add(person2.Key, euclideanSim);
            }

            return euclideanOutcome;
        }

        static void processData(string location)
        {
            StreamReader reader = new StreamReader(location);
            reader.ReadLine();  //Removes the header for ratings.csv
            while (!reader.EndOfStream) {
                string[] fileContent = reader.ReadLine().Split(',');
                string userID = fileContent[0];
                string productID = fileContent[1];
                double rating = double.Parse(fileContent[2], CultureInfo.InvariantCulture);
                if (!data.ContainsKey(userID))
                {
                    Dictionary<string, double> productDictionary = new Dictionary<string, double>();
                    productDictionary.Add(productID, rating);
                    data.Add(userID, productDictionary);
                }
                else {
                    data.Single(p => p.Key.Equals(userID)).Value.Add(productID, rating);
                }
            }
        }
    }
}
