// Copyright (C) 2017 Trinidad Sibajas Bodoque
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEditor;

namespace Trisibo
{
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldEditor : PropertyDrawer
    {
        // Scene in build data:
        const float sceneInBuildSeparationLeft  = 1;
        const float sceneInBuildSeparationRight = 10;
        const float sceneInBuildSeparationTotal = sceneInBuildSeparationLeft + sceneInBuildSeparationRight;

        GUIContent sceneInBuildYesContent        = new GUIContent("In build");
        GUIContent sceneInBuildNoContent         = new GUIContent("Not in build");
        GUIContent sceneInBuildUnassignedContent = new GUIContent("Unassigned");
        GUIContent sceneInBuildMultipleContent   = new GUIContent("—");

        GUIStyle _sceneInBuildStyle;
        GUIStyle SceneInBuildStyle => _sceneInBuildStyle ?? (_sceneInBuildStyle = new GUIStyle(EditorStyles.miniLabel));
        
        float _buildIndexWidth;
        float BuildIndexWidth
        {
            get
            {
                if (_buildIndexWidth == 0)
                    SceneInBuildStyle.CalcMinMaxWidth(sceneInBuildNoContent, out _buildIndexWidth, out _);
                
                return _buildIndexWidth;
            }
        }


        // Scene is required data:
        GUIContent sceneIsRequiredContent = new GUIContent("Required", "Logs an error and fails the build if the scene is not added to builds");
        
        GUIStyle _sceneIsRequiredStyleNormal;
        GUIStyle SceneIsRequiredStyleNormal => _sceneIsRequiredStyleNormal ?? (_sceneIsRequiredStyleNormal = new GUIStyle(EditorStyles.miniLabel));
        
        GUIStyle _sceneIsRequiredStylePrefabOverride;
        GUIStyle SceneIsRequiredStylePrefabOverride => _sceneIsRequiredStylePrefabOverride ?? (_sceneIsRequiredStylePrefabOverride = new GUIStyle(EditorStyles.miniBoldLabel));

        float _sceneIsRequiredWidth;
        float SceneIsRequiredWidth
        {
            get
            {
                if (_sceneIsRequiredWidth == 0)
                {
                    SceneIsRequiredStylePrefabOverride.CalcMinMaxWidth(sceneIsRequiredContent, out float min, out _);
                    _sceneIsRequiredWidth = min;
                    
                    EditorStyles.toggle.CalcMinMaxWidth(GUIContent.none, out min, out _);
                    _sceneIsRequiredWidth += min;
                }

                return _sceneIsRequiredWidth;
            }
        }








        /// <summary>
        /// Implementation of <see cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>.
        /// </summary>

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty sceneAssetProp = property.FindPropertyRelative("sceneAsset");
            SerializedProperty buildIndexProp = property.FindPropertyRelative("buildIndex");
            SerializedProperty requiredProp   = property.FindPropertyRelative("required");

            position.height = EditorGUIUtility.singleLineHeight;
            
            
            // Scene asset:
            position.width -= BuildIndexWidth + sceneInBuildSeparationTotal + SceneIsRequiredWidth;
            
            using (new EditorGUI.PropertyScope(position, label, sceneAssetProp))
            {
                EditorGUI.PropertyField(position, sceneAssetProp, label);
            }


            // Is the scene in builds?:
            position.x += position.width + sceneInBuildSeparationLeft;
            position.width = BuildIndexWidth + sceneInBuildSeparationRight;

            if (sceneAssetProp.hasMultipleDifferentValues)
            {
                GUI.Label(position, sceneInBuildMultipleContent, SceneInBuildStyle);
            }
            else if (sceneAssetProp.objectReferenceValue != null)
            {
                bool isInBuilds = buildIndexProp.intValue >= 0;
                
                Color prevColor = GUI.contentColor;
                if (!isInBuilds  &&  requiredProp.boolValue)
                    GUI.contentColor *= Color.red;
            
                GUI.Label(position, isInBuilds ? sceneInBuildYesContent : sceneInBuildNoContent, SceneInBuildStyle);

                GUI.contentColor = prevColor;
            }
            else if (requiredProp.boolValue)
            {
                Color prevColor = GUI.contentColor;
                GUI.contentColor *= Color.red;
                GUI.Label(position, sceneInBuildUnassignedContent, SceneInBuildStyle);
                GUI.contentColor = prevColor;
            }


            // Scene required:
            position.x += position.width;
            position.width = SceneIsRequiredWidth;

            using (new EditorGUI.PropertyScope(position, sceneIsRequiredContent, requiredProp))
            using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.showMixedValue = requiredProp.hasMultipleDifferentValues;
                bool newValue = EditorGUI.ToggleLeft(position, sceneIsRequiredContent, requiredProp.boolValue, requiredProp.prefabOverride && !requiredProp.hasMultipleDifferentValues  ?  SceneIsRequiredStylePrefabOverride  :  SceneIsRequiredStyleNormal);
                EditorGUI.showMixedValue = false;
                
                if (changeCheck.changed)
                    requiredProp.boolValue = newValue;
            }
        }
    }
}
