/*
 * Hoang Pham
 * 4/2/2014
 * window phone controlled quad
 * 
 * */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices.Sensors;

using Windows.Networking.Sockets;
using Windows.Networking.Proximity;



using Microsoft.Xna.Framework;
using equilibrium.Resources;

using Windows.System.Threading;
using Windows.Devices.Geolocation;

using BluetoothConnectionManager;

using kuntakinte;
using TextureGraph;
using redbox;
using libvideo;
using Windows.Phone.Media.Capture;
using System.Windows.Resources;
using System.Windows.Media.Imaging;

using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;
using System.Windows.Threading;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;


namespace equilibrium
{
    public partial class MainPage : PhoneApplicationPage
    {

        flightbox mflightbox;
        btConManager mConManager;
        btConManager controllerManager;
        private StreamSocket s = null;
        DataReader input;
        bool connected = false;

        float roll;
        float pitch;
        float yaw;

        float myPgain = 0;
        float myIgain = 0;
        float myDgain = 0;

        //declare speechrecognizerUI 
        SpeechRecognizerUI recoWithUI;

        //timer
        DispatcherTimer timer;
        StreamSocket socket;

        //throttle
        float mthrottle;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //new bluetooth manager
            mConManager = new btConManager();
            controllerManager = new btConManager();
            s = new StreamSocket();

            mflightbox = new flightbox(); // initialize a new flightbox


            //mflightbox.inclineEvent += fb_inclineEvent;

            mflightbox.motorEvent += mflightbox_motorEvent;


            mConManager.Initialize();
            controllerManager.Initialize();



            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();
            mthrottle = 0;
            //Loaded += MainPage_Loaded;

            ReadData(SetupBluetoothLink());
           // controllerManager.MessageReceived += controllerManager_MessageReceived;

        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PeerFinder.ConnectionRequested +=PeerFinder_ConnectionRequested;
        }

