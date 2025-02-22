using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "LevelData")]
public class LevelData : ScriptableObject
{
    public List<Level> Levels;
}

[Serializable]
public class Level
{
    public List<ImageInfo> imageInfos;
}

[Serializable]
public class ImageInfo
{
    public Sprite Sprite;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
}