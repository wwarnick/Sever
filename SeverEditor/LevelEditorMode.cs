using GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sever;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SeverEditor
{
	class LevelEditorMode : Mode
	{
		#region Members

		/// <summary>Contains the previous value of the currently focused TextBox.  If the contents of the textbox are invalid after it loses focus, the value is reverted to this value.</summary>
		private string curTxtVal;

		/// <summary>The current world being edited.</summary>
		private World world;

		/// <summary>The node that the cursor is currently hovering over.</summary>
		private Node hoveredNode;

		/// <summary>The currently selected node.</summary>
		private Node selectedNode;

		/// <summary>The segment that the cursor is currently hovering over.</summary>
		private Segment hoveredSeg;

		/// <summary>The currently selected segment.</summary>
		private Segment selectedSeg;

		/// <summary>The segment end that the cursor is currently hovering over.</summary>
		private Node hoveredSegEnd;

		/// <summary>The owner of the segment end that the cursor is currently hovering over.</summary>
		private Player hoveredSegEndOwner;

		/// <summary>The currently selected segment end.</summary>
		private Node selectedSegEnd;

		/// <summary>The geo that the cursor is currently hovering over.</summary>
		private Geo hoveredGeo;

		/// <summary>The geo vertex that the cursor is currently hovering over.</summary>
		private int hoveredGeoVertex;

		/// <summary>Whether or not the hovered geo vertex is just the first vertex of a line.</summary>
		private bool hoveredGeoIsLine;

		/// <summary>The currently selected geo.</summary>
		private Geo selectedGeo;

		/// <summary>The currently selected geo vertex.</summary>
		private int selectedGeoVertex;

		/// <summary>Whether or not the selected geo vertex is just the first vertex of a line.</summary>
		private bool selectedGeoIsLine;

		/// <summary>The hotspot that the cursor is currently hovering over.</summary>
		private Hotspot hoveredHotspot;

		/// <summary>The currently selected hotspot.</summary>
		private Hotspot selectedHotspot;

		/// <summary>The distance that the cursor has to move in order to begin a drag.</summary>
		private const float distToDrag = 15f;

		/// <summary>The point at which the cursor was last pressed.</summary>
		private VectorF lastClickedPoint;

		/// <summary>The cursor position offset to use when dragging.</summary>
		private VectorF dragOffset;

		/// <summary>Whether or not the cursor is dragging something.</summary>
		private bool isDragging;

		#region GUI

		/// <summary>This mode's desktop object.</summary>
		private Desktop desktop;

		/// <summary>Contains all of the editor controls.</summary>
		private Container cntSideBar;
		
		#region Tabs

		private ButtonText btnShowSettingsPnl;
		private ButtonText btnShowScriptsPnl;
		private ButtonText btnShowNodeTypesPnl;
		private ButtonText btnShowPlayersPnl;
		private ButtonText btnShowEditorPnl;

		#endregion Tabs
		#region pnlSettings

		private Container pnlSettings;

		private Label lblWorldWidth;
		private TextBox txtWorldWidth;
		private Label lblWorldHeight;
		private TextBox txtWorldHeight;

		private Control ctlPersonDivider;

		private Label lblPersonSpacing;
		private TextBox txtPersonSpacing;
		private Label lblPersonSpeedLower;
		private TextBox txtPersonSpeedLower;
		private Label lblPersonSpeedUpper;
		private TextBox txtPersonSpeedUpper;

		private Control ctlSegmentDivider;

		private Label lblRetractSpeed;
		private TextBox txtRetractSpeed;
		private Label lblBuildRate;
		private TextBox txtBuildRate;

		private Control ctlCamDivider;

		private Label lblCamWidth;
		private TextBox txtCamWidth;
		private Label lblCamHeight;
		private TextBox txtCamHeight;
		private Label lblCamX;
		private TextBox txtCamX;
		private Label lblCamY;
		private TextBox txtCamY;

		private Control ctlGridDivider;

		private Label lblGridRows;
		private TextBox txtGridRows;
		private Label lblGridCols;
		private TextBox txtGridCols;

		private Control ctlFogDivider;

		private Label lblFogRows;
		private TextBox txtFogRows;
		private Label lblFogCols;
		private TextBox txtFogCols;

		private Control ctlPathDivider;

		private Label lblPathRows;
		private TextBox txtPathRows;
		private Label lblPathCols;
		private TextBox txtPathCols;

		#endregion pnlSettings
		#region pnlScripts

		Container pnlScripts;

		private Label lblScriptsBeginUpdateScript;
		private Label lblScriptsBeginUpdateScriptView;
		private ButtonText btnScriptsBeginUpdateScript;

		#endregion pnlScripts
		#region pnlNodeTypes

		private Container pnlNodeTypes;
		private ComboBox cmbNodeTypes;
		private ButtonText btnNodeTypeLoad;
		private ButtonText btnNodeTypeUpdate;
		private ButtonText btnNodeTypeAddNew;
		private ButtonText btnNodeTypeDelete;
		private Label lblNodeTypeName;
		private TextBox txtNodeTypeName;
		private Label lblNodeTypeIsParent;
		private Button btnNodeTypeIsParent;
		private Label lblNodeTypeNumSegs;
		private TextBox txtNodeTypeNumSegs;
		private Label lblNodeTypeRadius;
		private TextBox txtNodeTypeRadius;
		private Label lblNodeTypeSpacing;
		private TextBox txtNodeTypeSpacing;
		private Label lblNodeTypeGenSpacing;
		private TextBox txtNodeTypeGenSpacing;
		private Label lblNodeTypeSightDistance;
		private TextBox txtNodeTypeSightDistance;
		private Label lblNodeTypeBuildRangeMin;
		private TextBox txtNodeTypeBuildRangeMin;

		#endregion pnlNodeTypes
		#region pnlPlayers

		private Container pnlPlayers;
		private ComboBox cmbPlayers;
		private ButtonText btnPlayerLoad;
		private ButtonText btnPlayerUpdate;
		private ButtonText btnPlayerAddNew;
		private ButtonText btnPlayerDelete;
		private Label lblPlayerID;
		private TextBox txtPlayerID;
		private Label lblPlayerName;
		private TextBox txtPlayerName;
		private Label lblPlayerTypes;
		private ComboBox cmbPlayerTypes;

		#endregion pnlPlayers
		#region pnlEditor

		private Container pnlEditor;
		private ButtonText btnAddNode;
		private ButtonText btnAddSeg;
		private ButtonText btnAddGeo;
		private ButtonText btnAddHotspot;

		private Container pnlEditorAddNodes;
		private Label lblEditorAddNodeOwner;
		private ComboBox cmbEditorAddNodeOwner;
		private Label lblEditorAddNodeType;
		private ComboBox cmbEditorAddNodeType;

		private Container pnlEditorAddSegs;
		private Label lblEditorAddSegOwner;
		private ComboBox cmbEditorAddSegOwner;

		private Container pnlEditorAddGeos;

		private Container pnlEditorAddHotspots;

		private Control ctlEditorDivider;

		private Container pnlEditorNodes;
		private Label lblEditorNodeOwner;
		private ComboBox cmbEditorNodeOwner;
		private Label lblEditorNodeType;
		private ComboBox cmbEditorNodeType;
		private ButtonText btnEditorNodeLoadType;
		private Label lblEditorNodeID;
		private TextBox txtEditorNodeID;
		private Label lblEditorNodeIsParent;
		private Button btnEditorNodeIsParent;
		private Label lblEditorNodeNumSegs;
		private TextBox txtEditorNodeNumSegs;
		private Label lblEditorNodeRadius;
		private TextBox txtEditorNodeRadius;
		private Label lblEditorNodeSpacing;
		private TextBox txtEditorNodeSpacing;
		private Label lblEditorNodeGenSpacing;
		private TextBox txtEditorNodeGenSpacing;
		private Label lblEditorNodeGenCountDown;
		private TextBox txtEditorNodeGenCountDown;
		private Label lblEditorNodeSightDist;
		private TextBox txtEditorNodeSightDist;
		private Label lblEditorNodeX;
		private TextBox txtEditorNodeX;
		private Label lblEditorNodeY;
		private TextBox txtEditorNodeY;
		private Label lblEditorNodeOwnsHotspot;
		private TextBox txtEditorNodeOwnsHotspot;
		private Label lblEditorNodeActive;
		private Button btnEditorNodeActive;
		private ButtonText btnEditorNodeApply;
		private ButtonText btnEditorNodeDel;

		private Container pnlEditorSegs;
		private Label lblEditorSegOwner;
		private ComboBox cmbEditorSegOwner;
		private Label lblEditorSegLenLbl;
		private Label lblEditorSegLenVal;
		private Label lblEditorSegEndLen0;
		private TextBox txtEditorSegEndLen0;
		private Label lblEditorSegEndLen1;
		private TextBox txtEditorSegEndLen1;
		private Label lblEditorSegEndState0;
		private ComboBox cmbEditorSegEndState0;
		private Label lblEditorSegEndState1;
		private ComboBox cmbEditorSegEndState1;
		private Label lblEditorSegLanePeople0;
		private TextBox txtEditorSegLanePeople0;
		private Label lblEditorSegLanePeople1;
		private TextBox txtEditorSegLanePeople1;
		private ButtonText btnEditorSegApply;
		private ButtonText btnEditorSegDel;

		private Container pnlEditorGeos;
		private Label lblEditorGeoCloseLoop;
		private Button btnEditorGeoCloseLoop;
		private Label lblEditorGeoDisplay;
		private Button btnEditorGeoDisplay;
		private ButtonText btnEditorGeoApply;
		private ButtonText btnEditorGeoDel;

		private Container pnlEditorHotspots;
		private Label lblEditorHotspotIDLbl;
		private Label lblEditorHotspotIDVal;
		private Label lblEditorHotspotX;
		private TextBox txtEditorHotspotX;
		private Label lblEditorHotspotY;
		private TextBox txtEditorHotspotY;
		private Label lblEditorHotspotScript;
		private Label lblEditorHotspotScriptVal;
		private Label lblEditorHotspotScriptView;
		private ButtonText btnEditorHotspotScript;
		private ButtonText btnEditorHotspotApply;
		private ButtonText btnEditorHotspotDel;

		#endregion pnlEditor
		#region Load and Save

		private ButtonText btnLoadWorld;
		private ButtonText btnSaveWorld;

		#endregion Load and Save
		#region cntMsgBox

		private Container cntMsgBox;
		private Label lblMsgBoxMessage;
		private ButtonText btnMsgBoxExit;

		#endregion cntMsgBox
		#region cntSaveLoad

		private Container cntSaveLoad;
		private Label lblSaveLoadTitle;
		private Label lblSaveLoadPath;
		private List<ButtonText> btnSaveLoadPath;
		private ListBoxText lsbSaveLoadFiles;
		private Label lblSaveLoadFileName;
		private TextBox txtSaveLoadFileName;
		private ButtonText btnSaveLoadConfirm;
		private ButtonText btnSaveLoadCancel;

		#endregion cntSaveLoad
		#region cntTextEditor

		private Container cntTextEditor;
		private Label lblTextEditorTitle;
		private TextBoxMulti txtTextEditorText;
		private ButtonText btnTextEditorOK;
		private ButtonText btnTextEditorCancel;

		#endregion cntTextEditor

		/// <summary> Displays the current position of the cursor.</summary>
		private Label lblCursorPos;

		#endregion

		#region Content

		/// <summary>The default font.</summary>
		private SpriteFont fSegoe;

		/// <summary>The default font - Bold.</summary>
		private SpriteFont fSegoeBold;

		/// <summary>Font - Courier New</summary>
		private SpriteFont fCourierNew;

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

		/// <summary>Creates a new instance of LevelEditorMode.</summary>
		/// <param name="graphics">The graphics device manager to use.</param>
		/// <param name="content">The content manager to use.</param>
		/// <param name="batch">The sprite batch to use.</param>
		/// <param name="bEffect">The basic effect to use.</param>
		public LevelEditorMode(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch batch, BasicEffect bEffect)
			: base(graphics, content, batch, bEffect)
		{
			world = new World(true);
			world.Grid = new GridManager(1, 1, world);
			hoveredNode = null;
			selectedNode = null;
			hoveredSeg = null;
			selectedSeg = null;
			hoveredSegEnd = null;
			hoveredSegEndOwner = null;
			selectedSegEnd = null;
			hoveredGeo = null;
			hoveredGeoVertex = -1;
			hoveredGeoIsLine = false;
			selectedGeo = null;
			selectedGeoVertex = -1;
			selectedGeoIsLine = false;
			desktop = new Desktop();
			lastClickedPoint = new VectorF();
			dragOffset = new VectorF();
			isDragging = false;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Builds the GUI.</summary>
		/// <param name="graphics">The graphics device manager to use.</param>
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
			Desktop.DefButtonTextFont = fSegoe;
			Desktop.DefButtonTextForeColor = Color.Black;
			Desktop.DefButtonTextForeColorHover = Color.Black;
			Desktop.DefButtonTextForeColorPressed = Color.Gray;
			Desktop.DefButtonTextForeColorP = Color.Gray;
			Desktop.DefButtonTextForeColorHoverP = Color.White;
			Desktop.DefComboBoxTextBoxBackColor = Color.White;
			Desktop.DefComboBoxTextBoxForeColor = Color.Black;
			Desktop.DefComboBoxSidePadding = 5;
			Desktop.DefComboBoxButtonWidth = 13;
			Desktop.DefLabelFont = fSegoe;
			Desktop.DefLabelForeColor = Color.White;
			Desktop.DefPopUpMenuFont = fSegoe;
			Desktop.DefPopUpMenuBackColor = Color.LightGray;
			Desktop.DefPopUpMenuBackColorHover = Color.Blue;
			Desktop.DefPopUpMenuForeColor = Color.Black;
			Desktop.DefPopUpMenuForeColorHover = Color.White;
			Desktop.DefPopUpMenuSidePadding = 5;
			Desktop.DefTextBoxFont = fSegoe;
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
			Desktop.DefListBoxTextFont = fSegoe;
			Desktop.DefListBoxTextForeColor = Color.Black;
			Desktop.DefListBoxTextForeColorHover = Color.Black;
			Desktop.DefListBoxTextForeColorSelected = Color.White;
			desktop.TBack = tSegment;

			#endregion Defaults
			#region cntSideBar

			// cntSideBar
			cntSideBar = new Container();
			cntSideBar.Width = 150;
			cntSideBar.Height = graphics.PreferredBackBufferHeight;
			cntSideBar.Left = graphics.PreferredBackBufferWidth - cntSideBar.Width;

			// btnShowSettingsPnl
			btnShowSettingsPnl = new ButtonText();
			btnShowSettingsPnl.Bounds = new Rectangle(10, 10, cntSideBar.Width - 20, 20);
			btnShowSettingsPnl.IsToggle = true;
			btnShowSettingsPnl.Text = "Settings";
			btnShowSettingsPnl.Font = fSegoeBold;
			btnShowSettingsPnl.MouseLeftUp += new EventHandler(btnShowWorldPnl_MouseLeftUp);
			cntSideBar.Controls.Add(btnShowSettingsPnl);

			// btnShowScriptsPnl
			btnShowScriptsPnl = new ButtonText(btnShowSettingsPnl);
			btnShowScriptsPnl.Top += 20;
			btnShowScriptsPnl.Text = "Scripts";
			btnShowScriptsPnl.MouseLeftUp += new EventHandler(btnShowScriptsPnl_MouseLeftUp);
			cntSideBar.Controls.Add(btnShowScriptsPnl);

			// btnShowNodeTypesPnl
			btnShowNodeTypesPnl = new ButtonText(btnShowScriptsPnl);
			btnShowNodeTypesPnl.Top += 20;
			btnShowNodeTypesPnl.Text = "Node Types";
			btnShowNodeTypesPnl.MouseLeftUp += new EventHandler(btnShowNodeTypesPnl_MouseLeftUp);
			cntSideBar.Controls.Add(btnShowNodeTypesPnl);

			// btnShowPlayersPnl
			btnShowPlayersPnl = new ButtonText(btnShowNodeTypesPnl);
			btnShowPlayersPnl.Top += 20;
			btnShowPlayersPnl.Text = "Players";
			btnShowPlayersPnl.MouseLeftUp += new EventHandler(btnShowPlayersPnl_MouseLeftUp);
			cntSideBar.Controls.Add(btnShowPlayersPnl);

			// btnShowEditorPnl
			btnShowEditorPnl = new ButtonText(btnShowPlayersPnl);
			btnShowEditorPnl.Top += 20;
			btnShowEditorPnl.Text = "Editor";
			btnShowEditorPnl.MouseLeftUp += new EventHandler(btnShowEditorPnl_MouseLeftUp);
			cntSideBar.Controls.Add(btnShowEditorPnl);

			#endregion cntSideBar
			#region pnlSettings

			//
			// pnlSettings
			// 
			pnlSettings = new Container();
			pnlSettings.DrawBack = false;
			pnlSettings.Bounds = new Rectangle(0, btnShowEditorPnl.Bottom + 10, cntSideBar.Width, 0);

			// lblWorldWidth
			lblWorldWidth = new Label();
			lblWorldWidth.AutoSize = false;
			lblWorldWidth.Bounds = new Rectangle(0, 0, 80, 20);
			lblWorldWidth.Text = "WldWidth:";
			lblWorldWidth.TextAlign = Desktop.Alignment.CenterRight;
			pnlSettings.Controls.Add(lblWorldWidth);

			// txtWorldWidth
			txtWorldWidth = new TextBox();
			txtWorldWidth.Top = lblWorldWidth.Top;
			txtWorldWidth.Left = lblWorldWidth.Right + 5;
			txtWorldWidth.Width = pnlSettings.Width - txtWorldWidth.Left - 10;
			txtWorldWidth.Height = 20;
			txtWorldWidth.AllowAlpha = false;
			txtWorldWidth.AllowedSpecChars = ".";
			txtWorldWidth.FocusLost += new EventHandler(textBox_FocusLost);
			txtWorldWidth.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtWorldWidth);

			// lblWorldHeight
			lblWorldHeight = new Label(lblWorldWidth);
			lblWorldHeight.Top += 25;
			lblWorldHeight.Text = "WldHeight:";
			pnlSettings.Controls.Add(lblWorldHeight);

			// txtWorldHeight
			txtWorldHeight = new TextBox(txtWorldWidth);
			txtWorldHeight.Top = lblWorldHeight.Top;
			txtWorldHeight.FocusLost += new EventHandler(textBox_FocusLost);
			txtWorldHeight.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtWorldHeight);

			/********************************************************************************************/

			// ctlPersonDivider
			ctlPersonDivider = new Control();
			ctlPersonDivider.Bounds = new Rectangle(10, txtWorldHeight.Bottom + 10, pnlSettings.Width - 20, 1);
			ctlPersonDivider.BackColor = Desktop.DefButtonBackColor;
			ctlPersonDivider.Ignore = true;
			pnlSettings.Controls.Add(ctlPersonDivider);

			// lblPersonSpacing
			lblPersonSpacing = new Label(lblWorldHeight);
			lblPersonSpacing.Top = ctlPersonDivider.Bottom + 10;
			lblPersonSpacing.Text = "PSpacing:";
			pnlSettings.Controls.Add(lblPersonSpacing);

			// txtPersonSpacing
			txtPersonSpacing = new TextBox(txtWorldHeight);
			txtPersonSpacing.Top = lblPersonSpacing.Top;
			txtPersonSpacing.FocusLost += new EventHandler(textBox_FocusLost);
			txtPersonSpacing.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtPersonSpacing);

			// lblPersonSpeedLower
			lblPersonSpeedLower = new Label(lblPersonSpacing);
			lblPersonSpeedLower.Top += 25;
			lblPersonSpeedLower.Text = "PSpeed-:";
			pnlSettings.Controls.Add(lblPersonSpeedLower);

			// txtPersonSpeedLower
			txtPersonSpeedLower = new TextBox(txtPersonSpacing);
			txtPersonSpeedLower.Top = lblPersonSpeedLower.Top;
			txtPersonSpeedLower.FocusLost += new EventHandler(textBox_FocusLost);
			txtPersonSpeedLower.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtPersonSpeedLower);

			// lblPersonSpeedUpper
			lblPersonSpeedUpper = new Label(lblPersonSpeedLower);
			lblPersonSpeedUpper.Top += 25;
			lblPersonSpeedUpper.Text = "PSpeed+:";
			pnlSettings.Controls.Add(lblPersonSpeedUpper);

			// txtPersonSpeedUpper
			txtPersonSpeedUpper = new TextBox(txtPersonSpeedLower);
			txtPersonSpeedUpper.Top = lblPersonSpeedUpper.Top;
			txtPersonSpeedUpper.FocusLost += new EventHandler(textBox_FocusLost);
			txtPersonSpeedUpper.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtPersonSpeedUpper);

			/********************************************************************************************/

			// ctlSegmentDivider
			ctlSegmentDivider = new Control(ctlPersonDivider);
			ctlSegmentDivider.Top = txtPersonSpeedUpper.Bottom + 10;
			pnlSettings.Controls.Add(ctlSegmentDivider);

			// lblWorldRetractSpeed
			lblRetractSpeed = new Label(lblPersonSpeedUpper);
			lblRetractSpeed.Top = ctlSegmentDivider.Bottom + 10;
			lblRetractSpeed.Text = "RSpeed:";
			pnlSettings.Controls.Add(lblRetractSpeed);

			// txtRetractSpeed
			txtRetractSpeed = new TextBox(txtPersonSpeedUpper);
			txtRetractSpeed.Top = lblRetractSpeed.Top;
			txtRetractSpeed.FocusLost += new EventHandler(textBox_FocusLost);
			txtRetractSpeed.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtRetractSpeed);

			// lblBuildRate
			lblBuildRate = new Label(lblRetractSpeed);
			lblBuildRate.Top += 25;
			lblBuildRate.Text = "BuildRt:";
			pnlSettings.Controls.Add(lblBuildRate);

			// txtBuildRate
			txtBuildRate = new TextBox(txtRetractSpeed);
			txtBuildRate.Top = lblBuildRate.Top;
			txtBuildRate.FocusLost += new EventHandler(textBox_FocusLost);
			txtBuildRate.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtBuildRate);

			/********************************************************************************************/

			// ctlCamDivider
			ctlCamDivider = new Control(ctlPersonDivider);
			ctlCamDivider.Top = txtBuildRate.Bottom + 10;
			pnlSettings.Controls.Add(ctlCamDivider);

			// lblCamWidth
			lblCamWidth = new Label(lblBuildRate);
			lblCamWidth.Top = ctlCamDivider.Bottom + 10;
			lblCamWidth.Text = "CamWidth:";
			pnlSettings.Controls.Add(lblCamWidth);

			// txtCamWidth
			txtCamWidth = new TextBox(txtBuildRate);
			txtCamWidth.Top = lblCamWidth.Top;
			txtCamWidth.FocusLost += new EventHandler(textBox_FocusLost);
			txtCamWidth.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtCamWidth);

			// lblCamHeight
			lblCamHeight = new Label(lblCamWidth);
			lblCamHeight.Top += 25;
			lblCamHeight.Text = "CamHeight:";
			pnlSettings.Controls.Add(lblCamHeight);

			// txtCamHeight
			txtCamHeight = new TextBox(txtCamWidth);
			txtCamHeight.Top = lblCamHeight.Top;
			txtCamHeight.FocusLost += new EventHandler(textBox_FocusLost);
			txtCamHeight.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtCamHeight);

			// lblCamX
			lblCamX = new Label(lblCamHeight);
			lblCamX.Top += 25;
			lblCamX.Text = "Cam X:";
			pnlSettings.Controls.Add(lblCamX);

			// txtCamX
			txtCamX = new TextBox(txtCamHeight);
			txtCamX.Top = lblCamX.Top;
			txtCamX.FocusLost += new EventHandler(textBox_FocusLost);
			txtCamX.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtCamX);

			// lblCamY
			lblCamY = new Label(lblCamX);
			lblCamY.Top += 25;
			lblCamY.Text = "Cam Y:";
			pnlSettings.Controls.Add(lblCamY);

			// txtCamY
			txtCamY = new TextBox(txtCamX);
			txtCamY.Top = lblCamY.Top;
			txtCamY.FocusLost += new EventHandler(textBox_FocusLost);
			txtCamY.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtCamY);

			/********************************************************************************************/

			// ctlGridDivider
			ctlGridDivider = new Control(ctlPersonDivider);
			ctlGridDivider.Top = txtCamY.Bottom + 10;
			pnlSettings.Controls.Add(ctlGridDivider);

			// lblGridRows
			lblGridRows = new Label(lblCamY);
			lblGridRows.Top = ctlGridDivider.Bottom + 10;
			lblGridRows.Text = "GridRows:";
			pnlSettings.Controls.Add(lblGridRows);

			// txtGridRows
			txtGridRows = new TextBox(txtCamY);
			txtGridRows.Top = lblGridRows.Top;
			txtGridRows.AllowedSpecChars = string.Empty;
			txtGridRows.FocusLost += new EventHandler(textBox_FocusLost);
			txtGridRows.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtGridRows);

			// lblGridCols
			lblGridCols = new Label(lblGridRows);
			lblGridCols.Top += 25;
			lblGridCols.Text = "GridCols:";
			pnlSettings.Controls.Add(lblGridCols);

			// txtGridCols
			txtGridCols = new TextBox(txtGridRows);
			txtGridCols.Top = lblGridCols.Top;
			txtGridCols.FocusLost += new EventHandler(textBox_FocusLost);
			txtGridCols.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtGridCols);

			/********************************************************************************************/

			// ctlFogDivider
			ctlFogDivider = new Control(ctlPersonDivider);
			ctlFogDivider.Top = txtGridCols.Bottom + 10;
			pnlSettings.Controls.Add(ctlFogDivider);

			// lblFogRows
			lblFogRows = new Label(lblGridCols);
			lblFogRows.Top = ctlFogDivider.Bottom + 10;
			lblFogRows.Text = "FogRows:";
			pnlSettings.Controls.Add(lblFogRows);

			// txtFogRows
			txtFogRows = new TextBox(txtGridCols);
			txtFogRows.Top = lblFogRows.Top;
			txtFogRows.FocusLost += new EventHandler(textBox_FocusLost);
			txtFogRows.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtFogRows);

			// lblFogCols
			lblFogCols = new Label(lblFogRows);
			lblFogCols.Top += 25;
			lblFogCols.Text = "FogCols:";
			pnlSettings.Controls.Add(lblFogCols);

			// txtFogCols
			txtFogCols = new TextBox(txtFogRows);
			txtFogCols.Top = lblFogCols.Top;
			txtFogCols.FocusLost += new EventHandler(textBox_FocusLost);
			txtFogCols.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtFogCols);

			/********************************************************************************************/

			// ctlPathDivider
			ctlPathDivider = new Control(ctlPersonDivider);
			ctlPathDivider.Top = txtFogCols.Bottom + 10;
			pnlSettings.Controls.Add(ctlPathDivider);

			// lblPathRows
			lblPathRows = new Label(lblFogCols);
			lblPathRows.Top = ctlPathDivider.Bottom + 10;
			lblPathRows.Text = "PathRows:";
			pnlSettings.Controls.Add(lblPathRows);

			// txtPathRows
			txtPathRows = new TextBox(txtFogCols);
			txtPathRows.Top = lblPathRows.Top;
			txtPathRows.FocusLost += new EventHandler(textBox_FocusLost);
			txtPathRows.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtPathRows);

			// lblPathCols
			lblPathCols = new Label(lblPathRows);
			lblPathCols.Top += 25;
			lblPathCols.Text = "PathCols:";
			pnlSettings.Controls.Add(lblPathCols);

			// txtPathCols
			txtPathCols = new TextBox(txtPathRows);
			txtPathCols.Top = lblPathCols.Top;
			txtPathCols.FocusLost += new EventHandler(textBox_FocusLost);
			txtPathCols.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlSettings.Controls.Add(txtPathCols);

			pnlSettings.AutoHeight();
			cntSideBar.Controls.Add(pnlSettings);

			#endregion pnlSettings
			#region pnlScripts

			//
			// pnlScripts
			//
			pnlScripts = new Container(pnlSettings);

			// lblScriptsBeginUpdateScript
			lblScriptsBeginUpdateScript = new Label(lblWorldWidth);
			lblScriptsBeginUpdateScript.Top = 0;
			lblScriptsBeginUpdateScript.Text = "Begin Update:";
			pnlScripts.Controls.Add(lblScriptsBeginUpdateScript);

			// btnScriptsBeginUpdateScript
			btnScriptsBeginUpdateScript = new ButtonText();
			btnScriptsBeginUpdateScript.Bounds = new Rectangle(pnlScripts.Width - 30, lblScriptsBeginUpdateScript.Top, 20, 20);
			btnScriptsBeginUpdateScript.Text = "...";
			btnScriptsBeginUpdateScript.MouseLeftUp += new EventHandler(btnScriptsBeginUpdateScript_MouseLeftUp);
			pnlScripts.Controls.Add(btnScriptsBeginUpdateScript);

			// lblScriptsBeginUpdateScriptView
			lblScriptsBeginUpdateScriptView = new Label(lblScriptsBeginUpdateScript);
			lblScriptsBeginUpdateScriptView.Left = lblScriptsBeginUpdateScript.Right + 5;
            lblScriptsBeginUpdateScriptView.Top = lblScriptsBeginUpdateScript.Top;
			lblScriptsBeginUpdateScriptView.Width = btnScriptsBeginUpdateScript.Left - lblScriptsBeginUpdateScriptView.Left;
			lblScriptsBeginUpdateScriptView.TextAlign = Desktop.Alignment.Center;
			lblScriptsBeginUpdateScriptView.Text = "---";
			pnlScripts.Controls.Add(lblScriptsBeginUpdateScriptView);

			pnlScripts.AutoHeight();
			cntSideBar.Controls.Add(pnlScripts);

			#endregion pnlScripts
			#region pnlNodeTypes

			//
			// pnlNodeTypes
			//
			pnlNodeTypes = new Container(pnlSettings);

			// cmbNodeTypes
			cmbNodeTypes = new ComboBox();
			cmbNodeTypes.Bounds = new Rectangle(10, 0, pnlNodeTypes.Width - 20, 20);
			pnlNodeTypes.Controls.Add(cmbNodeTypes);

			// btnNodeTypeLoad
			btnNodeTypeLoad = new ButtonText();
			btnNodeTypeLoad.Bounds = new Rectangle(10, cmbNodeTypes.Bottom + 10, (pnlNodeTypes.Width - 20) / 4, 20);
			btnNodeTypeLoad.Text = "Load";
			btnNodeTypeLoad.MouseLeftUp += new EventHandler(btnNodeTypeLoad_MouseLeftUp);
			pnlNodeTypes.Controls.Add(btnNodeTypeLoad);

			// btnNodeTypeUpdate
			btnNodeTypeUpdate = new ButtonText(btnNodeTypeLoad);
			btnNodeTypeUpdate.Left = btnNodeTypeLoad.Right;
			btnNodeTypeUpdate.Width = (pnlNodeTypes.Width - btnNodeTypeUpdate.Left - 10) / 3;
			btnNodeTypeUpdate.Text = "Save";
			btnNodeTypeUpdate.MouseLeftUp += new EventHandler(btnNodeTypeUpdate_MouseLeftUp);
			pnlNodeTypes.Controls.Add(btnNodeTypeUpdate);

			// btnNodeTypeAddNew
			btnNodeTypeAddNew = new ButtonText(btnNodeTypeUpdate);
			btnNodeTypeAddNew.Left = btnNodeTypeUpdate.Right;
			btnNodeTypeAddNew.Width = (pnlNodeTypes.Width - btnNodeTypeAddNew.Left - 10) / 2;
			btnNodeTypeAddNew.Text = "Add";
			btnNodeTypeAddNew.MouseLeftUp += new EventHandler(btnNodeTypeAddNew_MouseLeftUp);
			pnlNodeTypes.Controls.Add(btnNodeTypeAddNew);

			// btnNodeTypeDelete
			btnNodeTypeDelete = new ButtonText(btnNodeTypeAddNew);
			btnNodeTypeDelete.Left = btnNodeTypeDelete.Right;
			btnNodeTypeDelete.Width = pnlNodeTypes.Width - btnNodeTypeDelete.Left - 10;
			btnNodeTypeDelete.Text = "Del";
			btnNodeTypeDelete.MouseLeftUp += new EventHandler(btnNodeTypeDelete_MouseLeftUp);
			pnlNodeTypes.Controls.Add(btnNodeTypeDelete);

			// lblNodeTypeName
			lblNodeTypeName = new Label();
			lblNodeTypeName.AutoSize = false;
			lblNodeTypeName.Bounds = new Rectangle(0, btnNodeTypeDelete.Bottom + 10, 80, 20);
			lblNodeTypeName.Text = "Name:";
			lblNodeTypeName.TextAlign = Desktop.Alignment.CenterRight;
			pnlNodeTypes.Controls.Add(lblNodeTypeName);

			// txtNodeTypeName
			txtNodeTypeName = new TextBox();
			txtNodeTypeName.Top = lblNodeTypeName.Top;
			txtNodeTypeName.Left = lblNodeTypeName.Right + 5;
			txtNodeTypeName.Width = pnlNodeTypes.Width - txtNodeTypeName.Left - 10;
			txtNodeTypeName.Height = 20;
			txtNodeTypeName.FocusLost += new EventHandler(textBox_FocusLost);
			txtNodeTypeName.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlNodeTypes.Controls.Add(txtNodeTypeName);

			// lblNodeTypeIsParent
			lblNodeTypeIsParent = new Label(lblNodeTypeName);
			lblNodeTypeIsParent.Top += 25;
			lblNodeTypeIsParent.Text = "Parent:";
			pnlNodeTypes.Controls.Add(lblNodeTypeIsParent);

			// btnNodeTypeIsParent
			btnNodeTypeIsParent = new Button();
			btnNodeTypeIsParent.Left = lblNodeTypeIsParent.Right + 5;
			btnNodeTypeIsParent.Top = lblNodeTypeIsParent.Top + 5;
			btnNodeTypeIsParent.Height = lblNodeTypeIsParent.Height - 10;
			btnNodeTypeIsParent.Width = btnNodeTypeIsParent.Height;
			btnNodeTypeIsParent.IsToggle = true;
			btnNodeTypeIsParent.BackColorP = Desktop.DefButtonBackColor;
			btnNodeTypeIsParent.BackColor = Color.White;
			pnlNodeTypes.Controls.Add(btnNodeTypeIsParent);

			// lblNodeTypeNumSegs
			lblNodeTypeNumSegs = new Label(lblNodeTypeName);
			lblNodeTypeNumSegs.Top = lblNodeTypeIsParent.Bottom + 5;
			lblNodeTypeNumSegs.Text = "NumSegs:";
			pnlNodeTypes.Controls.Add(lblNodeTypeNumSegs);

			// txtNodeTypeNumSegs
			txtNodeTypeNumSegs = new TextBox(txtNodeTypeName);
			txtNodeTypeNumSegs.Top = lblNodeTypeNumSegs.Top;
			txtNodeTypeNumSegs.AllowAlpha = false;
			txtNodeTypeNumSegs.AllowedSpecChars = string.Empty;
			txtNodeTypeNumSegs.FocusLost += new EventHandler(textBox_FocusLost);
			txtNodeTypeNumSegs.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlNodeTypes.Controls.Add(txtNodeTypeNumSegs);

			// lblNodeTypeRadius
			lblNodeTypeRadius = new Label(lblNodeTypeNumSegs);
			lblNodeTypeRadius.Top = txtNodeTypeNumSegs.Bottom + 5;
			lblNodeTypeRadius.Text = "Radius:";
			pnlNodeTypes.Controls.Add(lblNodeTypeRadius);

			// txtNodeTypeRadius
			txtNodeTypeRadius = new TextBox(txtNodeTypeNumSegs);
			txtNodeTypeRadius.Top = lblNodeTypeRadius.Top;
			txtNodeTypeRadius.AllowedSpecChars = ".";
			txtNodeTypeRadius.FocusLost += new EventHandler(textBox_FocusLost);
			txtNodeTypeRadius.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlNodeTypes.Controls.Add(txtNodeTypeRadius);

			// lblNodeTypeSpacing
			lblNodeTypeSpacing = new Label(lblNodeTypeRadius);
			lblNodeTypeSpacing.Top = txtNodeTypeRadius.Bottom + 5;
			lblNodeTypeSpacing.Text = "Spacing:";
			pnlNodeTypes.Controls.Add(lblNodeTypeSpacing);

			// txtNodeTypeSpacing
			txtNodeTypeSpacing = new TextBox(txtNodeTypeRadius);
			txtNodeTypeSpacing.Top = lblNodeTypeSpacing.Top;
			txtNodeTypeSpacing.FocusLost += new EventHandler(textBox_FocusLost);
			txtNodeTypeSpacing.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlNodeTypes.Controls.Add(txtNodeTypeSpacing);

			// lblNodeTypeGenSpacing
			lblNodeTypeGenSpacing = new Label(lblNodeTypeSpacing);
			lblNodeTypeGenSpacing.Top = txtNodeTypeSpacing.Bottom + 5;
			lblNodeTypeGenSpacing.Text = "GenSpace:";
			pnlNodeTypes.Controls.Add(lblNodeTypeGenSpacing);

			// txtNodeTypeGenSpacing
			txtNodeTypeGenSpacing = new TextBox(txtNodeTypeSpacing);
			txtNodeTypeGenSpacing.Top = lblNodeTypeGenSpacing.Top;
			txtNodeTypeGenSpacing.FocusLost += new EventHandler(textBox_FocusLost);
			txtNodeTypeGenSpacing.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlNodeTypes.Controls.Add(txtNodeTypeGenSpacing);

			// lblNodeTypeSightDistance
			lblNodeTypeSightDistance = new Label(lblNodeTypeGenSpacing);
			lblNodeTypeSightDistance.Top = txtNodeTypeGenSpacing.Bottom + 5;
			lblNodeTypeSightDistance.Text = "SightDist:";
			pnlNodeTypes.Controls.Add(lblNodeTypeSightDistance);

			// txtNodeTypeSightDistance
			txtNodeTypeSightDistance = new TextBox(txtNodeTypeGenSpacing);
			txtNodeTypeSightDistance.Top = lblNodeTypeSightDistance.Top;
			txtNodeTypeSightDistance.FocusLost += new EventHandler(textBox_FocusLost);
			txtNodeTypeSightDistance.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlNodeTypes.Controls.Add(txtNodeTypeSightDistance);

			// lblNodeTypeBuildRangeMin
			lblNodeTypeBuildRangeMin = new Label(lblNodeTypeSightDistance);
			lblNodeTypeBuildRangeMin.Top = txtNodeTypeSightDistance.Bottom + 5;
			lblNodeTypeBuildRangeMin.Text = "BuildMin:";
			pnlNodeTypes.Controls.Add(lblNodeTypeBuildRangeMin);

			// txtNodeTypeBuildRangeMin
			txtNodeTypeBuildRangeMin = new TextBox(txtNodeTypeSightDistance);
			txtNodeTypeBuildRangeMin.Top = lblNodeTypeBuildRangeMin.Top;
			txtNodeTypeBuildRangeMin.FocusLost += new EventHandler(textBox_FocusLost);
			txtNodeTypeBuildRangeMin.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlNodeTypes.Controls.Add(txtNodeTypeBuildRangeMin);

			pnlNodeTypes.AutoHeight();
			cntSideBar.Controls.Add(pnlNodeTypes);

			#endregion pnlNodeTypes
			#region pnlPlayers

			//
			// pnlPlayers
			// 
			pnlPlayers = new Container(pnlSettings);

			// cmbPlayers
			cmbPlayers = new ComboBox();
			cmbPlayers.Bounds = new Rectangle(10, 0, pnlPlayers.Width - 20, 20);
			pnlPlayers.Controls.Add(cmbPlayers);

			// btnPlayerLoad
			btnPlayerLoad = new ButtonText();
			btnPlayerLoad.Bounds = new Rectangle(10, cmbPlayers.Bottom + 10, (pnlPlayers.Width - 20) / 4, 20);
			btnPlayerLoad.Text = "Load";
			btnPlayerLoad.MouseLeftUp += new EventHandler(btnPlayerLoad_MouseLeftUp);
			pnlPlayers.Controls.Add(btnPlayerLoad);

			// btnPlayerUpdate
			btnPlayerUpdate = new ButtonText(btnPlayerLoad);
			btnPlayerUpdate.Left = btnPlayerLoad.Right;
			btnPlayerUpdate.Width = (pnlPlayers.Width - btnPlayerUpdate.Left - 10) / 3;
			btnPlayerUpdate.Text = "Save";
			btnPlayerUpdate.MouseLeftUp += new EventHandler(btnPlayerUpdate_MouseLeftUp);
			pnlPlayers.Controls.Add(btnPlayerUpdate);

			// btnPlayerAddNew
			btnPlayerAddNew = new ButtonText(btnPlayerUpdate);
			btnPlayerAddNew.Left = btnPlayerUpdate.Right;
			btnPlayerAddNew.Width = (pnlPlayers.Width - btnPlayerAddNew.Left - 10) / 2;
			btnPlayerAddNew.Text = "Add";
			btnPlayerAddNew.MouseLeftUp += new EventHandler(btnPlayerAddNew_MouseLeftUp);
			pnlPlayers.Controls.Add(btnPlayerAddNew);

			// btnPlayerDelete
			btnPlayerDelete = new ButtonText(btnPlayerAddNew);
			btnPlayerDelete.Left = btnPlayerAddNew.Right;
			btnPlayerDelete.Width = pnlPlayers.Width - btnPlayerDelete.Left - 10;
			btnPlayerDelete.Text = "Del";
			btnPlayerDelete.MouseLeftUp += new EventHandler(btnPlayerDelete_MouseLeftUp);
			pnlPlayers.Controls.Add(btnPlayerDelete);

			// lblPlayerID
			lblPlayerID = new Label();
			lblPlayerID.AutoSize = false;
			lblPlayerID.Bounds = new Rectangle(0, btnPlayerDelete.Bottom + 10, 50, 20);
			lblPlayerID.Text = "ID:";
			lblPlayerID.TextAlign = Desktop.Alignment.CenterRight;
			pnlPlayers.Controls.Add(lblPlayerID);

			// txtPlayerID
			txtPlayerID = new TextBox();
			txtPlayerID.Top = lblPlayerID.Top;
			txtPlayerID.Left = lblPlayerID.Right + 5;
			txtPlayerID.Width = pnlPlayers.Width - txtPlayerID.Left - 10;
			txtPlayerID.Height = 20;
			txtPlayerID.FocusLost += new EventHandler(textBox_FocusLost);
			txtPlayerID.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlPlayers.Controls.Add(txtPlayerID);

			// lblPlayerName
			lblPlayerName = new Label(lblPlayerID);
			lblPlayerName.Top = txtPlayerID.Bottom + 10;
			lblPlayerName.Text = "Name:";
			pnlPlayers.Controls.Add(lblPlayerName);

			// txtPlayerName
			txtPlayerName = new TextBox();
			txtPlayerName.Top = lblPlayerName.Top;
			txtPlayerName.Left = lblPlayerName.Right + 5;
			txtPlayerName.Width = pnlPlayers.Width - txtPlayerName.Left - 10;
			txtPlayerName.Height = 20;
			txtPlayerName.FocusLost += new EventHandler(textBox_FocusLost);
			txtPlayerName.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlPlayers.Controls.Add(txtPlayerName);

			// lblPlayerTypes
			lblPlayerTypes = new Label(lblPlayerName);
			lblPlayerTypes.Top += 25;
			lblPlayerTypes.Text = "Type:";
			pnlPlayers.Controls.Add(lblPlayerTypes);

			// cmbPlayerTypes
			cmbPlayerTypes = new ComboBox(cmbPlayers, false);
			cmbPlayerTypes.Top = lblPlayerTypes.Top;
			cmbPlayerTypes.Left = lblPlayerTypes.Right + 5;
			cmbPlayerTypes.Width = pnlPlayers.Width - cmbPlayerTypes.Left - 10;
			cmbPlayerTypes.Menu.addRange(new string[] { "Human", "Network", "Computer" });
			pnlPlayers.Controls.Add(cmbPlayerTypes);

			pnlPlayers.AutoHeight();
			cntSideBar.Controls.Add(pnlPlayers);

			#endregion pnlPlayers
			#region pnlEditor

			//
			// pnlEditor
			//
			pnlEditor = new Container(pnlSettings);

			#region Add Objects

			// btnAddNode
			btnAddNode = new ButtonText(btnShowSettingsPnl);
			btnAddNode.Top = 0;
			btnAddNode.Text = "Add Node";
			btnAddNode.Font = fSegoe;
			btnAddNode.MouseLeftUp += new EventHandler(btnAddNode_MouseLeftUp);
			pnlEditor.Controls.Add(btnAddNode);

			// btnAddSeg
			btnAddSeg = new ButtonText(btnAddNode);
			btnAddSeg.Top += 20;
			btnAddSeg.Text = "Add Segment";
			btnAddSeg.MouseLeftUp += new EventHandler(btnAddSeg_MouseLeftUp);
			pnlEditor.Controls.Add(btnAddSeg);

			// btnAddGeo
			btnAddGeo = new ButtonText(btnAddSeg);
			btnAddGeo.Top += 20;
			btnAddGeo.Text = "Add Geo";
			btnAddGeo.MouseLeftUp += new EventHandler(btnAddGeo_MouseLeftUp);
			pnlEditor.Controls.Add(btnAddGeo);

			// btnAddHotSpot
			btnAddHotspot = new ButtonText(btnAddGeo);
			btnAddHotspot.Top += 20;
			btnAddHotspot.Text = "Add Hotspot";
			btnAddHotspot.MouseLeftUp += new EventHandler(btnAddHotspot_MouseLeftUp);
			pnlEditor.Controls.Add(btnAddHotspot);

			//
			// pnlEditorAddNodes
			//
			pnlEditorAddNodes = new Container();
			pnlEditorAddNodes.DrawBack = false;
			pnlEditorAddNodes.Bounds = new Rectangle(0, btnAddHotspot.Bottom + 10, pnlEditor.Width, 0);

			// lblEditorAddNodeOwner
			lblEditorAddNodeOwner = new Label();
			lblEditorAddNodeOwner.AutoSize = false;
			lblEditorAddNodeOwner.Bounds = new Rectangle(10, 0, 80, 20);
			lblEditorAddNodeOwner.Text = "Owner:";
			lblEditorAddNodeOwner.TextAlign = Desktop.Alignment.BottomLeft;
			pnlEditorAddNodes.Controls.Add(lblEditorAddNodeOwner);

			// cmbEditorAddNodeOwner
			cmbEditorAddNodeOwner = new ComboBox();
			cmbEditorAddNodeOwner.Bounds = new Rectangle(10, lblEditorAddNodeOwner.Bottom + 5, pnlEditor.Width - 20, 20);
			pnlEditorAddNodes.Controls.Add(cmbEditorAddNodeOwner);

			// lblEditorAddNodeType
			lblEditorAddNodeType = new Label(lblEditorAddNodeOwner);
			lblEditorAddNodeType.Top = cmbEditorAddNodeOwner.Bottom + 10;
			lblEditorAddNodeType.Text = "Type:";
			pnlEditorAddNodes.Controls.Add(lblEditorAddNodeType);

			// cmbEditorAddNodeType
			cmbEditorAddNodeType = new ComboBox(cmbEditorAddNodeOwner, false);
			cmbEditorAddNodeType.Top = lblEditorAddNodeType.Bottom + 5;
			pnlEditorAddNodes.Controls.Add(cmbEditorAddNodeType);

			pnlEditorAddNodes.AutoHeight();
			pnlEditor.Controls.Add(pnlEditorAddNodes);

			//
			// pnlEditorAddSegs
			//
			pnlEditorAddSegs = new Container(pnlEditorAddNodes);

			// lblEditorAddSegOwner
			lblEditorAddSegOwner = new Label(lblEditorAddNodeOwner);
			pnlEditorAddSegs.Controls.Add(lblEditorAddSegOwner);

			// cmbEditorAddSegOwner
			cmbEditorAddSegOwner = new ComboBox(cmbEditorAddNodeOwner, true);
			pnlEditorAddSegs.Controls.Add(cmbEditorAddSegOwner);

			pnlEditorAddSegs.AutoHeight();
			pnlEditor.Controls.Add(pnlEditorAddSegs);

			//
			// pnlEditorAddGeos
			//
			pnlEditorAddGeos = new Container(pnlEditorAddSegs);
			pnlEditorAddGeos.Height = 0;
			pnlEditor.Controls.Add(pnlEditorAddGeos);

			//
			// pnlEditorAddHotspots
			//
			pnlEditorAddHotspots = new Container(pnlEditorAddGeos);
			pnlEditorAddHotspots.Height = 0;
			pnlEditor.Controls.Add(pnlEditorAddHotspots);

			// ctlEditorDivider
			ctlEditorDivider = new Control();
			ctlEditorDivider.Bounds = new Rectangle(10, btnAddHotspot.Bottom + 10, pnlEditor.Width - 20, 1);
			ctlEditorDivider.BackColor = Desktop.DefButtonBackColor;
			ctlEditorDivider.Ignore = true;
			pnlEditor.Controls.Add(ctlEditorDivider);

			#endregion Add Objects
			#region pnlEditorNodes

			//
			// pnlEditorNodes
			//
			pnlEditorNodes = new Container(pnlEditorAddHotspots);
			pnlEditorNodes.Top = ctlEditorDivider.Bottom + 10;

			// lblEditorNodeOwner
			lblEditorNodeOwner = new Label(lblEditorAddNodeOwner);
			lblEditorNodeOwner.Top = 0;
			pnlEditorNodes.Controls.Add(lblEditorNodeOwner);

			// cmbEditorNodeOwner
			cmbEditorNodeOwner = new ComboBox(cmbEditorAddNodeOwner, true);
			cmbEditorNodeOwner.Top = lblEditorNodeOwner.Bottom + 5;
			pnlEditorNodes.Controls.Add(cmbEditorNodeOwner);

			// lblEditorNodeType
			lblEditorNodeType = new Label(lblEditorAddNodeType);
			lblEditorNodeType.Top = cmbEditorNodeOwner.Bottom + 10;
			pnlEditorNodes.Controls.Add(lblEditorNodeType);

			// cmbEditorNodeType
			cmbEditorNodeType = new ComboBox(cmbEditorAddNodeType, true);
			cmbEditorNodeType.Top = lblEditorNodeType.Bottom + 5;
			pnlEditorNodes.Controls.Add(cmbEditorNodeType);

			// btnEditorNodeLoadType
			btnEditorNodeLoadType = new ButtonText();
			btnEditorNodeLoadType.Bounds = new Rectangle(10, cmbEditorNodeType.Bottom + 5, cmbEditorNodeType.Width, 20);
			btnEditorNodeLoadType.Text = "Load";
			btnEditorNodeLoadType.MouseLeftUp += new EventHandler(btnEditorNodeLoadType_MouseLeftUp);
			pnlEditorNodes.Controls.Add(btnEditorNodeLoadType);

			// lblEditorNodeID
			lblEditorNodeID = new Label(lblEditorNodeOwner);
			lblEditorNodeID.Top = btnEditorNodeLoadType.Bottom + 10;
			lblEditorNodeID.Left = 0;
			lblEditorNodeID.TextAlign = Desktop.Alignment.CenterRight;
			lblEditorNodeID.Text = "ID:";
			pnlEditorNodes.Controls.Add(lblEditorNodeID);

			// txtEditorNodeID
			txtEditorNodeID = new TextBox();
			txtEditorNodeID.Top = lblEditorNodeID.Top;
			txtEditorNodeID.Left = lblEditorNodeID.Right + 5;
			txtEditorNodeID.Width = pnlEditorNodes.Width - txtEditorNodeID.Left - 10;
			txtEditorNodeID.Height = 20;
			txtEditorNodeID.AllowAlpha = false;
			txtEditorNodeID.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeID.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeID);

			// lblEditorNodeIsParent
			lblEditorNodeIsParent = new Label(lblEditorNodeID);
			lblEditorNodeIsParent.Top = txtEditorNodeID.Bottom + 10;
			lblEditorNodeIsParent.Text = "Parent:";
			pnlEditorNodes.Controls.Add(lblEditorNodeIsParent);

			// btnEditorNodeIsParent
			btnEditorNodeIsParent = new Button();
			btnEditorNodeIsParent.Left = lblEditorNodeIsParent.Right + 5;
			btnEditorNodeIsParent.Top = lblEditorNodeIsParent.Top + 5;
			btnEditorNodeIsParent.Height = lblEditorNodeIsParent.Height - 10;
			btnEditorNodeIsParent.Width = btnEditorNodeIsParent.Height;
			btnEditorNodeIsParent.IsToggle = true;
			btnEditorNodeIsParent.BackColorP = Desktop.DefButtonBackColor;
			btnEditorNodeIsParent.BackColor = Color.White;
			pnlEditorNodes.Controls.Add(btnEditorNodeIsParent);

			// lblEditorNodeNumSegs
			lblEditorNodeNumSegs = new Label(lblEditorNodeIsParent);
			lblEditorNodeNumSegs.Top = lblEditorNodeIsParent.Bottom + 5;
			lblEditorNodeNumSegs.Text = "NumSegs:";
			pnlEditorNodes.Controls.Add(lblEditorNodeNumSegs);

			// txtEditorNodeNumSegs
			txtEditorNodeNumSegs = new TextBox(txtEditorNodeID);
			txtEditorNodeNumSegs.Top = lblEditorNodeNumSegs.Top;
			txtEditorNodeNumSegs.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeNumSegs.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeNumSegs);

			// lblEditorNodeRadius
			lblEditorNodeRadius = new Label(lblEditorNodeNumSegs);
			lblEditorNodeRadius.Top = txtEditorNodeNumSegs.Bottom + 5;
			lblEditorNodeRadius.Text = "Radius:";
			pnlEditorNodes.Controls.Add(lblEditorNodeRadius);

			// txtEditorNodeRadius
			txtEditorNodeRadius = new TextBox(txtEditorNodeNumSegs);
			txtEditorNodeRadius.Top = lblEditorNodeRadius.Top;
			txtEditorNodeRadius.AllowedSpecChars = ".";
			txtEditorNodeRadius.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeRadius.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeRadius);

			// lblEditorNodeSpacing
			lblEditorNodeSpacing = new Label(lblEditorNodeRadius);
			lblEditorNodeSpacing.Top = txtEditorNodeRadius.Bottom + 5;
			lblEditorNodeSpacing.Text = "Spacing:";
			pnlEditorNodes.Controls.Add(lblEditorNodeSpacing);

			// txtEditorNodeSpacing
			txtEditorNodeSpacing = new TextBox(txtEditorNodeRadius);
			txtEditorNodeSpacing.Top = lblEditorNodeSpacing.Top;
			txtEditorNodeSpacing.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeSpacing.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeSpacing);

			// lblEditorNodeGenSpacing
			lblEditorNodeGenSpacing = new Label(lblEditorNodeSpacing);
			lblEditorNodeGenSpacing.Top = txtEditorNodeSpacing.Bottom + 5;
			lblEditorNodeGenSpacing.Text = "GenSpace:";
			pnlEditorNodes.Controls.Add(lblEditorNodeGenSpacing);

			// txtEditorNodeGenSpacing
			txtEditorNodeGenSpacing = new TextBox(txtEditorNodeSpacing);
			txtEditorNodeGenSpacing.Top = lblEditorNodeGenSpacing.Top;
			txtEditorNodeGenSpacing.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeGenSpacing.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeGenSpacing);

			// lblEditorNodeGenCountDown
			lblEditorNodeGenCountDown = new Label(lblEditorNodeGenSpacing);
			lblEditorNodeGenCountDown.Top = txtEditorNodeGenSpacing.Bottom + 5;
			lblEditorNodeGenCountDown.Text = "GenCntDwn:";
			pnlEditorNodes.Controls.Add(lblEditorNodeGenCountDown);

			// txtEditorNodeGenCountDown
			txtEditorNodeGenCountDown = new TextBox(txtEditorNodeGenSpacing);
			txtEditorNodeGenCountDown.Top = lblEditorNodeGenCountDown.Top;
			txtEditorNodeGenCountDown.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeGenCountDown.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeGenCountDown);

			// lblEditorNodeSightDist
			lblEditorNodeSightDist = new Label(lblEditorNodeGenCountDown);
			lblEditorNodeSightDist.Top = txtEditorNodeGenCountDown.Bottom + 5;
			lblEditorNodeSightDist.Text = "SightDist:";
			pnlEditorNodes.Controls.Add(lblEditorNodeSightDist);

			// txtEditorNodeSightDist
			txtEditorNodeSightDist = new TextBox(txtEditorNodeGenCountDown);
			txtEditorNodeSightDist.Top = lblEditorNodeSightDist.Top;
			txtEditorNodeSightDist.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeSightDist.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeSightDist);

			// lblEditorNodeX
			lblEditorNodeX = new Label(lblEditorNodeSightDist);
			lblEditorNodeX.Top = txtEditorNodeSightDist.Bottom + 5;
			lblEditorNodeX.Text = "X:";
			pnlEditorNodes.Controls.Add(lblEditorNodeX);

			// txtEditorNodeX
			txtEditorNodeX = new TextBox(txtEditorNodeSightDist);
			txtEditorNodeX.Top = lblEditorNodeX.Top;
			txtEditorNodeX.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeX.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeX);

			// lblEditorNodeY
			lblEditorNodeY = new Label(lblEditorNodeX);
			lblEditorNodeY.Top = txtEditorNodeX.Bottom + 5;
			lblEditorNodeY.Text = "Y:";
			pnlEditorNodes.Controls.Add(lblEditorNodeY);

			// txtEditorNodeY
			txtEditorNodeY = new TextBox(txtEditorNodeX);
			txtEditorNodeY.Top = lblEditorNodeY.Top;
			txtEditorNodeY.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeY.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeY);

			// lblEditorNodeOwnsHotspot
			lblEditorNodeOwnsHotspot = new Label(lblEditorNodeY);
			lblEditorNodeOwnsHotspot.Top = txtEditorNodeY.Bottom + 5;
			lblEditorNodeOwnsHotspot.Text = "OwnsHotSpot:";
			pnlEditorNodes.Controls.Add(lblEditorNodeOwnsHotspot);

			// txtEditorNodeOwnsHotspot
			txtEditorNodeOwnsHotspot = new TextBox(txtEditorNodeY);
			txtEditorNodeOwnsHotspot.Top = lblEditorNodeOwnsHotspot.Top;
			txtEditorNodeOwnsHotspot.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorNodeY.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorNodes.Controls.Add(txtEditorNodeOwnsHotspot);

			// lblEditorNodeActive
			lblEditorNodeActive = new Label(lblEditorNodeOwnsHotspot);
			lblEditorNodeActive.Top = txtEditorNodeOwnsHotspot.Bottom + 5;
			lblEditorNodeActive.Text = "Active:";
			pnlEditorNodes.Controls.Add(lblEditorNodeActive);

			// btnEditorNodeActive
			btnEditorNodeActive = new Button(btnEditorNodeIsParent);
			btnEditorNodeActive.Left = lblEditorNodeActive.Right + 5;
			btnEditorNodeActive.Top = lblEditorNodeActive.Top + 5;
			pnlEditorNodes.Controls.Add(btnEditorNodeActive);

			// btnEditorNodeApply
			btnEditorNodeApply = new ButtonText(btnEditorNodeLoadType);
			btnEditorNodeApply.Top = lblEditorNodeActive.Bottom + 10;
			btnEditorNodeApply.Text = "Apply";
			btnEditorNodeApply.MouseLeftUp += new EventHandler(btnEditorNodeApply_MouseLeftUp);
			pnlEditorNodes.Controls.Add(btnEditorNodeApply);

			// btnEditorNodeDel
			btnEditorNodeDel = new ButtonText(btnEditorNodeApply);
			btnEditorNodeDel.Top = btnEditorNodeApply.Bottom + 10;
			btnEditorNodeDel.Text = "Delete";
			btnEditorNodeDel.MouseLeftUp += new EventHandler(btnEditorNodeDel_MouseLeftUp);
			pnlEditorNodes.Controls.Add(btnEditorNodeDel);

			pnlEditorNodes.AutoHeight();
			pnlEditor.Controls.Add(pnlEditorNodes);

			#endregion pnlEditorNodes
			#region pnlEditorSegs

			//
			// pnlEditorSegs
			//
			pnlEditorSegs = new Container(pnlEditorNodes);

			// lblEditorSegOwner
			lblEditorSegOwner = new Label(lblEditorAddNodeOwner);
			lblEditorSegOwner.Top = 0;
			pnlEditorSegs.Controls.Add(lblEditorSegOwner);

			// cmbEditorSegOwner
			cmbEditorSegOwner = new ComboBox(cmbEditorAddNodeOwner, true);
			cmbEditorSegOwner.Top = lblEditorSegOwner.Bottom + 5;
			pnlEditorSegs.Controls.Add(cmbEditorSegOwner);

			// lblEditorSegLenLbl
			lblEditorSegLenLbl = new Label();
			lblEditorSegLenLbl.AutoSize = false;
			lblEditorSegLenLbl.Bounds = new Rectangle(10, cmbEditorSegOwner.Bottom + 10, 55, 20);
			lblEditorSegLenLbl.TextAlign = Desktop.Alignment.CenterRight;
			lblEditorSegLenLbl.Text = "Length:";
			pnlEditorSegs.Controls.Add(lblEditorSegLenLbl);

			// lblEditorSegLenVal
			lblEditorSegLenVal = new Label(lblEditorSegLenLbl);
			lblEditorSegLenVal.Left = lblEditorSegLenLbl.Right + 5;
			lblEditorSegLenVal.Top = lblEditorSegLenLbl.Top;
			lblEditorSegLenVal.Width = pnlEditorSegs.Width - lblEditorSegLenVal.Left - 10;
			lblEditorSegLenVal.TextAlign = Desktop.Alignment.CenterLeft;
			pnlEditorSegs.Controls.Add(lblEditorSegLenVal);

			// lblEditorSegEndLen0
			lblEditorSegEndLen0 = new Label(lblEditorSegLenLbl);
			lblEditorSegEndLen0.Top = lblEditorSegLenVal.Bottom + 5;
			lblEditorSegEndLen0.Text = "EndLen0:";
			pnlEditorSegs.Controls.Add(lblEditorSegEndLen0);

			// txtEditorSegEndLen0;
			txtEditorSegEndLen0 = new TextBox();
			txtEditorSegEndLen0.Top = lblEditorSegEndLen0.Top;
			txtEditorSegEndLen0.Left = lblEditorSegEndLen0.Right + 5;
			txtEditorSegEndLen0.Width = pnlEditorSegs.Width - txtEditorSegEndLen0.Left - 10;
			txtEditorSegEndLen0.Height = 20;
			txtEditorSegEndLen0.AllowAlpha = false;
			txtEditorSegEndLen0.AllowedSpecChars = ".";
			txtEditorSegEndLen0.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorSegEndLen0.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorSegs.Controls.Add(txtEditorSegEndLen0);

			// lblEditorSegEndLen1;
			lblEditorSegEndLen1 = new Label(lblEditorSegEndLen0);
			lblEditorSegEndLen1.Top = txtEditorSegEndLen0.Bottom + 5;
			lblEditorSegEndLen1.Text = "EndLen1:";
			pnlEditorSegs.Controls.Add(lblEditorSegEndLen1);

			// txtEditorSegEndLen1;
			txtEditorSegEndLen1 = new TextBox(txtEditorSegEndLen0);
			txtEditorSegEndLen1.Top = lblEditorSegEndLen1.Top;
			txtEditorSegEndLen1.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorSegEndLen1.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorSegs.Controls.Add(txtEditorSegEndLen1);

			// lblEditorSegEndState0;
			lblEditorSegEndState0 = new Label(lblEditorSegEndLen1);
			lblEditorSegEndState0.Top = txtEditorSegEndLen1.Bottom + 5;
			lblEditorSegEndState0.Text = "State0:";
			pnlEditorSegs.Controls.Add(lblEditorSegEndState0);
			
			// cmbEditorSegEndState0;
			cmbEditorSegEndState0 = new ComboBox();
			cmbEditorSegEndState0.Bounds = new Rectangle(lblEditorSegEndState0.Right + 5, lblEditorSegEndState0.Top, pnlPlayers.Width - lblEditorSegEndState0.Right - 15, 20);
			cmbEditorSegEndState0.Menu.addRange(new string[] { "Complete", "Building", "Retracting" });
			pnlEditorSegs.Controls.Add(cmbEditorSegEndState0);

			// lblEditorSegEndState1;
			lblEditorSegEndState1 = new Label(lblEditorSegEndState0);
			lblEditorSegEndState1.Top = cmbEditorSegEndState0.Bottom + 5;
			lblEditorSegEndState1.Text = "State1:";
			pnlEditorSegs.Controls.Add(lblEditorSegEndState1);

			// cmbEditorSegEndState1;
			cmbEditorSegEndState1 = new ComboBox(cmbEditorSegEndState0, true);
			cmbEditorSegEndState1.Top = lblEditorSegEndState1.Top;
			pnlEditorSegs.Controls.Add(cmbEditorSegEndState1);

			// lblEditorSegLanePeople0;
			lblEditorSegLanePeople0 = new Label(lblEditorSegEndState1);
			lblEditorSegLanePeople0.Top = cmbEditorSegEndState1.Bottom + 5;
			lblEditorSegLanePeople0.Text = "Ppl0:";
			pnlEditorSegs.Controls.Add(lblEditorSegLanePeople0);

			// txtEditorSegLanePeople0;
			txtEditorSegLanePeople0 = new TextBox(txtEditorSegEndLen1);
			txtEditorSegLanePeople0.Top = lblEditorSegLanePeople0.Top;
			txtEditorSegLanePeople0.AllowedSpecChars = ".,";
			txtEditorSegLanePeople0.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorSegLanePeople0.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorSegs.Controls.Add(txtEditorSegLanePeople0);

			// lblEditorSegLanePeople1;
			lblEditorSegLanePeople1 = new Label(lblEditorSegLanePeople0);
			lblEditorSegLanePeople1.Top = txtEditorSegLanePeople0.Bottom + 5;
			lblEditorSegLanePeople1.Text = "Ppl1:";
			pnlEditorSegs.Controls.Add(lblEditorSegLanePeople1);

			// txtEditorSegLanePeople1;
			txtEditorSegLanePeople1 = new TextBox(txtEditorSegLanePeople0);
			txtEditorSegLanePeople1.Top = lblEditorSegLanePeople1.Top;
			txtEditorSegLanePeople1.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorSegLanePeople1.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorSegs.Controls.Add(txtEditorSegLanePeople1);

			// btnEditorSegApply;
			btnEditorSegApply = new ButtonText(btnEditorNodeApply);
			btnEditorSegApply.Top = txtEditorSegLanePeople1.Bottom + 10;
			btnEditorSegApply.MouseLeftUp += new EventHandler(btnEditorSegApply_MouseLeftUp);
			pnlEditorSegs.Controls.Add(btnEditorSegApply);
			
			// btnEditorSegDel;
			btnEditorSegDel = new ButtonText(btnEditorSegApply);
			btnEditorSegDel.Top = btnEditorSegApply.Bottom + 10;
			btnEditorSegDel.Text = "Delete";
			btnEditorSegDel.MouseLeftUp += new EventHandler(btnEditorSegDel_MouseLeftUp);
			pnlEditorSegs.Controls.Add(btnEditorSegDel);

			pnlEditorSegs.AutoHeight();
			pnlEditor.Controls.Add(pnlEditorSegs);

			#endregion pnlEditorSegs
			#region pnlEditorGeos

			//
			// pnlEditorGeos
			//
			pnlEditorGeos = new Container(pnlEditorSegs);

			// lblEditorGeoCloseLoop
			lblEditorGeoCloseLoop = new Label();
			lblEditorGeoCloseLoop.AutoSize = false;
			lblEditorGeoCloseLoop.Bounds = new Rectangle(10, 0, 75, 20);
			lblEditorGeoCloseLoop.TextAlign = Desktop.Alignment.CenterRight;
			lblEditorGeoCloseLoop.Text = "CloseLoop:";
			pnlEditorGeos.Controls.Add(lblEditorGeoCloseLoop);

			// btnEditorGeoCloseLoop
			btnEditorGeoCloseLoop = new Button();
			btnEditorGeoCloseLoop.Left = lblEditorGeoCloseLoop.Right + 5;
			btnEditorGeoCloseLoop.Top = lblEditorGeoCloseLoop.Top + 5;
			btnEditorGeoCloseLoop.Height = lblEditorGeoCloseLoop.Height - 10;
			btnEditorGeoCloseLoop.Width = btnEditorGeoCloseLoop.Height;
			btnEditorGeoCloseLoop.IsToggle = true;
			btnEditorGeoCloseLoop.BackColorP = Desktop.DefButtonBackColor;
			btnEditorGeoCloseLoop.BackColor = Color.White;
			pnlEditorGeos.Controls.Add(btnEditorGeoCloseLoop);

			// lblEditorGeoDisplay
			lblEditorGeoDisplay = new Label(lblEditorGeoCloseLoop);
			lblEditorGeoDisplay.Top = lblEditorGeoCloseLoop.Bottom + 5;
			lblEditorGeoDisplay.Text = "Display:";
			pnlEditorGeos.Controls.Add(lblEditorGeoDisplay);

			// btnEditorGeoDisplay
			btnEditorGeoDisplay = new Button(btnEditorGeoCloseLoop);
			btnEditorGeoDisplay.Top = lblEditorGeoDisplay.Top + 5;
			pnlEditorGeos.Controls.Add(btnEditorGeoDisplay);

			// btnEditorGeoApply
			btnEditorGeoApply = new ButtonText(btnEditorNodeApply);
			btnEditorGeoApply.Top = btnEditorGeoDisplay.Bottom + 10;
			btnEditorGeoApply.MouseLeftUp += new EventHandler(btnEditorGeoApply_MouseLeftUp);
			pnlEditorGeos.Controls.Add(btnEditorGeoApply);

			// btnEditorGeoDel
			btnEditorGeoDel = new ButtonText(btnEditorNodeApply);
			btnEditorGeoDel.Top = btnEditorGeoApply.Bottom + 10;
			btnEditorGeoDel.Text = "Delete";
			btnEditorGeoDel.MouseLeftUp += new EventHandler(btnEditorGeoDel_MouseLeftUp);
			pnlEditorGeos.Controls.Add(btnEditorGeoDel);

			pnlEditorGeos.AutoHeight();
			pnlEditor.Controls.Add(pnlEditorGeos);

			#endregion pnlEditorGeos
			#region pnlEditorHotspots

			pnlEditorHotspots = new Container(pnlEditorGeos);

			// lblEditorHotspotIDLbl
			lblEditorHotspotIDLbl = new Label(lblEditorNodeActive);
			lblEditorHotspotIDLbl.Top = 0;
			lblEditorHotspotIDLbl.Text = "ID:";
			pnlEditorHotspots.Controls.Add(lblEditorHotspotIDLbl);

			// lblEditorHotspotIDVal
			lblEditorHotspotIDVal = new Label(lblEditorSegLenVal);
			lblEditorHotspotIDVal.Top = lblEditorHotspotIDLbl.Top;
			lblEditorHotspotIDVal.Left = txtBuildRate.Left;
			pnlEditorHotspots.Controls.Add(lblEditorHotspotIDVal);

			// lblEditorHotspotX
			lblEditorHotspotX = new Label(lblEditorHotspotIDLbl);
			lblEditorHotspotX.Top = lblEditorHotspotIDVal.Bottom + 5;
			lblEditorHotspotX.Text = "X:";
			pnlEditorHotspots.Controls.Add(lblEditorHotspotX);

			// txtEditorHotspotX
			txtEditorHotspotX = new TextBox(txtEditorNodeX);
			txtEditorHotspotX.Top = lblEditorHotspotX.Top;
			txtEditorHotspotX.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorHotspotX.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorHotspots.Controls.Add(txtEditorHotspotX);

			// lblEditorHotspotY
			lblEditorHotspotY = new Label(lblEditorHotspotX);
			lblEditorHotspotY.Top = txtEditorHotspotX.Bottom + 5;
			lblEditorHotspotY.Text = "Y:";
			pnlEditorHotspots.Controls.Add(lblEditorHotspotY);

			// txtEditorHotspotY
			txtEditorHotspotY = new TextBox(txtEditorHotspotX);
			txtEditorHotspotY.Top = lblEditorHotspotY.Top;
			txtEditorHotspotY.FocusLost += new EventHandler(textBox_FocusLost);
			txtEditorHotspotY.FocusReceived += new EventHandler(textBox_FocusReceived);
			pnlEditorHotspots.Controls.Add(txtEditorHotspotY);

			// lblEditorHotspotScript
			lblEditorHotspotScript = new Label(lblEditorHotspotY);
			lblEditorHotspotScript.Top = txtEditorHotspotY.Bottom + 5;
			lblEditorHotspotScript.Text = "Script:";
			pnlEditorHotspots.Controls.Add(lblEditorHotspotScript);

			// btnEditorHotspotScript
			btnEditorHotspotScript = new ButtonText(btnEditorNodeLoadType);
			btnEditorHotspotScript.Top = lblEditorHotspotScript.Top;
			btnEditorHotspotScript.Width = btnEditorHotspotScript.Height;
			btnEditorHotspotScript.Left = txtEditorHotspotY.Right - btnEditorHotspotScript.Width;
			btnEditorHotspotScript.Text = "...";
			btnEditorHotspotScript.MouseLeftUp += new EventHandler(btnEditorHotspotScript_MouseLeftUp);
			pnlEditorHotspots.Controls.Add(btnEditorHotspotScript);

			// lblEditorHotspotScriptView
			lblEditorHotspotScriptView = new Label(lblEditorHotspotIDVal);
			lblEditorHotspotScriptView.Top = lblEditorHotspotScript.Top;
			lblEditorHotspotScriptView.Width = btnEditorHotspotScript.Left - lblEditorHotspotScriptView.Left;
			lblEditorHotspotScriptView.TextAlign = Desktop.Alignment.Center;
			lblEditorHotspotScriptView.Text = "---";
			pnlEditorHotspots.Controls.Add(lblEditorHotspotScriptView);

			// lblEditorHotspotScriptVal
			lblEditorHotspotScriptVal = new Label(lblEditorHotspotScriptView);
			lblEditorHotspotScriptVal.Visible = false;
			pnlEditorHotspots.Controls.Add(lblEditorHotspotScriptVal);

			// btnEditorHotspotApply
			btnEditorHotspotApply = new ButtonText(btnEditorNodeLoadType);
			btnEditorHotspotApply.Top = lblEditorHotspotScriptView.Bottom + 10;
			btnEditorHotspotApply.Text = "Apply";
			btnEditorHotspotApply.MouseLeftUp += new EventHandler(btnEditorHotspotApply_MouseLeftUp);
			pnlEditorHotspots.Controls.Add(btnEditorHotspotApply);

			// btnEditorHotspotDel
			btnEditorHotspotDel = new ButtonText(btnEditorHotspotApply);
			btnEditorHotspotDel.Top = btnEditorHotspotApply.Bottom + 10;
			btnEditorHotspotDel.Text = "Delete";
			btnEditorHotspotDel.MouseLeftUp += new EventHandler(btnEditorHotspotDel_MouseLeftUp);
			pnlEditorHotspots.Controls.Add(btnEditorHotspotDel);

			pnlEditorHotspots.AutoHeight();
			pnlEditor.Controls.Add(pnlEditorHotspots);

			#endregion pnlEditorHotspots

			pnlEditor.AutoHeight();
			cntSideBar.Controls.Add(pnlEditor);

			#endregion pnlEditor
			#region Load and Save

			// btnSaveWorld
			btnSaveWorld = new ButtonText();
			btnSaveWorld.Bounds = new Rectangle(10, cntSideBar.Bottom - 30, cntSideBar.Width - 20, 20);
			btnSaveWorld.Text = "Save World";
			btnSaveWorld.MouseLeftUp += new EventHandler(btnSaveWorld_MouseLeftUp);
			cntSideBar.Controls.Add(btnSaveWorld);

			// btnLoadWorld
			btnLoadWorld = new ButtonText(btnSaveWorld);
			btnLoadWorld.Top -= 25;
			btnLoadWorld.Text = "Load World";
			btnLoadWorld.MouseLeftUp += new EventHandler(btnLoadWorld_MouseLeftUp);
			cntSideBar.Controls.Add(btnLoadWorld);

			#endregion Load and Save

			refreshSideBar();
			desktop.Controls.Add(cntSideBar);

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
			#region cntSaveLoad

			//
			// cntSaveLoad
			//
			cntSaveLoad = new Container();
			cntSaveLoad.Bounds = new Rectangle(Graphics.PreferredBackBufferWidth / 2 - 500, Graphics.PreferredBackBufferHeight / 2 - 200, 1000, 400);
			cntSaveLoad.Visible = false;

			// lblSaveLoadTitle
			lblSaveLoadTitle = new Label();
			lblSaveLoadTitle.AutoSize = false;
			lblSaveLoadTitle.DrawBack = true;
			lblSaveLoadTitle.Bounds = new Rectangle(0, 0, cntSaveLoad.Width, 20);
			lblSaveLoadTitle.TextAlign = Desktop.Alignment.Center;
			cntSaveLoad.Controls.Add(lblSaveLoadTitle);
			
			// lblSaveLoadPath
			lblSaveLoadPath = new Label();
			lblSaveLoadPath.Text = "Path:";
			lblSaveLoadPath.Location = new Point(10, lblSaveLoadTitle.Bottom + 10);
			cntSaveLoad.Controls.Add(lblSaveLoadPath);

			// btnSaveLoadPath
			btnSaveLoadPath = new List<ButtonText>(4);

			// btnSaveLoadConfirm
			btnSaveLoadConfirm = new ButtonText();
			btnSaveLoadConfirm.Bounds = new Rectangle(cntSaveLoad.Width - 60, cntSaveLoad.Height - 30, 50, 20);
			btnSaveLoadConfirm.MouseLeftUp += new EventHandler(btnSaveLoadConfirm_MouseLeftUp);
			cntSaveLoad.Controls.Add(btnSaveLoadConfirm);
			
			// btnSaveLoadCancel
			btnSaveLoadCancel = new ButtonText(btnSaveLoadConfirm);
			btnSaveLoadCancel.Left = btnSaveLoadConfirm.Left - 55;
			btnSaveLoadCancel.Text = "Cancel";
			btnSaveLoadCancel.MouseLeftUp += new EventHandler(btnSaveLoadCancel_MouseLeftUp);
			cntSaveLoad.Controls.Add(btnSaveLoadCancel);

			// lblSaveLoadFileName
			lblSaveLoadFileName = new Label();
			lblSaveLoadFileName.Text = "File Name:";
			lblSaveLoadFileName.Location = new Point(10, btnSaveLoadConfirm.Top - 30);
			cntSaveLoad.Controls.Add(lblSaveLoadFileName);

			// txtSaveLoadFileName
			txtSaveLoadFileName = new TextBox();
			txtSaveLoadFileName.Bounds = new Rectangle(lblSaveLoadFileName.Right + 5, lblSaveLoadFileName.Top, cntSaveLoad.Width - lblSaveLoadFileName.Right - 15, 20);
			cntSaveLoad.Controls.Add(txtSaveLoadFileName);

			// lsbSaveLoadFiles
			lsbSaveLoadFiles = new ListBoxText();
			lsbSaveLoadFiles.Bounds = new Rectangle(10, lblSaveLoadTitle.Bottom + 40, cntSaveLoad.Width - 20, txtSaveLoadFileName.Top - lblSaveLoadTitle.Bottom - 50);
			lsbSaveLoadFiles.MouseLeftDoubleClick += new EventHandler(lsbSaveLoadFiles_OnMouseLeftDoubleClick);
			lsbSaveLoadFiles.SelectedIndexChanged += new EventHandler(lsbSaveLoadFiles_SelectedIndexChanged);
			cntSaveLoad.Controls.Add(lsbSaveLoadFiles);

			desktop.Controls.Add(cntSaveLoad);

			#endregion cntSaveLoad
			#region cntTextEditor

			//
			// cntTextEditor
			//
			cntTextEditor = new Container();
			cntTextEditor.Bounds = new Rectangle(Graphics.PreferredBackBufferWidth / 2 - 500, Graphics.PreferredBackBufferHeight / 2 - 200, 1000, 400);
			cntTextEditor.Visible = false;

			// lblTextEditorTitle
			lblTextEditorTitle = new Label();
			lblTextEditorTitle.AutoSize = false;
			lblTextEditorTitle.DrawBack = true;
			lblTextEditorTitle.Bounds = new Rectangle(0, 0, cntTextEditor.Width, 20);
			lblTextEditorTitle.TextAlign = Desktop.Alignment.Center;
			cntTextEditor.Controls.Add(lblTextEditorTitle);

			// btnTextEditorOK
			btnTextEditorOK = new ButtonText();
			btnTextEditorOK.Bounds = new Rectangle(cntTextEditor.Width - 60, cntTextEditor.Height - 30, 50, 20);
			btnTextEditorOK.Text = "OK";
			btnTextEditorOK.MouseLeftUp += new EventHandler(btnTextEditorOK_MouseLeftUp);
			cntTextEditor.Controls.Add(btnTextEditorOK);

			btnTextEditorOK.MouseLeftUp += new EventHandler(btnTextEditorOK_MouseLeftUp_HotspotScript); // for hotspot scripts
			btnTextEditorOK.MouseLeftUp += new EventHandler(btnTextEditorOK_MouseLeftUp_BeginUpdateScript); // for hotspot scripts

			// btnTextEditorCancel
			btnTextEditorCancel = new ButtonText(btnTextEditorOK);
			btnTextEditorCancel.Left = btnTextEditorOK.Left - 55;
			btnTextEditorCancel.Text = "Cancel";
			btnTextEditorCancel.MouseLeftUp += new EventHandler(btnTextEditorCancel_MouseLeftUp);
			cntTextEditor.Controls.Add(btnTextEditorCancel);

			// txtTextEditorText
			txtTextEditorText = new TextBoxMulti();
			txtTextEditorText.Bounds = new Rectangle(10, lblTextEditorTitle.Bottom + 40, cntTextEditor.Width - 20, btnTextEditorOK.Top - lblTextEditorTitle.Bottom - 50);
			txtTextEditorText.AllowedSpecChars = null;
			txtTextEditorText.AllowMultiLine = true;
			txtTextEditorText.Font = fCourierNew;
			txtTextEditorText.EditPositionOffsetX = -3;
			txtTextEditorText.EditPositionOffsetY = 0;
			txtTextEditorText.StopOnTab = false;
			cntTextEditor.Controls.Add(txtTextEditorText);

			desktop.Controls.Add(cntTextEditor);

			#endregion cntTextEditor
		}

		/// <summary>Refreshes all controls in the sidebar to match the world.</summary>
		private void refreshSideBar()
		{
			btnShowSettingsPnl.Pressed = true;
			btnShowScriptsPnl.Pressed = false;
			btnShowNodeTypesPnl.Pressed = false;
			btnShowPlayersPnl.Pressed = false;
			btnShowEditorPnl.Pressed = false;

			// pnlSettings
			pnlSettings.Visible = true;
			txtWorldWidth.Text = world.Width.ToString();
			txtWorldHeight.Text = world.Height.ToString();

			txtPersonSpacing.Text = world.PersonSpacing.ToString();
			txtPersonSpeedLower.Text = world.PersonSpeedLower.ToString();
			txtPersonSpeedUpper.Text = world.PersonSpeedUpper.ToString();

			txtRetractSpeed.Text = world.RetractSpeed.ToString();
			txtBuildRate.Text = world.BuildRate.ToString();

			txtCamWidth.Text = world.Cam.Width.ToString();
			txtCamHeight.Text = world.Cam.Height.ToString();
			txtCamX.Text = world.Cam.CenterX.ToString();
			txtCamY.Text = world.Cam.CenterY.ToString();

			txtGridRows.Text = world.Grid.NumRows.ToString();
			txtGridCols.Text = world.Grid.NumCols.ToString();

			txtFogRows.Text = world.FogRows.ToString();
			txtFogCols.Text = world.FogCols.ToString();

			txtPathRows.Text = world.PathRows.ToString();
			txtPathCols.Text = world.PathCols.ToString();

			// pnlScripts
			lblScriptsBeginUpdateScriptView.Text = string.IsNullOrWhiteSpace(world.ScriptBeginUpdate) ? "---" : "###";
			pnlScripts.Visible = false;

			// pnlNodeTypes
			pnlNodeTypes.Visible = false;

			// pnlPlayers
			pnlPlayers.Visible = false;

			// pnlEditor
			pnlEditor.Visible = false;
			btnAddNode.Pressed = false;
			btnAddSeg.Pressed = false;
			btnAddGeo.Pressed = false;
			btnAddHotspot.Pressed = false;

			ctlEditorDivider.Top = btnAddHotspot.Bottom + 10;

			pnlEditorNodes.Top =
			pnlEditorSegs.Top =
			pnlEditorGeos.Top =
			pnlEditorHotspots.Top = ctlEditorDivider.Bottom + 10;

			pnlEditorAddNodes.Visible = false;
			pnlEditorAddSegs.Visible = false;
			pnlEditorAddGeos.Visible = false;
			pnlEditorAddHotspots.Visible = false;

			pnlEditorNodes.Visible = false;
			pnlEditorSegs.Visible = false;
			pnlEditorGeos.Visible = false;
			pnlEditorHotspots.Visible = false;

			// populate node type ComboBoxes
			cmbNodeTypes.Menu.clear();
			cmbEditorAddNodeType.Menu.clear();
			cmbEditorNodeType.Menu.clear();
			foreach (NodeType nt in world.NodeTypes)
			{
				cmbNodeTypes.Menu.addItem(nt.Name);
				cmbEditorAddNodeType.Menu.addItem(nt.Name);
				cmbEditorNodeType.Menu.addItem(nt.Name);
			}
			cmbNodeTypes.SelectedIndex =
			cmbEditorNodeType.SelectedIndex =
			cmbEditorAddNodeType.SelectedIndex = (cmbEditorAddNodeType.Menu.Count > 0) ? 0 : -1;
			loadSelNodeType();

			// populate player ComboBoxes
			cmbPlayers.Menu.clear();
			cmbEditorAddNodeOwner.Menu.clear();
			cmbEditorAddSegOwner.Menu.clear();
			cmbEditorNodeOwner.Menu.clear();
			cmbEditorSegOwner.Menu.clear();
			cmbEditorAddNodeOwner.Menu.addItem("[None]");
			cmbEditorAddSegOwner.Menu.addItem("[None]");
			cmbEditorNodeOwner.Menu.addItem("[None]");
			cmbEditorSegOwner.Menu.addItem("[None]");
			foreach (Player p in world.Players)
			{
				string nm = string.IsNullOrEmpty(p.Name) ? ("[Unnamed " + p.Type.ToString() + " " + p.ID + "]") : p.Name;
				cmbPlayers.Menu.addItem(nm);
				cmbEditorAddNodeOwner.Menu.addItem(nm);
				cmbEditorAddSegOwner.Menu.addItem(nm);
				cmbEditorNodeOwner.Menu.addItem(nm);
				cmbEditorSegOwner.Menu.addItem(nm);
			}
			cmbPlayers.SelectedIndex = (cmbPlayers.Menu.Count > 0) ? 0 : -1;
			cmbEditorAddNodeOwner.SelectedIndex = (cmbEditorAddNodeOwner.Menu.Count > 0) ? 0 : -1;
			cmbEditorAddSegOwner.SelectedIndex = (cmbEditorAddSegOwner.Menu.Count > 0) ? 0 : -1;
			cmbEditorNodeOwner.SelectedIndex = (cmbEditorNodeOwner.Menu.Count > 0) ? 0 : -1;
			cmbEditorSegOwner.SelectedIndex = (cmbEditorSegOwner.Menu.Count > 0) ? 0 : -1;
			loadSelPlayer();

			loadObjectEditor();
		}

		/// <summary>Loads the selected node type's settings into the interface.</summary>
		private void loadSelNodeType()
		{
			if (cmbNodeTypes.SelectedIndex == -1)
			{
				txtNodeTypeName.Text = "";
				btnNodeTypeIsParent.Pressed = false;
				txtNodeTypeNumSegs.Text = "1";
				txtNodeTypeRadius.Text = "1";
				txtNodeTypeSpacing.Text = "1";
				txtNodeTypeGenSpacing.Text = "0";
				txtNodeTypeSightDistance.Text = "1";
				txtNodeTypeBuildRangeMin.Text = "0";
			}
			else
			{
				NodeType nt = world.NodeTypes[cmbNodeTypes.SelectedIndex];
				txtNodeTypeName.Text = nt.Name;
				btnNodeTypeIsParent.Pressed = nt.IsParent;
				txtNodeTypeNumSegs.Text = nt.NumSegments.ToString();
				txtNodeTypeRadius.Text = nt.Radius.ToString();
				txtNodeTypeSpacing.Text = nt.Spacing.ToString();
				txtNodeTypeGenSpacing.Text = nt.GenSpacing.ToString();
				txtNodeTypeSightDistance.Text = nt.SightDistance.ToString();
				txtNodeTypeBuildRangeMin.Text = nt.BuildRangeMin.ToString();
			}
		}

		/// <summary>Loads the selected player's settings into the interface.</summary>
		private void loadSelPlayer()
		{
			if (cmbPlayers.SelectedIndex >= 0)
			{
				Player p = world.Players[cmbPlayers.SelectedIndex];
				txtPlayerID.Text = p.ID;
				txtPlayerName.Text = p.Name;
				cmbPlayerTypes.SelectedIndex = (int)p.Type;
			}
			else
			{
				txtPlayerID.Text = string.Empty;
				txtPlayerName.Text = string.Empty;
				cmbPlayerTypes.SelectedIndex = 0;
			}
		}

		/// <summary>Loads the object editor.</summary>
		private void loadObjectEditor()
		{
			pnlEditorNodes.Visible = selectedNode != null;
			pnlEditorSegs.Visible = selectedSeg != null;
			pnlEditorGeos.Visible = selectedGeo != null;
			pnlEditorHotspots.Visible = selectedHotspot != null;

			if (selectedNode != null)
			{
				cmbEditorNodeOwner.SelectedIndex = world.Players.IndexOf(selectedNode.Owner) + 1;
				cmbEditorNodeType.SelectedIndex = world.NodeTypes.IndexOf(selectedNode.NType);
				txtEditorNodeID.Text = selectedNode.ID;
				btnEditorNodeIsParent.Pressed = selectedNode.IsParent;
				txtEditorNodeNumSegs.Text = selectedNode.Segments.Length.ToString();
				txtEditorNodeRadius.Text = selectedNode.Radius.ToString();
				txtEditorNodeSpacing.Text = selectedNode.Spacing.ToString();
				txtEditorNodeGenSpacing.Text = selectedNode.GenSpacing.ToString();
				txtEditorNodeGenCountDown.Text = selectedNode.GenCountDown.ToString();
				txtEditorNodeSightDist.Text = selectedNode.SightDistance.ToString();
				txtEditorNodeX.Text = selectedNode.X.ToString();
				txtEditorNodeY.Text = selectedNode.Y.ToString();
				txtEditorNodeOwnsHotspot.Text = (selectedNode.OwnsHotspot == null) ? string.Empty : selectedNode.OwnsHotspot.ID;
				btnEditorNodeActive.Pressed = selectedNode.Active;
			}
			else if (selectedSeg != null)
			{
				cmbEditorSegOwner.SelectedIndex = world.Players.IndexOf(selectedSeg.Owner) + 1;
				lblEditorSegLenVal.Text = selectedSeg.Length.ToString();
				txtEditorSegEndLen0.Text = selectedSeg.EndLength[0].ToString();
				txtEditorSegEndLen1.Text = selectedSeg.EndLength[1].ToString();
				cmbEditorSegEndState0.SelectedIndex = (int)selectedSeg.State[0];
				cmbEditorSegEndState1.SelectedIndex = (int)selectedSeg.State[1];

				StringBuilder txt = new StringBuilder();
				foreach (FInt p in selectedSeg.People[0])
				{
					if (txt.Length > 0)
						txt.Append(",");
					txt.Append(p.ToString());
				}
				txtEditorSegLanePeople0.Text = txt.ToString();

				txt.Clear();
				foreach (FInt p in selectedSeg.People[1])
				{
					if (txt.Length > 0)
						txt.Append(",");
					txt.Append(p.ToString());
				}
				txtEditorSegLanePeople1.Text = txt.ToString();
			}
			else if (selectedGeo != null)
			{
				btnEditorGeoCloseLoop.Pressed = selectedGeo.CloseLoop;
				btnEditorGeoDisplay.Pressed = selectedGeo.Display;
			}
			else if (selectedHotspot != null)
			{
				lblEditorHotspotIDVal.Text = selectedHotspot.ID;
				txtEditorHotspotX.Text = selectedHotspot.X.ToString();
				txtEditorHotspotY.Text = selectedHotspot.Y.ToString();
				if (string.IsNullOrWhiteSpace(selectedHotspot.Script))
				{
					lblEditorHotspotScriptView.Text = "---";
					lblEditorHotspotScriptVal.Text = string.Empty;
				}
				else
				{
					lblEditorHotspotScriptView.Text = "###";
					lblEditorHotspotScriptVal.Text = selectedHotspot.Script;
				}
			}
		}

		/// <summary>Shows a message to the user.</summary>
		/// <param name="message">The message to show.</param>
		private void showMsg(string message)
		{
			lblMsgBoxMessage.Text = message;
			cntMsgBox.Visible = true;
		}

		/// <summary>Initializes initCntSaveLoad.</summary>
		private void initCntSaveLoad()
		{
            string path = Directory.GetCurrentDirectory().TrimEnd('\\');
			string[] folders = path.Split('\\');
			cntSaveLoad.Controls.Remove(btnSaveLoadPath);
			btnSaveLoadPath.Clear();
			foreach (string f in folders)
			{
				ButtonText newButton = new ButtonText();
				newButton.Text = f;
				newButton.Bounds = new Rectangle(((btnSaveLoadPath.Count == 0) ? lblSaveLoadPath.Right : btnSaveLoadPath[btnSaveLoadPath.Count - 1].Right) + 5, lblSaveLoadTitle.Bottom + 10, (int)(newButton.Font.MeasureString(f).X + .5f) + 10, 20);
				newButton.MouseLeftUp += new EventHandler(btnSaveLoadPath_MouseLeftUp);
				cntSaveLoad.Controls.Add(newButton);
				btnSaveLoadPath.Add(newButton);
			}

			fillSaveLoadFilesLB(path + "\\");
			txtSaveLoadFileName.Text = string.Empty;
		}

		/// <summary>Closes cntSaveLoad and clears extra resources.</summary>
		private void closeCntSaveLoad()
		{
			cntSaveLoad.Visible = false;
			cntSaveLoad.Controls.Remove(btnSaveLoadPath);
			btnSaveLoadPath.Clear();
			lsbSaveLoadFiles.Clear();
		}

		/// <summary>Fills lsbSaveLoadFiles from the provided path.</summary>
		/// <param name="path">The path the use.</param>
		private void fillSaveLoadFilesLB(string path)
		{
			lsbSaveLoadFiles.Clear();
			if (path != btnSaveLoadPath[0].Text + "\\")
				lsbSaveLoadFiles.Add("..");
			DirectoryInfo dir = new DirectoryInfo(path);
			foreach (DirectoryInfo d in dir.EnumerateDirectories())
			{
				if ((d.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
					lsbSaveLoadFiles.Add("<DIR> " + d.Name);
			}
			foreach (FileInfo f in dir.EnumerateFiles("*.txt"))
			{
				
				lsbSaveLoadFiles.Add(f.Name);
			}
		}

		/// <summary>Loads the currently selected file in lsbLoadSaveFile into the editor.</summary>
		private void loadSelectedFile()
		{
			StringBuilder path = new StringBuilder();
			foreach (ButtonText b in btnSaveLoadPath)
			{
				path.Append(b.Text);
				path.Append('\\');
			}
			path.Append(lsbSaveLoadFiles.SelectedItem);

			hoveredNode = null;
			selectedNode = null;
			hoveredSeg = null;
			selectedSeg = null;
			hoveredSegEnd = null;
			hoveredSegEndOwner = null;
			selectedSegEnd = null;
			hoveredGeo = null;
			selectedGeo = null;
			world = WorldLoader.loadWorld(path.ToString(), Graphics);

			refreshSideBar();
		}

		/// <summary>Saves the world to the file specified in cntLoadSave.</summary>
		private void saveWorld()
		{
			// TODO:
			StringBuilder path = new StringBuilder();
			foreach (ButtonText b in btnSaveLoadPath)
			{
				path.Append(b.Text);
				path.Append('\\');
			}
			path.Append(txtSaveLoadFileName.Text);

			string errorMessage = WorldSaver.saveWorld(world, path.ToString());
			showMsg(string.IsNullOrEmpty(errorMessage) ? "Saved Successfully" : ("Error Saving World: " + errorMessage));
		}

		/// <summary>Shows the text editor.</summary>
		/// <param name="title">The title to display on the text editor.</param>
		private void showTextEditor(string title)
		{
			lblTextEditorTitle.Text = title;
			txtTextEditorText.Text = string.Empty;

			cntTextEditor.Visible = true;
		}

		/// <summary>If the provided TextBox's value doesn't parse to a double, then replace it with the provided default value.</summary>
		/// <param name="txt">The default value to replace with.</param>
		/// <param name="def">The default value to revert to if the value is invalid.</param>
		private void enforceDouble(TextBox txt, string def)
		{
			enforceDouble(txt, def, double.MinValue);
		}

		/// <summary>If the provided TextBox's value doesn't parse to a double, then replace it with the provided default value.</summary>
		/// <param name="txt">The default value to replace with.</param>
		/// <param name="def">The default value to revert to if the value is invalid.</param>
		/// <param name="minValue">The lowest that the value is allowed to be.</param>
		private void enforceDouble(TextBox txt, string def, double minValue)
		{
			double val = 0d;
			if (!double.TryParse(txt.Text, out val) || val < minValue)
				txt.Text = def;
		}

		/// <summary>If the provided TextBox's value doesn't parse to an int, then replace it with the provided default value.</summary>
		/// <param name="txt">The default value to replace with.</param>
		/// <param name="def">The default value to revert to if the value is invalid.</param>
		private void enforceInt(TextBox txt, string def)
		{
			enforceInt(txt, def, int.MinValue);
		}

		/// <summary>If the provided TextBox's value doesn't parse to an int, then replace it with the provided default value.</summary>
		/// <param name="txt">The default value to replace with.</param>
		/// <param name="def">The default value to revert to if the value is invalid.</param>
		/// <param name="minValue">The lowest that the value is allowed to be.</param>
		private void enforceInt(TextBox txt, string def, int minValue)
		{
			int val = 0;
			if (!int.TryParse(txt.Text, out val) || val < minValue)
				txt.Text = def;
		}

		/// <summary>Validates a comma-delimited TextBox to make aure all values are valid and are in ascending order.</summary>
		/// <param name="txt">The TextBox to validate.</param>
		/// <param name="def">The default value to revert to if the value is invalid.</param>
		private void validatePpl(TextBox txt, string def)
		{
			if (txt.Text == string.Empty)
				return;

			bool valid = true;

			string[] ppl = txt.Text.Split(',');

			double p0 = 0d;
			double p1 = 0d;

			if (!double.TryParse(ppl[0], out p0))
			{
				valid = false;
			}
			else
			{
				for (int i = 1; i < ppl.Length; i++)
				{
					if (!double.TryParse(ppl[i], out p1) || p1 <= p0)
					{
						valid = false;
						break;
					}

					p0 = p1;
				}
			}

			if (!valid)
				txt.Text = def;
		}

		/// <summary>Moves the provided node to the specified coordinates.</summary>
		/// <param name="node">The node to move.</param>
		/// <param name="to">The coordinates to move the node to.</param>
		private void moveNode(Node node, VectorF to)
		{
			// remove from grid
			world.Grid.Point(node.Pos, node, world.gridRemoveNode);

			// remove attached segments from the grid
			foreach (Segment seg in world.Segments)
			{
				if (seg.Nodes[0] == node || seg.Nodes[1] == node)
					world.Grid.Line(seg.Nodes[0].Pos, seg.Nodes[1].Pos, seg, world.gridRemoveSegment);
			}

			node.Pos = to;

			// add back into grid
			if (world.Nodes.Contains(node))
				world.Grid.Point(node.Pos, node, world.gridAddNode);

			// add attached segments back into the grid
			foreach (Segment seg in world.Segments)
			{
				if (seg.Nodes[0] == node || seg.Nodes[1] == node)
				{
					double q0 = (double)seg.EndLength[0] / (double)seg.Length;
					double q1 = (double)seg.EndLength[1] / (double)seg.Length;
					seg.refreshMath();
					seg.EndLength[0] = (seg.State[0] == SegmentSkel.SegState.Complete) ? seg.Length : (FInt)((double)seg.Length * q0);
					seg.EndLength[1] = (seg.State[1] == SegmentSkel.SegState.Complete) ? seg.Length : (FInt)((double)seg.Length * q1);
					seg.refreshEndLocs();
					world.Grid.Line(seg.Nodes[0].Pos, seg.Nodes[1].Pos, seg, world.gridAddSegment);
				}
			}
		}

		private void moveHotspot(Hotspot hotspot, VectorF to)
		{
			// remove from grid
			world.Grid.Point(hotspot.Pos, hotspot, world.gridRemoveHotspot);

			hotspot.Pos = to;

			// add back into grid
			if (world.Hotspots.Contains(hotspot))
				world.Grid.Point(hotspot.Pos, hotspot, world.gridAddHotspot);
		}

		/// <summary>Removes the selected node from the world.</summary>
		private void removeSelNode()
		{
			// disconnect segments
			foreach (Segment seg in world.Segments)
			{
				for (int i = 0; i < 2; i++)
				{
					if (seg.Nodes[i] == selectedNode)
					{
						int oppLane = 1 - i;
						if (seg.State[oppLane] == SegmentSkel.SegState.Complete)
							seg.State[oppLane] = SegmentSkel.SegState.Retracting;
						break;
					}
				}
			}

			world.removeNode(selectedNode);
			selectedNode = null;
			loadObjectEditor();
		}

		/// <summary>Removes the selected segment from the world.</summary>
		private void removeSelSeg()
		{
			int index = selectedSeg.Nodes[0].getSegIndex(selectedSeg);
			if (index != -1)
				selectedSeg.Nodes[0].removeSegment(index, false, false);

			index = selectedSeg.Nodes[1].getSegIndex(selectedSeg);
			if (index != -1)
				selectedSeg.Nodes[1].removeSegment(index, false, false);

			world.removeSegment(selectedSeg);
			selectedSeg = null;
			loadObjectEditor();
		}

		/// <summary>Removes the selected geo vertex or line from the world.</summary>
		private void removeSelGeo()
		{
			VectorF[] vertices;

			if (selectedGeoIsLine)
			{
				if (selectedGeo.Vertices.Length == 2)
				{
					world.removeGeo(selectedGeo);
					selectedGeo = null;
					loadObjectEditor();
					return;
				}

				vertices = new VectorF[selectedGeo.Vertices.Length - 2];
				for (int i = 0; i < selectedGeoVertex; i++)
				{
					vertices[i] = selectedGeo.Vertices[i];
				}
				for (int i = selectedGeoVertex + 2; i < selectedGeo.Vertices.Length; i++)
				{
					vertices[i - 2] = selectedGeo.Vertices[i];
				}
			}
			else if (selectedGeoVertex == -1)
			{
				world.removeGeo(selectedGeo);
				selectedGeo = null;
				loadObjectEditor();
				return;
			}
			else
			{
				if (selectedGeo.Vertices.Length == 1)
				{
					world.removeGeo(selectedGeo);
					selectedGeo = null;
					loadObjectEditor();
					return;
				}

				if (selectedGeo.Vertices.Length == 2)
				{
					vertices = new VectorF[1];
					vertices[0] = selectedGeo.Vertices[1 - selectedGeoVertex];
				}
				else if (selectedGeoVertex == 0)
				{
					vertices = new VectorF[selectedGeo.Vertices.Length - 2];
					for (int i = 2; i < selectedGeo.Vertices.Length; i++)
					{
						vertices[i - 2] = selectedGeo.Vertices[i];
					}
				}
				else if (selectedGeoVertex == selectedGeo.Vertices.Length - 1)
				{
					vertices = new VectorF[selectedGeo.Vertices.Length - 2];
					for (int i = 0, max = selectedGeo.Vertices.Length - 2; i < max; i++)
					{
						vertices[i] = selectedGeo.Vertices[i];
					}
				}
				else
				{
					vertices = new VectorF[selectedGeo.Vertices.Length - 2];
					for (int i = 0, max = selectedGeoVertex - 1; i < max; i++)
					{
						vertices[i] = selectedGeo.Vertices[i];
					}
					for (int i = selectedGeoVertex + 1; i < selectedGeo.Vertices.Length; i++)
					{
						vertices[i - 2] = selectedGeo.Vertices[i];
					}
				}
			}

			world.Grid.Rect(selectedGeo.UpperLeft, selectedGeo.LowerRight, selectedGeo, world.gridRemoveGeo);
			selectedGeo.Vertices = vertices;
			if (selectedGeo.Vertices.Length < 3)
			{
				selectedGeo.CloseLoop = false;
				selectedGeo.Display = false;
			}
			selectedGeo.refreshMath(new Vector2((float)tGeo.Width, (float)tGeo.Height));
			world.Grid.Rect(selectedGeo.UpperLeft, selectedGeo.LowerRight, selectedGeo, world.gridAddGeo);
			selectedGeo = null;
			loadObjectEditor();
		}

		/// <summary>Removes the selected node from the world.</summary>
		private void removeSelHotspot()
		{
			foreach (Node node in world.Nodes)
			{
				if (node.OwnsHotspot == selectedHotspot)
					node.OwnsHotspot = null;
			}
			world.removeHotspot(selectedHotspot);
			selectedHotspot = null;
			loadObjectEditor();
		}

		#region Core Game Methods

		/// <summary>Loads all content related to this mode.</summary>
		public override void LoadContent()
		{
			fSegoe = Content.Load<SpriteFont>("fonts/Segoe");
			fSegoeBold = Content.Load<SpriteFont>("fonts/SegoeBold");
			fCourierNew = Content.Load<SpriteFont>("fonts/CourierNew");

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

			buildGUI(Graphics);

			lblCursorPos = new Label();
			lblCursorPos.AutoSize = false;
			lblCursorPos.Bounds = new Rectangle(0, 0, 400, 40);
			lblCursorPos.ForeColor = Color.White;
			desktop.Controls.Add(lblCursorPos);
		}

		/// <summary>Updates all variables in this mode.</summary>
		/// <param name="gameTime">The current game time.</param>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			FInt elapsed = (FInt)gameTime.ElapsedGameTime.TotalSeconds;

			// update gui
			if (Inp.OldMse.Position != Inp.Mse.Position)
				desktop.mouseMove(Inp.Mse.Position);
			
			GUI.Desktop.Event evnt = Desktop.Event.MouseRightUp;
			bool guiOwnedInput = false;

			if (Inp.OldMse.LeftButton == ButtonState.Pressed && Inp.Mse.LeftButton == ButtonState.Released)
				guiOwnedInput |= desktop.PerformMouseEvent(Desktop.Event.MouseLeftUp, Inp.Mse.Position, Inp);
			if (Inp.OldMse.LeftButton == ButtonState.Released && Inp.Mse.LeftButton == ButtonState.Pressed)
				guiOwnedInput |= desktop.PerformMouseEvent(Desktop.Event.MouseLeftDown, Inp.Mse.Position, Inp);
			if (Inp.OldMse.RightButton == ButtonState.Pressed && Inp.Mse.RightButton == ButtonState.Released)
				guiOwnedInput |= desktop.PerformMouseEvent(Desktop.Event.MouseRightUp, Inp.Mse.Position, Inp);
			if (Inp.OldMse.RightButton == ButtonState.Released && Inp.Mse.RightButton == ButtonState.Pressed)
				guiOwnedInput |= desktop.PerformMouseEvent(Desktop.Event.MouseRightUp, Inp.Mse.Position, Inp);

			Keys[] newKeys = Inp.Key.GetPressedKeys();
			
			if (newKeys.Length != 0)
				guiOwnedInput |= desktop.PerformKeyEvent(newKeys, Inp);

			if (guiOwnedInput)
			{
				hoveredNode = null;
				hoveredSeg = null;
				hoveredSegEnd = null;
				hoveredSegEndOwner = null;
				hoveredGeo = null;
				hoveredHotspot = null;
				isDragging = false;
			}
			else
			{
				// prepare grid manager
				world.Grid.startNewUpdate(gameTime);

				// move camera
				if (Inp.Key.IsKeyDown(Keys.OemPlus))
					world.Cam.Zoom += world.Cam.Zoom * elapsed;

				if (Inp.Key.IsKeyDown(Keys.OemMinus))
					world.Cam.Zoom -= world.Cam.Zoom * elapsed;

				if (Inp.Key.IsKeyDown(Keys.A))
					world.Cam.CenterX -= (700 / world.Cam.Zoom) * elapsed;

				if (Inp.Key.IsKeyDown(Keys.D))
					world.Cam.CenterX += (700 / world.Cam.Zoom) * elapsed;

				if (Inp.Key.IsKeyDown(Keys.W))
					world.Cam.CenterY -= (700 / world.Cam.Zoom) * elapsed;

				if (Inp.Key.IsKeyDown(Keys.S))
					world.Cam.CenterY += (700 / world.Cam.Zoom) * elapsed;

				world.Cam.Zoom += (FInt)(Inp.Mse.ScrollWheelValue - Inp.OldMse.ScrollWheelValue) / (FInt)120 * (FInt).1d * world.Cam.Zoom;

				world.Cam.refreshCorners();

				// get cursor world coordinates
				VectorF cursorPos = world.Cam.screenToWorld(new Vector2((float)Inp.Mse.X, (float)Inp.Mse.Y));

				// check for hovered node, segment, segment end, and hotspot
				hoveredNode = world.NodeAtPoint(cursorPos, false);
				hoveredSeg = (hoveredNode == null) ? world.segmentAtPoint(cursorPos, null) : null;
				hoveredSegEnd = world.SegmentEndAtPoint(cursorPos);

				if (hoveredSegEnd != null)
				{
					foreach (Segment seg in world.Segments)
					{
						if (seg.Nodes[0] == hoveredSegEnd || seg.Nodes[1] == hoveredSegEnd)
						{
							hoveredSegEndOwner = seg.Owner;
							break;
						}
					}
				}
				else
				{
					hoveredSegEndOwner = null;
				}

				hoveredHotspot = world.HotspotAtPoint(cursorPos);

				// test geo vertices
				hoveredGeo = world.geoAtPoint(cursorPos, out hoveredGeoVertex, true);

				// test geo lines
				if (hoveredGeo == null)
				{
					hoveredGeo = world.geoAtPoint(cursorPos, out hoveredGeoVertex, false);
					hoveredGeoIsLine = true;
				}
				else
				{
					hoveredGeoIsLine = false;
				}

				// if the user just released the left mouse button
				if (Inp.OldMse.LeftButton == ButtonState.Pressed && Inp.Mse.LeftButton == ButtonState.Released)
				{
					if (selectedSegEnd != null)
					{
						// do nothing
					}
					// add node
					else if (btnAddNode.Pressed && cmbNodeTypes.SelectedIndex != -1 && selectedGeo == null && (selectedNode == null || isDragging) && selectedSeg == null && selectedHotspot == null)
					{
						bool useHoveredEnd = (hoveredSegEnd != null && (selectedNode == null || selectedNode.Owner == hoveredSegEndOwner));

						// add node
						Node node = new Node(world, world.NodeTypes[cmbEditorAddNodeType.SelectedIndex]);
						node.Pos = useHoveredEnd ? hoveredSegEnd.Pos : cursorPos;
						node.Active = true;

						if (selectedNode != null && isDragging)
						{
							node.Owner = selectedNode.Owner;

							if (selectedNode.NumSegments < selectedNode.Segments.Length) // add segment too
							{
								Segment seg = new Segment(world);
								seg.Owner = selectedNode.Owner;
								seg.Nodes[0] = selectedNode;
								seg.Nodes[1] = node;
								selectedNode.addSegment(seg, false);
								node.addSegment(seg, false);
								seg.refreshMath();
								seg.EndLength[0] = seg.Length;
								seg.EndLength[1] = seg.Length;
								seg.refreshEndLocs();
								world.addSegment(seg);
							}
						}
						else if (selectedNode == null && hoveredSegEnd != null)
						{
							node.Owner = hoveredSegEndOwner;
						}
						else if (cmbEditorAddNodeOwner.SelectedIndex != 0)
						{
							node.Owner = world.Players[cmbEditorAddNodeOwner.SelectedIndex - 1];
						}

						if (useHoveredEnd)
						{
							// link segments to it
							foreach (Segment seg in (hoveredSegEndOwner == null ? world.Segments : hoveredSegEndOwner.Segments))
							{
								for (int i = 0; i < 2; i++)
								{
									if (seg.Nodes[i] == hoveredSegEnd)
									{
										seg.Nodes[i] = node;
										break;
									}
								}
							}
						}

						world.addNode(node);
						selectedNode = node;
						selectedSeg = null;
						loadObjectEditor();
					}
					// add segment
					else if (btnAddSeg.Pressed && isDragging && selectedSeg == null && selectedGeo == null && (selectedNode == null || selectedNode.NumSegments < selectedNode.Segments.Length) && selectedHotspot == null)
					{
						Segment seg = new Segment(world);
						seg.Owner = (selectedNode != null) ? selectedNode.Owner
							: (hoveredNode != null) ? hoveredNode.Owner
							: (cmbEditorAddSegOwner.SelectedIndex > 0) ? world.Players[cmbEditorAddSegOwner.SelectedIndex - 1]
							: null;

						if (selectedNode != null)
						{
							seg.Nodes[0] = selectedNode;
							seg.Owner = selectedNode.Owner;
							selectedNode.addSegment(seg, false);
						}
						else
						{
							seg.State[1] = SegmentSkel.SegState.Retracting;
							seg.Nodes[0] = new Node(world);
							seg.Nodes[0].Pos = lastClickedPoint;
							seg.Nodes[0].Active = false;
							seg.Nodes[0].Destroyed = true;
							seg.Nodes[0].initSegArrays(0);
						}

						if (hoveredNode != null && hoveredNode.NumSegments != hoveredNode.Segments.Length && (selectedNode == null || (hoveredNode != selectedNode && hoveredNode.Owner == selectedNode.Owner && hoveredNode.relatedSeg(selectedNode) == -1)))
						{
							seg.Nodes[1] = hoveredNode;
							hoveredNode.addSegment(seg, false);
						}
						else
						{
							seg.State[0] = SegmentSkel.SegState.Building;
							seg.Nodes[1] = new Node(world);
							seg.Nodes[1].Pos = cursorPos;
							seg.Nodes[1].Active = false;
							seg.Nodes[1].Destroyed = true;
							seg.Nodes[1].initSegArrays(0);
						}

						seg.refreshMath();
						seg.EndLength[0] = seg.Length;
						seg.EndLength[1] = seg.Length;
						seg.refreshEndLocs();
						world.addSegment(seg);

						selectedNode = null;
						selectedSeg = seg;
						loadObjectEditor();
					}
					// add geo vertex
					else if (btnAddGeo.Pressed && selectedNode == null && selectedSeg == null && selectedSegEnd == null && selectedHotspot == null)
					{
						// add vertex to existing geo
						if (selectedGeo != null && !selectedGeoIsLine && isDragging && (selectedGeoVertex == 0 || selectedGeoVertex == selectedGeo.Vertices.Length - 1))
						{
							VectorF[] vertices;
							if (selectedGeo.Vertices.Length == 1)
							{
								vertices = new VectorF[selectedGeo.Vertices.Length + 1];
								vertices[0] = cursorPos;
								vertices[1] = selectedGeo.Vertices[0];
							}
							else if (selectedGeoVertex == 0)
							{
								vertices = new VectorF[selectedGeo.Vertices.Length + 2];
								selectedGeo.Vertices.CopyTo(vertices, 2);
								vertices[0] = cursorPos;
								vertices[1] = selectedGeo.Vertices[0];
							}
							else
							{
								vertices = new VectorF[selectedGeo.Vertices.Length + 2];
								selectedGeo.Vertices.CopyTo(vertices, 0);
								vertices[selectedGeo.Vertices.Length] = selectedGeo.Vertices[selectedGeo.Vertices.Length - 1];
								vertices[selectedGeo.Vertices.Length + 1] = cursorPos;
								selectedGeoVertex = vertices.Length - 1;
							}

							world.Grid.Rect(selectedGeo.UpperLeft, selectedGeo.LowerRight, selectedGeo, world.gridRemoveGeo);
							selectedGeo.Vertices = vertices;
							selectedGeo.refreshMath(new Vector2((float)tGeo.Width, (float)tGeo.Height));
							world.Grid.Rect(selectedGeo.UpperLeft, selectedGeo.LowerRight, selectedGeo, world.gridAddGeo);

							selectedGeoIsLine = false;
							loadObjectEditor();
						}
						// add vertex in the middle of a line
						else if (hoveredGeo != null && hoveredGeoIsLine)
						{
							VectorF[] vertices = new VectorF[hoveredGeo.Vertices.Length + 2];
							int v2 = (hoveredGeoVertex == hoveredGeo.Vertices.Length - 1) ? 0 : hoveredGeoVertex + 1;
							float dist = (float)Calc.getAdj(VectorF.Distance(cursorPos, hoveredGeo.Vertices[hoveredGeoVertex]), Calc.LinePointDistance(cursorPos, hoveredGeo.Vertices[hoveredGeoVertex], hoveredGeo.Vertices[v2]));
							Vector2 dir = Vector2.Normalize((Vector2)(hoveredGeo.Vertices[v2] - hoveredGeo.Vertices[hoveredGeoVertex]));
							VectorF pos = (VectorF)((Vector2)hoveredGeo.Vertices[hoveredGeoVertex] + (dir * dist));

							for (int i = 0; i <= hoveredGeoVertex; i++)
							{
								vertices[i] = hoveredGeo.Vertices[i];
							}

							if (hoveredGeoVertex == selectedGeo.Vertices.Length - 1)
							{
								vertices[hoveredGeoVertex + 1] = selectedGeo.Vertices[selectedGeo.Vertices.Length - 1];
								vertices[hoveredGeoVertex + 2] = pos;
							}
							else
							{
								vertices[hoveredGeoVertex + 1] = pos;
								vertices[hoveredGeoVertex + 2] = pos;

								for (int i = hoveredGeoVertex + 1; i < hoveredGeo.Vertices.Length; i++)
								{
									vertices[i + 2] = hoveredGeo.Vertices[i];
								}
							}

							world.Grid.Rect(hoveredGeo.UpperLeft, hoveredGeo.LowerRight, hoveredGeo, world.gridRemoveGeo);
							hoveredGeo.Vertices = vertices;
							hoveredGeo.refreshMath(new Vector2((float)tGeo.Width, (float)tGeo.Height));
							world.Grid.Rect(hoveredGeo.UpperLeft, hoveredGeo.LowerRight, hoveredGeo, world.gridAddGeo);

							hoveredGeoIsLine = false;
							hoveredGeoVertex += 2;
							selectedGeo = hoveredGeo;
							selectedGeoIsLine = false;
							selectedGeoVertex = hoveredGeoVertex;
							loadObjectEditor();
						}
						// add new geo
						else if (hoveredGeo == null)
						{
							Geo geo = new Geo(world);
							geo.Vertices = new VectorF[] { cursorPos };
							geo.CloseLoop = false;
							geo.Display = false;
							geo.refreshMath(new Vector2((float)tGeo.Width, (float)tGeo.Height));
							world.addGeo(geo);

							selectedGeo = geo;
							selectedGeoIsLine = false;
							selectedGeoVertex = 0;
							loadObjectEditor();
						}
					}
					else if (btnAddHotspot.Pressed && selectedNode == null && selectedSeg == null && selectedSegEnd == null && selectedGeo == null)
					{
						// add hotspot
						Hotspot hotspot = new Hotspot(world);
						hotspot.Pos = cursorPos;

						world.addHotspot(hotspot);
						selectedHotspot = hotspot;
						loadObjectEditor();
					}

					selectedSegEnd = null;
					isDragging = false;
				}
				// if the player just released the right mouse button
				else if (Inp.OldMse.RightButton == ButtonState.Pressed && Inp.Mse.RightButton == ButtonState.Released)
				{

				}
				// user just pressed the left mouse button
				else if (Inp.OldMse.LeftButton == ButtonState.Released && Inp.Mse.LeftButton == ButtonState.Pressed)
				{
					lastClickedPoint = world.Cam.screenToWorld(new Vector2((float)Inp.Mse.X, (float)Inp.Mse.Y));
					isDragging = false;

					// check for selected node
					selectedNode = hoveredNode;

					// check for selected segment end
					selectedSegEnd = (selectedNode == null)
						? hoveredSegEnd
						: null;

					// check for selected segment
					selectedSeg = (selectedNode == null && selectedSegEnd == null)
						? hoveredSeg
						: null;

					// check for selected hotspot
					selectedHotspot = (selectedNode == null && selectedSegEnd == null && selectedSeg == null)
						? hoveredHotspot
						: null;

					// check for selected geo
					if (selectedNode == null && selectedSeg == null && selectedSegEnd == null && selectedHotspot == null)
					{
						selectedGeo = hoveredGeo;
						selectedGeoIsLine = hoveredGeoIsLine;
						selectedGeoVertex = hoveredGeoVertex;
					}
					else
					{
						selectedGeo = null;
					}

					loadObjectEditor();
				}
				// user just pressed the right mouse button
				else if (Inp.OldMse.RightButton == ButtonState.Released && Inp.Mse.RightButton == ButtonState.Pressed)
				{

				}
				// if the left mouse button is still pressed
				else if (desktop.Focused == null && Inp.OldMse.LeftButton == ButtonState.Pressed && Inp.Mse.LeftButton == ButtonState.Pressed)
				{
					if (!isDragging && Vector2.Distance(world.Cam.worldToScreen(lastClickedPoint), new Vector2((float)Inp.Mse.X, (float)Inp.Mse.Y)) >= distToDrag)
					{
						isDragging = true;

						if (selectedNode != null)
							dragOffset = selectedNode.Pos - lastClickedPoint;
						else if (selectedSegEnd != null)
							dragOffset = selectedSegEnd.Pos - lastClickedPoint;
						else if (selectedSeg != null)
							dragOffset = selectedSeg.Nodes[0].Pos - lastClickedPoint;
						else if (selectedHotspot != null)
							dragOffset = selectedHotspot.Pos - lastClickedPoint;
						else if (selectedGeo != null)
						{
							if (selectedGeoVertex == -1)
								dragOffset = selectedGeo.Center - lastClickedPoint;
							else
								dragOffset = selectedGeo.Vertices[selectedGeoVertex] - lastClickedPoint;
						}
					}
				}
				// if [Del] key is pressed
				else if (!Inp.OldKey.IsKeyDown(Keys.Delete) && Inp.Key.IsKeyDown(Keys.Delete))
				{
					if (selectedNode != null)
						removeSelNode();
					else if (selectedSeg != null)
						removeSelSeg();
					else if (selectedHotspot != null)
						removeSelHotspot();
					else if (selectedGeo != null)
						removeSelGeo();
				}

				// drag
				if (isDragging)
				{
					// move node
					if (selectedNode != null && !btnAddSeg.Pressed && (!btnAddNode.Pressed || cmbEditorAddNodeType.SelectedIndex < 0))
					{
						moveNode(selectedNode, cursorPos + dragOffset);
					}
					// move segment end
					else if (selectedSegEnd != null)
					{
						moveNode(selectedSegEnd, cursorPos + dragOffset);
					}
					// move segment
					else if (selectedSeg != null)
					{
						VectorF moveVect = cursorPos + dragOffset - selectedSeg.Nodes[0].Pos;
						moveNode(selectedSeg.Nodes[0], selectedSeg.Nodes[0].Pos + moveVect);
						moveNode(selectedSeg.Nodes[1], selectedSeg.Nodes[1].Pos + moveVect);
					}
					// move hotspot
					else if (selectedHotspot != null)
					{
						moveHotspot(selectedHotspot, cursorPos + dragOffset);
					}
					// move geo/vertex
					else if (selectedGeo != null && !btnAddGeo.Pressed)
					{
						world.Grid.Rect(selectedGeo.UpperLeft, selectedGeo.LowerRight, selectedGeo, world.gridRemoveGeo);
						if (selectedGeoIsLine)
						{
							if (selectedGeoVertex == selectedGeo.Vertices.Length - 1)
							{
								selectedGeo.Vertices[0] += cursorPos + dragOffset - selectedGeo.Vertices[selectedGeoVertex];
							}
							else
							{
								if (selectedGeoVertex != 0)
									selectedGeo.Vertices[selectedGeoVertex - 1] = cursorPos + dragOffset;

								selectedGeo.Vertices[selectedGeoVertex + 1] += cursorPos + dragOffset - selectedGeo.Vertices[selectedGeoVertex];

								if (selectedGeoVertex != selectedGeo.Vertices.Length - 2)
									selectedGeo.Vertices[selectedGeoVertex + 2] += cursorPos + dragOffset - selectedGeo.Vertices[selectedGeoVertex];
							}
							selectedGeo.Vertices[selectedGeoVertex] = cursorPos + dragOffset;
						}
						else if (selectedGeoVertex == -1)
						{
							VectorF toMove = cursorPos + dragOffset - selectedGeo.Center;
							for (int i = 0; i < selectedGeo.Vertices.Length; i++)
							{
								selectedGeo.Vertices[i] += toMove;
							}
						}
						else
						{
							if (selectedGeoVertex != 0 && selectedGeoVertex != selectedGeo.Vertices.Length - 1)
								selectedGeo.Vertices[selectedGeoVertex - 1] = cursorPos + dragOffset;
							selectedGeo.Vertices[selectedGeoVertex] = cursorPos + dragOffset;
						}
						selectedGeo.refreshMath(new Vector2((float)tGeo.Width, (float)tGeo.Height));
						world.Grid.Rect(selectedGeo.UpperLeft, selectedGeo.LowerRight, selectedGeo, world.gridAddGeo);
					}
				}
			}
		}

		/// <summary>Draws this mode.</summary>
		/// <param name="gameTime">The current game time.</param>
		public override void Draw(GameTime gameTime)
		{
			bool drawNewNode = Inp.Mse.LeftButton == ButtonState.Pressed && desktop.Focused == null && btnAddNode.Pressed && cmbEditorAddNodeType.SelectedIndex > -1 && selectedSeg == null && selectedSegEnd == null && selectedGeo == null && selectedHotspot == null;

			Graphics.GraphicsDevice.Clear(Color.Black);

			// get cursor world coordinates
			VectorF cursorPos = world.Cam.screenToWorld(new Vector2((float)Inp.Mse.X, (float)Inp.Mse.Y));

			Matrix scaleMatrix = Matrix.CreateScale((float)world.Cam.Zoom);
			Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, scaleMatrix);

			// draw background
			Batch.Draw(tSegment, world.Cam.worldToScreenDraw(world.Size / FInt.F2), null, Color.DarkBlue, 0f, oSegment, (Vector2)world.Size, SpriteEffects.None, 0f);

			// draw gridsquares
			for (int i = 1; i < world.Grid.NumCols; i++)
			{
				Vector2 pos = world.Cam.worldToScreenDraw(new VectorF((FInt)i * world.Grid.SqrWidth, world.Height / FInt.F2));
				Batch.Draw(tSegment, new Vector2(Calc.Round(pos.X), Calc.Round(pos.Y)), null, Color.Black, 0f, oSegment, new Vector2(1f / (float)world.Cam.Zoom, (float)world.Height), SpriteEffects.None, 0f);
			}

			for (int i = 1; i < world.Grid.NumRows; i++)
			{
				Vector2 pos = world.Cam.worldToScreenDraw(new VectorF(world.Width / FInt.F2, (FInt)i * world.Grid.SqrHeight));
				Batch.Draw(tSegment, new Vector2(Calc.Round(pos.X), Calc.Round(pos.Y)), null, Color.Black, 0f, oSegment, new Vector2((float)world.Width, 1f / (float)world.Cam.Zoom), SpriteEffects.None, 0f);
			}

			// draw node spacing
			{
				Color spcColor = new Color(Color.White, .25f);

				Player player = (selectedNode != null) ? selectedNode.Owner : null;

				// if creating a new node
				if (drawNewNode)
				{
					if (hoveredSegEnd != null && (selectedNode == null || selectedNode.Owner == hoveredSegEndOwner))
					{
						Batch.Draw(tSpacing, world.Cam.worldToScreenDraw(hoveredSegEnd.Pos), null, spcColor, 0f, oSpacing, (float)(world.NodeTypes[cmbEditorAddNodeType.SelectedIndex].Spacing + world.NodeTypes[cmbEditorAddNodeType.SelectedIndex].Radius) * 2f / (float)tSpacing.Width, SpriteEffects.None, 0f);
						player = hoveredSegEndOwner;
					}
					else if (selectedNode == null || isDragging)
					{
						Batch.Draw(tSpacing, world.Cam.worldToScreenDraw(cursorPos), null, spcColor, 0f, oSpacing, (float)(world.NodeTypes[cmbEditorAddNodeType.SelectedIndex].Spacing + world.NodeTypes[cmbEditorAddNodeType.SelectedIndex].Radius) * 2f / (float)tSpacing.Width, SpriteEffects.None, 0f);

						if (cmbEditorAddNodeOwner.SelectedIndex > 0)
							player = (selectedNode != null && isDragging) ? selectedNode.Owner : world.Players[cmbEditorAddNodeOwner.SelectedIndex - 1];
					}
				}

				if (player != null)
				{
					foreach (Node node in player.Nodes)
					{
						Batch.Draw(tSpacing, world.Cam.worldToScreenDraw(node.Pos), null, spcColor, 0f, oSpacing, (float)(node.Spacing + node.Radius) * 2f / (float)tSpacing.Width, SpriteEffects.None, 0f);
					}
				}

				// draw node type thresholds
				if (selectedNode != null)
				{
					Vector2 nPos = world.Cam.worldToScreenDraw(selectedNode.Pos);
					spcColor = new Color(Color.Black, .25f);
					foreach (NodeType type in world.NodeTypes)
					{
						if (type.BuildRangeMin > 0)
							Batch.Draw(tSpacing, nPos, null, spcColor, 0f, oSpacing, (float)type.BuildRangeMin * 2f / (float)tSpacing.Width, SpriteEffects.None, 0f);
					}
				}
			}

			// draw hotspots
			foreach (Hotspot hotspot in world.Hotspots)
			{
				Color hotspotCol = Color.Orange;
				if (hotspot == selectedHotspot)
					hotspotCol = Color.Yellow;
				else if (hotspot == hoveredHotspot)
					hotspotCol = Color.White;

				Batch.Draw(tHotspot, world.Cam.worldToScreenDraw(hotspot.Pos), null, hotspotCol, 0f, oHotspot, 1f, SpriteEffects.None, 0f);
			}

			// draw segments
			foreach (Segment seg in world.Segments)
			{
				Color segColor = (seg == selectedSeg) ? Color.Green : (seg == hoveredSeg && hoveredSegEnd == null) ? Color.White : (seg.IsRetracting() ? Color.DarkCyan : Color.YellowGreen);
				Vector2 segPos = world.Cam.worldToScreenDraw((seg.EndLoc[0] + seg.EndLoc[1]) / FInt.F2);
				Batch.Draw(tSegment, segPos, null, segColor, (float)seg.Angle, oSegment, new Vector2(6f, (float)seg.CurLength), SpriteEffects.None, 0f);

				// draw people
				VectorF oppDir = seg.Direction * FInt.FN1;
				for (int i = 0; i < 2; i++)
				{
					VectorF dir = (i == 0) ? seg.Direction : oppDir;
					FInt minVal = seg.Length - seg.EndLength[1 - i];
					foreach (FInt person in seg.People[i])
					{
						Color col = (person >= minVal && person < seg.EndLength[i]) ? Color.White : Color.Red;
						Batch.Draw(tPerson, world.Cam.worldToScreenDraw((seg.Nodes[i].Pos + (dir * person)) + ((i == 0 ? FInt.FN1 : FInt.F1) * seg.DirectionPerp * FInt.F5)), null, col, 0f, oPerson, 1f, SpriteEffects.None, 0f);
					}
				}
			}

			// new seg
			if (Inp.Mse.LeftButton == ButtonState.Pressed
				&& desktop.Focused == null
				&& isDragging
				&& selectedSeg == null
				&& selectedSegEnd == null
				&& selectedGeo == null
				&& selectedHotspot == null
				&& ((btnAddNode.Pressed && cmbEditorAddNodeType.SelectedIndex > -1) || btnAddSeg.Pressed)
				&& ((selectedNode != null && selectedNode.NumSegments < selectedNode.Segments.Length) || (selectedNode == null && btnAddSeg.Pressed)))
			{
				VectorF fromPos = (selectedNode != null) ? selectedNode.Pos : lastClickedPoint;

				VectorF toPos = (btnAddSeg.Pressed && hoveredNode != null && (selectedNode == null || (hoveredNode != selectedNode && hoveredNode.Owner == selectedNode.Owner)) && hoveredNode.NumSegments != hoveredNode.Segments.Length && hoveredNode.relatedSeg(selectedNode) == -1)
					? hoveredNode.Pos
					: (btnAddNode.Pressed && hoveredSegEnd != null && (selectedNode == null || selectedNode.Owner == hoveredSegEndOwner))
					? hoveredSegEnd.Pos
					: cursorPos;

				Vector2 segPos = world.Cam.worldToScreenDraw((fromPos + toPos) / FInt.F2);
				float segAngle = Calc.FindAngle(new Vector2((float)toPos.X, (float)toPos.Y), new Vector2((float)fromPos.X, (float)fromPos.Y));
				FInt segLength = VectorF.Distance(fromPos, toPos);
				Batch.Draw(tSegment, segPos, null, Color.YellowGreen, (float)segAngle, oSegment, new Vector2(6f, (float)segLength), SpriteEffects.None, 0f);
			}

			// draw nodes
			foreach (Node node in world.Nodes)
			{
				Color nodeCol = Color.YellowGreen;
				if (node == selectedNode)
					nodeCol = Color.Green;
				else if (node == hoveredNode)
					nodeCol = Color.White;
				else if (!node.Active)
					nodeCol = Color.Gray;

				Batch.Draw(tNode, world.Cam.worldToScreenDraw(node.Pos), null, nodeCol, 0f, oNode, (float)node.Radius * 2f / (float)tNode.Width, SpriteEffects.None, 0f);
			}

			// new node
			if (drawNewNode)
			{
				if (hoveredSegEnd != null && (selectedNode == null || selectedNode.Owner == hoveredSegEndOwner))
					Batch.Draw(tNode, world.Cam.worldToScreenDraw(hoveredSegEnd.Pos), null, Color.YellowGreen, 0f, oNode, (float)world.NodeTypes[cmbEditorAddNodeType.SelectedIndex].Radius * 2f / (float)tNode.Width, SpriteEffects.None, 0f);
				else if (selectedNode == null || isDragging)
					Batch.Draw(tNode, world.Cam.worldToScreenDraw(cursorPos), null, Color.YellowGreen, 0f, oNode, (float)world.NodeTypes[cmbEditorAddNodeType.SelectedIndex].Radius * 2f / (float)tNode.Width, SpriteEffects.None, 0f);
			}

			// new hotspot
			if (Inp.Mse.LeftButton == ButtonState.Pressed && desktop.Focused == null && btnAddHotspot.Pressed && selectedNode == null && selectedSeg == null && selectedSegEnd == null && selectedGeo == null)
			{
				Batch.Draw(tHotspot, world.Cam.worldToScreenDraw(cursorPos), null, Color.Yellow, 0f, oHotspot, 1f, SpriteEffects.None, 0f);
			}

			Batch.End();

			// draw geo textures
			{
				List<VertexPositionTexture> gvs = new List<VertexPositionTexture>();
				foreach (Geo geo in world.Geos)
				{
					if (geo.Display)
						gvs.AddRange(geo.getTriangles(world.Cam));
				}

				if (gvs.Count > 0)
				{
					VertexPositionTexture[] gVertices = gvs.ToArray();

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
			}

			// draw geo edges
			{
				List<VertexPositionColor> gvs = new List<VertexPositionColor>();
				foreach (Geo geo in world.Geos)
				{
					bool allSel = geo == selectedGeo && selectedGeoVertex == -1;
					bool allHov = geo == hoveredGeo && hoveredGeoVertex == -1;
					bool maybeSel = selectedGeoIsLine && geo == selectedGeo;
					bool maybeHov = hoveredGeoIsLine && geo == hoveredGeo;

					if (geo.Vertices.Length > 1)
					{
						Color col;
						for (int i = 0, max = geo.Vertices.Length; i < max; i += 2)
						{
							col = (allSel || (maybeSel && selectedGeoVertex == i)) ? Color.Red : (allHov || (maybeHov && hoveredGeoVertex == i)) ? Color.Green : Color.White;
							gvs.Add(new VertexPositionColor(new Vector3(world.Cam.worldToScreenDraw(geo.Vertices[i]), 0f), col));
							gvs.Add(new VertexPositionColor(new Vector3(world.Cam.worldToScreenDraw(geo.Vertices[i + 1]), 0f), col));
						}

						if (geo.CloseLoop)
						{
							col = (allSel || (maybeSel && selectedGeoVertex == geo.Vertices.Length - 1)) ? Color.Red : (allHov || (maybeHov && hoveredGeoVertex == geo.Vertices.Length - 1)) ? Color.Green : Color.White;
							gvs.Add(new VertexPositionColor(new Vector3(world.Cam.worldToScreenDraw(geo.Vertices[geo.Vertices.Length - 1]), 0f), col));
							gvs.Add(new VertexPositionColor(new Vector3(world.Cam.worldToScreenDraw(geo.Vertices[0]), 0f), col));
						}

						// draw center
						if (geo.Vertices.Length > 2)
						{
							col = (allSel || (!selectedGeoIsLine && selectedGeo == geo && selectedGeoVertex == -1)) ? Color.Red : (allHov || (!hoveredGeoIsLine && hoveredGeo == geo && hoveredGeoVertex == -1)) ? Color.Green : Color.White;
							Vector2 center = world.Cam.worldToScreenDraw(geo.Center);
							gvs.Add(new VertexPositionColor(new Vector3(center.X - 10f, center.Y, 0f), col));
							gvs.Add(new VertexPositionColor(new Vector3(center.X + 10f, center.Y, 0f), col));
							gvs.Add(new VertexPositionColor(new Vector3(center.X, center.Y - 10f, 0f), col));
							gvs.Add(new VertexPositionColor(new Vector3(center.X, center.Y + 10f, 0f), col));
						}
					}
				}

				// new edge
				if (btnAddGeo.Pressed && Inp.Mse.LeftButton == ButtonState.Pressed && desktop.Focused == null && selectedGeo != null && !selectedGeoIsLine && (selectedGeoVertex == 0 || selectedGeoVertex == selectedGeo.Vertices.Length - 1) && selectedNode == null && selectedSeg == null && selectedSegEnd == null)
				{
					gvs.Add(new VertexPositionColor(new Vector3(world.Cam.worldToScreenDraw(selectedGeo.Vertices[selectedGeoVertex]), 0f), Color.White));
					gvs.Add(new VertexPositionColor(new Vector3(world.Cam.worldToScreenDraw(cursorPos), 0f), Color.White));
				}

				// draw lines
				if (gvs.Count > 0)
				{
					VertexPositionColor[] gVertices = gvs.ToArray();

					BEffect.View = scaleMatrix;
					BEffect.VertexColorEnabled = true;
					BEffect.TextureEnabled = false;
					BEffect.CurrentTechnique.Passes[0].Apply();
					Graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, gVertices, 0, gVertices.Length / 2);
				}
			}

			// draw geo vertices
			Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null);
			{
				foreach (Geo geo in world.Geos)
				{
					bool allSel = geo == selectedGeo && selectedGeoVertex == -1;
					bool allHov = geo == hoveredGeo && hoveredGeoVertex == -1;
					bool maybeSel = !selectedGeoIsLine && geo == selectedGeo;
					bool maybeHov = !hoveredGeoIsLine && geo == hoveredGeo;
					for (int i = 0; i < geo.Vertices.Length; i += 2)
					{
						Color col = (allSel || (maybeSel && i == selectedGeoVertex)) ? Color.Red : (allHov || (maybeHov && i == hoveredGeoVertex)) ? Color.Green : Color.White;
						Batch.Draw(tSpacing, world.Cam.worldToScreen(geo.Vertices[i]), null, col, 0f, oSpacing, 20f / (float)tSpacing.Width, SpriteEffects.None, 0f);
					}

					if (geo.Vertices.Length > 1)
					{
						int i = geo.Vertices.Length - 1;
						Color col = (allSel || (maybeSel && i == selectedGeoVertex)) ? Color.Red : (allHov || (maybeHov && i == hoveredGeoVertex)) ? Color.Green : Color.White;
						Batch.Draw(tSpacing, world.Cam.worldToScreen(geo.Vertices[i]), null, col, 0f, oSpacing, 20f / (float)tSpacing.Width, SpriteEffects.None, 0f);
					}
				}
			}

			// new vertex
			if (btnAddGeo.Pressed && Inp.Mse.LeftButton == ButtonState.Pressed && desktop.Focused == null && selectedNode == null && selectedSeg == null && selectedSegEnd == null && selectedHotspot == null)
			{
				Vector2 pos;

				if (hoveredGeo != null && hoveredGeoIsLine)
				{
					int v2 = (hoveredGeoVertex == hoveredGeo.Vertices.Length - 1) ? 0 : hoveredGeoVertex + 1;
					float dist = (float)Calc.getAdj(VectorF.Distance(cursorPos, hoveredGeo.Vertices[hoveredGeoVertex]), Calc.LinePointDistance(cursorPos, hoveredGeo.Vertices[hoveredGeoVertex], hoveredGeo.Vertices[v2]));
					Vector2 dir = Vector2.Normalize((Vector2)(hoveredGeo.Vertices[v2] - hoveredGeo.Vertices[hoveredGeoVertex]));
					pos = world.Cam.worldToScreen((VectorF)((Vector2)hoveredGeo.Vertices[hoveredGeoVertex] + (dir * dist)));
				}
				else
				{
					pos = new Vector2((float)Inp.Mse.X, (float)Inp.Mse.Y);
				}

				Batch.Draw(tSpacing, pos, null, Color.White, 0f, oSpacing, 20f / (float)tSpacing.Width, SpriteEffects.None, 0f);
			}

			Batch.End();

			// draw gui
			lblCursorPos.Text = "Screen: " + Inp.Mse.X.ToString() + "x" + Inp.Mse.Y.ToString() + "\nWorld: " + cursorPos.X.ToString() + "x" + cursorPos.Y.ToString();
			Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, desktop.RState, null, null);
			desktop.Draw(Batch);
			Batch.End();
		}

		#endregion Core Game Methods

		#region Events

		#region TextBoxes

		/// <summary>Called when a TextBox receives focus.</summary>
		private void textBox_FocusReceived(object sender, EventArgs e)
		{
			curTxtVal = ((TextBox)sender).Text;
		}

		/// <summary>Called when a TextBox loses focus.</summary>
		private void textBox_FocusLost(object sender, EventArgs e)
		{
			TextBox txt = (TextBox)sender;
			int tempInt = 0;
			double tempDbl = 0d;

			if (txt == txtWorldWidth)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.Width)
				{
					world.Width = (FInt)tempDbl;
					world.Grid = new GridManager(world.Grid.NumCols, world.Grid.NumRows, world);
					world.refreshGrid();
					foreach (Player p in world.Players)
					{
						p.Fog = new FogOfWar(world.FogRows, world.FogCols, world.Size, p);
					}
				}
			}
			else if (txt == txtWorldHeight)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.Height)
				{
					world.Height = (FInt)tempDbl;
					world.Grid = new GridManager(world.Grid.NumCols, world.Grid.NumRows, world);
					world.refreshGrid();
					foreach (Player p in world.Players)
					{
						p.Fog = new FogOfWar(world.FogRows, world.FogCols, world.Size, p);
					}
				}
			}
			else if (txt == txtPersonSpacing)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.PersonSpacing)
					world.PersonSpacing = (FInt)tempDbl;
			}
			else if (txt == txtPersonSpeedLower)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.PersonSpeedLower)
					world.PersonSpeedLower = (FInt)tempDbl;
			}
			else if (txt == txtPersonSpeedUpper)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.PersonSpeedUpper)
					world.PersonSpeedUpper = (FInt)tempDbl;
			}
			else if (txt == txtRetractSpeed)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.RetractSpeed)
					world.RetractSpeed = (FInt)tempDbl;
			}
			else if (txt == txtBuildRate)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.BuildRate)
					world.BuildRate = (FInt)tempDbl;
			}
			else if (txt == txtCamWidth)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.Cam.Width)
					world.Cam.Width = (FInt)tempDbl;
			}
			else if (txt == txtCamHeight)
			{
				enforceDouble(txt, curTxtVal, 1d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.Cam.Height)
					world.Cam.Height = (FInt)tempDbl;
			}
			else if (txt == txtCamX)
			{
				enforceDouble(txt, curTxtVal, 0d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.Cam.CenterX)
					world.Cam.CenterX = (FInt)tempDbl;
			}
			else if (txt == txtCamY)
			{
				enforceDouble(txt, curTxtVal, 0d);
				double.TryParse(txt.Text, out tempDbl);
				if ((FInt)tempDbl != world.Cam.CenterY)
					world.Cam.CenterY = (FInt)tempDbl;
			}
			else if (txt == txtGridRows)
			{
				enforceInt(txt, curTxtVal, 1);
				int.TryParse(txt.Text, out tempInt);
				if (tempInt != world.Grid.NumRows)
				{
					world.Grid = new GridManager(world.Grid.NumCols, tempInt, world);
					world.refreshGrid();
				}
			}
			else if (txt == txtGridCols)
			{
				enforceInt(txt, curTxtVal, 1);
				int.TryParse(txt.Text, out tempInt);
				if (tempInt != world.Grid.NumCols)
				{
					world.Grid = new GridManager(tempInt, world.Grid.NumRows, world);
					world.refreshGrid();
				}
			}
			else if (txt == txtFogRows)
			{
				enforceInt(txt, curTxtVal, 1);
				int.TryParse(txt.Text, out tempInt);
				if (tempInt != world.FogRows)
				{
					world.FogRows = tempInt;
					foreach (Player p in world.Players)
					{
						p.Fog = new FogOfWar(world.FogRows, world.FogCols, world.Size, p);
					}
				}
			}
			else if (txt == txtFogCols)
			{
				enforceInt(txt, curTxtVal, 1);
				int.TryParse(txt.Text, out tempInt);
				if (tempInt != world.FogCols)
				{
					world.FogCols = tempInt;
					foreach (Player p in world.Players)
					{
						p.Fog = new FogOfWar(world.FogRows, world.FogCols, world.Size, p);
					}
				}
			}
			else if (txt == txtPathRows)
			{
				enforceInt(txt, curTxtVal, 1);
				int.TryParse(txt.Text, out tempInt);
				if (tempInt != world.PathRows)
					world.PathRows = tempInt;
			}
			else if (txt == txtPathCols)
			{
				enforceInt(txt, curTxtVal, 1);
				int.TryParse(txt.Text, out tempInt);
				if (tempInt != world.PathCols)
					world.PathCols = tempInt;
			}
			else if (txt == txtNodeTypeName)
			{
				txt.Text = txt.Text.Trim();
			}
			else if (txt == txtNodeTypeNumSegs)
			{
				enforceInt(txt, curTxtVal, 1);
			}
			else if (txt == txtNodeTypeRadius
				|| txt == txtNodeTypeSpacing)
			{
				enforceDouble(txt, curTxtVal, 1d);
			}
			else if (txt == txtNodeTypeGenSpacing)
			{
				enforceDouble(txt, curTxtVal, 0d);
			}
			else if (txt == txtNodeTypeSightDistance)
			{
				enforceDouble(txt, curTxtVal, 1d);
			}
			else if (txt == txtNodeTypeBuildRangeMin)
			{
				enforceDouble(txt, curTxtVal, 0d);
			}
			else if (txt == txtPlayerID)
			{
				txt.Text = txt.Text.Trim();
			}
			else if (txt == txtPlayerName)
			{
				txt.Text = txt.Text.Trim();
			}
			else if (txt == txtEditorNodeID)
			{
				txt.Text = txt.Text.Trim();
			}
			else if (txt == txtEditorNodeNumSegs)
			{
				enforceInt(txt, curTxtVal, 1);
			}
			else if (txt == txtEditorNodeRadius
				|| txt == txtEditorNodeSpacing
				|| txt == txtEditorNodeGenSpacing
				|| txt == txtEditorNodeGenCountDown
				|| txt == txtEditorNodeSightDist)
			{
				enforceDouble(txt, curTxtVal, 1d);
			}
			else if (txt == txtEditorNodeX
				|| txt == txtEditorNodeY
				|| txt == txtEditorSegEndLen0
				|| txt == txtEditorSegEndLen1)
			{
				enforceDouble(txt, curTxtVal, 0d);
			}
			else if (txt == txtEditorSegLanePeople0
				|| txt == txtEditorSegLanePeople1)
			{
				validatePpl(txt, curTxtVal);
			}
			else if (txt == txtEditorNodeOwnsHotspot)
			{
				txt.Text = txt.Text.Trim();
			}
			else if (txt == txtEditorHotspotX
				|| txt == txtEditorHotspotY)
			{
				enforceDouble(txt, curTxtVal, 0d);
			}
		}

		#endregion TextBoxes
		#region Tabs

		/// <summary>Called when btnShowWorldPnl is clicked.</summary>
		private void btnShowWorldPnl_MouseLeftUp(object sender, EventArgs e)
		{
			ButtonText btn = (ButtonText)sender;

			if (btn.Pressed)
			{
				btnShowScriptsPnl.Pressed = false;
				btnShowNodeTypesPnl.Pressed = false;
				btnShowPlayersPnl.Pressed = false;
				btnShowEditorPnl.Pressed = false;

				pnlScripts.Visible = false;
				pnlNodeTypes.Visible = false;
				pnlPlayers.Visible = false;
				pnlEditor.Visible = false;
			}

			pnlSettings.Visible = btn.Pressed;
		}

		/// <summary>Called when btnShowScriptsPnl is clicked.</summary>
		private void btnShowScriptsPnl_MouseLeftUp(object sender, EventArgs e)
		{
			ButtonText btn = (ButtonText)sender;

			if (btn.Pressed)
			{
				btnShowSettingsPnl.Pressed = false;
				btnShowNodeTypesPnl.Pressed = false;
				btnShowPlayersPnl.Pressed = false;
				btnShowEditorPnl.Pressed = false;

				pnlSettings.Visible = false;
				pnlNodeTypes.Visible = false;
				pnlPlayers.Visible = false;
				pnlEditor.Visible = false;
			}

			pnlScripts.Visible = btn.Pressed;
		}

		/// <summary>Called when btnShowNodeTypesPnl is clicked.</summary>
		private void btnShowNodeTypesPnl_MouseLeftUp(object sender, EventArgs e)
		{
			ButtonText btn = (ButtonText)sender;

			if (btn.Pressed)
			{
				btnShowSettingsPnl.Pressed = false;
				btnShowScriptsPnl.Pressed = false;
				btnShowPlayersPnl.Pressed = false;
				btnShowEditorPnl.Pressed = false;

				pnlSettings.Visible = false;
				pnlScripts.Visible = false;
				pnlPlayers.Visible = false;
				pnlEditor.Visible = false;
			}

			pnlNodeTypes.Visible = btn.Pressed;
		}

		/// <summary>Called when btnShowPlayersPnl is clicked.</summary>
		private void btnShowPlayersPnl_MouseLeftUp(object sender, EventArgs e)
		{
			ButtonText btn = (ButtonText)sender;

			if (btn.Pressed)
			{
				btnShowSettingsPnl.Pressed = false;
				btnShowScriptsPnl.Pressed = false;
				btnShowNodeTypesPnl.Pressed = false;
				btnShowEditorPnl.Pressed = false;

				pnlSettings.Visible = false;
				pnlScripts.Visible = false;
				pnlNodeTypes.Visible = false;
				pnlEditor.Visible = false;
			}

			pnlPlayers.Visible = btn.Pressed;
		}

		/// <summary>Called when btnShowEditorPnl is clicked.</summary>
		private void btnShowEditorPnl_MouseLeftUp(object sender, EventArgs e)
		{
			ButtonText btn = (ButtonText)sender;

			if (btn.Pressed)
			{
				btnShowSettingsPnl.Pressed = false;
				btnShowScriptsPnl.Pressed = false;
				btnShowNodeTypesPnl.Pressed = false;
				btnShowPlayersPnl.Pressed = false;

				pnlSettings.Visible = false;
				pnlScripts.Visible = false;
				pnlNodeTypes.Visible = false;
				pnlPlayers.Visible = false;
			}

			pnlEditor.Visible = btn.Pressed;
		}

		#endregion Tabs
		#region Scripts

		/// <summary>Called when btnScriptsBeginUpdateScript is clicked.</summary>
		private void btnScriptsBeginUpdateScript_MouseLeftUp(object sender, EventArgs e)
		{
			showTextEditor("Begin Update Script");
			txtTextEditorText.Text = world.ScriptBeginUpdate;
		}

		/// <summary>Called when btnTextEditorOK is clicked.</summary>
		private void btnTextEditorOK_MouseLeftUp_BeginUpdateScript(object sender, EventArgs e)
		{
			if (lblTextEditorTitle.Text != "Begin Update Script")
				return;

			world.ScriptBeginUpdate = txtTextEditorText.Text.Trim();
			lblScriptsBeginUpdateScriptView.Text = string.IsNullOrWhiteSpace(world.ScriptBeginUpdate) ? "---" : "###";
		}

		#endregion Scripts
		#region Node Types

		/// <summary>Called when btnNodeTypeLoad is clicked.</summary>
		private void btnNodeTypeLoad_MouseLeftUp(object sender, EventArgs e)
		{
			if (cmbNodeTypes.SelectedIndex < 0)
			{
				showMsg("Select a node type first.");
				return;
			}

			loadSelNodeType();
		}

		/// <summary>Called when btnNodeTypeUpdate is clicked.</summary>
		private void btnNodeTypeUpdate_MouseLeftUp(object sender, EventArgs e)
		{
			// make sure a node type is selected
			if (cmbNodeTypes.SelectedIndex < 0)
			{
				showMsg("Select a node type first.");
				return;
			}

			// make sure the name isn't blank
			if (string.IsNullOrWhiteSpace(txtNodeTypeName.Text))
			{
				showMsg("Name cannot be blank");
				return;
			}

			// make sure the name isn't a duplicate
			for (int i = 0; i < world.NodeTypes.Count; i++)
			{
				if (i == cmbNodeTypes.SelectedIndex)
					continue;

				if (world.NodeTypes[i].Name == txtNodeTypeName.Text)
				{
					showMsg("A node type with that name already exists.");
					return;
				}
			}

			NodeType nt = world.NodeTypes[cmbNodeTypes.SelectedIndex];

			if (nt.Name != txtNodeTypeName.Text)
			{
				// update other lists first
				cmbEditorAddNodeType.Menu[cmbNodeTypes.SelectedIndex] = txtNodeTypeName.Text;
				cmbEditorNodeType.Menu[cmbNodeTypes.SelectedIndex] = txtNodeTypeName.Text;

				cmbNodeTypes.Menu[cmbNodeTypes.SelectedIndex] = txtNodeTypeName.Text;
			}

			nt.Name = txtNodeTypeName.Text;
			nt.IsParent = btnNodeTypeIsParent.Pressed;
			nt.NumSegments = int.Parse(txtNodeTypeNumSegs.Text);
			nt.Radius = (FInt)double.Parse(txtNodeTypeRadius.Text);
			nt.Spacing = (FInt)double.Parse(txtNodeTypeSpacing.Text);
			nt.GenSpacing = (FInt)double.Parse(txtNodeTypeGenSpacing.Text);
			nt.SightDistance = (FInt)double.Parse(txtNodeTypeSightDistance.Text);
			nt.BuildRangeMin = (FInt)double.Parse(txtNodeTypeBuildRangeMin.Text);
		}

		/// <summary>Called when btnNodeTypeAddNew is clicked.</summary>
		private void btnNodeTypeAddNew_MouseLeftUp(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtNodeTypeName.Text))
			{
				showMsg("Name cannot be blank.");
				return;
			}

			for (int i = 0; i < world.NodeTypes.Count; i++)
			{
				if (world.NodeTypes[i].Name == txtNodeTypeName.Text)
				{
					showMsg("A node type with that name already exists.");
					return;
				}
			}

			NodeType nt = new NodeType();
			nt.Name = txtNodeTypeName.Text;
			nt.IsParent = btnNodeTypeIsParent.Pressed;
			nt.NumSegments = int.Parse(txtNodeTypeNumSegs.Text);
			nt.Radius = (FInt)double.Parse(txtNodeTypeRadius.Text);
			nt.Spacing = (FInt)double.Parse(txtNodeTypeSpacing.Text);
			nt.GenSpacing = (FInt)double.Parse(txtNodeTypeGenSpacing.Text);
			nt.SightDistance = (FInt)double.Parse(txtNodeTypeSightDistance.Text);
			nt.BuildRangeMin = (FInt)double.Parse(txtNodeTypeBuildRangeMin.Text);

			world.NodeTypes.Add(nt);
			cmbNodeTypes.Menu.addItem(nt.Name);
			cmbNodeTypes.SelectedIndex = cmbNodeTypes.Menu.Count - 1;

			// update other lists
			cmbEditorAddNodeType.Menu.addItem(nt.Name);
			cmbEditorNodeType.Menu.addItem(nt.Name);
		}

		/// <summary>Called when btnNodeTypeDelete is clicked.</summary>
		private void btnNodeTypeDelete_MouseLeftUp(object sender, EventArgs e)
		{
			// make sure a node type is selected
			if (cmbNodeTypes.SelectedIndex < 0)
			{
				showMsg("Select a node type first.");
				return;
			}

			// make sure the node type isn't in use
			NodeType nt = world.NodeTypes[cmbNodeTypes.SelectedIndex];
			foreach (Node n in world.Nodes)
			{
				if (n.NType == nt)
				{
					showMsg("Cannot delete a node type that is in use.");
					return;
				}
			}

			// update other lists first
			cmbEditorAddNodeType.Menu.removeAt(cmbNodeTypes.SelectedIndex);
			cmbEditorNodeType.Menu.removeAt(cmbNodeTypes.SelectedIndex);

			world.NodeTypes.RemoveAt(cmbNodeTypes.SelectedIndex);
			cmbNodeTypes.Menu.removeAt(cmbNodeTypes.SelectedIndex);
			loadSelNodeType();
		}

		#endregion Node Types
		#region Players

		/// <summary>Called when btnPlayerLoad is clicked.</summary>
		private void btnPlayerLoad_MouseLeftUp(object sender, EventArgs e)
		{
			if (cmbPlayers.SelectedIndex < 0)
			{
				showMsg("Select a player first.");
				return;
			}

			loadSelPlayer();
		}

		/// <summary>Called when btnPlayerUpdate is clicked.</summary>
		private void btnPlayerUpdate_MouseLeftUp(object sender, EventArgs e)
		{
			// make sure a player is selected
			if (cmbPlayers.SelectedIndex < 0)
			{
				showMsg("Select a player first.");
				return;
			}

			// make sure the ID is valid
			if (string.IsNullOrWhiteSpace(txtPlayerID.Text))
			{
				showMsg("ID cannot be blank.");
				return;
			}

			// make sure the ID and name aren't a duplicates
			for (int i = 0; i < world.Players.Count; i++)
			{
				if (i == cmbPlayers.SelectedIndex)
					continue;

				if (world.Players[i].ID == txtPlayerID.Text)
				{
					showMsg("A player with that ID already exists.");
					return;
				}

				if (world.Players[i].Name == txtPlayerName.Text)
				{
					showMsg("A player with that name already exists.");
					return;
				}
			}
			
			Player player = world.Players[cmbPlayers.SelectedIndex];

			if (player.Name != txtPlayerName.Text)
			{
				// update other lists first
				string nm = string.IsNullOrEmpty(txtPlayerName.Text) ? ("[Unnamed " + player.Type.ToString() + " " + player.ID + "]") : txtPlayerName.Text;
				cmbEditorAddNodeOwner.Menu[cmbPlayers.SelectedIndex + 1] = nm;
				cmbEditorAddSegOwner.Menu[cmbPlayers.SelectedIndex + 1] = nm;
				cmbEditorNodeOwner.Menu[cmbPlayers.SelectedIndex + 1] = nm;
				cmbEditorSegOwner.Menu[cmbPlayers.SelectedIndex + 1] = nm;

				cmbPlayers.Menu[cmbPlayers.SelectedIndex] = nm;
			}

			player.ID = txtPlayerID.Text;
			player.Name = txtPlayerName.Text;
			player.Type = (Player.PlayerType)cmbPlayerTypes.SelectedIndex;

			world.refreshNextGenIDs();
		}

		/// <summary>Called when btnPlayerAddNew is clicked.</summary>
		private void btnPlayerAddNew_MouseLeftUp(object sender, EventArgs e)
		{
			// make sure the ID and name aren't duplicates
			foreach (Player p in world.Players)
			{
				if (p.ID == txtPlayerID.Text)
				{
					showMsg("A player with that ID already exists.");
					return;
				}

				if (!string.IsNullOrEmpty(p.Name) && p.Name == txtPlayerName.Text)
				{
					showMsg("A player with that name already exists.");
					return;
				}
			}
			
			Player player = new Player(world);
			player.Name = txtPlayerName.Text;
			player.Type = (Player.PlayerType)cmbPlayerTypes.SelectedIndex;
			player.Fog = new FogOfWar(world.FogRows, world.FogCols, world.Size, player);
			
			world.Players.Add(player);

			Dictionary<string, WorldEvent.EventType> nev = null;
			Dictionary<string, WorldEvent.EventType> sev = null;
			if (player.Type == Player.PlayerType.Computer)
			{
				nev = new Dictionary<string, WorldEvent.EventType>();
				sev = new Dictionary<string, WorldEvent.EventType>();
			}
			world.SegEvents.Add(nev);
			world.NodeEvents.Add(sev);

			if (string.IsNullOrWhiteSpace(txtPlayerID.Text))
			{
				player.ID = world.getNextPlayerID();
				txtPlayerID.Text = player.ID;
			}
			else
			{
				player.ID = txtPlayerID.Text;
				world.refreshNextGenIDs();
			}

			string nm = string.IsNullOrEmpty(player.Name) ? ("[Unnamed " + player.Type.ToString() + " " + player.ID + "]") : player.Name;

			cmbPlayers.Menu.addItem(nm);
			cmbPlayers.SelectedIndex = cmbPlayers.Menu.Count - 1;

			// update other lists
			cmbEditorAddNodeOwner.Menu.addItem(nm);
			cmbEditorAddSegOwner.Menu.addItem(nm);
			cmbEditorNodeOwner.Menu.addItem(nm);
			cmbEditorSegOwner.Menu.addItem(nm);
		}

		/// <summary>Called when btnPlayerDelete is clicked.</summary>
		private void btnPlayerDelete_MouseLeftUp(object sender, EventArgs e)
		{
			// make sure a player is selected
			if (cmbPlayers.SelectedIndex < 0)
			{
				showMsg("Select a player first.");
				return;
			}

			// make sure the player isn't in use
			bool inUse = false;
			Player player = world.Players[cmbPlayers.SelectedIndex];
			foreach (Node n in world.Nodes)
			{
				if (n.Owner == player)
				{
					inUse = true;
					break;
				}
			}
			if (!inUse)
			{
				foreach (Segment s in world.Segments)
				{
					if (s.Owner == player)
					{
						inUse = true;
						break;
					}
				}
			}
			if (inUse)
			{
				showMsg("Cannot delete a player that is in use.");
				return;
			}

			// update other lists first
			cmbEditorAddNodeOwner.Menu.removeAt(cmbPlayers.SelectedIndex + 1);
			cmbEditorAddSegOwner.Menu.removeAt(cmbPlayers.SelectedIndex + 1);
			cmbEditorNodeOwner.Menu.removeAt(cmbPlayers.SelectedIndex + 1);
			cmbEditorSegOwner.Menu.removeAt(cmbPlayers.SelectedIndex + 1);
			world.NodeEvents.RemoveAt(cmbPlayers.SelectedIndex);
			world.SegEvents.RemoveAt(cmbPlayers.SelectedIndex);

			world.Players.RemoveAt(cmbPlayers.SelectedIndex);
			cmbPlayers.Menu.removeAt(cmbPlayers.SelectedIndex);
			loadSelPlayer();
		}

		#endregion Players
		#region Editor

		/// <summary>Called when btnAddNode is clicked.</summary>
		private void btnAddNode_MouseLeftUp(object sender, EventArgs e)
		{
			btnAddSeg.Pressed = false;
			btnAddGeo.Pressed = false;
			btnAddHotspot.Pressed = false;

			pnlEditorAddNodes.Visible = btnAddNode.Pressed;
			pnlEditorAddSegs.Visible = false;
			pnlEditorAddGeos.Visible = false;
			pnlEditorAddHotspots.Visible = false;

			ctlEditorDivider.Top = btnAddNode.Pressed ? pnlEditorAddNodes.Bottom + 10 : pnlEditorAddNodes.Top;

			pnlEditorNodes.Top =
			pnlEditorSegs.Top =
			pnlEditorGeos.Top =
			pnlEditorHotspots.Top = ctlEditorDivider.Bottom + 10;
		}

		/// <summary>Called when btnAddSeg is clicked.</summary>
		private void btnAddSeg_MouseLeftUp(object sender, EventArgs e)
		{
			btnAddNode.Pressed = false;
			btnAddGeo.Pressed = false;
			btnAddHotspot.Pressed = false;

			pnlEditorAddNodes.Visible = false;
			pnlEditorAddSegs.Visible = btnAddSeg.Pressed;
			pnlEditorAddGeos.Visible = false;
			pnlEditorAddHotspots.Visible = false;

			ctlEditorDivider.Top = btnAddSeg.Pressed ? pnlEditorAddSegs.Bottom + 10 : pnlEditorAddSegs.Top;

			pnlEditorNodes.Top =
			pnlEditorSegs.Top =
			pnlEditorGeos.Top =
			pnlEditorHotspots.Top = ctlEditorDivider.Bottom + 10;
		}

		/// <summary>Called when btnAddGeo is clicked.</summary>
		private void btnAddGeo_MouseLeftUp(object sender, EventArgs e)
		{
			btnAddNode.Pressed = false;
			btnAddSeg.Pressed = false;
			btnAddHotspot.Pressed = false;

			pnlEditorAddNodes.Visible = false;
			pnlEditorAddSegs.Visible = false;
			pnlEditorAddGeos.Visible = btnAddGeo.Pressed;
			pnlEditorAddHotspots.Visible = false;

			ctlEditorDivider.Top = pnlEditorAddGeos.Top;

			pnlEditorNodes.Top =
			pnlEditorSegs.Top =
			pnlEditorGeos.Top =
			pnlEditorHotspots.Top = ctlEditorDivider.Bottom + 10;
		}

		/// <summary>Called when btnAddHotspot is clicked.</summary>
		private void btnAddHotspot_MouseLeftUp(object sender, EventArgs e)
		{
			btnAddNode.Pressed = false;
			btnAddSeg.Pressed = false;
			btnAddGeo.Pressed = false;

			pnlEditorAddNodes.Visible = false;
			pnlEditorAddSegs.Visible = false;
			pnlEditorAddGeos.Visible = false;
			pnlEditorAddHotspots.Visible = btnAddHotspot.Pressed;

			ctlEditorDivider.Top = pnlEditorAddHotspots.Top;

			pnlEditorNodes.Top =
			pnlEditorSegs.Top =
			pnlEditorGeos.Top =
			pnlEditorHotspots.Top = ctlEditorDivider.Bottom + 10;
		}

		/// <summary>Called when btnEditorNodeLoadType is clicked.</summary>
		private void btnEditorNodeLoadType_MouseLeftUp(object sender, EventArgs e)
		{
			if (cmbEditorNodeType.SelectedIndex < 0)
			{
				showMsg("Select a node type first.");
				return;
			}

			NodeType nt = world.NodeTypes[cmbEditorNodeType.SelectedIndex];

			btnEditorNodeIsParent.Pressed = nt.IsParent;
			txtEditorNodeNumSegs.Text = nt.NumSegments.ToString();
			txtEditorNodeRadius.Text = nt.Radius.ToString();
			txtEditorNodeSpacing.Text = nt.Spacing.ToString();
			txtEditorNodeGenSpacing.Text = nt.GenSpacing.ToString();
			txtEditorNodeSightDist.Text = nt.SightDistance.ToString();
		}

		/// <summary>Called when btnEditorNodeApply is clicked.</summary>
		private void btnEditorNodeApply_MouseLeftUp(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtEditorNodeID.Text))
			{
				showMsg("The \"ID\" value cannot be blank.");
				return;
			}

			int numSegs = int.Parse(txtEditorNodeNumSegs.Text);
			if (numSegs < selectedNode.NumSegments)
			{
				showMsg("The \"NumSeg\" value must be greater than or equal to the number of segments already attached to the selected node.");
				return;
			}
			if (!string.IsNullOrWhiteSpace(txtEditorNodeOwnsHotspot.Text) && !world.HotspotByID.ContainsKey(txtEditorNodeOwnsHotspot.Text))
			{
				showMsg("The \"OwnsHotspot\" value must either be empty or be equal to an existing hotspot's ID.");
				return;
			}

			// make sure the node ID is not a duplicate
			foreach (Node n in world.Nodes)
			{
				if (n.ID == txtEditorNodeID.Text && n != selectedNode)
				{
					showMsg("A node with that ID already exists.");
					return;
				}
			}

			selectedNode.Owner = (cmbEditorNodeOwner.SelectedIndex == 0)
				? null
				: world.Players[cmbEditorNodeOwner.SelectedIndex - 1];

			if (selectedNode.NType != world.NodeTypes[cmbEditorNodeType.SelectedIndex])
				selectedNode.setNodeType(world.NodeTypes[cmbEditorNodeType.SelectedIndex]);

			selectedNode.IsParent = btnEditorNodeIsParent.Pressed;
			selectedNode.Radius = (FInt)double.Parse(txtEditorNodeRadius.Text);
			selectedNode.Spacing = (FInt)double.Parse(txtEditorNodeSpacing.Text);
			selectedNode.GenSpacing = (FInt)double.Parse(txtEditorNodeGenSpacing.Text);
			selectedNode.GenCountDown = (FInt)double.Parse(txtEditorNodeGenCountDown.Text);
			selectedNode.SightDistance = (FInt)double.Parse(txtEditorNodeSightDist.Text);
			selectedNode.Active = btnEditorNodeActive.Pressed;

			if (!string.IsNullOrWhiteSpace(txtEditorNodeOwnsHotspot.Text))
				selectedNode.OwnsHotspot = world.HotspotByID[txtEditorNodeOwnsHotspot.Text];

			if (numSegs != selectedNode.Segments.Length)
			{
				Segment[] segs = new Segment[numSegs];
				int[] segNumPpl = new int[numSegs];
				FInt[] segCap = new FInt[numSegs];

				int index = 0;
				for (int i = 0; i < selectedNode.Segments.Length; i++)
				{
					if (selectedNode.Segments[i] != null)
					{
						segs[index] = selectedNode.Segments[i];
						segNumPpl[index] = selectedNode.SegNumPeople[i];
						segCap[index] = selectedNode.SegCapacity[i];

						index++;
					}
				}
			}

			// move node
			if (selectedNode.X.ToString() != txtEditorNodeX.Text
				|| selectedNode.Y.ToString() != txtEditorNodeY.Text)
			{
				VectorF moveTo = new VectorF((FInt)double.Parse(txtEditorNodeX.Text), (FInt)double.Parse(txtEditorNodeY.Text));
				moveNode(selectedNode, moveTo);
			}

			// apply node ID
			if (selectedNode.ID != txtEditorNodeID.Text)
			{
				selectedNode.ID = txtEditorNodeID.Text;
				world.refreshNextGenIDs();
			}
		}

		/// <summary>Called when btnEditorNodeDel is clicked.</summary>
		private void btnEditorNodeDel_MouseLeftUp(object sender, EventArgs e)
		{
			removeSelNode();
		}

		/// <summary>Called when btnEditorSegDel is clicked.</summary>
		private void btnEditorSegDel_MouseLeftUp(object sender, EventArgs e)
		{
			removeSelSeg();
		}

		/// <summary>Called when btnEditorSegApply is clicked.</summary>
		private void btnEditorSegApply_MouseLeftUp(object sender, EventArgs e)
		{
			FInt end0 = (FInt)double.Parse(txtEditorSegEndLen0.Text);
			FInt end1 = (FInt)double.Parse(txtEditorSegEndLen1.Text);

			if (end0 + end1 < selectedSeg.Length)
			{
				showMsg("Sum of end lengths must be greater than or equal to the segment's total length.");
				return;
			}

			if ((SegmentSkel.SegState)cmbEditorSegEndState0.SelectedIndex == SegmentSkel.SegState.Complete && selectedSeg.Nodes[1].getSegIndex(selectedSeg) == -1)
			{
				if (!world.Nodes.Contains(selectedSeg.Nodes[1]))
				{
					showMsg("State0 cannot be complete without being connected to a node.");
					return;
				}
				else if (selectedSeg.Nodes[1].NumSegments == selectedSeg.Nodes[1].Segments.Length)
				{
					showMsg("Node 1 has no more free segment slots");
					return;
				}
			}

			if ((SegmentSkel.SegState)cmbEditorSegEndState1.SelectedIndex == SegmentSkel.SegState.Complete && selectedSeg.Nodes[0].getSegIndex(selectedSeg) == -1)
			{
				if (!world.Nodes.Contains(selectedSeg.Nodes[0]))
				{
					showMsg("State1 cannot be complete without being connected to a node.");
					return;
				}
				else if (selectedSeg.Nodes[0].NumSegments == selectedSeg.Nodes[0].Segments.Length)
				{
					showMsg("Node 0 has no more free segment slots");
					return;
				}
			}

			for (int i = 0; i < 2; i++)
			{
				int oppLane = 1 - i;
				bool contains = selectedSeg.Nodes[oppLane].getSegIndex(selectedSeg) != -1;
				if (selectedSeg.State[i] == SegmentSkel.SegState.Complete)
				{
					if (!contains)
						selectedSeg.Nodes[oppLane].addSegment(selectedSeg, false);
				}
				else if (contains)
				{
					selectedSeg.Nodes[oppLane].removeSegment(selectedSeg, false, false);
				}
			}

			selectedSeg.Owner = (cmbEditorSegOwner.SelectedIndex == 0) ? null : world.Players[cmbEditorSegOwner.SelectedIndex - 1];
			selectedSeg.EndLength[0] = end0;
			selectedSeg.EndLength[1] = end1;
			selectedSeg.State[0] = (SegmentSkel.SegState)cmbEditorSegEndState0.SelectedIndex;
			selectedSeg.State[1] = (SegmentSkel.SegState)cmbEditorSegEndState1.SelectedIndex;

			selectedSeg.People[0].Clear();
			selectedSeg.People[1].Clear();

			if (txtEditorSegLanePeople0.Text != string.Empty)
			{
				string[] ppl = txtEditorSegLanePeople0.Text.Split(',');
				foreach (string p in ppl)
				{
					selectedSeg.People[0].AddLast((FInt)double.Parse(p));
				}
			}

			if (txtEditorSegLanePeople1.Text != string.Empty)
			{
				string[] ppl = txtEditorSegLanePeople1.Text.Split(',');
				foreach (string p in ppl)
				{
					selectedSeg.People[1].AddLast((FInt)double.Parse(p));
				}
			}

			// update nodes
			for (int i = 0; i < 2; i++)
			{
				int oppLane = 1 - i;
				bool contains = selectedSeg.Nodes[oppLane].getSegIndex(selectedSeg) != -1;
				if (selectedSeg.State[i] == SegmentSkel.SegState.Complete)
				{
					if (!contains)
						selectedSeg.Nodes[oppLane].addSegment(selectedSeg, false);
				}
				else if (contains)
				{
					selectedSeg.Nodes[oppLane].removeSegment(selectedSeg, false, false);
				}
			}

			selectedSeg.refreshMath();
		}

		/// <summary>Called when btnEditorGeoApply is clicked.</summary>
		private void btnEditorGeoApply_MouseLeftUp(object sender, EventArgs e)
		{
			if (selectedGeo.Vertices.Length < 3 && btnEditorGeoCloseLoop.Pressed)
			{
				showMsg("A geo with less than three vertices cannot have a closed loop.");
				return;
			}

			if (selectedGeo.Vertices.Length < 3 && btnEditorGeoDisplay.Pressed)
			{
				showMsg("A geo with less than three vertices cannot display a texture.");
				return;
			}

			selectedGeo.CloseLoop = btnEditorGeoCloseLoop.Pressed;
			selectedGeo.Display = btnEditorGeoDisplay.Pressed;
			selectedGeo.refreshMath(new Vector2((float)tGeo.Width, (float)tGeo.Height));
		}

		/// <summary>Called when btnEditorGeoDel is clicked.</summary>
		private void btnEditorGeoDel_MouseLeftUp(object sender, EventArgs e)
		{
			removeSelGeo();
		}

		/// <summary>Called when btnEditorHotspotScript is clicked.</summary>
		private void btnEditorHotspotScript_MouseLeftUp(object sender, EventArgs e)
		{
			showTextEditor("Hotspot Script");
			txtTextEditorText.Text = lblEditorHotspotScriptVal.Text;
		}

		/// <summary>Called when btnTextEditorOK is clicked.</summary>
		private void btnTextEditorOK_MouseLeftUp_HotspotScript(object sender, EventArgs e)
		{
			if (lblTextEditorTitle.Text != "Hotspot Script")
				return;

			lblEditorHotspotScriptVal.Text = txtTextEditorText.Text.Trim();
			lblEditorHotspotScriptView.Text = string.IsNullOrWhiteSpace(lblEditorHotspotScriptVal.Text) ? "---" : "###";
		}

		/// <summary>Called when btnEditorHotspotApply is clicked.</summary>
		private void btnEditorHotspotApply_MouseLeftUp(object sender, EventArgs e)
		{
			// move hotspot
			if (selectedHotspot.X.ToString() != txtEditorHotspotX.Text
				|| selectedHotspot.Y.ToString() != txtEditorHotspotY.Text)
			{
				VectorF moveTo = new VectorF((FInt)double.Parse(txtEditorHotspotX.Text), (FInt)double.Parse(txtEditorHotspotY.Text));
				moveHotspot(selectedHotspot, moveTo);
			}

			selectedHotspot.Script = lblEditorHotspotScriptVal.Text.Trim();
		}

		/// <summary>Called when btnEditorHotspotDel is clicked.</summary>
		private void btnEditorHotspotDel_MouseLeftUp(object sender, EventArgs e)
		{
			removeSelHotspot();
		}

		#endregion Editor
		#region MsgBox

		/// <summary>Called when btnMsgBoxExit is clicked.</summary>
		private void btnMsgBoxExit_MouseLeftUp(object sender, EventArgs e)
		{
			cntMsgBox.Visible = false;
		}

		#endregion MsgBox
		#region Save and Load

		/// <summary>Called when btnSaveWorld is clicked.</summary>
		private void btnSaveWorld_MouseLeftUp(object sender, EventArgs e)
		{
			lblSaveLoadTitle.Text = "Save World";
			btnSaveLoadConfirm.Text = "Save";
			initCntSaveLoad();
			cntSaveLoad.Visible = true;
		}

		/// <summary>Called when btnLoadWorld is clicked.</summary>
		private void btnLoadWorld_MouseLeftUp(object sender, EventArgs e)
		{
			lblSaveLoadTitle.Text = "Load World";
			btnSaveLoadConfirm.Text = "Load";
			initCntSaveLoad();
			cntSaveLoad.Visible = true;
		}

		/// <summary>Called when btnSaveLoadCancel is clicked.</summary>
		private void btnSaveLoadCancel_MouseLeftUp(object sender, EventArgs e)
		{
			closeCntSaveLoad();
		}

		/// <summary>Called when lsbSaveLoadFiles is double-clicked.</summary>
		private void lsbSaveLoadFiles_OnMouseLeftDoubleClick(object sender, EventArgs e)
		{
			if (lsbSaveLoadFiles.SelectedIndex == -1)
				return;
			
			if (lsbSaveLoadFiles.SelectedItem == "..")
			{
				cntSaveLoad.Controls.Remove(btnSaveLoadPath[btnSaveLoadPath.Count - 1]);
				btnSaveLoadPath.RemoveAt(btnSaveLoadPath.Count - 1);

				StringBuilder newPath = new StringBuilder();
				for (int i = 0; i < btnSaveLoadPath.Count; i++)
				{
					newPath.Append(btnSaveLoadPath[i].Text);
					newPath.Append('\\');
				}
				fillSaveLoadFilesLB(newPath.ToString());
			}
			else if (lsbSaveLoadFiles.SelectedItem.StartsWith("<DIR> "))
			{
				ButtonText newButton = new ButtonText();
				newButton.Text = lsbSaveLoadFiles.SelectedItem.Substring(6);;
				newButton.Bounds = new Rectangle(btnSaveLoadPath[btnSaveLoadPath.Count - 1].Right + 5, btnSaveLoadPath[0].Top, (int)(newButton.Font.MeasureString(newButton.Text).X + .5f) + 10, 20);
				newButton.MouseLeftUp += new EventHandler(btnSaveLoadPath_MouseLeftUp);
				cntSaveLoad.Controls.Add(newButton);
				btnSaveLoadPath.Add(newButton);

				StringBuilder newPath = new StringBuilder();
				for (int i = 0; i < btnSaveLoadPath.Count; i++)
				{
					newPath.Append(btnSaveLoadPath[i].Text);
					newPath.Append('\\');
				}

				fillSaveLoadFilesLB(newPath.ToString());
			}
			else
			{
				// TODO: save or load
				if (btnSaveLoadConfirm.Text == "Load")
					loadSelectedFile();
				else
					saveWorld();
				closeCntSaveLoad();
			}
		}

		/// <summary>Called when btnSaveLoadConfirm is clicked.</summary>
		private void btnSaveLoadConfirm_MouseLeftUp(object sender, EventArgs e)
		{
			// TODO: save or load
			if (btnSaveLoadConfirm.Text == "Load")
				loadSelectedFile();
			else
				saveWorld();
			closeCntSaveLoad();
		}

		/// <summary>Called when one of the ButtonTexts in btnSaveLoad path are clicked.</summary>
		private void btnSaveLoadPath_MouseLeftUp(object sender, EventArgs e)
		{
			StringBuilder newPath = new StringBuilder();

			for (int i = 0; i < btnSaveLoadPath.Count; i++)
			{
				newPath.Append(btnSaveLoadPath[i].Text);
				newPath.Append('\\');

				if (sender == btnSaveLoadPath[i])
				{
					if (i < btnSaveLoadPath.Count - 1)
					{
						for (int j = btnSaveLoadPath.Count - 1; j > i; j--)
						{
							cntSaveLoad.Controls.Remove(btnSaveLoadPath[j]);
						}
						btnSaveLoadPath.RemoveRange(i + 1, btnSaveLoadPath.Count - i - 1);
					}
					break;
				}
			}

			fillSaveLoadFilesLB(newPath.ToString());
		}

		/// <summary>Called when lsbSaveLoadFiles's selected index is changed.</summary>
		private void lsbSaveLoadFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtSaveLoadFileName.Text = ((lsbSaveLoadFiles.SelectedIndex == -1 || lsbSaveLoadFiles.SelectedItem.StartsWith("<DIR> ")) ? string.Empty : lsbSaveLoadFiles.SelectedItem);
		}

		#endregion Save and Load
		#region Text Editor

		/// <summary>Called when btnTextEditorOK is clicked.</summary>
		private void btnTextEditorOK_MouseLeftUp(object sender, EventArgs e)
		{
			cntTextEditor.Visible = false;
		}

		/// <summary>Called when btnTextEditorCancel is clicked.</summary>
		private void btnTextEditorCancel_MouseLeftUp(object sender, EventArgs e)
		{
			cntTextEditor.Visible = false;
		}

		#endregion Text Editor

		#endregion Events

		#endregion Methods
	}
}
