using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public struct BoundPoint
{
    internal Vector2 _Pos;
    internal Vector2 _Size;

    // only leaf node having value of ContactCollider
    internal Collider2D _ContactCollider;
    internal bool _Collision;
    internal bool _IsCorner;

    public Vector2 Pos => _Pos;

    public Vector2 Size => _Size;

    public Collider2D ContactCollider => _ContactCollider;

    public bool Collision => _Collision;

    public bool IsCorner => _IsCorner;

    public Bounds GetBounds()
    {
        return new Bounds(_Pos, _Size * 2f);
    }
}

public class BoundNode
{
    internal BoundPoint _Bound;
    internal BoundNode _Parent;
    internal BoundNode[] _Childs = new BoundNode[4];
    internal List<BoundNode> _Neighbor = new List<BoundNode>(4);
    internal List<BoundNode> _CornerNeighbor = new List<BoundNode>();
    internal int _Depth = 1;

    public BoundPoint Bound => _Bound;

    public BoundNode Parent => _Parent;

    public BoundNode[] Childs => _Childs;

    public List<BoundNode> Neighbor => _Neighbor;
    public List<BoundNode> CornerNeighbor => _CornerNeighbor;

    public int Depth => _Depth;

    public bool IsLeaf
    {
        get
        {
            int count = 0;
            for (int i = 0; i < _Childs.Length; i++)
            {
                if (_Childs[i] != null) count++;
            }

            return count == 0;
        }
    }

    public void GetLeafCondition(ref List<BoundNode> container, Func<BoundNode, bool> predicate)
    {
        Debug.Assert(container != null);
        Debug.Assert(predicate != null);

        if (IsLeaf)
        {
            if (predicate(this))
            {
                container.Add(this);
            }
        }
        else
        {
            foreach (BoundNode child in _Childs)
            {
                if (child == null) continue;
                child.GetLeafCondition(ref container, predicate);
            }
        }
    }
}

public class PathGenerator : MonoBehaviour
{
    [SerializeField] private LayerMask _wallLayer;

    [SerializeField] private Vector2 _size;

    [SerializeField] private int _maxDepth = 2;
    [SerializeField] private int _maxDefaultDepth = 2;
    [SerializeField] private int _outterCornerSampleingCount;
    [SerializeField] private int _innerCornerSampleingCount;

    [SerializeField] private float _cornerConnectivityRadius;


    [SerializeField] private bool DEBUG_BoudingBox;
    [SerializeField] private bool DEBUG_NeighvorBoudingBox;
    [SerializeField] private bool DEBUG_CornerBoudingBox;
    [SerializeField] private bool DEBUG_NeighborLine;
    [SerializeField] private bool DEBUG_CornerNeighborLine;
    [SerializeField] private bool DEBUG_Reset;

    private Vector2 _leftUpDir;
    private Vector2 _rightUpDir;
    private Vector2 _leftDownDir;
    private Vector2 _rightDownDir;

    private Vector2 LeftUpDir => transform.TransformDirection(_leftUpDir);
    private Vector2 RightUpDir => transform.TransformDirection(_rightUpDir);
    private Vector2 LeftDownDir => transform.TransformDirection(_leftDownDir);
    private Vector2 RightDownDir => transform.TransformDirection(_rightDownDir);

    private BoundNode _rootNode;

    //TODO: test 전용 코드, 수정 요망
    public static PathGenerator Test_Instance { get; private set; }

    private void Start()
    {
        Test_Instance = this;

        Generate();
        GenerateNeighbor(_rootNode);
        GenerateCorner();
        GenerateCornerEdge();
    }

    private void Update()
    {
        if (DEBUG_Reset)
        {
            if (Application.isPlaying)
            {
                Generate();
                GenerateNeighbor(_rootNode);
                GenerateCorner();
                GenerateCornerEdge();
            }

            DEBUG_Reset = false;
        }
    }

    private void Generate()
    {
        _leftUpDir = new Vector2(-1f, 1f).normalized;
        _rightUpDir = new Vector2(1f, 1f).normalized;
        _leftDownDir = new Vector2(-1f, -1f).normalized;
        _rightDownDir = new Vector2(1f, -1f).normalized;

        List<BoundPoint> points = new List<BoundPoint>(100);

        _rootNode = new BoundNode()
        {
            _Bound = new BoundPoint()
            {
                _Pos = transform.position,
                _Size = _size,
                _ContactCollider = null
            },
            _Parent = null,
        };

        GeneratePoints(transform.position, _size, 1, _rootNode);
    }

