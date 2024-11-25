using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NHL_Assignment
{
    public class Player
    {
        public string PlayerName { get; set; }
        public string Team { get; set; }
        public string Pos { get; set; }
        public int GP { get; set; }
        public int G { get; set; }
        public int A { get; set; }
        public int P { get; set; }
        public int PlusMinus { get; set; }
        public int PIM { get; set; }
        public double PGP { get; set; }
        public int PPG { get; set; }
        public int PPP { get; set; }
        public int SHG { get; set; }
        public int SHP { get; set; }
        public int GWG { get; set; }
        public int OTG { get; set; }
        public int S { get; set; }
        public double SPercent { get; set; }
        public double TOIGP { get; set; }
        public double ShiftsPerGP { get; set; }
        public double FOWPercent { get; set; }
    }

    internal class Program
    {
        //list for player object
        static List<Player> players = new List<Player>();

        private static string NextValue(string line, ref int index)
        {
            int start = index;
            while (index < line.Length && line[index] != ',')
                index++;
            string value = line.Substring(start, index - start).Trim();
            index++;
            return value;
        }

        static void BuildPlayerDBFromFile()
        {
            using (StreamReader reader = File.OpenText("NHL Player Stats 2017-18.csv"))
            {
                //skip header row
                string line = reader.ReadLine();

                //start reading the file until the end
                while ((line = reader.ReadLine()) != null)
                {
                    int index = 0;

                    //method to parse fields safely
                    //in some cases the csv is missing data (has '--' instead)
                    //so use 0 instead
                    string GetValue(string field) => field == "--" ? "0" : field;

                    Player player = new Player
                    {
                        PlayerName = NextValue(line, ref index),
                        Team = NextValue(line, ref index),
                        Pos = NextValue(line, ref index),
                        GP = int.TryParse(GetValue(NextValue(line, ref index)), out int gp) ? gp : 0,
                        G = int.TryParse(GetValue(NextValue(line, ref index)), out int g) ? g : 0,
                        A = int.TryParse(GetValue(NextValue(line, ref index)), out int a) ? a : 0,
                        P = int.TryParse(GetValue(NextValue(line, ref index)), out int p) ? p : 0,
                        PlusMinus = int.TryParse(GetValue(NextValue(line, ref index)), out int plusMinus) ? plusMinus : 0,
                        PIM = int.TryParse(GetValue(NextValue(line, ref index)), out int pim) ? pim : 0,
                        PGP = double.TryParse(GetValue(NextValue(line, ref index)), out double pgp) ? pgp : 0.0,
                        PPG = int.TryParse(GetValue(NextValue(line, ref index)), out int ppg) ? ppg : 0,
                        PPP = int.TryParse(GetValue(NextValue(line, ref index)), out int ppp) ? ppp : 0,
                        SHG = int.TryParse(GetValue(NextValue(line, ref index)), out int shg) ? shg : 0,
                        SHP = int.TryParse(GetValue(NextValue(line, ref index)), out int shp) ? shp : 0,
                        GWG = int.TryParse(GetValue(NextValue(line, ref index)), out int gwg) ? gwg : 0,
                        OTG = int.TryParse(GetValue(NextValue(line, ref index)), out int otg) ? otg : 0,
                        S = int.TryParse(GetValue(NextValue(line, ref index)), out int s) ? s : 0,
                        SPercent = double.TryParse(GetValue(NextValue(line, ref index)), out double sPercent) ? sPercent : 0.0,
                        TOIGP = 0.0,
                        ShiftsPerGP = double.TryParse(GetValue(NextValue(line, ref index)), out double shiftsPerGP) ? shiftsPerGP : 0.0,
                        FOWPercent = double.TryParse(GetValue(NextValue(line, ref index)), out double fowPercent) ? fowPercent : 0.0
                    };

                    players.Add(player);

                }
            }
        }


        //display player stats in a table format
        static void DisplayPlayerStats(IEnumerable<Player> playerList)
        {
            Console.WriteLine("\nPlayer Stats:");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("{0,-25} {1,-7} {2,-4} {3,4} {4,4} {5,4} {6,4} {7,6} {8,4} {9,6} {10,4}",
                              "Player", "Team", "Pos", "GP", "G", "A", "P", "+/-", "PIM", "P/GP", "S%");

            foreach (var player in playerList)
            {
                Console.WriteLine("{0,-25} {1,-7} {2,-4} {3,4} {4,4} {5,4} {6,4} {7,6} {8,4} {9,6:F2} {10,6:F2}",
                                  player.PlayerName, player.Team, player.Pos, player.GP, player.G,
                                  player.A, player.P, player.PlusMinus, player.PIM, player.PGP, player.SPercent);
            }
            Console.WriteLine("--------------------------------------------------------------");
        }

        //filter players based on the user-provided filter expression
        static IEnumerable<Player> ApplyFilters(IEnumerable<Player> players, string filterExpression)
        {
            var filters = filterExpression.Split(',');
            foreach (var filter in filters)
            {
                var parts = filter.Trim().Split(' ');
                if (parts.Length == 3)
                {
                    string property = parts[0];
                    string operatorStr = parts[1];
                    string valueStr = parts[2];

                    players = players.Where(player => FilterCondition(player, property, operatorStr, valueStr));
                }
            }
            return players;
        }

        //evaluates a single filter condition
        static bool FilterCondition(Player player, string property, string operatorStr, string valueStr)
        {
            //parsing string value into numeric, if it succeeds, use it or use 0
            double numericValue = double.TryParse(valueStr, out var val) ? val : 0;
            double propertyValue = 0;

            //because we force user input to lowercase, all the columns are lower cased
            if (property == "gp") propertyValue = player.GP;
            else if (property == "g") propertyValue = player.G;
            else if (property == "a") propertyValue = player.A;
            else if (property == "p") propertyValue = player.P;
            else if (property == "pim") propertyValue = player.PIM;
            else if (property == "pgp") propertyValue = player.PGP;
            else if (property == "spercent") propertyValue = player.SPercent;
            else return false;

            //evaluate filter condition
            if (operatorStr == ">") return propertyValue > numericValue;
            else if (operatorStr == "<") return propertyValue < numericValue;
            else if (operatorStr == "==") return propertyValue == numericValue;
            else if (operatorStr == ">=") return propertyValue >= numericValue;
            else if (operatorStr == "<=") return propertyValue <= numericValue;
            else return false;
        }

        //sort players based on the user-provided sort expression
        static IEnumerable<Player> ApplySorting(IEnumerable<Player> players, string sortExpression)
        {
            var parts = sortExpression.Split(' ');
            //if there is no user input for the sorting order, return the unsorted players list
            if (parts.Length != 2) return players;

            string property = parts[0].Trim(); //trim spaces
            
            //flag that checks if the user wants ascending order
            bool ascending = parts[1].Equals("asc");

            //coverting from IEnumerable to list for comparison
            List<Player> sortedList = players.ToList();

            sortedList.Sort((p1, p2) =>
            {
                int comparisonResult = 0;

                //determine the property to compare
                if (string.Equals(property, "gp", StringComparison.OrdinalIgnoreCase))
                    comparisonResult = p1.GP.CompareTo(p2.GP);
                else if (string.Equals(property, "g", StringComparison.OrdinalIgnoreCase))
                    comparisonResult = p1.G.CompareTo(p2.G);
                else if (string.Equals(property, "a", StringComparison.OrdinalIgnoreCase))
                    comparisonResult = p1.A.CompareTo(p2.A);
                else if (string.Equals(property, "p", StringComparison.OrdinalIgnoreCase))
                    comparisonResult = p1.P.CompareTo(p2.P);
                else if (string.Equals(property, "pim", StringComparison.OrdinalIgnoreCase))
                    comparisonResult = p1.PIM.CompareTo(p2.PIM);
                else if (string.Equals(property, "pgp", StringComparison.OrdinalIgnoreCase))
                    comparisonResult = p1.PGP.CompareTo(p2.PGP);
                else if (string.Equals(property, "spercent", StringComparison.OrdinalIgnoreCase))
                    comparisonResult = p1.SPercent.CompareTo(p2.SPercent);

                //reverse the order if descending
                return ascending ? comparisonResult : -comparisonResult;
            });

            return sortedList;
        }



        static void Main(string[] args)
        {
            BuildPlayerDBFromFile();

            while (true)
            {
                //display menu options
                Console.Clear();
                Console.WriteLine("Select an option:");
                Console.WriteLine("1. View Player Stats");
                Console.WriteLine("2. Sort Player Stats");
                Console.WriteLine("3. Filter Player Stats");
                Console.WriteLine("4. Exit");
                Console.Write("Enter your choice (1-4): ");
                string choice = Console.ReadLine()?.ToLower();
                switch (choice)
                {
                    case "1":
                        DisplayPlayerStats(players);
                        break;
                    case "2":
                        Console.Write("\nEnter sort column and direction (e.g. 'G asc'): ");
                        string sortExpression = Console.ReadLine()?.ToLower();
                        var sortedPlayers = ApplySorting(players, sortExpression);
                        DisplayPlayerStats(sortedPlayers);
                        break;
                    case "3":
                        Console.Write("\nEnter filter expression (e.g., 'GP > 10'): ");
                        string filterExpression = Console.ReadLine()?.ToLower();
                        var filteredPlayers = ApplyFilters(players, filterExpression);
                        DisplayPlayerStats(filteredPlayers);
                        break;
                    case "4":
                        Console.WriteLine("Exiting program...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please select a valid option.");
                        break;
                }

                Console.WriteLine("\nPress any key to return to the menu.");
                Console.ReadKey();
            }
        }
    }
}