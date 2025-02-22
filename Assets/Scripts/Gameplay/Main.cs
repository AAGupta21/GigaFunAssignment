using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public LevelData levelData;
    public GameObject inputHandler;
    public GameObject ghostImg;
    public Transform ghostParent;
    public Transform inputParent;
    private List<GameObject> _ghostPool = new List<GameObject>();

    private void Start()
    {
        GenerateLevel(0);
    }

    private void GenerateLevel(int levelNumber)
    {
        DisableAllGhosts();

        var level = levelData.Levels[levelNumber];
        
        if (_ghostPool.Count < level.imageInfos.Count)
        {
            var diff = level.imageInfos.Count - _ghostPool.Count;
            for (var i = 0; i < diff; i++)
                _ghostPool.Add(Instantiate(ghostImg, ghostParent));
        }

        var counter = 0;

        foreach (var info in level.imageInfos)
        {
            var gh = _ghostPool[counter];
            gh.GetComponent<Image>().sprite = info.Sprite;
            gh.transform.localScale = new Vector3(info.Scale.x / gh.transform.lossyScale.x, info.Scale.y / gh.transform.lossyScale.y, 1f);
            gh.transform.localRotation = Quaternion.Euler(info.Rotation);
            gh.GetComponent<RectTransform>().sizeDelta = info.sizeDelta;
            gh.transform.localPosition = info.Position;
            gh.SetActive(true);
            counter++;
        }
    }

    private void DisableAllGhosts()
    {
        foreach (var ghost in _ghostPool)
        {
            ghost.SetActive(false);
        }
    }
}
