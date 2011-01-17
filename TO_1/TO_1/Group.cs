namespace TO_1
{
    public class Group
    {
        public CenterOfMass centerOfMass;
        public byte id;
        public long distance = 0;

        public Group(byte groupId)
        {
            id = groupId;
            centerOfMass = new CenterOfMass();
        }

        public void AddPoint(Point point)
        {
            centerOfMass.AddPoint(point);
        }
    }
}
