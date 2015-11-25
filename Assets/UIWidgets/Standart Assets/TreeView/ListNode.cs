using UnityEngine;

namespace UIWidgets {
	/// <summary>
	/// List node.
	/// </summary>
	[System.Serializable]
	public class ListNode<TItem>
	{
		/// <summary>
		/// The depth.
		/// </summary>
		[SerializeField]
		public int Depth;

		/// <summary>
		/// The node.
		/// </summary>
		[SerializeField]
		public TreeNode<TItem> Node;

		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="depth">Depth.</param>
		public ListNode(TreeNode<TItem> node, int depth)
		{
			Node = node;
			Depth = depth;
		}
	}
}