        private void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            ConnectToPeer(args.PeerInformation);
        }

        async void ConnectToPeer(PeerInformation peerInformation)
        {
            socket = await PeerFinder.ConnectAsync(peerInformation);
        }

        void controllerManager_MessageReceived(string message)
        {
            //Dispatcher.BeginInvoke(() =>
           // {
            
                throttleText.Text = message;
           // });
        }

        async void mflightbox_motorEvent(int[] data)
        {
            //throw new NotImplementedException();
            //updateMotorDrive(data);
            await mConManager.SendCommand(data);
            byte[] read;

            double answer= 0;

            //if (connected)
               // answer = await controllerManager.recieveCommand();
            

            double throttle = 0.0;
            if (connected)
            {
                // controllerManager.recievemessages();
                // await input.LoadAsync(1);
                //throttle = input.ReadDouble();
            }

            if (socket != null)
            {
               // read = await ReadAsync(socket);

            }

            Dispatcher.BeginInvoke(() =>
            {


                motor0.Text = data[0].ToString();
                motor1.Text = data[1].ToString();
                motor2.Text = data[2].ToString();
                motor3.Text = data[3].ToString();
               // throttleText.Text = "Throttle: " + answer;
                //updateMotorDrive(data);
            });
            //updateMotorDrive(data[1]);
            //updateMotorDrive(data[2]);
            //updateMotorDrive(data[3]);
        }
        private static byte[] Read(StreamSocket sockets)
        {
            Task<byte[]> task = ReadAsync(sockets);
            task.Wait();
            return task.Result;
        }
        private static async Task<byte[]> ReadAsync(StreamSocket sockets)
        {
            // all responses are smaller that 1024
            IBuffer buffer = new byte[1024].AsBuffer();
            await sockets.InputStream.ReadAsync(
                    buffer, buffer.Capacity, InputStreamOptions.Partial);
            return buffer.ToArray();
        }

        void mflightbox_accelEvent(float[] __param0)
        {
            //updateMotorDrive(data);
            Dispatcher.BeginInvoke(() =>
            {

            });
        }

        private async void boutThatAction()
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            await synth.SpeakTextAsync("nah, I'm Just about that action baus");

        }

        private async void Listen()
        {
            this.recoWithUI = new SpeechRecognizerUI();

            SpeechRecognitionUIResult recoResult = await recoWithUI.RecognizeWithUIAsync();
            if (recoResult.ResultStatus == SpeechRecognitionUIStatus.Succeeded)
                MessageBox.Show(string.Format("You said {0}.",
                                               recoResult.RecognitionResult.Text));

        }

        private async void AppToDevice()
        {

            ConnectAppToDeviceButton.Content = "Connecting...";
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var pairedDevices = await PeerFinder.FindAllPeersAsync();

            if (pairedDevices.Count == 0)
            {
                MessageBox.Show("No paired devices were found.");
            }
            else
            {
                foreach (var pairedDevice in pairedDevices)
                {
                    if (pairedDevice.DisplayName == DeviceName.Text)
                    {
                        
                        mConManager.Connect(pairedDevice.HostName);
                        ConnectAppToDeviceButton.Content = "Connected";
                        DeviceName.IsReadOnly = true;
                        ConnectAppToDeviceButton.IsEnabled = false;
                        continue;
                    }
                }
            }
        }
        private async void AppToApp()
        {
            PeerFinder.Start();
            ConnectAppToAppButton.Content = "Connecting...";
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var pairedDevices = await PeerFinder.FindAllPeersAsync();

            if (pairedDevices.Count == 0)
            {
                MessageBox.Show("No paired devices were found.");
            }
            else
            {
                foreach (var pairedDevice in pairedDevices)
                {
                    if (pairedDevice.DisplayName == WindowsPhoneName.Text)
                    {
                        
                        controllerManager.Connect(pairedDevice.HostName);
                        //PeerInformation[] peers = pairedDevices.ToArray();
                        //PeerInformation peerInfo = pairedDevices.FirstOrDefault(c => c.DisplayName.Contains(WindowsPhoneName.Text));

                        //await s.ConnectAsync(peerInfo.HostName, "1");
                        ConnectAppToAppButton.Content = "Connected";
                        WindowsPhoneName.IsReadOnly = true;
                        ConnectAppToAppButton.IsEnabled = false;

                        //input = new DataReader(s.InputStream);
                        connected = true;
                        continue;
                    }
                }
            }
        }


        void fb_accelEvent(float[] data)
        {
            //updateMotorDrive(data);
            Dispatcher.BeginInvoke(() =>
            {

            });

        }

        private async void fb_inclineEvent(float[] data)
        {

            Dispatcher.BeginInvoke(() =>
            {
                roll = data[0];
                pitch = data[1];
                yaw = data[2];
                rollTextBlock.Text = roll.ToString("f2");
                pitchTextBlock.Text = pitch.ToString("f2");
                yawTextBlock.Text = yaw.ToString("f2");

            });

            //updateMotorDrive(roll);
            //updateMotorDrive(yaw);
        }


        private async void updateMotorDrive(int[] cmd)
        {
            await mConManager.SendCommand(cmd);
        }

        private void ConnectAppToDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            AppToDevice();
        }

        private async void Reco1_Click(object sender, RoutedEventArgs e)
        {
            //start recognition
            //SpeechRecognitionUIResult recoResult = await recoWithUI.RecognizeWithUIAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Reco1_Click(sender, e);
            //boutThatAction();
            timer.Start();
            //Listen();
        }

        private void throttleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mflightbox.throttle((float)e.NewValue);
        }

        void timer_Tick(object sender, EventArgs e)
        {

            mthrottle += 20;


            if (mthrottle > timedThrottle.Value)
            {
                mthrottle = 0;
                timer.Stop();
            }
            mflightbox.throttle(mthrottle);
        }

        private void timedThrottle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(pGain.Text, out myPgain) && float.TryParse(iGain.Text, out myIgain) && float.TryParse(dGain.Text, out myDgain))
            {
                mflightbox.rollPID(myPgain, myIgain, myDgain);
            }
        }

        private void ConnectAppToAppButton_Click(object sender, RoutedEventArgs e)
        {
            AppToApp();
        }


        private async Task<bool> SetupBluetoothLink()
        {
            // Tell PeerFinder that we're a pair to anyone that has been paried with us over BT
            PeerFinder.AlternateIdentities["Bluetooth:PAIRED"] = "";

            // Find all peers
            var devices = await PeerFinder.FindAllPeersAsync();

            // If there are no peers, then complain
            if (devices.Count == 0)
            {
                MessageBox.Show("No bluetooth devices are paired, please pair your FoneAstra");

                // Neat little line to open the bluetooth settings
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Convert peers to array from strange datatype return from PeerFinder.FindAllPeersAsync()
            PeerInformation[] peers = devices.ToArray();

            // Find paired peer that is the FoneAstra
            PeerInformation peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("Windows Phone"));

            // If that doesn't exist, complain!
            if (peerInfo == null)
            {
                MessageBox.Show("No paired FoneAstra was found, please pair your FoneAstra");
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Otherwise, create our StreamSocket and connect it!
            s = new StreamSocket();
            await s.ConnectAsync(peerInfo.HostName, "1");
            return true;
        }

        // Read in bytes one at a time until we get a newline, then return the whole line
        private async Task<string> readLine(DataReader input)
        {
            string line = "";
            char a = ' ';
            // Keep looping as long as we haven't hit a newline OR line is empty
            while ((a != '\n' && a != '\r') || line.Length == 0)
            {
                // Wait until we have 1 byte available to read
                await input.LoadAsync(1);
                // Read that one byte, typecasting it as a char
                a = (char)input.ReadByte();

                // If the char is a newline or a carriage return, then don't add it on to the line and quit out
                if (a != '\n' && a != '\r')
                    line += a;
            }

            // Return the string we've built
            return line;
        }

        private async void ReadData(Task<bool> setupOK)
        {
            // Wait for the setup function to finish, when it does, it returns a boolean
            // If the boolean is false, then something failed and we shouldn't attempt to read data
            if (!await setupOK)
                return;

            // Construct a dataReader so we can read junk in
            DataReader input = new DataReader(s.InputStream);

            // Loop forever
            while (true)
            {
                // Read a line from the input, once again using await to translate a "Task<xyz>" to an "xyz"
                string line = "";
                if (input != null)
                    line = (await readLine(input));

                // Append that line to our TextOutput
                throttleText.Text = line;
            }
        }

    }
}
