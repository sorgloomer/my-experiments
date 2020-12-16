using WindowsFormsApp1.PhysicsEngine.KDBoxTree;

namespace WindowsFormsApp1.PhysicsEngine
{
    public class RigidbodyKdTree : KdTree<Rigidbody>
    {
        private BoundingRects rects = new BoundingRects();

        public override AaRect GetBodyFitRect(Rigidbody body)
        {
            return rects.GetBodyFitRect(body);
        }

        public override AaRect GetBodyFatRect(Rigidbody body)
        {
            return rects.GetBodyFatRect(body);
        }
    }
    
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
    
    public class DummyRigidbodyIndex : DummySpatialIndex<Rigidbody>
    {
        private BoundingRects rects = new BoundingRects();
        public override AaRect GetBodyFitRect(Rigidbody body)
        {
            return rects.GetBodyFitRect(body);
        }

        public override AaRect GetBodyFatRect(Rigidbody body)
        {
            return rects.GetBodyFatRect(body);
        }
    }
}