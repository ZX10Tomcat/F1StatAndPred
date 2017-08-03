using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using F1StatAndPred.DTO;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using F1StatAndPred.PreProcessors;

namespace F1StatAndPred.Controllers
{
    [Route("api/[controller]")]
    public class QualificationsController : Controller
    {

       const string solrUrl = "http://ubuntudocker-56l10dd2.cloudapp.net:8983/solr/f1_qualifications/";

       const string apiEndpoint = "http://ergast.com/api/f1";

        [HttpGet]
        public IEnumerable<string> Get()
        {  
            return new string[] { "Formula1", "Statistics and predictions" };
        }
        // Fill in qualification results
        [Route("setresults")]
        [HttpGet]
        public async Task<HttpStatusCode> SetQualResultsAsync()
        {   
            string format = "json";
            string type = "qualifying";

            using (var client = new HttpClient())
            {
                //Download every races from 1994 year
                for (int year = 1994; year <= DateTime.Now.Year; year++)
                {
                    // There's no more 20 racer per year
                    for (int race = 1; race <= 20; race++)
                    {
                        string requestUrl = CreateRequest(apiEndpoint, year, race, type, format);
                        var response = client.GetStringAsync(requestUrl).Result;
                        JObject joResponse = JObject.Parse(response);
                        JObject raceData = (JObject)joResponse["MRData"]["RaceTable"];
                        JToken races = raceData["Races"];
                        // If no race data
                        if (races.Count() == 0)
                            continue;
                        // Goes through positions
                        for (int position = 0; position < joResponse["MRData"]["total"].Value<int>(); position++)
                        {
                            QualificationResult document = GetDocument(raceData, position);
                            await SaveDocumentToSolr(document);
                        }
                    }

                }
            }

            return HttpStatusCode.OK;
        }

        private static async Task SaveDocumentToSolr(QualificationResult document)
        {
            using (var solrClient = new HttpClient())
            {
                string solrUrlUpdate = $"{solrUrl}update/json?wt=json&commit=true";
               
                string documentStr = MakeSolrJson(document);
                Task<HttpResponseMessage> solrResponse = solrClient.PostAsync(new Uri(solrUrlUpdate), new StringContent(documentStr));
                string responseMsg = String.Empty;
                if (solrResponse.Result.StatusCode != HttpStatusCode.OK)
                    responseMsg = await solrResponse.Result.Content.ReadAsStringAsync();
            }
        }

        private static string MakeSolrJson(QualificationResult document)
        {
            StringBuilder documentBuilder = new StringBuilder();
            documentBuilder.Append("{add:{doc:");
            documentBuilder.Append(JsonConvert.SerializeObject(document));
            documentBuilder.Append("}}");
            return documentBuilder.ToString();
        }

        private QualificationResult GetDocument(JObject raceData, int position)
        {
            QualificationResult result = new QualificationResult();

            try
            {   
              
                JToken driver = raceData["Races"][0]["QualifyingResults"][position]["Driver"];

                result.Season = raceData["season"].Value<int>();
                result.NumberOfRace = raceData["round"].Value<int>();
                result.NameOfRace = raceData["Races"][0]["raceName"].Value<string>();
                result.Position = raceData["Races"][0]["QualifyingResults"][position]["position"].Value<int>();
                result.DriverName = $"{driver["givenName"].Value<string>()} {driver["familyName"].Value<string>()}";
                result.ComandName = raceData["Races"][0]["QualifyingResults"][position]["Constructor"]["name"].Value<string>();
                result.Q1Time = raceData["Races"][0]["QualifyingResults"][position]["Q1"]?.Value<string>() ?? "";
                result.Q2Time = raceData["Races"][0]["QualifyingResults"][position]["Q2"]?.Value<string>() ?? "";
                result.Q3Time = raceData["Races"][0]["QualifyingResults"][position]["Q3"]?.Value<string>() ?? "";
                result.id = $"{result.Season}_{result.NumberOfRace}_{result.Position}";
                
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }

            return result;
        }
        
        public static string CreateRequest(string apiEndPoint, int year, int race, string type, string format)
        {
            string UrlRequest = $"{apiEndPoint}/{year}/{race}/{type}.{format}";
            return (UrlRequest);
        }
        // GET api/qualifications/Bottas
        [HttpGet("{searchString}")]
        public string Get(string searchString)
        {
            string documents = string.Empty;
            string parser = "edismax";

            // Maybe paiping
            string searchKeyWords = SearchPreProcessor.MakeSearchTerm(searchString);
            
            string query = parser == "standard" ? SearchPreProcessor.MakeEscaping(searchKeyWords) : searchKeyWords;
           
            string fields = SearchPreProcessor.GiveQualificationFields("F1StatAndPred.DTO.QualificationResult");
            
            string searchUrl = $"{solrUrl}select?defType={parser}&q={query}&qf={fields}&wt=json";

            using (var client = new HttpClient())
            {
                documents = client.GetStringAsync(searchUrl).Result;
            }

            return documents;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
