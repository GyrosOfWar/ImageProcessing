using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessing {
    public partial class MainForm: Form {
        private ColorImage img;
        public MainForm() {
            InitializeComponent();
            var bmp = new System.Drawing.Bitmap(@"res\Desert.jpg");
            pictureBox1.Image = bmp;
            img = new ColorImage(bmp.Height, bmp.Width);
            img.LoadFromBitmap(bmp);
        }

        public ColorImage MedianFilter(ColorImage img, int windowSize) {
            ColorImage ret = new ColorImage(img.Height, img.Width);
            for(int i = 1; i < img.Height - 1; i++) {
                for(int j = 1; j < img.Width - 1; j++) {

                    var neighborhood_r = ColorImage.GetNeighborhood(img, i, j, windowSize, 0);
                    var neighborhood_g = ColorImage.GetNeighborhood(img, i, j, windowSize, 1);
                    var neighborhood_b = ColorImage.GetNeighborhood(img, i, j, windowSize, 2);
                    Array.Sort(neighborhood_r);
                    Array.Sort(neighborhood_g);
                    Array.Sort(neighborhood_b);

                    ret.Red[i, j] = neighborhood_r[neighborhood_r.Length / 2];
                    ret.Green[i, j] = neighborhood_g[neighborhood_g.Length / 2];
                    ret.Blue[i, j] = neighborhood_b[neighborhood_b.Length / 2];
                }
            }
            return ret;
        }

        public ColorImage MedianFilterFast(ColorImage img, int windowSize) {
            ColorImage ret = new ColorImage(img.Height, img.Width);
            int[] hist = new int[256];
            // TODO implement Huang's fast median filtering algorithm
            return ret;
        }
        // TODO Write Kernel class to get stuff like scaling (rational coefficient to be multiplied into the kernel
        // and kernel offset more easily
        public ColorImage Convolution(ColorImage img, byte[][] kernel) {
            ColorImage ret = new ColorImage(img.Height, img.Width);
            if(kernel.Length != kernel[0].Length)
                throw new ArgumentException("Kernel must be quadratic in shape (n x n)");
            int low = -kernel.Length / 2;
            int high = kernel.Length / 2;
            for(int i = 0; i < img.Height; i++) {
                for(int j = 0; j < img.Width; j++) {
                    byte newR = 0, newG = 0, newB = 0;
                    for(int i_k = low; i_k <= high; i_k++) {
                        for(int j_k = low; j_k <= high; j_k++) {
                            newR += (byte) (img.Red[i + i_k, j + j_k] * kernel[i_k + high][j_k + high]);
                            newG += (byte) (img.Green[i + i_k, j + j_k] * kernel[i_k + high][j_k + high]);
                            newB += (byte) (img.Blue[i + i_k, j + j_k] * kernel[i_k + high][j_k + high]);
                        }
                    }
                    ret.Red[i, j] = newR;
                    ret.Green[i, j] = newG;
                    ret.Blue[i, j] = newB;
                }
            }

            return ret;
        }

        private void MainForm_Load(object sender, EventArgs e) {
            var unfiltered_bmp = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            img.SaveToBitmap(unfiltered_bmp);
            pictureBox1.Image = unfiltered_bmp;

            var filtered_bmp = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var img_filtered = MedianFilter(img, 3);
            img_filtered.SaveToBitmap(filtered_bmp);
            pictureBox2.Image = filtered_bmp;
        }
    }

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

        public static byte[] GetNeighborhood(ColorImage c, int x, int y, int size, int channel) {
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
                        default:
                            throw new ArgumentException("Illegal channel");
                    }
                }
            }
            return ret;
        }
    }
}
