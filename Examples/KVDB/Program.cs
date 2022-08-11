using System;
using System.Net;

namespace KVDB;

class Program
{
    static void Main(string[] args)
    {
        Server server = new Server(IPAddress.Any, 5000);

        try
        {
            server.Start();
            Console.WriteLine("Server started.");
            Console.WriteLine("Press enter to stop the server.");
            Console.ReadLine();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e.Message);
        }
        finally
        {
            server.Stop();
            Console.WriteLine("Server stopped.");
        }

    }
}
