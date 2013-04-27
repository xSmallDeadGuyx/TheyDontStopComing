using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TheyDontStopComing {
	public class FontRenderer {
		public FontRenderer(FontFile fontFile, Texture2D fontTexture, SpriteBatch spriteBatch) {
			_fontFile = fontFile;
			_texture = fontTexture;
			_spriteBatch = spriteBatch;
			_characterMap = new Dictionary<char, FontChar>();

			foreach(var fontCharacter in _fontFile.Chars) {
				char c = (char) fontCharacter.ID;
				_characterMap.Add(c, fontCharacter);
			}
		}

		private SpriteBatch _spriteBatch;
		private Dictionary<char, FontChar> _characterMap;
		private FontFile _fontFile;
		private Texture2D _texture;
		public void DrawText(int x, int y, string text) {
			int dx = x;
			int dy = y;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc)) {
					var sourceRectangle = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
					var position = new Vector2(dx + fc.XOffset, dy + fc.YOffset);

					_spriteBatch.Draw(_texture, position, sourceRectangle, Color.White);
					dx += fc.XAdvance;
				}
			}
		}

		public int GetTextWidth(string text) {
			int dx = 0;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc))
					dx += fc.XAdvance;
			}
			return dx;
		}

		public int GetMaxHeight(string text) {
			int max = 0;
			foreach(char c in text) {
				FontChar fc;
				if(_characterMap.TryGetValue(c, out fc)) {
					if(fc.Height > max)
						max = fc.Height;
				}
			}
			return max;
		}
	}
}
