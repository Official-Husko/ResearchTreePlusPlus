// Node.cs
// Copyright Karel Kroeze, 2019-2020

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FluffyResearchTree;

public class Node
{
    private const float Offset = 2f;

    private Rect _costIconRect;

    private Rect _costLabelRect;

    private Rect _iconsRect;

    private Rect _labelRect;

    protected bool _largeLabel;

    private Vector2 _left = Vector2.zero;

    private Rect _lockRect;

    protected Vector2 _pos = Vector2.zero;

    private Rect _rect;

    private bool _rectsSet;

    private Vector2 _right = Vector2.zero;

    private Vector2 _topLeft = Vector2.zero;

    private List<Node> _outNodesCache;
    private int _outNodesCacheVersion = -1;

    private List<Node> _inNodesCache;
    private int _inNodesCacheVersion = -1;

    private List<Edge<Node, Node>> _edgesCache;
    private int _edgesCacheInVersion = -1;
    private int _edgesCacheOutVersion = -1;

    private List<Node> _nodesCache;
    private int _nodesCacheInVersion = -1;
    private int _nodesCacheOutVersion = -1;

    private int _inEdgeVersion;
    private int _outEdgeVersion;

    public List<Node> Descendants => OutNodes.Concat(OutNodes.SelectMany(n => n.Descendants)).ToList();

    public List<Edge<Node, Node>> OutEdges { get; } = [];

    public void AddOutEdge(Edge<Node, Node> edge)
    {
        OutEdges.Add(edge);
        _outEdgeVersion++;
        _outNodesCache = null;
        _edgesCache = null;
        _nodesCache = null;
    }

    public bool RemoveOutEdge(Edge<Node, Node> edge)
    {
        var removed = OutEdges.Remove(edge);
        if (removed)
        {
            _outEdgeVersion++;
            _outNodesCache = null;
            _edgesCache = null;
            _nodesCache = null;
        }

        return removed;
    }

    public List<Node> OutNodes
    {
        get
        {
            if (_outNodesCache == null || _outNodesCacheVersion != _outEdgeVersion)
            {
                _outNodesCache = OutEdges.Select(e => e.Out).ToList();
                _outNodesCacheVersion = _outEdgeVersion;
            }

            return _outNodesCache;
        }
    }

    public List<Edge<Node, Node>> InEdges { get; } = [];

    public void AddInEdge(Edge<Node, Node> edge)
    {
        InEdges.Add(edge);
        _inEdgeVersion++;
        _inNodesCache = null;
        _edgesCache = null;
        _nodesCache = null;
    }

    public bool RemoveInEdge(Edge<Node, Node> edge)
    {
        var removed = InEdges.Remove(edge);
        if (removed)
        {
            _inEdgeVersion++;
            _inNodesCache = null;
            _edgesCache = null;
            _nodesCache = null;
        }

        return removed;
    }

    public List<Node> InNodes
    {
        get
        {
            if (_inNodesCache == null || _inNodesCacheVersion != _inEdgeVersion)
            {
                _inNodesCache = InEdges.Select(e => e.In).ToList();
                _inNodesCacheVersion = _inEdgeVersion;
            }

            return _inNodesCache;
        }
    }

    public List<Edge<Node, Node>> Edges
    {
        get
        {
            if (_edgesCache == null || _edgesCacheInVersion != _inEdgeVersion || _edgesCacheOutVersion != _outEdgeVersion)
            {
                _edgesCache = InEdges.Concat(OutEdges).ToList();
                _edgesCacheInVersion = _inEdgeVersion;
                _edgesCacheOutVersion = _outEdgeVersion;
            }

            return _edgesCache;
        }
    }

    public List<Node> Nodes
    {
        get
        {
            if (_nodesCache == null || _nodesCacheInVersion != _inEdgeVersion || _nodesCacheOutVersion != _outEdgeVersion)
            {
                _nodesCache = InNodes.Concat(OutNodes).ToList();
                _nodesCacheInVersion = _inEdgeVersion;
                _nodesCacheOutVersion = _outEdgeVersion;
            }

            return _nodesCache;
        }
    }

