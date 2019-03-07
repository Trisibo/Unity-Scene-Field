// Copyright (C) 2018-2019 Trinidad Sibajas Bodoque
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
        [SerializeField] UnityEditor.SceneAsset sceneAsset = null;
        [SerializeField] bool logErrorIfNotInBuild = false;
        #endif

        #pragma warning disable 414
        [SerializeField] int buildIndex = 0;
        #pragma warning restore 414

        
        #endregion








        /// <summary>
        /// Gets the scene build index.
        /// </summary>

        public int BuildIndex
        {
            get
            {
                #if UNITY_EDITOR
                {
                    return GetBuildIndex(sceneAsset, logErrorIfNotInBuild);
                }
                #else
                {
                    return buildIndex;
                }
                #endif
            }
        }








        #region ISerializationCallbackReceiver implementation
        #if UNITY_EDITOR


        /// <summary>
        /// Implementation of <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>.
        /// </summary>

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            buildIndex = GetBuildIndex(sceneAsset, logErrorIfNotInBuild);
        }








        /// <summary>
        /// Implementation of <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>.
        /// </summary>

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }


        #endif
        #endregion








        #region Editor members
        #if UNITY_EDITOR


        /// <summary>
        /// Retrieves the build index (editor only).
        /// </summary>
        /// <param name="sceneAsset">The scene asset.</param>
        /// <param name="logErrorIfSceneNotInBuild">Log an error if the scene is not in builds.</param>
        /// <returns>The build index, -1 if not found.</returns>

        static int GetBuildIndex(UnityEditor.SceneAsset sceneAsset, bool logErrorIfSceneNotInBuild)
        {
            int buildIndex;

            if (sceneAsset != null)
            {
                string scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);

                buildIndex = -1;
                int i = -1;
                foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
                {
                    if (scene.enabled)
                        i++;
                    if (scene.path == scenePath)
                    {
                        buildIndex = scene.enabled  ?  i  :  -1;
                        break;
                    }
                }
                
                if (logErrorIfSceneNotInBuild  &&  buildIndex < 0)
                    Debug.LogError("The scene \"" + scenePath + "\" is referenced by an object, but is not added to builds");
            }
            else
            {
                buildIndex = -1;
            }

            return buildIndex;
        }








        /// <summary>
        /// Implementation of MonoBehaviour.Reset().
        /// </summary>

        void Reset()
        {
            sceneAsset = null;
            logErrorIfNotInBuild = true;
        }


        #endif
        #endregion
    }
}
