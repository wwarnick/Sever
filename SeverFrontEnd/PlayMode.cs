using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sever;
using GUI;

namespace SeverFrontEnd
{
	class PlayMode : Mode
	{
		#region Members

		/// <summary>The world map.  Contains all game variables.</summary>
		private World map;

		/// <summary>The node that the cursor is currently hovering over.</summary>
		private Node hoveredNode;

		/// <summary>The segment that the cursor is currently hovering over.</summary>
		private Segment hoveredSeg;

		/// <summary>The hotspot that the cursor is currently hovering over.</summary>
		private Hotspot hoveredHotspot;

		/// <summary>The currently selected node.</summary>
		private Node selectedNode;

		/// <summary>The currently selected node type.</summary>
		private NodeType selNodeType;

		/// <summary>The point of intersection, if any.</summary>
		private VectorF? intersectPoint;

		/// <summary>Whether or not the game is paused.</summary>
		private bool paused = true;

		private FInt maxApproxErr = FInt.F0;

		/// <summary>The shortest path to each path node from a starting path node.</summary>
		private PathEdge[] shortest;

		private Desktop desktop;

		private Label lblCursorPos;

		//private int closestPathNode = 0;

		#region GUI

		#region cntMsgBox

		private Container cntMsgBox;
		private Label lblMsgBoxMessage;
		private ButtonText btnMsgBoxExit;

		#endregion cntMsgBox

		#endregion GUI
		#region Content

		/// <summary>The default font.</summary>
		private SpriteFont fDefault;

		/// <summary>The node texture.</summary>
		private Texture2D tNode;

		/// <summary>The segment texture.</summary>
		private Texture2D tSegment;

		/// <summary>The person texture.</summary>
		private Texture2D tPerson;

		/// <summary>The geo texture.</summary>
		private Texture2D tGeo;

		/// <summary>The node spacing texture.</summary>
		private Texture2D tSpacing;
		/// <summary>The hotspot texture.</summary>
		private Texture2D tHotspot;

		/// <summary>The origin of the node texture.</summary>
		private Vector2 oNode;

		/// <summary>The origin of the segment texture.</summary>
		private Vector2 oSegment;

		/// <summary>The origin of the person texture.</summary>
		private Vector2 oPerson;

		/// <summary>The origin of the node spacing texture.</summary>
		private Vector2 oSpacing;

		/// <summary>The origin of the hotspot texture.</summary>
		private Vector2 oHotspot;

		#endregion Content

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of PlayMode.</summary>
		/// <param name="graphics">The graphics device manager to use.</param>
		/// <param name="content">The content manager to use.</param>
		/// <param name="batch">The sprite batch to use.</param>
		/// <param name="bEffect">The basic effect to use.</param>
		public PlayMode(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch batch, BasicEffect bEffect)
			: base(graphics, content, batch, bEffect)
		{
			hoveredNode = null;
			hoveredSeg = null;
			hoveredHotspot = null;
			selectedNode = null;
			desktop = new Desktop();

			lblCursorPos = new Label();
			lblCursorPos.Left = 0;
			lblCursorPos.Top = 0;
			lblCursorPos.Width = 0;
			lblCursorPos.Height = 0;
			lblCursorPos.ForeColor = Color.White;
			desktop.Controls.Add(lblCursorPos);

			/* TEMPORARY */
			loadWorld(@"..\..\..\testscripting.txt");//testLevel.txt");
            //map = WorldLoader.loadWorld(@"..\..\..\testLevel.txt", Graphics);
			/* TEMPORARY */
		}

		#endregion Constructors

		#region Methods

		/// <summary>Loads all content related to this mode.</summary>
		public override void LoadContent()
		{
			fDefault = Content.Load<SpriteFont>("fonts/Segoe");

			tNode = Content.Load<Texture2D>("images/node");
			tSegment = Content.Load<Texture2D>("images/segment");
			tPerson = Content.Load<Texture2D>("images/person");
			tGeo = Content.Load<Texture2D>("images/primTexture");
			tSpacing = Content.Load<Texture2D>("images/spacing");
			tHotspot = Content.Load<Texture2D>("images/hotspot");

			oNode = new Vector2((float)tNode.Width / 2f, (float)tNode.Height / 2f);
			oSegment = new Vector2((float)tSegment.Width / 2f, (float)tSegment.Height / 2f);
			oPerson = new Vector2((float)tPerson.Width / 2f, (float)tPerson.Height / 2f);
			oSpacing = new Vector2((float)tSpacing.Width / 2f, (float)tSpacing.Height / 2f);
			oHotspot = new Vector2((float)tHotspot.Width / 2f, (float)tHotspot.Height / 2f);

			desktop.TBack = tSegment;
			lblCursorPos.Font = fDefault;

			buildGUI(Graphics);
		}

