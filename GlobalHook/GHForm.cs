using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;

namespace GH {
	
	/// <summary>
	/// 透過PNG描画・アニメーションなどの特殊効果を付与したウィンドウ
	/// </summary>
	abstract public class GHForm : Form {

		public GHForm() {
			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar = false;
			HideTimer = new System.Windows.Forms.Timer() {
				Interval = 400,
				Enabled = false
			};
			HideTimer.Tick += new EventHandler(HideTimer_Tick);
			AnimationStopwatch = new Stopwatch();
			Shown += (sender, e) => TopMost = true;
			Enter += (sender, e) => HideTimer.Stop();
			TempControl = new Control {
				Visible = false
			};
			Controls.Add(TempControl);
			NoSelectItem();
		}

		// マウス操作
		public bool MouseActive { get; set; } = false;
		// キー操作
		public bool KeyboardActive { get; set; } = false;
		// 固定 優先度が高い
		public bool FixedActive { get; set; } = false;
		// 表示状態
		public bool FormVisible { get; private set; } = false;

		private int _SelectIndex = -1;

		// 選択項目のインデックス
		public int SelectIndex {
			get {
				return FormVisible ? _SelectIndex : -1;
			}
			protected set {
				_SelectIndex = value;
			}
		}

		// 何も選択しないときの選択するコントロール
		private Control TempControl { get; set; }

		// タスクのトークン
		private CancellationTokenSource TokenSource { get; set; }
		private CancellationToken CancelToken { get; set; }

		/// <summary>
		/// ウィンドウを非表示にするまでのタイマー
		/// </summary>
		protected System.Windows.Forms.Timer HideTimer { get; set; }

		/// <summary>
		/// アニメーションの時間を測る
		/// </summary>
		private Stopwatch AnimationStopwatch { get; set; }

		/// <summary>
		/// アニメーションの情報
		/// </summary>
		public GHAnimateInfo animateInfo = new GHAnimateInfo(400, 200, true, true);

		/// <summary>
		/// １アニメーション分の累計経過時間
		/// </summary>
		private long StopwatchTotal { get; set; } = 0;

		/// <summary>
		/// アニメーションされているか
		/// </summary>
		public bool IsAnimation { get; private set; } = false;

		/// <summary>
		/// 移動元の位置
		/// </summary>
		protected int GHFormSrcPosition { get; set; } = 0;

		/// <summary>
		/// 移動先の位置
		/// </summary>
		protected int GHFormDestPosition { get; set; } = 0;

		private int RemainingDistanceOfSlide { get; set; } = 0;

		/// <summary>
		/// アニメーションのフレーム間の時間
		/// </summary>
		private int AnimationSleepMili { get; set; } = 5;

		#region アニメーション

		private delegate void DelegateSetLeftOrTop(int pos);
		private delegate int DelegateGetLeftOrTop();
		private delegate IntPtr DelegateGetHandle();
		private delegate void DelegateAddSlideDistance(int spd);
		private delegate int DelegateGetSlideDistance();
		private delegate void DelegateAnimationInitial();
		private delegate void DelegateAnimationFinal(bool slide, bool fade);
		private delegate bool DelegateGetSlideFlag();
		private delegate bool DelegateGetFadeFlag();
		private delegate bool DelegateGetVisible();
		private delegate void DelegateEndAnimation();
		private delegate int DelegateGetSrcPosition();
		private delegate int DelegateGetDestPosition();
		private delegate byte DelegateGetAlpha();
		private delegate void DelegateSetAlpha(byte alpha);
		private delegate long DelegateGetAnimateTime();
		private delegate long DelegateGetTotalTime();

		/// <summary>
		/// ランチャーの向きによって位置を設定
		/// </summary>
		/// <param name="pos">位置</param>
		private void SetLeftOrTop(int pos) {
			if (GHManager.IsVertical)
				Left = pos;
			else
				Top = pos;
		}

		/// <summary>
		/// ランチャーの向きによって位置を取得
		/// </summary>
		/// <returns></returns>
		private int GetLeftOrTop() {
			if (GHManager.IsVertical)
				return Left;
			else
				return Top;
		}

		/// <summary>
		/// ウィンドウハンドルの取得
		/// </summary>
		/// <returns></returns>
		private IntPtr GetFormHandle() {
			return Handle;
		}

