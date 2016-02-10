using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sever
{
	public class ScriptMan
	{
		private Lua lua;

		private World map;

		/// <summary>Creates a new instance of ScriptMan.</summary>
		/// <param name="world">The world that this script manager belongs to.</param>
		public ScriptMan(World world)
		{
			lua = new Lua();
			map = world;

            lua.RegisterFunction("showMessage", this, this.GetType().GetMethod("showMessage"));
			lua.RegisterFunction("loadWorld", this, this.GetType().GetMethod("loadWorld"));
			lua.RegisterFunction("getOpenNodeIDs", this, this.GetType().GetMethod("getOpenNodeIDs"));
			lua.RegisterFunction("getNumOpenSegs", this, this.GetType().GetMethod("getNumOpenSegs"));
			lua.RegisterFunction("buildSeg", this, this.GetType().GetMethod("buildSeg"));
			lua.RegisterFunction("destroyNode", this, this.GetType().GetMethod("destroyNode"));
			lua.RegisterFunction("nodeExists", this, this.GetType().GetMethod("nodeExists"));
			lua.RegisterFunction("nodeActive", this, this.GetType().GetMethod("nodeActive"));
			lua.RegisterFunction("getConnectedNodes", this, this.GetType().GetMethod("getConnectedNodes"));
        }

		/// <summary>Runs a script.</summary>
		/// <param name="script">The script to run.</param>
		/// <returns>An error message if there is one.</returns>
		public string runScript(string script)
		{
			try
			{
				lua.DoString(script);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			
			return string.Empty;
		}

		/// <summary>Shows a message to the player.</summary>
		/// <param name="message">The message to show.</param>
		public void showMessage(string message)
		{
			map.FrontEndActions.Enqueue(new FrontEndAction(FrontEndAction.ActionType.Message, message));
		}

		/// <summary>Loads a new map.</summary>
		/// <param name="path">The path of the map to load.</param>
		public void loadWorld(string path)
		{
			map.FrontEndActions.Enqueue(new FrontEndAction(FrontEndAction.ActionType.LoadMap, path));
		}

		/// <summary>Gets an array of open node ids for the specified player.</summary>
		/// <param name="playerID">The id of the player to search.</param>
		public string[] getOpenNodeIDs(string playerID)
		{
			List<string> ids = new List<string>();
			Player player = null;

			foreach (Player p in map.Players)
			{
				if (p.ID == playerID)
				{
					player = p;
					break;
				}
			}

			if (player == null)
				return null;

			foreach (Node n in player.Nodes)
			{
				if (n.NumSegments < n.Segments.Length)
					ids.Add(n.ID);
			}

			return ids.ToArray();
		}

		/// <summary>Gets the number of open segments in the specified node.</summary>
		/// <param name="nodeID">The ID of the node to test.</param>
		/// <returns>The number of open segments in the specified node.</returns>
		public double getNumOpenSegs(string nodeID)
		{
			Node node = map.NodeByID[nodeID];
			return node.Segments.Length - node.NumSegments;
		}

		/// <summary>Builds a segment from the specified node to the specified location.</summary>
		/// <param name="nodeID">The ID of the node to build from.</param>
		/// <param name="x">The x-coordinate of the destination of the segment.</param>
		/// <param name="y">The y-coordinate of the destination of the segment.</param>
		public void buildSeg(string nodeID, double x, double y)
		{
			Node node = map.NodeByID[nodeID];
			map.PlayerActions.Add(new PlayerAction(node.Owner, PlayerAction.ActionType.BuildSeg, nodeID, false, new VectorF((FInt)x, (FInt)y)));
		}

		/// <summary>Destroys the specified node.</summary>
		/// <param name="nodeID">The ID of the node to destroy.</param>
		public void destroyNode(string nodeID)
		{
			Node node = map.NodeByID[nodeID];
			map.PlayerActions.Add(new PlayerAction(node.Owner, PlayerAction.ActionType.DestroyNode, nodeID));
		}

		/// <summary>Determines whether or not the specified node exists.</summary>
		/// <param name="nodeID">The ID of the node to search for.</param>
		/// <returns>Whether or not the specified node exists.</returns>
		public bool nodeExists(string nodeID)
		{
			return map.NodeByID.ContainsKey(nodeID);
		}

		/// <summary>Determines whether or not the specified node is active.</summary>
		/// <param name="nodeID">The ID of the node to search for.</param>
		/// <returns>Whether or not the specified node is active.</returns>
		public bool nodeActive(string nodeID)
		{
			return map.NodeByID[nodeID].Active;
		}

		/// <summary>Returns the IDs of all nodes connected by segments to the specified node.</summary>
		/// <param name="nodeID">The ID of the node to search.</param>
		/// <returns>The IDs of all nodes connected by segments to the specified node.</returns>
		public string[] getConnectedNodes(string nodeID)
		{
			Node node = map.NodeByID[nodeID];
			List<string> nodes = new List<string>();

			foreach (Segment s in node.Segments)
			{
				if (s == null)
					continue;
				int index = s.getNodeIndex(node);
				if (s.State[index] == SegmentSkel.SegState.Complete)
					nodes.Add(s.Nodes[1 - index].ID);
			}

			return nodes.ToArray();
		}
	}
}