		/// <summary>Builds the GUI.</summary>
		/// <param name="graphics">The graphics device manager in use.</param>
		private void buildGUI(GraphicsDeviceManager graphics)
		{
			#region Defaults
			// set defaults first
			Desktop.DefControlBackColor = new Color(Color.Blue, .5f);
			Desktop.DefButtonBackColor = new Color(Color.White, .5f);
			Desktop.DefButtonBackColorHover = new Color(Color.White, .75f);
			Desktop.DefButtonBackColorPressed = new Color(Color.Black, .75f);
			Desktop.DefButtonBackColorP = new Color(Color.Black, .65f);
			Desktop.DefButtonBackColorHoverP = new Color(Color.Black, .5f);
			Desktop.DefButtonTextFont = fDefault;
			Desktop.DefButtonTextForeColor = Color.Black;
			Desktop.DefButtonTextForeColorHover = Color.Black;
			Desktop.DefButtonTextForeColorPressed = Color.Gray;
			Desktop.DefButtonTextForeColorP = Color.Gray;
			Desktop.DefButtonTextForeColorHoverP = Color.White;
			Desktop.DefComboBoxTextBoxBackColor = Color.White;
			Desktop.DefComboBoxTextBoxForeColor = Color.Black;
			Desktop.DefComboBoxSidePadding = 5;
			Desktop.DefComboBoxButtonWidth = 13;
			Desktop.DefLabelFont = fDefault;
			Desktop.DefLabelForeColor = Color.White;
			Desktop.DefPopUpMenuFont = fDefault;
			Desktop.DefPopUpMenuBackColor = Color.LightGray;
			Desktop.DefPopUpMenuBackColorHover = Color.Blue;
			Desktop.DefPopUpMenuForeColor = Color.Black;
			Desktop.DefPopUpMenuForeColorHover = Color.White;
			Desktop.DefPopUpMenuSidePadding = 5;
			Desktop.DefTextBoxFont = fDefault;
			Desktop.DefTextBoxBackColor = Color.White;
			Desktop.DefTextBoxForeColor = Color.Black;
			Desktop.DefTextBoxSidePadding = 5;
			Desktop.DefTextBoxVertPadding = 5;
			Desktop.DefTextBoxHighlightColor = new Color(0, 0, 255, 100);
			Desktop.DefTextBoxEditPositionOffsetX = -1;
			Desktop.DefTextBoxEditPositionOffsetY = -1;
			Desktop.DefListBoxBackColor = Color.LightGray;
			Desktop.DefListBoxBackColorHover = Color.CornflowerBlue;
			Desktop.DefListBoxBackColorSelected = Color.Blue;
			Desktop.DefListBoxTextFont = fDefault;
			Desktop.DefListBoxTextForeColor = Color.Black;
			Desktop.DefListBoxTextForeColorHover = Color.Black;
			Desktop.DefListBoxTextForeColorSelected = Color.White;
			desktop.TBack = tSegment;

			#endregion Defaults
			#region cntMsgBox

			//
			// cntMsgBox
			//
			cntMsgBox = new Container();
			cntMsgBox.Width = 600;
			cntMsgBox.Visible = false;

			// lblMsgBoxMessage
			lblMsgBoxMessage = new Label();
			lblMsgBoxMessage.AutoSize = false;
			lblMsgBoxMessage.Bounds = new Rectangle(0, 10, cntMsgBox.Width, 20);
			lblMsgBoxMessage.TextAlign = Desktop.Alignment.Center;
			cntMsgBox.Controls.Add(lblMsgBoxMessage);

			// btnMsgBoxExit
			btnMsgBoxExit = new ButtonText();
			btnMsgBoxExit.Bounds = new Rectangle((cntMsgBox.Width - 40) / 2, lblMsgBoxMessage.Bottom + 10, 40, 20);
			btnMsgBoxExit.Text = "OK";
			btnMsgBoxExit.MouseLeftUp += new EventHandler(btnMsgBoxExit_MouseLeftUp);
			cntMsgBox.Controls.Add(btnMsgBoxExit);

			cntMsgBox.Height = btnMsgBoxExit.Bottom + 10;
			cntMsgBox.Left = (graphics.PreferredBackBufferWidth - cntMsgBox.Width) / 2;
			cntMsgBox.Top = (graphics.PreferredBackBufferHeight - cntMsgBox.Height) / 2;

			desktop.Controls.Add(cntMsgBox);

			#endregion cntMsgBox
		}

