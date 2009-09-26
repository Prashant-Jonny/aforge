﻿// AForge Math Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2007-2009
// andrew.kirillov@aforgenet.com
//

namespace AForge.Math.Geometry
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Set of tools for processing collection of points in 2D space.
    /// </summary>
    /// 
    /// <remarks><para>The static class contains set of routines, which provide different
    /// operations with collection of points in 2D space. For example, finding the
    /// furthest point from a specified point or line.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create points' list
    /// List&lt;IntPoint&gt; points = new List&lt;IntPoint&gt;( );
    /// points.Add( new IntPoint( 10, 10 ) );
    /// points.Add( new IntPoint( 20, 15 ) );
    /// points.Add( new IntPoint( 15, 30 ) );
    /// points.Add( new IntPoint( 40, 12 ) );
    /// points.Add( new IntPoint( 30, 20 ) );
    /// // get furthest point from the specified point
    /// IntPoint p1 = PointsCloud.GetFurthestPoint( points, new IntPoint( 15, 15 ) );
    /// Console.WriteLine( p1.X + ", " + p1.Y );
    /// // get furthest point from line
    /// IntPoint p2 = PointsCloud.GetFurthestPointFromLine( points,
    ///     new IntPoint( 50, 0 ), new IntPoint( 0, 50 ) );
    /// Console.WriteLine( p2.X + ", " + p2.Y );
    /// </code>
    /// </remarks>
    /// 
    public static class PointsCloud
    {
        /// <summary>
        /// Shift cloud by adding specified value to all points in the collection.
        /// </summary>
        /// 
        /// <param name="cloud">Collection of points to shift their coordinates.</param>
        /// <param name="shift">Point to shift by.</param>
        /// 
        public static void Shift( List<IntPoint> cloud, IntPoint shift )
        {
            for ( int i = 0, n = cloud.Count; i < n; i++ )
            {
                cloud[i] = cloud[i] + shift;
            }
        }

        /// <summary>
        /// Find furhtest point from the specified point.
        /// </summary>
        /// 
        /// <param name="cloud">Collection of points to search furthest point in.</param>
        /// <param name="referencePoint">The point to search furthest point from.</param>
        /// 
        /// <returns>Returns a point, which is the furthest away from the <paramref name="referencePoint"/>.</returns>
        /// 
        public static IntPoint GetFurthestPoint( List<IntPoint> cloud, IntPoint referencePoint )
        {
            IntPoint furthestPoint = referencePoint;
            double maxDistance = -1;

            int rx = referencePoint.X;
            int ry = referencePoint.Y;

            foreach ( IntPoint point in cloud )
            {
                int dx = rx - point.X;
                int dy = ry - point.Y;
                // we are not calculating square root for finding "real" distance,
                // since it is really not important for finding furthest point
                double distance = dx * dx + dy * dy;

                if ( distance > maxDistance )
                {
                    maxDistance = distance;
                    furthestPoint = point;
                }
            }

            return furthestPoint;
        }

        /// <summary>
        /// Find two furhtest points from the specified line.
        /// </summary>
        /// 
        /// <param name="cloud">Collection of points to search furthest points in.</param>
        /// <param name="linePoint1">First point forming the line.</param>
        /// <param name="linePoint2">Second point forming the line.</param>
        /// <param name="furthestPoint1">First found furthest point.</param>
        /// <param name="furthestPoint2">Second found furthest point (which is on the
        /// opposite side from the line compared to the <paramref name="furthestPoint1"/>);</param>
        /// 
        /// <remarks><para>The method finds two furhtest points from the specified line,
        /// where one point is on one side from the line and the second point is on
        /// another side from the line.</para></remarks>
        /// 
        public static void GetFurthestPointsFromLine( List<IntPoint> cloud, IntPoint linePoint1, IntPoint linePoint2,
            out IntPoint furthestPoint1, out IntPoint furthestPoint2 )
        {
            double d1, d2;

            GetFurthestPointsFromLine( cloud, linePoint1, linePoint2,
                out furthestPoint1, out d1, out furthestPoint2, out d2 );
        }

        /// <summary>
        /// Find two furhtest points from the specified line.
        /// </summary>
        /// 
        /// <param name="cloud">Collection of points to search furthest points in.</param>
        /// <param name="linePoint1">First point forming the line.</param>
        /// <param name="linePoint2">Second point forming the line.</param>
        /// <param name="furthestPoint1">First found furthest point.</param>
        /// <param name="distance1">Distance between the first found point and the given line.</param>
        /// <param name="furthestPoint2">Second found furthest point (which is on the
        /// opposite side from the line compared to the <paramref name="furthestPoint1"/>);</param>
        /// <param name="distance2">Distance between the second found point and the given line.</param>
        /// 
        /// <remarks><para>The method finds two furhtest points from the specified line,
        /// where one point is on one side from the line and the second point is on
        /// another side from the line.</para></remarks>
        ///
        public static void GetFurthestPointsFromLine( List<IntPoint> cloud, IntPoint linePoint1, IntPoint linePoint2,
            out IntPoint furthestPoint1, out double distance1, out IntPoint furthestPoint2, out double distance2 )
        {
            furthestPoint1 = linePoint1;
            distance1 = 0;

            furthestPoint2 = linePoint2;
            distance2 = 0;

            if ( linePoint2.X != linePoint1.X )
            {
                // line's equation y(x) = k * x + b
                double k = (double) ( linePoint2.Y - linePoint1.Y ) / ( linePoint2.X - linePoint1.X );
                double b = linePoint1.Y - k * linePoint1.X;

                double div = Math.Sqrt( k * k + 1 );
                double distance = 0;

                foreach ( IntPoint point in cloud )
                {
                    distance = ( k * point.X + b - point.Y ) / div;

                    if ( distance > distance1 )
                    {
                        distance1 = distance;
                        furthestPoint1 = point;
                    }
                    if ( distance < distance2 )
                    {
                        distance2 = distance;
                        furthestPoint2 = point;
                    }
                }
            }
            else
            {
                int lineX = linePoint1.X;
                double distance = 0;

                foreach ( IntPoint point in cloud )
                {
                    distance = lineX - point.X;

                    if ( distance > distance1 )
                    {
                        distance1 = distance;
                        furthestPoint1 = point;
                    }
                    if ( distance < distance2 )
                    {
                        distance2 = distance;
                        furthestPoint2 = point;
                    }
                }
            }

            distance2 = -distance2;
        }

        /// <summary>
        /// Find the furhtest point from the specified line.
        /// </summary>
        /// 
        /// <param name="cloud">Collection of points to search furthest point in.</param>
        /// <param name="linePoint1">First point forming the line.</param>
        /// <param name="linePoint2">Second point forming the line.</param>
        /// 
        /// <returns>Returns a point, which is the furthest away from the
        /// specified line.</returns>
        /// 
        /// <remarks><para>The method finds the furthest point from the specified line.
        /// Unlike the <see cref="GetFurthestPointsFromLine( List{IntPoint}, IntPoint, IntPoint, out IntPoint, out IntPoint )"/>
        /// method, this method find only one point, which is the furthest away from the line
        /// regardless of side from the line.</para></remarks>
        ///
        public static IntPoint GetFurthestPointFromLine( List<IntPoint> cloud, IntPoint linePoint1, IntPoint linePoint2 )
        {
            double d;

            return GetFurthestPointFromLine( cloud, linePoint1, linePoint2, out d );
        }

        /// <summary>
        /// Find the furhtest point from the specified line.
        /// </summary>
        /// 
        /// <param name="cloud">Collection of points to search furthest points in.</param>
        /// <param name="linePoint1">First point forming the line.</param>
        /// <param name="linePoint2">Second point forming the line.</param>
        /// <param name="distance">Distance between the furhtest found point and the given line.</param>
        /// 
        /// <returns>Returns a point, which is the furthest away from the
        /// specified line.</returns>
        /// 
        /// <remarks><para>The method finds the furthest point from the specified line.
        /// Unlike the <see cref="GetFurthestPointsFromLine( List{IntPoint}, IntPoint, IntPoint, out IntPoint, out double, out IntPoint, out double )"/>
        /// method, this method find only one point, which is the furthest away from the line
        /// regardless of side from the line.</para></remarks>
        ///
        public static IntPoint GetFurthestPointFromLine( List<IntPoint> cloud, IntPoint linePoint1, IntPoint linePoint2, out double distance )
        {
            IntPoint furthestPoint = linePoint1;
            distance = 0;

            if ( linePoint2.X != linePoint1.X )
            {
                // line's equation y(x) = k * x + b
                double k = (double) ( linePoint2.Y - linePoint1.Y ) / ( linePoint2.X - linePoint1.X );
                double b = linePoint1.Y - k * linePoint1.X;

                double div = Math.Sqrt( k * k + 1 );
                double pointDistance = 0;

                foreach ( IntPoint point in cloud )
                {
                    pointDistance = Math.Abs( ( k * point.X + b - point.Y ) / div );

                    if ( pointDistance > distance )
                    {
                        distance = pointDistance;
                        furthestPoint = point;
                    }
                }
            }
            else
            {
                int lineX = linePoint1.X;
                double pointDistance = 0;

                foreach ( IntPoint point in cloud )
                {
                    distance = Math.Abs( lineX - point.X );

                    if ( pointDistance > distance )
                    {
                        distance = pointDistance;
                        furthestPoint = point;
                    }
                }
            }

            return furthestPoint;
        }

        /// <summary>
        /// Find corners of quadrilateral area, which contains the specified collection of points.
        /// </summary>
        /// 
        /// <param name="cloud">Collection of points to search quadrilateral for.</param>
        /// 
        /// <returns>Returns a list of 4 points, which are corners of the quadrilateral area filled
        /// by specified collection of point. The first point in the list is the point with lowest
        /// X coordinate (and with lowest Y if there are several points with the same X value).</returns>
        /// 
        /// <remarks><para>The method makes an assumption that the specified collection of points
        /// form some sort of quadrilateral area. With this assumption it tries to find corners
        /// of the quadrilateral area.</para>
        /// 
        /// <para><note>The method does not search for <b>bounding</b> quadrilateral area, where
        /// all specified points are <b>inside</b> of the found quadrilateral. Some of the specified
        /// points potentially may be outside of the found quadrilateral, since the method takes
        /// corners only from the specified collection of points, but does not calculate such to form
        /// true bounding quadrilateral.</note></para>
        /// </remarks>
        /// 
        public static List<IntPoint> FindQuadrilateralCorners( List<IntPoint> cloud )
        {
            // quadrilateral's corners
            List<IntPoint> corners = new List<IntPoint>( );

            // get the furthest point from (0,0)
            IntPoint point1 = PointsCloud.GetFurthestPoint( cloud,
                new IntPoint( 0, 0 ) );
            // get the furthest point from the first point
            IntPoint point2 = PointsCloud.GetFurthestPoint( cloud, point1 );

            // get two furthest points from line
            IntPoint point3, point4;
            double distance3, distance4;

            PointsCloud.GetFurthestPointsFromLine( cloud, point1, point2,
                out point3, out distance3, out point4, out distance4 );

            // ideally points 1 and 2 form a diagonal of the
            // quadrilateral area, and points 3 and 4 form another diagonal

            // but if one of the points (3 or 4) is very close to the line
            // connecting points 1 and 2, then it is one the same line ...
            // which means corner was not found

            if ( ( distance3 < 3 ) || ( distance4 < 3 ) )
            {
                // it seems that we deal with kind of trapezoid,
                // where point 1 and 2 are on the same edge

                IntPoint tempPoint = ( distance3 > -distance4 ) ? point3 : point4;

                // try to find 3rd point
                PointsCloud.GetFurthestPointsFromLine( cloud, point1, tempPoint,
                    out point3, out distance3, out point4, out distance4 );

                bool thirdPointIsFound = false;

                if ( ( distance3 >= 3 ) && ( point3.DistanceTo( point2 ) >= 3 ) )
                {
                    thirdPointIsFound = true;
                }
                else if ( ( distance4 >= 3 ) && ( point4.DistanceTo( point2 ) >= 3 ) )
                {
                    point3 = point4;
                    thirdPointIsFound = true;
                }

                if ( !thirdPointIsFound )
                {
                    PointsCloud.GetFurthestPointsFromLine( cloud, point2, tempPoint,
                        out point3, out distance3, out point4, out distance4 );

                    if ( ( distance3 < 3 ) || ( point3.DistanceTo( point1 ) < 3 ) )
                    {
                        point3 = point4;
                    }
                }

                // try to find 4th point
                double tempDistance;

                PointsCloud.GetFurthestPointsFromLine( cloud, point1, point3,
                    out tempPoint, out tempDistance, out point4, out distance4 );

                bool fourthPointIsFound = false;

                if ( ( distance4 >= 3 ) && ( point4.DistanceTo( point2 ) >= 3 ) )
                {
                    fourthPointIsFound = true;
                }
                else if ( ( tempDistance >= 3 ) && ( tempPoint.DistanceTo( point2 ) >= 3 ) )
                {
                    point4 = tempPoint;
                    fourthPointIsFound = true;
                }

                if ( !fourthPointIsFound )
                {
                    PointsCloud.GetFurthestPointsFromLine( cloud, point2, point3,
                        out tempPoint, out tempDistance, out point4, out distance4 );

                    if ( ( distance4 < 3 ) || ( point4.DistanceTo( point1 ) < 3 ) )
                    {
                        point4 = tempPoint;
                    }
                }
            }

            corners.Add( point1 );
            corners.Add( point2 );
            corners.Add( point3 );
            corners.Add( point4 );

            // put the point with lowest X as the first
            for ( int i = 1; i < 4; i++ )
            {
                if ( ( corners[i].X < corners[0].X ) ||
                     ( ( corners[i].X == corners[0].X ) && ( corners[i].Y < corners[0].Y ) ) )
                {
                    IntPoint temp = corners[i];
                    corners[i] = corners[0];
                    corners[0] = temp;
                }
            }

            // sort other points in clock-wise order (GDI coordinates system)
            double k1 = ( corners[1].X != corners[0].X ) ?
                ( (double) ( corners[1].Y - corners[0].Y ) / ( corners[1].X - corners[0].X ) ) :
                ( ( corners[1].Y > corners[0].Y ) ? double.PositiveInfinity : double.NegativeInfinity );

            double k2 = ( corners[2].X != corners[0].X ) ?
                ( (double) ( corners[2].Y - corners[0].Y ) / ( corners[2].X - corners[0].X ) ) :
                ( ( corners[2].Y > corners[0].Y ) ? double.PositiveInfinity : double.NegativeInfinity );

            double k3 = ( corners[3].X != corners[0].X ) ?
                ( (double) ( corners[3].Y - corners[0].Y ) / ( corners[3].X - corners[0].X ) ) :
                ( ( corners[3].Y > corners[0].Y ) ? double.PositiveInfinity : double.NegativeInfinity );

            if ( k2 < k1 )
            {
                IntPoint temp = corners[1];
                corners[1] = corners[2];
                corners[2] = temp;

                double tk = k1;
                k1 = k2;
                k2 = tk;
            }
            if ( k3 < k1 )
            {
                IntPoint temp = corners[1];
                corners[1] = corners[3];
                corners[3] = temp;

                double tk = k1;
                k1 = k3;
                k3 = tk;
            }
            if ( k3 < k2 )
            {
                IntPoint temp = corners[2];
                corners[2] = corners[3];
                corners[3] = temp;

                double tk = k2;
                k2 = k3;
                k3 = tk;
            }

            return corners;
        }
    }
}
