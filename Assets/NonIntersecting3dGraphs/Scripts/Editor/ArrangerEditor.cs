using UnityEditor;
using UnityEngine;
using E = UnityEditor;

namespace NonIntersecting3dGraphs.Editor {

    [CustomEditor(typeof(Arranger))]
    public class ArrangerEditor : E.Editor {

        private Arranger _arranger;
        private E.Editor _spawnerEditor;

        private void OnEnable() => _arranger = (Arranger)target;
        public override void OnInspectorGUI() {
            // Show the Spawner picker
            var newSpawner = (MultiSpawner)EditorGUILayout.ObjectField(
                new GUIContent("Graph Spawner"),
                _arranger.GraphSpawner,
                typeof(MultiSpawner),
                allowSceneObjects: true);
            if (newSpawner == null && _arranger.GraphSpawner != null)
                DestroyImmediate(_spawnerEditor);
            _arranger.GraphSpawner = newSpawner;

            // Show the Spawner inspector right in this Inspector
            if (_spawnerEditor == null && _arranger.GraphSpawner != null)
                _spawnerEditor = CreateEditor(_arranger.GraphSpawner);
            if (_spawnerEditor != null) {
                _spawnerEditor.DrawDefaultInspector();
                EditorGUILayout.Space();
            }

            DrawDefaultInspector();

            // Draw buttons
            EditorGUILayout.Space();
            if (GUILayout.Button($"Reset Nodes"))
                _arranger.ResetNodes();
        }

    }

}
