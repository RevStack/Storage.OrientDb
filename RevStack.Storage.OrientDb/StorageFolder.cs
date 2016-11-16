using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RevStack.Pattern;
using RevStack.OrientDb;
using RevStack.Storage.Model;

namespace Storage.OrientDb
{
    public class StorageFolder : IStorageFolder
    {
        public StorageFolder()
        {
            Files = new List<IStorageFile>();
            Folders = new List<IStorageFolder>();
        }

        public int Id { get; set; }

        public string Path { get; set; }

        [JsonConverter(typeof(InterfaceArrayConverter<IStorageFile, StorageFile>))]
        public List<IStorageFile> Files { get; set; }

        [JsonConverter(typeof(InterfaceArrayConverter<IStorageFolder, StorageFolder>))]
        public List<IStorageFolder> Folders { get; set; }

        [JsonProperty(PropertyName = "@class")]
        public string _class
        {
            get { return this.GetType().Name; }
        }

        [JsonProperty(PropertyName = "@rid")]
        public string _rid { get; set; }
    }
}
