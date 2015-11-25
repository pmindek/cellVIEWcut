using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.ComponentModel;

namespace UIWidgets {
	/// <summary>
	/// Tree node.
	/// </summary>
	[System.Serializable]
	public class TreeNode<TItem> : IObservable, IDisposable, INotifyPropertyChanged
	{
		/// <summary>
		/// Occurs when on change.
		/// </summary>
		public event OnChange OnChange;

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// The pause observation.
		/// </summary>
		public bool PauseObservation;

		[SerializeField]
		bool isVisible = true;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is visible.
		/// </summary>
		/// <value><c>true</c> if this instance is visible; otherwise, <c>false</c>.</value>
		public bool IsVisible {
			get {
				return isVisible;
			}
			set {
				isVisible = value;
				Changed("IsVisible");
			}
		}

		[SerializeField]
		bool isExpanded;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is expanded.
		/// </summary>
		/// <value><c>true</c> if this instance is expanded; otherwise, <c>false</c>.</value>
		public bool IsExpanded {
			get {
				return isExpanded;
			}
			set {
				isExpanded = value;
				Changed("IsExpanded");
			}
		}
		
		[SerializeField]
		TItem item;

		/// <summary>
		/// Gets or sets the item.
		/// </summary>
		/// <value>The item.</value>
		public TItem Item {
			get {
				return item;
			}
			set {
				item = value;
				Changed("Item");
			}
		}
		
		[SerializeField]
		IObservableList<TreeNode<TItem>> nodes;

		/// <summary>
		/// Gets or sets the nodes.
		/// </summary>
		/// <value>The nodes.</value>
		public IObservableList<TreeNode<TItem>> Nodes {
			get {
				return nodes;
			}
			set {
				if (nodes!=null)
				{
					nodes.OnChange -= Changed;
				}
				nodes = value;
				if (nodes!=null)
				{
					nodes.OnChange += Changed;
				}
				Changed("Nodes");
			}
		}

		/// <summary>
		/// Gets the total nodes count.
		/// </summary>
		/// <value>The total nodes count.</value>
		public int TotalNodesCount {
			get {
				if (nodes==null)
				{
					return 1;
				}
				return nodes.Sum(x => x.TotalNodesCount) + 1;
			}
		}

		/// <summary>
		/// The used nodes count.
		/// </summary>
		public int UsedNodesCount;

		/// <summary>
		/// Gets all used nodes count.
		/// </summary>
		/// <value>All used nodes count.</value>
		public int AllUsedNodesCount {
			get {
				if (!isVisible)
				{
					return 0;
				}
				if (!isExpanded)
				{
					return 0 + UsedNodesCount;
				}
				if (nodes==null)
				{
					return 0 + UsedNodesCount;
				}
				return nodes.Sum(x => x.AllUsedNodesCount) + UsedNodesCount;
			}
		}

		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		/// <param name="nodeItem">Node item.</param>
		/// <param name="nodeNodes">Node nodes.</param>
		/// <param name="nodeIsExpanded">If set to <c>true</c> node is expanded.</param>
		/// <param name="nodeIsVisible">If set to <c>true</c> node is visible.</param>
		public TreeNode(TItem nodeItem,
		                IObservableList<TreeNode<TItem>> nodeNodes = null,
		                bool nodeIsExpanded = false,
		                bool nodeIsVisible = true)
		{
			item = nodeItem;
			nodes = nodeNodes;

			isExpanded = nodeIsExpanded;
			isVisible = nodeIsVisible;

			if (nodes!=null)
			{
				nodes.OnChange += Changed;
			}
		}

		void Changed(string propertyName = "Nodes")
		{
			if (PauseObservation)
			{
				return ;
			}
			if (OnChange!=null)
			{
				OnChange();
			}
			if (PropertyChanged!=null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private bool disposed = false;
		
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="UIWidgets.ObservableList`1"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="UIWidgets.ObservableList`1"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="UIWidgets.ObservableList`1"/> so
		/// the garbage collector can reclaim the memory that the <see cref="UIWidgets.ObservableList`1"/> was occupying.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void DisposeItem(TreeNode<TItem> node)
		{
			node.Dispose();
			node.OnChange -= Changed;
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Free other state (managed objects).</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// Free other state (managed objects).
				}
				if (Nodes!=null)
				{
					Nodes.ForEach(DisposeItem);
					Nodes = null;
				}
				
				// Free your own state (unmanaged objects).
				// Set large fields to null.
				disposed = true;
			}
		}

		// Use C# destructor syntax for finalization code.
		~TreeNode()
		{
			// Simply call Dispose(false).
			Dispose(false);
		}
	}
}