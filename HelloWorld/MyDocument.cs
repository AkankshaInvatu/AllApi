using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    public class MyDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }
        public void Update(MyDocument updatedDocument)
        {
            // Implement your logic to update properties based on the updatedDocument
            this.Id = updatedDocument.Id;
            this.Name = updatedDocument.Name;
        }
    }
  
}
