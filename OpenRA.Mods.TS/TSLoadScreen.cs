#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Drawing;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;

namespace OpenRA.Mods.TS
{
	public class TSLoadScreen : ILoadScreen
	{
		Dictionary<string, string> Info;
		static string[] Comments = new[] { "Updating EVA installation...", "Changing perspective..." };

		Stopwatch lastLoadScreen = new Stopwatch();
		Rectangle StripeRect;
		Sprite Stripe, Logo;
		float2 LogoPos;

		Renderer r;
		public void Init(Dictionary<string, string> info)
		{
			Info = info;
			// Avoid standard loading mechanisms so we
			// can display loadscreen as early as possible
			r = Game.Renderer;
			if (r == null) return;

			var s = new Sheet(Info["LoadScreenImage"]);
			Logo = new Sprite(s, new Rectangle(0,0,256,256), TextureChannel.Alpha);
			Stripe = new Sprite(s, new Rectangle(256,0,256,256), TextureChannel.Alpha);
			StripeRect = new Rectangle(0, r.Resolution.Height/2 - 128, r.Resolution.Width, 256);
			LogoPos =  new float2(r.Resolution.Width/2 - 128, r.Resolution.Height/2 - 128);
		}

		public void Display()
		{
			if (r == null)
				return;

			// Update text at most every 0.5 seconds
			if (lastLoadScreen.ElapsedTime() < 0.5)
				return;

			if (r.Fonts == null)
				return;

			lastLoadScreen.Reset();
			var text = Comments.Random(Game.CosmeticRandom);
			var textSize = r.Fonts["Bold"].Measure(text);

			r.BeginFrame(float2.Zero, 1f);
			WidgetUtils.FillRectWithSprite(StripeRect, Stripe);
			r.RgbaSpriteRenderer.DrawSprite(Logo, LogoPos);
			r.Fonts["Bold"].DrawText(text, new float2(r.Resolution.Width - textSize.X - 20, r.Resolution.Height - textSize.Y - 20), Color.White);
			r.EndFrame( new NullInputHandler() );
		}

		public void StartGame()
		{
			TestAndContinue();
		}

		void TestAndContinue()
		{
			Ui.ResetAll();
			if (!FileSystem.Exists(Info["TestFile"]))
			{
				var args = new WidgetArgs()
				{
					{ "continueLoading", () => TestAndContinue() },
					{ "installData", Info }
				};
				Ui.OpenWindow(Info["InstallerMenuWidget"], args);
			}
			else
				Game.LoadShellMap();
		}
	}
}

