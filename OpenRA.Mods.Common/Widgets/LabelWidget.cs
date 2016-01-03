#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public enum TextAlign { Left, Center, Right }
	public enum TextVAlign { Top, Middle, Bottom }
	public enum LineVAlign { Top, Middle, Bottom, Collapsed }
	public enum LineSpacingType { Percentage, FixedMargin, FixedHeight }

	public class LabelWidget : Widget
	{
		[Translate] public string Text = null;
		public TextAlign Align = TextAlign.Left;
		public TextVAlign VAlign = TextVAlign.Middle;
		public string Font = ChromeMetrics.Get<string>("TextFont");
		public Color TextColor = ChromeMetrics.Get<Color>("TextColor");
		public bool Contrast = ChromeMetrics.Get<bool>("TextContrast");
		public Color ContrastColor = ChromeMetrics.Get<Color>("TextContrastColor");
		public bool WordWrap = false;

		[Desc("Space between lines as a percentage (default) of line height or fixed pixel amount.")]
		public int LineSpacing = 140;

		[Desc("Percentage: line height = LineSpacing% * font size.",
			"FixedMargin: line height = font size + LineSpacing.",
			"FixedHeight: line height = LineSpacing.")]
		public LineSpacingType LineSpacingType = LineSpacingType.Percentage;
		public LineVAlign LineVAlign = LineVAlign.Middle;
		public Func<string> GetText;
		public Func<Color> GetColor;
		public Func<Color> GetContrastColor;
		public int FontSize { get { return font.Value.Size; } }
		public int LinePixelSpacing { get { return linePixelSpacing.Value; } }
		public SpriteFont SpriteFont { get { return font.Value; } }
		Lazy<int> linePixelSpacing;
		Lazy<SpriteFont> font;

		SpriteFont GetFont()
		{
			SpriteFont font;
			if (!Game.Renderer.Fonts.TryGetValue(Font, out font))
				throw new ArgumentException("Requested font '{0}' was not found.".F(Font));
			return font;
		}

		int GetLinePixelSpacing()
		{
			switch (LineSpacingType)
			{
				case LineSpacingType.Percentage:
					return (LineSpacing - 100) * FontSize / 100;
				case LineSpacingType.FixedHeight:
					return LineSpacing - FontSize;
				case LineSpacingType.FixedMargin:
				default:
					return LineSpacing;
			}
		}

		public LabelWidget()
		{
			GetText = () => Text;
			GetColor = () => TextColor;
			GetContrastColor = () => ContrastColor;
			linePixelSpacing = new Lazy<int>(GetLinePixelSpacing);
			font = new Lazy<SpriteFont>(GetFont);
		}

		protected LabelWidget(LabelWidget other)
			: base(other)
		{
			Text = other.Text;
			Align = other.Align;
			Font = other.Font;
			TextColor = other.TextColor;
			Contrast = other.Contrast;
			ContrastColor = other.ContrastColor;
			WordWrap = other.WordWrap;
			GetText = other.GetText;
			GetColor = other.GetColor;
			GetContrastColor = other.GetContrastColor;
			linePixelSpacing = other.linePixelSpacing;
			font = other.font;
		}

		public int2 MeasureText(string text)
		{
			var textSize = font.Value.Measure(text, linePixelSpacing.Value);
			if (LineVAlign != LineVAlign.Collapsed)
				textSize += new int2(0, linePixelSpacing.Value);
			return textSize;
		}

		public string WrapText(string text)
		{
			return WidgetUtils.WrapText(text, Bounds.Width, font.Value);
		}

		public string TruncateText(string text)
		{
			return WidgetUtils.TruncateText(text, Bounds.Width, font.Value);
		}

		public int2 ResizeToText(string text)
		{
			var size = MeasureText(text);
			Bounds.Width = size.X;
			Bounds.Height = size.Y;
			return size;
		}

		public override void Draw()
		{
			var text = GetText();
			if (text == null)
				return;

			var textSize = MeasureText(text);
			var position = RenderOrigin;
			var lineSpacing = linePixelSpacing.Value;

			if (VAlign == TextVAlign.Middle)
				position += new int2(0, (Bounds.Height - textSize.Y) / 2);

			if (VAlign == TextVAlign.Bottom)
				position += new int2(0, Bounds.Height - textSize.Y);

			if (Align == TextAlign.Center)
				position += new int2((Bounds.Width - textSize.X) / 2, 0);

			if (Align == TextAlign.Right)
				position += new int2(Bounds.Width - textSize.X, 0);

			if (LineVAlign == LineVAlign.Middle)
				position += new int2(0, lineSpacing / 2);

			if (LineVAlign == LineVAlign.Bottom)
				position += new int2(0, lineSpacing);

			if (WordWrap)
				text = WidgetUtils.WrapText(text, Bounds.Width, font.Value);

			var color = GetColor();
			var contrast = GetContrastColor();
			if (Contrast)
				font.Value.DrawTextWithContrast(text, position, color, contrast, 2, lineSpacing);
			else
				font.Value.DrawText(text, position, color, lineSpacing);
		}

		public override Widget Clone() { return new LabelWidget(this); }
	}
}
