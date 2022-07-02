using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JSRF_ModTool.DataFormats;
using JSRF_ModTool.DataFormats.JSRF;

namespace JSRF_ModTool
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern Boolean AllocConsole();
    }

    static class Program
    {
        // / <summary>
        // / The main entry point for the application.
        // / </summary>
        [STAThread]
        static void Main(string[] args)
        {

            // if no arguments, run as winforms
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
                return;
            }

            if (args[0] != "stage_compile") { return; }

            if (args.Length != 4)
            {
                MessageBox.Show("JSRF ModTool\n\n" + "Number of arguments is unexpected.\n\nExpected (4) arguments:\n\n" + "'stage_compile'  source_dir   media_dir   stg00");
                return;
            }

            Stage_Compiler Stage_compiler = new Stage_Compiler();
            // arguments: export_dir, media_dir, stage_num
            Stage_compiler.Compile(args[1], args[2], args[3]);
            System.Media.SystemSounds.Asterisk.Play();
        }
    }
}
