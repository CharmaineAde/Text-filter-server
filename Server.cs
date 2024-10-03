using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

class SimpleTcpSrvr
{
    static Socket clientSocket1;
    static Socket clientSocket2;

    public static void Main()
    {
        // Set up server socket
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 9050);

        serverSocket.Bind(endPoint);
        serverSocket.Listen(10);

        Console.WriteLine("Waiting for clients...");

        // Accept first client
        clientSocket1 = serverSocket.Accept();
        IPEndPoint clientEndPoint1 = (IPEndPoint)clientSocket1.RemoteEndPoint;
        Console.WriteLine("Connected with {0} at port {1}", clientEndPoint1.Address, clientEndPoint1.Port);

        // Send welcome message to client 1
        string welcomeMessage1 = "Welcome to the server. You are client 1.";
        byte[] welcomeData1 = Encoding.ASCII.GetBytes(welcomeMessage1);
        clientSocket1.Send(welcomeData1, welcomeData1.Length, SocketFlags.None);

        // Accept second client
        clientSocket2 = serverSocket.Accept();
        IPEndPoint clientEndPoint2 = (IPEndPoint)clientSocket2.RemoteEndPoint;
        Console.WriteLine("Connected with {0} at port {1}", clientEndPoint2.Address, clientEndPoint2.Port);

        // Send welcome message to client 2
        string welcomeMessage2 = "Welcome to the server. You are client 2.";
        byte[] welcomeData2 = Encoding.ASCII.GetBytes(welcomeMessage2);
        clientSocket2.Send(welcomeData2, welcomeData2.Length, SocketFlags.None);

        // Handle clients in separate threads
        Thread clientThread1 = new Thread(() => HandleClient(clientSocket1, clientSocket2));
        clientThread1.Start();

        Thread clientThread2 = new Thread(() => HandleClient(clientSocket2, clientSocket1));
        clientThread2.Start();
    }

    static void HandleClient(Socket clientSocket, Socket otherClientSocket)
    {
        int recv;
        byte[] data = new byte[1024];

        while (true)
        {
            data = new byte[1024];
            recv = clientSocket.Receive(data);
            if (recv == 0)
                break;

            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
            string filteredMessage = FilterBadWords(receivedMessage);
            Console.WriteLine(filteredMessage);

            // Check if the message is blocked due to containing four or more bad words
            bool messageBlocked = filteredMessage == "Bad words message has been blocked";

            // Send feedback to both clients if the message is blocked
            if (messageBlocked)
            {
                byte[] feedbackData = Encoding.ASCII.GetBytes(filteredMessage);
                clientSocket.Send(feedbackData, feedbackData.Length, SocketFlags.None);
                otherClientSocket.Send(feedbackData, feedbackData.Length, SocketFlags.None);
                Console.WriteLine("Feedback sent to both clients: " + filteredMessage);
            }
            else
            {
                // Forward message to the other client
                byte[] filteredData = Encoding.ASCII.GetBytes(filteredMessage);
                otherClientSocket.Send(filteredData, filteredData.Length, SocketFlags.None);
            }
        }

        IPEndPoint clientEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
        Console.WriteLine("Disconnected from {0}", clientEndPoint.Address);
        clientSocket.Close();
    }

    static string FilterBadWords(string message)
    {
        // Dictionary to store the count of each bad word
        Dictionary<string, int> badWordCounts = new Dictionary<string, int>();

        // Read bad words from file
        string filePath = "badwords.txt";
        string[] badWords = File.ReadAllLines(filePath);

        foreach (string badWord in badWords)
        {
            // Count occurrences of each bad word in the message
            int count = Regex.Matches(message, "\\b" + badWord + "\\b").Count;
            badWordCounts[badWord] = count;
        }

        foreach (var entry in badWordCounts)
        {
            string badWord = entry.Key;
            int count = entry.Value;

            // If any bad word occurs four or more times, block the message
            if (count >= 4)
            {
                return "Bad words message has been blocked";
            }

            // Replace bad words with asterisks
            string filteredWord = new string('*', badWord.Length);
            message = message.Replace(badWord, filteredWord);
        }

        return message;
    }
}