		/// <summary>Shows a message to the user.</summary>
		/// <param name="message">The message to show.</param>
		private void showMsg(string message)
		{
			lblMsgBoxMessage.Text = message;
			cntMsgBox.Visible = true;
		}

		/// <summary>Ends the game.</summary>
		public override void End()
		{
			map.endGame();
		}

		/// <summary>Loads the specified world.</summary>
		/// <param name="path">The path of the world to load.</param>
		public void loadWorld(string path)
		{
			hoveredNode = null;
			selectedNode = null;
			hoveredSeg = null;
			map = WorldLoader.loadWorld(path.ToString(), Graphics);
		}

		/// <summary>Updates all game variables in this mode.</summary>
		/// <param name="gameTime">The current game time.</param>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			FInt elapsed = (FInt)gameTime.ElapsedGameTime.TotalSeconds;
			hoveredNode = null;
			hoveredSeg = null;
			hoveredHotspot = null;
			Player hPlayer = null;
			Player cPlayer = null;

			foreach (Player p in map.Players)
			{
				if (p.Type == Player.PlayerType.Human)
				{
					hPlayer = p;
					break;
				}
			}

			foreach (Player p in map.Players)
			{
				if (p.Type == Player.PlayerType.Computer)
				{
					cPlayer = p;
					break;
				}
			}

			// prepare grid manager
			map.Grid.startNewUpdate(gameTime);

			// move camera
			if (Inp.Key.IsKeyDown(Keys.OemPlus))
				map.Cam.Zoom += map.Cam.Zoom * elapsed;

			if (Inp.Key.IsKeyDown(Keys.OemMinus))
				map.Cam.Zoom -= map.Cam.Zoom * elapsed;

			if (Inp.Key.IsKeyDown(Keys.A))
				map.Cam.CenterX -= (700 / map.Cam.Zoom) * elapsed;

			if (Inp.Key.IsKeyDown(Keys.D))
				map.Cam.CenterX += (700 / map.Cam.Zoom) * elapsed;

			if (Inp.Key.IsKeyDown(Keys.W))
				map.Cam.CenterY -= (700 / map.Cam.Zoom) * elapsed;

			if (Inp.Key.IsKeyDown(Keys.S))
				map.Cam.CenterY += (700 / map.Cam.Zoom) * elapsed;

			map.Cam.Zoom += (FInt)(Inp.Mse.ScrollWheelValue - Inp.OldMse.ScrollWheelValue) / (FInt)120 * (FInt).1d * map.Cam.Zoom;

			map.Cam.refreshCorners();

			// get cursor world coordinates
			VectorF cursorPos = map.Cam.screenToWorld(new Vector2((float)Inp.Mse.X, (float)Inp.Mse.Y));

			/*/ find path
			if (!tempBool)
			{
				Node fromNode = cPlayer.Nodes[cPlayer.Nodes.Count - 1];
				VectorF fromPos = fromNode.Pos;

				// find path
				VectorF nde = fromPos / cPlayer.Path.NodeSpacing;
				nde.X = (FInt)Math.Round((double)nde.X);
				nde.Y = (FInt)Math.Round((double)nde.Y);
				int srcNode = (int)nde.Y * cPlayer.Path.NumCols + (int)nde.X;

				nde = (hPlayer.Nodes[0].Pos + hPlayer.Nodes[1].Pos) / FInt.F2 / cPlayer.Path.NodeSpacing;
				nde.X = (FInt)Math.Round((double)nde.X);
				nde.Y = (FInt)Math.Round((double)nde.Y);
				int destNode = (int)nde.Y * cPlayer.Path.NumCols + (int)nde.X;

				shortest = cPlayer.Path.search(cPlayer.Path.Nodes[srcNode], cPlayer.Path.Nodes[destNode]);

				tempBool = true;
			}*/

