using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace neural_project
{
    public partial class Form1 : Form
    {
        Bitmap OriginalImage;

        // Выделение буквы
        private bool IsSelecting = false;

        // Область выделенной буквы
        private int X0, Y0, X1, Y1;

        Bitmap image;
        Bitmap grayImage;
        Bitmap shape;
        Bitmap matix;

        // Размер изображения (кратен n), цветовой порог
        int width = 60, height = 60, ident = 160;

        // Таргеты весов
        int correctTarget = 2;
        int incorrectTarget = -4;

        // Размер матрицы
        int n = 6;

        private int[] blank = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public int[] inputModel { get; set; }

        /**
         * Образ "Н"
         */
        public int[] Model1 { get; set; }

        /**
         * Образ "О"
         */
        public int[] Model2 { get; set; }

        /**
         * Образ "Ч"
         */
        public int[] Model3 { get; set; }

        /**
         * Образ "А"
         */
        public int[] Model4 { get; set; }

        /*
         * Two int arrays summator
         */
        public static int[] AddArrays(int[] a, int[] b)
        {
            return a.Zip(b, (x, y) => x + y).ToArray();
        }

        /**
         * Взвешивание весов
         */
        public int[] calcWeights(int[] model, int target)
        {
            int[] result = new int[n * n];
            for (int i = 0; i < n * n; i++)
            {
                result[i] = model[i] * target;
            }
            return result;
        }

        public Form1()
        {
            InitializeComponent();
            inputModel = blank;
            Model1 = blank;
            Model2 = blank;
            Model3 = blank;
            Model4 = blank;
        }

        /**
         * Выбор файла
         */
        private string choose_file()
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\Users\\alexe\\Pictures\\neu";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }

                return filePath;
            }
        }

        private void process_image(Bitmap image)
        {
             image = ResizeBitmap(image, width, height);

             grayImage = ToGrayScale(new Bitmap(image));

             var scannedText = new Bitmap(grayImage);

             int x, y;

             progressBar1.Value = 1;
             progressBar1.Step = 1;
             progressBar1.Minimum = 0;
             progressBar1.Maximum = grayImage.Width * grayImage.Height;

             // Loop through the images pixels to reset color.
             for (x = 0; x < grayImage.Width; x++)
             {
                 for (y = 0; y < grayImage.Height; y++)
                 {
                     Color pixel = grayImage.GetPixel(x, y);

                     // Console.WriteLine(pixel);

                     if (pixel.R < ident && pixel.G < ident && pixel.B < ident)
                     {
                         scannedText.SetPixel(x, y, Color.Black);
                     }
                     else
                     {
                         scannedText.SetPixel(x, y, Color.White);
                     }
                     progressBar1.PerformStep();
                 }
             }

             // Включить панель с изображениями
             groupBox2.Enabled = true;
             label1.Show();
             label2.Show();
             label3.Show();
             label4.Show();
             label12.Show();

             // Включить панель с обучением
             groupBox3.Enabled = true;

             // Обрабатываем shape
             shape = ResizeBitmap(Crop(scannedText), width, height);

            // Включить кнопку распознавания
            button6.Enabled = true;

            // Обрабатываем matrix
            matix = new Bitmap(shape);
             Pen pen = new Pen(Color.Red, 1);
             using (var graphics = Graphics.FromImage(matix))
             {
                 for (int i = 10; i < 70; i = i + 10)
                 {
                     graphics.DrawLine(pen, i, 0, i, height);
                     graphics.DrawLine(pen, 0, i, width, i);
                 }
             }

             // Показать изображения
             pictureBox1.Image = image;
             pictureBox2.Image = grayImage;
             pictureBox3.Image = shape;
             pictureBox4.Image = matix;
        }

        /**
         * Start selecting the rectangle.
         */
        private void pictureBox5_MouseDown(object sender, MouseEventArgs e)
        {
            IsSelecting = true;

            // Save the start point.
            X0 = e.X;
            Y0 = e.Y;
        }

        // Continue selecting.
        private void pictureBox5_MouseMove(object sender, MouseEventArgs e)
        {
            // Do nothing it we're not selecting an area.
            if (!IsSelecting) return;

            // Save the new point.
            X1 = e.X;
            Y1 = e.Y;

            // Make a Bitmap to display the selection rectangle.
            Bitmap bm = new Bitmap(OriginalImage);

            // Draw the rectangle.
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawRectangle(Pens.Red,
                    Math.Min(X0, X1), Math.Min(Y0, Y1),
                    Math.Abs(X0 - X1), Math.Abs(Y0 - Y1));
            }

            // Display the temporary bitmap.
            pictureBox5.Image = bm;
        }

        // Finish selecting the area.
        private void pictureBox5_MouseUp(object sender, MouseEventArgs e)
        {
            // Do nothing it we're not selecting an area.
            if (!IsSelecting) return;
            IsSelecting = false;

            // Display the original image.
            pictureBox5.Image = OriginalImage;

            // Copy the selected part of the image.
            int wid = Math.Abs(X0 - X1);
            int hgt = Math.Abs(Y0 - Y1);
            if ((wid < 1) || (hgt < 1)) return;

            Bitmap area = new Bitmap(wid, hgt);
            using (Graphics gr = Graphics.FromImage(area))
            {
                Rectangle source_rectangle =
                    new Rectangle(Math.Min(X0, X1), Math.Min(Y0, Y1),
                        wid, hgt);
                Rectangle dest_rectangle =
                    new Rectangle(0, 0, wid, hgt);
                gr.DrawImage(OriginalImage, dest_rectangle,
                    source_rectangle, GraphicsUnit.Pixel);
            }

            // Display the result.
            process_image(area);
        }

        /**
         * Изменить размер
         */
        public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }

        /**
         * Перевести в ЧБ
         */
        private Bitmap ToGrayScale(Bitmap bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    c = bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
            }

            return bmp;
        }

        /**
         * Обрезать пустоту вокруг текста
         */
        private Bitmap Crop(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            Func<int, bool> allWhiteRow = row =>
            {
                for (int i = 0; i < w; ++i)
                    if (bmp.GetPixel(i, row).R != 255)
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = col =>
            {
                for (int i = 0; i < h; ++i)
                    if (bmp.GetPixel(col, i).R != 255)
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                    topmost = row;
                else break;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                    bottommost = row;
                else break;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                    leftmost = col;
                else
                    break;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                    rightmost = col;
                else
                    break;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }

        /**
         * Получить бинарную матрицу цифрового изображения
         */
        private void getModelFromShape()
        {
            // Проход по чанкам
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int chunkIdent = 0;
                    int chunkFill = 0;

                    // Проход по чанку
                    for (int x = 0 + (j * 10); x < (1 + j) * 10; x++)
                    {
                        for (int y = 0 + (i * 10); y < (1 + i) * 10; y++)
                        {
                            Color pixel = this.shape.GetPixel(x, y);

                            // Console.WriteLine("Chunk " + i + "," + j + " pixel=" + pixel.ToString());

                            if (pixel.R < ident && pixel.G < ident && pixel.B < ident)
                            {
                                chunkIdent++;
                            }
                        }
                    }

                    if( chunkIdent > 10)
                    {
                        chunkFill = 1;
                    }

                    inputModel[i * n + j] = chunkFill;

                    // Console.WriteLine("Chunk " + i + "," + j + " ident=" + chunkIdent);
                }
            }
        }

        /**
         * Выбрать изображение
         */
        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = this.choose_file();

            try
            {
                OriginalImage = new Bitmap(filePath, true);
                OriginalImage = ResizeBitmap(OriginalImage, 588, 307);
                groupBox4.Enabled = true;
                pictureBox5.Image = OriginalImage;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("There was an error. Check the path to the image file.");
            }
        }

        private int countSumOfWeight(int[] model, int[] weight)
        {
            int sum = 0;
            for (int i = 0; i < n*n; i++)
            {
                sum += model[i] * weight[i];
            }
            return sum;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            getModelFromShape();

            // Суммы моделей (соотв. Model1, Model2, Model3, Model4)
            int[] sums = { 0, 0, 0, 0 };
            string[] syms = { "Н", "О", "Ч", "А" };

            sums[0] = countSumOfWeight(inputModel, Model1);
            sums[1] = countSumOfWeight(inputModel, Model2);
            sums[2] = countSumOfWeight(inputModel, Model3);
            sums[3] = countSumOfWeight(inputModel, Model4);

            int max = sums.Max();
            int index = Array.IndexOf(sums, max);

            MessageBox.Show("Распознана буква: " + syms[index]);
        }

        /**
         * Обучить "Н"
         */
        private void button2_Click(object sender, EventArgs e)
        {
            getModelFromShape();

            Model1 = AddArrays(Model1, calcWeights(inputModel, correctTarget));
            Model2 = AddArrays(Model2, calcWeights(inputModel, incorrectTarget));
            Model3 = AddArrays(Model3, calcWeights(inputModel, incorrectTarget));
            Model4 = AddArrays(Model4, calcWeights(inputModel, incorrectTarget));
           
            Console.WriteLine("Входной вектор:");
            logModel(inputModel);

            Console.WriteLine("Н:");
            logModel(Model1);

            Console.WriteLine("О:");
            logModel(Model2);

            Console.WriteLine("Ч:");
            logModel(Model3);

            Console.WriteLine("А:");
            logModel(Model4);

            label8.Text = (Int32.Parse(label8.Text) + 1).ToString();
        }

        /**
         * Обучить "О"
         */
        private void button3_Click(object sender, EventArgs e)
        {
            getModelFromShape();

            Model1 = AddArrays(Model1, calcWeights(inputModel, incorrectTarget));
            Model2 = AddArrays(Model2, calcWeights(inputModel, correctTarget));
            Model3 = AddArrays(Model3, calcWeights(inputModel, incorrectTarget));
            Model4 = AddArrays(Model4, calcWeights(inputModel, incorrectTarget));

            Console.WriteLine("Входной вектор:");
            logModel(inputModel);

            Console.WriteLine("Н:");
            logModel(Model1);

            Console.WriteLine("О:");
            logModel(Model2);

            Console.WriteLine("Ч:");
            logModel(Model3);

            Console.WriteLine("А:");
            logModel(Model4);

            label9.Text = (Int32.Parse(label9.Text) + 1).ToString();
        }

        /**
         * Обучить "Ч"
         */
        private void button4_Click(object sender, EventArgs e)
        {
            getModelFromShape();

            Model1 = AddArrays(Model1, calcWeights(inputModel, incorrectTarget));
            Model2 = AddArrays(Model2, calcWeights(inputModel, incorrectTarget));
            Model3 = AddArrays(Model3, calcWeights(inputModel, correctTarget));
            Model4 = AddArrays(Model4, calcWeights(inputModel, incorrectTarget));

            Console.WriteLine("Входной вектор:");
            logModel(inputModel);

            Console.WriteLine("Н:");
            logModel(Model1);

            Console.WriteLine("О:");
            logModel(Model2);

            Console.WriteLine("Ч:");
            logModel(Model3);

            Console.WriteLine("А:");
            logModel(Model4);

            label10.Text = (Int32.Parse(label10.Text) + 1).ToString();
        }

        /**
         * Обучить "А"
         */
        private void button5_Click(object sender, EventArgs e)
        {
            getModelFromShape();

            Model1 = AddArrays(Model1, calcWeights(inputModel, incorrectTarget));
            Model2 = AddArrays(Model2, calcWeights(inputModel, incorrectTarget));
            Model3 = AddArrays(Model3, calcWeights(inputModel, incorrectTarget));
            Model4 = AddArrays(Model4, calcWeights(inputModel, correctTarget));

            Console.WriteLine("Входной вектор:");
            logModel(inputModel);

            Console.WriteLine("Н:");
            logModel(Model1);

            Console.WriteLine("О:");
            logModel(Model2);

            Console.WriteLine("Ч:");
            logModel(Model3);

            Console.WriteLine("А:");
            logModel(Model4);

            label11.Text = (Int32.Parse(label11.Text) + 1).ToString();
        }

        private void logModel(int[] model)
        {
            for (int i = 0; i < n * n; i++)
            {
                Console.Write(model[i] + " ");
                if ((i + 1) % n == 0 && i > 0)
                {
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
    }
}
