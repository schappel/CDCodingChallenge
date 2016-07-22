using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AddressLoader.Models;
using Nest;
using Json = Newtonsoft.Json;

namespace AddressApiHost.Controllers
{
    /*
    public class SuggestResponse
    {
        [Json.JsonProperty("id")]
        public string Id { get; set; }

        [Json.JsonProperty("text")]
        public string Text { get; set; }
    }
    */

    public class AddressController : ApiController
    {
        // Inject this later
        private static ElasticClient GetClient()
        {
            var elasticSearchUrl = System.Configuration.ConfigurationManager.AppSettings["elasticSearchUrl"];
            var indexName = System.Configuration.ConfigurationManager.AppSettings["elasticSearchIndexName"];

            var connectionSettings = new ConnectionSettings(new Uri(elasticSearchUrl))
                .DefaultIndex(indexName)
                .MapDefaultTypeNames(m => m.
                   Add(typeof(Address), "address")
                )
                .MapDefaultTypeIndices(m => m
                   .Add(typeof(Address), indexName )
                );

            return new ElasticClient(connectionSettings);
        }


        [HttpGet]
        [Route("suggest")]
        public IEnumerable<Address> GetSuggest( string text="", string count="10" )
        {
            var recordsToReturn = 10;
            int.TryParse(count, out recordsToReturn);

            var client = GetClient();
            var searchResponse = client.Search<Address>(s => s
                .Size(recordsToReturn)
                .Query( q => q
                    .Match( m => m
                        .Field( f => f.FullAddressLine )
                        .Operator( Operator.And )
                        .Query( text )
                    )
                )
            );
            if( !searchResponse.IsValid ) throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);

            return searchResponse
                .Hits
                .Select(h => h.Source);
                //.Select( h => new SuggestResponse { Id = h.Source.AddressDetailPid, Text = h.Source.FullAddressLine } );
        }
    }

}
