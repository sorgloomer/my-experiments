using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine.KDBoxTree
{
    public abstract class KdTree<T> : AbstractTree<T>
    {
        public KdTreeNode<T> root = new KdTreeNode<T>();

        private Dictionary<T, KdTreeHolder<T>> leaves = new Dictionary<T, KdTreeHolder<T>>();
        internal int newRefresh = 10000;
        
        private RebalanceQueue<T> rebalance;
        
        public List<KdTreeHolder<T>> tempHolderList = new List<KdTreeHolder<T>>();

        public override IRectTreeNode Root => root;
        public abstract AaRect GetBodyFitRect(T body);
        public abstract AaRect GetBodyFatRect(T body);
        
        
        public override bool Contains(T body)
        {
            return leaves.ContainsKey(body);
        }
        
        public override bool Remove(T body)
        {
            if (leaves.TryGetValue(body, out var holder))
            {
                var node = holder.parent;
                RemoveInternal(holder);
                leaves.Remove(body);
                RefreshAndMarkParents(node);
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
                UpdateInternal(holder);
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
            AddNewInternal(holder);
            RebalanceOne();
            return true;
        }


        private void RebalanceOne()
        {
            KdTreeNode<T> node;
            for (;;)
            { 
                node = DequeueRebalanceQueue();
                if (node == null) return;
                if (IsNodeUnbalanced(node)) break;
            }

            node.Rebalance(this);
        }
        private void AddNewInternal(KdTreeHolder<T> holder)
        {
            KdTreeNode<T> fittingLeaf = DetermineFittingLeaf(holder);
            AddInternal(holder, fittingLeaf);
            RefreshAndMarkParents(fittingLeaf);
        }

        private void UpdateInternal(KdTreeHolder<T> holder)
        {
            if (!holder.Update(this))
            {
                return;
            }
            KdTreeNode<T> oldLeaf = holder.parent;
            KdTreeNode<T> newLeaf = DetermineFittingLeaf(holder);
            if (oldLeaf == newLeaf)
            {
                return;
            }

            RemoveInternal(holder);
            AddInternal(holder, newLeaf);
            
            RefreshAndMarkParents(oldLeaf);
            RefreshAndMarkParents(newLeaf);
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
            int sideThreshold = node.count * 2 / 3;
            if (node.type == NodeType.Inner)
            {
                return node.count <= 1 || node.inner.nodeLess.count > sideThreshold || node.inner.nodeMore.count > sideThreshold; 
            }
            return node.count > 1;
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
    }
}