using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace DDlotSpider.Service
{
    public static class FateadmApi
    {
        static string app_id = AppSettings.AppId;
        static string app_key = AppSettings.AppKey;
        static string usr_id = AppSettings.PdId;
        static string usr_key = AppSettings.PdKey;


        public static FateadmRsp QueryRsp()
        {
            FateadmRsp rsp = new FateadmRsp();

            Util.QueryBalc(rsp, usr_id, usr_key);
            return rsp;
        }
        public static FateadmRsp Predict(string pred_type, byte[] img_data)
        {
            FateadmRsp rsp = new FateadmRsp();

            Util.Predict(rsp, app_id, app_key, usr_id, usr_key, pred_type, img_data);
            return rsp;
        }
        public static FateadmRsp PredictFromFile(string pred_type, string file_name)
        {
            FateadmRsp rsp = new FateadmRsp();

            Util.PredictFromFile(rsp, app_id, app_key, usr_id, usr_key, pred_type, file_name);
            return rsp;
        }
        public static FateadmRsp Justice(string order_id)
        {
            FateadmRsp rsp = new FateadmRsp();

            Util.Justice(rsp, usr_id, usr_key, order_id);
            return rsp;
        }


        public class FateadmRsp
        {
            //操作返回码，0为正常，其他为异常情况，异常原因保存在err_msg中
            public int ret_code;
            // 保存异常原因
            public string err_msg;
            // 如果为识别操作，此处保存识别结果
            // ret_code 不等于0时，pred_reslt 为空串
            public string pred_reslt;
            // 订单号
            public string order_id;
            // 余额查询时，此处得到用户的余额信息
            public double cust_val;
        }
        public class HttpExtraInfo
        {
            public double cust_val;
            public string result;
        }
        public class HttpRspData
        {
            public string RetCode;
            public string ErrMsg;
            public string RequestId;
            public string RspData;
            public HttpExtraInfo einfo;
        }

        public class JsonPaserWeb
        {
            // Object->Json  
            public static string Serialize<T>(T obj)
            {
                string json = JsonConvert.SerializeObject(obj);
                return json;
            }

            public static T Deserialize<T>(string json)
            {
                T obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
        }
        class Util
        {
            private static string URL = "http://pred.fateadm.com";
            public static string Md5(string src)
            {
                string str = "";
                byte[] data = Encoding.GetEncoding("utf-8").GetBytes(src);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(data);
                for (int i = 0; i < bytes.Length; i++)
                {
                    str += bytes[i].ToString("x2");
                }
                return str;
            }
            public static string CalcSign(string id, string key, string tm)
            {
                string chk = Md5(tm + key);
                string sum = Md5(id + tm + chk);
                //Console.WriteLine("calc sign, id: {0} key: {1} tm: {2} chk: {3} sum: {4}", id, key, tm, chk, sum);
                return sum;
            }
            /// <summary>  
            /// 创建POST方式的HTTP请求  
            /// </summary>   
            private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                return true; //总是接受     
            }
            //public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int timeout, string userAgent, CookieCollection cookies)
            public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, Encoding charset)
            {
                HttpWebRequest request = null;
                //HTTPSQ请求  
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
              //  request.Proxy = null;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
                //如果需要POST數据     
                if (!(parameters == null || parameters.Count == 0))
                {
                    StringBuilder buffer = new StringBuilder();
                    int i = 0;
                    foreach (string key in parameters.Keys)
                    {
                        if (i > 0)
                        {
                            buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                        }
                        else
                        {
                            buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        }
                        i++;
                    }
                    byte[] data = charset.GetBytes(buffer.ToString());
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                return request.GetResponse() as HttpWebResponse;
            }
            public static HttpRspData HttpPost(string url, IDictionary<string, string> param)
            {
                //string url = "";
                Encoding charset = Encoding.GetEncoding("utf-8");
                //IDictionary<string, string> param = new Dictionary<string, string>();
                //param.Add("usrid", "10000");
                HttpWebResponse resp = CreatePostHttpResponse(url, param, charset);
                Stream stream = resp.GetResponseStream();   //获取响应的字符串流  
                StreamReader sr = new StreamReader(stream); //创建一个stream读取流  
                string html = sr.ReadToEnd();   //从头读到尾，放到字符串html  
                                                //Console.WriteLine(html);
                HttpRspData data = JsonPaserWeb.Deserialize<HttpRspData>(html);
                if (!string.IsNullOrEmpty(data.RspData))
                {
                    // 附带附加信息
                    HttpExtraInfo einfo = JsonPaserWeb.Deserialize<HttpExtraInfo>(data.RspData);
                    data.einfo = einfo;
                }
                return data;
            }
            public static string GetCurrentTimeUnix()
            {
                TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
                long t = (long)cha.TotalSeconds;
                return t.ToString();
            }
            public static void QueryBalc(FateadmRsp rsp, string usr_id, string usr_key)
            {
                string cur_tm = GetCurrentTimeUnix();
                string sign = CalcSign(usr_id, usr_key, cur_tm);
                IDictionary<string, string> param = new Dictionary<string, string>();
                param.Add("user_id", usr_id);
                param.Add("timestamp", cur_tm);
                param.Add("sign", sign);
                string url = URL + "/api/custval";
                HttpRspData jrsp = HttpPost(url, param);
                rsp.ret_code = int.Parse(jrsp.RetCode);
                rsp.err_msg = jrsp.ErrMsg;
                if (rsp.ret_code == 0)
                {
                    rsp.cust_val = jrsp.einfo.cust_val;
                }
            }
            public static void Justice(FateadmRsp rsp, string usr_id, string usr_key, string order_id)
            {
                string cur_tm = GetCurrentTimeUnix();
                string sign = CalcSign(usr_id, usr_key, cur_tm);
                IDictionary<string, string> param = new Dictionary<string, string>();
                param.Add("user_id", usr_id);
                param.Add("timestamp", cur_tm);
                param.Add("sign", sign);
                param.Add("request_id", order_id);
                string url = URL + "/api/capjust";
                HttpRspData jrsp = HttpPost(url, param);
                rsp.ret_code = int.Parse(jrsp.RetCode);
                rsp.err_msg = jrsp.ErrMsg;
            }
            public static void Charge(FateadmRsp rsp, string usr_id, string usr_key, string cardid, string cardkey)
            {
                string cur_tm = GetCurrentTimeUnix();
                string sign = CalcSign(usr_id, usr_key, cur_tm);
                string csign = Md5(usr_key + cur_tm + cardid + cardkey);
                IDictionary<string, string> param = new Dictionary<string, string>();
                param.Add("user_id", usr_id);
                param.Add("timestamp", cur_tm);
                param.Add("sign", sign);
                param.Add("cardid", cardid);
                param.Add("csign", csign);
                string url = URL + "/api/charge";
                HttpRspData jrsp = HttpPost(url, param);
                rsp.ret_code = int.Parse(jrsp.RetCode);
                rsp.err_msg = jrsp.ErrMsg;
                rsp.order_id = jrsp.RequestId;
            }
            public static void PredictFromFile(FateadmRsp rsp, string app_id, string app_key, string usr_id, string usr_key, string pred_type, string file_name)
            {
                byte[] img_data;
                try
                {
                    FileStream fs = new FileStream(file_name, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    img_data = br.ReadBytes((int)fs.Length);
                    br.Close();
                    br.Dispose();
                    fs.Close();
                    fs.Dispose();
                }
                catch (Exception ex)
                {
                    rsp.ret_code = -1;
                    rsp.err_msg = "文件读取失败，请检查文件路径, err: " + ex.ToString();
                    return;
                }
                Predict(rsp, app_id, app_key, usr_id, usr_key, pred_type, img_data);
            }
            public static void Predict(FateadmRsp rsp, string app_id, string app_key, string usr_id, string usr_key, string pred_type, byte[] img_data)
            {
                string cur_tm = GetCurrentTimeUnix();
                string sign = CalcSign(usr_id, usr_key, cur_tm);
                IDictionary<string, string> param = new Dictionary<string, string>();
                param.Add("user_id", usr_id);
                param.Add("timestamp", cur_tm);
                param.Add("sign", sign);
                if (!string.IsNullOrEmpty(app_id))
                {
                    string asign = CalcSign(app_id, app_key, cur_tm);
                    param.Add("appid", app_id);
                    param.Add("asign", asign);
                }
                param.Add("predict_type", pred_type);               
                string b64data = Convert.ToBase64String(img_data, 0, img_data.Length);
                param.Add("img_data", HttpUtility.UrlEncode(b64data));
                string url = URL + "/api/capreg";
                HttpRspData jrsp = HttpPost(url, param);
                rsp.ret_code = int.Parse(jrsp.RetCode);
                rsp.err_msg = jrsp.ErrMsg;
                rsp.order_id = jrsp.RequestId;
                if (rsp.ret_code == 0)
                {
                    rsp.pred_reslt = jrsp.einfo.result;
                }
            }
        }
    }
}
