using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Input;

namespace SeverFrontEnd
{
	abstract class Mode
	{
		/// <summary>The graphics device manager to use.</summary>
		protected GraphicsDeviceManager Graphics { get; set; }

		/// <summary>The content manager to use.</summary>
		protected ContentManager Content { get; set; }

		/// <summary>The sprite batch to use.</summary>
		public SpriteBatch Batch { get; set; }

		/// <summary>The basic effect to use.</summary>
		public BasicEffect BEffect { get; set; }

		/// <summary>The mode to switch to.  null while this mode is active.  Initialize it to a new instance of another mode and the engine will automatically switch them.</summary>
		public Mode SwitchTo { get; set; }

		/// <summary>The input states.</summary>
		protected InpState Inp { get; set; }

		/// <summary>Creates a new instance of Mode.</summary>
		/// <param name="graphics">The graphics device manager to use.</param>
		/// <param name="content">The content manager to use.</param>
		/// <param name="batch">The sprite batch to use.</param>
		/// <param name="bEffect">The basic effect to use.</param>
		public Mode(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch batch, BasicEffect bEffect)
		{
			Graphics = graphics;
			Content = content;
			Batch = batch;
			BEffect = bEffect;
			SwitchTo = null;
			Inp = new InpState();
		}

		/// <summary>Loads all content related to this mode.</summary>
		public abstract void LoadContent();

		/// <summary>Updates all game variables in this mode.</summary>
		/// <param name="gameTime">The current game time.</param>
		public virtual void Update(GameTime gameTime)
		{
			Inp.refreshStates();
		}

		/// <summary>Draws this mode.</summary>
		/// <param name="gameTime">The current game time.</param>
		public abstract void Draw(GameTime gameTime);
		
		/// <summary>Unloads all content related to this mode.</summary>
		public virtual void UnloadContent()
		{
			Content.Unload();
		}

		/// <summary>Cleans up and prepares to end this mode.</summary>
		public abstract void End();
	}
}
