// AForge Genetic Library
// AForge.NET framework
//
// Copyright � Andrew Kirillov, 2006-2008
// andrew.kirillov@gmail.com
//


namespace AForge.Genetic
{
    using System;
    using System.Text;
    using AForge.Math.Random;

    /// <summary>
    /// Double array chromosome.
    /// </summary>
    /// 
    /// <remarks><para>Double array chromosome represents array of double values.
    /// Array length is in the range of [2, 65536].
    /// </para></remarks>
    /// 
    public class DoubleArrayChromosome : IChromosome
    {
        /// <summary>
        /// Chromosome generator.
        /// </summary>
        /// 
        /// <remarks><para>This random number generator is used to initialize chromosome's genes,
        /// which is done by calling <see cref="Generate"/> method.</para></remarks>
        /// 
        protected IRandomNumberGenerator chromosomeGenerator;

        /// <summary>
        /// Mutation generator.
        /// </summary>
        /// 
        /// <remarks><para>This random number generator is used to generate random values,
        /// which are added to chromosome's genes during mutation.</para></remarks>
        /// 
        protected IRandomNumberGenerator mutationGenerator;

        /// <summary>
        /// Random number generator for crossover and mutation points selection.
        /// </summary>
        /// 
        /// <remarks><para>This random number generator is used to select crossover
        /// and mutation points.</para></remarks>
        /// 
        protected static Random rand = new Random( );

        /// <summary>
        /// Chromosome's maximum length.
        /// </summary>
        /// 
        /// <remarks><para>Maxim chromosome's length in array elements.</para></remarks>
        /// 
        public const int MaxLength = 65536;
        
        /// <summary>
        /// Chromosome's length in number of elements.
        /// </summary>
        private int length;

        /// <summary>
        /// Chromosome's value.
        /// </summary>
        protected double[] val = null;

        /// <summary>
        /// Chromosome's fintess value.
        /// </summary>
        protected double fitness = 0;

        /// <summary>
        /// Chromosome's length.
        /// </summary>
        /// 
        /// <remarks><para>Length of the chromosome in array elements.</para></remarks>
        ///
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// Chromosome's value.
        /// </summary>
        /// 
        /// <remarks><para>Current value of the chromosome.</para></remarks>
        ///
        public double[] Value
        {
            get { return val; }
        }

