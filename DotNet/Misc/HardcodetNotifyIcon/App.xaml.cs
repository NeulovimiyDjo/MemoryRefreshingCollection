using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using XTest.Properties;

namespace XTest
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			this.notifyIcon = (TaskbarIcon)base.FindResource("NotifyIcon");

			runner.OnMsgReceived += delegate(object sender, string msg)
			{
				this.notifyIcon.ShowBalloonTip("XTest", msg, BalloonIcon.Info);
			};
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.notifyIcon.Dispose();
			base.OnExit(e);
		}

		private TaskbarIcon notifyIcon;
	}
}
