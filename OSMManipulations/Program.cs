using MapsOperations;
using System;

namespace OSMManipulations
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Controller controller = new Controller();
            await controller.Do();
            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
