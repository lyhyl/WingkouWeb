using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
        public ActionResult PImg(string file, string connId, string method)
        {
            var stream = Request.InputStream;
            var errMsg = string.Empty;
            var result = string.Empty;
            try
            {
                long totalLen = stream.Length, savedLen = 0;
                var task = Task.Factory.StartNew(() =>
                {
                    string base64 = string.Empty;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024 * 1024];
                        while (savedLen < totalLen)
                        {
                            var len = stream.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer, 0, len);
                            savedLen += len;
                        }
                        base64 = Convert.ToBase64String(ms.ToArray());
                    }
                    using (ImageProcessingServiceClient client = new ImageProcessingServiceClient())
                    {
                        result = client.ProcessImage(base64, method);
                    }
                });

                while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
                {
                    ProcessingHub.UpdateProgress(connId, 0.3f * savedLen / totalLen * 100, string.Empty);
                    Thread.Sleep(200);
                }

                if (task.IsCompleted && !task.IsFaulted && result != string.Empty)
                {
                    ProcessingHub.UpdateProgress(connId, 100, string.Empty);
                    return Content(result);
                }
                else
                {
                    ProcessingHub.UpdateProgress(connId, -1, errMsg);
                    errMsg = task.Exception.InnerExceptions.Select(o => o.Message).Aggregate((a, b) => $"{a}|{b}");
                    return Content(errMsg);
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return Content(errMsg);
            }
        }
    }
}