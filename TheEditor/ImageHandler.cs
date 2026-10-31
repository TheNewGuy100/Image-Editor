using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TheEditor
{

    public class ImageHandler
    {
        public string imageDataString;
        public Image imageData;
        public Image DefaultRef;
        public Control imageControllerPreview;
        public Histogram HistogramModule;

        private int offsetNoise = 1;

        public void increaseOffsetNoise(int value)
        {
            this.offsetNoise = value;
        }

        public ImageHandler(Image data, Control controllerRef)
        {
            this.imageControllerPreview = controllerRef;
            this.DefaultRef = data;
            this.updateImageRef(data);
        }

        public Color[][] GetBitmapMatrix()
        {
            Bitmap bitmapToConvert = (Bitmap)DefaultRef;

            int height = bitmapToConvert.Height;
            int width = bitmapToConvert.Width;

            Color[][] colorMatrix = new Color[width][];

            for(int i = 0; i < width; i++)
            {
                colorMatrix[i] = new Color[height];
                for (int j = 0; j < height; j++)
                {
                    colorMatrix[i][j] = bitmapToConvert.GetPixel(i, j);
                }
            }

            return colorMatrix;
        }

        public void updateImageRef(Image data)
        {
            this.imageDataString = data.ToString();
            this.imageData = data;
            this.imageControllerPreview.BackgroundImage = data;
        }

        public void convertImageToBlackAndWhite(double conversionValue)
        {
            // gets decoded bitmap
            Color[][] matrix = GetBitmapMatrix();

            // creates a new bitmap
            Bitmap convertedBitmap = new Bitmap(matrix.GetLength(0), matrix[0].GetLength(0));

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix[0].GetLength(0); j++)
                {
                    Color colorBlock = matrix[i][j];

                    int rgb = (int) Math.Round((
                        (0.299 * conversionValue) * colorBlock.R +
                        (0.587 * conversionValue) * colorBlock.G +
                        (0.114 * conversionValue) * colorBlock.B
                    ));

                    int blendR = (int) ((rgb * conversionValue) + (colorBlock.R * (1 - conversionValue)));
                    int blendG = (int) ((rgb * conversionValue) + (colorBlock.G * (1 - conversionValue)));
                    int blendB = (int) ((rgb * conversionValue) + (colorBlock.B * (1 - conversionValue)));

                    convertedBitmap.SetPixel(i, j, Color.FromArgb(blendR, blendG, blendB));
                }
            }

            this.updateImageRef(convertedBitmap);
            this.imageControllerPreview.Refresh();
            Console.WriteLine("Finished black/white transition...");
        }

        public void rotateImage90degrees()
        {
            Console.WriteLine("Starting image rotation...");

            imageData.RotateFlip(RotateFlipType.Rotate90FlipNone);

            this.imageControllerPreview.Refresh();
            Console.WriteLine("Finished image rotation...");
        }
        public void MirrorImageH()
        {
            Console.WriteLine("Starting image mirroring...");

            imageData.RotateFlip(RotateFlipType.RotateNoneFlipX);

            this.imageControllerPreview.Refresh();
            Console.WriteLine("Finished image rotation...");
        }
        public void MirrorImageV()
        {
            Console.WriteLine("Starting image mirroring...");

            imageData.RotateFlip(RotateFlipType.RotateNoneFlipY);

            this.imageControllerPreview.Refresh();
            Console.WriteLine("Finished image rotation...");
        }

        /// <summary>Mediana de uma lista já ordenada (média dos dois centrais quando a contagem é par).</summary>
        private int getMedianPoint(List<int> elementos)
        {
            int middle = elementos.Count / 2;

            if (elementos.Count % 2 == 0)
            {
                return (elementos[middle - 1] + elementos[middle]) / 2;
            }

            return elementos[middle];
        }

        public Color CalculateMedianPoint(List<Color> Values)
        {
            // ALPHA, RED, GREEN, BLUE

            if (Values.Count == 0)
                return default(Color);

            List<int> red = new List<int>();
            List<int> green = new List<int>();
            List<int> blue = new List<int>();

            foreach (Color element in Values)
            {
                red.Add(element.R);
                green.Add(element.G);
                blue.Add(element.B);
            }

            red.Sort();
            green.Sort();
            blue.Sort();

            int medianRed = this.getMedianPoint(red);
            int medianGreen = this.getMedianPoint(green);
            int medianBlue = this.getMedianPoint(blue);

            return Color.FromArgb(medianRed, medianGreen, medianBlue);
        }

        public void CauseNoiseReduction()
        {
            Console.WriteLine("Noise reduction running...");

            // gets decoded bitmap
            Color[][] matrix = GetBitmapMatrix();

            // creates a new bitmap
            Bitmap convertedBitmap = new Bitmap(matrix.GetLength(0), matrix[0].GetLength(0));

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix[0].GetLength(0); j++)
                {
                    Color currentElement = matrix[i][j];
                    // vizinhos a distância "offsetNoise"; os que caem fora da imagem são ignorados
                    List<Color> items = new List<Color> { currentElement };

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int x = i + dx * this.offsetNoise;
                            int y = j + dy * this.offsetNoise;

                            if (x >= 0 && x < matrix.Length && y >= 0 && y < matrix[0].Length)
                            {
                                items.Add(matrix[x][y]);
                            }
                        }
                    }

                    // MEDIANA POR CANAL ENTRE OS PIXELS
                    Color medianPoint = this.CalculateMedianPoint(items);

                    convertedBitmap.SetPixel(i, j, medianPoint);
                }
            }

            this.updateImageRef(convertedBitmap);
            this.imageControllerPreview.Refresh();
            Console.WriteLine("Noise reduction finished.");
        }

        public String ApplyRobertsCrossGradient()
        {
            // gets decoded bitmap
            Color[][] matrix = GetBitmapMatrix();

            // creates a new bitmap
            Bitmap convertedBitmap = new Bitmap(matrix.GetLength(0), matrix[0].GetLength(0));

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix[0].GetLength(0); j++)
                {
                    Color currentElement = matrix[i][j];
                    // nas bordas, repete o pixel mais próximo (sem "pixels pretos" fantasmas)
                    int nextX = Math.Min(i + 1, matrix.Length - 1);
                    int nextY = Math.Min(j + 1, matrix[0].Length - 1);

                    Color right = matrix[nextX][j];
                    Color bottom = matrix[i][nextY];
                    Color bottomRight = matrix[nextX][nextY];

                    // left to right
                    int redX = Math.Abs(currentElement.R - bottomRight.R);
                    int greenX = Math.Abs(currentElement.G - bottomRight.G);
                    int blueX = Math.Abs(currentElement.B - bottomRight.B);

                    // right to left
                    int redY = Math.Abs(right.R - bottom.R);
                    int greenY = Math.Abs(right.G - bottom.G);
                    int blueY = Math.Abs(right.B - bottom.B);

                    // get max
                    int red = Math.Max(redX, redY);
                    int green = Math.Max(greenX, greenY);
                    int blue = Math.Max(blueX, blueY);

                    Color edgeColor = Color.FromArgb(red, green, blue);
                    convertedBitmap.SetPixel(i, j, edgeColor);
                }
            }

            this.updateImageRef(convertedBitmap);
            this.imageControllerPreview.Refresh();
            return "Roberts Border detector applied, results on screen.";
        }

    }
}