			if (Inp.OldMse.Position != Inp.Mse.Position)
				desktop.mouseMove(Inp.Mse.Position);

			GUI.Desktop.Event evnt = Desktop.Event.MouseRightUp;
			bool evntHappened = true;
			if (Inp.OldMse.LeftButton == ButtonState.Pressed && Inp.Mse.LeftButton == ButtonState.Released)
				evnt = Desktop.Event.MouseLeftUp;
			else if (Inp.OldMse.LeftButton == ButtonState.Released && Inp.Mse.LeftButton == ButtonState.Pressed)
				evnt = Desktop.Event.MouseLeftDown;
			else if (Inp.OldMse.RightButton == ButtonState.Pressed && Inp.Mse.RightButton == ButtonState.Released)
				evnt = Desktop.Event.MouseRightUp;
			else if (Inp.OldMse.RightButton == ButtonState.Released && Inp.Mse.RightButton == ButtonState.Pressed)
				evnt = Desktop.Event.MouseRightUp;
			else
				evntHappened = false;

			if (selectedNode != null)
				hoveredHotspot = map.HotspotAtPoint(cursorPos);

			if (selectedNode == null && evntHappened && desktop.PerformMouseEvent(evnt, Inp.Mse.Position, Inp))
			{
				hoveredNode = null;
			}
			else
			{
				// check for hovered node and segment
				hoveredNode = map.NodeAtPoint(cursorPos, true);
				hoveredSeg = (hoveredNode == null) ? map.segmentAtPoint(cursorPos, hPlayer) : null;

				// if node is selected, determine which node type may be built at the selected distance
				selNodeType = (selectedNode == null) ? null : map.getNodeType(selectedNode.Pos, cursorPos);

				// if the user just released the left mouse button
				if (Inp.OldMse.LeftButton == ButtonState.Pressed && Inp.Mse.LeftButton == ButtonState.Released)
				{
					if (selectedNode != null)
					{
						if (hoveredNode == null)
						{
							if (hoveredHotspot != null)
								map.PlayerActions.Add(new PlayerAction(hPlayer, PlayerAction.ActionType.BuildSeg, selectedNode.ID, true, hoveredHotspot.ID));
							else if (selNodeType != null)
								map.PlayerActions.Add(new PlayerAction(hPlayer, PlayerAction.ActionType.BuildSeg, selectedNode.ID, false, cursorPos));
						}
						else if (hoveredNode != selectedNode)
						{
							map.PlayerActions.Add(new PlayerAction(hPlayer, PlayerAction.ActionType.ClaimNode, selectedNode.ID, hoveredNode.ID));
						}

						selectedNode = null;
					}
				}
				// user just pressed the left mouse button
				else if ((Inp.OldMse.LeftButton == ButtonState.Released && Inp.Mse.LeftButton == ButtonState.Pressed)
					|| (Inp.OldMse.RightButton == ButtonState.Released && Inp.Mse.RightButton == ButtonState.Pressed))
				{
					if (hoveredNode != null && hoveredNode.Owner == hPlayer && (Inp.Mse.RightButton == ButtonState.Pressed || hoveredNode.NumSegments < hoveredNode.Segments.Length))
						selectedNode = hoveredNode;
				}
				// if the left mouse button is still pressed
				else if (Inp.OldMse.LeftButton == ButtonState.Pressed && Inp.Mse.LeftButton == ButtonState.Pressed)
				{
					if (selectedNode != null && cursorPos.Y >= FInt.F0 && cursorPos.X >= FInt.F0 && cursorPos.X < map.Width && cursorPos.Y < map.Height)
					{
						List<SegmentSkel> ignoreSeg = new List<SegmentSkel>(selectedNode.Segments.Length);
						foreach (Segment seg in selectedNode.Segments)
						{
							if (seg != null)
								ignoreSeg.Add(seg);
						}

						List<NodeSkel> ignoreNode = new List<NodeSkel>(1) { selectedNode };

						SegmentSkel segColl;
						NodeSkel nodeColl;
						GeoSkel geoColl;

						map.Collision.segCollisionIntPoint(selectedNode.Pos, cursorPos, ignoreSeg, ignoreNode, out segColl, out nodeColl, out geoColl, out intersectPoint);
					}
				}
				// if the player just released the right mouse button
				else if (Inp.OldMse.RightButton == ButtonState.Pressed && Inp.Mse.RightButton == ButtonState.Released)
				{
					if (selectedNode != null && hoveredNode == selectedNode)
					{
						map.PlayerActions.Add(new PlayerAction(hPlayer, PlayerAction.ActionType.DestroyNode, selectedNode.ID));
					}
					else if (hoveredSeg != null)
					{
						map.PlayerActions.Add(new PlayerAction(hPlayer, PlayerAction.ActionType.SplitSeg, hoveredSeg.ID, cursorPos));
						hoveredSeg = null;
					}

					selectedNode = null;
				}

				// if the player just pressed the spacebar
				else if (!Inp.OldKey.IsKeyDown(Keys.Space) && Inp.Key.IsKeyDown(Keys.Space))
				{
					paused = !paused;
				}
			}

