#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace SeverEditor
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class SeverEditorFront : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		BasicEffect basicEffect;
		Mode curMode;

		public SeverEditorFront()
			: base()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
			graphics.PreferredBackBufferHeight = 750;   // set this value to the desired height of your window
			graphics.ApplyChanges();
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			curMode = new LevelEditorMode(graphics, Content, spriteBatch, basicEffect);
			curMode.LoadContent();
			IsMouseVisible = true;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			//graphics.GraphicsDevice.SamplerStates[0] = SamplerSt.AddressU = TextureAddressMode.Wrap;
			//graphics.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			curMode.Batch = spriteBatch; // chances are the current mode was given a null sprite batch earlier

			basicEffect = new BasicEffect(graphics.GraphicsDevice);
			basicEffect.TextureEnabled = true;
			//basicEffect.VertexColorEnabled = true;
			basicEffect.Projection = Matrix.CreateOrthographicOffCenter
				(0, graphics.GraphicsDevice.Viewport.Width,     // left, right
				 graphics.GraphicsDevice.Viewport.Height, 0,    // bottom, top
				 0, 1); 
			curMode.BEffect = basicEffect;

			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// TODO: Add your update logic here
			
			// state switching
			if (curMode.SwitchTo != null)
			{
				curMode.UnloadContent();
				curMode = curMode.SwitchTo;
				curMode.LoadContent();
			}

			curMode.Update(gameTime);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// TODO: Add your drawing code here
			curMode.Draw(gameTime);

			base.Draw(gameTime);
		}
	}
}
