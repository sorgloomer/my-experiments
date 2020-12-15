using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp1.PhysicsEngine.KDBoxTree
{
    public class KdTreeNode<T> : IRectTreeNode
    {
        public KdTreeNode<T> parent;
        public AaRect? bounds;
        public int count;
        public int refresh = 0;
        public RebalanceLink<T> rebalance;

        public NodeType type = NodeType.Leaf;
        public NodeInner<T> inner;
        public NodeLeaf<T> leaf;

        public KdTreeNode<T> DetermineInnerNode(KdTreeHolder<T> holder)
        {
            var pos = DetermineInnerNodePosition(holder.fatRect);
            switch (pos)
            {
                case ChildPosition.Less:
                    return inner.nodeLess;
                case ChildPosition.More:
                    return inner.nodeMore;
                default:
                    return inner.nodeMiddle;
            }
        }
        public ChildPosition DetermineInnerNodePosition(AaRect rect)
        {
            float pivot = inner.pivot;
            float holderMin = ByAxis(rect.min);
            float holderMax = ByAxis(rect.max);
            if (holderMax < pivot)
                return ChildPosition.Less;
            if (holderMin > pivot)
                return ChildPosition.More;
            return ChildPosition.Both;
        }
        
        private float ByAxis(Vec2 v)
        {
            return ByAxis(inner.axis, v);
        }
        
        private float ByAxis(Axis axis, Vec2 v)
        {
            return axis == Axis.X ? v.x : v.y;
        }

        public void RefreshBounds()
        {
            bounds = CalculateNewBounds();
        }
        
        private AaRect? CalculateNewBounds()
        {
            if (type == NodeType.Leaf)
            {
                AaRect? newBounds = null;
                KdTreeHolder<T> holder = leaf.holder;
                while (holder != null)
                {
                    newBounds = AaRects.MergeNullable(newBounds, holder.fatRect);
                    holder = holder.sibling.next;
                }
                return newBounds;
            }
            return AaRects.MergeNullable(inner.nodeLess.bounds, inner.nodeMiddle.bounds, inner.nodeMore.bounds);
        }

        public void Rebalance(KdTree<T> tree)
        {
            switch (type)
            {
                case NodeType.Inner:
                {
                    if (count <= 1)
                    {
                        ConvertToLeaf();
                    }
                    else
                    {
                        ConvertToLeaf();
                        ConvertToInnerMaybe(tree); // TODO: proper progressive rebalancing
                    }
                    
                } break;
                case NodeType.Leaf:
                {
                    ConvertToInnerMaybe(tree);
                } break;
            }
        }

        private void ConvertToInnerMaybe(KdTree<T> tree)
        {
            if (count <= 1) return;
            
            bool successX = AttemptAxis(Axis.X, tree.tempHolderList, out var middleCountX, out var pivotX);
            bool successY = AttemptAxis(Axis.Y, tree.tempHolderList, out var middleCountY, out var pivotY);
            Axis foundAxis;
            int middleCount;
            float pivot;
            
            if (successX && (!successY || middleCountX < middleCountY))
            {
                middleCount = middleCountX;
                foundAxis = Axis.X;
                pivot = pivotX;
            }
            else if (successY)
            {
                middleCount = middleCountY;
                foundAxis = Axis.Y;
                pivot = pivotY;
            }
            else
            {
                return;
            }
            

            if (middleCount >= count * 2 / 3)
            {
                return;
            }

            
            type = NodeType.Inner;
            inner.axis = foundAxis;
            inner.pivot = pivot;
            inner.nodeMiddle = new KdTreeNode<T> { parent = this };
            inner.nodeLess = new KdTreeNode<T> { parent = this };
            inner.nodeMore = new KdTreeNode<T> { parent = this };

            var holder = leaf.holder;
            leaf.holder = null;
            while (holder != null)
            {
                var nextHolder = holder.sibling.next;
                KdTreeNode<T> newParent = DetermineInnerNode(holder);
                holder.AddToHolderList(newParent);
                newParent.count++;
                holder = nextHolder;
            }
            
            tree.MarkRebalanceIfNeeded(inner.nodeMiddle);
            tree.MarkRebalanceIfNeeded(inner.nodeLess);
            tree.MarkRebalanceIfNeeded(inner.nodeMore);
        }

        private bool AttemptAxis(Axis axis, List<KdTreeHolder<T>> holders, out int outMiddleCount, out float outPivot)
        {
            float sumSpan = 0;
            KdTreeHolder<T> holder;
            holder = leaf.holder;
            while (holder != null)
            {
                var axisSpan = SpanByAxis(axis, holder);
                sumSpan += axisSpan;
                holder = holder.sibling.next;
            }
            float maxSpan = sumSpan * 2f / count;
            holders.Clear();
            holder = leaf.holder;
            while (holder != null)
            {
                var axisSpan = SpanByAxis(axis, holder);
                if (axisSpan < maxSpan)
                {
                    holders.Add(holder);
                }
                holder = holder.sibling.next;
            }

            if (holders.Count < 2)
            {
                outPivot = -1;
                outMiddleCount = -1;
                return false;
            }
            SortByAxis(axis, holders);
            int pivotIndex = holders.Count / 2;
            float pivotA = ByAxis(axis, holders[pivotIndex - 1].fatRect.max);
            float pivotB = ByAxis(axis, holders[pivotIndex].fatRect.min);
            float pivotC = ByAxis(axis, holders[pivotIndex].fatRect.max);
            float pivotBC = pivotB > pivotA ? pivotB : pivotC;
            float pivot = (pivotA + pivotBC) * 0.5f;

            int middleCount = 0;
            holder = leaf.holder;
            while (holder != null)
            {
                float axisMin = ByAxis(axis, holder.fatRect.min);
                float axisMax = ByAxis(axis, holder.fatRect.max);
                if (axisMin <= pivot && axisMax >= pivot)
                {
                    middleCount++;
                }
                holder = holder.sibling.next;
            }

            outMiddleCount = middleCount;
            outPivot = pivot;
            return true;
        }

        private static void SortByAxis(Axis axis, List<KdTreeHolder<T>> holders)
        {
            if (axis == Axis.X)
            {
                holders.Sort(CompareByX);
            }
            else
            {
                holders.Sort(CompareByY);
            }
        }

        private static int CompareByX(KdTreeHolder<T> a, KdTreeHolder<T> b)
        {
            return a.fatRect.max.x.CompareTo(b.fatRect.max.x);
        }
        
        private static int CompareByY(KdTreeHolder<T> a, KdTreeHolder<T> b)
        {
            return a.fatRect.max.y.CompareTo(b.fatRect.max.y);
        }

        private float SpanByAxis(Axis axis, KdTreeHolder<T> holder)
        {
            var axisMin = ByAxis(axis, holder.fatRect.min);
            var axisMax = ByAxis(axis, holder.fatRect.max);
            return axisMax - axisMin;
        }

        private void ConvertToLeaf()
        {
            leaf.holder = null;
            MoveAllHoldersWhileDestroying(this);
            type = NodeType.Leaf;
            inner.nodeLess = null;
            inner.nodeMiddle = null;
            inner.nodeMore = null;
        }

        private void MoveAllHoldersWhileDestroying(KdTreeNode<T> newLeaf)
        {
            switch (type)
            {
                case NodeType.Inner:
                {
                    inner.nodeLess.MoveAllHoldersWhileDestroying(newLeaf);
                    inner.nodeMiddle.MoveAllHoldersWhileDestroying(newLeaf);
                    inner.nodeMore.MoveAllHoldersWhileDestroying(newLeaf);
                } break;
                case NodeType.Leaf:
                {
                    var holder = leaf.holder;
                    leaf.holder = null;
                    while (holder != null)
                    {
                        var next = holder.sibling.next;
                        holder.AddToHolderList(newLeaf);
                        holder = next;
                    }
                } break;
            }
        }

        public AaRect? Bounds => bounds;

        public IEnumerable<IRectTreeNode> Children()
        {
            if (type == NodeType.Leaf)
            {
                var holder = leaf.holder;
                while (holder != null)
                {
                    var next = holder.sibling.next;
                    yield return holder;
                    holder = next;
                }
            }
            else
            {
                yield return inner.nodeLess;
                yield return inner.nodeMiddle;
                yield return inner.nodeMore;
            }
        }
    }
    
    
    
    public class KdTreeHolder<T> : IRectTreeNode
    {
        
        public KdTreeNode<T> parent;
        public T body;
        public AaRect fitRect, fatRect;
        public HolderLink<T> sibling;
        public int refresh;
        
        public bool Update(KdTree<T> tree)
        {
            fitRect = tree.GetBodyFitRect(body);
            if (fatRect.Contains(fitRect) && refresh > 0)
            {
                refresh--;
                // body still in fat rect, skipping
                return false;
            }
            fatRect = tree.GetBodyFatRect(body);
            refresh = tree.newRefresh;
            return true;
        }
        
        
        public void RemoveFromHolderList()
        {
            var oldprev = sibling.prev;
            var oldnext = sibling.next;
            if (oldprev != null)
            {
                oldprev.sibling.next = sibling.next;
            }
            else
            {
                parent.leaf.holder = oldnext;
            }
            if (oldnext != null)
            {
                oldnext.sibling.prev = sibling.prev;
            }
            sibling.prev = null;
            sibling.next = null;
            parent = null;
        }

        public void AddToHolderList(KdTreeNode<T> newParent)
        {
            KdTreeHolder<T> oldfirst = newParent.leaf.holder;
            newParent.leaf.holder = this;
            if (oldfirst != null)
            {
                oldfirst.sibling.prev = this;
            }
            parent = newParent;
            sibling.next = oldfirst;
            sibling.prev = null; 
        }

        public AaRect? Bounds => fatRect;
        public IEnumerable<IRectTreeNode> Children()
        {
            return Enumerable.Empty<IRectTreeNode>();
        }
    }


    public enum NodeType
    {
        Leaf, Inner
    }

    public struct NodeInner<T>
    {
        public float pivot;
        public Axis axis;
        public KdTreeNode<T> nodeLess;
        public KdTreeNode<T> nodeMore;
        public KdTreeNode<T> nodeMiddle;
    }
    public struct NodeLeaf<T>
    {
        public KdTreeHolder<T> holder;
    }
    
    public struct HolderLink<T>
    {
        public KdTreeHolder<T> prev;
        public KdTreeHolder<T> next;
    }
    
    public struct RebalanceLink<T>
    {
        public KdTreeNode<T> next;
        public bool added;
    }
    
    public struct RebalanceQueue<T>
    {
        public KdTreeNode<T> first;
        public KdTreeNode<T> last;
    }
    
    public enum Axis
    {
        X, Y
    }
    
    public enum ChildPosition
    {
        Less, More, Both
    }
}