			// update world
			if (!paused && !cntMsgBox.Visible)
				map.update(gameTime);

			// execute action queue
			if (map.FrontEndActions.Count > 0 && !cntMsgBox.Visible)
			{
				FrontEndAction action = map.FrontEndActions.Dequeue();

				switch (action.Action)
				{
					case FrontEndAction.ActionType.LoadMap:
						loadWorld((string)action.Params[0]);
						break;
					case FrontEndAction.ActionType.Message:
						showMsg((string)action.Params[0]);
						break;
				}
			}
		}

		/// <summary>Draws this mode.</summary>
		/// <param name="gameTime">The current game time.</param>
		public override void Draw(GameTime gameTime)
		{
			bool compView = false;
			bool drawFogOfWar = false;

			VectorF cursorPos = map.Cam.screenToWorld(new Vector2(Inp.Mse.X, Inp.Mse.Y));
			Player hPlayer = null;
			Player cPlayer = null;

			foreach (Player p in map.Players)
			{
				if (p.Type == Player.PlayerType.Human)
				{
					hPlayer = p;
					break;
				}
			}

			foreach (Player p in map.Players)
			{
				if (p.Type == Player.PlayerType.Computer)
				{
					cPlayer = p;
					break;
				}
			}

			Graphics.GraphicsDevice.Clear(Color.Black);

			Matrix scaleMatrix = Matrix.CreateScale((float)map.Cam.Zoom);
			Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, scaleMatrix);

			// draw background
			Batch.Draw(tSegment, map.Cam.worldToScreenDraw(map.Size / FInt.F2), null, Color.DarkBlue, 0f, oSegment, (Vector2)map.Size, SpriteEffects.None, 0f);

			// draw gridsquares
			for (int i = 1; i < map.Grid.NumCols; i++)
			{
				Batch.Draw(tSegment, map.Cam.worldToScreenDraw(new VectorF((FInt)i * map.Grid.SqrWidth, map.Height / FInt.F2)), null, Color.Black, 0f, oSegment, new Vector2(1f, (float)map.Height), SpriteEffects.None, 0f);
			}

			for (int i = 1; i < map.Grid.NumRows; i++)
			{
				Batch.Draw(tSegment, map.Cam.worldToScreenDraw(new VectorF(map.Width / FInt.F2, (FInt)i * map.Grid.SqrHeight)), null, Color.Black, 0f, oSegment, new Vector2((float)map.Width, 1f), SpriteEffects.None, 0f);
			}

			// draw node spacing
			if (selectedNode != null)
			{
				Color spcColor = new Color(Color.White, .25f);

				foreach (Node node in hPlayer.Nodes)
				{
					Batch.Draw(tSpacing, map.Cam.worldToScreenDraw(node.Pos), null, spcColor, 0f, oSpacing, (float)(node.Spacing + node.Radius) * 2f / (float)tSpacing.Width, SpriteEffects.None, 0f);
				}

				if (selNodeType != null)
					Batch.Draw(tSpacing, map.Cam.worldToScreenDraw(cursorPos), null, spcColor, 0f, oSpacing, (float)(selNodeType.Spacing + selNodeType.Radius) * 2f / (float)tSpacing.Width, SpriteEffects.None, 0f);

				// draw node type thresholds
				Vector2 nPos = map.Cam.worldToScreenDraw(selectedNode.Pos);
				spcColor = new Color(Color.Black, .25f);
				foreach (NodeType type in map.NodeTypes)
				{
					if (type.BuildRangeMin > 0)
						Batch.Draw(tSpacing, nPos, null, spcColor, 0f, oSpacing, (float)type.BuildRangeMin * 2f / (float)tSpacing.Width, SpriteEffects.None, 0f);
				}
			}

