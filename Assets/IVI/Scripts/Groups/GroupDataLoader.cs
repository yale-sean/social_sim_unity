using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IVI
{
    public class GroupDataLoader : MonoBehaviour
    {
        public static List<GroupData> groupData;

        void Awake()
        {
            var sr = new StreamReader(Path.Combine(Application.streamingAssetsPath, "tracking.csv"));
            string line;
            var data = new List<List<string>>();
            while ((line = sr.ReadLine()) != null)
            {
                var tokens = line.Split(',');
                var frameData = new List<string>() { tokens[0], tokens[3], tokens[4], tokens[5] };
                data.Add(frameData);

                System.Console.WriteLine(line);
            }

            var currTime = "";
            groupData = new List<GroupData>();
            for (int i = 1; i < data.Count; i++)
            {
                var time = data[i][0];
                var pos = new Vector3(float.Parse(data[i][1]), 0, float.Parse(data[i][2]));
                var ang = float.Parse(data[i][3]);
                var dir = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang));

                if (!time.Equals(currTime))
                {
                    groupData.Add(new GroupData());
                    currTime = time;
                }

                groupData.Last().pos.Add(pos);
                groupData.Last().ang.Add(ang);
                groupData.Last().dir.Add(dir);
            }

            foreach (var group in groupData)
                group.Standardize();

            //var temp = groupData[1];
            //for (int i = 0; i < temp.pos.Count; i++)
            //{
            //    Debug.DrawLine(temp.pos[i], temp.pos[i] + Vector3.up * 3, Color.red, 100);
            //    Debug.DrawLine(temp.pos[i], temp.pos[i] + temp.dir[i], Color.green, 100);
            //}
        }

        #region Utility Classes

        public class GroupData
        {
            public List<Vector3> pos;
            public List<float> ang;
            public List<Vector3> dir;

            public GroupData()
            {
                pos = new List<Vector3>();
                ang = new List<float>();
                dir = new List<Vector3>();
            }

            public void Standardize()
            {
                var avg = Vector3.zero;
                foreach (var p in pos)
                    avg += p;
                avg /= pos.Count;

                for (int i = 0; i < pos.Count; i++)
                    pos[i] -= avg;

                //var minDist = pos.Min(p => p.magnitude);
            }
        }

        #endregion
    }
}
