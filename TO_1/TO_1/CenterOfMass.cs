namespace TO_1
{
    public class CenterOfMass : Point
    {
        public int weight;

        public CenterOfMass()
            : base()
        {
            weight = 0;
            x = 0;
            y = 0;
        }

        public void AddPoint(Point point)
        {
            weight++;
            x += (point.x - x) / weight;            
            y += (point.y - y) / weight;
        }
    }
}
