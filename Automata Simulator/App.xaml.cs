using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.IO;

namespace Automata_Simulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            try // for capturing and ignoring FileNotFoundException
            {
                if (e.Args.Length != 0)
                {
                    mainWindow.menuLoad_Click(e.Args[0], null);
                    mainWindow.isFiringFromStartScreen = false;
                }
                else
                    mainWindow.isFiringFromStartScreen = true;

            }
            catch(FileNotFoundException)
            {
                MessageBox.Show("The specified file do not exist");
            }
            //catch(Exception)
            //{
            //    MessageBox.Show("An unknown Error has occured, Automata Simulator will now quit");
            //    Application.Current.MainWindow.Close();
            //    return;
            //}
            mainWindow.Show();
            
        }
    }
}
