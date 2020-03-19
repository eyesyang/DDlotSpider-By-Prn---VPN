using DDlotSpider.Service;
using System;
using System.Drawing;
using System.Windows.Forms;
using ConnectVPN;
using System.Threading;

namespace DDlotSpider
{
    public delegate void LogInfoDelegate(string msg, Color color);
    public partial class Main : Form
    {
        private bool _IsInit = true;
        VPN vpn = new VPN();
        Thread main_thread;
        public Main()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
        }
        private OrderHandler _OrderHandler = new OrderHandler();
        private void Main_Load(object sender, EventArgs e)
        {
            _OrderHandler.LogInfoEventHandler += Main_logInfoEvent;

            main_thread = new Thread(new ThreadStart(CheckConnectionStatus));
            main_thread.Start();

        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (main_thread != null)
            {
                main_thread.Abort();
                main_thread = null;
            }
            vpn.DisconnectVPN();
            this.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var bro = (WebBrowser)sender;

            if (bro.ReadyState != WebBrowserReadyState.Complete)
            {
                return;
            }
            if (_IsInit)
            {
                _IsInit = false;
                _OrderHandler.Run(bro, 1);

            }
            _OrderHandler.DocumentCompleted(bro);
        }

        private void Main_logInfoEvent(string msg, Color color)
        {
            try
            {
                if (!LogTxtBox.IsDisposed)
                {
                    if (LogTxtBox.Lines.Length > 1000)
                    {
                        LogTxtBox.Clear();
                    }
                    var text = string.Format("[{0}] {1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg, Environment.NewLine);

                    var length = text.Length;
                    var loglength = LogTxtBox.Text.Length;
                    LogTxtBox.Select(loglength, length);
                    LogTxtBox.AppendText(text);
                    LogTxtBox.SelectionColor = color;
                    LogTxtBox.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //add thread function to keep checking connection status
        private void CheckConnectionStatus()
        {
            bool isConnected = false;
            while (main_thread != null)
            {
                _OrderHandler.isConnected = isConnected = vpn.ConnectionStatus().Equals("Connected");
                if (!isConnected)
                {
                    btnVPN_Click(null, null);
                }
                Thread.Sleep(1000);
            }
        }

        private void btnVPN_Click(object sender, EventArgs e)
        {
            bool isConnected = false;
            if (btnVPN.Text.Equals("Connect"))
            {
                btnVPN.Enabled = false;
                vpn.ConnectVPN_Async();


                while (!isConnected)
                {
                    isConnected = vpn.ConnectionStatus().Equals("Connected");
                    //Main_logInfoEvent(string.Format("VPN Status: {0}", isConnected), Color.BlueViolet);
                    Thread.Sleep(100);
                }

                if (isConnected)
                {
                    _OrderHandler.isConnected = isConnected;
                    string ip = vpn.GetIP();
                    lblIP.Text = ip;
                    lblVPNStatus.Text = vpn.ConnectionStatus();
                    btnVPN.Text = "Disconnect";
                    btnVPN.Enabled = isConnected;
                    //Main_logInfoEvent(string.Format("VPN IP address: {0}", ip), Color.BlueViolet);
                }
            }
            else
            {
                btnVPN.Enabled = false;
                isConnected = !vpn.DisconnectVPN();
                if (!isConnected)
                {
                    string ip = vpn.GetIP();
                    lblIP.Text = ip;
                    lblVPNStatus.Text = vpn.ConnectionStatus();
                    btnVPN.Text = "Connect";
                    btnVPN.Enabled = true;
                    _OrderHandler.isConnected = isConnected;
                }
            }

        }
    }
}