        /// <summary>
        /// Chromosome's fintess value.
        /// </summary>
        /// 
        /// <remarks><para>Fitness value (usefulness) of the chromosome calculate by calling
        /// <see cref="Evaluate"/> method. The greater the value, the more useful the chromosome.
        /// </para></remarks>
        /// 
        public double Fitness
        {
            get { return fitness; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleArrayChromosome"/> class.
        /// </summary>
        /// 
        /// <param name="chromosomeGenerator">Chromosome generator - random number generator, which is 
        /// used to initialize chromosome's genes, which is done by calling <see cref="Generate"/> method
        /// or in class constructor.</param>
        /// <param name="mutationGenerator">Mutation generator - random number generator, which is
        /// used to generate random values, which are added to chromosome's genes during mutation.</param>
        /// <param name="length">Chromosome's length in array elements, [2, <see cref="MaxLength"/>].</param>
        /// 
        public DoubleArrayChromosome(
            IRandomNumberGenerator chromosomeGenerator,
            IRandomNumberGenerator mutationGenerator,
            int length )
        {
            // save parameters
            this.chromosomeGenerator = chromosomeGenerator;
            this.mutationGenerator   = mutationGenerator;
            this.length = Math.Max( 2, Math.Min( MaxLength, length ) ); ;

            // allocate array
            val = new double[length];

            // generate random chromosome
            Generate( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleArrayChromosome"/> class.
        /// </summary>
        /// 
        /// <param name="source">Source chromosome to copy.</param>
        /// 
        /// <remarks><para>This is a copy constructor, which creates the exact copy
        /// of specified chromosome.</para></remarks>
        /// 
        public DoubleArrayChromosome( DoubleArrayChromosome source )
        {
            this.chromosomeGenerator = source.chromosomeGenerator;
            this.mutationGenerator   = source.mutationGenerator;
            this.length  = source.length;
            this.fitness = source.fitness;

            // copy genes
            val = (double[]) source.val.Clone( );
        }

        /// <summary>
        /// Get string representation of the chromosome.
        /// </summary>
        /// 
        /// <returns>Returns string representation of the chromosome.</returns>
        /// 
        public override string ToString( )
        {
            StringBuilder sb = new StringBuilder( );

            // append first gene
            sb.Append( val[0] );
            // append all other genes
            for ( int i = 1; i < length; i++ )
            {
                sb.Append( ' ' );
                sb.Append( val[i] );
            }

            return sb.ToString( );
        }

        /// <summary>
        /// Compare two chromosomes.
        /// </summary>
        /// 
        /// <param name="o">Short array chromosome to compare to.</param>
        /// 
        /// <returns>Returns comparison result, which equals to 0 if fitness values
        /// of both chromosomes are equal, 1 if fitness value of this chromosome
        /// is less than fitness value of the specified chromosome, -1 otherwise.</returns>
        ///
        public int CompareTo( object o )
        {
            double f = ( (DoubleArrayChromosome) o ).fitness;

            return ( fitness == f ) ? 0 : ( fitness < f ) ? 1 : -1;
        }

        /// <summary>
        /// Generate random chromosome value.
        /// </summary>
        /// 
        /// <remarks><para>Regenerates chromosome's value using random number generator.</para>
        /// </remarks>
        ///
        public virtual void Generate( )
        {
            for ( int i = 0; i < length; i++ )
            {
                // generate next value
                val[i] = chromosomeGenerator.Next( );
            }
        }

        /// <summary>
        /// Create new random chromosome with same parameters (factory method).
        /// </summary>
        /// 
        /// <remarks><para>The method creates new chromosome of the same type, but randomly
        /// initialized. The method is useful as factory method for those classes, which work
        /// with chromosome's interface, but not with particular chromosome type.</para></remarks>
        ///
        public virtual IChromosome CreateOffspring( )
        {
            return new DoubleArrayChromosome( chromosomeGenerator, mutationGenerator, length );
        }

        /// <summary>
        /// Clone the chromosome.
        /// </summary>
        /// 
        /// <remarks><para>The method clones the chromosome return the exact copy of it.</para>
        /// </remarks>
        ///
        public virtual IChromosome Clone( )
        {
            return new DoubleArrayChromosome( this );
        }

        /// <summary>
        /// Mutation operator.
        /// </summary>
        /// 
        /// <remarks><para>The method performs chromosome's mutation, adding random numbers
        /// to each chromosome's gene. These random numbers are generated with help of
        /// <see cref="mutationGenerator">mutation generator</see>.</para></remarks>
        /// 
        public virtual void Mutate( )
        {
            for ( int i = 0; i < length; i++ )
            {
                // generate next value
                val[i] += mutationGenerator.Next( );
            }
        }

        /// <summary>
        /// Crossover operator.
        /// </summary>
        /// 
        /// <param name="pair">Pair chromosome to crossover with.</param>
        /// 
        /// <remarks><para>The method performs crossover between two chromosomes � interchanging
        /// range of genes (array elements) between these chromosomes.</para></remarks>
        ///
        public virtual void Crossover( IChromosome pair )
        {
            DoubleArrayChromosome p = (DoubleArrayChromosome) pair;

            // check for correct pair
            if ( ( p != null ) && ( p.length == length ) )
            {
                // crossover point
                int crossOverPoint = rand.Next( length - 1 ) + 1;
                // length of chromosome to be crossed
                int crossOverLength = length - crossOverPoint;
                // temporary array
                double[] temp = new double[crossOverLength];

                // copy part of first (this) chromosome to temp
                Array.Copy( val, crossOverPoint, temp, 0, crossOverLength );
                // copy part of second (pair) chromosome to the first
                Array.Copy( p.val, crossOverPoint, val, crossOverPoint, crossOverLength );
                // copy temp to the second
                Array.Copy( temp, 0, p.val, crossOverPoint, crossOverLength );
            }
        }

        /// <summary>
        /// Evaluate chromosome with specified fitness function.
        /// </summary>
        /// 
        /// <param name="function">Fitness function to use for evaluation of the chromosome.</param>
        /// 
        /// <remarks><para>Calculates chromosome's fitness using the specifed fitness function.</para></remarks>
        ///
        public void Evaluate( IFitnessFunction function )
        {
            fitness = function.Evaluate( this );
        }
    }
}
