/*
    Clase principal que agrupa las funcionalidades de la Clase que controla Kinect 
    y la clase que se encarga del ratón. 
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;            
using System.Windows.Media;            
using System.Windows.Media.Imaging;
using Microsoft.Kinect;                //Nombre de espacios para Kinect
using System.Media;
using System.Runtime.InteropServices;  //Utilizado para usar DllImport
using System.Threading.Tasks;
using System.Media;                    //Librería Música
using System.Threading;                //Utilizado para usar Thread.Sleep(x)

namespace Microsoft.Samples.Kinect.BodyBasics
{
   
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {

        // VARIABLES GLOBALES
       
        /// <summary>
        /// Objeto SoundPlayer que se encarga de reproducir la música del menú.
        /// </summary>
        private SoundPlayer music = new SoundPlayer(@"C:\Users\Jesús\Desktop\Práctica 2 NPI\Music\menu.wav");

        /// <summary>
        /// Variable para la gestión y uso de Kinect,  Clase KinectControl
        /// </summary>
        KinectControl kinectcontrol = null;

        ///<summary>
        /// Path y args necesarios para lanzar procesos
        /// </summary>
        string path = null;
        string args = null;

        ///<summary>
        /// Booleano para controlar el que un juego esté seleccionado antes de pulsar el 
        /// botón Jugar.
        /// </summary>
        bool gameSelected = false;

        /// <summary>
        /// Inicializa una nueva clase de tipo MainWindow.
        /// </summary>
        public MainWindow()
        {

            //Instanciación e inicialización de un objeto para control de Kinect.
            kinectcontrol = new KinectControl();

            // Usa el objeto window como el view model.
            this.DataContext = this; 

            // Inicializa la ventana WPF
            this.InitializeComponent();
         
            //Inicia la música de menu.
            music.PlayLooping();  
        }


        ///<summary>
        /// Método para pasar la primera foto de la instrucción.
        /// </summary>
        public void step1(object sender, RoutedEventArgs e)
        {
            int1.Visibility = System.Windows.Visibility.Hidden;
            int2.Visibility = System.Windows.Visibility.Visible;
        }

        ///<summary>
        /// Método para pasar la segunda foto de la instrucción y dar paso 
        /// al menú principal.
        /// </summary>
        public void step2(object sender, RoutedEventArgs e)
        {
            int2.Visibility = System.Windows.Visibility.Hidden;
            botonhs.Visibility = System.Windows.Visibility.Visible;
            botonlol.Visibility = System.Windows.Visibility.Visible;
            botonmario.Visibility = System.Windows.Visibility.Visible;
            GS.Visibility = System.Windows.Visibility.Visible;
            salir.Visibility = System.Windows.Visibility.Visible;
            jugar.Visibility = System.Windows.Visibility.Visible;

           
        }

        ///<summary>
        /// Método para el evento de click del botón de Heartstone
        /// </summary>
        private void Button_Hs(object sender, RoutedEventArgs e)
        {
            //feedback con el usuario del juego seleccionado.
            GS.Content = "Juego seleccionado: Heartstone";

            //Argumentos
            path = @"C:\Program Files (x86)\Hearthstone\Hearthstone Beta Launcher.exe";
            gameSelected = true;
        }

        ///<summary>
        /// Método para el evento de click del botón de League of Legends
        /// </summary>
        private void Button_Lol(object sender, RoutedEventArgs e)
        {
            //feedback con el usuario del juego seleccionado.
            GS.Content = "Juego seleccionado: League of legends";

            //Argumentos
            path = @"C:\Riot Games\League of Legends\lol.launcher.exe";
            gameSelected = true;
        }

        ///<summary>
        /// Método para el evento de click del botón de Mario Kart
        /// </summary>
        private void Button_Mario(object sender, RoutedEventArgs e)
        {
            //feedback con el usuario del juego seleccionado.
            GS.Content = "Juego seleccionado: Mario Bros Super Circuit";

            //Argumentos
            path = @"C:\GameBoy\VisualBoyAdvance-SDL.exe";
            args = @"C:\GameBoy\marioKart.gba";
            gameSelected = true;
        }

        ///<summary>
        /// Método para el evento de click del botón de salida.
        /// </summary>
        private void Button_Exit(object sender, RoutedEventArgs e)
        {
            //Cierra la aplicación.
            App.Current.Shutdown();
        }

        ///<summary>
        /// Método para el evento de click del botón de Jugar
        /// </summary>
        private void Button_Launch(object sender, RoutedEventArgs e)
        {
            //Si hay un juego seleccionado:
            if (gameSelected)
            {

                //Lanzamiendo del proceso con el juego en cuestión.
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = path;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                if (args != null)

                    //Añade argumentos a la ejecución del proceso si son necesarios.
                    startInfo.Arguments = " " + args;

                try
                {

                    //Empieza el proceso con toda la información puesta anteriormente.
                    using (Process execProcess = Process.Start(startInfo))
                    {

                    }
                }
                catch
                {
                    Console.WriteLine("Error");
                }

                //Se para la música al lanzar un juego.
                music.Stop();
            }
        }
    }
}
