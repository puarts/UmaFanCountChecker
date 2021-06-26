using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reflection;

namespace UmaFanCountChecker
{
    public class MainWindowViewModel
    {
        private Process _gameProcess;
        private volatile bool _isCaptureThreadCanceled = false;
        private Dictionary<int, string> _fanCountInfos = new Dictionary<int, string>();

        public MainWindowViewModel()
        {
            var assembly = Assembly.GetEntryAssembly() ?? throw new Exception();
            
            Title.Value = $"ウマ娘ファン数調べ太郎 {assembly.GetName().Version}";

            CopyListCommand.Subscribe(() =>
            {
                Clipboard.SetText(TsvText.Value);
            }).AddTo(Disposable);

            ClearListCommand.Subscribe(() =>
            {
                _fanCountInfos.Clear();
                UpdateTabSeparatedText();
            }).AddTo(Disposable);
        }

        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<BitmapSource> CapturedBitmap { get; } = new ReactiveProperty<BitmapSource>();

        public ReactiveProperty<int> CaptureIntervalMilliseconds { get; } = new ReactiveProperty<int>(30);

        public ReactiveCommand CopyListCommand { get; } = new ReactiveCommand();

        public ReactiveCommand ClearListCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<string> TsvInfo { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> RecognizedText { get; } = new ReactiveProperty<string>();

        public MainWindow View { get; set; }

        public ReactiveProperty<int> CaptureWidth { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> CaptureHeight { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<float> CropRateLeft { get; } = new ReactiveProperty<float>(0.2783f);
        public ReactiveProperty<float> CropRateRight { get; } = new ReactiveProperty<float>(0.7575f);
        public ReactiveProperty<float> CropRateTop { get; } = new ReactiveProperty<float>(0.46f);
        public ReactiveProperty<float> CropRateBottom { get; } = new ReactiveProperty<float>(0.668f);

        public ReactiveProperty<float> CropVerticalOffset { get; } = new ReactiveProperty<float>(0.0f);

        public ReactiveProperty<string> TsvText { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> Log { get; } = new ReactiveProperty<string>();

        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        private Bitmap CapturedRawBitmap { get; set; }

        public void StartCaptureThread()
        {
            _isCaptureThreadCanceled = false;
            Task.Run(() =>
            {
                while (!_isCaptureThreadCanceled)
                {
                    Thread.Sleep(CaptureIntervalMilliseconds.Value);

                    UpdateCapture();
                    if (CapturedRawBitmap is null)
                    {
                        continue;
                    }

                    var result = OcrUtility.RecognizeText(CapturedRawBitmap).Result;

                    var nameText = ExtractNameText(result.Text);
                    var fanCountText = ExtractFanCountText(result.Text);
                    RecognizedText.Value = result.Text + "\n\n名前:" + nameText + "\nファン数:" + fanCountText;

                    if (fanCountText is null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(nameText) || !IsValidName(nameText))
                    {
                        continue;
                    }

                    if (TryParseFanCountText(fanCountText, out int count))
                    {
                        const int MinimuCount = 1000;
                        if (count < MinimuCount)
                        {
                            continue;
                        }

                        if (_fanCountInfos.ContainsKey(count))
                        {
                            _fanCountInfos[count] = nameText;
                        }
                        else
                        {
                            bool add = true;
                            foreach (var elem in _fanCountInfos)
                            {
                                if (elem.Value == nameText)
                                {
                                    if (count > elem.Key)
                                    {
                                        add = true;
                                        _fanCountInfos.Remove(elem.Key);
                                    }
                                    else
                                    {
                                        add = false;
                                    }
                                    break;
                                }
                            }

                            if (add)
                            {
                                _fanCountInfos.Add(count, nameText);
                                View?.Dispatcher?.Invoke(() =>
                                {
                                    View.ScrollFanCountListToEnd();
                                });
                            }
                        }
                        UpdateTabSeparatedText();
                    }
                }
            });
        }

        private bool IsValidName(string nameText)
        {
            const int MaxLength = 15;
            if (nameText.Length > MaxLength)
            {
                return false;
            }

            var invalidNameTexts = new string[]
            {
                "時間前",
                "ログイン",
                "ロワイン",
                "ロクイン",
                "ロワン",
                "ファン数",
            };

            foreach (var invalidText in invalidNameTexts)
            {
                if (nameText.Contains(invalidText))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateTabSeparatedText()
        {
            lock (TsvText)
            {
                StringBuilder tsvText = new StringBuilder();
                foreach (var elem in _fanCountInfos)
                {
                    tsvText.AppendLine($"{elem.Value}\t{elem.Key}");
                }

                TsvText.Value = tsvText.ToString();
                TsvInfo.Value = $"{_fanCountInfos.Count}人";
            }
        }

        public void StopCaptureThread()
        {
            _isCaptureThreadCanceled = true;
        }

        private string ExtractNameText(string inputText)
        {
            int startIndex = 0;
            var replaced = inputText;
            var topNoiseTexts = new[]
            {
                "ロ グ イ ン"
            };
            foreach (var text in topNoiseTexts)
            {
                for (int i = 0; i < 3; ++i)
                {
                    if (replaced.Length <= i)
                    {
                        break;
                    }

                    if (replaced.Substring(i).StartsWith(text))
                    {
                        int index = replaced.IndexOf("前");
                        if (index == -1)
                        {
                            startIndex = text.Length;
                        }
                        else
                        {
                            startIndex = index + 1;
                        }
                        break;
                    }
                }
            }

            replaced = replaced.Substring(startIndex);
            var endIndex = FindNameEndIndex(replaced);
            return endIndex <= 0 ?
                string.Empty : replaced.Substring(0, endIndex).Replace(" ", string.Empty);
        }

        private int FindNameEndIndex(string inputText)
        {
            var endStrings = new[]
            {
                "()",
                "( の",
                "( ①",
                "①",
                "(↓)",
                "( ↓ )",
                "(i)",
                "(1)",
                "(!)",
                "i 得",
                "O ー",
                "O 得",
                "@)",
                "Q) 得",
                "蓄 得",
                " i ",
                "@ ",
            };

            int result = int.MaxValue;
            foreach (var endString in endStrings)
            {
                var endIndex = inputText.IndexOf(endString);
                if (endIndex > 0)
                {
                    result = Math.Min(result, endIndex);
                }
            }

            return result == int.MaxValue ? -1 : result;
        }

        private bool TryParseFanCountText(string fanCountText, out int result)
        {
            string numberText = "";
            for (int i = fanCountText.Length - 1; i >= 0; --i)
            {
                if (char.IsNumber(fanCountText[i]))
                {
                    numberText = fanCountText[i] + numberText;
                }
                else
                {
                    break;
                }
            }

            return int.TryParse(numberText, out result);
        }

        private string ExtractFanCountText(string inputText)
        {
            var replaced = inputText.Replace(" ", string.Empty);
            var startIndex = replaced.IndexOf('数');
            if (startIndex < 0)
            {
                return null;
            }
            var replaced2 = replaced.Substring(startIndex + 1);
            var endIndex = replaced2.IndexOf('人');
            if (endIndex < 0)
            {
                return null;
            }
            var countText = replaced2.Substring(0, endIndex);
            var removeStrings = new[]
            {
                ",",
                "、",
            };
            foreach (var replaceStr in removeStrings)
            {
                countText = countText.Replace(replaceStr, string.Empty);
            }
            return countText;
        }

        private Process GetGameProcess()
        {
            if (_gameProcess is null || _gameProcess.HasExited || _gameProcess.MainWindowHandle == (IntPtr)0)
            {
                _gameProcess = Process.GetProcessesByName("umamusume").FirstOrDefault();
            }

            return _gameProcess;
        }

        private void UpdateCapture()
        {
            if (View is null)
            {
                return;
            }

            var gameProcess = GetGameProcess();
            if (gameProcess is null)
            {
                return;
            }

            try
            {
                View.Dispatcher.Invoke(() =>
                {
                    var handle = gameProcess.MainWindowHandle;
                    if (handle == (IntPtr)0)
                    {
                            // プロセスはいるけどウインドウがまだない
                            return;
                    }

                    {
                        float top = CropRateTop.Value + CropVerticalOffset.Value > 1 ? 1.0f : CropRateTop.Value + CropVerticalOffset.Value;
                        float bottom = CropRateBottom.Value + CropVerticalOffset.Value > 1 ? 1.0f : CropRateBottom.Value + CropVerticalOffset.Value;
                        var bitmap = ScreenCaptureUtility.CaptureWindow(handle,
                            CropRateLeft.Value,
                            CropRateRight.Value,
                            top,
                            bottom);
                        if (bitmap is null)
                        {
                            return;
                        }

                        CaptureWidth.Value = bitmap.Width;
                        CaptureHeight.Value = bitmap.Height;
                        CapturedRawBitmap = bitmap;
                        CapturedBitmap.Value = ScreenCaptureUtility.ConvertBitmapToBitmapSource(bitmap);
                    }
                });
            }
#if !DEBUG

            catch (Exception e)
            {
                WriteErrorLine("Failed to capture game screen.", e);
            }
#endif
            finally
            {
            }
        }

        private void WriteErrorLine(string message, Exception exception = null)
        {
            StringBuilder messageBuilder = new StringBuilder(message);
            var tmpException = exception;
            while (tmpException != null)
            {
                messageBuilder.AppendLine(tmpException.Message);
                messageBuilder.AppendLine(tmpException.StackTrace);
                tmpException = tmpException.InnerException;
            }

            WriteLogLine(messageBuilder.ToString());
        }

        private void WriteLogLine(string message)
        {
            this.Log.Value += message + Environment.NewLine;
        }
    }
}
