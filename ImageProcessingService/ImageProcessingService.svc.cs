using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ImageProcessingService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ImageProcessingService : IImageProcessingService
    {
        public int ProcessImage(string uri, string methodName)
        {
            return -1;
        }

        public IEnumerable<ProcessMethod> GetAvailableMethod()
        {
            return null;
        }
    }
}
