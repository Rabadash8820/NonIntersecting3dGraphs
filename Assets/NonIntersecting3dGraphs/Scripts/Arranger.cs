using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NonIntersecting3dGraphs {

    public class Arranger : MonoBehaviour {

        private Transform[][] _nodes = Array.Empty<Transform[]>();
        private readonly IList<Vector3> _intersections = new List<Vector3>();

        [HideInInspector]
        public MultiSpawner GraphSpawner;

        [Header("Group Gizmos")]
        public bool RotateGroups = true;
        public Gradient GroupEdgeGradient;
        public bool UseSeparateIntraGroupEdgeGradient = true;
        public Gradient IntraGroupEdgeGradient;

        [Header("Edge Intersection Gizmos")]
        public string CheckIntersectionsButton = "CheckIntersections";
        public float IntersectionMarkerRadius = 0.05f;
        public Color IntersectionMarkerColor = Color.magenta;

        private void OnDrawGizmos() {
            if (_nodes.Length == 0)
                return;

            // Draw edges between all pairs of nodes
            for (int g1 = 0; g1 < _nodes.Length; ++g1) {
                Gizmos.color = GroupEdgeGradient.Evaluate((float)g1 / _nodes.Length);
                for (int n1 = 0; n1 < _nodes[g1].Length; ++n1) {
                    for (int g2 = g1; g2 < _nodes.Length; ++g2) {
                        int initN2 = (g2 == g1) ? n1 + 1 : 0;
                        if (UseSeparateIntraGroupEdgeGradient) {
                            Gradient grad = (g2 == g1) ? IntraGroupEdgeGradient : GroupEdgeGradient;
                            Gizmos.color = grad.Evaluate((float)g1 / _nodes.Length);
                        }
                        for (int n2 = initN2; n2 < _nodes[g2].Length; ++n2) {
                            Gizmos.DrawLine(_nodes[g1][n1].position, _nodes[g2][n2].position);
                        }
                    }
                }
            }

            // Draw edge intersections
            Gizmos.color = IntersectionMarkerColor;
            for (int i = 0; i < _intersections.Count; ++i)
                Gizmos.DrawSphere(_intersections[i], IntersectionMarkerRadius);
        }

        public void ResetNodes() {
            // Delete all existing nodes
            Debug.Log("Delete existing");
            GameObject[] children =
                GraphSpawner.CloneParent
                .GetComponentsInChildren<Transform>()
                .Except(new[] { GraphSpawner.CloneParent })
                .Select(t => t.gameObject)
                .ToArray();
            for (int ch = 0; ch < children.Length; ++ch)
                DestroyImmediate(children[ch]);

            // Respawn nodes
            Debug.Log("Spawn");
            GraphSpawner.Spawn();
            Debug.Log("Get nodes");
            getNodes();

            // Adjust nodes, if requested
            Debug.Log("Rotate");
            if (RotateGroups)
                rotateGroups();

            // Check for edge intersections
            Debug.Log("Intersections");
            checkIntersections();
        }

        private void getNodes() {
            _nodes =
                GameObject.FindGameObjectsWithTag(GraphSpawner.GroupTag)
                          .Select(g =>
                            g.GetComponentsInChildren<Transform>()
                             .Except(new[] { g.transform })
                             .ToArray()
                          ).ToArray();
        }
        private void rotateGroups() {
            float baseGrpRot = (360f / GraphSpawner.NumToGroupBy) / _nodes.Length;
            for (int g = 0; g < _nodes.Length - 1; ++g) {
                Transform trans = _nodes[g][0].parent.transform;
                trans.localRotation = Quaternion.Euler(g * baseGrpRot * Vector3.up);
            }
        }
        public void checkIntersections() {
            Gizmos.color = IntersectionMarkerColor;

            _intersections.Clear();

            // For each possible edge...
            for (int e1g1 = 0; e1g1 < _nodes.Length - 1; ++e1g1) {
                for (int e1n1 = 0; e1n1 < _nodes[e1g1].Length; ++e1n1) {
                    for (int e1g2 = e1g1; e1g2 < _nodes.Length; ++e1g2) {
                        int e1InitN2 = (e1g2 == e1g1) ? e1n1 + 1 : 0;
                        for (int e1n2 = e1InitN2; e1n2 < _nodes[e1g2].Length; ++e1n2) {

                            // For each edge that isn't incident on one of the same nodes...
                            for (int e2g1 = 0; e2g1 < _nodes.Length - 1; ++e2g1) {
                                for (int e2n1 = 0; e2n1 < _nodes[e2g1].Length; ++e2n1) {
                                    if (e2g1 == e1g1 && e2n1 == e1n1)
                                        continue;
                                    for (int e2g2 = e2g1; e2g2 < _nodes.Length; ++e2g2) {
                                        int e2InitN2 = (e2g2 == e2g1) ? e2n1 + 1 : 0;
                                        for (int e2n2 = e2InitN2; e2n2 < _nodes[e2g2].Length; ++e2n2) {
                                            if (e2g2 == e1g2 && e2n2 == e1n2)
                                                continue;

                                            // Check if these two edges intersect
                                            Vector2 pos1 = _nodes[e1g1][e1n1].position;
                                            Vector2 pos2 = _nodes[e2g1][e2n1].position;
                                            Vector2 dir1 = pos1 - (Vector2)_nodes[e1g2][e1n2].position;
                                            Vector2 dir2 = pos2 - (Vector2)_nodes[e2g2][e2n2].position;
                                            float determinant = dir1.x * -dir2.y - dir2.x * -dir1.y;
                                            if (determinant == 0f)
                                                continue;

                                            // If so, then draw a marker at the point of intersection
                                            var inverseMatrix = new Matrix4x4();
                                            inverseMatrix.SetRow(0, 1f / determinant * -dir2);
                                            inverseMatrix.SetRow(1, 1f / determinant * dir1);
                                            var posMatrix = new Matrix4x4(pos1 - pos2, Vector4.zero, Vector4.zero, Vector4.zero);
                                            Matrix4x4 resMatrix = inverseMatrix * posMatrix;
                                            float param = resMatrix[0, 0];
                                            Vector3 intersect = dir1 * param + pos1;
                                            _intersections.Add(intersect);

                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

    }


}
