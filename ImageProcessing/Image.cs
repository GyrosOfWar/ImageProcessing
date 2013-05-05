using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ImageProcessing {
    public class Image {
        private byte[][] vals;
        public int Height { get; private set; }
        public int Width { get; private set; }
        public byte this[int h, int w] {
            get { return vals[h][w]; }
            set { vals[h][w] = value; }
        }

        public Image(int height, int width) {
            this.Height = height;
            this.Width = width;
            this.vals = new byte[height][];
            for(int i = 0; i < height; i++) {
                vals[i] = new byte[width];
            }
        }
        public override string ToString() {
            return "Height: " + Height + ", Width: " + Width;
        }
    }

    public class ColorImage {
        public Image Red { get; private set; }
        public Image Green { get; private set; }
        public Image Blue { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public const System.Drawing.Imaging.PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

        public ColorImage(Image r, Image g, Image b) {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
            this.Height = r.Height;
            this.Width = r.Width;
        }

        public ColorImage(int h, int w) {
            this.Red = new Image(h, w);
            this.Green = new Image(h, w);
            this.Blue = new Image(h, w);
            this.Height = h;
            this.Width = w;
        }
        public override string ToString() {
            return Red.ToString();
        }
        // The next three methods were
        // taken from http://www.sergejusz.com/engineering_tips/median_filter.htm
        #region saveMethods
        // implemented only for Format24bppRgb !!!
        public void LoadFromBitmap(System.Drawing.Bitmap bmp) {
            switch(bmp.PixelFormat) {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    LoadFrom24bppBitmap(bmp);
                    break;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    break;
            }
        }

        // implemented only for Format24bppRgb !!!
        public void SaveToBitmap(System.Drawing.Bitmap bitmap) {
            switch(bitmap.PixelFormat) {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    SaveTo24bppBitmap(bitmap);
                    break;

            }
        }

        public void LoadFrom24bppBitmap(System.Drawing.Bitmap bitmap) {
            System.Drawing.Imaging.BitmapData bmd = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int offset = 0;
            for(int y = 0; y < bmd.Height; y++) {
                for(int x = 0; x < bmd.Width; x++) {
                    int pixel = System.Runtime.InteropServices.Marshal.ReadInt32(bmd.Scan0, offset + x * 3);
                    byte[] value = BitConverter.GetBytes(pixel);
                    Red[y, x] = value[0];
                    Green[y, x] = value[1];
                    Blue[y, x] = value[2];
                }
                offset += bmd.Stride;
            }
            bitmap.UnlockBits(bmd);
        }

        public void SaveTo24bppBitmap(System.Drawing.Bitmap bitmap) {
            System.Drawing.Imaging.BitmapData bmd = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int offset = 0;
            byte[] value = new byte[4];
            for(int y = 0; y < bmd.Height; y++) {
                for(int x = 0; x < bmd.Width; x++) {
                    value[0] = Red[y, x];
                    value[1] = Green[y, x];
                    value[2] = Blue[y, x];
                    int pixel = BitConverter.ToInt32(value, 0);
                    System.Runtime.InteropServices.Marshal.WriteInt32(bmd.Scan0, offset + x * 3, BitConverter.ToInt32(value, 0));
                }
                offset += bmd.Stride;
            }
            bitmap.UnlockBits(bmd);
        }
        #endregion
        /// <summary>
        /// Returns the size x size neighborhood (central pixel included) of the pixel located at (x, y)
        /// in the image c. Returns the color in channel (0: Red, 1: Green, 2: Blue).
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static byte[] GetNeighborhood(ColorImage c, int x, int y, int size, int channel) {
            if(channel < 0 || channel > 2)
                throw new ArgumentException("Illegal channel");
            byte[] ret = new byte[size * size];
            int count = 0;
            int low = -size / 2;
            int high = size / 2;
            for(int i = low; i <= high; i++) {
                for(int j = low; j <= high; j++) {
                    switch(channel) {
                        case 0:
                            ret[count++] = c.Red[x + i, y + j];
                            break;
                        case 1:
                            ret[count++] = c.Green[x + i, y + j];
                            break;
                        case 2:
                            ret[count++] = c.Blue[x + i, y + j];
                            break;
                    }
                }
            }
            return ret;
        }

        public Image ToGrayscaleImage() {
            Image gray = new Image(Height, Width);
            //0.21 R + 0.71 G + 0.07 B
            for(int i = 0; i < Height; i++) {
                for(int j = 0; j < Width; j++) {
                    double result = 0.21 * (double) Red[i, j] + 0.71 * (double) Green[i, j] + 0.07 * (double) Blue[i, j];
                    gray[i, j] = (byte) result;
                }
            }
            return gray;
        }
    }
}
