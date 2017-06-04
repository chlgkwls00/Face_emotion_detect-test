using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reconocimiento_facial
{
    static class Program
    {
        static DateTime date { get; }
    
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Reconocimiento_facial.Reconocimiento());
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR!!::" + ex);
            }

        }
    }
}