		/// <summary>
		/// スライドした距離を増やす
		/// </summary>
		/// <param name="spd">進んだ距離</param>
		private void AddSlideDistance(int spd) {
			RemainingDistanceOfSlide += Math.Abs(spd);
		}

		/// <summary>
		/// スライドした距離を取得
		/// </summary>
		/// <returns></returns>
		private int SlideDistance {
			get {
				return RemainingDistanceOfSlide;
			}
		}

		/// <summary>
		/// アニメーション内のループ毎の最初の処理
		/// </summary>
		private void AnimationInitial() {
			AnimationStopwatch.Stop();

			StopwatchTotal += AnimationStopwatch.ElapsedMilliseconds;
		}

		private long GetTotalTime() {
			return StopwatchTotal;
		}

		/// <summary>
		/// アニメーション内のループ毎の最後の処理
		/// </summary>
		/// <param name="slide">スライドが続くか</param>
		/// <param name="fade">フェードが続くか</param>
		private void AnimationFinal(bool slide, bool fade) {
			GHManager.Launcher.BringToFront();

			// 終了していない場合、続ける
			if (slide || fade) {
				AnimationStopwatch.Restart();
			}
		}

		/// <summary>
		/// スライドするかを取得
		/// </summary>
		/// <returns></returns>
		private bool GetSlideFlag() {
			return animateInfo._Slide;
		}

		/// <summary>
		/// フェードするかを取得
		/// </summary>
		/// <returns></returns>
		private bool GetFadeFlag() {
			return animateInfo._Fade;
		}

		/// <summary>
		/// 表示か非表示かを取得
		/// </summary>
		/// <returns></returns>
		private bool GetVisible() {
			return FormVisible;
		}

		/// <summary>
		/// アニメーションを終了させる
		/// </summary>
		private void EndAnimation() {
			IsAnimation = false;
		}

		private int GetGetSrcPosition() {
			return GHFormSrcPosition;
		}

		private int GetGetDestPosition() {
			return GHFormDestPosition;
		}

		private byte GetAlpha() {
			return animateInfo._Alpha;
		}

		private void SetAlpha(byte alpha) {
			animateInfo._Alpha = alpha;
		}

		private long GetAnimateTime() {
			return animateInfo._AnimateTime;
		}

		/// <summary>
		/// ウィンドウをスライドさせる
		/// </summary>
		/// <param name="visible">ウィンドウの表示・非表示</param>
		/// <param name="loopMax">ループ回数</param>
		/// <param name="loopCount">現在のループ数</param>
		/// <param name="handle">ウィンドウハンドル</param>
		/// <param name="slideDistance">スライドした距離</param>
		/// <param name="getLeftOrTop">位置を取得する関数のデリゲート</param>
		/// <param name="setLeftOrTop">位置を設定する関数のデリゲート</param>
		/// <returns>スライドが終了したか</returns>
		private bool GHFormSlider(bool visible, int srcPos, int destPos, int loopMax, int loopCount, long animateTime, long totalTime, ref IntPtr handle, ref int slideDistance, ref DelegateGetLeftOrTop getLeftOrTop, ref DelegateSetLeftOrTop setLeftOrTop) {

			//Rectangle screen = Screen.FromHandle(handle).Bounds;
			int BasePos = (int)Invoke(getLeftOrTop);
			bool reverse = GHManager.Settings.Launcher.Pos >= 2;

			// 全体距離
			int total = destPos - srcPos;

			// 進む距離
			int spd = (int)Math.Ceiling((decimal)total / loopMax);

			// 距離の進んだ割合
			double progressDistance = (slideDistance == 0 ? 0 : ((double)slideDistance) / total);
			// ループの進んだ割合
			double progressLoop = (loopCount == 0 ? 0 : ((double)loopCount / loopMax));

			// ループより先に進んでいた場合、速度を0にする
			if (progressDistance > progressLoop) {
				spd = 0;
			}

			// 反対なら逆向きにする
			if (reverse)
				spd *= -1;

			// 非表示なら逆向きにする
			if (!visible)
				spd *= -1;

			BasePos += spd;
			slideDistance += Math.Abs(spd);

			// 位置を補正
			if (BasePos < srcPos)
				BasePos = srcPos;
			if (BasePos > destPos)
				BasePos = destPos;

			// 移動させる
			Invoke(setLeftOrTop, BasePos);

			// 経過時間がアニメーション時間を超えたら終了
			if (totalTime > animateTime) {
				return false;
			}
			else {
				return true;
			}

		}

