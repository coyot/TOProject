using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            draw(@"D:\sol.txt",1.0);
            draw(@"D:\presol.txt",0.75);

        }

        private void draw(string solFileName,double opacity)
        {
            
            var instance = new List<Point>();
            string fileName = @"D:\kroB100.txt";
            if (!File.Exists(fileName))
            {
                Console.WriteLine("{0} does not exist.", fileName);
                return;
            }
            using (StreamReader sr = File.OpenText(fileName))
            {
                String input;
                while ((input = sr.ReadLine()) != null)
                {
                    instance.Add(new Point(int.Parse(input.Split(';')[1]), int.Parse(input.Split(';')[2])));
                }
            }
            fileName = solFileName;
            if (!File.Exists(fileName))
            {
                Console.WriteLine("{0} does not exist.", fileName);
                return;
            }
            using (StreamReader sr = File.OpenText(fileName))
            {
                Brush[] brushes = new Brush[4] { System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.Purple, System.Windows.Media.Brushes.Green, System.Windows.Media.Brushes.Gold };
                int brushIndex = 0;
                String input;
                int lastPos = int.Parse(sr.ReadLine()) - 1;
                int firstInCycle = lastPos;
                while ((input = sr.ReadLine()) != null)
                {
                    if (input.Equals(string.Empty))
                    {
                        Line finalLine = new Line();
                        finalLine.Stroke = Brushes.Pink;
                        finalLine.X1 = instance[lastPos].X / 4;
                        finalLine.X2 = instance[firstInCycle].X / 4;
                        finalLine.Y1 = instance[lastPos].Y / 4;
                        finalLine.Y2 = instance[firstInCycle].Y / 4;
                        finalLine.HorizontalAlignment = HorizontalAlignment.Left;
                        finalLine.VerticalAlignment = VerticalAlignment.Top;
                        finalLine.StrokeThickness = 2 * opacity * opacity;
                        myGrid.Children.Add(finalLine);

                        Ellipse myEllipse = new Ellipse();

                        // Create a SolidColorBrush with a red color to fill the 
                        // Ellipse with.
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush();

                        // Describes the brush's color using RGB values. 
                        // Each value has a range of 0-255.
                        mySolidColorBrush.Color = Color.FromArgb(128, 255, 0, 0);
                        myEllipse.Fill = mySolidColorBrush;

                        // Set the width and height of the Ellipse.
                        myEllipse.Width = 4;
                        myEllipse.Height = 4;

                        myEllipse.VerticalAlignment = VerticalAlignment.Top;
                        myEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                        myEllipse.Margin = new Thickness(instance[firstInCycle].X / 4 - 1, instance[firstInCycle].Y / 4 - 1, 0, 0);

                        // Add the Ellipse to the StackPanel.
                        myGrid.Children.Add(myEllipse);

                        if ((input = sr.ReadLine()) == null)
                            break;
                        if (input.Equals(string.Empty))
                            break;

                        lastPos = int.Parse(input) - 1;
                        firstInCycle = lastPos;
                        input = sr.ReadLine();
                        brushIndex++;

                    }
                    int pos = int.Parse(input) - 1;
                    Line myLine = new Line();
                    myLine.Stroke = brushes[brushIndex]; ;
                    myLine.X1 = instance[lastPos].X / 4;
                    myLine.X2 = instance[pos].X / 4;
                    myLine.Y1 = instance[lastPos].Y / 4;
                    myLine.Y2 = instance[pos].Y / 4;
                    myLine.HorizontalAlignment = HorizontalAlignment.Left;
                    myLine.VerticalAlignment = VerticalAlignment.Top;
                    myLine.StrokeThickness = 2 * opacity * opacity;
                    myLine.Opacity = opacity;
                    myGrid.Children.Add(myLine);
                    lastPos = pos;
                }
                sr.Close();
            }
        }
    }
}
