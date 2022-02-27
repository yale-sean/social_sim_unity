// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

namespace SEAN.Scenario
{
    public class Situation
    {
        private Situation(string name, int idx)
        {
            this.name = name;
            this.idx = idx;
            this.val = 0f;
        }
        public string name { get; private set; }
        public int idx { get; private set; }
        public float val { get; private set; }
        public string ToString() { return name + ": " + val; }
        public static Situation CrossPath { get { return new Situation("cross_path", 0); } }
        public static Situation Empty { get { return new Situation("empty", 1); } }
        public static Situation JoinGroup { get { return new Situation("join_group", 2); } }
        public static Situation LeaveGroup { get { return new Situation("leave_group", 3); } }
        public static Situation DownPath { get { return new Situation("down_path", 4); } }

        public static implicit operator bool(Situation s) => s.val == 1.0f;

        public Situation Set(float val)
        {
            this.val = val;
            return this;
        }
    }
}