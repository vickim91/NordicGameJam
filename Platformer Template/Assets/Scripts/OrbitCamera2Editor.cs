using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OrbitCamera2))]
[CanEditMultipleObjects]
public class OrbitCamera2Editor : Editor
{
    public bool myToggle = false;
    OrbitCamera2 orbitCam;

    SerializedProperty player;

    void OnEnable()
    {
        orbitCam = (OrbitCamera2)target;
        player = serializedObject.FindProperty("player");
    }

    public override void OnInspectorGUI()
    {
        orbitCam.rotationSpeed = EditorGUILayout.FloatField("rotation speed", orbitCam.rotationSpeed);

        serializedObject.Update();
        EditorGUILayout.PropertyField(player);
        serializedObject.ApplyModifiedProperties();
      
        orbitCam.distanceToPlayer = EditorGUILayout.FloatField("Camera distance to player", orbitCam.distanceToPlayer);
        EditorGUILayout.Space();

        orbitCam.useTwoSmoothZones = EditorGUILayout.Toggle("Two follow zones", orbitCam.useTwoSmoothZones);
        EditorGUILayout.Space();

        if (!orbitCam.useTwoSmoothZones)
        {
            orbitCam.smoothTypeInnerCircle = (OrbitCamera2.InnerSmoothingType)EditorGUILayout.EnumPopup("Smoothing type", orbitCam.smoothTypeInnerCircle);
            orbitCam.innerFollowSpeed = EditorGUILayout.Slider("Follow Speed",orbitCam.innerFollowSpeed, 0, 10);
        }

        if (orbitCam.useTwoSmoothZones)
        {
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            orbitCam.circleRadius = EditorGUILayout.Slider("Circle Radius", orbitCam.circleRadius, 0.1f, 4);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inner Circle", EditorStyles.boldLabel);
            orbitCam.smoothTypeInnerCircle = (OrbitCamera2.InnerSmoothingType)EditorGUILayout.EnumPopup("Inner smoothing type",orbitCam.smoothTypeInnerCircle);

            if(orbitCam.smoothTypeInnerCircle != OrbitCamera2.InnerSmoothingType.DontFollow && orbitCam.smoothTypeInnerCircle != OrbitCamera2.InnerSmoothingType.Instant)
                orbitCam.innerFollowSpeed = EditorGUILayout.Slider("Inner follow Speed", orbitCam.innerFollowSpeed, 0, 10);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Outer Circle", EditorStyles.boldLabel);
            orbitCam.smoothTypeOuterCircle = (OrbitCamera2.OuterSmoothingType)EditorGUILayout.EnumPopup("Outer smoothing type", orbitCam.smoothTypeOuterCircle);

            if (orbitCam.smoothTypeOuterCircle != OrbitCamera2.OuterSmoothingType.Instant)
                orbitCam.outerFollowSpeed = EditorGUILayout.Slider("Outer follow Speed", orbitCam.outerFollowSpeed, 0, 10);

        }
    }
}
