using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft {

    public class Program {

        public static void Main(string[] args) {

            Console.WriteLine(new String('-', Console.WindowWidth - 1));
            Loader.LoadingLogging += LoadingLogging;

            try { Loader.LoadPathes(); }
            catch (Exception e) { Console.WriteLine(new String('\t', Loader.Layer) + "LOADING ERROR: " + e.Message); Console.WriteLine(new String('-', Console.WindowWidth)); }

            Game G = new Game();
            G.LoadingLogging += LoadingLogging;
            G.Start();

            Console.ReadLine();
        }

        public static void LoadingLogging(string message, int layer) {

            Console.WriteLine(new String('\t', layer) + "LOADER: " + message);
            Console.WriteLine(new String('-', Console.WindowWidth - 1));
        }
    }
}
