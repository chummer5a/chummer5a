using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Chummer.UI.Shared;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
	public partial class Mainform : Form
	{
		//Main display
		private readonly BindingList<CrashReport> _crashReports = new BindingList<CrashReport>();
		private BindingListDisplay<CrashReport> _bldCrashReports;

		//Status strip
		private delegate void MainThreadDelegate(INotifyThreadStatus sender, StatusChangedEventArgs args);
		private MainThreadDelegate _mainThreadDelegate;
		private readonly Dictionary<INotifyThreadStatus, ToolStripItem> _statusLabels = new Dictionary<INotifyThreadStatus, ToolStripItem>();

		//background workers
		private DynamoDbLoader _loader;
		private DownloaderWorker _downloader;


		private readonly Dictionary<string, Action<StatusChangedEventArgs>> _specificHandlers;
		

		public Mainform()
		{
			_specificHandlers = new Dictionary<string, Action<StatusChangedEventArgs>>()
			{
				{"DynamoDBConnection" , DynamoDbStatus}
			};
			InitializeComponent();
		}

		private void Mainform_Load(object sender, EventArgs e)
		{
			if (!PersistentState.Setup)
			{
				SetupForm setupForm = new SetupForm();
				DialogResult result = setupForm.ShowDialog();

				if (result != DialogResult.OK)
				{
					Application.Exit();
					return;
				}

				PersistentState.Initialize(setupForm.Id, setupForm.Key);

			}

			_loader = new DynamoDbLoader();
			_loader.StatusChanged += OtherThreadNotificationHandler;

			_downloader = new DownloaderWorker();
			_downloader.StatusChanged += OtherThreadNotificationHandler;

			_mainThreadDelegate = MainThreadAction;

			//lstCrashes.View = View.Details;


			foreach (CrashReport crashReport in PersistentState.Database.GetAllCrashes())
			{
				_crashReports.Add(crashReport);
			}
			
			_bldCrashReports = new BindingListDisplay<CrashReport>(_crashReports, c => new CrashReportView(c, _downloader), true)
			{
				Anchor  = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left,
				Location = new Point(12, 69),
				Size = new Size(871, 336),
			};

			_bldCrashReports.Sort(new CrashReportTimeStampFilter());
			Controls.Add(_bldCrashReports);
			
		}

		//This is used to subscribe to an action happening on another thread. Least ugly way i know to re-route it to ui thread
		private void OtherThreadNotificationHandler(INotifyThreadStatus sender, StatusChangedEventArgs args)
		{
			try
			{
				if (Disposing || IsDisposed) return;

				Invoke(_mainThreadDelegate, sender, args);
			}
			catch
			{
				// ignored
			}
		}

		private void MainThreadAction(INotifyThreadStatus sender, StatusChangedEventArgs args)
		{
			ToolStripItem item;
			if (_statusLabels.TryGetValue(sender, out item))
			{
				item.Text =  $"{sender.Name}: {args.Status}";
			}
			else
			{
				item = tsBackground.Items.Add($"{sender.Name}: {args.Status}");
				_statusLabels.Add(sender, item);
			}

			Action<StatusChangedEventArgs> action;
			if (_specificHandlers.TryGetValue(sender.Name, out action))
			{
				action(args);
			}
		}

		private void DynamoDbStatus(StatusChangedEventArgs statusChangedEventArgs)
		{
			List<Guid> list = statusChangedEventArgs.AttachedData;

			if (list == null)
				return;

			foreach (Guid guid in list)
			{
				CrashReport item = PersistentState.Database.GetCrash(guid);
				if(item != null) _crashReports.Add(item);
			}
		}
	}

	internal class CrashReportTimeStampFilter : IComparer<CrashReport>
	{
		public int Compare(CrashReport x, CrashReport y)
		{
			return y.Timestamp.CompareTo(x.Timestamp);
		}
	}
}
