using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackboardREST;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var myApp = new RestApp("https://mcphs.blackboard.com", "abcdefg", "hijklmn");
            Console.ReadLine();
        }
    }
}
