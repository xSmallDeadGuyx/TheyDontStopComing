using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Xml.Serialization;

namespace TheyDontStopComing {
	public class Entity {
		public Vector pos;
		public Vector movement;
		public Color color;
		public bool canPushCrates = false;

		public Trail getTrail() {
			return new Trail(pos, color);
		}

		public Vector getMovingTo() {
			return new Vector(pos.x / 32 + movement.x / Map.speed, pos.y / 32 + movement.y / Map.speed);
		}
	}
	public class Trail {
		public Vector pos;
		public Color color;

		public Trail(Vector p, Color c) {
			pos = p;
			color = c;
		}
	}

	public class Map {
		TDSC tdsc;
		bool levelLoaded = false;
		MapStore mapstore = new MapStore();
		KeyboardState kbPrev = Keyboard.GetState();

		bool isTest = false;
		bool isCustom = false;

		public Map(TDSC tdsc) {
			this.tdsc = tdsc;
		}

		public Entity player = new Entity(); // S
		List<Entity> ais = new List<Entity>(); // A
		List<Entity> crates = new List<Entity>(); // C
		List<Entity> noCrates = new List<Entity>(); // N
		List<Entity> safe = new List<Entity>(); // G
		List<Trail> trails = new List<Trail>();
		Vector end;

		bool moving;
		public static int speed = 8;
		Pathfinder pathfinder;

		public bool[,] blocks;
		int level = 0;

		public void loadLevel(int n) {
			levelLoaded = false;
			level = n;
			isTest = false;
			isCustom = false;
			if(n >= mapstore.maps.GetLength(0) || n < 0)
				return;

			trails.Clear();
			ais.Clear();
			crates.Clear();
			noCrates.Clear();
			safe.Clear();

			blocks = new bool[21, 21];
			bool foundStart = false;
			bool foundEnd = false;
			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++) {
					blocks[i, j] = false;
					switch(mapstore.maps[n, j][i]) {
						case '#':
							blocks[i, j] = true;
							break;
						case 'S':
							player.pos = new Vector(32 * i, 32 * j);
							foundStart = true;
							break;
						case 'E':
							end = new Vector(i, j);
							foundEnd = true;
							break;
						case 'A':
							Entity ai = new Entity();
							ai.pos = new Vector(32 * i, 32 * j);
							ai.color = Color.Red;
							ai.canPushCrates = true;
							ais.Add(ai);
							break;
						case 'C':
							Entity crate = new Entity();
							crate.pos = new Vector(32 * i, 32 * j);
							crate.color = Color.Blue;
							crates.Add(crate);
							break;
						case 'N':
							Entity noCrate = new Entity();
							noCrate.pos = new Vector(32 * i, 32 * j);
							noCrate.color = Color.LightBlue;
							noCrates.Add(noCrate);
							break;
						case 'G':
							Entity s = new Entity();
							s.pos = new Vector(32 * i, 32 * j);
							s.color = Color.LightGray;
							safe.Add(s);
							break;
					}
				}
			if(!foundStart || !foundEnd) {
				levelLoaded = false;
				Console.WriteLine("INVALID LEVEL MISSING A START OR END. OH GOD WHAT DO I DO");
				return;
			}

			moving = false;
			levelLoaded = true;
		}

		public void loadTestLevel(char[,] map) {
			levelLoaded = false;
			isTest = true;
			isCustom = false;

			trails.Clear();
			ais.Clear();
			crates.Clear();
			noCrates.Clear();
			safe.Clear();

			blocks = new bool[21, 21];
			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++) {
					blocks[i, j] = false;
					switch(map[i, j]) {
						case '#':
							blocks[i, j] = true;
							break;
						case 'S':
							player.pos = new Vector(32 * i, 32 * j);
							break;
						case 'E':
							end = new Vector(i, j);
							break;
						case 'A':
							Entity ai = new Entity();
							ai.pos = new Vector(32 * i, 32 * j);
							ai.color = Color.Red;
							ai.canPushCrates = true;
							ais.Add(ai);
							break;
						case 'C':
							Entity crate = new Entity();
							crate.pos = new Vector(32 * i, 32 * j);
							crate.color = Color.Blue;
							crates.Add(crate);
							break;
						case 'N':
							Entity noCrate = new Entity();
							noCrate.pos = new Vector(32 * i, 32 * j);
							noCrate.color = Color.LightBlue;
							noCrates.Add(noCrate);
							break;
						case 'G':
							Entity s = new Entity();
							s.pos = new Vector(32 * i, 32 * j);
							s.color = Color.LightGray;
							safe.Add(s);
							break;
					}
				}

