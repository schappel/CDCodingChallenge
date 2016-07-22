using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options.Extensions;

using AddressLoader.Utility;
using AddressLoader.Models;
using System.IO;
using Newtonsoft.Json;
using Nest;

namespace AddressLoader
{
    class Program
    {
        const string IndexName = "addresses";

        private static RequiredValuesOptionSet options;
        private static Variable<string> filenameOption;
        private static Variable<string> elasticSearchUrlOption;
        private static Variable<string> indexNameOption;

        private static RequiredValuesOptionSet SetupOptions()
        {
            options = new RequiredValuesOptionSet();
            filenameOption = options.AddRequiredVariable<string>("filename", "path to file that will be imported");
            elasticSearchUrlOption = options.AddRequiredVariable<string>("url", "url to elastic search instance");
            indexNameOption = options.AddVariable<string>("index", "optional index name to use");
            return options;
        }

        private static ElasticClient GetClient()
        {

            var connectionSettings = new ConnectionSettings(new Uri(elasticSearchUrlOption.Value))
                .DefaultIndex(IndexName)
                .MapDefaultTypeNames(m => m.
                   Add(typeof(Address), "address")
                )
                .MapDefaultTypeIndices(m => m
                   .Add(typeof(Address), string.IsNullOrEmpty(indexNameOption.Value) ? IndexName : indexNameOption.Value )
                );

            return new ElasticClient(connectionSettings);
        }

        private static bool SetupIndex( ElasticClient client, string indexName, out string errorMessage )
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(indexName)) indexName = IndexName;

            if (client.IndexExists(indexName).Exists)
            {
                var deleteIndexResponse = client.DeleteIndex(indexName);
                if( !deleteIndexResponse.IsValid)
                {
                    errorMessage = "Failed to delete index: " + deleteIndexResponse.OriginalException.Message;
                    return false;
                }
            }

            var indexResponse = client.CreateIndex(indexName, i => i
               .Settings(s => s
                  .NumberOfShards(2)
                  .NumberOfReplicas(0)
               )
            );
            if( !indexResponse.IsValid )
            {
                errorMessage = "Failed to create index: " + indexResponse.OriginalException.Message;
                return false;
            }

            var mappingResponse = client.Map<Address>(m => m.AutoMap());
            if (!mappingResponse.IsValid)
            {
                errorMessage = "Failed to create index mapping: " + mappingResponse.OriginalException.Message;
                return false;
            }

            return true;
        }

        static void Main(string[] args)
        {
            SetupOptions();
            var cm = new ConsoleManager("Address Loader", options);
            if (!cm.TryParseOrShowHelp(Console.Out, args)) Environment.Exit(1);

            if( !File.Exists( filenameOption.Value ) )
            {
                Console.WriteLine("Cannot open file: \"{0}\"", filenameOption.Value);
                Environment.Exit(1);
            }

            // Get our Elastic Search client
            var client = GetClient();

            // Setup the index and mappings...note: this deletes any previous index of the given name
            var errorMessage = string.Empty;
            if( !SetupIndex(client, indexNameOption.Value, out errorMessage ) )
            {
                Console.WriteLine(errorMessage);
                Environment.Exit(1);
            }

            // Iniatate a build request for all documents (note: this could be batched if the dataset is too large)
            using (var fileReader = new FileReader<Address>(filenameOption.Value))
            {
                do
                {
                    Console.Write(".");

                    var result = client.Bulk(r =>
                    {
                        foreach (var address in fileReader.Take(1000))
                        {
                            r.Index<Address>(i => i.Document(address));
                        }
                        return r;
                    });
                    if (!result.IsValid)
                    {
                        Console.WriteLine(result.OriginalException.Message);
                        Environment.Exit(1);
                    }

                } while (!fileReader.EndOfFile);
            }

#if DEBUG
            if( System.Diagnostics.Debugger.IsAttached )
            {
                Console.WriteLine("\r\nPress ENTER when ready");
                Console.ReadLine();
            }
#endif
        }
    }
}
