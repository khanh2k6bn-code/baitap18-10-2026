using System;
using System.Windows.Forms;

namespace GymManagerApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Khởi chạy Form Quản lý phòng gym
            Application.Run(new frmGymMemberManager());
        }
    }
}
