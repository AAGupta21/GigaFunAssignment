using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Main : MonoBehaviour
{
    public LevelData levelData;
    public GameObject inputHandler;
    public GameObject ghostImg;
    public Transform ghostParent;
    public Transform inputParent;
    private List<GameObject> _ghostPool;
    private List<GameObject> _selectionPool;
    private Dictionary<int, Vector3> _selectionKey;
    private Dictionary<int, bool> _winCheckKey;
    private int _winCount;
    private int _levelNumber;
    
    [SerializeField] private SoundHandler soundHandler;

    private void Awake()
    {
        _ghostPool = new();
        _selectionPool = new();
        _selectionKey = new();
        _winCheckKey = new();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            LoadLevel();
        }
        else
        {
            _levelNumber = 0;
            GenerateLevel();   
        }
    }

    private void GenerateLevel()
    {
        ResetLevel();

        var level = levelData.Levels[_levelNumber];
        
        if (_ghostPool.Count < level.imageInfos.Count)
        {
            var diff = level.imageInfos.Count - _ghostPool.Count;
            for (var i = 0; i < diff; i++)
                _ghostPool.Add(Instantiate(ghostImg, ghostParent));
            diff = level.imageInfos.Count - _selectionPool.Count;
            for(var i = 0; i < diff; i++)
                _selectionPool.Add(Instantiate(inputHandler, inputParent));
        }

        var counter = 0;
        var listOfPos = new List<Vector3>();
        var dict = new Dictionary<int, Vector3>();

        foreach (var info in level.imageInfos)
        {
            var gh = _ghostPool[counter];
            gh.GetComponent<Image>().sprite = info.Sprite;
            gh.transform.localScale = new Vector3(info.Scale.x / gh.transform.lossyScale.x, info.Scale.y / gh.transform.lossyScale.y, 1f);
            gh.transform.localRotation = Quaternion.Euler(info.Rotation);
            gh.GetComponent<RectTransform>().sizeDelta = info.sizeDelta;
            gh.transform.localPosition = info.Position;
            gh.SetActive(true);
            
            var sel = _selectionPool[counter];
            sel.GetComponent<Image>().sprite = info.Sprite;
            sel.transform.localScale = gh.transform.localScale;
            sel.transform.localRotation = gh.transform.localRotation;
            sel.GetComponent<RectTransform>().sizeDelta = info.sizeDelta;
            sel.SetActive(true);
            
            dict.Add(counter, gh.transform.position);
            _winCheckKey.Add(counter, false);
            listOfPos.Add(info.Position);
            counter++;
        }

        _winCount = counter;
        listOfPos = listOfPos.OrderBy(x => Random.value).ToList();

        for (var index = 0; index < counter; index++)
        {
            var pos = listOfPos[index];
            _selectionKey.Add(index, pos);
            _selectionPool[index].transform.localPosition = pos;
            var iHandler = _selectionPool[index].GetComponent<InputHandler>();
            iHandler.DoSetUp(pos, dict[index], index, DropAction, OnPickAction);
        }
        
        DoSave();
    }

    private void LoadLevel()
    {
        ResetLevel();
        
        var saveData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("SaveData"));
        _levelNumber = saveData.currentLevel;
        
        foreach (var curr in saveData.currentCompleted)
        {
            _winCheckKey.Add(curr.index, curr.status);
        }
        
        foreach (var currPos in saveData.currentPositions)
        {
            _selectionKey.Add(currPos.index, currPos.position);
        }
        
        var level = levelData.Levels[_levelNumber];
        
        if (_ghostPool.Count < level.imageInfos.Count)
        {
            var diff = level.imageInfos.Count - _ghostPool.Count;
            for (var i = 0; i < diff; i++)
                _ghostPool.Add(Instantiate(ghostImg, ghostParent));
            diff = level.imageInfos.Count - _selectionPool.Count;
            for(var i = 0; i < diff; i++)
                _selectionPool.Add(Instantiate(inputHandler, inputParent));
        }

        var counter = 0;
        var dict = new Dictionary<int, Vector3>();
        
        foreach (var info in level.imageInfos)
        {
            var gh = _ghostPool[counter];
            gh.GetComponent<Image>().sprite = info.Sprite;
            gh.transform.localScale = new Vector3(info.Scale.x / gh.transform.lossyScale.x, info.Scale.y / gh.transform.lossyScale.y, 1f);
            gh.transform.localRotation = Quaternion.Euler(info.Rotation);
            gh.GetComponent<RectTransform>().sizeDelta = info.sizeDelta;
            gh.transform.localPosition = info.Position;
            gh.SetActive(true);
            
            var sel = _selectionPool[counter];
            sel.GetComponent<Image>().sprite = info.Sprite;
            sel.transform.localScale = gh.transform.localScale;
            sel.transform.localRotation = gh.transform.localRotation;
            sel.GetComponent<RectTransform>().sizeDelta = info.sizeDelta;
            sel.SetActive(true);
            
            dict.Add(counter, gh.transform.position);
            counter++;
        }
        
        _winCount = counter;
        
        for (var index = 0; index < counter; index++)
        {
            if (_winCheckKey[index])
            {
                _selectionPool[index].transform.position = dict[index];
                _selectionPool[index].GetComponent<InputHandler>().DoCompletedSetup();
                _winCount--;
                continue;
            }
            
            var pos = _selectionKey[index];
            _selectionPool[index].transform.localPosition = pos;
            var iHandler = _selectionPool[index].GetComponent<InputHandler>();
            iHandler.DoSetUp(pos, dict[index], index, DropAction, OnPickAction);
        }
    }

    private void OnPickAction()
    {
        soundHandler.PlaySound(SoundType.Pick);
    }

    private void DropAction(bool result, int key)
    {
        soundHandler.PlaySound(result? SoundType.Placed: SoundType.Failed);
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif
        
        if (!result) return;
        
        _winCheckKey[key] = true;
        _winCount--;

        foreach (var pair in _winCheckKey)
        {
            if(pair.Value)
                _selectionPool[pair.Key].GetComponent<InputHandler>().PlayEffectAfterDelay();
        }

        if (_winCount <= 0)
        {
            _levelNumber++;
            if(_levelNumber >= levelData.Levels.Count)
                _levelNumber = 0;

            StartCoroutine(GenerateLevelAfterDelay());
        }
        else
        {
            DoSave();
        }
    }

    private void ResetLevel()
    {
        DisableAllGhosts();
        DisableAllSelections();
        _selectionKey.Clear();
        _winCheckKey.Clear();
    }

    private void DisableAllGhosts()
    {
        foreach (var ghost in _ghostPool)
        {
            ghost.SetActive(false);
        }
    }

    private void DisableAllSelections()
    {
        foreach (var select in _selectionPool)
        {
            select.SetActive(false);
        }
    }

    private void DoSave()
    {
        var listComplete = _winCheckKey.Select(x => new CurrentCompletion() { index = x.Key, status = x.Value }).ToList();
        var listPositions = _selectionKey.Select(x => new CurrentPositions() { index = x.Key, position = x.Value }).ToList();

        var save = new SaveData()
        {
            currentLevel = _levelNumber,
            currentCompleted = listComplete,
            currentPositions = listPositions
        };
        
        var saveJson = JsonUtility.ToJson(save);
        PlayerPrefs.SetString("SaveData", saveJson);
    }

    private IEnumerator GenerateLevelAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        GenerateLevel();
    }
}

[Serializable]
public class SaveData
{
    public int currentLevel;
    public List<CurrentPositions> currentPositions;
    public List<CurrentCompletion> currentCompleted;
}

[Serializable]
public class CurrentPositions
{
    public int index;
    public Vector3 position;
}

[Serializable]
public class CurrentCompletion
{
    public int index;
    public bool status;
}

