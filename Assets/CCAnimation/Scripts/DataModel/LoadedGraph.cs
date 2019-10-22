﻿using SEE;
using SEE.DataModel;
using SEE.Layout;

public class LoadedGraph
{
    private readonly Graph graph;
    private readonly AbstractCCALayout layout;
    private readonly GraphSettings graphSettings;

    public LoadedGraph(Graph graph, AbstractCCALayout layout, GraphSettings graphSettings)
    {
        this.graph = graph;
        this.layout = layout;
        this.graphSettings = graphSettings;
    }

    public Graph Graph => graph;

    public AbstractCCALayout Layout => layout;

    public GraphSettings Settings => graphSettings;
}
