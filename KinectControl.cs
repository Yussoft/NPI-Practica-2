using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Kinect;
using System.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Microsoft.Samples.Kinect.BodyBasics
{
    class KinectControl
    {

        ///******************************KINECT******************************

        /// <summary>
        /// Sensor de Kinect
        /// </summary>
        KinectSensor sensor;

        /// <summary>
        /// Lector de frames del cuerpo
        /// </summary>
        BodyFrameReader bodyFrameReader;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Vector con los cuerpos detectados por kinect
        /// </summary>
        private Body[] bodies = null;

        ///*****************************MOUSE VARIABLES*******************************

        /// <summary>
        /// Alto y ancho de la pantalla lo que determina la sensibilidad del ratón.
        /// </summary>
        int screenWidth, screenHeight;


        /// <summary>
        /// Temporizador utilizado para la funcionalidad de clickar al parar el ratón.
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// La sensibilidad dice cuanto se moverá el ratón de acuerdo al movimiento de la mano.
        /// </summary>
        public float mouseSensitivity = MOUSE_SENSITIVITY;

        /// <summary>
        /// Tiempo requerido para clickar sin mover el ratón. (Pause-click feature)
        /// </summary>
        public float timeRequired = TIME_REQUIRED;

        /// <summary>
        /// El radio del círculo en el que se ha de mover tu mano para ser considerado
        /// "pause-click".
        /// </summary>
        public float pauseThresold = PAUSE_THRESOLD;

        /// <summary>
        /// Booleano que decide si se clicka o únicamente se mueve la mano/cursor.
        /// </summary>
        public bool doClick = DO_CLICK;

        /// <summary> 
        /// Booleano para indicar si hacemos click al cerrar la mano.
        /// </summary>
        public bool useCloseHandGesture = USE_CLOSE_HAND;

        /// <summary>
        /// Valor de 0 - 0.95f, mientras mayor sea el valor más suave será el movimiento.
        /// </summary>
        public float cursorSmoothing = CURSOR_SMOOTHING;


        // Valores por defecto de las variables anteriores.
        public const float MOUSE_SENSITIVITY = 3.5f;
        public const float TIME_REQUIRED = 2f;
        public const float PAUSE_THRESOLD = 90f;
        public const bool DO_CLICK = true;
        public const bool USE_CLOSE_HAND = false;
        public const float CURSOR_SMOOTHING = 0.2f;

        /// <summary>
        /// Determina si hemos trackeado la mano y utilizado esta para mover el cursor,
        /// </summary>
        bool alreadyTrackedPos = false;

        /// <summary>
        /// Se guarda el tiempo para saber si se está clickando.
        /// </summary>
        float timeCount = 0;

        /// <summary>
        /// Almacena la última posición del puntero.
        /// </summary>
        Point lastCurPos = new Point(0, 0);

        /// <summary>
        /// If true, user did a left hand Grip gesture
        /// </summary>
        bool wasLeftGrip = false;
        /// <summary>
        /// If true, user did a right hand Grip gesture
        /// </summary>
        bool wasRightGrip = false;

        public KinectSensor getSensor()
        {
            return sensor;
        }

        public CoordinateMapper getCoordinateMapper()
        {
            return coordinateMapper;
        }

        /// <summary>
        /// Función que activa el sensor de kinect y preparará los eventos.
        /// </summary>
        public KinectControl()
        {
            // Obtenemos Active Kinect Sensor
            sensor = KinectSensor.GetDefault();

            // Obtiene la profundidad.
            // Utilizamos el objeto kinectcontrol de la clase KinectControl
            FrameDescription frameDescription = sensor.DepthFrameSource.FrameDescription;

            // Abrimos el lector de frames del cuerpo
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            // Obtenemos el tamaño de nuestra pantalla.
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;
           
            // Establecemos un contador que se ejecutará cada 0.1s.
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
            
            // Empezar a utilizar Kinect.
            sensor.Open();
        }

        /// <summary>
        /// Temporizador de pausar para clickar (Pause-click)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer_Tick(object sender, EventArgs e)
        {
            //Si no estamos clickando y estamos haciendo el gesto de cerrar la mano
            if (!doClick || useCloseHandGesture) return;

            //Si no hemos trackeado la mano.
            if (!alreadyTrackedPos)
            {
                timeCount = 0;
                return;
            }

            //Obtenemos la posición del cursor
            Point curPos = MouseControl.GetCursorPosition();

            //Si la posición del ultimo cursor menos la actual son menores 
            //que el radio permitido se comienza a contar tiempo.
            if ((lastCurPos - curPos).Length < pauseThresold)
            {
                if ((timeCount += 0.1f) > timeRequired)
                {
                    MouseControl.DoMouseClick();
                    timeCount = 0;
                }
            }
            else
            {
                //Reinicio del contador.
                timeCount = 0;
            }

            //Actualización del ultimo cursor.
            lastCurPos = curPos;
        }

        /// <summary>
        /// Lectura de los body frames.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // Primero Kinect meterá en un array todos los Bodies.
                    // Mientras los Bodies este disponibles serán utilizados.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            //Si no se reciben datos.
            if (!dataReceived)
            {
                //No hemos trackeado nada.
                alreadyTrackedPos = false;
                return;
            }

            foreach (Body body in this.bodies)
            {
                if (body.IsTracked)
                {
                    // Obtenemos la cadera y manos como puntos.
                    CameraSpacePoint handLeft = body.Joints[JointType.HandLeft].Position;
                    CameraSpacePoint handRight = body.Joints[JointType.HandRight].Position;
                    CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;

                    if (handRight.Z - spineBase.Z < -0.15f) // if right hand lift forward
                    {
                        /* hand x calculated by this. we don't use shoulder right as a reference cause the shoulder right
                         * is usually behind the lift right hand, and the position would be inferred and unstable.
                         * because the spine base is on the left of right hand, we plus 0.05f to make it closer to the right. */
                        float x = handRight.X - spineBase.X + 0.05f;
                        /* hand y calculated by this. ss spine base is way lower than right hand, we plus 0.51f to make it
                         * higer, the value 0.51f is worked out by testing for a several times, you can set it as another one you like. */
                        float y = spineBase.Y - handRight.Y + 0.51f;
                        // get current cursor position
                        Point curPos = MouseControl.GetCursorPosition();
                        // smoothing for using should be 0 - 0.95f. The way we smooth the cusor is: oldPos + (newPos - oldPos) * smoothValue
                        float smoothing = 1 - cursorSmoothing;
                        // set cursor position
                        MouseControl.SetCursorPos((int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing), (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing));

                        alreadyTrackedPos = true;

                        // Grip gesture
                        if (doClick && useCloseHandGesture)
                        {
                            if (body.HandRightState == HandState.Closed)
                            {
                                if (!wasRightGrip)
                                {
                                    MouseControl.MouseLeftDown();
                                    wasRightGrip = true;
                                }
                            }
                            else if (body.HandRightState == HandState.Open)
                            {
                                if (wasRightGrip)
                                {
                                    MouseControl.MouseLeftUp();
                                    wasRightGrip = false;
                                }
                            }
                        }
                    }
                    else if (handLeft.Z - spineBase.Z < -0.15f) // if left hand lift forward
                    {
                        float x = handLeft.X - spineBase.X + 0.3f;
                        float y = spineBase.Y - handLeft.Y + 0.51f;
                        Point curPos = MouseControl.GetCursorPosition();
                        float smoothing = 1 - cursorSmoothing;
                        MouseControl.SetCursorPos((int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing), (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing));
                        alreadyTrackedPos = true;

                        if (doClick && useCloseHandGesture)
                        {
                            if (body.HandLeftState == HandState.Closed)
                            {
                                if (!wasLeftGrip)
                                {
                                    MouseControl.MouseLeftDown();
                                    wasLeftGrip = true;
                                }
                            }
                            else if (body.HandLeftState == HandState.Open)
                            {
                                if (wasLeftGrip)
                                {
                                    MouseControl.MouseLeftUp();
                                    wasLeftGrip = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        wasLeftGrip = true;
                        wasRightGrip = true;
                        alreadyTrackedPos = false;
                    }

                    // get first tracked body only
                    break;
                }
            }
        }

        //Cierre de la aplicación.
        public void Close()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
        }

    }
}