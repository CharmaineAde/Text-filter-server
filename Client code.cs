


using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class SimpleTcpClient
{
    static Socket server;
    static byte[] data = new byte[1024];
    static string input;

    public static void Main()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Connect(ipep);
            Console.WriteLine("Connected to the server.");
        }
        catch (SocketException e)
        {
            Console.WriteLine("Unable to connect to server.");
            Console.WriteLine(e.ToString());
            return;
        }

        // Receive welcome message from server
        int recv = server.Receive(data);
        Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

        // Start thread for receiving messages
        Thread receiveThread = new Thread(ReceiveMessage);
        receiveThread.Start();

        // Main thread for sending messages
        while (true)
        {
            input = Console.ReadLine();
            server.Send(Encoding.ASCII.GetBytes(input));
        }
    }

    static void ReceiveMessage()
    {
        while (true)
        {
            int recv = server.Receive(data);
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
        }
    }
}
