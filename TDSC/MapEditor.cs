using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml.Serialization;

namespace TheyDontStopComing {
	public class MapEditor {
		private TDSC tdsc;

		int saveTimer;

		MouseState msPrev;
		bool hitButton = false;

		public char[,] level = new char[21, 21];
		Dictionary<char, Color> tiles = new Dictionary<char, Color>();
		bool levelValid = false;

		char selectedTool = '#';

		public MapEditor(TDSC t) {
			tdsc = t;

			clearLevel();

			tiles.Add(' ', Color.White);
			tiles.Add('S', Color.Green);
			tiles.Add('E', Color.Yellow);
			tiles.Add('#', Color.Black);
			tiles.Add('A', Color.Red);
			tiles.Add('C', Color.Blue);
			tiles.Add('N', Color.LightBlue);
			tiles.Add('G', Color.LightGray);
		}

		public void clearLevel() {
			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++)
					level[i, j] = ' ';
		}

		private void saveLevel() {
			string[] map = new string[21];
			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++)
					map[i] += level[j, i];

			FileStream stream = File.Open("mapeditor.xml", FileMode.OpenOrCreate);
			XmlSerializer serializer = new XmlSerializer(typeof(string[]));
			serializer.Serialize(stream, map);
			stream.Close();

			saveTimer = 300;
		}

		private void loadLevel() {
			if(!File.Exists("mapeditor.xml"))
				return;

			FileStream stream = File.Open("mapeditor.xml", FileMode.OpenOrCreate, FileAccess.Read);
			XmlSerializer serializer = new XmlSerializer(typeof(string[]));
			string[] map = (string[]) serializer.Deserialize(stream);
			for(int i = 0; i < 21; i++) {
				if(map[i].Length > 0)
					for(int j = 0; j < 21; j++)
						if(map[i].Length > 0) // XML reads all spaces as blank string
							level[j, i] = map[i][j];
						else
							level[j, i] = ' ';
			}
			stream.Close();

			checkValidLevel();
		}

		private void checkValidLevel() {
			levelValid = false;
			bool foundStart = false;
			bool foundEnd = false;
			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++)
					if(level[i, j] == 'S') {
						if(foundStart) {
							levelValid = false;
							return;
						}
						else
							foundStart = true;
					}
					else if(level[i, j] == 'E') {
						if(foundEnd) {
							levelValid = false;
							return;
						}
						else
							foundEnd = true;
					}
			levelValid = foundStart && foundEnd;
		}

		public void init() {
			msPrev = Mouse.GetState();
		}

		public void update() {
			MouseState ms = Mouse.GetState();

			if(saveTimer > 0)
				saveTimer--;

			if(ms.LeftButton == ButtonState.Released)
				hitButton = false;

			if(!hitButton && ms.LeftButton == ButtonState.Pressed && msPrev.LeftButton == ButtonState.Released) {
				if(ms.X >= 16 + tdsc.fontRenderer.GetTextWidth("Tiles:") && ms.X < 16 + tdsc.fontRenderer.GetTextWidth("Tiles:") + tiles.Count * 16 && ms.Y >= 16 && ms.Y < 32) {
					int tID = (ms.X - 16 - tdsc.fontRenderer.GetTextWidth("Tiles:")) / 16;
					selectedTool = tiles.ElementAt(tID).Key;
					hitButton = true;
				}

				if(ms.X >= 624 - tdsc.fontRenderer.GetTextWidth("Test level") && ms.X < 664 && ms.Y >= 8 && ms.Y < 40 && levelValid) {
					tdsc.map.loadTestLevel(level);
					tdsc.state = TDSC.GameState.InGame;
					hitButton = true;
				}

				if(ms.Y >= 624 & ms.Y < 664) {
					if(ms.X >= 8 && ms.X < 48 + tdsc.fontRenderer.GetTextWidth("Save") && levelValid && saveTimer == 0) {
						hitButton = true;
						saveLevel();
					}
					if(ms.X >= (672 - tdsc.fontRenderer.GetTextWidth("Load")) / 2 - 20 && ms.X < (672 + tdsc.fontRenderer.GetTextWidth("Load")) / 2 + 20) {
						hitButton = true;
						loadLevel();
					}
					if(ms.X >= 624 - tdsc.fontRenderer.GetTextWidth("Exit") && ms.X < 664) {
						hitButton = true;
						tdsc.state = TDSC.GameState.MainMenu;
					}
				}
			}

			if(ms.LeftButton == ButtonState.Pressed && !hitButton) {
				int cx = ms.X / 32;
				int cy = ms.Y / 32;
				if(cx >= 0 && cy >= 0 && cx < 21 && cy < 21) {
					level[cx, cy] = selectedTool;
					if(selectedTool == 'S')
						for(int i = 0; i < 21; i++)
							for(int j = 0; j < 21; j++)
								if((i != cx || j != cy) && level[i, j] == 'S')
									level[i, j] = ' ';
					if(selectedTool == 'E')
						for(int i = 0; i < 21; i++)
							for(int j = 0; j < 21; j++)
								if((i != cx || j != cy) && level[i, j] == 'E')
									level[i, j] = ' ';
					checkValidLevel();
				}
			}

			msPrev = ms;
		}

		public void draw() {
			MouseState ms = Mouse.GetState();

			for(int i = 0; i < 21; i++)
				for(int j = 0; j < 21; j++)
					tdsc.DrawRectangle(new Rectangle(i * 32, j * 32, 32, 32), tiles[level[i, j]]);

			tdsc.fontRenderer.DrawText(8, 15, "Tiles:");
			tdsc.DrawRectangle(new Rectangle(16 + tdsc.fontRenderer.GetTextWidth("Tiles:"), 16, tiles.Count * 16, 16), Color.Black);
			for(int i = 0; i < tiles.Count; i++) {
				if(tiles.ElementAt(i).Key == selectedTool)
					tdsc.DrawRectangle(new Rectangle(16 + tdsc.fontRenderer.GetTextWidth("Tiles:") + i * 16, 16, 16, 16), Color.Gray);
				tdsc.DrawRectangle(new Rectangle(17 + tdsc.fontRenderer.GetTextWidth("Tiles:") + i * 16, 17, 14, 14), tiles.ElementAt(i).Value);
			}

			bool overButton = false;
			if(ms.X >= 16 + tdsc.fontRenderer.GetTextWidth("Tiles:") && ms.X < 16 + tdsc.fontRenderer.GetTextWidth("Tiles:") + tiles.Count * 16 && ms.Y >= 16 && ms.Y < 32)
				overButton = true;

			tdsc.fontRenderer.DrawText(664 - tdsc.fontRenderer.GetTextWidth("Test level"), 15, "Test level");
			if(ms.X >= 624 - tdsc.fontRenderer.GetTextWidth("Test level") && ms.X < 664 && ms.Y >= 8 && ms.Y < 40 && levelValid) {
				tdsc.DrawRectangle(new Rectangle(624 - tdsc.fontRenderer.GetTextWidth("Test level"), 8, 32, 32), Color.LimeGreen);
				overButton = true;
			}

			tdsc.fontRenderer.DrawText(48, 631, saveTimer == 0 ? "Save" : "Saved to mapeditor.xml");
			tdsc.fontRenderer.DrawText((672 - tdsc.fontRenderer.GetTextWidth("Load")) / 2 + 20, 631, "Load");
			tdsc.fontRenderer.DrawText(664 - tdsc.fontRenderer.GetTextWidth("Exit"), 631, "Exit");
			
			if(ms.Y >= 624 & ms.Y < 664) {
				if(ms.X >= 8 && ms.X < 48 + tdsc.fontRenderer.GetTextWidth("Save") && levelValid && saveTimer == 0) {
					overButton = true;
					tdsc.DrawRectangle(new Rectangle(8, 624, 32, 32), Color.Purple);
				}
				if(ms.X >= (672 - tdsc.fontRenderer.GetTextWidth("Load")) / 2 - 20 && ms.X < (672 + tdsc.fontRenderer.GetTextWidth("Load")) / 2 + 20) {
					overButton = true;
					tdsc.DrawRectangle(new Rectangle((672 - tdsc.fontRenderer.GetTextWidth("Load")) / 2 - 20, 624, 32, 32), Color.LimeGreen);
				}
				if(ms.X >= 624 - tdsc.fontRenderer.GetTextWidth("Exit") && ms.X < 664) {
					overButton = true;
					tdsc.DrawRectangle(new Rectangle(624 - tdsc.fontRenderer.GetTextWidth("Exit"), 624, 32, 32), Color.GreenYellow);
				}
			}

			if(!overButton) {
				Color c = tiles[selectedTool];
				c.A = 100;
				tdsc.DrawRectangle(new Rectangle((ms.X / 32) * 32, (ms.Y / 32) * 32, 32, 32), c);
			}
		}
	}
}
