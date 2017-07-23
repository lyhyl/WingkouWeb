using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace ImageProcessingService
{
    public class PluginSettingsSection : ConfigurationSection
    {
        [ConfigurationProperty("path",IsRequired =true)]
        public string Path
        {
            get => (string)this["path"];
            set => this["path"] = value;
        }
    }
}