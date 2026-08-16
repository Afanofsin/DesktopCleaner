using System;
using System.Collections.Generic;
using R3;
using Sirenix.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class CaptchaCatGame : CaptchaGame
{
    [OdinSerialize] private List<CatGameSlot> _catSlots = new();
    [OdinSerialize] private List<CatGameCat> _goodCatsPrefabs = new();
    [OdinSerialize] private CatGameCat _badCatPrefab;
    
    [OdinSerialize] private Transform _freeCatsRoot;
    
    [OdinSerialize] private AudioSource _audioSource;
    [OdinSerialize] private List<AudioClip> _goodClips = new();
    [OdinSerialize] private AudioClip _badClip;
    
    private List<CatGameCat> _goodCats;
    private List<CatGameCat> _badCats;

    private int _lastIndex = -1;
    private int _lastSoundIndex = -1;
     
    private void Awake()
    {
        _goodCats = new List<CatGameCat>();
        _badCats = new List<CatGameCat>();

        PointsGoal = 20;
        
        foreach (var cat in _goodCatsPrefabs)
        {
            var prefab = Instantiate(cat, _freeCatsRoot, false);
            prefab.gameObject.SetActive(false);
            prefab.button.OnClickAsObservable().Subscribe(_ => CheckCat(prefab)).AddTo(this);
            _goodCats.Add(prefab);
        }

        for (var i = 0; i < 9; i++)
        {
            var prefab = Instantiate(_badCatPrefab, _freeCatsRoot, false);
            prefab.gameObject.SetActive(false);
            prefab.button.OnClickAsObservable().Subscribe(_ => CheckCat(prefab)).AddTo(this);
            _badCats.Add(prefab);
        }
    }

    private void ClearStage()
    {
        foreach (var slot in _catSlots)
        {
            var cat = slot.CurrentCat;

            if (cat == null) continue;
            AddFreeCat(cat);
        }

        for (var i = 0; i < _goodCats.Count && i < _badCats.Count; i++)
        {
            _goodCats[i].button.interactable = true;
            _badCats[i].button.interactable = true;
        }
    }

    private void AddFreeCat(CatGameCat cat)
    {
        cat.transform.SetParent(_freeCatsRoot, false);
        cat.gameObject.SetActive(false);
    }

    public override void StartGame()
    {
        base.StartGame();
        StartLevel();
    }

    private void StartLevel()
    {
        ClearStage();
        
        int goodSlotIndex;

        do
        {
            goodSlotIndex = Random.Range(0, _catSlots.Count);
        }
        while (goodSlotIndex == _lastIndex);

        var goodCat = _goodCats[Random.Range(0, _goodCats.Count)];

        _lastIndex = goodSlotIndex;

        var badCatIndex = 0;

        for (var i = 0; i < _catSlots.Count; i++)
        {
            var cat = i == goodSlotIndex ? goodCat : _badCats[badCatIndex++];

            _catSlots[i].AssignCat(cat);
        }
    }

    public override void FailGame()
    {
        _audioSource.PlayOneShot(_badClip);
        
        for (var i = 0; i < _catSlots.Count; i++)
        {
            _catSlots[i].AssignCat(_badCats[i]);

            _catSlots[i].CurrentCat.button.interactable = false;
        }
        
        base.FailGame();
    }

    public override void WinGame()
    {
        for (var i = 0; i < _catSlots.Count; i++)
        {
            _catSlots[i].AssignCat(_goodCats[i]);

            _catSlots[i].CurrentCat.button.interactable = false;
        }
        
        base.WinGame();
    }

    private void CheckCat(CatGameCat cat)
    {
        if (cat.IsBad)
        {
            FailGame();
        }
        else
        {
            int soundIndex;

            do
            {
                soundIndex = Random.Range(0, _goodClips.Count);
            }
            while (_goodClips.Count > 1 && soundIndex == _lastSoundIndex);

            _lastSoundIndex = soundIndex;
             
            _audioSource.PlayOneShot(_goodClips[soundIndex]);
            
            if (CurrentPointsRx.Value++ >= PointsGoal - 1)
            {
                WinGame();
            }
            else
            {
                StartLevel();
            }
        }
    }
}