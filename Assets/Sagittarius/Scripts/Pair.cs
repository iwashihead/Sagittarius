using System;

namespace Griphone.Sagittarius
{
    [Serializable]
    public class Pair<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public Pair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("key:{0}  value:{1}", key, value);
        }
    }
}