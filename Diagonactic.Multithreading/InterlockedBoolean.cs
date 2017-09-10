using System;
using System.Collections.Generic;
using System.Threading;

namespace Diagonactic.Multithreading
{
    /// <summary>
    /// A boolean structure that uses Interlocked ints for updates and reads.
    /// </summary>
    /// <remarks>Take caution, this is a struct for memory/performance reasons.  Assignment will result in a copy of the entire structure and assignment of the structure is <strong>not</strong> thread safe</remarks>
    public class InterlockedBoolean : IEquatable<InterlockedBoolean>
    {
        public InterlockedBoolean(bool initialValue = false)
        {
            m_value = initialValue ? IntTrue : IntFalse;
        }

        private const int IntTrue = 1, IntFalse = 0;
        private int m_value;

        /// <summary>
        /// The value of the boolean protected by this struct.
        /// </summary>
        /// <remarks>Assignment of a true or false value to this field is thread safe and will result in the latest value being grabbed whenever it is used, however, assignment of the value to another variable that is not similarly
        /// protected will not result in thread safe guarantees.</remarks>
        public bool Value
        {
            get { return Interlocked.CompareExchange(ref m_value, IntTrue, IntTrue) == IntTrue; }
            set { Interlocked.Exchange(ref m_value, value ? IntTrue : IntFalse); }
        }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => Value.ToString();


        /// <summary>Determines whether the specified <see cref="System.Object" />, is equal to this instance.</summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><see langword="true" /> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        /// <summary>
        /// Determins if this instance's <see cref="Value"/> is equal to <paramref name="@bool"/>
        /// </summary>
        /// <param name="bool">A boolean value to test against this instance's <see cref="Value"/></param>
        /// <returns></returns>
        public bool Equals(bool @bool)
        {
            return Value == @bool;
        }

        public static bool Equals(InterlockedBoolean @this, InterlockedBoolean other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(@this, other)) return true;
            return Interlocked.CompareExchange(ref @this.m_value, IntTrue, IntTrue) == Interlocked.CompareExchange(ref other.m_value, IntTrue, IntTrue);
        }

        public bool Equals(InterlockedBoolean other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return m_value == other.m_value;
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
        public override int GetHashCode()
        {
            return Interlocked.CompareExchange(ref m_value, IntTrue, IntTrue);
        }

        public static bool operator ==(InterlockedBoolean left, InterlockedBoolean right)
        {
            return Equals(left, right);
        }
        
        public static bool operator !=(InterlockedBoolean left, InterlockedBoolean right)
        {
            return !Equals(left, right);
        }

        /// <summary>Performs an explicit conversion from <see cref="InterlockedBoolean"/> to <see cref="System.Boolean"/>.</summary>
        /// <param name="this">The this.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="@bool"/> is <see langword="null"/></exception>
        public static explicit operator bool(InterlockedBoolean @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.Value;
        }
    }
}