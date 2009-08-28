﻿// AForge Surveyor Robotics Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2007-2009
// andrew.kirillov@aforgenet.com
//

namespace AForge.Robotics.Surveyor
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using AForge.Video;

    public partial class SVS
    {
        public enum CameraResolution
        {
            Tiny   = 'a',
            Small  = 'b',
            Medium = 'c',
            Large  = 'd'
        }


        public class Camera : IVideoSource
        {
            private SVSCommunicator communicator;

            // received frames count
            private int framesReceived;
            // recieved bytes count
            private int bytesReceived;
            // frame interval in milliseconds
            private int frameInterval = 0;

            private Thread thread = null;
            private ManualResetEvent stopEvent = null;

            // buffer size used to download JPEG image
            private const int bufferSize = 768 * 1024;
            /// <summary>
            /// New frame event.
            /// </summary>
            /// 
            /// <remarks><para>Notifies clients about new available frame from video source.</para>
            /// 
            /// <para><note>Since video source may have multiple clients, each client is responsible for
            /// making a copy (cloning) of the passed video frame, because the video source disposes its
            /// own original copy after notifying of clients.</note></para>
            /// </remarks>
            /// 
            public event NewFrameEventHandler NewFrame;

            /// <summary>
            /// Video source error event.
            /// </summary>
            /// 
            /// <remarks>This event is used to notify clients about any type of errors occurred in
            /// video source object, for example internal exceptions.</remarks>
            /// 
            public event VideoSourceErrorEventHandler VideoSourceError;

            /// <summary>
            /// Frame interval.
            /// </summary>
            /// 
            /// <remarks><para>The property sets the interval in milliseconds betwen frames. If the property is
            /// set to 100, then the desired frame rate will be 10 frames per second.</para>
            /// 
            /// <para>Default value is set to <b>0</b> - get new frames as fast as possible.</para>
            /// </remarks>
            /// 
            public int FrameInterval
            {
                get { return frameInterval; }
                set { frameInterval = value; }
            }

            /// <summary>
            /// Video source string.
            /// </summary>
            /// 
            /// <remarks>
            /// <para>The property keeps connection string, which is used to connect to SVS camera.</para>
            /// </remarks>
            /// 
            public string Source
            {
                get { return ""; }
                set
                {
                    throw new NotImplementedException( "Setting the property is not allowed" );
                }
            }

            /// <summary>
            /// Received frames count.
            /// </summary>
            /// 
            /// <remarks>Number of frames the video source provided from the moment of the last
            /// access to the property.
            /// </remarks>
            /// 
            public int FramesReceived
            {
                get
                {
                    int frames = framesReceived;
                    framesReceived = 0;
                    return frames;
                }
            }

            /// <summary>
            /// Received bytes count.
            /// </summary>
            /// 
            /// <remarks>Number of bytes the video source provided from the moment of the last
            /// access to the property.
            /// </remarks>
            /// 
            public int BytesReceived
            {
                get
                {
                    int bytes = bytesReceived;
                    bytesReceived = 0;
                    return bytes;
                }
            }

            /// <summary>
            /// User data.
            /// </summary>
            /// 
            /// <remarks>The property allows to associate user data with video source object.</remarks>
            /// 
            public object UserData
            {
                get { return null; }
                set {  }
            }

            /// <summary>
            /// State of the video source.
            /// </summary>
            /// 
            /// <remarks>Current state of video source object - running or not.</remarks>
            /// 
            public bool IsRunning
            {
                get
                {
                    if ( thread != null )
                    {
                        // check thread status
                        if ( thread.Join( 0 ) == false )
                            return true;

                        // the thread is not running, free resources
                        Free( );
                    }
                    return false;
                }
            }

            // The class may be instantiate using SVS object only
            internal Camera( SVSCommunicator communicator )
            {
                this.communicator = communicator;
            }

            /// <summary>
            /// Start video source.
            /// </summary>
            /// 
            /// <remarks>Starts video source and return execution to caller. Video source
            /// object creates background thread and notifies about new frames with the
            /// help of <see cref="NewFrame"/> event.</remarks>
            /// 
            public void Start( )
            {
                if ( thread == null )
                {
                    framesReceived = 0;
                    bytesReceived  = 0;

                    // create events
                    stopEvent = new ManualResetEvent( false );

                    // create and start new thread
                    thread = new Thread( new ThreadStart( WorkerThread ) );
                    thread.Name = Source; // mainly for debugging
                    thread.Start( );
                }
            }

            /// <summary>
            /// Signal video source to stop its work.
            /// </summary>
            /// 
            /// <remarks>Signals video source to stop its background thread, stop to
            /// provide new frames and free resources.</remarks>
            /// 
            public void SignalToStop( )
            {
                // stop thread
                if ( thread != null )
                {
                    // signal to stop
                    stopEvent.Set( );
                }
            }

            /// <summary>
            /// Wait for video source has stopped.
            /// </summary>
            /// 
            /// <remarks>Waits for video source stopping after it was signalled to stop using
            /// <see cref="SignalToStop"/> method.</remarks>
            /// 
            public void WaitForStop( )
            {
                if ( thread != null )
                {
                    // wait for thread stop
                    thread.Join( );

                    Free( );
                }
            }

            /// <summary>
            /// Stop video source.
            /// </summary>
            /// 
            /// <remarks><para>Stops video source aborting its thread.</para>
            /// 
            /// <para><note>Since the method aborts background thread, its usage is highly not preferred
            /// and should be done only if there are no other options. The correct way of stopping camera
            /// is <see cref="SignalToStop">signaling it to stop</see> and then
            /// <see cref="WaitForStop">waiting</see> for background thread's completion.</note></para>
            /// </remarks>
            /// 
            public void Stop( )
            {
                if ( this.IsRunning )
                {
                    thread.Abort( );
                    WaitForStop( );
                }
            }

            /// <summary>
            /// Free resource.
            /// </summary>
            /// 
            private void Free( )
            {
                thread = null;

                // release events
                stopEvent.Close( );
                stopEvent = null;
            }

            public void SetQuality( byte quality )
            {
                if ( ( quality < 1 ) || ( quality > 8 ) )
                    throw new ArgumentOutOfRangeException( "Invalid quality level was specified." );

                communicator.Send( new byte[] { (byte) 'q', (byte) ( quality + (byte) '0' ) } );
            }

            public void SetResolution( CameraResolution resolution )
            {
                communicator.Send( new byte[] { (byte) resolution } );
            }

            /// <summary>
            /// Worker thread.
            /// </summary>
            /// 
            private void WorkerThread( )
            {
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch( );

                // buffer to read stream into
                byte[] buffer = new byte[bufferSize];

                while ( !stopEvent.WaitOne( 0, true ) )
                {
                    try
                    {
                        stopWatch.Reset( );
                        stopWatch.Start( );

                        int bytesRead = communicator.SendAndReceive( new byte[] { (byte) 'I' }, buffer );

                        bytesReceived += bytesRead;

                        if ( bytesRead > 10 )
                        {
                            // check for image reply signature
                            if (
                                ( buffer[0] == (byte) '#' ) &&
                                ( buffer[1] == (byte) '#' ) &&
                                ( buffer[2] == (byte) 'I' ) &&
                                ( buffer[3] == (byte) 'M' ) &&
                                ( buffer[4] == (byte) 'J' ) )
                            {
                                // extract image size
                                int imageSize = System.BitConverter.ToInt32( buffer, 6 );

                                // check if image is in the buffer
                                if ( !stopEvent.WaitOne( 0, true ) )
                                {
                                    try
                                    {
                                        // decode image from memory stream
                                        Bitmap bitmap = (Bitmap) Bitmap.FromStream( new MemoryStream( buffer, 10, imageSize ) );
                                        framesReceived++;

                                        // let subscribers know if there are any
                                        if ( NewFrame != null )
                                        {
                                            NewFrame( this, new NewFrameEventArgs( bitmap ) );
                                        }

                                        bitmap.Dispose( );
                                    }
                                    catch
                                    {
                                    }

                                    // wait for a while ?
                                    if ( frameInterval > 0 )
                                    {
                                        // get download duration
                                        stopWatch.Stop( );

                                        // miliseconds to sleep
                                        int msec = frameInterval - (int) stopWatch.ElapsedMilliseconds;

                                        while ( ( msec > 0 ) && ( stopEvent.WaitOne( 0, true ) == false ) )
                                        {
                                            // sleeping ...
                                            Thread.Sleep( ( msec < 100 ) ? msec : 100 );
                                            msec -= 100;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int stop = 0;
                            }
                        }
                    }
                    catch
                    {
                        if ( VideoSourceError != null )
                        {
                            VideoSourceError( this, new VideoSourceErrorEventArgs( "" ) );
                        }
                    }
                }

                stopWatch.Stop( );
            }           
        }
    }
}
