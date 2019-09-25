namespace Fanex.Bot.Core._Shared.Enumerations
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [Serializable]
    public class Enumeration : IComparable
    {
        protected Enumeration()
        {
        }

        protected Enumeration(byte value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }

        public byte Value { get; set; }

        public static byte AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
        {
            var absoluteDifference = (byte)Math.Abs(firstValue.Value - secondValue.Value);
            return absoluteDifference;
        }

        public static T FromDisplayName<T>(string displayName) where T : Enumeration, new()
        {
            var matchingItem = Parse<T, string>(displayName, "display name", item => item.DisplayName == displayName);
            return matchingItem;
        }

        public static T FromValue<T>(byte value) where T : Enumeration, new()
        {
            var matchingItem = Parse<T, byte>(value, "value", item => item.Value == value);
            return matchingItem;
        }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration, new()
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = new T();
                var locatedValue = info.GetValue(instance) as T;

                if (locatedValue != null)
                {
                    yield return locatedValue;
                }
            }
        }

        public static IEnumerable GetAll(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                object instance = Activator.CreateInstance(type);
                yield return info.GetValue(instance);
            }
        }

        public static bool operator ==(Enumeration left, Enumeration right)
            => left is null
            ? right is null
            : !(right is null) && left.Value == right.Value;

        public static bool operator !=(Enumeration left, Enumeration right)
        {
            return !(left == right);
        }

        public static bool operator >(Enumeration left, Enumeration right)
        {
            ValidateInputArguments(left, right);
            return left.Value > right.Value;
        }

        public static bool operator >=(Enumeration left, Enumeration right)
        {
            ValidateInputArguments(left, right);
            return left.Value >= right.Value;
        }

        public static bool operator <(Enumeration left, Enumeration right)
        {
            ValidateInputArguments(left, right);
            return left.Value < right.Value;
        }

        public static bool operator <=(Enumeration left, Enumeration right)
        {
            ValidateInputArguments(left, right);
            return left.Value <= right.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherValue = obj as Enumeration;

            if (otherValue == null)
            {
                return false;
            }

            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = Value.Equals(otherValue.Value);

            return typeMatches && valueMatches;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public virtual int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return Value.CompareTo(((Enumeration)obj).Value);
        }

        private static void ValidateInputArguments(Enumeration left, Enumeration right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }
        }

        private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration, new()
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);

            if (matchingItem == null)
            {
                var message = string.Format("'{0}' is not a valid {1} in {2}", value, description, typeof(T));
                throw new ArgumentOutOfRangeException(message);
            }

            return matchingItem;
        }
    }
}