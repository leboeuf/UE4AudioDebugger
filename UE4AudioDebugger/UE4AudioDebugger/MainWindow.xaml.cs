using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UE4AudioDebugger.Models;
using UE4AudioDebugger.Server;

namespace UE4AudioDebugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PORT = 8089;
        const int NB_UI_REFRESHES_PER_SECOND = 5;
        const string OUTPUT_WINDOW_DATETIME_FORMAT = "hh:mm:ss.fff";
        private UdpServer _udpServer;
        private CancellationTokenSource _processMessageQueueCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _UiRefreshLoopCancellationTokenSource = new CancellationTokenSource();

        private List<UActor> _actors = new List<UActor>();
        private StringBuilder _outputWindowBuffer = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
        }

        public void WriteToOutputWindow(string text)
        {
            outputWindow.AppendText(text);
            outputWindow.ScrollToEnd();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start server
            _udpServer = new UdpServer(PORT);
            _udpServer.Start();
            WriteToOutputWindow($"Server started on port {PORT}.");

            // Start processing messages
            var ct = _processMessageQueueCancellationTokenSource.Token;
            Task.Run(() => ProcessMessageQueue(ct), ct);

            // Start refreshing UI
            var ct2 = _UiRefreshLoopCancellationTokenSource.Token;
            Task.Run(() => UiRefreshLoop(ct2), ct2);
        }

        private void ProcessMessageQueue(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var hasMessage = _udpServer.MessageQueue.TryDequeue(out Message message);
                    if (!hasMessage)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    var data = Encoding.ASCII.GetString(message.MessageBytes).TrimEnd('\0').Substring(4); // Substring to workaround UE4 "Writer << message" issue
                    lock (_outputWindowBuffer)
                    {
                        _outputWindowBuffer.Append($"\n{message.Timestamp.ToString(OUTPUT_WINDOW_DATETIME_FORMAT)}: {data}");
                    }

                    // Parse message
                    var split = data.Split('=');
                    if (split.Length < 3) continue;
                    var name = split[0].Substring(0, split[0].Length - 1);
                    var x = Convert.ToDecimal(split[1].Substring(0, split[1].IndexOf(' ')));
                    var y = Convert.ToDecimal(split[2].Substring(0, split[2].IndexOf(' ')));
                    var z = Convert.ToDecimal(split[3]);

                    var actor = new UActor
                    {
                        Name = name,
                        Location = new Location { X = x, Y = y, Z = z },
                    };

                    _actors.Add(actor);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void UiRefreshLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Thread.Sleep(1000 / NB_UI_REFRESHES_PER_SECOND);

                    lock (_outputWindowBuffer)
                    {
                        Dispatcher.Invoke(() => WriteToOutputWindow(_outputWindowBuffer.ToString()), DispatcherPriority.Background);
                        _outputWindowBuffer.Clear();
                    }

                    Dispatcher.Invoke(() => canvas.InvalidateVisual(), DispatcherPriority.Render);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
