namespace Ops.Exchange.Diagnostics;

/// <summary>
/// 表示诊断数据的基类。
/// </summary>
public abstract class DiagnosticsEventData : IReadOnlyList<KeyValuePair<string, object>>
{
    protected const string EventNamespace = "Ops.Exchange.";

    protected abstract int Count { get; }

    protected abstract KeyValuePair<string, object> this[int index] { get; }

    int IReadOnlyCollection<KeyValuePair<string, object>>.Count => Count;

    KeyValuePair<string, object> IReadOnlyList<KeyValuePair<string, object>>.this[int index] => this[index];

    Enumerator GetEnumerator() => new(this);

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<KeyValuePair<string, object>>
    {
        private readonly DiagnosticsEventData _eventData;
        private readonly int _count;
        private int _index;

        public KeyValuePair<string, object> Current { get; private set; }

        internal Enumerator(DiagnosticsEventData eventData)
        {
            _eventData = eventData;
            _count = eventData.Count;
            _index = -1;
            Current = default;
        }

        public bool MoveNext()
        {
            var index = _index + 1;
            if (index >= _count)
            {
                return false;
            }

            _index = index;

            Current = _eventData[index];
            return true;
        }

        public void Dispose() { }

        object IEnumerator.Current => Current;

        void IEnumerator.Reset() => throw new NotSupportedException();
    }
}
