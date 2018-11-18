// Copyright (C) 2018 Trinidad Sibajas Bodoque
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
        // Build index parameters:
        const string buildIndexText = "Index: {0}";
        const string buildIndexTextNA = "Index: ‒";
        const string buildIndexTooltip = "Scene build index";

        GUIStyle buildIndexStyle = EditorStyles.miniLabel;
        
        float _buildIndexWidth;
        float BuildIndexWidth
        {
            get
            {
                if (_buildIndexWidth == 0)
                {
                    float min, max;
                    buildIndexStyle.CalcMinMaxWidth(new GUIContent(string.Format(buildIndexText, 9999)), out min, out max);
                    _buildIndexWidth = min;
                }
                
                return _buildIndexWidth;
            }
        }


        // "Throw error if not in build" parameters:
        GUIContent logErrorContent = new GUIContent("Log err.", "Log error if the scene is not added to builds");
        GUIStyle logErrorStyleNormal = EditorStyles.miniLabel;
        GUIStyle logErrorStylePrefabOverride = EditorStyles.miniBoldLabel;

        float _logErrorWidth;
        float LogErrorWidth
        {
            get
            {
                if (_logErrorWidth == 0)
                {
                    float min, max;
                    logErrorStylePrefabOverride.CalcMinMaxWidth(logErrorContent, out min, out max);
                    _logErrorWidth = min;
                    
                    EditorStyles.toggle.CalcMinMaxWidth(GUIContent.none, out min, out max);
                    _logErrorWidth += min;
                }

                return _logErrorWidth;
            }
        }








        /// <summary>
        /// Implementation of <see cref="PropertyDrawer.OnGUI(Rect, SerializedProperty, GUIContent)"/>.
        /// </summary>

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            
            
            // Scene asset:
            position.width -= BuildIndexWidth + LogErrorWidth;
            
            var sceneAssetProp = property.FindPropertyRelative("sceneAsset");
            using (new EditorGUI.PropertyScope(position, label, sceneAssetProp))
            {
                EditorGUI.PropertyField(position, sceneAssetProp, label);
            }


            // Build index:
            position.x += position.width;
            position.width = BuildIndexWidth;
            
            if (property.hasMultipleDifferentValues)
            {
                GUI.Label(position, new GUIContent(buildIndexTextNA, buildIndexTooltip), buildIndexStyle);
            }
            else
            {
                int buildIndex = property.FindPropertyRelative("buildIndex").intValue;
                Color prevColor = GUI.contentColor;
                if (sceneAssetProp.objectReferenceValue != null  &&  buildIndex < 0)
                    GUI.contentColor *= Color.red;
            
                position.width = BuildIndexWidth;
                GUI.Label(position, new GUIContent(sceneAssetProp.objectReferenceValue != null ? string.Format(buildIndexText, buildIndex) : buildIndexTextNA, buildIndexTooltip), buildIndexStyle);

                GUI.contentColor = prevColor;
            }


            // Log error if not in build?:
            position.x += position.width;
            position.width = LogErrorWidth;
            
            var logErrorIfNotInBuildProp = property.FindPropertyRelative("logErrorIfNotInBuild");
            using (new EditorGUI.PropertyScope(position, logErrorContent, logErrorIfNotInBuildProp))
            using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.showMixedValue = logErrorIfNotInBuildProp.hasMultipleDifferentValues;
                bool newValue = EditorGUI.ToggleLeft(position, logErrorContent, logErrorIfNotInBuildProp.boolValue, logErrorIfNotInBuildProp.prefabOverride && !logErrorIfNotInBuildProp.hasMultipleDifferentValues  ?  logErrorStylePrefabOverride  :  logErrorStyleNormal);
                EditorGUI.showMixedValue = false;
                
                if (changeCheck.changed)
                    logErrorIfNotInBuildProp.boolValue = newValue;
            }
        }
    }
}