		/// <summary>
		/// ウィンドウをフェードさせる
		/// </summary>
		/// <param name="loopMax">ループ最大</param>
		/// <param name="loopCount">現在のループ数</param>
		/// <returns></returns>
		private bool GHFormFader(bool visible, int srcPos, int destPos, int loopMax, int loopCount, long animateTime, long totalTime, ref DelegateGetAlpha getAlpha, ref DelegateSetAlpha setAlpha) {

			int alpha = (byte)Invoke(getAlpha);

			// アルファ値の間隔
			int interval = (int)Math.Floor((decimal)(255 / loopMax));

			alpha += (interval * (visible ? 1 : -1));

			if (alpha > 255)
				alpha = 255;
			if (alpha < 0)
				alpha = 0;

			Invoke(setAlpha, (byte)alpha);

			// 経過時間がアニメーション時間を超えたら終了
			if (totalTime < animateTime) {
				return true;
			}
			else {
				if (!visible)
					Invoke(setAlpha, (byte)0);
				if (visible) {
					Invoke(setAlpha, (byte)255);
				}

				return false;
			}
		}

		/// <summary>
		/// アニメーション処理
		/// </summary>
		private async void GHFormAnimation(CancellationToken token) {
			await Task.Run(() => {
				int loopMax = 0;
				int slideDistance = 0;
				long totalTime = 0;
				bool slide = false;
				bool fade = false;
				bool visible = (bool)Invoke(new DelegateGetVisible(GetVisible));
				int srcPos = (int)Invoke(new DelegateGetSrcPosition(GetGetSrcPosition));
				int destPos = (int)Invoke(new DelegateGetDestPosition(GetGetDestPosition));
				long animateTime = (long)Invoke(new DelegateGetAnimateTime(GetAnimateTime));
				IntPtr handle = (IntPtr)Invoke(new DelegateGetHandle(GetFormHandle));
				DelegateAnimationInitial animationInitial = AnimationInitial;
				DelegateAnimationFinal animationFinal = AnimationFinal;
				DelegateSetLeftOrTop setLeftOrTop = SetLeftOrTop;
				DelegateGetLeftOrTop getLeftOrTop = GetLeftOrTop;
				DelegateGetAlpha getAlpha = GetAlpha;
				DelegateSetAlpha setAlpha = SetAlpha;
				DelegateGetTotalTime getTotalTime = GetTotalTime;

				// スライド・フェードのフラグを取得
				slide = (bool)Invoke(new DelegateGetSlideFlag(GetSlideFlag));
				fade = (bool)Invoke(new DelegateGetFadeFlag(GetFadeFlag));
				bool slideEnd = slide;
				bool fadeEnd = fade;

				// ループ回数
				loopMax = (int)(animateInfo._AnimateTime / AnimationSleepMili);

				if (!slideEnd && !fadeEnd) {
					loopMax = 0;
				}

				for (int i = 0; i < loopMax; ++i) {

					if (token.IsCancellationRequested) {
						return;
					}

					Invoke(animationInitial);
					totalTime = (long)Invoke(getTotalTime);

					slideEnd = slide ? GHFormSlider(visible, srcPos, destPos, loopMax, i, animateTime, totalTime, ref handle, ref slideDistance, ref getLeftOrTop, ref setLeftOrTop) : false;
					fadeEnd = fade ? GHFormFader(visible, srcPos, destPos, loopMax, i, animateTime, totalTime, ref getAlpha, ref setAlpha) : false;

					Invoke(animationFinal, new object[] { slideEnd, fadeEnd });

					// 指定時間待つ
					Thread.Sleep(AnimationSleepMili);
				}

				// 正確な位置に修正
				bool reverse = GHManager.Settings.Launcher.Pos >= 2;

				if (slide) {
					if (!visible) {
						Invoke(setLeftOrTop, (reverse ? destPos : srcPos));
					}
					if (visible) {
						Invoke(setLeftOrTop, (reverse ? srcPos : destPos));
					}
				}
				else {
					if (!visible) {
						Invoke(setLeftOrTop, destPos);
					}
					if (visible) {
						Invoke(setLeftOrTop, destPos);
					}
				}

				Invoke(new DelegateEndAnimation(EndAnimation));
			}, token);
		}

