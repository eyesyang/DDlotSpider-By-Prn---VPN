using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotRas;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace ConnectVPN
{
    class VPN : IDisposable
    {
        private bool disposed = false;

        string ConnectionName = "VPN";
        bool isConnected = false;
        RasConnection conn = null;
        bool isLimitedToJapan = false;

        public VPN()
        {
            isLimitedToJapan = false;
        }
        public VPN(bool _islimitedToJP)
        {
            isLimitedToJapan = _islimitedToJP;
        }

        public bool ConnectVPN()
        {
            try
            {
                DisconnectVPN();

                string _server = GetVPNServers(isLimitedToJapan);

                conn = RasConnection.GetActiveConnections().Where(c => c.EntryName == ConnectionName).FirstOrDefault();

                if (conn != null)
                {
                    conn.HangUp();

                    Console.WriteLine("VPN Disconnected!");
                }

                Master(_server);
                RasDialer dialer = new RasDialer();
                dialer.EntryName = ConnectionName;
                dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);
                dialer.Credentials = new NetworkCredential("vpn", "vpn");

                //dialer.EapOptions = new DotRas.RasEapOptions(false, false, false);
                //dialer.HangUpPollingInterval = 0;
                //dialer.Options = new DotRas.RasDialOptions(false, false, false, false, false, false, false, false, false, false);

                dialer.DialCompleted += new System.EventHandler<DialCompletedEventArgs>(dialer_DialCompleted);
                dialer.StateChanged += new System.EventHandler<StateChangedEventArgs>(dialer_StateChanged);

                //dialer.DialAsync();
                dialer.Dial();

                //Console.ReadLine();

                //DisplayIPAddresses();

                //Console.ReadLine();
                isConnected = true;
            }
            catch
            {
                isConnected = false;
                Dispose();
            }
            return isConnected;
        }

        public void ConnectVPN_Async()
        {
            try
            {
                DisconnectVPN();

                string _server = GetVPNServers(isLimitedToJapan);

                conn = RasConnection.GetActiveConnections().Where(c => c.EntryName == ConnectionName).FirstOrDefault();

                if (conn != null)
                {
                    conn.HangUp();

                    Console.WriteLine("VPN Disconnected!");
                }

                Master(_server);
                RasDialer dialer = new RasDialer();
                dialer.EntryName = ConnectionName;
                dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);
                dialer.Credentials = new NetworkCredential("vpn", "vpn");

                //dialer.EapOptions = new DotRas.RasEapOptions(false, false, false);
                //dialer.HangUpPollingInterval = 0;
                //dialer.Options = new DotRas.RasDialOptions(false, false, false, false, false, false, false, false, false, false);

                dialer.DialCompleted += new System.EventHandler<DialCompletedEventArgs>(dialer_DialCompleted);
                dialer.StateChanged += new System.EventHandler<StateChangedEventArgs>(dialer_StateChanged);

                dialer.DialAsync();
                
                //Console.ReadLine();

                //DisplayIPAddresses();

                //Console.ReadLine();
                
            }
            catch
            {
                
                //Dispose();
            }
            
        }

        public string ConnectionStatus()
        {
            string returnval = isConnected ? "Connected" : "Disconnected";
            return returnval;
        }

        public string CheckConnection()
        {
            string returnval = isConnected ? "Connected" : "Disconnected";
            Console.WriteLine("VPN connected? {0} with IP: {1}", isConnected, GetIP());
            return returnval;
        }

        public bool DisconnectVPN()
        {
            bool isSuccess = false;
            try
            {
                conn = RasConnection.GetActiveConnections().Where(c => c.EntryName == ConnectionName).FirstOrDefault();

                if (conn != null)
                {
                    conn.HangUp();

                    Console.WriteLine("VPN Disconnected!");
                    isConnected = false;
                }

                ClearPhoneBook();

                isSuccess = true;
            }
            catch
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        private void dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            if (isConnected)
                Console.WriteLine("Dial Completed");
            else
                Console.WriteLine("Dail Failed");
        }

        private void dialer_StateChanged(object sender, StateChangedEventArgs e)
        {
            Console.WriteLine("Status Changed: {0}", e.State);
            if (e.State == RasConnectionState.Connected)
            {
                isConnected = true;
            }
        }

        private void ClearPhoneBook()
        {
            string path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);
            RasPhoneBook rpb = new RasPhoneBook();
            rpb.Open(path);


            // Check for existance of the same connection
            if (rpb.Entries.Contains(ConnectionName))
            {
                rpb.Entries.Remove(ConnectionName);
            }
        }
        private void DisplayIPAddresses()
        {
            StringBuilder sb = new StringBuilder();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network 
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses 
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now 
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1) 
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")");
                }
            }

            Console.WriteLine(sb.ToString());
        }

        public string GetIP()
        {
            string externalIP = "";
            try
            {              
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new System.Text.RegularExpressions.Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
           
            }
            catch
            {
                externalIP = "";
            }
            return externalIP;
        }

        private void Master(string _server)
        {
            // Connection Parameters
            string ServerAddress = _server;
            RasVpnStrategy strategy = RasVpnStrategy.SstpOnly;
            RasDevice device = RasDevice.Create("SSTP", RasDeviceType.Vpn);
            bool useRemoteDefaultGateway = true;

            // Create entry
            RasEntry entry = RasEntry.CreateVpnEntry(ConnectionName, ServerAddress, strategy, device, false);

            entry.DnsAddress = IPAddress.Parse("8.8.8.8");
            //entry.IPAddress = IPAddress.Parse("219.100.37.219");


            entry.EncryptionType = RasEncryptionType.Require;
            entry.EntryType = RasEntryType.Vpn;
            entry.Options.RequireDataEncryption = false;
            entry.Options.UseLogOnCredentials = false;
            entry.Options.RequireMSChap2 = false;
            entry.Options.RemoteDefaultGateway = useRemoteDefaultGateway;
            entry.Options.SecureFileAndPrint = true;
            entry.Options.SecureClientForMSNet = true;
            entry.Options.ReconnectIfDropped = false;
            //entry.Options.RegisterIPWithDns = true;


            // Get phone book (list of connetions) path
            string path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);

            // Load
            RasPhoneBook rpb = new RasPhoneBook();
            rpb.Open(path);
            
            // Check for existance of the same connection
            if (!rpb.Entries.Contains(entry.Name))
            {
                rpb.Entries.Remove(entry.Name);
            }
            rpb.Entries.Add(entry);
            // Set user and password
            entry.ClearCredentials();
        }

        internal class cls_Server
        {
            public string country { get; set; }
            public string server { get; set; }
        }

        private string GetVPNServers(bool limitedToJP)
        {
            var url = "https://www.vpngate.net/cn/";

            //var url = "http://vpn-free.duckdns.org/ms-sstp";

            var client = new WebClient();

            client.Headers[HttpRequestHeader.ContentType] = "text/html; charset=utf-8";

            var html = client.DownloadString(url);

            //var regex = new Regex(@"<tr.*?><td.*?>(.+?)</td><td.*?><u>(.+?)</u><br/>(.+?)</td>");

            var regex = new Regex(@"<tr.*?>\s*<td.*?><img.*?/><br>(.*?)</td>\s*<td.*?>\s*<td.*?>\s*<td.*?>\s*<td.*?>\s*<td.*?>\s*<td.*?>\s*<td.*?>.+?<span.+?>.+?<span.+?>(.+?)</span>.+?\s*<td.*?>\s*<td.*?>");

            var matches = regex.Matches(html);

            List<cls_Server> list = new List<cls_Server>();

            foreach (Match m in matches)
            {
                string country = m.Groups[1].Value.ToUpper();
                string server = m.Groups[2].Value;

                cls_Server servercls = new cls_Server();
                servercls.country = country;
                servercls.server = server;

                list.Add(servercls);

                //Console.WriteLine(string.Format("Country: {0} SSTP: {1}", country, server));
            }
            List<cls_Server> shuffled_list = new List<cls_Server>();

            if (limitedToJP)
                shuffled_list = list.Where(a=> a.country.Equals("JAPAN") && !a.server.Contains("public-vpn-")).OrderBy(a => Guid.NewGuid()).ToList();
            else
                //shuffled_list = list.Where(a=> !a.server.Contains(":")).OrderBy(a => Guid.NewGuid()).ToList();
                shuffled_list = list.OrderBy(a => Guid.NewGuid()).ToList();

            Console.WriteLine(string.Format("Country: {0} SSTP: {1}", shuffled_list[0].country, shuffled_list[0].server));
            return shuffled_list[0].server;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The virtual dispose method that allows
        /// classes inherithed from this one to dispose their resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here.
                    DisconnectVPN();
                    ClearPhoneBook();
                }

                // Dispose unmanaged resources here.
            }

            disposed = true;
        }

    }
}
