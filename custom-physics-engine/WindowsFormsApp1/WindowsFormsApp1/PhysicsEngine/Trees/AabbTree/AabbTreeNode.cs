using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public class AabbTreeNode<T> : IRectTreeNode
    {
        public DrawnParams? drawn;
        public int fatsleep;
        public AaRect bounds;
        public AabbTreeNode<T> parent;
        public List<AabbTreeNode<T>> children;
        public T body;
        public bool leaf;
        public int depth;
        public AaRect? Bounds => bounds;
        public IEnumerable<IRectTreeNode> Children() => children;
    }

    public struct DrawnParams
    {
        public Vec2 position;
        public Vec2 layersize;

        public DrawnParams Approach(float vp, float vw, DrawnParams towards)
        {
            
            return new DrawnParams
            {
                position = position + (towards.position - position).Clamp(vp),
                layersize = layersize + (towards.layersize - layersize).Clamp(vw),
            };
        }
    }

}