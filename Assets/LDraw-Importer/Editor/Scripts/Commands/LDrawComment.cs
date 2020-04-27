using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{
    public class LDrawComment : LDrawCommand
    {
        private string _Content;
        private bool _IsStep;

        public void GetCommentGameObject(Transform parent)
        {
            if (_IsStep)
            {
                GameObject go = new GameObject("Step");
                go.AddComponent<Step>();
                go.transform.parent = parent;
            }
        }

        public override void Deserialize(string serialized)
        {
            // Sanitize and trim
            _Content = serialized.Substring(1).Trim();
            _IsStep = _Content.Equals("STEP");
        }

        public override void PrepareMeshData(List<int> triangles, List<Vector3> verts)
        {

        }
    }

}
