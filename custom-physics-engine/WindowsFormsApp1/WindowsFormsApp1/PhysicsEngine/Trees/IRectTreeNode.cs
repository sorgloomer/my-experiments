using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public interface IRectTreeNode
    {
        AaRect? Bounds { get; }
        IEnumerable<IRectTreeNode> Children();
    }
}