    protected Rect CostIconRect
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _costIconRect;
        }
    }

    protected Rect CostLabelRect
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _costLabelRect;
        }
    }

    public virtual Color Color => Color.white;

    public virtual Color EdgeColor => Color;

    protected Rect IconsRect
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _iconsRect;
        }
    }

    protected Rect LabelRect
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _labelRect;
        }
    }

    public Vector2 Left
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _left;
        }
    }

    public Rect QueueRect { get; set; }

    public Rect LockRect
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _lockRect;
        }
    }

    protected internal Rect Rect
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _rect;
        }
    }

    public Vector2 Right
    {
        get
        {
            if (!_rectsSet)
            {
                SetRects();
            }

            return _right;
        }
    }

    public Vector2 Center => (Left + Right) / Offset;

    public virtual int X
    {
        get => (int)_pos.x;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (Math.Abs(_pos.x - value) < Constants.Epsilon)
            {
                return;
            }

            var previous = (int)_pos.x;
            _pos.x = value;
            _rectsSet = false;
            if (value > Tree.Size.x)
            {
                Tree.Size = new IntVec2(value, Tree.Size.z);
            }
            else if (previous == Tree.Size.x && value < previous)
            {
                Tree.RecomputeSizeX();
            }
            Tree.OrderDirty = true;
        }
    }

    public virtual int Y
    {
        get => (int)_pos.y;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (Math.Abs(_pos.y - value) < Constants.Epsilon)
            {
                return;
            }

            var previous = (int)_pos.y;
            _pos.y = value;
            _rectsSet = false;
            if (value > Tree.Size.z)
            {
                Tree.Size = new IntVec2(Tree.Size.x, value);
            }
            else if (previous == Tree.Size.z && value < previous)
            {
                Tree.RecomputeSizeZ();
            }
            Tree.OrderDirty = true;
        }
    }

    public virtual Vector2 Pos => new(X, Y);

    public virtual float Yf
    {
        get => _pos.y;
        set
        {
            if (Math.Abs(_pos.y - value) < Constants.Epsilon)
            {
                return;
            }

            var previous = (int)_pos.y;
            _pos.y = value;
            var valueInt = (int)value;
            if (valueInt > Tree.Size.z)
            {
                Tree.Size = new IntVec2(Tree.Size.x, valueInt);
            }
            else if (previous == Tree.Size.z && valueInt < previous)
            {
                Tree.RecomputeSizeZ();
            }
            Tree.OrderDirty = true;
        }
    }

    public virtual string Label { get; }

    public virtual bool Completed => false;

    public virtual bool Available => false;

    public virtual bool Highlighted { get; set; }

    public virtual bool IsVisible => true;

    protected internal virtual bool SetDepth(int min = 1)
    {
        var num = Mathf.Max(InNodes.NullOrEmpty() ? 1 : InNodes.Max(n => n.X) + 1, min);
        if (num == X)
        {
            return false;
        }

        X = num;
        return true;
    }

    public virtual void Debug()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Label} ({X}, {Y}):");
        stringBuilder.AppendLine("- Parents");
        foreach (var inNode in InNodes)
        {
            stringBuilder.AppendLine($"-- {inNode.Label}");
        }

        stringBuilder.AppendLine("- Children");
        foreach (var outNode in OutNodes)
        {
            stringBuilder.AppendLine($"-- {outNode.Label}");
        }

        stringBuilder.AppendLine("");
        Logging.Message(stringBuilder.ToString());
    }

    public override string ToString()
    {
        var label = Label;
        var pos = _pos;
        return label + pos;
    }

    protected void SetRects()
    {
        // origin
        _topLeft = new Vector2(
            (X - 1) * (Constants.NodeSize.x + Constants.NodeMargins.x),
            (Yf - 1) * (Constants.NodeSize.y + Constants.NodeMargins.y));

        SetRects(_topLeft);
    }

    protected void SetRects(Vector2 topLeft)
    {
        // main rect
        _rect = new Rect(topLeft.x,
            topLeft.y,
            Constants.NodeSize.x,
            Constants.NodeSize.y);

        // left and right edges
        _left = new Vector2(_rect.xMin, _rect.yMin + (_rect.height / 2f));
        _right = new Vector2(_rect.xMax, _left.y);

        // label rect
        _labelRect = new Rect(_rect.xMin + 6f,
            _rect.yMin + 3f,
            (_rect.width * 2f / 3f) - 6f,
            (_rect.height * .5f) - 3f);

        // research cost rect
        _costLabelRect = new Rect(_rect.xMin + (_rect.width * 3f / 5f),
            _rect.yMin + 3f,
            (_rect.width * 2f / 5f) - 16f - 3f,
            (_rect.height * .5f) - 3f);

        // research icon rect
        _costIconRect = new Rect(_costLabelRect.xMax,
            _rect.yMin + ((_costLabelRect.height - 16f) / 2),
            16f,
            16f);

        // icon container rect
        _iconsRect = new Rect(_rect.xMin,
            _rect.yMin + (_rect.height * .5f),
            _rect.width,
            _rect.height * .5f);

        // lock icon rect
        _lockRect = new Rect(0f, 0f, 32f, 32f);
        _lockRect = _lockRect.CenteredOnXIn(_rect);
        _lockRect = _lockRect.CenteredOnYIn(_rect);

        // see if the label is too big
        _largeLabel = Text.CalcHeight(Label, _labelRect.width) > _labelRect.height;

        // done
        _rectsSet = true;
    }

    public bool IsWithinViewport(Rect visibleRect)
    {
        // Check if the node's rectangle intersects with the visible rectangle
        return Rect.Overlaps(visibleRect.ExpandedBy(50f));
    }

    public virtual void Draw(Rect visibleRect, bool forceDetailedMode = false)
    {
    }
}
