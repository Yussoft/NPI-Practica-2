/*
    Clase utilizada para encapsular la gestión del ratón.
*/
using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    class MouseControl
    {
        //Función para el evento en el que el botón izq del ratón se presiona.
        public static void MouseLeftDown()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }

        //Función para el evento en el que el botón izq del ratón se levanta.
        public static void MouseLeftUp()
        {
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        //Función que simula un click con ratón utilizando los eventos de este.
        //Pulsación de botón izquierdo.
        public static void DoMouseClick()
        {
            mouse_event(MouseEventFlag.LeftDown | MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }


        //Posición X e Y del cursor
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        
        //Función para gestionar los eventos del ratón.
        //https://msdn.microsoft.com/es-es/library/windows/desktop/ms646260(v=vs.85).aspx
        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
        [Flags]
        enum MouseEventFlag : uint
        {

            /// <summary>
            /// Flags para los diferentes eventos del ratón como moverlo, clickar der o izq
            /// usar la rueda, etc.
            /// </summary>
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Obtiene la posicón del cursor en coordenadas de la pantalla.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        //Método que utiliza la función anterior para obtener el punto donde está el cursor
        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);

            return lpPoint;
        }

    }
}