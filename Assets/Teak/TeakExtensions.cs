/// @cond hide_from_doxygen
using System.Collections.Generic;

namespace TeakExtensions {
    static class Dictionary {
        public static TValue Opt<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue)) {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
/// @endcond
