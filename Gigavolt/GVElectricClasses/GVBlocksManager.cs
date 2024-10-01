using System;
using System.Collections.Generic;

namespace Game {
    public static class GVBlocksManager {
        public static int GetBlockIndex<T>(bool findSubtypes = true, bool throwIfNotFound = true) {
            Type type = typeof(T);
            if (BlocksManager.BlockTypeToIndex.TryGetValue(type, out int result)
                && result is > -1 and < 1024) {
                return result;
            }
            if (findSubtypes) {
                foreach (KeyValuePair<Type, int> pair in BlocksManager.BlockTypeToIndex) {
                    if (pair.Key.IsSubclassOf(type)) {
                        return pair.Value;
                    }
                }
            }
            throw new KeyNotFoundException($"Block with name <{typeof(T).Name}> is not found.");
        }

        public static T GetBlock<T>(bool findSubtypes = true, bool throwIfNotFound = true) where T : Block {
            if (BlocksManager.Blocks[GetBlockIndex<T>(findSubtypes, throwIfNotFound)] is T result) {
                return result;
            }
            if (throwIfNotFound) {
                throw new KeyNotFoundException($"Block with name <{typeof(T).Name}> is not found.");
            }
            return null;
        }
    }
}