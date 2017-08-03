
using F1StatAndPred.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1StatAndPred.DTO
{
    public class QualificationResult 
    {
        
        public string id { get; set; }

        [SolrAttributes(FieldName = "Season")]
        public int Season { get; set; }

        [SolrAttributes(FieldName = "NumberOfRace")]
        public int NumberOfRace { get; set; }

        [SolrAttributes(FieldName = "NameOfRace")]
        public string NameOfRace { get; set; }

        [SolrAttributes(FieldName = "Position")]
        public int Position { get; set; }

        [SolrAttributes(FieldName = "DriverName", Boost = "1")]
        public string DriverName { get; set; }


        [SolrAttributes(FieldName = "ComandName")]
        public string ComandName { get; set; }

        [SolrAttributes(FieldName = "Q1Time")]
        public string Q1Time { get; set; }

        [SolrAttributes(FieldName = "Q2Time")]
        public string Q2Time { get; set; }

        [SolrAttributes(FieldName = "Q3Time")]
        public string Q3Time { get; set; }

    }
}
