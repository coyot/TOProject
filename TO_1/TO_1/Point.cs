using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TO_1
{
    public class Point : IEquatable<Point>
    {
        public int x;
        public int y;
        public int id;
        public byte groupId;
        public Point() { }
        public Point(string[] input)
        {
            x = int.Parse(input[1]);
            y = int.Parse(input[2]);
            id = int.Parse(input[0]);
        }

        public Point(int _id, int _x, int _y, byte _groupId)
        {
            this.id = _id;
            this.x = _x;
            this.y = _y;
            this.groupId = _groupId;
        }

        public Point Clone()
        {
            return new Point(this.id, this.x, this.y, this.groupId);
        }

        public int Distance(Point other)
        {
            if (other == null)
                return 0;
            return (int)Math.Round(Math.Sqrt((this.x - other.x) * (this.x - other.x) + (this.y - other.y) * (this.y - other.y)), 0);
        }

        [Obsolete]
        public Point GetMassCenter(IList<Point> pointsList)
        {
            return null;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.id == id;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Point)) return false;
            return Equals((Point) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return id;
        }

        public static bool operator ==(Point left, Point right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !Equals(left, right);
        }
    }
}
