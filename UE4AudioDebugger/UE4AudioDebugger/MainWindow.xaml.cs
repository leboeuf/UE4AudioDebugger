using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UE4AudioDebugger.Server;

namespace UE4AudioDebugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PORT = 8089;
        const string OUTPUT_WINDOW_DATETIME_FORMAT = "hh:mm:ss.fff";
        private UdpServer _udpServer;
        private CancellationTokenSource _processMessageQueueCancellationTokenSource = new CancellationTokenSource();

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
        }

        private void ProcessMessageQueue(CancellationToken ct)
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
                Dispatcher.Invoke(() => WriteToOutputWindow($"\n{message.Timestamp.ToString(OUTPUT_WINDOW_DATETIME_FORMAT)}: {data}"), DispatcherPriority.Background);
            }
        }
    }
}
