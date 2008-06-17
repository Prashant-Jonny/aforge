// AForge Lego Robotics Library
// AForge.NET framework
//
// Copyright � Andrew Kirillov, 2007-2008
// andrew.kirillov@gmail.com
//

namespace AForge.Robotics.Lego
{
    using System;
    using System.Text;
    using AForge.Robotics.Lego.Internals;

    /// <summary>
    /// Manipulation of Lego Mindstorms RCX device.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// 
    /// <para><note>Only communication through USB IR tower is supported at this point.</note></para>
    /// 
    /// </remarks>
    /// 
    public class RCXBrick
    {
        /// <summary>
        /// Enumeration of sound type playable by Lego RCX brick.
        /// </summary>
        public enum SoundType
        {
            /// <summary>
            /// Blip sound.
            /// </summary>
            Blip,
            /// <summary>
            /// Double beep spund.
            /// </summary>
            BeepBeep,
            /// <summary>
            /// Downward tones sound.
            /// </summary>
            DownwardTones,
            /// <summary>
            /// Upward tones sound.
            /// </summary>
            UpwardTones,
            /// <summary>
            /// Low buzz sound.
            /// </summary>
            LowBuzz,
            /// <summary>
            /// Fast upward tones sound.
            /// </summary>
            FastUpwardTones,
        }

        /// <summary>
        /// Enumeration of RCX brick sensors.
        /// </summary>
        public enum Sensor
        {
            /// <summary>
            /// First sensor.
            /// </summary>
            First,
            /// <summary>
            /// Second sensor.
            /// </summary>
            Second,
            /// <summary>
            /// Third sensor.
            /// </summary>
            Third
        }

        /// <summary>
        /// Enumeration of RCX brick sensor types.
        /// </summary>
        public enum SensorType
        {
            /// <summary>
            /// Raw sensor.
            /// </summary>
            Raw,
            /// <summary>
            /// Touch sensor (default mode is boolean).
            /// </summary>
            Touch,
            /// <summary>
            /// Temperature sensor (default mode is temperature in �C).
            /// </summary>
            Temperatur,
            /// <summary>
            /// Light sensor (default mode is percentage).
            /// </summary>
            Light,
            /// <summary>
            /// Rotation sensor (default mode is angle).
            /// </summary>
            Rotation
        }

        /// <summary>
        /// Enumeration of RCX brick sensor modes.
        /// </summary>
        public enum SensorMode
        {
            /// <summary>
            /// Raw mode - value in [0, 1023].
            /// </summary>
            Raw,
            /// <summary>
            /// Boolean - either 0 or 1.
            /// </summary>
            Boolean,
            /// <summary>
            /// Number of boolean transitions.
            /// </summary>
            EdgeCount,
            /// <summary>
            /// Number of boolean transitions divided by two.
            /// </summary>
            PulseCount,
            /// <summary>
            /// Raw value scaled to [0, 100].
            /// </summary>
            Percentage,
            /// <summary>
            /// Temperature in �C - 1/10ths of a degree, [-19.8, 69.5].  
            /// </summary>
            TemperatureC,
            /// <summary>
            /// Temperature in �F - 1/10ths of a degree, [-3.6, 157.1].  
            /// </summary>
            TemperatureF,
            /// <summary>
            /// Angle - 1/16ths of a rotation, represented as a signed short.
            /// </summary>
            Angle
        }

        /// <summary>
        /// Enumeration of RCX brick motors.
        /// </summary>
        [Flags]
        public enum Motor
        {
            /// <summary>
            /// Motor A.
            /// </summary>
            A = 1,
            /// <summary>
            /// Motor B.
            /// </summary>
            B = 2,
            /// <summary>
            /// Motor C.
            /// </summary>
            C = 4,

            /// <summary>
            /// Motors A and B.
            /// </summary>
            AB = 3,
            /// <summary>
            /// Motors A and C.
            /// </summary>
            AC = 5,
            /// <summary>
            /// Motors B and C.
            /// </summary>
            BC = 6,
            /// <summary>
            /// Motors A, B and C.
            /// </summary>
            ABC = 7,
            /// <summary>
            /// All motors (A, B and C).
            /// </summary>
            All = 7
        }

        // Ghost communication stack
        private IntPtr stack;

        /// <summary>
        /// Initializes a new instance of the <see cref="RCXBrick"/> class.
        /// </summary>
        /// 
        public RCXBrick( )
        {
        }

        /// <summary>
        /// Destroys the instance of the <see cref="RCXBrick"/> class.
        /// </summary>
        /// 
        ~RCXBrick( )
		{
            Disconnect( );
		}

