using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ImageProcessingService
{
    [ServiceContract]
    public interface IImageProcessingService
    {
        [OperationContract]
        string ProcessImage(string uri, string methodName, Action<double> callback);

        [OperationContract]
        IEnumerable<ProcessMethod> GetAvailableMethod();

        [OperationContract]
        Exception GetLastError();
    }

    [DataContract]
    public class ProcessMethod
    {
        [DataMember]
        public string Name { private set; get; }

        [DataMember]
        public string Description { private set; get; }

        public ProcessMethod(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
