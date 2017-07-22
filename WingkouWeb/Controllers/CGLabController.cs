using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        [HttpPost]
        public ActionResult HRUT(string file)
        {
            var stream = Request.InputStream;
            long totalLen = stream.Length, uploadedBytes = 0;

            byte[] buffer = new byte[1024 * 1024];
            string outPath = Path.Combine(Server.MapPath("~/App_Data"), file);

            using (FileStream fs = new FileStream(outPath, FileMode.Create))
            {
                while (uploadedBytes < totalLen)
                {
                    var len = stream.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, len);
                    uploadedBytes += len;
                }
            }

            return Content("OK");
        }
    }
}