        /// <summary>
        /// Connect to Lego RCX brick.
        /// </summary>
        /// 
        /// <returns>Returns <b>true</b> on successful connection or <b>false</b>
        /// otherwise.</returns>
        /// 
        public bool Connect( )
        {
            // check if we are already connected
            if ( stack != IntPtr.Zero )
                return true;

            uint status;

            // create stack
            status = GhostAPI.GhCreateStack(
                "LEGO.Pbk.CommStack.Port.USB",
                "LEGO.Pbk.CommStack.Protocol.IR",
                "LEGO.Pbk.CommStack.Session",
                out stack );

            if ( !GhostAPI.PBK_SUCCEEDED( status ) )
                return false;

            // select first available device
            StringBuilder sb = new StringBuilder( 200 );
            status = GhostAPI.GhSelectFirstDevice( stack, sb, sb.Length );

            if ( !GhostAPI.PBK_SUCCEEDED( status ) )
            {
                Disconnect( );
                return false;
            }

            // open stack, set interleave, set wait mode and check if the brick is alive
            if (
                !GhostAPI.PBK_SUCCEEDED( GhostAPI.GhOpen( stack ) ) ||
                !GhostAPI.PBK_SUCCEEDED( GhostAPI.GhSetWaitMode( stack, IntPtr.Zero ) ) ||
                !GhostAPI.PBK_SUCCEEDED( GhostAPI.GhSetInterleave( stack, 1, 0 ) ) ||
                !IsAlive( )
                )
            {
                Disconnect( );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Disconnnect from Lego RCX brick.
        /// </summary>
        public void Disconnect( )
        {
            if ( stack != IntPtr.Zero )
            {
                Internals.GhostAPI.GhClose( stack );
                stack = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Check if the RCX brick is alive and responds to messages.
        /// </summary>
        /// 
        /// <returns>Returns <b>true</b> if device is alive or <b>false</b> otherwise.</returns>
        /// 
        public bool IsAlive( )
        {
            return SendCommand( new byte[] { 0x10 }, new byte[1], 1 );
        }

        /// <summary>
        /// Play one of supported souns.
        /// </summary>
        /// 
        /// <param name="type">Sound type to play.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool PlaySound( SoundType type )
        {
            return SendCommand( new byte[] { 0x51, (byte) type }, new byte[1], 1 ); ;
        }

        /// <summary>
        /// Play tone of specified frequency.
        /// </summary>
        /// 
        /// <param name="frequency">Tone frequency in Hz.</param>
        /// <param name="duration">Tone duration in 1/100ths of a second.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool PlayTone( short frequency, byte duration )
        {
            return SendCommand( new byte[] { 0x23,
                (byte) ( frequency & 0xFF ),
                (byte) ( frequency >> 16 ),
                duration },
                new byte[1], 1 ); ;
        }

        /// <summary>
        /// Get version information of RCX brick.
        /// </summary>
        /// 
        /// <param name="romVersion">ROM version number.</param>
        /// <param name="firmwareVersion">Firmware version number.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool GetVersion( out string romVersion, out string firmwareVersion )
        {
            byte[] reply = new byte[9];

            if ( SendCommand( new byte[] { 0x15, 1, 3, 5, 7, 11 }, reply, 9 ) )
            {
                romVersion = string.Format( "{0}.{1}",
                    reply[2] | ( reply[1] << 8 ),
                    reply[4] | ( reply[3] << 8 ) );
                firmwareVersion = string.Format( "{0}.{1}",
                    reply[6] | ( reply[5] << 8 ),
                    reply[8] | ( reply[7] << 8 ) );
                return true;
            }

            romVersion = null;
            firmwareVersion = null;

            return false;
        }

        /// <summary>
        /// Get battery power of RCX brick.
        /// </summary>
        /// 
        /// <param name="power">RCX brick's battery power in millivolts.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool GetBatteryPower( out int power )
        {
            byte[] reply = new byte[3];

            if ( SendCommand( new byte[] { 0x30 }, reply, 3 ) )
            {
                power = reply[1] | ( reply[2] << 8 );
                return true;
            }

            power = 0;

            return false;
        }

        /// <summary>
        /// Set current time for the RCX brick.
        /// </summary>
        /// 
        /// <param name="hours">Hours, [0..23].</param>
        /// <param name="minutes">Minutes, [0..59].</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool SetTime( byte hours, byte minutes )
        {
            return SendCommand( new byte[] { 0x22, hours, minutes }, new byte[1], 1 );
        }

        /// <summary>
        /// Turn off the RCX brick.
        /// </summary>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool PowerOff( )
        {
            return SendCommand( new byte[] { 0x60 }, new byte[1], 1 );
        }

        /// <summary>
        /// Get sensor's value.
        /// </summary>
        /// 
        /// <param name="sensor">Sensor to get value of.</param>
        /// <param name="value">Retrieved sensor's value (units depend on current
        /// sensor's type and mode).</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool GetSensorValue( Sensor sensor, out short value )
        {
            byte[] reply = new byte[3];

            if ( SendCommand( new byte[] { 0x12, 9, (byte) sensor }, reply, 3 ) )
            {
                value = (short) ( reply[1] | ( reply[2] << 8 ) );
                return true;
            }

            value = 0;

            return false;
        }

        /// <summary>
        /// Set sensor's type.
        /// </summary>
        /// 
        /// <param name="sensor">Sensor to set type of.</param>
        /// <param name="type">Sensor type to set.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool SetSensorType( Sensor sensor, SensorType type )
        {
            return SendCommand( new byte[] { 0x32, (byte) sensor, (byte) type }, new byte[1], 1 );
        }

        /// <summary>
        /// Set sensor's mode.
        /// </summary>
        /// 
        /// <param name="sensor">Sensor to set mode of.</param>
        /// <param name="mode">Sensor mode to set.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool SetSensorMode( Sensor sensor, SensorMode mode )
        {
            return SendCommand( new byte[] { 0x42, (byte) sensor, (byte) ( (byte) mode << 5 ) }, new byte[1], 1 );
        }

        /// <summary>
        /// Clear the counter associated with the specified sensor by setting it to a value of zero.
        /// </summary>
        /// 
        /// <param name="sensor">Sensor to clear value of.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool ClearSensor( Sensor sensor )
        {
            return SendCommand( new byte[] { 0xD1, (byte) sensor }, new byte[1], 1 );
        }

        /// <summary>
        /// Turn on/off specified motors.
        /// </summary>
        /// 
        /// <param name="motors">Motors to turn on/off.</param>
        /// <param name="on">True to turn motors on, otherwise false.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool SetMotorOn( Motor motors, bool on )
        {
            return SendCommand( new byte[] { 0x21,
                (byte) ( (byte) motors | ( on ? 0x80 : 0x40 ) )  },
                new byte[1], 1 );
        }

        /// <summary>
        /// Set power of specified motors.
        /// </summary>
        /// 
        /// <param name="motors">Motors to set power of.</param>
        /// <param name="power">Power level to set, [0..7].</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool SetMotorPower( Motor motors, byte power )
        {
            return SendCommand( new byte[] { 0x13, (byte) motors, 2, Math.Min( power, (byte) 7 ) },
                new byte[1], 1 );
        }

        /// <summary>
        /// Set direction of specified motors.
        /// </summary>
        /// 
        /// <param name="motors">Motors to set direction of.</param>
        /// <param name="isForward">True to set forward direction, false to set backward.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool SetMotorDirection( Motor motors, bool isForward )
        {
            return SendCommand( new byte[] { 0xE1,
                (byte) ( (byte) motors | ( isForward ? 0x80 : 0 ) )  },
                new byte[1], 1 );
        }

        /// <summary>
        /// Set transmitter's range.
        /// </summary>
        /// 
        /// <param name="isLongRange">True is long range should be set, otherwise false.</param>
        /// 
        /// <returns>Returns <b>true</b> if command was executed successfully or <b>false</b> otherwise.</returns>
        /// 
        public bool SetTransmitterRange( bool isLongRange )
        {
            return SendCommand( new byte[] { 0x31, (byte) ( ( isLongRange) ? 1 : 0 ) }, new byte[1], 1 );
        }

        /// <summary>
        /// Send command to Lego RCX brick.
        /// </summary>
        /// 
        /// <param name="command">Command to send.</param>
        /// <param name="reply">Buffer to receive reply into.</param>
        /// <param name="expectedReplyLen">Expected reply length.</param>
        /// 
        /// <returns>Returns <b>true</b> if the command was sent successfully and reply was
        /// received, otherwise <b>false</b>.</returns>
        /// 
        /// <exception cref="ArgumentException">Reply buffer size is smaller than the reply data size.</exception>
        /// <exception cref="ApplicationException">Reply does not correspond to command (first byte of reply
        /// should be complement (bitwise NOT) to the first byte of command orred with 0x08.</exception>
        /// 
        protected bool SendCommand( byte[] command, byte[] reply, int expectedReplyLen )
        {
            bool result = false;
            uint status;
            IntPtr queue;

            // create command queue
            status = GhostAPI.GhCreateCommandQueue( out queue );

            if ( !GhostAPI.PBK_SUCCEEDED( status ) )
                return false;

            // append command to the queue
            status = GhostAPI.GhAppendCommand( queue, command, command.Length, expectedReplyLen );

            if ( GhostAPI.PBK_SUCCEEDED( status ) )
            {
                // execute command
                status = GhostAPI.GhExecute( stack, queue );

                if ( GhostAPI.PBK_SUCCEEDED( status ) )
                {
                    IntPtr commandHandle;
                    uint replyLen;

                    // get first command and its reply data lenght
                    if (
                        ( GhostAPI.PBK_SUCCEEDED( GhostAPI.GhGetFirstCommand( queue, out commandHandle ) ) ) &&
                        ( GhostAPI.PBK_SUCCEEDED( GhostAPI.GhGetCommandReplyLen( commandHandle, out replyLen ) ) )
                        )
                    {
                        // check provided reply buffer size
                        if ( reply.Length < replyLen )
                            throw new ArgumentException( "Reply buffer is too small" );

                        // get reply data
                        status = GhostAPI.GhGetCommandReply( commandHandle, reply, replyLen );

                        if ( GhostAPI.PBK_SUCCEEDED( status ) )
                        {
                            // check that reply corresponds �� command
                            if ( ( command[0] | 0x08 ) != (byte) ~reply[0] )
                                throw new ApplicationException( "Reply does not correspond to command" );

                            result = true;
                        }
                    }
                }
            }

            // destroy command queue
            GhostAPI.GhDestroyCommandQueue( queue );

            return result;
        }
    }
}
