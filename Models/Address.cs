using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json = Newtonsoft.Json;

namespace AddressLoader.Models
{
    [Nest.ElasticsearchType(Name = "address", IdProperty = "AddressDetailPid")]
    public class Address
    {
        [Json.JsonProperty("address_detail_pid")]
        public string AddressDetailPid { get; set; }

        [Json.JsonProperty("full_address_line")]
        [Nest.String(Name = "full_address_line")]
        public string FullAddressLine { get; set; }
    }

}
