using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace UmaFanCountChecker
{
    public static class OcrUtility
    {
        public static async Task<OcrResult> RecognizeText(SoftwareBitmap snap)
        {
            OcrEngine ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            
            // OCR実行
            var ocrResult = await ocrEngine.RecognizeAsync(snap);
            return ocrResult;
        }

        public static async Task<OcrResult> RecognizeText(System.Drawing.Bitmap bitmap)
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            return await RecognizeText(softwareBitmap);
        }
    }
}
