// AForge Core Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright � Andrew Kirillov, 2007-2010
// andrew.kirillov@aforgenet.com
//
// Copyright � Fabio L. Caversan, 2008
// fabio.caversan@gmail.com
//

namespace AForge
{
    using System;

    /// <summary>
    /// Structure for representing a pair of coordinates of double type.
    /// </summary>
    /// 
    /// <remarks><para>The structure is used to store a pair of floating point
    /// coordinates with double precision.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // assigning coordinates in the constructor
    /// DoublePoint p1 = new DoublePoint( 10, 20 );
    /// // creating a point and assigning coordinates later
    /// DoublePoint p2;
    /// p2.X = 30;
    /// p2.Y = 40;
    /// // calculating distance between two points
    /// double distance = p1.DistanceTo( p2 );
    /// </code>
    /// </remarks>
    /// 
    public struct DoublePoint
    {
        /// <summary> 
        /// X coordinate.
        /// </summary> 
        /// 
        public double X;

        /// <summary> 
        /// Y coordinate.
        /// </summary> 
        /// 
        public double Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoublePoint"/> structure.
        /// </summary>
        /// 
        /// <param name="x">X axis coordinate.</param>
        /// <param name="y">Y axis coordinate.</param>
        /// 
        public DoublePoint( double x, double y )
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Calculate Euclidean distance between two points.
        /// </summary>
        /// 
        /// <param name="anotherPoint">Point to calculate distance to.</param>
        /// 
        /// <returns>Returns Euclidean distance between this point and
        /// <paramref name="anotherPoint"/> points.</returns>
        /// 
        public double DistanceTo( DoublePoint anotherPoint )
        {
            double dx = X - anotherPoint.X;
            double dy = Y - anotherPoint.Y;

            return System.Math.Sqrt( dx * dx + dy * dy );
        }

        /// <summary>
        /// Addition operator - adds values of two points.
        /// </summary>
        /// 
        /// <param name="p1">First point for addition.</param>
        /// <param name="p2">Second point for addition.</param>
        /// 
        /// <returns>Returns new point which coordinates equal to sum of corresponding
        /// coordinates of specified points.</returns>
        /// 
        public static DoublePoint operator +( DoublePoint p1, DoublePoint p2 )
        {
            return new DoublePoint( p1.X + p2.X, p1.Y + p2.Y );
        }

        /// <summary>
        /// Subtraction operator - subtracts values of two points.
        /// </summary>
        /// 
        /// <param name="p1">Point to subtract from.</param>
        /// <param name="p2">Point to subtract.</param>
        /// 
        /// <returns>Returns new point which coordinates equal to difference of corresponding
        /// coordinates of specified points.</returns>
        ///
        public static DoublePoint operator -( DoublePoint p1, DoublePoint p2 )
        {
            return new DoublePoint( p1.X - p2.X, p1.Y - p2.Y );
        }

        /// <summary>
        /// Addition operator - adds scalar to the specified point.
        /// </summary>
        /// 
        /// <param name="p">Point to increase coordinates of.</param>
        /// <param name="valueToAdd">Value to add to coordinates of the specified point.</param>
        /// 
        /// <returns>Returns new point which coordinates equal to coordinates of
        /// the specified point increased by specified value.</returns>
        /// 
        public static DoublePoint operator +( DoublePoint p, double valueToAdd )
        {
            return new DoublePoint( p.X + valueToAdd, p.Y + valueToAdd );
        }

        /// <summary>
        /// Subtraction operator - subtracts scalar from the specified point.
        /// </summary>
        /// 
        /// <param name="p">Point to decrease coordinates of.</param>
        /// <param name="valueToSubtract">Value to subtract from coordinates of the specified point.</param>
        /// 
        /// <returns>Returns new point which coordinates equal to coordinates of
        /// the specified point decreased by specified value.</returns>
        /// 
        public static DoublePoint operator -( DoublePoint p, double valueToSubtract )
        {
            return new DoublePoint( p.X - valueToSubtract, p.Y - valueToSubtract );
        }

        /// <summary>
        /// Multiplication operator - multiplies coordinates of the specified point by scalar value.
        /// </summary>
        /// 
        /// <param name="p">Point to multiply coordinates of.</param>
        /// <param name="factor">Multiplication factor.</param>
        /// 
        /// <returns>Returns new point which coordinates equal to coordinates of
        /// the specified point multiplied by specified value.</returns>
        ///
        public static DoublePoint operator *( DoublePoint p, double factor )
        {
            return new DoublePoint( p.X * factor, p.Y * factor );
        }

        /// <summary>
        /// Division operator - divides coordinates of the specified point by scalar value.
        /// </summary>
        /// 
        /// <param name="p">Point to divide coordinates of.</param>
        /// <param name="factor">Division factor.</param>
        /// 
        /// <returns>Returns new point which coordinates equal to coordinates of
        /// the specified point divided by specified value.</returns>
        /// 
        public static DoublePoint operator /( DoublePoint p, double factor )
        {
            return new DoublePoint( p.X / factor, p.Y / factor );
        }

        /// <summary>
        /// Explicit conversion to <see cref="IntPoint"/>.
        /// </summary>
        /// 
        /// <param name="p">Double precision point to convert to integer point.</param>
        /// 
        /// <returns>Returns new integer point which coordinates are explicitly converted
        /// to integers from coordinates of the specified double precision point by
        /// casting double values to integers value.</returns>
        /// 
        public static explicit operator IntPoint( DoublePoint p )
        {
            return new IntPoint( (int) p.X, (int) p.Y );
        }

        /// <summary>
        /// Rounds the double precision point.
        /// </summary>
        /// 
        /// <returns>Returns new integer point, which coordinates equal to whole numbers
        /// nearest to the corresponding coordinates of the double precision point.</returns>
        /// 
        public IntPoint Round( )
        {
            return new IntPoint( (int) Math.Round( X ), (int) Math.Round( Y ) );
        }

        /// <summary>
        /// Get string representation of the class.
        /// </summary>
        /// 
        /// <returns>Returns string, which contains values of the point in readable form.</returns>
        ///
        public override string ToString( )
        {
            return string.Format( "{0}, {1}", X, Y );
        }

        /// <summary>
        /// Calculate Euclidean norm of the vector comprised of the point's 
        /// coordinates - distance from (0, 0) in other words.
        /// </summary>
        /// 
        /// <returns>Returns point's distance from (0, 0) point.</returns>
        /// 
        public double EuclideanNorm( )
        {
            return System.Math.Sqrt( X * X + Y * Y );
        }
    }
}
