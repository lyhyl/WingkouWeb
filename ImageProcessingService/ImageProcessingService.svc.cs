using ImageProcessingServicePlugin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;

namespace ImageProcessingService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ImageProcessingService : IImageProcessingService
    {
        private static PluginManager _manager = null;
        private static PluginManager PluginManager
        {
            get
            {
                if (_manager == null)
                {
                    var config = (PluginSettingsSection)ConfigurationManager.GetSection("pluginSettingsGroup/pluginSettings");
                    _manager = new PluginManager(config.Path);
                }
                return _manager;
            }
        }

        public string ProcessImage(string uri, string methodName)
        {
            string res = string.Empty;
            IIPSPlugin plugin = null;
            try
            {
                plugin = PluginManager.Create(methodName);
                res = plugin.Process(uri);
            }
            catch(Exception e)
            {
            }
            finally
            {
                plugin?.Dispose();
            }
            return res;
        }

        public IEnumerable<ProcessMethod> GetAvailableMethod()
        {
            return PluginManager.Plugins.Select(p => new ProcessMethod(p.Name, string.Empty));
        }
    }
}
