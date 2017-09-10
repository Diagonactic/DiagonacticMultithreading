using System;
using System.Collections.Generic;

namespace Diagonactic.Multithreading
{
    public enum LockResultOutcome : byte
    {
        /// <summary>The <see cref="LockResult{T}"/> was not started</summary>
        Unset = 0,
        /// <summary>The <see cref="LockResult{T}"/> executed all delegates and completed without timing out or failing any conditions.</summary>
        Completed = 1,
        /// <summary>The <see cref="LockResult{T}"/> timed out while acquiring the lock necessary to reach the <see cref="Completed"/> state.</summary>
        TimedOut = 2,
        /// <summary>The <see cref="LockResult{T}"/> executed the condition required for executing the second delegate, but the conditional portion returned false.</summary>
        ConditionalFailure = 3
    }
    public class ConditionalLockResult<T>
    {
        
    }

    public class LockResult : IEquatable<LockResult>
    {
        public LockResultOutcome State { get; }
        public virtual object ResultObject { get; }

        protected LockResult(object result)
        {
            State = LockResultOutcome.Completed;
            ResultObject = result;
        }

        protected LockResult(LockResultOutcome state, object nullResult)
        {
            State = state;
            ResultObject = nullResult;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LockResult) obj);
        }

        public bool Equals(LockResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return State == other.State && Equals(ResultObject, other.ResultObject);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) State*397) ^ (ResultObject != null ? ResultObject.GetHashCode() : 0);
            }
        }

        public static bool operator ==(LockResult left, LockResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LockResult left, LockResult right)
        {
            return !Equals(left, right);
        }
    }
    /// <summary>
    /// The result of a lock extension method
    /// </summary>
    /// <typeparam name="T">The return type of the delegate used in the extension method</typeparam>
    public class LockResult<T> : LockResult, IEquatable<LockResult<T>>
    {
        internal LockResult(LockResultOutcome outcome) : base(outcome) { }

        internal LockResult(T result) : base(result)
        {
            Result = result;
        }

        /// <summary>
        /// Value will be <see langword="true"/> if the lock operation times out; otherwise <see langword="false"/>
        /// </summary>

        /// <summary>
        /// The result of the delegate if it did not time out, otherwise default(T);
        /// </summary>
        public T Result { get; }

        public static explicit operator T(LockResult<T> lockResult)
        {
            return lockResult.Result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LockResult<T>) obj);
        }

        public bool Equals(LockResult<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && EqualityComparer<T>.Default.Equals(Result, other.Result);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ EqualityComparer<T>.Default.GetHashCode(Result);
            }
        }

        public static bool operator ==(LockResult<T> left, LockResult<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LockResult<T> left, LockResult<T> right)
        {
            return !Equals(left, right);
        }
    }
}