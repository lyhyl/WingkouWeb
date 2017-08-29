using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WingkouWeb.Models
{
    public enum CGLabParamsType
    {
        Value,
        Option,
        Combo
    }

    public class CGLabParamsItem
    {
        public string Name { set; get; }
        public string HName { set; get; }
        public CGLabParamsType Type { set; get; }
        public dynamic Params { set; get; }
    }

    public class CGLabParams
    {
        public List<CGLabParamsItem> Items { set; get; }

        public CGLabParams()
        {
            Items = new List<CGLabParamsItem>();
        }

        public CGLabParams(IEnumerable<CGLabParamsItem> items)
        {
            Items = new List<CGLabParamsItem>(items);
        }

        public IHtmlString GetParamsQueryJS()
        {
            if (Items.Count == 0)
                return new HtmlString(string.Empty);
            StringBuilder js = new StringBuilder();
            js.AppendLine("function(){");
            js.AppendLine("var s = '';");
            foreach (var item in Items)
            {
                js.AppendLine($"var {item.HName}=document.getElementById('_CGP_{item.HName}');");
            }
            js.AppendLine("return s;");
            js.AppendLine("}");
            return new HtmlString(js.ToString());
        }

        public IHtmlString GetParamsPanel()
        {
            if (Items.Count == 0)
                return new HtmlString(string.Empty);
            StringBuilder html = new StringBuilder();
            html.AppendLine("<div class='panel panel-default'>");
            html.AppendLine("<div class='panel-body'>");
            foreach (var item in Items)
            {
                html.AppendLine($"<div class='text-primary'>{item.HName}</div>");
                switch (item.Type)
                {
                    case CGLabParamsType.Value:
                        html.AppendLine($"<input id='_CGP_{item.HName}' data-slider-id='ex1Slider' type='text' data-slider-min='0' data-slider-max='20' data-slider-step='1' data-slider-value='14' />");
                        break;
                    case CGLabParamsType.Option:
                        html.AppendLine($"<input id='_CGP_{item.HName}' type='checkbox' name='name' value='' />");
                        break;
                    case CGLabParamsType.Combo:
                        html.AppendLine("<div class='btn-group'>");
                        html.AppendLine($"<a id='_CGP_{item.HName}' class='btn btn-default dropdown-toggle btn-select' href='#' data-toggle='dropdown'>");
                        html.AppendLine("Action <span class='caret'></span>");
                        html.AppendLine("</a>");
                        html.AppendLine("<ul class='dropdown-menu'>");
                        foreach (var p in item.Params)
                        {
                            html.AppendLine($"<li><a href='#'>{p}</a></li>");
                        }
                        html.AppendLine("</ul>");
                        html.AppendLine("</div>");
                        break;
                    default:
                        break;
                }
            }
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return new HtmlString(html.ToString());
        }
    }
}