// AForge Image Processing Library
// AForge.NET framework
//
// Copyright � Andrew Kirillov, 2005-2009
// andrew.kirillov@aforgenet.com
//

namespace AForge.Imaging
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    /// <summary>
    /// Blob counter - counts objects in binrary image.
    /// </summary>
    /// 
    /// <remarks><para>The class counts and extracts stand alone objects in
    /// binary images using connected components labeling algorithm.</para>
    /// 
    /// <para><note>The algorithm treats all black pixels as background, but not an object.
    /// This means that all objects, which could be located be the algorithm, should have other
    /// than black color.</note></para>
    /// 
    /// <para>For blobs' searching the class supports only 8 bpp indexed grayscale images. 
    /// See documentation about <see cref="BlobCounterBase"/> for information about which
    /// pixel formats are supported for extraction of blobs.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create an instance of blob counter algorithm
    /// BlobCounter bc = new BlobCounter( );
    /// // process binary image
    /// bc.ProcessImage( image );
    /// Rectangle[] rects = bc.GetObjectsRectangles( );
    /// // process blobs
    /// foreach ( Rectangle rect in rects )
    /// {
    ///     // ...
    /// }
    /// </code>
    /// </remarks>
    /// 
    public class BlobCounter : BlobCounterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounter"/> class.
        /// </summary>
        /// 
        /// <remarks>Creates new instance of the <see cref="BlobCounter"/> class with
        /// an empty objects map. Before using methods, which provide information about blobs
        /// or extract them, the <see cref="BlobCounterBase.ProcessImage(Bitmap)"/>,
        /// <see cref="BlobCounterBase.ProcessImage(BitmapData)"/> or <see cref="BlobCounterBase.ProcessImage(UnmanagedImage)"/>
        /// method should be called to collect objects map.</remarks>
        /// 
        public BlobCounter( ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounter"/> class.
        /// </summary>
        /// 
        /// <param name="image">Binary image to look for objects in.</param>
        /// 
        public BlobCounter( Bitmap image ) : base( image ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounter"/> class.
        /// </summary>
        /// 
        /// <param name="imageData">Binary image data to look for objects in.</param>
        /// 
        public BlobCounter( BitmapData imageData ) : base( imageData ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounter"/> class.
        /// </summary>
        /// 
        /// <param name="image">Binary unmanaged image to look for objects in.</param>
        /// 
        public BlobCounter( UnmanagedImage image ) : base( image ) { }

        /// <summary>
        /// Actual objects map building.
        /// </summary>
        /// 
        /// <param name="image">Unmanaged image to process.</param>
        /// 
        /// <remarks>The method supports only 8 bpp indexed grayscale image.</remarks>
        /// 
        /// <exception cref="UnsupportedImageFormat">Unsupported pixel format of the source image.</exception>
        /// 
        protected override void BuildObjectsMap( UnmanagedImage image )
        {
            int stride = image.Stride;
            int offset = stride - imageWidth;

            // check pixel format
            if ( image.PixelFormat != PixelFormat.Format8bppIndexed )
            {
                throw new UnsupportedImageFormat( "Unsupported pixel format of the source image." );
            }

            // we don't want one pixel width images
            if ( imageWidth == 1 )
                throw new InvalidImageProperties( "Too small image." );

            // allocate labels array
            objectLabels = new int[imageWidth * imageHeight];
            // initial labels count
            int labelsCount = 0;

            // create map
            int maxObjects = ( ( imageWidth / 2 ) + 1 ) * ( ( imageHeight / 2 ) + 1 ) + 1;
            int[] map = new int[maxObjects];

            // initially map all labels to themself
            for ( int i = 0; i < maxObjects; i++ )
            {
                map[i] = i;
            }

            // do the job
            unsafe
            {
                byte* src = (byte*) image.ImageData.ToPointer( );
                int p = 0;

                // 1 - for pixels of the first row
                if ( *src != 0 )
                {
                    objectLabels[p] = ++labelsCount;
                }
                ++src;
                ++p;

                // process the rest of the first row
                for ( int x = 1; x < imageWidth; x++, src++, p++ )
                {
                    // check if we need to label current pixel
                    if ( *src != 0 )
                    {
                        // check if the previous pixel already was labeled
                        if ( src[-1] != 0 )
                        {
                            // label current pixel, as the previous
                            objectLabels[p] = objectLabels[p - 1];
                        }
                        else
                        {
                            // create new label
                            objectLabels[p] = ++labelsCount;
                        }
                    }
                }
                src += offset;

                // 2 - for other rows
                // for each row
                for ( int y = 1; y < imageHeight; y++ )
                {
                    // for the first pixel of the row, we need to check
                    // only upper and upper-right pixels
                    if ( *src != 0 )
                    {
                        // check surrounding pixels
                        if ( src[-stride] != 0 )
                        {
                            // label current pixel, as the above
                            objectLabels[p] = objectLabels[p - imageWidth];
                        }
                        else if ( src[1 - stride] != 0 )
                        {
                            // label current pixel, as the above right
                            objectLabels[p] = objectLabels[p + 1 - imageWidth];
                        }
                        else
                        {
                            // create new label
                            objectLabels[p] = ++labelsCount;
                        }
                    }
                    ++src;
                    ++p;

                    // check left pixel and three upper pixels for the rest of pixels
                    for ( int x = 1; x < imageWidth - 1; x++, src++, p++ )
                    {
                        if ( *src != 0 )
                        {
                            // check surrounding pixels
                            if ( src[-1] != 0 )
                            {
                                // label current pixel, as the left
                                objectLabels[p] = objectLabels[p - 1];
                            }
                            else if ( src[-1 - stride] != 0 )
                            {
                                // label current pixel, as the above left
                                objectLabels[p] = objectLabels[p - 1 - imageWidth];
                            }
                            else if ( src[-stride] != 0 )
                            {
                                // label current pixel, as the above
                                objectLabels[p] = objectLabels[p - imageWidth];
                            }

                            if ( src[1 - stride] != 0 )
                            {
                                if ( objectLabels[p] == 0 )
                                {
                                    // label current pixel, as the above right
                                    objectLabels[p] = objectLabels[p + 1 - imageWidth];
                                }
                                else
                                {
                                    int l1 = objectLabels[p];
                                    int l2 = objectLabels[p + 1 - imageWidth];

                                    if ( ( l1 != l2 ) && ( map[l1] != map[l2] ) )
                                    {
                                        // merge
                                        if ( map[l1] == l1 )
                                        {
                                            // map left value to the right
                                            map[l1] = map[l2];
                                        }
                                        else if ( map[l2] == l2 )
                                        {
                                            // map right value to the left
                                            map[l2] = map[l1];
                                        }
                                        else
                                        {
                                            // both values already mapped
                                            map[map[l1]] = map[l2];
                                            map[l1] = map[l2];
                                        }

                                        // reindex
                                        for ( int i = 1; i <= labelsCount; i++ )
                                        {
                                            if ( map[i] != i )
                                            {
                                                // reindex
                                                int j = map[i];
                                                while ( j != map[j] )
                                                {
                                                    j = map[j];
                                                }
                                                map[i] = j;
                                            }
                                        }
                                    }
                                }
                            }

                            // label the object if it is not yet
                            if ( objectLabels[p] == 0 )
                            {
                                // create new label
                                objectLabels[p] = ++labelsCount;
                            }
                        }
                    }

                    // for the last pixel of the row, we need to check
                    // only upper and upper-left pixels
                    if ( *src != 0 )
                    {
                        // check surrounding pixels
                        if ( src[-1] != 0 )
                        {
                            // label current pixel, as the left
                            objectLabels[p] = objectLabels[p - 1];
                        }
                        else if ( src[-1 - stride] != 0 )
                        {
                            // label current pixel, as the above left
                            objectLabels[p] = objectLabels[p - 1 - imageWidth];
                        }
                        else if ( src[-stride] != 0 )
                        {
                            // label current pixel, as the above
                            objectLabels[p] = objectLabels[p - imageWidth];
                        }
                        else
                        {
                            // create new label
                            objectLabels[p] = ++labelsCount;
                        }
                    }
                    ++src;
                    ++p;

                    src += offset;
                }
            }

            // allocate remapping array
            int[] reMap = new int[map.Length];

            // count objects and prepare remapping array
            objectsCount = 0;
            for ( int i = 1; i <= labelsCount; i++ )
            {
                if ( map[i] == i )
                {
                    // increase objects count
                    reMap[i] = ++objectsCount;
                }
            }
            // second pass to complete remapping
            for ( int i = 1; i <= labelsCount; i++ )
            {
                if ( map[i] != i )
                {
                    reMap[i] = reMap[map[i]];
                }
            }

            // repair object labels
            for ( int i = 0, n = objectLabels.Length; i < n; i++ )
            {
                objectLabels[i] = reMap[objectLabels[i]];
            }
        }
    }
}
