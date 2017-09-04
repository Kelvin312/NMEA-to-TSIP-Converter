using System;

namespace Test
{
    public class KeyValueHolder<TKey, TValue> : IComparable<KeyValueHolder<TKey, TValue>>
        where TKey : IComparable<TKey>
    {
        public KeyValueHolder(TKey key, TValue value = default(TValue))
        {
            this.Key = key;
            this.Value = value;
        }

        public TKey Key { get; }
        public TValue Value { get; }

        public int CompareTo(KeyValueHolder<TKey, TValue> other)
        {
            return this.Key.CompareTo(other.Key);
        }

        public override bool Equals(object obj)
        {
            var other = obj as KeyValueHolder<TKey, TValue>;
            return other != null && other.Key.Equals(this.Key);
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }
    }
}