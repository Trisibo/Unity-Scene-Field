using System;
using System.Collections;
using System.Collections.Generic;
using Trisibo;
using UnityEngine;

[CreateAssetMenu]
public class ScenesHolder : ScriptableObject
{
    [SerializeField] public SceneField[] scenes = default;
}
