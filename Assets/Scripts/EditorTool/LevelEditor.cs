using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor : EditorWindow
{
    private const string DataPath = "Assets/Data/LevelData.asset";
    private LevelData _levelData;
    
    [MenuItem("Tools/Generate Level")]
    public static void GenerateLevel()
    {
        GetWindow<LevelEditor>("Level Editor");
    }

    private void OnEnable()
    {
        LoadOrCreateLevelData();
    }

    private void LoadOrCreateLevelData()
    {
        if(_levelData == null)
            _levelData = AssetDatabase.LoadAssetAtPath<LevelData>(DataPath);

        if (_levelData != null) return;
        if (!Directory.Exists("Assets/Data"))
        {
            Directory.CreateDirectory("Assets/Data");
        }

        _levelData = CreateInstance<LevelData>();
        _levelData.Levels = new List<Level>();
        AssetDatabase.CreateAsset(_levelData, DataPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void OnGUI()
    {
        if (_levelData == null)
        {
            Debug.LogError("Error Loading Level Data...");
            Close();
            return;
        }

        if (Selection.count < 4)
        {
            Debug.LogError("At least Four GameObject needs to be Selected to Create Level Data, but ideally 4.");
            Close();
            return;
        }
        
        SaveLevelData();
    }

    private void SaveLevelData()
    {
        var selections = Selection.gameObjects;
        
        var offsetPos = selections[0].GetComponent<RectTransform>().rect.width / 2f;
        var leftMostPos = selections[0].transform.localPosition.x - offsetPos;

        foreach (var selection in selections)
        {
            if(!selection.TryGetComponent<RectTransform>(out var rect)) continue;
            var lMostPos = rect.transform.localPosition.x - rect.rect.width / 2f;
            if(lMostPos < leftMostPos)
                leftMostPos = lMostPos;
        }
        
        var level = new Level
        {
            imageInfos = new List<ImageInfo>()
        };

        foreach (var go in selections)
        {
            if (!go.TryGetComponent<Image>(out var img)) continue;
            var pos = img.rectTransform.localPosition;
            pos.x -= leftMostPos;
            level.imageInfos.Add(new ImageInfo()
            {
                Sprite = img.sprite,
                Position = pos,
                Rotation = img.transform.rotation.eulerAngles,
                Scale = img.transform.lossyScale
            });
        }
        
        _levelData.Levels.Add(level);
        
        EditorUtility.SetDirty(_levelData);
        AssetDatabase.SaveAssets();
        Close();
    }
}