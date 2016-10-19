using System;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 汎用的なKeyValueテンプレートクラス
    /// </summary>
    /// <typeparam name="TKey">キー</typeparam>
    /// <typeparam name="TValue">バリュー</typeparam>
    [Serializable]
    public class Pair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("key:{0}  value:{1}", Key, Value);
        }
    }
}