﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using WingkouWeb.Hubs;
using WingkouWeb.ImageProcessingService;
using WingkouWeb.Models;

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
            ViewBag.Title = "How Are U Today?";
            ViewBag.Method = "ImageProcessingHRUT";
            ViewBag.Description = "来张自拍\n生成你的心情图片~";
            ViewBag.ButtonText = "生成心情图片";

            var model = new CGLabParams();
            return View("Lab", model);
        }

        public ActionResult IOIO()
        {
            ViewBag.Title = "IOIO Sketch Drawing";
            ViewBag.Method = "ImageProcessingIOIO";
            ViewBag.Description = "IOIO\nSketch Drawing";
            ViewBag.ButtonText = "Sketch!";

            var model = new CGLabParams(
                new List<CGLabParamsItem>()
                {
                    new CGLabParamsItem()
                    {
                        Name = "密度",
                        HName = "ithr",
                        Type = CGLabParamsType.Value
                    },
                    new CGLabParamsItem()
                    {
                        Name = "反相",
                        HName = "inv",
                        Type = CGLabParamsType.Option
                    },
                    new CGLabParamsItem()
                    {
                        Name = "旋转",
                        HName = "rot",
                        Type = CGLabParamsType.Combo,
                        Params = new List<string>(new string[]{ "0","90","180","270" })
                    }
                });
            return View("Lab", model);
        }
        
        [HttpPost]
        public ActionResult PImg(string connId, string method, string parameters)
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
                ProcessingHub.UpdateProgress(connId, -1, $"{e.Message}|{e.StackTrace}");
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
            {
                result = client.ProcessImage(base64, method);

                report(1);

                if (string.IsNullOrEmpty(result))
                    throw new InvalidDataException($"Null result", client.GetLastError());
            }

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