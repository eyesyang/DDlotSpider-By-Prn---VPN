using System;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using DDlotSpider.Entity;
using CentaLine.Common;
using System.IO;
using static DDlotSpider.Service.FateadmApi;
using System.Diagnostics;
using System.Drawing;

namespace DDlotSpider.Service
{
    public class OrderHandler
    {
        public event LogInfoDelegate LogInfoEventHandler;

        public string msg { get; set; }

        public bool isConnected { get; set; }

        private FateadmRsp RepCode = null;

        public OrderItem CurrentItem = new OrderItem();

        private delegate void WbDelegate();

        int ti = 3600000 / Convert.ToInt32(AppSettings.TimeNum);
        public void Run(WebBrowser wb, int firstLoop)
        {
            try
            {
                if (firstLoop == 1)
                {
                    Execute(wb, firstLoop);
                }
                else
                {
                    while (!isConnected)
                    {
                        msg = "Waiting VPN Connection";
                        LogInfoEventHandler(msg, Color.DarkOrange);

                        Thread.Sleep(1000);
                    }
                    if (isConnected)
                    {
                        Execute(wb, firstLoop);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Execute(WebBrowser wb, int firstLoop)
        {
            wb.Invoke(new WbDelegate(() =>
            {
                var prns = DbService.GetOrder();

                var prn = string.Empty;
                if (prns != null)
                {
                    prn = prns[0].ToString();

                    OrderItem dr = new OrderItem();
                    dr.Machine = AppSettings.Machine;
                    dr.Prn = prn.ToString();

                    wb.Navigate("https://www.iris.gov.hk/eservices/searchlandregister/search.jsp?language=zh_TW&prn=" + prn);
                    CurrentItem = new OrderItem();
                    CurrentItem.Keyword = prn;
                    if (firstLoop == -1)
                    {
                        dr.Status = ProgressStatus.NotFound;
                        DbService.UpdateOrder(dr);
                    }
                    else
                    {
                        dr.Status = ProgressStatus.Search;
                        DbService.UpdateOrder(dr);
                    }
                }
                else
                {
                    if (LogInfoEventHandler != null)
                    {
                        msg = "所有PRN已抓取完畢！";
                        LogInfoEventHandler(msg, Color.Green);
                    }
                }

            }));
        }

        private void ContinueHandler(HtmlDocument htmlDoc, bool isContinue)
        {

            if (AppSettings.Dama2Enabled)
            {
                var form = htmlDoc.Forms["captchaForm"];

                var radio = form.GetElementsByTagName("input");

                if (LogInfoEventHandler != null)
                {
                    var msg = "處理驗證碼";
                    LogInfoEventHandler(msg, Color.Blue);
                }

                RepCode = GetVerifyCode(htmlDoc);
                if (RepCode != null)
                {
                    var code = RepCode.pred_reslt;
                    if (LogInfoEventHandler != null)
                    {
                        var msg = "驗證碼: " + code;
                        LogInfoEventHandler(msg, Color.Blue);
                    }

                    foreach (HtmlElement item in radio)
                    {
                        if (item.Name == "captchaAnswer")
                        {
                            var answer = item;

                            answer.SetAttribute("value", code);
                        }
                    }
                }
                else
                {
                    var answer = radio[isContinue ? 2 : 6];

                    answer.SetAttribute("value", "erro");
                }
                Thread.Sleep(1000);
                var a = form.GetElementsByTagName("a");
                var al = a[2];
                Thread.Sleep(2000);

                al.InvokeMember("click");
            }
            else
            {
                if (LogInfoEventHandler != null)
                {
                    var msg = "手動輸入驗證碼";
                    LogInfoEventHandler(msg, Color.Blue);
                }
                return;
            }
        }

        private void LotListHandler(HtmlDocument htmlDoc, WebBrowser wb)
        {
            int c = 0, d = 0;
            var srtTable = htmlDoc.GetElementById("sdrTable");
            var lotTable = htmlDoc.GetElementById("lotTable");
            HtmlElement activeTr = null;
            if (srtTable != null && lotTable != null)
            {
                var srtTr = srtTable.GetElementsByTagName("tr");
                c = srtTr.Count - 1;

                var lotTr = lotTable.GetElementsByTagName("tr");
                d = lotTr.Count - 1;

                CurrentItem.FieldName = "地段登記冊";
                if (CurrentItem.LotIdx < c)
                {
                    activeTr = srtTr[CurrentItem.LotIdx + 1];
                    CurrentItem.FieldName = "分層土地登記冊的參考地段";
                }
                else if (CurrentItem.LotIdx - c < d)
                {
                    activeTr = lotTr[CurrentItem.LotIdx + 1 - c];
                }
                else
                {
                    activeTr = lotTr[d];
                }
            }
            else if (srtTable != null)
            {
                var tr = srtTable.GetElementsByTagName("tr");
                c = tr.Count - 1;
                if (CurrentItem.LotIdx < c)
                {
                    activeTr = tr[CurrentItem.LotIdx + 1];
                }
                else
                {
                    activeTr = tr[c];
                }
                CurrentItem.FieldName = "分層土地登記冊的參考地段";
            }
            else if (lotTable != null)
            {
                var tr = lotTable.GetElementsByTagName("tr");
                d = tr.Count - 1;

                if (CurrentItem.LotIdx < d)
                {
                    activeTr = tr[CurrentItem.LotIdx + 1];
                }
                else
                {
                    activeTr = tr[d];
                }
                CurrentItem.FieldName = "地段登記冊";
            }
            else
            {
                RandomSleep();
                return;
            }

            CurrentItem.LotCount = c + d;

            if (CurrentItem.LotIdx >= CurrentItem.LotCount && CurrentItem.BlockIdx >= CurrentItem.BlockCount && CurrentItem.FloorIdx >= CurrentItem.FloorCount)
            {
                var tableCollection = htmlDoc.GetElementsByTagName("table");

                c = tableCollection.Count;

                var table = tableCollection[c - 2];

                var link = table.GetElementsByTagName("a");

                CurrentItem.LotIdx = 0;
                CurrentItem.LotCount = 0;

                CurrentItem.BlockIdx = 0;
                CurrentItem.BlockCount = 0;

                CurrentItem.FloorIdx = 0;
                CurrentItem.FloorCount = 0;

                CurrentItem.SetStatus();

                link[0].InvokeMember("click");
            }
            else
            {
                RandomSleep();
                var td = activeTr.GetElementsByTagName("td");

                if (CurrentItem.FieldName.ToString() == "地段登記冊")
                {

                    CurrentItem.Lot = ConvertUtility.Trim(td[0].InnerText);

                    CurrentItem.Lot = td[1].InnerText;

                    CurrentItem.HouseNo = string.Empty;
                    CurrentItem.StreetName = string.Empty;

                    LogInfoEventHandler("地段: " + CurrentItem.Lot, Color.Blue);
                }
                else
                {
                    CurrentItem.Lot = td[0].InnerText;
                    CurrentItem.Lot = string.Empty;
                    CurrentItem.HouseNo = td[1].InnerText;
                    CurrentItem.StreetName = td[2].InnerText;
                    LogInfoEventHandler("地段: " + CurrentItem.Lot, Color.Blue);
                }

                CurrentItem.Floor = string.Empty;

                CurrentItem.Block = string.Empty;

                CurrentItem.BlockDesc = string.Empty;

                td[1].InvokeMember("click");
            }
        }
        private void BlockListHandler(HtmlDocument htmlDoc, WebBrowser wb)
        {
            int c = 0, d = 0;
            var srtTable = htmlDoc.GetElementById("sdrTable");
            var lotTable = htmlDoc.GetElementById("lotTable");
            HtmlElement activeTr = null;

            if (srtTable != null && lotTable != null)
            {
                var srtTr = srtTable.GetElementsByTagName("tr");
                c = srtTr.Count - 1;

                var lotTr = lotTable.GetElementsByTagName("tr");
                d = lotTr.Count - 1;

                if (CurrentItem.BlockIdx < c)
                {
                    activeTr = srtTr[CurrentItem.BlockIdx + 1];
                    CurrentItem.FieldName = "分層土地登記冊的參考地段";
                }
                else if (CurrentItem.BlockIdx - c < d)
                {
                    activeTr = lotTr[CurrentItem.BlockIdx + 1 - c];
                    CurrentItem.FieldName = "地段登記冊";
                }
                else
                {
                    activeTr = lotTr[d];
                    CurrentItem.FieldName = "地段登記冊";
                }
            }
            else if (srtTable != null)
            {
                var tr = srtTable.GetElementsByTagName("tr");
                c = tr.Count - 1;
                if (CurrentItem.BlockIdx < c)
                {
                    activeTr = tr[CurrentItem.BlockIdx + 1];
                    CurrentItem.FieldName = "分層土地登記冊的參考地段";
                }
                else
                {
                    activeTr = tr[c];
                    CurrentItem.FieldName = "分層土地登記冊的參考地段";
                }
            }
            else if (lotTable != null)
            {
                var tr = lotTable.GetElementsByTagName("tr");
                d = tr.Count - 1;

                if (CurrentItem.BlockIdx < d)
                {
                    activeTr = tr[CurrentItem.BlockIdx + 1];
                    CurrentItem.FieldName = "地段登記冊";
                }
                else
                {
                    activeTr = tr[d];
                    CurrentItem.FieldName = "地段登記冊";
                }
            }
            else
            {
                RandomSleep();
                return;
            }

            CurrentItem.BlockCount = c + d;

            if (CurrentItem.BlockIdx >= CurrentItem.BlockCount && CurrentItem.FloorIdx >= CurrentItem.FloorCount)
            {
                var tableCollection = htmlDoc.GetElementsByTagName("table");

                c = tableCollection.Count;

                var table = tableCollection[c - 2];

                var link = table.GetElementsByTagName("a");

                CurrentItem.BlockIdx = 0;
                CurrentItem.BlockCount = 0;

                CurrentItem.FloorIdx = 0;
                CurrentItem.FloorCount = 0;

                CurrentItem.SetStatus();

                link[link.Count - 1].InvokeMember("click");

            }
            else
            {
                RandomSleep();
                var td = activeTr.GetElementsByTagName("td");
                CurrentItem.Block = td[0].InnerText;
                LogInfoEventHandler("座號: " + CurrentItem.Block, Color.Blue);
                if (CurrentItem.FieldName.ToString() == "地段登記冊")
                {

                }
                else
                {
                    CurrentItem.Lot = td[1].InnerText;
                    CurrentItem.BlockDesc = td[2].InnerText;
                }

                CurrentItem.Floor = string.Empty;

                td[1].InvokeMember("click");
            }
        }
        private void FloorListHandler(HtmlDocument htmlDoc, WebBrowser wb)
        {

            var table = htmlDoc.GetElementById("multiSelect");

            if (table == null)
            {
                RandomSleep();
                return;
            }

            var td = table.GetElementsByTagName("td");

            CurrentItem.FloorCount = 0;

            for (var i = 4; i < td.Count; i++)
            {
                if (!string.IsNullOrEmpty(ConvertUtility.Trim(td[i].InnerText)))
                {
                    CurrentItem.FloorCount++;
                }
            }

            if (CurrentItem.FloorIdx >= CurrentItem.FloorCount)
            {
                var tableCollection = htmlDoc.GetElementsByTagName("table");

                var c = tableCollection.Count;

                table = tableCollection[c - 2];

                var link = table.GetElementsByTagName("a");

                CurrentItem.FloorIdx = 0;
                CurrentItem.FloorCount = 0;

                CurrentItem.SetStatus();

                link[link.Count - 1].InvokeMember("click");

            }
            else
            {
                RandomSleep();
                var idx = CurrentItem.FloorIdx + 4;
                CurrentItem.Floor = ConvertUtility.Trim(td[idx].InnerText);
                LogInfoEventHandler("樓層: " + CurrentItem.Floor, Color.Blue);
                td[idx].InvokeMember("click");
            }
        }

        public void NotFound(object prn)
        {
            OrderItem dr = new OrderItem();
            dr.Machine = AppSettings.Machine;
            dr.Prn = prn.ToString();
            dr.Status = ProgressStatus.NotFound;
            DbService.UpdateOrder(dr);
        }
        private void SearchResultHandler(HtmlDocument htmlDoc, WebBrowser wb)
        {
            try
            {
                var table = htmlDoc.GetElementById("prnTable");

                if (table == null)
                {
                    if (LogInfoEventHandler != null)
                    {
                        msg = "当前Prn:" + CurrentItem.Keyword + "-找不到所需的土地登記冊";
                        LogInfoEventHandler(msg, Color.Blue);
                    }
                    RandomSleep();
                    NotFound(CurrentItem.Keyword);
                    Run(wb, -1);
                    return;
                }

                var tr = table.GetElementsByTagName("tr");

                try
                {
                    for (var i = 1; ; i++)
                    {
                        if (tr.Count == i + (i - 1) * 4)
                        {
                            break;
                        }

                        var r = tr[i + (i - 1) * 4];

                        if (r.InnerHtml.Contains("doSelectAll"))
                        {
                            break;
                        }

                        var td = r.GetElementsByTagName("td");

                        CurrentItem.Flat = td[1].InnerText;
                        CurrentItem.Prn = td[2].InnerText;
                        CurrentItem.IsClosed = CurrentItem.Prn.Contains("(Closed)").ToString();
                        CurrentItem.Prn = CurrentItem.Prn.Replace("(Closed)", string.Empty);

                        CurrentItem.AddressLot = td[3].InnerText;

                        CurrentItem.Save2Db();

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                RandomSleep();

                var tableCollection = htmlDoc.GetElementsByTagName("table");

                var c = tableCollection.Count;

                table = tableCollection[c - 4];

                var h = table.InnerHtml;

                var link = table.GetElementsByTagName("a");

                var idx = link.Count - 1;

                CurrentItem.FloorIdx++;

                if (CurrentItem.FloorIdx >= CurrentItem.FloorCount)
                {
                    CurrentItem.BlockIdx++;

                    if (CurrentItem.BlockIdx >= CurrentItem.BlockCount)
                    {
                        CurrentItem.LotIdx++;

                        if (CurrentItem.LotIdx >= CurrentItem.LotCount)
                        {
                            CurrentItem.Status = ProgressStatus.Done;
                            CurrentItem.NextSearch = true;
                            CurrentItem.Machine = AppSettings.Machine;
                            CurrentItem.UpdateStatus();

                            CurrentItem.FloorIdx = 0;
                            CurrentItem.FloorCount = 0;

                            CurrentItem.BlockIdx = 0;
                            CurrentItem.BlockCount = 0;

                            CurrentItem.LotIdx = 0;
                            CurrentItem.LotCount = 0;

                            link[0].InvokeMember("click");
                        }
                        else
                        {
                            link[idx].InvokeMember("click");
                        }
                    }
                    else
                    {
                        link[idx].InvokeMember("click");
                    }
                }
                else
                {
                    link[idx].InvokeMember("click");
                }

                CurrentItem.SetStatus();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void RandomSleep()
        {
            var r = new Random();
            var t = r.Next(2000, 5000);
            Thread.Sleep(t);
        }

        public Thread DocumentHandlerThread { get; set; }
        public WebBrowser SearchSuggestWb { get; set; }

        private FateadmRsp GetVerifyCode(HtmlDocument htmlDoc)
        {
            try
            {
                var imgElement = htmlDoc.Images["captchaImg"];

                if (imgElement != null)
                {
                    var imgSrc = imgElement.GetAttribute("src");

                    var doc = (HTMLDocument)htmlDoc.DomDocument;
                    var body = (HTMLBody)doc.body;
                    var rang = (IHTMLControlRange)body.createControlRange();
                    var img = (IHTMLControlElement)imgElement.DomElement;
                    rang.add(img);
                    rang.execCommand("Copy", false, null);

                    var image = Clipboard.GetImage();

                    if (image != null)
                    {
                        var imageName = AppSettings.VerifyImageName;

                        image.Save(imageName);
                        Thread.Sleep(2000);
                        var rs = FateadmApi.PredictFromFile(AppSettings.PredType, imageName);

                        if (File.Exists(imageName))
                        {
                            File.Delete(imageName);
                        }
                        return rs;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public void DocumentCompleted(WebBrowser wb)
        {
            try
            {
                var requestUrl = wb.Url.ToString();

                var htmlDoc = wb.Document;
                DocumentHandlerThread = new Thread(() =>
                {

                    var htmlBody = htmlDoc.Body;

                    if (htmlBody != null)
                    {
                        var msg = string.Empty;

                        var innerText = htmlBody.InnerText;

                        if (innerText == null)
                        {
                            return;
                        }
                        if (innerText.Contains("不可以同時在多個瀏覽視窗內下訂單") || innerText.Contains("Unusual behaviour at your browser is observed"))
                        {

                            Thread.Sleep(1300000);
                            Run(wb, 0);

                            return;
                        }
                        else if (requestUrl.Contains("eservices/common/selectuser.jsp"))
                        {
                            while (!isConnected)
                            {
                                msg = "Wait for VPN connection";
                                LogInfoEventHandler(msg, Color.DarkOrange);
                                Thread.Sleep(1000);
                            }
                            if (isConnected)
                            {
                                ContinueHandler(htmlDoc, true);
                            }
                        }
                        else if (requestUrl.Contains("eservices/searchlandregister/blocklist.jsp"))
                        {
                            if (LogInfoEventHandler != null)
                            {
                                msg = "当前Prn:" + CurrentItem.Keyword + "-處理座號";
                                LogInfoEventHandler(msg, Color.Blue);
                            }
                            BlockListHandler(htmlDoc, wb);
                        }
                        else if (requestUrl.Contains("eservices/searchlandregister/lotlist.jsp"))
                        {

                            if (LogInfoEventHandler != null)
                            {
                                msg = "当前Prn:" + CurrentItem.Keyword + "-處理地段";
                                LogInfoEventHandler(msg, Color.Blue);
                            }

                            LotListHandler(htmlDoc, wb);
                        }
                        else if (requestUrl.Contains("eservices/searchlandregister/floorlist.jsp"))
                        {

                            if (LogInfoEventHandler != null)
                            {
                                msg = "当前Prn:" + CurrentItem.Keyword + "-處理層數";
                                LogInfoEventHandler(msg, Color.Blue);
                            }

                            FloorListHandler(htmlDoc, wb);
                        }
                        else if (requestUrl.Contains("eservices/searchlandregister/result.jsp"))
                        {
                            if (LogInfoEventHandler != null)
                            {
                                msg = "当前Prn:" + CurrentItem.Keyword + "-處理结果";
                                LogInfoEventHandler(msg, Color.Blue);
                            }
                            SearchResultHandler(htmlDoc, wb);
                        }
                        else if (requestUrl.Contains("eservices/searchlandregister/search.jsp"))
                        {
                            var imgs = htmlDoc.GetElementsByTagName("li");
                            var src = 0;
                            foreach (HtmlElement item in imgs)
                            {
                                if (item.InnerText.Contains("找不到所需的土地登記冊") || item.InnerText.Contains("找不到所需的地段及分層土地登記冊"))
                                {
                                    src = 1;
                                }
                            }
                            if (src == 1)
                            {
                                if (LogInfoEventHandler != null)
                                {
                                    msg = "当前Prn:" + CurrentItem.Keyword + "-找不到所需的土地登記冊";
                                    LogInfoEventHandler(msg, Color.Blue);
                                }
                                NotFound(CurrentItem.Keyword);
                            }
                            if (LogInfoEventHandler != null)
                            {
                                msg = "距下條Prn執行時間" + ti / 60000 + "分鐘...";
                                LogInfoEventHandler(msg, Color.Blue);
                            }
                            Thread.Sleep(ti);

                            Run(wb, 0);
                        }
                        else
                        {

                        }
                    }
                });
                DocumentHandlerThread.SetApartmentState(ApartmentState.STA);
                DocumentHandlerThread.Start();
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