			// draw hotspots
			foreach (Hotspot hotspot in map.Hotspots)
			{
				Color hsCol = hotspot == hoveredHotspot ? Color.White : Color.Yellow;
				Batch.Draw(tHotspot, map.Cam.worldToScreenDraw(hotspot.Pos), null, hsCol, 0f, oHotspot, 1f, SpriteEffects.None, 0f);
			}

			// draw segments
			foreach (Segment seg in map.Segments)
			{
				if ((!compView && !seg.Visible) || (compView && !cPlayer.AIThreadBase.getSegVisibility(seg.ID)))
					continue;

				Color segColor = (seg == hoveredSeg) ? Color.Green : (seg.IsRetracting() ? Color.DarkCyan : Color.YellowGreen);
				if (seg.Owner != hPlayer)
					segColor = (seg.IsRetracting() ? Color.Pink : Color.Red);

				Vector2 segPos = map.Cam.worldToScreenDraw(seg.EndLoc[0] + (seg.Direction * seg.CurLength / FInt.F2));
				Batch.Draw(tSegment, segPos, null, segColor, (float)seg.Angle, oSegment, new Vector2(6f, (float)seg.CurLength), SpriteEffects.None, 0f);

				// draw people
				VectorF oppDir = seg.Direction * FInt.FN1;
				for (int i = 0; i < 2; i++)
				{
					VectorF dir = (i == 0) ? seg.Direction : oppDir;
					FInt minVal = seg.Length - seg.EndLength[1 - i];
					foreach (FInt person in seg.People[i])
					{
						if (person >= minVal)
							Batch.Draw(tPerson, map.Cam.worldToScreenDraw((seg.Nodes[i].Pos + (dir * person)) + ((i == 0 ? FInt.FN1 : FInt.F1) * seg.DirectionPerp * FInt.F5)), null, Color.White, 0f, oPerson, 1f, SpriteEffects.None, 0f);
					}
				}
			}

#if DEBUG
			StringBuilder debugText = new StringBuilder();
#endif
			// draw nodes
			foreach (Node node in map.Nodes)
			{
				if ((!compView && (!node.Visible || (node.Owner != hPlayer && !node.Active)))
					|| (compView && (!cPlayer.AIThreadBase.getNodeVisibility(node.ID) || (node.Owner != cPlayer && !node.Active))))
					continue;

				Color nodeCol = Color.YellowGreen;
				if (node.Owner == hPlayer)
				{
					if (node == selectedNode)
						nodeCol = Color.Green;
					else if (node == hoveredNode)
						nodeCol = Color.White;
					else if (!node.Active)
						nodeCol = Color.Gray;
				}
				else
				{
					nodeCol = Color.Red;
				}

				Batch.Draw(tNode, map.Cam.worldToScreenDraw(node.Pos), null, nodeCol, 0f, oNode, (float)node.Radius * 2f / (float)tNode.Width, SpriteEffects.None, 0f);
			}

			// draw intersection
			/*if (intersectPoint != null && selectedNode != null && hoveredNode == null)
			{
				Batch.Draw(tPerson, map.Cam.worldToScreenDraw(intersectPoint.Value), null, Color.Red, 0f, oPerson, 1f, SpriteEffects.None, 0f);
			}*/

			Batch.End();

			// draw geos
			List<VertexPositionTexture> gvs = new List<VertexPositionTexture>();
			foreach (Geo geo in map.Geos)
			{
				if (geo.Display)
					gvs.AddRange(geo.getTriangles(map.Cam));
			}