    private void GenerateCorner()
    {
        var list = new List<BoundNode>(100);
        _rootNode.GetLeafCondition(ref list, x =>
        {
            if (x._Bound._Collision) return false;

            var count = x._Neighbor.Count(y => y._Bound._Collision);
            if (count == 0) return false;

            return count < _outterCornerSampleingCount ||
                   (count >= _innerCornerSampleingCount && _innerCornerSampleingCount != 0);
        });

        foreach (BoundNode node in list)
        {
            node._Bound._IsCorner = true;
        }
    }

    private void GeneratePoints(Vector2 parentPoint, Vector2 parentSize, int depth, BoundNode parentNode)
    {
        var overlapCollider =
            Physics2D.OverlapBox(parentPoint, parentSize * 2f, transform.eulerAngles.z, _wallLayer.value);

        parentNode._Bound._Collision = overlapCollider;

        bool pass = false;

        if (overlapCollider && depth <= _maxDepth)
        {
            pass = true;
        }

        if (overlapCollider == false && depth <= _maxDepth)
        {
            if (depth <= _maxDefaultDepth)
            {
                pass = true;
            }
        }

        if (pass == false)
        {
            parentNode._Bound._Collision = overlapCollider;
            parentNode._Bound._ContactCollider = overlapCollider;

            return;
        }

        depth += 1;
        float magnitude = parentSize.magnitude * 0.5f;
        Vector2 size = parentSize * 0.5f;

        parentNode._Childs[0] = new BoundNode()
        {
            _Bound = new BoundPoint()
            {
                _Pos = parentPoint + LeftUpDir * magnitude,
                _Size = size,
            },
            _Parent = parentNode,
            _Depth = depth,
        };
        parentNode._Childs[1] = new BoundNode()
        {
            _Bound = new BoundPoint()
            {
                _Pos = parentPoint + RightUpDir * magnitude,
                _Size = size
            },
            _Parent = parentNode,
            _Depth = depth
        };
        parentNode._Childs[2] = new BoundNode()
        {
            _Bound = new BoundPoint()
            {
                _Pos = parentPoint + LeftDownDir * magnitude,
                _Size = size
            },
            _Parent = parentNode,
            _Depth = depth
        };
        parentNode._Childs[3] = new BoundNode()
        {
            _Bound = new BoundPoint()
            {
                _Pos = parentPoint + RightDownDir * magnitude,
                _Size = size
            },
            _Parent = parentNode,
            _Depth = depth
        };

        GeneratePoints(parentNode._Childs[0]._Bound._Pos, size, depth, parentNode._Childs[0]);
        GeneratePoints(parentNode._Childs[1]._Bound._Pos, size, depth, parentNode._Childs[1]);
        GeneratePoints(parentNode._Childs[2]._Bound._Pos, size, depth, parentNode._Childs[2]);
        GeneratePoints(parentNode._Childs[3]._Bound._Pos, size, depth, parentNode._Childs[3]);
    }

    private void GenerateNeighbor(BoundNode parentNode)
    {
        List<BoundNode> leafs = new List<BoundNode>(100);

        parentNode.GetLeafCondition(ref leafs, x => x.IsLeaf);

        foreach (BoundNode node in leafs)
        {
            SetNeighbor(parentNode, node);
        }
    }

    public bool Intersects(Bounds a, Bounds b)
    {
        a.center = transform.InverseTransformPoint(a.center);
        b.center = transform.InverseTransformPoint(b.center);

        return a.Intersects(b);
    }

    private void SetNeighbor(BoundNode parentNode, BoundNode targetNode)
    {
        Debug.Assert(targetNode.IsLeaf);
        if (parentNode.IsLeaf)
        {
            Bounds a = parentNode._Bound.GetBounds();
            Bounds b = targetNode._Bound.GetBounds();

            a.center = transform.InverseTransformPoint(a.center);
            a.size += (Vector3)Vector2.one * 0.01f;
            b.center = transform.InverseTransformPoint(b.center);

            var isIntersacts = a.Intersects(b);

            if (isIntersacts)
            {
                targetNode._Neighbor.Add(parentNode);
            }
        }
        else
        {
            foreach (var child in parentNode._Childs)
            {
                SetNeighbor(child, targetNode);
            }
        }
    }

