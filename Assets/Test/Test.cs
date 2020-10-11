using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Trisibo;
using System.Collections;

public class Test : MonoBehaviour
{
    #region Parameters


    [SerializeField] ScenesHolder scenesHolder = default;


    #endregion








    /// <summary>
    /// Implementation of MonoBehaviour.Update().
    /// </summary>

    void Update()
    {
        int index = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) index = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) index = 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) index = 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) index = 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) index = 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) index = 8;
        if (Input.GetKeyDown(KeyCode.Alpha0)) index = 9;

        if (index >= 0  &&  scenesHolder != null  &&  index < scenesHolder.scenes.Length  &&  scenesHolder.scenes[index] != null  &&  scenesHolder.scenes[index].BuildIndex >= 0)
            SceneManager.LoadScene(scenesHolder.scenes[index].BuildIndex);
    }
}
