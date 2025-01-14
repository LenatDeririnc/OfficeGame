﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InitSequence : MonoBehaviour, IInit
{
    public static InitSequence current;
    [SerializeField] private CanvasScript _canvasScript;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameEvents _gameEvents;
    [SerializeField] private BallsScripts _ballsScripts;
    [SerializeField] private NotesScript _notesScript;
    [SerializeField] private InteractItems _interactItems;

    private List<IInit> INIT_ORDER;

    private void FILL_INIT_ORDER()
    {
        INIT_ORDER = new List<IInit>();
        INIT_ORDER.Add(_ballsScripts);
        INIT_ORDER.Add(_notesScript);
        INIT_ORDER.Add(_interactItems);
        INIT_ORDER.Add(_gameManager);
        INIT_ORDER.Add(_canvasScript);
        INIT_ORDER.Add(_gameEvents);
    }
    
    public void INIT()
    {
        current = this;
        FILL_INIT_ORDER();
        foreach (var initElement in INIT_ORDER)
        {
            initElement.INIT();
        }
    }

    public void GET()
    {
        foreach (var initElement in INIT_ORDER)
        {
            initElement.GET();
        }
    }

    public void AFTER_INIT()
    {
        foreach (var initElement in INIT_ORDER)
        {
            initElement.AFTER_INIT();
        }
        _gameManager.FILL_INPUT_DELIGATES();
        _gameManager.SubscribeInput();
    }

    private void Awake()
    {
        START_INIT();
    }

    public void START_INIT()
    {
        INIT();
        GET();
        AFTER_INIT();
    }
}
