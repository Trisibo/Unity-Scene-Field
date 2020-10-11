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

using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace Trisibo
{
    /// <summary>
    /// Makes it possible to assign a scene asset in the inspector and load the scene data in a build.
    /// </summary>

    [Serializable]
    public class SceneField
    #if UNITY_EDITOR
    : ISerializationCallbackReceiver
    #endif
    {
        #region Parameters


        #if UNITY_EDITOR
        [SerializeField] SceneAsset sceneAsset = null;
        [UnityEngine.Serialization.FormerlySerializedAs("logErrorIfNotInBuild")]
        [SerializeField] bool required = false;
        #endif

        #pragma warning disable 414
        [SerializeField] int buildIndex = 0;
        #pragma warning restore 414


        #endregion








        /// <summary>
        /// Gets the scene build index. -1 if no scene was assigned or it's not added to builds.
        /// Don't use it from a <see cref="ISerializationCallbackReceiver"/> method.
        /// </summary>

        public int BuildIndex
        {
            get
            {
                #if UNITY_EDITOR
                {
                    buildIndex = GetSceneBuildIndex(sceneAsset);
                    if (required  &&  buildIndex < 0)
                    {
                        if (sceneAsset != null)
                            Debug.LogError($"Trisibo.SceneField: The following scene is assigned to a scene field as required, but isn't added to builds: {AssetDatabase.GetAssetPath(sceneAsset)}");
                        else
                            Debug.LogError($"Trisibo.SceneField: A scene field is marked as required, but no scene is assigned");
                    }
                }
                #endif

                return buildIndex;
            }
        }








        #region ISerializationCallbackReceiver implementation
        #if UNITY_EDITOR


        /// <summary>
        /// Implementation of <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>.
        /// </summary>

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            buildIndex = GetSceneBuildIndex(sceneAsset);
            
            if (required  &&  buildIndex < 0)
                BuildProcessor.AddMissingRequiredSceneBuildError(sceneAsset);
        }








        /// <summary>
        /// Implementation of <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>.
        /// </summary>

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }


        #endif
        #endregion








        #region Build processor
        #if UNITY_EDITOR


        class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
        {
            static HashSet<SceneAsset> missingRequiredSceneAssets = new HashSet<SceneAsset>();
            static bool requiredSceneIsUnassigned;


            /// <summary>Adds a missing required scene error to be shown when building. The added errors will be cleared when a new build is started.</summary>
            /// <param name="sceneAsset">The asset of the scene missing in the build. Can be null.</param>
            public static void AddMissingRequiredSceneBuildError(SceneAsset sceneAsset)
            {
                if (sceneAsset != null)
                    missingRequiredSceneAssets.Add(sceneAsset);
                else
                    requiredSceneIsUnassigned = true;
            }
            
                
            /// <summary>Implementation of <see cref="IOrderedCallback.callbackOrder"/>.</summary>
            int IOrderedCallback.callbackOrder => 0;


            /// <summary>Implementation of <see cref="IPreprocessBuildWithReport.OnPreprocessBuild"/>.</summary>
            void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
            {
                UpdateCachedBuildIndexes();
                missingRequiredSceneAssets.Clear();
                requiredSceneIsUnassigned = false;
            }


            /// <summary>Implementation of <see cref="IPostprocessBuildWithReport.OnPostprocessBuild"/>.</summary>
            void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
            {
                string errorMessage = null;

                if (requiredSceneIsUnassigned)
                    errorMessage += "  - A required scene field doesn't have an assigned scene";
                
                if (missingRequiredSceneAssets.Count > 0)
                {
                    if (errorMessage != null)
                        errorMessage += "\n";
                    errorMessage += "  - The following scenes are assigned to scene fields as required, but aren't added to builds:";
                    foreach (var sceneAsset in missingRequiredSceneAssets)
                        errorMessage += $"\n    - {AssetDatabase.GetAssetPath(sceneAsset)}";

                    missingRequiredSceneAssets.Clear();
                }

                if (errorMessage != null)
                {
                    errorMessage = $"Trisibo.SceneField: The following errors have been found:\n" + errorMessage;
                    throw new BuildFailedException(errorMessage);
                }
            }
        }


        #endif
        #endregion








        #region Editor members
        #if UNITY_EDITOR


        static Dictionary<SceneAsset, int> cachedBuildIndexes = new Dictionary<SceneAsset, int>();




        /// <summary>
        /// Updates the cached build indexes.
        /// </summary>

        static void UpdateCachedBuildIndexes()
        {
            cachedBuildIndexes.Clear();

            int buildIndex = -1;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    buildIndex++;
                    SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                    if (sceneAsset != null)
                        cachedBuildIndexes.Add(sceneAsset, buildIndex);
                }
            }
        }








        /// <summary>
        /// Called by Unity when loading the editor.
        /// </summary>

        [InitializeOnLoadMethod]
        static void OnEditorInitializeOnLoad()
        {
            UpdateCachedBuildIndexes();

            EditorBuildSettings.sceneListChanged -= UpdateCachedBuildIndexes;
            EditorBuildSettings.sceneListChanged += UpdateCachedBuildIndexes;
        }








        /// <summary>
        /// ** Editor-only **
        /// Gets the scene asset, if assigned.
        /// </summary>

        public SceneAsset EditorSceneAsset => sceneAsset;








        /// <summary>
        /// ** Editor-only **
        /// Retrieves the build index of the specified scene asset.
        /// </summary>
        /// <param name="sceneAsset">The scene asset.</param>
        /// <returns>The build index, -1 if not found.</returns>

        static int GetSceneBuildIndex(SceneAsset sceneAsset)
        {
            if (sceneAsset == null  ||  !cachedBuildIndexes.TryGetValue(sceneAsset, out int buildIndex))
                return -1;
            else
                return buildIndex;
        }


        #endif
        #endregion
    }
}
