﻿using SEE;
using SEE.DataModel;
using SEE.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TODO flo: make Abstract fpr CCAnimation to simpler Switch
/// </summary>
public class CCAStateManager : MonoBehaviour
{
    public AbstractCCARender render;

    private readonly CCALoader graphLoader = new CCALoader();
    private readonly Dictionary<string, AbstractCCALayout> layouts = new Dictionary<string, AbstractCCALayout>();
    private IScale scaler;

    private bool _isEndlessMode = false;

    private GraphSettings EditorSettings => graphLoader.settings;
    private Dictionary<string, Graph> Graphs => graphLoader.graphs;

    private List<string> GraphOrder => graphLoader.graphOrder;

    public int GraphCount => GraphOrder.Count;

    private int _openGraphIndex = 0;
    public int OpenGraphIndex
    {
        get => _openGraphIndex;
        set
        {
            _openGraphIndex = value;
            ViewDataChangedEvent.Invoke();

        }
    }

    private UnityEvent _viewDataChangedEvent;
    public UnityEvent ViewDataChangedEvent
    {
        get
        {
            if (_viewDataChangedEvent == null)
                _viewDataChangedEvent = new UnityEvent();
            return _viewDataChangedEvent;
        }
    }

    public bool IsEndlessMode
    {
        get => _isEndlessMode;
        private set
        {
            ViewDataChangedEvent.Invoke();
            _isEndlessMode = value;
        }
    }

    private bool HasLoadedGraph(out LoadedGraph loadedGraph)
    {
        return HasLoadedGraph(_openGraphIndex, out loadedGraph);
    }

    private bool HasLoadedGraph(int index, out LoadedGraph loadedGraph)
    {
        // TODO FLo: KeyNotFoundException
        loadedGraph = null;
        var graph = Graphs[GraphOrder[index]];
        if (graph == null)
        {
            Debug.LogError("There ist no Graph available at index " + index);
            return false;
        }
        var layout = layouts[GraphOrder[index]];
        if (layout == null)
        {
            Debug.LogError("There ist no Layout available at index " + index);
            return false;
        }
        if (EditorSettings == null)
        {
            Debug.LogError("There ist no GraphSettings available");
            return false;
        }
        loadedGraph = new LoadedGraph(graph, layout, EditorSettings);
        return true;
    }

    [Obsolete]
    void InitRandomLayouts()
    {
        Graphs.Keys.ToList().ForEach(key => layouts[key] = new RandomCCALayout(EditorSettings, null, scaler));
    }


    void Start()
    {
        if (render == null)
        {
            Debug.LogError("There ist no render selected for this StateManager");
            return;
        }

        EditorSettings.MinimalBlockLength = 1;
        EditorSettings.MaximalBlockLength = 10;
        EditorSettings.ZScoreScale = false;

        graphLoader.Init();
        ViewDataChangedEvent.Invoke();

        List<string> nodeMetrics = new List<string>() { EditorSettings.WidthMetric, EditorSettings.HeightMetric, EditorSettings.DepthMetric };
        nodeMetrics.AddRange(EditorSettings.IssueMap().Keys);
        scaler = new LinearMultiScale(Graphs.Values.ToList(), EditorSettings.MinimalBlockLength, EditorSettings.MaximalBlockLength, nodeMetrics);

        // TODO Flo: load layouts
        InitRandomLayouts();

        if (HasLoadedGraph(out LoadedGraph loadedGraph))
        {
            render.DisplayGraph(loadedGraph);
        }
        else
        {
            Debug.LogError("Could not create LoadedGraph to render.");
        }
    }

    /// <summary>
    /// TODO: doc
    /// </summary>
    public void ShowNextGraph()
    {
        if (render.IsStillAnimating || IsEndlessMode)
        {
            Debug.Log("The render is already occupied with animating, wait till animations are finished.");
            return;
        }
        var canShowNext = ShowNextIfPossible();
        if (!canShowNext)
        {
            Debug.Log("This is already the last graph revision.");
            return;
        }
    }

    /// <summary>
    /// TODO: doc
    /// </summary>
    public void ShowPreviousGraph()
    {
        if (render.IsStillAnimating || IsEndlessMode)
        {
            Debug.Log("The render is already occupied with animating, wait till animations are finished.");
            return;
        }
        if (OpenGraphIndex == 0)
        {
            Debug.Log("This is already the first graph revision.");
            return;
        }
        OpenGraphIndex--;

        if (HasLoadedGraph(out LoadedGraph loadedGraph) &&
            HasLoadedGraph(OpenGraphIndex + 1, out LoadedGraph oldLoadedGraph))
        {
            render.TransitionToPreviousGraph(oldLoadedGraph, loadedGraph);
        }
        else
        {
            Debug.LogError("Could not create LoadedGraph to render.");
        }
    }

    internal void ToggleEndlessMode()
    {
        IsEndlessMode = !IsEndlessMode;
        if (IsEndlessMode)
        {
            render.AnimationFinishedEvent.AddListener(OnEndlessModeCanContinue);
            var canShowNext = ShowNextIfPossible();
            if (!canShowNext)
            {
                Debug.Log("This is already the last graph revision.");
            }
        }
        else
        {
            render.AnimationFinishedEvent.RemoveListener(OnEndlessModeCanContinue);
        }
        ViewDataChangedEvent.Invoke();
    }

    private bool ShowNextIfPossible()
    {
        if (_openGraphIndex == GraphOrder.Count - 1)
        {
            return false;
        }
        OpenGraphIndex++;

        if (HasLoadedGraph(out LoadedGraph loadedGraph) &&
            HasLoadedGraph(OpenGraphIndex - 1, out LoadedGraph oldLoadedGraph))
        {
            render.TransitionToNextGraph(oldLoadedGraph, loadedGraph);
        }
        else
        {
            Debug.LogError("Could not create LoadedGraph to render.");
        }
        return true;
    }

    internal void OnEndlessModeCanContinue()
    {
        var canShowNext = ShowNextIfPossible();
        if (!canShowNext)
        {
            ToggleEndlessMode();
        }
    }
}
