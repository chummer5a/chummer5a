using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Chummer.UI.Shared
{
	public partial class SkillsDisplay<TType> : UserControl
	{
		public PropertyChangedEventHandler ChildPropertyChanged;

		private readonly BindingList<TType> _contents;
		private readonly Func<TType, Control> _createFunc;
		private readonly List<Control> _controls = new List<Control>();
		private readonly Dictionary<TType, Control> _map = new Dictionary<TType, Control>();

		private int _progress = 0;

		public SkillsDisplay(BindingList<TType> list, Func<TType, Control> createFunc)
		{
			_contents = list;
			_createFunc = createFunc;
			DoubleBuffered = true;
			InitializeComponent();
		}

		private void ApplicationOnIdle(object sender, EventArgs eventArgs)
		{
			if (AddMore()) return;

			SkillsDisplay_Resize(null, null); //Updates size to show new item.
			_contents.RaiseListChangedEvents = true;
			_contents.ListChanged += ContentsOnListChanged;

			Log.Info($"Finished {_progress}(ALL) items this round");
			Log.Info($"Final height {ClientSize.Height}");
			tblContents.ResumeLayout();
			Application.Idle -= ApplicationOnIdle;
		}

		

		private bool AddMore()
		{
			int round = 0;
			Stopwatch sw = Stopwatch.StartNew();
			

			while (_progress < _contents.Count)
			{
				AddItem(_contents[_progress]);

				_progress++;
				round++;

				long time = sw.ElapsedMilliseconds;
				time /= round;
				time *= (round + 1);
				if (time > 100)
				{
					Log.Info($"Finished {_progress} items, {round} this round in {sw.ElapsedMilliseconds} ms");
					
					
					Log.Info($"ResumeLayout took finished at {sw.ElapsedMilliseconds}");
					return true;
				}
			}

			return false;
		}

		private void SkillsDisplay_Load(object sender, EventArgs e)
		{
			Application.Idle += ApplicationOnIdle;
			Log.Info($"Initial height suspected {ClientSize.Height}");
			//tblContents.SuspendLayout();

			tblContents.SuspendLayout();
			foreach (TType content in _contents)
			{
				AddItem(content);
				_progress++;
				if (tblContents.Controls.Count > 0 && tblContents.Controls.Count*tblContents.Controls[0].Height > ClientSize.Height)
				{
					break;
				}
			}
			
			SkillsDisplay_Resize(null, null);
			
			tblContents.ResumeLayout();
			tblContents.SuspendLayout();

		}

		private void ContentsOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
		{
			ListChangedType type = listChangedEventArgs.ListChangedType;
			if (type == ListChangedType.ItemAdded)
			{
				AddItem(_contents[listChangedEventArgs.NewIndex]);
			}
			else if (type == ListChangedType.ItemDeleted)
			{
				tblContents.Controls.RemoveAt(listChangedEventArgs.NewIndex);
				//tblContents.Controls.Remove(_map[_contents[listChangedEventArgs.NewIndex]]);
			}
			SkillsDisplay_Resize(null, null);
		}

		private void AddItem(TType content)
		{
			Control c = _createFunc?.Invoke(content);

			if (c != null)
			{
				_map.Add(content, c);
				_controls.Add(c);
				tblContents.Controls.Add(c);
				
			}
			else if (Debugger.IsAttached) Debugger.Break();

			INotifyPropertyChanged possibleInterface = (INotifyPropertyChanged) content;
			if (possibleInterface != null)
			{
				possibleInterface.PropertyChanged += OnChildChanged;
			}

		}

		private void OnChildChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			ChildPropertyChanged?.Invoke(sender, propertyChangedEventArgs);
		}

		public void Filter(Func<TType, bool> predicate)
		{
			tblContents.SuspendLayout();
			foreach (KeyValuePair<TType, Control> keyValuePair in _map)
			{
				keyValuePair.Value.Visible = predicate?.Invoke(keyValuePair.Key) ?? true;

			}
			tblContents.ResumeLayout();
		}

		private void SkillsDisplay_Resize(object sender, EventArgs e)
		{
			tblContents.Size = new Size(Width - SystemInformation.VerticalScrollBarWidth, tblContents.PreferredSize.Height);
		}
	}
}
