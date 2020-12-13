using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Windows.Forms;

namespace WindowsFormsApp1.PhysicsEngine.KDBoxTree
{
    public class KdTree<T> : AbstractTree<T>
    {
        public KdTreeNode<T> root;

        private Dictionary<Rigidbody, KdTreeNode<T>> leaves = new Dictionary<Rigidbody, KdTreeNode<T>>();
        private HashSet<Rigidbody> bodies = new HashSet<Rigidbody>();
        private int newRefresh = 10000;
        
        private BoundingRects rects = new BoundingRects();

        public void AddOrUpdate(Rigidbody rigidbody)
        {
            bodies.Add(rigidbody);
            if (leaves.TryGetValue(rigidbody, out var leaf))
            {
                if (leaf.bounds.Contains(rects.GetBodyFitRect(rigidbody)))
                {
                    // body still in fat rect, skipping
                    if (leaf.refresh > 0)
                    {
                        leaf.refresh--;
                        return;
                    }
                }
                Remove(leaf);
                leaf.bounds = rects.GetBodyFatRect(rigidbody);
                leaf.refresh = newRefresh;
            }
            else
            {
                leaf = new KdTreeNode()
                {
                    leaf = true,
                    rigidbody = rigidbody, 
                    bounds = rects.GetBodyFatRect(rigidbody),
                    count = 1,
                    refresh = newRefresh,
                };
            }

            root = Add(null, root, leaf);
        }

        public KdTreeNode Add(KdTreeNode parent, KdTreeNode node, KdTreeNode leaf)
        {
            if (node == null)
            {
                leaf.parent = parent;
                return leaf;
            }

            if (node.leaf)
            {
                return Split(node, leaf);
            }
            float lmin, lmax;
            float pivot = node.pivot;
            
            if (node.axis == 0)
            {
                lmin = leaf.bounds.min.x;
                lmax = leaf.bounds.max.x;
            }
            else
            {
                lmin = leaf.bounds.min.y;
                lmax = leaf.bounds.max.y;
            }

            if (lmax < pivot)
            {
                node.nodeLess = Add(node, node.nodeLess, leaf);
            }
            else if (lmin > pivot)
            {
                node.nodeMore = Add(node, node.nodeMore, leaf);
            }
            else
            {
                node.nodeMiddle = Add(node, node.nodeMiddle, leaf);
            }


            node.count = node.nodeLess?.count ?? 0 + node.nodeMiddle?.count ?? 0 + node.nodeMore?.count ?? 0;
            node.bounds = rects.MergeNullable(node.nodeLess?.bounds, node.nodeMiddle?.bounds, node.nodeMore?.bounds).Value;
            return node;
        }

        private KdTreeNode Split(KdTreeNode node1, KdTreeNode node2)
        {
            var bounds = AaRect.Merge(node1.bounds, node2.bounds);
            var size = bounds.GetSize();
            var axis = size.x > size.y ? 0 : 1;
            float min1, max1, min2, max2;
            if (axis == 0)
            {
                min1 = node1.bounds.min.x;
                max1 = node1.bounds.max.x;
                min2 = node2.bounds.min.x;
                max2 = node2.bounds.max.x;
            }
            else
            {
                min1 = node1.bounds.min.y;
                max1 = node1.bounds.max.y;
                min2 = node2.bounds.min.y;
                max2 = node2.bounds.max.y;
            }
            
            var result = new KdTreeNode()
            {
                axis = axis,
                leaf = false,
                count = node1.count + node2.count,
                bounds = bounds,
            };

            if (max1 < min2)
            {
                result.pivot = (max1 + min2) / 2;
                result.nodeLess = node1;
                result.nodeMore = node2;
            } 
            else if (max2 < min1)
            {
                result.pivot = (max2 + min1) / 2;
                result.nodeLess = node2;
                result.nodeMore = node1;
            }
            else if (max1 - min1 < )
            {
                
            }
            
            return result;
        }

        private void Remove(KdTreeNode node)
        {
            for (;;)
            {
                var parent = node.parent;
                if (parent == null)
                {
                    root = null;
                    return;
                }
                if (parent.nodeLess == node) parent.nodeLess = null;
                if (parent.nodeMiddle == node) parent.nodeMiddle = null;
                if (parent.nodeMore == node) parent.nodeMore = null;
                if (parent.count > node.count)
                {
                    parent.count -= node.count;
                    return;
                }
                node = parent;
            }


        }

    }
}