using ImageProcessingServicePlugin;

namespace IPPRaw
{
    public class ImageProcessingRaw : IIPSPlugin
    {
        public string Name => "Raw";

        public string Description => "No processing, return the raw image";

        public void Dispose() { }

        public string Process(string uri) => uri;
    }
}
