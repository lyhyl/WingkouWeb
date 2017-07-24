using System;
using System.IO;
using System.Web.Mvc;
using WingkouWeb.Hubs;
using WingkouWeb.ImageProcessingService;

namespace WingkouWeb.Controllers
{
    public class CGLabController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HRUT()
        {
            ViewBag.Message = "How Are U Today?";

            return View();
        }

        //http://blog.darkthread.net/post-2014-03-09-upload-progress-bar-w-xhr2.aspx
        //http://blog.darkthread.net/post-2014-03-10-upload-progress-bar-w-signalr.aspx
        [HttpPost]
        public ActionResult PImg(string connId, string method)
        {
            Action<float, float, float> report =
                (p, o, l) => ProcessingHub.UpdateProgress(connId, o + l * p, string.Empty);
            try
            {
                string base64 = ConvertToBase64(Request.InputStream, (p) => report(p, 0, 20));

                string result = Process(method, base64, (p) => report(p, 20, 20));

                SendBack(connId, result, (p) => report(p, 40, 60));

                ProcessingHub.UpdateProgress(connId, 100, string.Empty);
            }
            catch (Exception e)
            {
                ProcessingHub.UpdateProgress(connId, -1, e.Message);
                return Content(e.Message);
            }
            return Content("");
        }

        private static void SendBack(string connId, string result, Action<float> report)
        {
            int rd = 0, rdt = result.Length;
            while (rd < rdt)
            {
                // 128Kb
                int len = Math.Min(128 * 1024, rdt - rd);
                ProcessingHub.ReceiveData(connId, result.Substring(rd, len), len, rdt);
                rd += len;

                report((float)rd / rdt);
            }
        }

        private static string Process(string method, string base64, Action<float> report)
        {
            string result;
            using (ImageProcessingServiceClient client = new ImageProcessingServiceClient())
                result = client.ProcessImage(base64, method);

            report(1);

            if (string.IsNullOrEmpty(result))
                throw new InvalidDataException("Null result");

            return result;
        }

        private static string ConvertToBase64(Stream stream, Action<float> report)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // 1Mb
                byte[] buffer = new byte[1024 * 1024];
                long totalLen = stream.Length, savedLen = 0;
                while (savedLen < totalLen)
                {
                    var len = stream.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, len);
                    savedLen += len;

                    report((float)savedLen / totalLen);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}