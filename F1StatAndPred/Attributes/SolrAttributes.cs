using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1StatAndPred.Attributes
{
    public class SolrAttributes : Attribute
    {
        public string  FieldName { get; set; }
        public string Boost { get; set; }
    }
}
