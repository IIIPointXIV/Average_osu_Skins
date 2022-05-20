using System;
using System.Windows.Forms;

namespace avg_osu_skins
{
    public class Program
    {
        public static Form1 form = new Form1();
        [STAThread]
        static void Main(string[] args)
        {
            form.RunForm();
            Application.Run(form);
        }
    }
}