		/// <summary>
		/// アニメーションを開始する
		/// </summary>
		/// <param name="visible">ウィンドウの表示・非表示</param>
		private bool StartAnimation(bool visible) {

			if (TokenSource != null) {
				if (IsAnimation) {
					TokenSource.Cancel();
				}
				TokenSource.Dispose();
				TokenSource = null;
			}
			TokenSource = new CancellationTokenSource();
			CancelToken = TokenSource.Token;

			if ((!visible && FormVisible) || (visible && !FormVisible)) {

				GHFormDraw();

				FormVisible = visible;

				// 非表示遅延時間の更新
				HideTimer.Interval = animateInfo._DelayTime;
				IsAnimation = true;
				StopwatchTotal = 0;

				// 移動元と移動先の位置を修正
				if (GHManager.Settings.Launcher.Pos >= 2) {
					int temp = GHFormDestPosition;
					GHFormDestPosition = GHFormSrcPosition;
					GHFormSrcPosition = temp;
				}

				// スライドアニメーション
				if (animateInfo._Slide) {
					RemainingDistanceOfSlide = 0;
					if (GHManager.IsVertical) {
						Left = !visible ? GHFormDestPosition : GHFormSrcPosition;
					}
					else {
						Top = !visible ? GHFormDestPosition : GHFormSrcPosition;
					}
				}
				else {
					// スライドなしフェード
					if (GHManager.IsVertical) {
						Left = GHFormSrcPosition = GHFormDestPosition;
					}
					else {
						Top = GHFormSrcPosition = GHFormDestPosition;
					}
				}

				// フェードアニメーション
				if (animateInfo._Fade) {
					animateInfo._Alpha = (byte)(!visible ? 255 : 0);
				}
				else {
					// フェードなしスライド
					if (animateInfo._Slide) {
						animateInfo._Alpha = 255;
					}
					else {
						animateInfo._Alpha = (byte)(!visible ? 0 : 255);
					}
				}

				// アニメーション開始
				AnimationStopwatch.Restart();
				GHFormAnimation(CancelToken);

				// ランチャーを最前面
				GHManager.ItemList.BringToFront();
				GHManager.MysetList.BringToFront();
				GHManager.Launcher.BringToFront();

				return true;
			}

			return false;
		}

		public bool ShowAnimation() {
			if (StartAnimation(true)) {
				return true;
			}
			else {
				return false;
			}
		}

		public bool HideAnimation() {
			if (StartAnimation(false)) {
				return false;
			}
			else {
				return true;
			}
		}

		#endregion

		/// <summary>
		/// ウィンドウを描画
		/// </summary>
		public void GHFormDraw() {

			//if (!IsAnimation) return;
			if (!FormVisible && !IsAnimation) return;

			DrawPriorUpdate();

			Point point = Location.IsEmpty ? new Point(0, 0) : Location;
			Size size = Size.IsEmpty ? new Size(1, 1) : Size;

			using (Bitmap bmp = new Bitmap(size.Width, size.Height)) {

				Graphics graph = Graphics.FromImage(bmp);

				if (GHManager.Settings.DrawQuality == 2) {
					graph.SmoothingMode = SmoothingMode.HighQuality;
					graph.InterpolationMode = InterpolationMode.HighQualityBilinear;
					graph.CompositingQuality = CompositingQuality.HighQuality;
					graph.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
					graph.PixelOffsetMode = PixelOffsetMode.HighQuality;
				}
				else if (GHManager.Settings.DrawQuality == 1) {
					graph.SmoothingMode = SmoothingMode.AntiAlias;
					graph.InterpolationMode = InterpolationMode.Bilinear;
					graph.CompositingQuality = CompositingQuality.AssumeLinear;
					graph.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
					graph.PixelOffsetMode = PixelOffsetMode.Half;
				}
				else {
					graph.SmoothingMode = SmoothingMode.None;
					graph.InterpolationMode = InterpolationMode.Low;
					graph.CompositingQuality = CompositingQuality.HighSpeed;
					graph.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
					graph.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				}

				graph.Clear(Color.FromArgb(0, 0, 0, 0));

				//設定された描画メソッドを実行
				DrawForm(ref graph);

				//レイヤーに描画
				IntPtr hbmp = bmp.GetHbitmap();
				GHFormDrawingOnLayer(ref graph, ref hbmp, ref point, ref size);

				graph.Dispose();
				graph = null;
			}

		}

