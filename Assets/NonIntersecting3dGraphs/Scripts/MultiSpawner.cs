using UnityEngine;

namespace NonIntersecting3dGraphs {

    public class MultiSpawner : MonoBehaviour {

        [Header("Spawn behavior")]
        public GameObject Prefab;
        public int TotalClones = 10;
        public string CloneBaseName = "clone-{0}-{1}";
        public Transform CloneParent;
        public bool SpawnOnStart = true;

        [Header("Group Clones")]
        public int NumToGroupBy = 1;
        public string GroupBaseName = "group-{0}";
        public string GroupTag = "Untagged";
        public LayerMask GroupLayer;
        public Vector3 GroupExtendOffset = Vector3.up;
        public bool SpreadClonesInCircleWithinGroup = true;
        public float SpreadCircleRadius = 1f;
        public bool RotateClonesWithinGroup = true;

        // Use this for initialization
        void Start() {
            if (SpawnOnStart)
                Spawn();
        }

        public void Spawn() {
            int numGrps = Mathf.CeilToInt(TotalClones / (float)NumToGroupBy);
            int copyNum = 0;
            float baseGrpRot = 2f * Mathf.PI / NumToGroupBy;
            for (int g = 0; g < numGrps; ++g) {
                var grpObj = new GameObject(string.Format(GroupBaseName, g)) {
                    tag = GroupTag,
                    layer = GroupLayer.value
                };
                grpObj.transform.parent = CloneParent;
                grpObj.transform.localPosition = g * GroupExtendOffset;
                grpObj.transform.up = GroupExtendOffset;
                for (int c = 0; c < NumToGroupBy && copyNum < TotalClones; ++c) {
                    GameObject cloneObj = Instantiate(Prefab, grpObj.transform);
                    float rads = c * baseGrpRot;
                    if (SpreadClonesInCircleWithinGroup) {
                        var localPos = new Vector3(Mathf.Cos(rads), 0f, Mathf.Sin(rads));
                        cloneObj.transform.localPosition = SpreadCircleRadius * localPos;
                    }
                    if (RotateClonesWithinGroup) {
                        float degs = Mathf.Rad2Deg * rads;
                        cloneObj.transform.localRotation = Quaternion.Euler(-degs * Vector3.up);
                    }
                    cloneObj.name = string.Format(CloneBaseName, g, copyNum);
                    ++copyNum;
                }
            }
        }

    }

}