			if (gvs.Count > 0)
			{
				VertexPositionTexture[] gVertices = new VertexPositionTexture[gvs.Count];
				gvs.CopyTo(gVertices);

				Graphics.GraphicsDevice.BlendState = BlendState.Opaque;
				Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
				Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
				Graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

				BEffect.View = scaleMatrix;
				BEffect.VertexColorEnabled = false;
				BEffect.TextureEnabled = true;
				BEffect.Texture = tGeo;
				BEffect.CurrentTechnique.Passes[0].Apply();
				Graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, gVertices, 0, gVertices.Length / 3);
			}

			Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicWrap, DepthStencilState.None, RasterizerState.CullNone, null, scaleMatrix);

			// draw fog of war
			if (drawFogOfWar)
			{
				if (compView)
					lock (cPlayer.Fog) { cPlayer.Fog.draw(Batch, tSegment); }
				else
					hPlayer.Fog.draw(Batch, tSegment);
			}

			// draw user interaction
			if (selectedNode != null)
			{
				VectorF segPos = (selectedNode.Pos + cursorPos) / FInt.F2;
				float segAngle = (float)Calc.FindAngle(cursorPos, selectedNode.Pos);//(float)((Calc.PI / FInt.F2) + Calc.Atan((cursorPos.Y - selectedNode.Y) / (cursorPos.X - selectedNode.X)));
				FInt segLength = VectorF.Distance(selectedNode.Pos, cursorPos);
				//FInt segLenApprox = VectorF.DistanceApprox(selectedNode.Pos, cursorPos);
				//if (Calc.Abs(segLength - segLenApprox) > segLength && Calc.Abs(segLength - segLenApprox) > maxApproxErr)
					//maxApproxErr = Calc.Abs(segLength - segLenApprox);*/

				Batch.Draw(tSegment, map.Cam.worldToScreenDraw(segPos), null, Color.Green, segAngle, oSegment, new Vector2(6f, (float)segLength), SpriteEffects.None, 0f);

				if (selNodeType != null)
					Batch.Draw(tNode, map.Cam.worldToScreenDraw(cursorPos), null, Color.White, 0f, oNode, (float)selNodeType.Radius * 2f / (float)tNode.Width, SpriteEffects.None, 0f);
			}

			Batch.End();

