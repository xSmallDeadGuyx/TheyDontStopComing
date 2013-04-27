using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheyDontStopComing {
	public class Pathfinder {
		Map map;
		public Pathfinder(Map m) {
			map = m;
		}

		private Vector start;
		private Vector end;
		private Entity entity;
		private int[,] gScore;
		private int[,] hScore;
		private int[,] fScore;
		private Vector[,] cameFrom;

		private int calculateHeuristic(Vector pos) {
			return 10 * (Math.Abs(pos.x - end.x) + Math.Abs(pos.y - end.y));
		}

		private int distanceBetween(Vector pos1, Vector pos2) {
			return (int) Math.Round(10 * Math.Sqrt(Math.Pow(pos1.x - pos2.x, 2) + Math.Pow(pos1.y - pos2.y, 2)));
		}

		private Vector getLowestPointIn(List<Vector> list) {
			int lowest = -1;
			Vector found = new Vector(-1, -1);
			foreach(Vector p in list) {
				Vector c = cameFrom[p.x, p.y];
				int dist = c == new Vector(-1, -1) ? 0 : gScore[c.x, c.y] + distanceBetween(p, c) + calculateHeuristic(p);
				if(dist <= lowest || lowest == -1) {
					lowest = dist;
					found = p;
				}
			}
			return found;
		}

		private bool canMoveTo(int x, int y, int fx, int fy) {
			if(end == new Vector(x, y)) return true;
			if(new Vector(x, y) == map.player.getMovingTo()) return true;
			return map.canMoveTo(new Vector(x, y), new Vector(fx, fy), entity);
		}

		private List<Vector> getNeighbourPoints(Vector p) {
			List<Vector> found = new List<Vector>();
			if(canMoveTo(p.x + 1, p.y, p.x, p.y)) found.Add(new Vector(p.x + 1, p.y));
			if(canMoveTo(p.x - 1, p.y, p.x, p.y)) found.Add(new Vector(p.x - 1, p.y));
			if(canMoveTo(p.x, p.y + 1, p.x, p.y)) found.Add(new Vector(p.x, p.y + 1));
			if(canMoveTo(p.x, p.y - 1, p.x, p.y)) found.Add(new Vector(p.x, p.y - 1));
			return found;
		}

		private List<Vector> reconstructPath(Vector p) {
			if(p != start) {
				List<Vector> path = reconstructPath(cameFrom[p.x, p.y]);
				path.Add(p);
				return path;
			}
			else
				return new List<Vector>();
		}

		public List<Vector> FindPath(Vector s, Vector e, Entity o) {
			start = s;
			end = e;
			entity = o;

			int w = map.blocks.GetLength(0);
			int h = map.blocks.GetLength(1);
			gScore = new int[w, h];
			hScore = new int[w, h];
			fScore = new int[w, h];
			cameFrom = new Vector[w, h];

			List<Vector> open = new List<Vector>();
			List<Vector> closed = new List<Vector>();

			open.Add(start);
			gScore[start.x, start.y] = 0;
			hScore[start.x, start.y] = calculateHeuristic(start);
			fScore[start.x, start.y] = hScore[start.x, start.y];

			while(open.Count > 0) {
				Vector point = getLowestPointIn(open);
				if(point == end) return reconstructPath(cameFrom[point.x, point.y]);
				open.Remove(point);
				closed.Add(point);

				List<Vector> neighbours = getNeighbourPoints(point);
				foreach(Vector p in neighbours) {
					if(closed.Contains(p)) continue;

					int gPossible = gScore[point.x, point.y] + distanceBetween(p, point);

					if(!open.Contains(p) || (open.Contains(p) && gPossible < gScore[p.x, p.y])) {
						if(!open.Contains(p)) open.Add(p);
						cameFrom[p.x, p.y] = point;
						gScore[p.x, p.y] = gPossible;
						hScore[p.x, p.y] = calculateHeuristic(p);
						fScore[p.x, p.y] = gPossible + hScore[p.x, p.y];
					}
				}
			}
			return null;
		}
	}
}
