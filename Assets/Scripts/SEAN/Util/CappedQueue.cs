// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;

namespace SEAN.Util
{
    public sealed class CappedQueue<T> : Queue<T>
    {
        public int Capacity { get; }
        public CappedQueue(int capacity)
        {
            this.Capacity = capacity;
        }
        public new T Enqueue(T item)
        {
            base.Enqueue(item);
            if (base.Count > Capacity)
            {
                return base.Dequeue();
            }
            return default;
        }
    }
}