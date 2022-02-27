// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using UnityEngine;

namespace SEAN.Util
{
    public class Unity
    {
        // from: https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html
        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            if (typeof(T) == typeof(Camera))
            {
                (copy as Camera).CopyFrom(original as Camera);
            }
            else
            {
                throw new System.ArgumentException("CopyComponent: Unsupported type: " + type);
                // TODO: Also needs to iterate  SerializedProperties
            }
            return copy as T;
        }
    }
}
