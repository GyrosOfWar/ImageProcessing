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
    public partial class MainForm : Form {
        private ColorImage img;
        private Stopwatch stpw = new Stopwatch();
        public MainForm() {
            InitializeComponent();
            var bmp = new System.Drawing.Bitmap(@"res\Desert.jpg");
            img = ColorImage.FromBitmap(bmp);
        }

        public ColorImage MedianFilter(ColorImage img, int windowSize) {
            for (int i = 1; i < img.Width - 1; i++) {
                for (int j = 1; j < img.Height - 1; j++) {
                    var neighborhood_r = new byte[windowSize * windowSize];
                    var neighborhood_g = new byte[windowSize * windowSize];
                    var neighborhood_b = new byte[windowSize * windowSize];
                    for (int k = -windowSize / 2; k < windowSize / 2; k++) {
                        for (int l = -windowSize / 2; l < windowSize / 2; l++) {
                            neighborhood_r[k * windowSize + l] = img[i + windowSize, j + windowSize][0];
                            neighborhood_g[k * windowSize + l] = img[i + windowSize, j + windowSize][1];
                            neighborhood_b[k * windowSize + l] = img[i + windowSize, j + windowSize][2];
                        }
                    }
                    Array.Sort(neighborhood_r);
                    Array.Sort(neighborhood_g);
                    Array.Sort(neighborhood_b);
                    img.Red[i, j] = neighborhood_r[neighborhood_r.Length / 2];
                    img.Green[i, j] = neighborhood_g[neighborhood_g.Length / 2];
                    img.Blue[i, j] = neighborhood_b[neighborhood_b.Length / 2];
                }
            }

            return img;
        }

        private void MainForm_Load(object sender, EventArgs e) {
            pictureBox1.Image = img.ToBitmap();
            var img2 = MedianFilter(img, 3);
            pictureBox2.Image = img2.ToBitmap();
        }
    }

    public class Color : IComparable<Color> {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Color(int r, int g, int b) {
            this.R = (byte)r;
            this.G = (byte)g;
            this.B = (byte)b;
        }
        public int CompareTo(Color other) {
            int thisSum = R + G + B;
            int otherSum = other.R + other.G + other.B;
            if (thisSum > otherSum)
                return 1;
            if (thisSum == otherSum)
                return 0;
            if (thisSum < otherSum)
                return -1;
            return 0;
        }
        public override bool Equals(object obj) {
            Color other = (Color)obj;
            return other.R == this.R && other.G == this.G && other.B == this.B;
        }
        public override int GetHashCode() {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        public override string ToString() {
            return "{R = " + R + ", G = " + G + ", B = " + B + "}";
        }

        public System.Drawing.Color ToArgb() {
            return System.Drawing.Color.FromArgb(R, G, B);
        }

        public static Color FromArgb(System.Drawing.Color c) {
            return new Color(c.R, c.G, c.B);
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
            for (int i = 0; i < height; i++) {
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

        public ColorImage(Image r, Image g, Image b)
        {
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
        }

        public byte[] this[int h, int w] {
            get {
                return new byte[] { Red[h, w], Green[h, w], Blue[h, w] };
            }
        }

        public System.Drawing.Bitmap ToBitmap() {
            var retval = new System.Drawing.Bitmap(Height, Width);

            for (int i = 0; i < Height; i++) {
                for (int j = 0; j < Width; j++) {
                    retval.SetPixel(i, j, System.Drawing.Color.FromArgb(this[i, j][0], this[i,j][1], this[i,j][1]));
                }
            }
            return retval;
        }
        public static ColorImage FromBitmap(System.Drawing.Bitmap bmp) {
            var retval = new ColorImage(bmp.Height, bmp.Width);

            for (int i = 0; i < bmp.Height; i++) {
                for (int j = 0; j < bmp.Width; j++) {
                    retval.Red[i, j] = bmp.GetPixel(i, j).R;
                    retval.Green[i, j] = bmp.GetPixel(i, j).G;
                    retval.Blue[i, j] = bmp.GetPixel(i, j).B;
                }
            }

            return retval;
        }
    }
}
