using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SimpleScan
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {

        TwainSession _twain;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetEntryAssembly());
            _twain = new TwainSession(appId);
            if (_twain.State < 3)
            {
                // use this for internal msg loop
                _twain.Open();
                // use this to hook into current app loop
                //_twain.Open(new WindowsFormsMessageLoopHook(this.Handle));
            }

            Console.WriteLine("State" + _twain.State);

            foreach (var src in _twain)
            {
                Console.WriteLine("Source"+ src.Name);
            }

            FillComboBox();
        }

        //protected override void OnHandleCreated(EventArgs e)
        //{
        //    base.OnHandleCreated(e);
        //    SetupTwain();

        //}


        private void FillComboBox()
        {
            Console.Write("Yo");

            Dictionary<int, string> spacecship = new Dictionary<int, string>();

            int index = 1;

            foreach (var src in _twain)
            {
                spacecship[index] = src.Name;
                Console.WriteLine("Source" + src.Name);
                index++;
            }

            //spacecship[1] = "Enterprise";
            //spacecship[2] = "Spitzer";
            //spacecship[3] = "WMAP";
            //spacecship[4] = "Spitzer";
            //spacecship[5] = "Casini";

            BindingSource bindingSource = new BindingSource(spacecship, null);
            metroComboBox1.DataSource = bindingSource;
            metroComboBox1.DisplayMember = "Value";
            metroComboBox1.ValueMember = "Value";
        }

        private void SetupTwain()
        {
            //var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetEntryAssembly());
            //_twain = new TwainSession(appId);
            _twain.StateChanged += (s, e) =>
            {
                PlatformInfo.Current.Log.Info("State changed to " + _twain.State + " on thread " + Thread.CurrentThread.ManagedThreadId);
            };
            _twain.TransferError += (s, e) =>
            {
                PlatformInfo.Current.Log.Info("Got xfer error on thread " + Thread.CurrentThread.ManagedThreadId);
            };
            _twain.DataTransferred += (s, e) =>
            {
                PlatformInfo.Current.Log.Info("Transferred data event on thread " + Thread.CurrentThread.ManagedThreadId);

                // example on getting ext image info
                var infos = e.GetExtImageInfo(ExtendedImageInfo.Camera).Where(it => it.ReturnCode == ReturnCode.Success);
                foreach (var it in infos)
                {
                    var values = it.ReadValues();
                    PlatformInfo.Current.Log.Info(string.Format("{0} = {1}", it.InfoID, values.FirstOrDefault()));
                    break;
                }

                Image img = null;
                if (e.NativeData != IntPtr.Zero)
                {
                    var stream = e.GetNativeImageStream();
                    if (stream != null)
                    {
                        var outPut = StreamToByte(stream);
                        //foreach (var socket in allSockets.ToList())
                        //{
                        //    socket.Send(outPut);
                        //}
                    }
                }
                else if (!string.IsNullOrEmpty(e.FileDataPath))
                {
                    img = new Bitmap(e.FileDataPath);
                }

            };
            _twain.SourceDisabled += (s, e) =>
            {
                PlatformInfo.Current.Log.Info("Source disabled event on thread " + Thread.CurrentThread.ManagedThreadId);
            };
            _twain.TransferReady += (s, e) =>
            {
                PlatformInfo.Current.Log.Info("Transferr ready event on thread " + Thread.CurrentThread.ManagedThreadId);
                //e.CancelAll = _stopScan;
            };

            // either set sync context and don't worry about threads during events,
            // or don't and use control.invoke during the events yourself
            PlatformInfo.Current.Log.Info("Setup thread = " + Thread.CurrentThread.ManagedThreadId);
            _twain.SynchronizationContext = SynchronizationContext.Current;
            if (_twain.State < 3)
            {
                // use this for internal msg loop
                _twain.Open();
                // use this to hook into current app loop
                //_twain.Open(new WindowsFormsMessageLoopHook(this.Handle));
            }
        }

        public static byte[] StreamToByte(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }


        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var src = metroComboBox1.SelectedItem as DataSource;
            //var src = _twain.CurrentSource;
            Console.WriteLine("State: "+_twain.State);
            Console.WriteLine("Source: "+src);
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("State: " + _twain.State);
            SetupTwain();
        }
    }
}
