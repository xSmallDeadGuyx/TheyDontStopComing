#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace TheyDontStopComing {
	public class Options {
		public Keys LeftKey;
		public Keys RightKey;
		public Keys DownKey;
		public Keys UpKey;
		public Keys RestartKey;
		public Keys ExitKey;
		public float volume;
		public Options() {}
		public Options(Keys l, Keys r, Keys d, Keys u, Keys re, Keys e, float v) {
			LeftKey = l;
			RightKey = r;
			DownKey = d;
			UpKey = u;
			RestartKey = re;
			ExitKey = e;
			volume = v;
		}
	}
	public class TDSC : Game {
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Texture2D rectangleTex;

		public enum GameState{MainMenu, InGame, MapEditor, Options, Won};
		public GameState state;
		MouseState msPrev;
		KeyboardState kbPrev;

		public Map map;
		public MapEditor mapEditor;

		public FontRenderer fontRenderer;
		public FontRenderer largeFontRenderer;
		public SoundEffectInstance music;

		int totalMainMenuHeight;
		int maxMainMenuWidth;

		public Keys leftKey = Keys.Left;
		public Keys rightKey = Keys.Right;
		public Keys upKey = Keys.Up;
		public Keys downKey = Keys.Down;
		public Keys restartKey = Keys.R;
		public Keys exitKey = Keys.Escape;
		bool choosingLeft = false;
		bool choosingRight = false;
		bool choosingUp = false;
		bool choosingDown = false;
		bool choosingRestart = false;
		bool choosingExit = false;

		int totalOptionsHeight;
		int maxOptionsWidth;

		int totalWinHeight;

		public TDSC() : base() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			graphics.PreferredBackBufferWidth = 672;
			graphics.PreferredBackBufferHeight = 672;

			map = new Map(this);
			mapEditor = new MapEditor(this);
		}

		public void saveKeys() {
			string filename = "options.xml";
			FileStream stream = File.Open(filename, FileMode.OpenOrCreate);
			Options ko = new Options(leftKey, rightKey, downKey, upKey, restartKey, exitKey, music.Volume);
			XmlSerializer serializer = new XmlSerializer(typeof(Options));
			serializer.Serialize(stream, ko);
			stream.Close();
		}

		public void loadKeys() {
			string filename = "options.xml";
			if(!File.Exists(filename))       
				return;

			FileStream stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read);
			XmlSerializer serializer = new XmlSerializer(typeof(Options));
			Options ko = (Options) serializer.Deserialize(stream);
			stream.Close();

			leftKey = ko.LeftKey;
			rightKey = ko.RightKey;
			downKey = ko.DownKey;
			upKey = ko.UpKey;
			restartKey = ko.RestartKey;
			exitKey = ko.ExitKey;
			music.Volume = ko.volume;
		}

		protected override void Initialize() {
			base.Initialize();
			map.init();
			mapEditor.init();
			msPrev = Mouse.GetState();
			kbPrev = Keyboard.GetState();
		}

		protected override void LoadContent() {
			spriteBatch = new SpriteBatch(GraphicsDevice);

			rectangleTex = Content.Load<Texture2D>("rectangle.png");

			SoundEffect musicSE = Content.Load<SoundEffect>("music");
			music = musicSE.CreateInstance();
			music.Volume = 1.0F;
			music.IsLooped = true;

			fontRenderer = new FontRenderer(FontLoader.Load(Path.Combine(Content.RootDirectory, "visitor.fnt")), Content.Load<Texture2D>("visitor_0.png"), spriteBatch);
			largeFontRenderer = new FontRenderer(FontLoader.Load(Path.Combine(Content.RootDirectory, "visitorL.fnt")), Content.Load<Texture2D>("visitorL_0.png"), spriteBatch);

			totalMainMenuHeight = largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 200;
			maxMainMenuWidth = Math.Max(Math.Max(Math.Max(Math.Max(fontRenderer.GetTextWidth("Play"), fontRenderer.GetTextWidth("Options")), fontRenderer.GetTextWidth("Quit")), fontRenderer.GetTextWidth("Map Editor")), fontRenderer.GetTextWidth("Custom Level")) + 40;

			totalOptionsHeight = 328;
			maxOptionsWidth = fontRenderer.GetTextWidth("+ Volume level: 100% -") + 80;

			totalWinHeight = largeFontRenderer.GetMaxHeight("You win, I think") + 80;

			loadKeys();
		}

		protected override void UnloadContent() {
			rectangleTex.Dispose();
		}

		protected override void Update(GameTime gameTime) {
			if(music.State == SoundState.Stopped && music.Volume > 0f)
				music.Play();

			MouseState ms = Mouse.GetState();
			KeyboardState kb = Keyboard.GetState();
			switch(state) {
				case GameState.MainMenu:
					if(ms.LeftButton == ButtonState.Pressed && msPrev.LeftButton == ButtonState.Released)
						if(ms.X >= (672 - maxMainMenuWidth) / 2 && ms.X < (672 + maxMainMenuWidth) / 2) {
							if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 4 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 44) {
								map.loadLevel(0);
								state = GameState.InGame;
							}
							if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 44 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 84) {
								map.loadCustomLevel();
								state = GameState.InGame;
							}
							if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 84 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 124) {
								mapEditor.clearLevel();
								state = GameState.MapEditor;
							}
							if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 124 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 164)
								state = GameState.Options;
							if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 164 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 204)
								Exit();
						}
					break;
				case GameState.InGame:
					map.update();
					break;
				case GameState.MapEditor:
					mapEditor.update();
					break;
				case GameState.Options:
					List<Keys> keysPressed = new List<Keys>();
					foreach(Keys k in kb.GetPressedKeys())
						if(!kbPrev.IsKeyDown(k))
							keysPressed.Add(k);
					if(keysPressed.Count == 1) {
						Keys keyChanged = keysPressed[0];

						if(leftKey == keyChanged)
							leftKey = Keys.None;
						if(rightKey == keyChanged)
							rightKey = Keys.None;
						if(downKey == keyChanged)
							downKey = Keys.None;
						if(upKey == keyChanged)
							upKey = Keys.None;
						if(restartKey == keyChanged)
							restartKey = Keys.None;
						if(exitKey == keyChanged)
							exitKey = Keys.None;

						if(choosingLeft) {
							leftKey = keyChanged;
							choosingLeft = false;
						}
						else if(choosingRight) {
							rightKey = keyChanged;
							choosingRight = false;
						}
						else if(choosingDown) {
							downKey = keyChanged;
							choosingDown = false;
						}
						else if(choosingUp) {
							upKey = keyChanged;
							choosingUp = false;
						}
						else if(choosingRestart) {
							restartKey = keyChanged;
							choosingRestart = false;
						}
						else if(choosingExit) {
							exitKey = keyChanged;
							choosingExit = false;
						}
					}

					if(ms.LeftButton == ButtonState.Pressed && msPrev.LeftButton == ButtonState.Released)
						if(!choosingLeft && !choosingRight && !choosingDown && !choosingUp && !choosingRestart && !choosingExit)
							if(ms.X >= (672 - maxOptionsWidth) / 2 && ms.X < (672 + maxOptionsWidth) / 2) {
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 4 && ms.Y < (672 - totalOptionsHeight) / 2 + 44)
									choosingLeft = true;
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 44 && ms.Y < (672 - totalOptionsHeight) / 2 + 84)
									choosingRight = true;
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 84 && ms.Y < (672 - totalOptionsHeight) / 2 + 124)
									choosingDown = true;
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 124 && ms.Y < (672 - totalOptionsHeight) / 2 + 164)
									choosingUp = true;
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 164 && ms.Y < (672 - totalOptionsHeight) / 2 + 204)
									choosingRestart = true;
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 204 && ms.Y < (672 - totalOptionsHeight) / 2 + 244)
									choosingExit = true;
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 244 && ms.Y < (672 - totalOptionsHeight) / 2 + 284)
									if(ms.X < 336 && music.Volume < 1f) {
										music.Volume += 0.1f;
										music.Play();
									}
									else if(ms.X >= 336 && music.Volume > 0f) {
										music.Volume -= 0.1f;
										if(music.Volume <= 0f)
											music.Stop();
									}
								if(ms.Y >= (672 - totalOptionsHeight) / 2 + 284 && ms.Y < (672 - totalOptionsHeight) / 2 + 324) {
									saveKeys();
									state = GameState.MainMenu;
								}
							}
					break;
				case GameState.Won:
					if(ms.LeftButton == ButtonState.Pressed && msPrev.LeftButton == ButtonState.Released)
						if(ms.X >= (672 - fontRenderer.GetTextWidth("Back to main menu")) / 2 && ms.X < (672 + fontRenderer.GetTextWidth("Back to main menu")) / 2 && ms.Y >= (672 - totalWinHeight) / 2 + largeFontRenderer.GetMaxHeight("You win, I think") + 48 && ms.Y < (672 - totalWinHeight) / 2 + largeFontRenderer.GetMaxHeight("You win, I think") + 88)
							state = GameState.MainMenu;
					break;
			}

			msPrev = ms;
			kbPrev = kb;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.White);
			MouseState ms = Mouse.GetState();
			spriteBatch.Begin();

			switch(state) {
				case GameState.MainMenu:
					largeFontRenderer.DrawText((672 - largeFontRenderer.GetTextWidth("They Don't Stop Coming")) / 2, (672 - totalMainMenuHeight) / 2, "They Don't Stop Coming");
					fontRenderer.DrawText((672 - maxMainMenuWidth) / 2 + 40, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 15, "Play");
					fontRenderer.DrawText((672 - maxMainMenuWidth) / 2 + 40, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 55, "Custom Level");
					fontRenderer.DrawText((672 - maxMainMenuWidth) / 2 + 40, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 95, "Map Editor");
					fontRenderer.DrawText((672 - maxMainMenuWidth) / 2 + 40, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 135, "Options");
					fontRenderer.DrawText((672 - maxMainMenuWidth) / 2 + 40, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 175, "Quit");

					if(ms.X >= (672 - maxMainMenuWidth) / 2 && ms.X < (672 + maxMainMenuWidth) / 2) {
						if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 4 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 44)
							DrawRectangle(new Rectangle((672 - maxMainMenuWidth) / 2, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 8, 32, 32), Color.Green);
						if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 44 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 84 && File.Exists("mapeditor.xml"))
							DrawRectangle(new Rectangle((672 - maxMainMenuWidth) / 2, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 48, 32, 32), Color.Blue);
						if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 84 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 124)
							DrawRectangle(new Rectangle((672 - maxMainMenuWidth) / 2, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 88, 32, 32), Color.Yellow);
						if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 124 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 164)
							DrawRectangle(new Rectangle((672 - maxMainMenuWidth) / 2, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 128, 32, 32), Color.Red);
						if(ms.Y >= (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 164 && ms.Y < (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 204)
							DrawRectangle(new Rectangle((672 - maxMainMenuWidth) / 2, (672 - totalMainMenuHeight) / 2 + largeFontRenderer.GetMaxHeight("They Don't Stop Coming") + 168, 32, 32), Color.Black);
					}
					break;
				case GameState.InGame:
					map.draw();
					break;
				case GameState.MapEditor:
					mapEditor.draw();
					break;
				case GameState.Options:
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 15, "Move left: " + (choosingLeft ? "<choosing>" : leftKey.ToString()));
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 55, "Move right: " + (choosingRight ? "<choosing>" : rightKey.ToString()));
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 95, "Move down: " + (choosingDown ? "<choosing>" : downKey.ToString()));
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 135, "Move up: " + (choosingUp ? "<choosing>" : upKey.ToString()));
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 175, "Restart level: " + (choosingRestart ? "<choosing>" : restartKey.ToString()));
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 215, "Exit key: " + (choosingExit ? "<choosing>" : exitKey.ToString()));
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 255, "+ Volume level: " + Math.Round(100 * music.Volume) + "% -");
					fontRenderer.DrawText((672 - maxOptionsWidth) / 2 + 40, (672 - totalOptionsHeight) / 2 + 295, "Save & Return");

					if(ms.X >= (672 - maxOptionsWidth) / 2 && ms.X < (672 + maxOptionsWidth) / 2) {
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 4 && ms.Y < (672 - totalOptionsHeight) / 2 + 44)
							DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 8, 32, 32), Color.Red);
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 44 && ms.Y < (672 - totalOptionsHeight) / 2 + 84)
							DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 48, 32, 32), Color.LightGray);
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 84 && ms.Y < (672 - totalOptionsHeight) / 2 + 124)
							DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 88, 32, 32), Color.Yellow);
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 124 && ms.Y < (672 - totalOptionsHeight) / 2 + 164)
							DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 128, 32, 32), Color.Purple);
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 164 && ms.Y < (672 - totalOptionsHeight) / 2 + 204)
							DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 168, 32, 32), Color.LightBlue);
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 204 && ms.Y < (672 - totalOptionsHeight) / 2 + 244)
							DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 208, 32, 32), Color.Magenta);
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 244 && ms.Y < (672 - totalOptionsHeight) / 2 + 284)
							if(ms.X < 336)
								DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 248, 32, 32), Color.Orange);
							else
								DrawRectangle(new Rectangle((672 + maxOptionsWidth) / 2 - 32, (672 - totalOptionsHeight) / 2 + 248, 32, 32), Color.Orange);
						if(ms.Y >= (672 - totalOptionsHeight) / 2 + 284 && ms.Y < (672 - totalOptionsHeight) / 2 + 324)
							DrawRectangle(new Rectangle((672 - maxOptionsWidth) / 2, (672 - totalOptionsHeight) / 2 + 288, 32, 32), Color.Black);
					}
					break;
				case GameState.Won:
					largeFontRenderer.DrawText((672 - largeFontRenderer.GetTextWidth("You win!")) / 2, (672 - totalWinHeight) / 2, "You win!");
					fontRenderer.DrawText((672 - fontRenderer.GetTextWidth("Apparently they do stop coming...")) / 2, (672 - totalWinHeight) / 2 + largeFontRenderer.GetMaxHeight("You win!") + 15, "Apparently they do stop coming...");
					fontRenderer.DrawText((672 - fontRenderer.GetTextWidth("Back to main menu")) / 2 + 20, (672 - totalWinHeight) / 2 + largeFontRenderer.GetMaxHeight("You win!") + 55, "Back to main menu");
					if(ms.X >= (672 - fontRenderer.GetTextWidth("Back to main menu")) / 2 - 20 && ms.X < (672 + fontRenderer.GetTextWidth("Back to main menu")) / 2 + 20 && ms.Y >= (672 - totalWinHeight) / 2 + largeFontRenderer.GetMaxHeight("You win!") + 48 && ms.Y < (672 - totalWinHeight) / 2 + largeFontRenderer.GetMaxHeight("You win, I think") + 88)
						DrawRectangle(new Rectangle((672 - fontRenderer.GetTextWidth("Back to main menu")) / 2 - 20, (672 - totalWinHeight) / 2 + largeFontRenderer.GetMaxHeight("You win!") + 48, 32, 32), Color.Black);
					break;
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}

		public void DrawRectangle(Rectangle rectangle, Color color) {
			spriteBatch.Draw(rectangleTex, rectangle, color);
		}

		public void winSequence() {
			state = GameState.Won;
		}
	}
}
