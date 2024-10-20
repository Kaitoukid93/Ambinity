using Newtonsoft.Json;

namespace adrilight_shared.Models.Store
{
    /// <summary>
    /// legacy model for deserialization from store
    /// </summary>
    public class OnlineItemModel
    {
        public OnlineItemModel()
        {
        }

        private bool _isDownloading = false;
        private bool _isLocalExisted = false;
        private bool _isUpgradeAvailable = false;
        private string _gravatarID;
        private string _name;
        public string Name { get; set; }
        public string Owner { get; set; } // the name of creator
        public string Type { get; set; } // ledsetup or color palette
        public string Description { get; set; }
        [JsonIgnore]
        public string Path { get; set; }
        public string MarkDownDescription { get; set; }
        public string Version { get; set; }
        
    }
}