    private void GenerateCornerEdge()
    {
        List<BoundNode> nodes = new List<BoundNode>(100);
        _rootNode.GetLeafCondition(ref nodes, x => x._Bound is { _Collision: false, _IsCorner: true });

        foreach (var node in nodes)
        {
            if (node._CornerNeighbor.Any() == false)
            {
                SetCornerEdge(node);
            }
        }

        List<BoundNode> results = new List<BoundNode>(10);

        foreach (var node in nodes)
        {
            Collider2D callbackCollider = null;
            foreach (var neighbor in node.Neighbor)
            {
                if (neighbor.Bound.Collision)
                {
                    callbackCollider = neighbor.Bound.ContactCollider;
                    break;
                }
            }

            var callbackNode = node;
            GetNodeFromBox(node.Bound.Pos, Vector2.one * _cornerConnectivityRadius, results, x =>
            {
                if (x.Bound.IsCorner == false) return false;
                if (Vector2.SqrMagnitude(x.Bound.Pos - callbackNode.Bound.Pos) >
                    _cornerConnectivityRadius * _cornerConnectivityRadius) return false;

                bool pass = false;
                foreach (var neighbor in x.Neighbor)
                {
                    if (neighbor.Bound.Collision & callbackCollider != neighbor.Bound.ContactCollider)
                    {
                        pass = true;
                        break;
                    }
                }

                if (pass == false) return false;

                return true;
            });


            foreach (BoundNode resultNode in results)
            {
                if (node == resultNode) continue;
                resultNode._CornerNeighbor.Add(node);
                node._CornerNeighbor.Add(resultNode);
            }

            results.Clear();
        }
    }

    private void SetCornerEdge(BoundNode startNode)
    {
        HashSet<BoundNode> visitor = new HashSet<BoundNode>();

        BoundNode currentNode = startNode;
        BoundNode lastCornerNode = startNode;
        Collider2D collider2D = null;

        foreach (BoundNode neighbor in currentNode.Neighbor)
        {
            if (neighbor.Bound.ContactCollider)
            {
                collider2D = neighbor.Bound.ContactCollider;
                break;
            }
        }

        if (collider2D == null)
        {
            return;
        }

        do
        {
            visitor.Add(currentNode);
            BoundNode nextNode = null;
            bool end = true;

            foreach (var neighbor in currentNode.Neighbor)
            {
                if (visitor.Contains(neighbor))
                    continue;
                if (IsCanExitCornerGen(neighbor, collider2D, visitor) == false) continue;

                if (neighbor.Bound.IsCorner)
                {
                    foreach (var a in neighbor.Neighbor)
                    {
                        if (a.Bound.Collision && collider2D == a.Bound.ContactCollider)
                        {
                            nextNode = neighbor;
                            end = false;
                            break;
                        }
                    }
                }

                if (end == false) break;
            }

            if (end)
            {
                foreach (BoundNode neighbor in currentNode.Neighbor)
                {
                    if (neighbor.Bound.Collision) continue;

                    if (visitor.Contains(neighbor)) continue;

                    //if (IsCanExitCornerGen(neighbor, collider2D, visitor) == false) continue;

                    foreach (var a in neighbor.Neighbor)
                    {
                        if (a.Bound.Collision)
                        {
                            if (collider2D != a.Bound.ContactCollider)
                                continue;

                            nextNode = neighbor;
                            end = false;
                            visitor.Add(neighbor);
                            break;
                        }
                    }

                    if (end == false) break;
                }
            }

            if (end) break;

            if (nextNode.Bound.IsCorner)
            {
                nextNode._CornerNeighbor.Add(lastCornerNode);
                lastCornerNode._CornerNeighbor.Add(nextNode);
                lastCornerNode = nextNode;
            }

            currentNode = nextNode;
        } while (true);

        if (lastCornerNode != startNode)
        {
            lastCornerNode._CornerNeighbor.Add(startNode);
            startNode._CornerNeighbor.Add(lastCornerNode);
        }
    }

    private bool IsCanExitCornerGen(BoundNode node, Collider2D col, HashSet<BoundNode> hashSet)
    {
        foreach (var neighbor in node.Neighbor)
        {
            if (neighbor.Bound.Collision) continue;
            bool has = false;
            foreach (var a in neighbor.Neighbor)
            {
                if (a.Bound.Collision && a.Bound.ContactCollider == col)
                {
                    has = true;
                    break;
                }
            }

            if (has == false) continue;

            if (hashSet.Contains(neighbor) == false)
            {
                return true;
            }
        }

        return false;
    }

