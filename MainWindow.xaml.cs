﻿using SharpPcap;
using SharpPcap.LibPcap;
using System.Text;
using System.Windows;
using PacketDotNet;

namespace client_packet_detection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string[] keywords = [
            "HelloWorld",
            "Hello",
            "YES",
        ];

        public MainWindow()
        {
            InitializeComponent();

            var device = LibPcapLiveDeviceList.Instance[4];
            Console.WriteLine(device.ToString());
            device.Open();
            device.OnPacketArrival += Device_OnPacketArrival;
            device.StartCapture();
        }

        void Device_OnPacketArrival(object s, PacketCapture e)
        {
            byte[] rawBytes = e.GetPacket().Data;
            Packet packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, rawBytes);

            EthernetPacket? ether = packet.Extract<EthernetPacket>();
            if (ether == null) return;
            IPPacket? ip = packet.Extract<IPPacket>();
            if (ip == null) return;
            TcpPacket? tcp = packet.Extract<TcpPacket>();
            if (tcp == null) return;

            byte[] payload = tcp.PayloadData;
            if (payload == null || payload.Length <= 0) return;

            string text = Encoding.ASCII.GetString(rawBytes);

            foreach (string keyword in keywords)
            {
                if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) continue;

                Console.WriteLine($"Detected Keyword: {keyword}");
                Console.WriteLine(text);
                Console.WriteLine("======================");
                break;
            }
        }
    }
}