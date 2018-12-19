﻿using UnityEngine;
using UnityEditor;
using E = UnityEditor;
using System.Linq;

namespace NonIntersecting3dGraphs.Editor {

    [CustomEditor(typeof(MultiSpawner))]
    public class MultiSpawnerEditor : E.Editor {

        private MultiSpawner _spawner;

        private void OnEnable() => _spawner = (MultiSpawner)target;
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Spawn"))
                _spawner.Spawn();
        }

    }

}
