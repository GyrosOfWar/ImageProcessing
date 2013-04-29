using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Profiler;

namespace ImageProcessing {
    public partial class MainForm: Form {
        private ColorImage img;
        private Stopwatch stpw = new Stopwatch();
        public MainForm() {
            InitializeComponent();
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
        public ColorImage Convolution(ColorImage img, Filter f) {
            int[][] kernel = f.Values;
            double scale = f.ScaleFactor;
            int offset = f.Offset;
            ColorImage ret = new ColorImage(img.Height, img.Width);
            if(kernel.Length != kernel[0].Length)
                throw new ArgumentException("Kernel must be quadratic in shape (n x n)");
            int low = -kernel.Length / 2;
            int high = kernel.Length / 2;
            for(int i = high; i < img.Height-high; i++) {
                for(int j = high; j < img.Width-high; j++) {
                    int newR = 0, newG = 0, newB = 0;
                    for(int i_k = low; i_k <= high; i_k++) {
                        for(int j_k = low; j_k <= high; j_k++) {
                            // TODO Edge problem
                            int r_i=0, g_i=0, b_i = 0;
                            r_i = img.Red[i + i_k, j + j_k];
                            g_i = img.Green[i + i_k, j + j_k];
                            b_i = img.Blue[i + i_k, j + j_k];
                            newR += (r_i * kernel[i_k + high][j_k + high]);
                            newG += (g_i * kernel[i_k + high][j_k + high]);
                            newB += (b_i * kernel[i_k + high][j_k + high]);
                        }
                    }
                    double r = (double) newR * scale + offset;
                    double g = (double) newG * scale + offset;
                    double b = (double) newB * scale + offset;
                    ret.Red[i, j] = Clamp((int) r, 0, 255);
                    ret.Green[i, j] = Clamp((int) g, 0, 255);
                    ret.Blue[i, j] = Clamp((int) b, 0, 255);
                }
            }
            return ret;
        }

        private byte Clamp(int val, int low, int high) {
            return (byte) Math.Min(Math.Max(val, low), high);
        }

        private void MainForm_Load(object sender, EventArgs e) {
            //var unfiltered_bmp = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //img.SaveToBitmap(unfiltered_bmp);

            //var filtered_bmp = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //int[][] kernel = new int[3][];
            //kernel[0] = new int[] { 1, 1, 1 };
            //kernel[1] = new int[] { 1, 1, 1 };
            //kernel[2] = new int[] { 1, 1, 1 };
            //var f = new Filter(kernel, 1.0 / 9.0, 0);
            //var img_filtered = Convolution(img, f);
            //img_filtered.SaveToBitmap(filtered_bmp);
        }
    }

    public class Filter {
        public int[][] Values { get; set; }
        public double ScaleFactor { get; set; }
        public int Offset { get; set; }

        public Filter(int[][] values, double scale, int offset) {
            Values = values;
            ScaleFactor = scale;
            Offset = offset;
        }
    }

}
