using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    
    public partial class MainWindow : Window
    {
        private Bitmap transformedBitmap;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                BitmapImage originalImage = new BitmapImage(new Uri(filePath));

                // Convert the BitmapImage to Bitmap
                Bitmap bitmap = BitmapImageToBitmap(originalImage);
                // 1. Resize the image to 240x320
                Bitmap resizedImage = ResizeImage(bitmap, 240, 320);

                // 2. Apply grayscale filter
                Bitmap grayScaleImage = ApplyGrayscale(resizedImage);

                // 3. Crop the image to focus on the face
                Bitmap croppedImage = CropImageToFace(grayScaleImage);

                // 4. Convert image to an oval shape as marked
                Bitmap ovalImage = ConvertToCustomOval(croppedImage);

                // Display the final image
                ImageDisplay.Source = BitmapToBitmapImage(ovalImage);
                transformedBitmap = ConvertToCustomOval(ovalImage);
            }
        }

        private Bitmap ResizeImage(Bitmap bitmap, int width, int height)
        {
            Bitmap resizedBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedBitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, width, height);
            }
            return resizedBitmap;
        }

        private Bitmap ApplyGrayscale(Bitmap bitmap)
        {
            Bitmap grayscaleBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    System.Drawing.Color originalColor = bitmap.GetPixel(x, y);
                    int grayscaleValue = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                    System.Drawing.Color grayscaleColor = System.Drawing.Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
                    grayscaleBitmap.SetPixel(x, y, grayscaleColor);
                }
            }
            return grayscaleBitmap;
        }

        private Bitmap CropImageToFace(Bitmap bitmap)
        {
            // Crop to the area around the face, keeping more space at the top and less at the bottom
            int newHeight = (int)(bitmap.Height * 0.7);  // Use 80% of the height
            int startY = (int)(bitmap.Height * 0.039);   // Start cropping 5% from the top

            Rectangle cropArea = new Rectangle(0, startY, bitmap.Width, newHeight);
            Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
            return croppedBitmap;
        }

        private Bitmap ConvertToCustomOval(Bitmap bitmap)
        {
            Bitmap ovalBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            using (Graphics g = Graphics.FromImage(ovalBitmap))
            {
                // Set the background to white
                g.Clear(System.Drawing.Color.White);

                // Create a custom oval shape like the marked red oval
                GraphicsPath path = new GraphicsPath();

                // Adjust the oval shape to match the example
                int ovalWidth = bitmap.Width - 40; // Decrease the width slightly to match the red oval
                int ovalHeight = (int)(bitmap.Height * 1.2); // Increase the height more vertically
                int ovalX = 20; // Center the oval horizontally
                int ovalY = -20; // Move the oval slightly upwards to fit the face properly

                path.AddEllipse(ovalX, ovalY, ovalWidth, ovalHeight);
                g.SetClip(path);
                g.DrawImage(bitmap, 0, 0);
            }
            return ovalBitmap;
        }

        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            // Create a SaveFileDialog to ask the user where to save the file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp",
                Title = "Save the Transformed Image"
            };

            // Show the dialog and get the user's chosen file path
            if (saveFileDialog.ShowDialog() == true)
            {
                // Resize the transformed image to 240x320 pixels
                Bitmap resizedBitmap = ResizeImage(transformedBitmap, 240, 320);

                // Save the resized image to the chosen path
                resizedBitmap.Save(saveFileDialog.FileName, ImageFormat.Jpeg); // Save in JPEG format or change based on file extension
                MessageBox.Show("Image saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}