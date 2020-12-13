namespace WindowsFormsApp1.PhysicsEngine.KDBoxTree
{
    public class KdTreeLeaf<T>
    {
        public KdTreeNode<T> parent;
        public T body;
        public AaRect fitRect, fatRect;
    }
}