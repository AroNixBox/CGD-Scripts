using UnityEngine;

namespace Common.Runtime._Scripts.Common.Runtime.Extensions {
    public static class ListExtensions {
        public static bool IsNullOrEmpty<T>(this System.Collections.Generic.List<T> list, bool throwException) {
            if (list == null) {
                if (throwException)
                    Debug.LogWarning($"List {nameof(list)} is null.");
                
                return true;
            }

            if (list.Count == 0) {
                if(throwException)
                    Debug.LogWarning($"List {nameof(list)} empty.");
                
                return true;
            }
            
            return list.Count == 0;
        }
        
        public static bool DoesIndexExist<T>(this System.Collections.Generic.List<T> list, int index, bool throwException) {
            if (list == null) return false;

            var outOfRange = index < 0 || index >= list.Count;
            if (outOfRange) {
                if(throwException)
                    Debug.LogWarning($"ListExtensions: Index {index} is out of range for list of count {list.Count}.");
               
                return false;
            }
            
            return true;
        }
    }
}