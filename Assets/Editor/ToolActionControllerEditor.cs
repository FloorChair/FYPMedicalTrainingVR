using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ToolActionController))]
public class ToolActionControllerEditor : Editor
{
    private bool showHaptics = true;
    private bool showGuides = true;
    private bool showGhost = true;
    private bool showProjection = true;
    private bool showHighlight = true;

    public override void OnInspectorGUI()
    {
        ToolActionController controller = (ToolActionController)target;

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activateTriggerAction"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useToolBelt"));


        DrawSection("Haptics", ref showHaptics, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableHaptics"));

            if (controller.enableHaptics)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hapticMode"));

                if (controller.hapticMode == ToolActionController.HapticMode.Simple)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hapticAmplitude"));
                }

                if (controller.hapticMode == ToolActionController.HapticMode.Advanced)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onAdvancedHaptics"));
                }
            }
        });

        DrawSection("Guides", ref showGuides, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableGuides"));

            if (controller.enableGuides)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("associatedGuides"), true);
            }
        });

        DrawSection("Ghost Guide", ref showGhost, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableGhostGuide"));

            if (controller.enableGhostGuide)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ghostGuideHideDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ghostGuide"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ghostAnimator"));
            }
        });

        DrawSection("Highlight", ref showHighlight, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableHighlight"));

            if (controller.enableHighlight)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectsTag"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("highlightLayerName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("originalLayerName"));
            }
        });

        DrawSection("Projection", ref showProjection, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableProjection"));

            if (controller.enableProjection)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolTip"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("localForward"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxProjectionDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectionLayer"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectionMarkerPrefab"));
                SerializedProperty projectionModeProp = serializedObject.FindProperty("projectionMode");
                EditorGUILayout.PropertyField(projectionModeProp);
                if ((ToolActionController.ProjectionMode)projectionModeProp.enumValueIndex
                    == ToolActionController.ProjectionMode.DrawLine)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Draw Line Settings", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lineMaterial"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lineWidth"));
                }
            }
        });

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("procedureGenerator"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("controllerDisplay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onTriggerPressed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onTriggerReleased"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onPickup"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onDrop"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onProjectionActionComplete"));

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(string title, ref bool foldout, System.Action drawContent)
    {
        EditorGUILayout.Space();
        foldout = EditorGUILayout.Foldout(foldout, title, true);

        if (foldout)
        {
            EditorGUI.indentLevel++;
            drawContent.Invoke();
            EditorGUI.indentLevel--;
        }
    }
}