			moving = false;
			levelLoaded = true;
		}

		public void loadCustomLevel() {
			FileStream stream = File.Open("mapeditor.xml", FileMode.OpenOrCreate, FileAccess.Read);
			XmlSerializer serializer = new XmlSerializer(typeof(string[]));
			string[] map = (string[]) serializer.Deserialize(stream);
			levelLoaded = false;
			isTest = false;
			isCustom = true;

			trails.Clear();
			ais.Clear();
			crates.Clear();
			noCrates.Clear();
			safe.Clear();

			blocks = new bool[21, 21];
			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++) {
					blocks[i, j] = false;
					if(map[j].Length == 0) continue;
					switch(map[j][i]) {
						case '#':
							blocks[i, j] = true;
							break;
						case 'S':
							player.pos = new Vector(32 * i, 32 * j);
							break;
						case 'E':
							end = new Vector(i, j);
							break;
						case 'A':
							Entity ai = new Entity();
							ai.pos = new Vector(32 * i, 32 * j);
							ai.color = Color.Red;
							ai.canPushCrates = true;
							ais.Add(ai);
							break;
						case 'C':
							Entity crate = new Entity();
							crate.pos = new Vector(32 * i, 32 * j);
							crate.color = Color.Blue;
							crates.Add(crate);
							break;
						case 'N':
							Entity noCrate = new Entity();
							noCrate.pos = new Vector(32 * i, 32 * j);
							noCrate.color = Color.LightBlue;
							noCrates.Add(noCrate);
							break;
						case 'G':
							Entity s = new Entity();
							s.pos = new Vector(32 * i, 32 * j);
							s.color = Color.LightGray;
							safe.Add(s);
							break;
					}
				}

			moving = false;
			levelLoaded = true;
			stream.Close();
		}

		public bool nextLevel() {
			return level < mapstore.maps.GetLength(0) - 1;
		}

		public void resetLevel() {
			if(levelLoaded) {
				if(isTest)
					loadTestLevel(tdsc.mapEditor.level);
				else if(isCustom)
					loadCustomLevel();
				else
					loadLevel(level);
			}
		}

		public void init() {
			player.color = Color.Green;
			player.canPushCrates = true;
			pathfinder = new Pathfinder(this);
			loadLevel(level);
		}

		private void startPlayerMovement() {
			moving = true;
			foreach(Entity crate in crates)
				if(crate.pos / 32 == player.pos / 32 + player.movement / speed)
					crate.movement = player.movement;

			for(int i = 0; i < ais.Count; i++) {
				List<Vector> path = pathfinder.FindPath(ais[i].pos / 32, player.pos / 32, ais[i]);
				ais[i].movement = new Vector(0, 0);
				if(path != null && path.Count > 0) {
					if(canMoveTo(path[0], ais[i].pos / 32, ais[i])) {
						Vector nextPos = path[0];
						Vector diff = nextPos - (ais[i].pos / 32);
						ais[i].movement = diff * speed;

						foreach(Entity crate in crates)
							if(crate.pos / 32 == ais[i].pos / 32 + ais[i].movement / speed)
								crate.movement = ais[i].movement;
					}
				}
			}
		}

		private void endPlayerMovement() {
			moving = false;
			player.movement = new Vector(0, 0);
			for(int i = 0; i < ais.Count; i++)
				ais[i].movement = new Vector(0, 0);
			for(int i = 0; i < crates.Count; i++)
				crates[i].movement = new Vector(0, 0);
		}

		public bool canMoveTo(Vector to, Vector from, Entity e) {
			if(to.x < 0 || to.y < 0 || to.x > 20 || to.y > 20) return false;
			if(e != player && player.getMovingTo() == to) return false;
			foreach(Entity ai in ais)
				if(e != ai && ai.getMovingTo() == to) return false;
			foreach(Entity crate in crates)
				if(e != null && e.canPushCrates && crate.pos / 32 == to) {
					Vector diff = crate.pos / 32 - from;
					return canMoveTo(to + diff, crate.pos / 32, crate);
				}
				else if(e != crate && crate.getMovingTo() == to) return false;
			foreach(Entity n in noCrates)
				if(n.pos / 32 == to && crates.Contains(e))
					return false;
			foreach(Entity s in safe)
				if(s.pos / 32 == to)
					return e == player;
			return !blocks[to.x, to.y];
		}

		public void update() {
			if(!levelLoaded) return;

			KeyboardState kb = Keyboard.GetState();
			if(kb.IsKeyDown(tdsc.restartKey) && !kbPrev.IsKeyDown(tdsc.restartKey))
				resetLevel();
			if(kb.IsKeyDown(tdsc.exitKey) && !kbPrev.IsKeyDown(tdsc.exitKey)) {
				if(isTest)
					tdsc.state = TDSC.GameState.MapEditor;
				else if(isCustom)
					tdsc.state = TDSC.GameState.MainMenu;
			}

			if(!moving) {
				player.movement = new Vector(0, 0);
				if(kb.IsKeyDown(tdsc.leftKey) && !kbPrev.IsKeyDown(tdsc.leftKey))
					player.movement += new Vector(-speed, 0);
				if(kb.IsKeyDown(tdsc.rightKey) && !kbPrev.IsKeyDown(tdsc.rightKey))
					player.movement += new Vector(speed, 0);
				if(player.movement == new Vector(0, 0)) { // prevent diagonal movement
					if(kb.IsKeyDown(tdsc.upKey) && !kbPrev.IsKeyDown(tdsc.upKey))
						player.movement += new Vector(0, -speed);
					if(kb.IsKeyDown(tdsc.downKey) && !kbPrev.IsKeyDown(tdsc.downKey))
						player.movement += new Vector(0, speed);
				}

				if(player.movement != new Vector(0, 0) && canMoveTo(player.pos / 32 + player.movement / speed, player.pos / 32, player))
					startPlayerMovement();
			}

			if(player.pos / 32 == end) {
				if(isTest) {
					tdsc.state = TDSC.GameState.MapEditor;
					return;
				}

				if(nextLevel() && !isCustom) {
					level++;
					loadLevel(level);
				}
				else
					tdsc.winSequence();
			}

			if(moving) {
				trails.Add(player.getTrail());
				player.pos += player.movement;

				for(int i = 0; i < ais.Count; i++) {
					trails.Add(ais[i].getTrail());
					ais[i].pos += ais[i].movement;
				}

				for(int i = 0; i < crates.Count; i++) {
					crates[i].pos += crates[i].movement;
				}

				if(player.pos.x % 32 == 0 && player.pos.y % 32 == 0)
					endPlayerMovement();
			}

			List<Trail> trailsToRemove = new List<Trail>();
			for(int i = 0; i < trails.Count; i++) {
				if(trails[i].color.A <= 30)
					trailsToRemove.Add(trails[i]);
				else
					trails[i].color.A -= 30;
			}
			foreach(Trail t in trailsToRemove)
				trails.Remove(t);

			kbPrev = kb;
		}

		public void draw() {
			if(!levelLoaded) return;
			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++)
					if(blocks[i, j])
						tdsc.DrawRectangle(new Rectangle(i * 32, j * 32, 32, 32), Color.Black);
			tdsc.DrawRectangle(new Rectangle(end.x * 32, end.y * 32, 32, 32), Color.Yellow);
			foreach(Entity n in noCrates)
				tdsc.DrawRectangle(new Rectangle(n.pos.x, n.pos.y, 32, 32), n.color);
			foreach(Entity s in safe)
				tdsc.DrawRectangle(new Rectangle(s.pos.x, s.pos.y, 32, 32), s.color);

			foreach(Trail t in trails)
				tdsc.DrawRectangle(new Rectangle(t.pos.x, t.pos.y, 32, 32), t.color);
			foreach(Entity ai in ais)
				tdsc.DrawRectangle(new Rectangle(ai.pos.x, ai.pos.y, 32, 32), ai.color);
			foreach(Entity crate in crates)
				tdsc.DrawRectangle(new Rectangle(crate.pos.x, crate.pos.y, 32, 32), crate.color);
			tdsc.DrawRectangle(new Rectangle(player.pos.x, player.pos.y, 32, 32), player.color);

			if(isTest)
				tdsc.fontRenderer.DrawText(8, 640, "Press " + tdsc.exitKey + " to return to editor");
			else if(isCustom)
				tdsc.fontRenderer.DrawText(8, 640, "Press " + tdsc.exitKey + " to return to main menu");
			else
				tdsc.fontRenderer.DrawText(8, 640, mapstore.levelText[level]);
		}
	}
}
