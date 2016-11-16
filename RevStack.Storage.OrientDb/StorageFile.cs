using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RevStack.Storage.Model;

namespace Storage.OrientDb
{
    public class StorageFile : IStorageFile
    {
        public int Id { get; set; }
        public string Path { get; set; }
        [JsonProperty(PropertyName = "@class")]
        public string _class
        {
            get { return this.GetType().Name; }
        }
        [JsonProperty(PropertyName = "@rid")]
        public string _rid { get; set; }
    }
}
