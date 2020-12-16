using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine.KDBoxTree
{
    public abstract class KdTree<T> : AbstractSpatialIndex<T>
    {
        public KdTreeNode<T> root = new KdTreeNode<T>();

        private Dictionary<T, KdTreeHolder<T>> leaves = new Dictionary<T, KdTreeHolder<T>>();
        internal int newRefresh = 10000;
        private int rebalanceDivider = 10;
        private int rebalanceCounter = 0;
        
        private RebalanceQueue<T> rebalance;
        
        public List<KdTreeHolder<T>> tempHolderList = new List<KdTreeHolder<T>>();

        public override IRectTreeNode Root => root;

        public override bool Contains(T body)
        {
            return leaves.ContainsKey(body);
        }
        
        public override bool Remove(T body)
        {
            if (leaves.TryGetValue(body, out var holder))
            {
                var oldParent = holder.parent;
                RemoveInternal(holder);
                leaves.Remove(body);
                RefreshAndMarkParents(oldParent);
                return true;
            }
            return false;
        }

        public override void TraverseOverlapping(T body, Action<T> action)
        {
            TraverseOverlapping(root, GetBodyFitRect(body), action);
        }

        private void TraverseOverlapping(KdTreeNode<T> node, AaRect rect, Action<T> action)
        {
            if (node.type == NodeType.Leaf)
            {
                var holder = node.leaf.holder;
                while (holder != null)
                {
                    if (AaRect.Overlaps(rect, holder.fitRect))
                    {
                        action(holder.body);
                    }
                    holder = holder.sibling.next;
                }
            }
            else
            {
                var pos = node.DetermineInnerNodePosition(rect);
                if (pos != ChildPosition.More)
                    TraverseOverlapping(node.inner.nodeLess, rect, action);
                if (pos != ChildPosition.Less)
                    TraverseOverlapping(node.inner.nodeMore, rect, action);
                TraverseOverlapping(node.inner.nodeMiddle, rect, action);
            }
        }

        public override IEnumerable<T> GetAll()
        {
            return leaves.Keys;
        }

        
        public override bool Add(T body)
        {
            if (leaves.TryGetValue(body, out var holder))
            {
                UpdateAndRefreshInternal(holder);
                RebalanceOne();
                return false;
            }

            holder = new KdTreeHolder<T>()
            {
                body = body,
                fitRect = GetBodyFatRect(body),
                fatRect = GetBodyFitRect(body),
            };
            leaves[body] = holder;
            AddAndRefreshInternal(holder);
            RebalanceOne();
            return true;
        }


        private void RebalanceOne()
        {
            AssertConsistency();
            if (rebalanceCounter > 0)
            {
                rebalanceCounter--;
                return;
            }
            rebalanceCounter = rebalanceDivider;
            KdTreeNode<T> node;
            for (;;)
            { 
                node = DequeueRebalanceQueue();
                if (node == null)
                {
                    return;
                }
                if (!node.deleted && IsNodeUnbalanced(node))
                {
                    AssertConsistency();
                    node.Rebalance(this);
                    RefreshAndMarkParents(node);
                    AssertConsistency();
                    return;
                }
            }
        }
        
        private void AddAndRefreshInternal(KdTreeHolder<T> holder)
        {
            AssertConsistency();
            KdTreeNode<T> fittingLeaf = DetermineFittingLeaf(holder);
            AddInternal(holder, fittingLeaf);
            RefreshAndMarkParents(fittingLeaf);
            AssertConsistency();
        }

        private void UpdateAndRefreshInternal(KdTreeHolder<T> holder)
        {
            AssertConsistency();
            if (!holder.Update(this))
            {
                return;
            }
            KdTreeNode<T> oldLeaf = holder.parent;
            KdTreeNode<T> newLeaf = DetermineFittingLeaf(holder);
            if (oldLeaf == newLeaf)
            {
                RefreshAndMarkParents(oldLeaf);
                return;
            }

            AssertConsistency();
            RemoveInternal(holder);
            AddInternal(holder, newLeaf);
            RefreshAndMarkParents(oldLeaf);
            RefreshAndMarkParents(newLeaf);
            AssertConsistency();
        }

        private void RefreshAndMarkParents(KdTreeNode<T> node)
        {
            while (node != null)
            {
                node.RefreshBounds();
                MarkRebalanceIfNeeded(node);
                node = node.parent;
            }
        }

        public void MarkRebalanceIfNeeded(KdTreeNode<T> node)
        {
            if (!node.rebalance.added && IsNodeUnbalanced(node))
            {
                DeferRebalance(node);
            }
        }

        private bool IsNodeUnbalanced(KdTreeNode<T> node)
        {
            if (node.type == NodeType.Inner)
            {
                int sideThreshold = node.count * 2 / 3;
                int middleThreshold = node.count * 1 / 3;
                if (node.count <= 1)
                {
                    return true;
                }
                if (node.inner.nodeLess.count > sideThreshold)
                {
                    return true;
                }
                if (node.inner.nodeMore.count > sideThreshold)
                {
                    return true;
                }
                if (node.inner.nodeMiddle.count > middleThreshold)
                {
                    return true;
                }
                return false;
            }
            return node.count > 2;
        }

        private void AddInternal(KdTreeHolder<T> holder, KdTreeNode<T> newParent)
        {
            holder.AddToHolderList(newParent);
            var node = newParent;
            while (node != null)
            {
                node.count++;
                node = node.parent;
            }
        }

        private void RemoveInternal(KdTreeHolder<T> holder)
        {
            var node = holder.parent;
            holder.RemoveFromHolderList();
            while (node != null)
            {
                node.count--;
                node = node.parent;
            }
        }
        
        private KdTreeNode<T> DetermineFittingLeaf(KdTreeHolder<T> holder)
        {
            var node = root;
            while (node.type == NodeType.Inner)
            {
                node = node.DetermineInnerNode(holder);
            }
            return node;
        }
        
        private bool DeferRebalance(KdTreeNode<T> node)
        {
            if (node.rebalance.added)
            {
                return false;
            }
            node.rebalance.added = true;
            node.rebalance.next = null;
            if (rebalance.last != null)
            {
                rebalance.last.rebalance.next = node;
            }
            else
            {
                rebalance.first = node;
            }
            rebalance.last = node;
            return true;
        }
        
        private KdTreeNode<T> DequeueRebalanceQueue()
        {
            var result = rebalance.first;
            if (result == null)
            {
                return null;
            }
            var newfirst = result.rebalance.next; 
            rebalance.first = newfirst;
            if (newfirst == null)
            {
                rebalance.last = null;
            }
            result.rebalance.added = false;
            return result;
        }

        private void AssertConsistency()
        {
            // AssertConsistency(root);
        }
        private void AssertConsistency(KdTreeNode<T> node)
        {
            if (
                node.parent != null &&
                node.bounds.HasValue && 
                !node.parent.bounds.Value.Contains(node.bounds.Value)
            )
            {
                throw new Exception();
            }
            if (node.count > 0 != node.bounds.HasValue)
            {
                throw new Exception();
            }
            if (node.type == NodeType.Inner)
            {
                AssertConsistency(node.inner.nodeLess);
                AssertConsistency(node.inner.nodeMiddle);
                AssertConsistency(node.inner.nodeMore);
            }
            if (node.type == NodeType.Leaf)
            {
                KdTreeHolder<T> holder = node.leaf.holder;
                while (holder != null)
                {
                    if (!node.bounds.Value.Contains(holder.fatRect))
                    {
                        
                    }
                    holder = holder.sibling.next;
                }
            }
        }
    }
}