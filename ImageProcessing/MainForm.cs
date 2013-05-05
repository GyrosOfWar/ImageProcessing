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
        public enum FilterName { Median = 0, Average, Gauss, Sobel, Custom };
        private ColorImage img;
        private Stopwatch stpw = new Stopwatch();
        private System.Drawing.Bitmap bmp;
        private int height, width;
        private FilterName selectedFilter;
        private Filter average, gauss, sobel1, sobel2;
        public MainForm() {
            InitializeComponent();
            height = pictureBox1.Height;
            width = pictureBox1.Width;
            img = new ColorImage(height, width);
            bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            comboBox1.SelectedIndex = 0;
            selectedFilter = (FilterName) comboBox1.SelectedIndex;
            initFilters();
        }

        private void initFilters() {
            int[][] kernel = new int[3][];
            kernel[0] = new int[] { 1, 1, 1 };
            kernel[1] = new int[] { 1, 1, 1 };
            kernel[2] = new int[] { 1, 1, 1 };
            average = new Filter(kernel, 1.0 / 9.0, 0);

            int[][] kernel2 = new int[3][];
            kernel2[0] = new int[] { 1, 2, 1 };
            kernel2[1] = new int[] { 2, 4, 2 };
            kernel2[2] = new int[] { 1, 2, 1 };
            gauss = new Filter(kernel2, 1.0 / 16.0, 0);

            int[][] kernel3 = new int[3][];
            kernel3[0] = new int[] { 1, 2, 1 };
            kernel3[1] = new int[] { 0, 0, 0 };
            kernel3[2] = new int[] { -1, -2, -1 };
            sobel1 = new Filter(kernel3, 1.0, 128);

            int[][] kernel4 = new int[3][];
            kernel4[0] = new int[] { 1, 0, -1 };
            kernel4[1] = new int[] { 2, 0, -2 };
            kernel4[2] = new int[] { 1, 0, -1 };
            sobel2 = new Filter(kernel4, 1.0, 128);
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
            for(int i = high; i < img.Height - high; i++) {
                for(int j = high; j < img.Width - high; j++) {
                    int newR = 0, newG = 0, newB = 0;
                    for(int i_k = low; i_k <= high; i_k++) {
                        for(int j_k = low; j_k <= high; j_k++) {
                            // TODO Edge problem
                            int r_i = 0, g_i = 0, b_i = 0;
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

        private void openButton_Click(object sender, EventArgs e) {
            if(openFileDialog1.ShowDialog() == DialogResult.OK) {
                bmp = new System.Drawing.Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = bmp;
                img = new ColorImage(bmp.Height, bmp.Width);
                img.LoadFromBitmap(bmp);
            }
        }

        private void applyFilterButton_Click(object sender, EventArgs e) {
            var filtered_bmp = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            switch(selectedFilter) {
                case FilterName.Median:
                    var filtered_median = MedianFilter(img, 3);
                    filtered_median.SaveToBitmap(filtered_bmp);
                    this.bmp = filtered_bmp;
                    this.img = filtered_median;
                    pictureBox1.Image = bmp;
                    break;
                case FilterName.Average:
                    var filtered_avg = Convolution(img, average);
                    filtered_avg.SaveToBitmap(filtered_bmp);
                    this.bmp = filtered_bmp;
                    this.img = filtered_avg;
                    pictureBox1.Image = bmp;
                    break;
               // Gaussian filter
                case FilterName.Gauss:
                    var filtered_gauss = Convolution(img, gauss);
                    filtered_gauss.SaveToBitmap(filtered_bmp);
                    this.bmp = filtered_bmp;
                    this.img = filtered_gauss;
                    pictureBox1.Image = bmp;
                    break;
                case FilterName.Sobel:
                    SobelFiter(img);
                    break; 
            }
        }

        private void SobelFiter(ColorImage img) {
            var filtered_sobel = Convolution(img, sobel1).ToGrayscaleImage();
            ColorImage tmp = new ColorImage(filtered_sobel, filtered_sobel, filtered_sobel);
            tmp.SaveToBitmap(filtered_bmp);
            this.bmp = filtered_bmp;
            this.img = tmp;
            pictureBox1.Image = bmp;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            selectedFilter = (FilterName) comboBox1.SelectedIndex;
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
