using System;
using System.Diagnostics;
using System.ServiceProcess;
using static System.Net.Mime.MediaTypeNames;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        Process frp_process = null;
        protected override void OnStart(string[] args)
        {

            bool systemType = Environment.Is64BitOperatingSystem;
            if (systemType)//64bit
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "baseDir\\frp");
            }
            else//32bit
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "baseDir\\frp32bit");
            }
            
            wLog("Service Start");
            startFrp();
        }

        protected override void OnStop()
        {
            CloseFrp();
            wLog("Service Stop\n");
        }
        private void startFrp(Object sender = null, EventArgs e = null)
        {
            wLog("Run frpc.exe");
            frp_process = new Process();
            frp_process.StartInfo.FileName = "frpc.exe";
            frp_process.StartInfo.Arguments = " -c frpc.ini";
            frp_process.StartInfo.CreateNoWindow = true;
            frp_process.StartInfo.UseShellExecute = false;
            // 守护重启
            frp_process.EnableRaisingEvents = true;
            frp_process.Exited += new EventHandler(startFrp);
            // 进程输出
            frp_process.StartInfo.RedirectStandardOutput = true;
            frp_process.StartInfo.RedirectStandardError = true;
            frp_process.OutputDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            frp_process.ErrorDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            frp_process.Start();
            frp_process.BeginOutputReadLine();
            frp_process.BeginErrorReadLine();
        }
        private void CloseFrp()
        {
            wLog("Kill frpc.exe");
            if (null == frp_process)
                return;
            frp_process.Kill();
            frp_process.Close();
            frp_process = null;
        }
        private void wLog(string logStr, bool wTime = true)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\FrpWindowsServiceAutoService.log", true))
            {
                string timeStr = wTime == true ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") : "";
                sw.WriteLine(timeStr + logStr);
            }
        }

        private void MyProcOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                wLog(outLine.Data, false);
            }
        }
    }
}