    public void GetNodeFromBox(Vector2 point, Vector2 size, List<BoundNode> nodes,
        [CanBeNull] Func<BoundNode, bool> condition = null)
    {
        Queue<BoundNode> currentTargets = new Queue<BoundNode>(10);

        currentTargets.Enqueue(_rootNode);
        Bounds bounds = new Bounds(point, size);
        bool hasCondition = condition != null;

        while (currentTargets.Any())
        {
            int length = currentTargets.Count;
            for (int i = 0; i < length; i++)
            {
                var target = currentTargets.Dequeue();

                if (Intersects(target._Bound.GetBounds(), bounds))
                {
                    if (target.IsLeaf)
                    {
                        if (hasCondition == false || condition.Invoke(target))
                        {
                            nodes.Add(target);
                        }
                    }
                    else
                    {
                        foreach (BoundNode child in target._Childs)
                        {
                            currentTargets.Enqueue(child);
                        }
                    }
                }
            }
        }
    }

    public BoundNode GetNodeFromPoint(Vector2 point)
    {
        BoundNode currentNode = _rootNode;

        var m = transform.worldToLocalMatrix;
        m.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

        while (true)
        {
            bool isContinue = false;
            if (currentNode.IsLeaf) return currentNode;

            foreach (var node in currentNode._Childs)
            {
                var bounds = node._Bound.GetBounds();
                bounds.center = m.MultiplyPoint(bounds.center);

                if (bounds.Contains(m.MultiplyPoint(point)))
                {
                    currentNode = node;
                    isContinue = true;
                    break;
                }
            }

            if (isContinue == false)
            {
                return null;
            }
        }
    }

    public BoundNode Root => _rootNode;

    private void OnDrawGizmos()
    {
        List<BoundNode> noCollisionLeafs = new List<BoundNode>(100);
        List<BoundNode> neighborLeafs = new List<BoundNode>(100);
        List<BoundNode> cornerLeafs = new List<BoundNode>(100);
        List<BoundNode> cornerEdgeLeafs = new List<BoundNode>(100);
        if (_rootNode == null) return;

        _rootNode.GetLeafCondition(ref noCollisionLeafs, x => x._Bound._Collision == false);
        _rootNode.GetLeafCondition(ref neighborLeafs,
            x => x._Bound._Collision == false && x._Neighbor.Any(y => y._Bound._Collision));
        _rootNode.GetLeafCondition(ref cornerLeafs, x => x._Bound is { _Collision: false, _IsCorner: true });
        _rootNode.GetLeafCondition(ref cornerEdgeLeafs, x => x._CornerNeighbor.Any());

        foreach (var node in cornerEdgeLeafs)
        {
            if (DEBUG_CornerNeighborLine == false) break;

            foreach (BoundNode neighbor in node._CornerNeighbor)
            {
                Gizmos.DrawLine(node._Bound._Pos, neighbor._Bound._Pos);
            }
        }

        foreach (BoundNode node in noCollisionLeafs)
        {
            if (DEBUG_NeighborLine == false) break;

            foreach (BoundNode neighbor in node._Neighbor)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(node._Bound._Pos, neighbor._Bound._Pos);
            }
        }

        var mm = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            Vector3.one
        );
        Gizmos.matrix = mm;
        Gizmos.DrawWireCube(Vector3.zero, _size * 2);

        foreach (var node in noCollisionLeafs)
        {
            if (DEBUG_BoudingBox == false) break;

            var m = Matrix4x4.TRS(
                node._Bound._Pos,
                transform.rotation,
                Vector3.one
            );

            Gizmos.matrix = m;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
            Gizmos.DrawWireCube(Vector3.zero, node._Bound._Size * 2);
        }

        foreach (var node in neighborLeafs)
        {
            if (DEBUG_NeighvorBoudingBox == false) break;

            var m = Matrix4x4.TRS(
                node._Bound._Pos,
                transform.rotation,
                Vector3.one
            );

            Gizmos.matrix = m;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
            Gizmos.DrawWireCube(Vector3.zero, node._Bound._Size * 2);
        }

        foreach (var node in cornerLeafs)
        {
            if (DEBUG_CornerBoudingBox == false) break;

            var m = Matrix4x4.TRS(
                node._Bound._Pos,
                transform.rotation,
                Vector3.one
            );

            Gizmos.matrix = m;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
            Gizmos.DrawWireCube(Vector3.zero, node._Bound._Size * 2);
        }
    }
}