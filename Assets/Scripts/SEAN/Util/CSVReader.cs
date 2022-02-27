// Copyright (c) 2021, Members of Yale Interactive Machines Group, Yale University,
// Nathan Tsoi
// All rights reserved.
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace SEAN.Util
{
    public class CSVReader
    {
        // Adapted from: https://bravenewmethod.com/2014/09/13/lightweight-csv-reader-for-unity/
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };
        public static List<Dictionary<int, object>> Read(TextAsset csvFile)
        {
            var list = new List<Dictionary<int, object>>();
            var lines = Regex.Split(csvFile.text, LINE_SPLIT_RE);
            if (lines.Length <= 1) return list;
            for (var i = 0; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;
                var entry = new Dictionary<int, object>();
                for (var j = 0; j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    object finalvalue = value;
                    int n;
                    float f;
                    if (int.TryParse(value, out n))
                    {
                        finalvalue = n;
                    }
                    else if (float.TryParse(value, out f))
                    {
                        finalvalue = f;
                    }
                    entry[j] = finalvalue;
                }
                list.Add(entry);
            }
            return list;
        }
    }
}
