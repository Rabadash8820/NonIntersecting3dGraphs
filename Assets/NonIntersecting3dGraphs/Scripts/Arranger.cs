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
        public bool DrawEdges = true;
        public bool DrawIntersections = true;
        public float IntersectionMarkerRadius = 0.05f;
        public Color IntersectionMarkerColor = Color.magenta;

        private void OnDrawGizmos() {
            if (_nodes.Length == 0)
                return;

            if (DrawEdges)
                drawEdges();
            if (DrawIntersections)
                drawIntersections();
        }

        public void ResetNodes() {
            // Delete all existing nodes
            GameObject[] children =
                GraphSpawner.CloneParent
                .GetComponentsInChildren<Transform>()
                .Except(new[] { GraphSpawner.CloneParent })
                .Select(t => t.gameObject)
                .ToArray();
            for (int ch = 0; ch < children.Length; ++ch)
                DestroyImmediate(children[ch]);

            // Respawn nodes
            GraphSpawner.Spawn();
            getNodes();

            // Adjust nodes, if requested
            if (RotateGroups)
                rotateGroups();

            // Check for edge intersections
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
        private void drawEdges() {
            for (int g1 = 0; g1 < _nodes.Length; ++g1) {
                Gizmos.color = GroupEdgeGradient.Evaluate((float)g1 / _nodes.Length);
                for (int n1 = 0; n1 < _nodes[g1].Length; ++n1) {
                    for (int g2 = g1; g2 < _nodes.Length; ++g2) {
                        int initN2 = (g2 == g1) ? n1 + 1 : 0;
                        if (UseSeparateIntraGroupEdgeGradient) {
                            Gradient grad = (g2 == g1) ? IntraGroupEdgeGradient : GroupEdgeGradient;
                            Gizmos.color = grad.Evaluate((float)g1 / _nodes.Length);
                        }
                        for (int n2 = initN2; n2 < _nodes[g2].Length; ++n2)
                            Gizmos.DrawLine(_nodes[g1][n1].position, _nodes[g2][n2].position);
                    }
                }
            }
        }
        private void drawIntersections() {
            Gizmos.color = IntersectionMarkerColor;
            for (int i = 0; i < _intersections.Count; ++i)
                Gizmos.DrawSphere(_intersections[i], IntersectionMarkerRadius);
        }
        private void checkIntersections() {
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
                                            Vector3 pos1 = _nodes[e1g1][e1n1].position;
                                            Vector3 pos2 = _nodes[e2g1][e2n1].position;
                                            Vector3 dir1 = pos1 - _nodes[e1g2][e1n2].position;
                                            Vector3 dir2 = pos2 - _nodes[e2g2][e2n2].position;
                                            float determinant = dir1.x * dir2.y - dir2.x * dir1.y;
                                            if (determinant == 0f)
                                                continue;

                                            // Check that the intersection point is inside the "coil" of the graph
                                            float t2 = 1f / determinant * (-dir1.x * (pos2.y - pos1.y) + dir1.y * (pos2.x - pos1.x));
                                            //bool within = (0f < Mathf.Abs(t2) && Mathf.Abs(t2) < Mathf.Abs((pos2.x - pos1.x) / dir1.x));
                                            //if (!within)
                                            //    continue;

                                            // If so, add a marker for this intersection point
                                            Vector3 intersect = pos2 + dir2 * t2;
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
