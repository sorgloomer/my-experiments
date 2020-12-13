using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp1.PhysicsEngine
{
    public class RigidbodyAabbTree : AabbTree<Rigidbody>
    {
        public BoundingRects rects = new BoundingRects();
        
        public override AaRect GetBodyFitRect(Rigidbody body)
        {
            return rects.GetBodyFitRect(body);
        }

        public override AaRect GetBodyFatRect(Rigidbody body)
        {
            return rects.GetBodyFatRect(body);
        }
    }

    public abstract class AabbTree<T> : AbstractTree<T>
    {
        public AabbTreeNode<T> root;
        public HashSet<T> bodies = new HashSet<T>();
        public List<AabbTreeNode<T>> marks;

        public int maxChildren = 5;
        public int minChildren = 2;

        public int fatSleepFrames = 10000;

        public bool balanceEnableRotations = true;
        public int balanceDepthDifferenceThreshold = 4;
        public int balanceDepthDifferencePercent = 5;

        public readonly TreeType type = TreeType.Tree;
        private Dictionary<T, AabbTreeNode<T>> bodyNodes = new Dictionary<T, AabbTreeNode<T>>();

        public abstract AaRect GetBodyFitRect(T body);
        public abstract AaRect GetBodyFatRect(T body);

        public override bool Contains(T body)
        {
            return bodies.Contains(body);
        }

        public override bool Add(T body)
        {
            if (type == TreeType.Bag)
            {
                return bodies.Add(body);
            }

            AabbTreeNode<T> node;
            bool result = bodyNodes.TryGetValue(body, out node);
            if (result)
            {
                if (node.bounds.Contains(GetBodyFitRect(body)))
                {
                    if (--node.fatsleep > 0)
                    {
                        return true; // nothing to do, still inside fat bounds
                    }
                }
                RemoveNode(node);
            }
            else
            {
                node = new AabbTreeNode<T>
                {
                    leaf = true,
                    body = body,
                };
                bodyNodes[body] = node;
                bodies.Add(body);
            }
            node.bounds = GetBodyFatRect(body);
            node.fatsleep = fatSleepFrames;
            AddToRoot(node);
            return result;
        }
        
        public override bool Remove(T body)
        {
            if (bodyNodes.TryGetValue(body, out var node))
            {
                bodyNodes.Remove(body);
                bodies.Remove(body);
                RemoveNode(node);
                return true;
            }
            return false;
        }

        public override IEnumerable<T> GetAll()
        {
            return bodies;
        }


        private void AddToRoot(AabbTreeNode<T> leaf)
        {
            if (root == null)
            {
                root = new AabbTreeNode<T>
                {
                    children = new List<AabbTreeNode<T>> {leaf},
                    depth = leaf.depth + 1,
                    bounds = leaf.bounds,
                };
                leaf.parent = root;
            }
            else
            {
                AddToNode(root, leaf);
            }
        }
        
        private void AddToNode(AabbTreeNode<T> inner, AabbTreeNode<T> leaf)
        {
            for (;;)
            {
                inner = FindChildNodeWithLeastOverlapInInner(inner, leaf.bounds);
                if (inner == null)
                {
                    throw new InvalidOperationException();
                }
                if (inner.leaf)
                {
                    AddChildNode(inner.parent, leaf);
                    break;
                }
            }
            PropagateNodeChange(inner.parent, true);
        }

        private void AddChildNode(AabbTreeNode<T> parent, AabbTreeNode<T> child)
        {
            if (parent?.children == null) throw new InvalidOperationException("AddChild parent cannot be null");
            if (child == null) throw new InvalidOperationException("AddChild child cannot be null");
            if (parent.leaf) throw new InvalidOperationException("AddChild parent cannot be a leaf node");
            var node = parent;
            while (node != null)
            {
                if (node == child) throw new InvalidOperationException("AddChild attempted to create circular graph");
                node = node.parent;
            }
            parent.children.Add(child);
            child.parent = parent;
        }

        private AabbTreeNode<T> FindChildNodeWithLeastOverlapInInner(AabbTreeNode<T> node, AaRect fat)
        {
            return node.children.MinBy<AabbTreeNode<T>, List<AabbTreeNode<T>>, float>(
                n => AaRect.Merge(n.bounds, fat).GetArea() - n.bounds.GetArea()
            );
        }

        private void RemoveNode(AabbTreeNode<T> leaf)
        {
            leaf.parent.children.Remove(leaf);
            PropagateNodeChange(leaf.parent, true);
        }
        
        private void ReplaceNode(AabbTreeNode<T> node, AabbTreeNode<T> replacement)
        {
            var parent = node?.parent;
            if (parent == null)
            {
                root = replacement;
                replacement.parent = null;
            }
            else
            {
                parent.children.Remove(node);
                if (replacement != null)
                {
                    AddChildNode(parent, replacement);
                }
            }
        }

        private void PropagateNodeChange(AabbTreeNode<T> innernode, bool recalculateBounds)
        {
            while (innernode != null)
            {
                var parent = innernode.parent;
                if (innernode.children.Count <= 0)
                {
                    ReplaceNode(innernode, null);
                } else if (parent != null && innernode.children.Count < minChildren)
                {
                    ReplaceNode(innernode, null);
                    foreach (var child in innernode.children)
                    {
                        AddChildNode(parent, child);
                    }
                } else {
                    if (innernode.children.Count > maxChildren)
                    {
                        innernode = ExplodeNode(innernode);
                        recalculateBounds = true;
                    }
                    ShallowUpdateNode(innernode, recalculateBounds);
                    BalanceNodeByRotations(innernode);
                }
                innernode = parent;
            }
        }

        private AabbTreeNode<T> GetMinDepthChild(AabbTreeNode<T> node)
        {
            return node.children.MinBy(DepthOf);
        }
        private AabbTreeNode<T> GetMaxDepthChild(AabbTreeNode<T> node)
        {
            return node.children.MaxBy(DepthOf);
        }

        private void BalanceNodeByRotations(AabbTreeNode<T> node)
        {
            if (!balanceEnableRotations) return;
            if (node.leaf) return;
            for (var safetyi = 0; safetyi < 3; safetyi++)
            {
                var maxnode = GetMaxDepthChild(node);
                var minnode = GetMinDepthChild(node);
                var threshold = balanceDepthDifferenceThreshold + node.depth * balanceDepthDifferencePercent / 100;
                if (maxnode.depth <= minnode.depth + threshold) break;
                var maxnodechild = GetMaxDepthChild(maxnode);
                SwapNodes(maxnodechild, minnode);
                ShallowUpdateNode(maxnode, true);
            }
        }

        private void ShallowUpdateNode(AabbTreeNode<T> node, bool recalculateBounds)
        {
            if (node.leaf) return;
            var maxnode = GetMaxDepthChild(node);
            if (node == null)
            {
                throw new InvalidOperationException();
            }

            if (maxnode == null)
            {
                throw new InvalidOperationException();
            }
            node.depth = maxnode.depth + 1;
            if (recalculateBounds)
            {
                node.bounds = AaRects.MergeAll(node.children.Select(c => c.bounds));
            }
        }

        private void SwapNodes(AabbTreeNode<T> a, AabbTreeNode<T> b)
        {
            AabbTreeNode<T> aparent = a.parent, bparent = b.parent;
            if (aparent == null || bparent == null || a == b) 
            {
                return;
            }
            DetachNode(a);
            DetachNode(b);
            AddChildNode(bparent, a);
            AddChildNode(aparent, b);
        }

        private void DetachNode(AabbTreeNode<T> node)
        {
            node.parent.children.Remove(node);
            node.parent = null;
        }

        private Comparison<AabbTreeNode<T>> OrderNodeByX = OrderBy<AabbTreeNode<T>, float>(n => n.bounds.min.x); 
        private Comparison<AabbTreeNode<T>> OrderNodeByY = OrderBy<AabbTreeNode<T>, float>(n => n.bounds.min.y); 
        private AabbTreeNode<T> ExplodeNode(AabbTreeNode<T> node)
        {
            if (node.leaf) return node;
            var size = node.bounds.GetSize();
            node.children.Sort(size.y > size.x ? OrderNodeByY : OrderNodeByX);
            var oldChildren = node.children;
            var childCount = oldChildren.Count;
            node.children = new List<AabbTreeNode<T>>();
            var intermediatorCount = childCount / minChildren;
            for (var i = 0; i < intermediatorCount; i++)
            {
                AddChildNode(node, new AabbTreeNode<T>()
                {
                    children = new List<AabbTreeNode<T>>(),
                    parent = node,
                    leaf = false
                });
            }

            var childIndex = 0;
            foreach (var child in oldChildren)
            {
                var intermediate = node.children[childIndex * intermediatorCount / childCount]; 
                AddChildNode(intermediate, child);
                childIndex++;
            }
            foreach (var intermediator in node.children)
            {
                ShallowUpdateNode(intermediator, true);
            }
            return node;
        }
        
        private static Comparison<T> OrderBy<T, U>(Func<T, U> selector)
        where U : IComparable<U>
        {
            var comparer = Comparer<U>.Default;
            return (a, b) => comparer.Compare(selector(a), selector(b));
        }

        public override void TraverseOverlapping(AaRect rect, Action<T> action)
        {
            if (type == TreeType.Bag)
            {
                var skipped = 0;
                foreach (var body in bodies)
                {
                    if (AaRect.Overlaps(GetBodyFitRect(body), rect))
                    {
                        action(body);
                    }
                    else
                    {
                        skipped++;
                    }
                }

                return;
            }

            if (root != null)
            {
                PossibleCollisionsUsingTree(rect, root, action);
            }
        }

        private int DebugCountLeaves(AabbTreeNode<T> node)
        {
            if (node == null) return 0;
            if (node.leaf) return 1;
            int result = 0;
            foreach (var child in node.children)
            {
                result += DebugCountLeaves(child);
            }
            return result;
        }
        
        private void PossibleCollisionsUsingTree(AaRect rect, AabbTreeNode<T> node, Action<T> action)
        {
            if (!AaRect.Overlaps(rect, node.bounds))
            {
                return;
            }

            marks?.Add(node);
            if (node.leaf)
            {
                if (AaRect.Overlaps(rect, GetBodyFitRect(node.body)))
                {
                    action?.Invoke(node.body);
                }
            }
            else
            {
                foreach (var child in node.children)
                {
                    PossibleCollisionsUsingTree(rect, child, action);
                }
            }
        }


        private static Func<AabbTreeNode<T>, int> DepthOf = c => c.depth;

        public void Rebuild()
        {
            if (type != TreeType.RebuildTree) return;
            
            var list = new List<AabbTreeNode<T>>();

            foreach (var b in bodies)
            {
                var n = new AabbTreeNode<T>
                {
                    body = b,
                    bounds = GetBodyFitRect(b),
                    leaf = true,
                };
                bodyNodes[b] = n;
                list.Add(n);
            }
            root = RebuildNode(list, null);
        }

        private AabbTreeNode<T> RebuildNode(List<AabbTreeNode<T>> leaves, AabbTreeNode<T> parent)
        {
            int expectedChildren = maxChildren - 1; 
            if (leaves.Count <= expectedChildren)
            {
                var n = new AabbTreeNode<T>
                {
                    children = leaves,
                    depth = MaxDepth(leaves) + 1,
                    parent = parent,
                };
                foreach (var leaf in leaves)
                {
                    leaf.parent = n;
                }
                return n;
            }
            var unionrect = AaRects.MergeAll(leaves.Select(l => l.bounds));
            if (unionrect.GetSize().x > unionrect.GetSize().y)
            {
                leaves.Sort(OrderNodeByX);
            }
            else
            {
                leaves.Sort(OrderNodeByY);
            }
            var result = new AabbTreeNode<T>
            {
                children = new List<AabbTreeNode<T>>(),
                parent = parent,
            };

            for (int i = 0; i < expectedChildren; i++)
            {
                var i0 = i * leaves.Count / expectedChildren;
                var i1 = (i + 1) * leaves.Count / expectedChildren;
                result.children.Add(RebuildNode(leaves.GetRange(i0, i1 - i0), result));
            }
            
            ShallowUpdateNode(result, true);
            return result;
        }

        private int MaxDepth(List<AabbTreeNode<T>> leaves)
        {
            return leaves.Max(DepthOf);
        }
    }
}
