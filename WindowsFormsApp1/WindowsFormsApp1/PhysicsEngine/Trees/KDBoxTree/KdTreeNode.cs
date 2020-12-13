using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine.KDBoxTree
{
    public class KdTreeNode<T>
    {
        public const int CHILD_INDEX_LESS = 0;
        public const int CHILD_INDEX_MIDDLE = 1;
        public const int CHILD_INDEX_MORE = 2;
        
        public KdTreeNode<T> parent;
        public AaRect bounds;
        public List<KdTreeLeaf<T>> leaves;
        public float pivot;
        public int axis;
        public KdTreeNode<T>[] children = new KdTreeNode<T>[3];
        public int count;
        public int refresh;

        public KdTreeNode<T> NodeLess => children[CHILD_INDEX_LESS];
        public KdTreeNode<T> NodeMiddle => children[CHILD_INDEX_MIDDLE];
        public KdTreeNode<T> NodeMore => children[CHILD_INDEX_MORE];

    }
}