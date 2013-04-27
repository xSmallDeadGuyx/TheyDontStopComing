using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheyDontStopComing {
	public struct Vector {
		public int x;
		public int y;

		public Vector(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public static Vector operator +(Vector v1, Vector v2) {
			return new Vector(v1.x + v2.x, v1.y + v2.y);
		}

		public static Vector operator -(Vector v1, Vector v2) {
			return new Vector(v1.x - v2.x, v1.y - v2.y);
		}

		public static Vector operator *(Vector v1, Vector v2) {
			return new Vector(v1.x * v2.x, v1.y * v2.y);
		}

		public static Vector operator *(Vector v1, int m) {
			return new Vector(v1.x * m, v1.y * m);
		}

		public static Vector operator /(Vector v1, Vector v2) {
			return new Vector(v1.x / v2.x, v1.y / v2.y);
		}

		public static Vector operator /(Vector v1, int m) {
			return new Vector(v1.x / m, v1.y / m);
		}

		public static bool operator ==(Vector v1, Vector v2) {
			return v1.x == v2.x && v1.y == v2.y;
		}

		public static bool operator !=(Vector v1, Vector v2) {
			return v1.x != v2.x || v1.y != v2.y;
		}

		public override bool Equals(object o) {
			try {
				return this == (Vector) o;
			}
			catch {
				return false;
			}
		}

		public override string ToString() {
			return "(" + x + ", " + y + ")";
		}
	}
}
