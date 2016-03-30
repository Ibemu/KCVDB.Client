using Fiddler;
using KCVDB.Client;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace KanColleDbPost
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			this.ShowInTaskbar = false;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.client = KCVDBClientService.Instance.CreateClient("KanColleDbPost");
			this.deserializer = new ApiDataDeserializer();
			Observable
				.Zip(
					FiddlerApplication_AfterSessionComplete(),
					IKCVDBClient_ApiDataSent(),
					(first, second) => Enumerable.Concat(first, second))
				.Subscribe(texts => {
					foreach (var text in texts) {
						this.AppendText(text + "\n");
					}
				});
			Application.ApplicationExit += Application_ApplicationExit;
			this.button1_Click(this, EventArgs.Empty);
		}

		void Application_ApplicationExit(object sender, EventArgs e)
		{
			if (isCapture)
			{
				// Fiddlerのシャットダウン
				FiddlerApplication.Shutdown();
			}
			global::KanColleDbPost.Properties.Settings.Default.Save();
			this.client.Dispose();
		}

		private bool isCapture = false;

		private IKCVDBClient client;
		private ApiDataDeserializer deserializer;

		private IObservable<string[]> FiddlerApplication_AfterSessionComplete()
		{
			return Observable
				.FromEvent<SessionStateHandler, Session>(
					h => FiddlerApplication.AfterSessionComplete += h,
					h => FiddlerApplication.AfterSessionComplete -= h)
				.Where(oSession => {
					return oSession.PathAndQuery.StartsWith("/kcsapi") &&
						oSession.oResponse.MIMEType.Equals("text/plain");
				})
				.Select(oSession => {
					string url = oSession.fullUrl;
					string responseBody = oSession.GetResponseBodyAsString();
					responseBody.Replace("svdata=", "");

					string str = "Post server from " + url;

					PostServer(oSession);

					return new[] { str };
				});
		}

		private void PostServer(Session oSession)
		{
			string url = oSession.fullUrl;
			string requestBody = HttpUtility.HtmlDecode(oSession.GetRequestBodyAsString());
			requestBody = Regex.Replace(requestBody, @"&api(_|%5F)token=[0-9a-f]+|api(_|%5F)token=[0-9a-f]+&?", "");	// api_tokenを送信しないように削除
			string responseBody = oSession.GetResponseBodyAsString();
			responseBody.Replace("svdata=", "");

			this.client.SendRequestDataAsync(new Uri(url), oSession.ResponseHeaders.HTTPResponseCode, requestBody, responseBody, oSession.ResponseHeaders["Date"]);
		}

		private IObservable<string[]> IKCVDBClient_ApiDataSent()
		{
			return Observable
				.FromEvent<EventHandler<ApiDataSentEventArgs>, ApiDataSentEventArgs>(
					h => (sender, e) => h(e),
					h => this.client.ApiDataSent += h,
					h => this.client.ApiDataSent -= h)
				.Select(e => {
					return this.deserializer.Test(e.TrackingId, e.ApiData, e.SentApiData);
				});
		}

		/// <summary>
		/// キャプチャ開始
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button1_Click(object sender, EventArgs e)
		{
			if (!isCapture)
			{
				int iListenPort = 8877;
				FiddlerApplication.Startup(iListenPort, FiddlerCoreStartupFlags.ChainToUpstreamGateway | FiddlerCoreStartupFlags.RegisterAsSystemProxy | FiddlerCoreStartupFlags.OptimizeThreadPool | FiddlerCoreStartupFlags.MonitorAllConnections);
				isCapture = true;
				AppendText(string.Format("----- Capture start. Port {0}\n", iListenPort));
				button1.Text = "停止";
			}
			else
			{
				AppendText("----- Capture stop\n");
				FiddlerApplication.Shutdown();
				isCapture = false;
				button1.Text = "開始";
			}
		}


		// Windowsフォームコントロールに対して非同期な呼び出しを行うためのデリゲート
		delegate void SetTextCallback(string text);

		private void AppendText(string text)
		{
			// 呼び出し元のコントロールのスレッドが異なるか確認をする
			if (this.textBox1.InvokeRequired)
			{
				// 同一メソッドへのコールバックを作成する
				SetTextCallback delegateMethod = new SetTextCallback(AppendText);

				// コントロールの親のInvoke()メソッドを呼び出すことで、呼び出し元の
				// コントロールのスレッドでこのメソッドを実行する
				this.Invoke(delegateMethod, new object[] { text });
			}
			else
			{
				// コントロールを直接呼び出す
				this.textBox1.AppendText(text);
				this.textBox1.SelectionStart = textBox1.Text.Length;
				this.textBox1.ScrollToCaret();
				var dateTime = DateTime.Now;
				File.AppendAllText(
					Path.Combine(Application.StartupPath, string.Format("{0}.log", dateTime.ToString("yyyyMMdd"))),
					string.Format("[{0}] {1}", dateTime.ToString(), text.Replace("\n", "\r\n")),
					new UTF8Encoding(true));
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			// トレイリストのアイコンを非表示にする  
			notifyIcon1.Visible = false;
		}

		private void Form1_ClientSizeChanged(object sender, EventArgs e)
		{
			if (this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
			{
				// フォームが最小化の状態であればフォームを非表示にする  
				this.Hide();
				// トレイリストのアイコンを表示する  
				notifyIcon1.Visible = true;
			}
		}

		private void notifyIcon1_DoubleClick(object sender, EventArgs e)
		{
			// フォームを表示する  
			this.Visible = true;
			// 現在の状態が最小化の状態であれば通常の状態に戻す  
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.WindowState = FormWindowState.Normal;
			}
			// フォームをアクティブにする  
			this.Activate();
		}

		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
			notifyIcon1_DoubleClick(sender, e);
		}

		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