		/// <summary>
		/// レイヤーにビットマップを描画
		/// </summary>
		/// <param name="graph">描画したサーフェース</param>
		/// <param name="hBitmap">描画するビットマップ</param>
		private void GHFormDrawingOnLayer(ref Graphics graph, ref IntPtr hBitmap, ref Point point, ref Size size) {

			using (Graphics g_sc = Graphics.FromHwnd(IntPtr.Zero)) {

				IntPtr hdc_sc = g_sc.GetHdc();
				IntPtr hdc_bmp = graph.GetHdc();

				IntPtr oldhbmp = WinAPI.SelectObject(hdc_bmp, hBitmap);

				WinAPI.BLENDFUNCTION blend = new WinAPI.BLENDFUNCTION {
					BlendOp = WinAPI.AC_SRC_OVER,
					BlendFlags = 0,
					SourceConstantAlpha = animateInfo._Alpha,
					AlphaFormat = WinAPI.AC_SRC_ALPHA
				};

				Point surfacePos = new Point(0, 0);

				WinAPI.UpdateLayeredWindow(Handle, hdc_sc, ref point, ref size,
					hdc_bmp, ref surfacePos, 0, ref blend, WinAPI.ULW_ALPHA);

				WinAPI.DeleteObject(WinAPI.SelectObject(hdc_bmp, oldhbmp));
				g_sc.ReleaseHdc();
				graph.ReleaseHdc();

			}
		}

		/// <summary>
		/// ウィンドウの更新
		/// </summary>
		public void GHFormUpdate() {
			// 表示状態の更新
			Update_Visible();
			// タイマーの更新
			Update_Timer();
			// 位置を更新
			Update_Bounds();
			Update_ItemPos();
			Update_SlideMaxAndMin();
		}

		/// <summary>
		/// 表示状態を更新
		/// </summary>
		protected abstract void Update_Visible();
		/// <summary>
		/// タイマーの状態を更新
		/// </summary>
		protected abstract void Update_Timer();
		/// <summary>
		/// ウィンドウの位置・サイズの更新
		/// </summary>
		protected abstract void Update_Bounds();
		/// <summary>
		/// アイテム位置の更新
		/// </summary>
		protected abstract void Update_ItemPos();
		/// <summary>
		/// スライドの位置を設定
		/// </summary>
		protected abstract void Update_SlideMaxAndMin();
		/// <summary>
		/// 非表示にするかを指定
		/// </summary>
		/// <returns>非表示ならtrue</returns>
		protected abstract bool Hide_Criteria();
		/// <summary>
		/// ウィンドウ上の描画
		/// </summary>
		/// <param name="graph"></param>
		protected virtual void DrawForm(ref Graphics graph) {
		}

		/// <summary>
		/// 頻繁に更新が必要な処理
		/// </summary>
		protected abstract void DrawPriorUpdate();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hideAnimateTime"></param>
		public void GetHideAnimateTime(ref GHAnimateInfo destAnimateInfo) {
			destAnimateInfo = new GHAnimateInfo(animateInfo);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hideAnimateTime"></param>
		public void SetHideAnimateTime(ref GHAnimateInfo srcAnimateInfo) {
			animateInfo = new GHAnimateInfo(srcAnimateInfo);
		}

		/// <summary>
		/// ウィンドウを非表示にするタイマー処理
		/// </summary>
		private void HideTimer_Tick(object sender, EventArgs e) {
			if (Hide_Criteria()) {
				HideAnimation();
			}
			HideTimer.Stop();
		}

		// アイテムの数
		protected abstract int GetItemCount();

		// 次のアイテムを選択する next=進める数
		public void SelectNextItem(int next = 1) {
			int cnt = GetItemCount();
			if (cnt < 0) {
				_SelectIndex = 0;
			}
			else {
				_SelectIndex += next;

				if (_SelectIndex < 0) {
					_SelectIndex = cnt - 1;
				}
				else if (_SelectIndex >= cnt) {
					_SelectIndex = 0;
				}
			}
		}

		public void NoSelectItem() {
			TempControl.Focus();
			TempControl.Select();
		}

		protected void ControlsClear() {
			Controls.Clear();
			Controls.Add(TempControl);
		}

		/// <summary>
		/// ウィンドウ生成時のパラメータを書き換え
		/// </summary>
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= WinAPI.WS_EX_LAYERED | WinAPI.WS_EX_NOACTIVATE;

				return cp;
			}
		}
	}
}
