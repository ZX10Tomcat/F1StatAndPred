using F1StatAndPred.Attributes;
using F1StatAndPred.DTO;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace F1StatAndPred.PreProcessors
{
    public class SearchPreProcessor
    {
        /// <summary>
        /// Add wildcards to every words
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public static string MakeSearchTerm(string searchString)
        {
            IEnumerable<string> words = searchString.Split(null);
            string escapedString = String.Empty;
            foreach (string item in words)
            {
                // If it's integer (ie position or umber of race don't escape
                int number;
                bool result = Int32.TryParse(item, out number);
                escapedString += result == true ? number.ToString() : $"*{item}* ";
            }
            return escapedString.TrimEnd();
        }

        /// <summary>
        /// Escaping special symbols
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public static string MakeEscaping (string searchString)
        {
            string escapedString = searchString
                .Replace("+", "\\+")
                .Replace("-", "\\-")
                .Replace("&&", "\\&\\&")
                .Replace("||", "\\|\\|")/* And so on */;

            return escapedString;
        }

        public static string GiveQualificationFields(string ClassName)
        {
            //It would be good send object type here
            //return "Season NumberOfRace NameOfRace Position DriverName ComandName Q1Time Q2Time Q3Time";

            Type type = Type.GetType(ClassName);
            object instance = Activator.CreateInstance(type);
            string fields = string.Empty;
            foreach (var prop in instance.GetType().GetProperties())
            {
                var attr = prop.GetCustomAttribute<SolrAttributes>(false);
                fields += $" {attr?.FieldName}";
                if (attr?.Boost != null)
                    fields += $"^{attr.Boost}";
            }

            return fields.Trim();
            
            //It would be good send object type here
            //return "Season NumberOfRace NameOfRace Position DriverName ComandName Q1Time Q2Time Q3Time";
        }
    }
}
