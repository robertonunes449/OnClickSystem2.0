using System;
using System.Windows.Forms;

namespace OnClickSystem.Desktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // O programa começa AQUI e SÓ AQUI. Direto para o Login.
            System.Windows.Forms.Application.Run(new FormVideo());
        }
    }
}