#if DEBUG

			// draw pathing info
			if (false)
			{
				lock (cPlayer.AIThreadBase.Path)
				{
					List<VertexPositionColor> pvs = new List<VertexPositionColor>();

					PathFinder path = cPlayer.AIThreadBase.Path;
					int left = Math.Max(0, (int)((double)map.Cam.Left / path.NodeSpacing.X));
					int right = Math.Min(path.NumCols - 1, (int)((double)map.Cam.Right / path.NodeSpacing.X));
					int top = Math.Max(0, (int)((double)map.Cam.Top / path.NodeSpacing.Y));
					int bottom = Math.Min(path.NumRows - 1, (int)((double)map.Cam.Bottom / path.NodeSpacing.Y));

					for (int row = top; row <= bottom; row++)
					{
						for (int col = left; col <= right; col++)
						{
							if (col < path.NumCols - 1)
							{
								VectorF pos = (VectorF)path.Grid[row, col].Position;
								for (PathEdge edge = path.Grid[row, col].FirstEdge; edge != null; edge = edge.Next)
								{
									Color ccc = Color.White;
									if (edge.NumIntersections > 0)
										ccc = Color.Gray;
									pvs.Add(new VertexPositionColor(new Vector3(map.Cam.worldToScreenDraw(pos), 0f), ccc));
									pvs.Add(new VertexPositionColor(new Vector3(map.Cam.worldToScreenDraw((VectorF)path.Nodes[edge.NodeDest].Position), 0f), ccc));
								}
							}
						}
					}

					// draw lines
					if (pvs.Count > 0)
					{
						VertexPositionColor[] gVertices = pvs.ToArray();

						BEffect.View = scaleMatrix;
						BEffect.VertexColorEnabled = true;
						BEffect.TextureEnabled = false;
						BEffect.CurrentTechnique.Passes[0].Apply();
						Graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, gVertices, 0, gVertices.Length / 2);
					}
				}
			}

			// debug text
			Batch.Begin();

			/*foreach (SegmentSkel seg in cPlayer.AIThreadBase.Grid.Squares[0, 0].Segments)
			{
				Vector2 segPos = map.Cam.worldToScreen((seg.EndLoc[0] + seg.EndLoc[1]) / FInt.F2);
				debugText.Clear();
				debugText.AppendLine("ID: " + seg.ID);
				debugText.AppendLine("Length: " + seg.CurLength.ToString());
				debugText.AppendLine("End[0]: " + seg.EndLoc[0].ToString());
				debugText.AppendLine("End[1]: " + seg.EndLoc[1].ToString());
				//Batch.DrawString(fDefault, debugText, segPos, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}

			foreach (NodeSkel node in cPlayer.AIThreadBase.Grid.Squares[0,0].Nodes)
			{
				// draw node debug text
				debugText.Clear();
				debugText.AppendLine("ID: " + node.ID);
				//debugText.AppendLine("Type: " + node.NType.Name);
				//debugText.AppendLine("GenCountDown: " + node.GenCountDown);
				//debugText.AppendLine("GenSpacing: " + node.GenSpacing);
				//debugText.AppendLine("IsParent: " + node.IsParent);
				//debugText.AppendLine("LastCheck: " + node.LastCheck);
				//debugText.AppendLine("LastCheckNum: " + node.LastCheckNum);
				//debugText.AppendLine("Owner: " + ((node.Owner == null) ? "NULL" : node.Owner.Name));
				//debugText.AppendLine("Parents: " + node.Parents.Count);
				//debugText.AppendLine("PeopleToSort: " + node.PeopleToSort);
				//debugText.AppendLine("Radius: " + node.Radius);
				//debugText.AppendLine("X: " + node.X);
				//debugText.AppendLine("Y: " + node.Y);
				//debugText.AppendLine("Seg Cap: " + node.Segments.Length);

				/*for (int i = 0; i < node.Segments.Length; i++)
				{
					if (node.Segments[i] == null)
					{
						debugText.AppendLine("===Segment " + i + " == NULL");
					}
					else
					{
						debugText.AppendLine("===Segment " + i + "===");
						debugText.AppendLine("Num People: " + node.SegNumPeople[i]);
						debugText.AppendLine("Total Capacity: " + node.SegCapacity[i]);
						debugText.AppendLine("Seg Cur Cap: " + node.Segments[i].CurCapacity);
					}
				}/

				Vector2 textLoc = map.Cam.worldToScreen(node.Pos);
				textLoc.X = (float)(int)textLoc.X;
				textLoc.Y = (float)(int)textLoc.Y;
				//Batch.DrawString(fDefault, debugText, textLoc, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}*/

			/*if (selectedNode != null && selNodeType != null)
			{
				Batch.DrawString(fDefault, selNodeType.Name, map.Cam.worldToScreen(cursorPos), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}*/

			lblCursorPos.Text = "Screen: " + Inp.Mse.X.ToString().PadLeft(3, '0') + "x" + Inp.Mse.Y.ToString().PadLeft(3, '0') + "\nWorld: " + cursorPos.X.ToString().PadLeft(3, '0') + "x" + cursorPos.Y.ToString().PadLeft(3, '0');
			//Batch.DrawString(fDefault, "Screen: " + Inp.Mse.X.ToString().PadLeft(3, '0') + "x" + Inp.Mse.Y.ToString().PadLeft(3, '0') + "\nWorld: " + cursorPos.X.ToString().PadLeft(3, '0') + "x" + cursorPos.Y.ToString().PadLeft(3, '0'), Vector2.Zero, Color.White);

			string frameRate = "FPS: " + (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString();
			Vector2 frSize = fDefault.MeasureString(frameRate);
			Batch.DrawString(fDefault, frameRate, new Vector2(Graphics.PreferredBackBufferWidth - frSize.X, 0f), Color.White);

			/*Vector2 nde = cursorPos / cPlayer.Path.NodeSpacing;
			nde.X = Calc.Round(nde.X);
			nde.Y = Calc.Round(nde.Y);
			Batch.DrawString(fDefault, tempCount.ToString() + " : " + ((int)nde.Y * cPlayer.Path.NumCols + (int)nde.X).ToString() + " : " + closestPathNode.ToString(), new Vector2(0f, Graphics.PreferredBackBufferHeight - frSize.Y), Color.White);*/
			

			Batch.End();

			// draw gui
			Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, desktop.RState);
			desktop.Draw(Batch);
			Batch.End();
#endif
		}

		#region Events

		#region MsgBox

		/// <summary>Called when btnMsgBoxExit is clicked.</summary>
		private void btnMsgBoxExit_MouseLeftUp(object sender, EventArgs e)
		{
			cntMsgBox.Visible = false;
		}

		#endregion MsgBox

		#endregion Events

		#endregion Methods
	}
}
