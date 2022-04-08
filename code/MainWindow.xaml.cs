using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HorseRace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        //members
        static Random rand = new Random();
        static BrushConverter brush_converter = new BrushConverter();
        public static double TotalCash = 1000.00;
        public static double CurrentBet = 0;
        DispatcherTimer timer;
        List<Horse> finished = new List<Horse>();

        public List<Horse> AllHorses = new List<Horse>
        {
            new Horse("Blinky", Colors.Red, rand),
            new Horse("Pinky", Colors.Magenta, rand),
            new Horse("Inky", Colors.DarkCyan, rand),
            new Horse("Clyde", Colors.DarkOrange, rand),
            new Horse("Betsy", Colors.DarkBlue, rand),
            new Horse("Izzy", Colors.Yellow, rand),
            new Horse("Poe", Colors.Black, rand),
            new Horse("Ignatz", Colors.YellowGreen, rand),
            new Horse("Boy Blue", Colors.Purple, rand),
            new Horse("Forrest", Colors.ForestGreen, rand)
        };
        public List<Horse> HorseList = new List<Horse>();

        Line starting_line = new Line();
        Line finishing_line = new Line();

        //construction
        public MainWindow()
        {
            InitializeComponent();
            TB_Bet.IsEnabled = true;
            CB_Horses.IsEnabled = true;

            int num_horses = rand.Next(2, 7); //selects number of horses
            int rand_index;
            for (int i = 0; i < num_horses; i++)
            {
                rand_index = rand.Next(AllHorses.Count);
                HorseList.Add(AllHorses[rand_index]);
                AllHorses.RemoveAt(rand_index);
                HorseList[i].Coords = new Point(HorseList[i].Coords.X, HorseList[i].Coords.Y + i * 150/(num_horses-1)); //change y pos
            }

            FixHorseOdds();
            for (int i = 0; i < num_horses; i++)
            {
                ComboBoxItem current_CBI = new ComboBoxItem();
                current_CBI.Content = HorseList[i].Name;
                current_CBI.Foreground = (Brush)brush_converter.ConvertFromString(HorseList[i].Colour.ToString());
                CB_Horses.Items.Add(current_CBI);
            }

            TK_TotalCash.Text = "$" + TotalCash.ToString();

            Loaded += delegate
            {
                Horse.TrackLength = BR_RaceScreen.ActualWidth;

                MakeTrack();

                ResetHorseXPostitions();
                MakeIMGs();
            };
        }
        private void FixHorseOdds()
        {
            float sum = 0;
            foreach(Horse i in HorseList)
            {
                sum += i.Odds;
            }
            foreach(Horse i in HorseList)
            {
                i.Odds = (float)Math.Round(i.Odds / (double)sum, 4);
            }
        }
        void MakeIMGs()
        {
            foreach (Horse i in HorseList)
            {
                CV_RaceScreen.Children.Add(i.IMG);
            }
        }
        void MakeTrack()
        {
            Rectangle rect = new Rectangle();
            rect.Margin = new Thickness() { Top = 120 };
            rect.Height = 300;
            rect.Width = BR_RaceScreen.ActualWidth;
            rect.Fill = Brushes.SandyBrown;
            CV_RaceScreen.Children.Add(rect);

            starting_line.X1 = 100;
            starting_line.Y1 = 120;
            starting_line.X2 = 200;
            starting_line.Y2 = 420;
            starting_line.StrokeThickness = 5;
            starting_line.Stroke = Brushes.Brown;
            CV_RaceScreen.Children.Add(starting_line);
            
            finishing_line.X1 = (int)Horse.TrackLength - starting_line.X2;
            finishing_line.Y1 = starting_line.Y1;
            finishing_line.X2 = (int)Horse.TrackLength - starting_line.X1;
            finishing_line.Y2 = starting_line.Y2;
            finishing_line.StrokeThickness = 5;
            finishing_line.Stroke = Brushes.Brown;
            CV_RaceScreen.Children.Add(finishing_line);

        }
         
        //WPF Methods
        private void CB_Horses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_Horses.SelectedItem != null)
            {
                Horse selected_horse = HorseList[CB_Horses.SelectedIndex];
                TK_HorseOdds.Text = (selected_horse.Odds * 100).ToString() + "%";
                if (TB_Bet.Text != "")
                {
                    BU_GO.IsEnabled = true;
                }
                else
                {
                    BU_GO.IsEnabled = false;
                }
            }
        }
        private void TB_Bet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CB_Horses.SelectedItem != null)
            {
                BU_GO.IsEnabled = true;
            }
            else
            {
                BU_GO.IsEnabled = false;
            }
        }
        private void BU_GO_Click(object sender, RoutedEventArgs e)
        {
            BU_GO.IsEnabled = false;
            TB_Bet.IsEnabled = false;
            CB_Horses.IsEnabled = false;

            finished = new List<Horse>();

            try
            {
                CurrentBet = double.Parse(TB_Bet.Text);

                if (TotalCash >= CurrentBet && CurrentBet > 0)
                {
                    TotalCash -= CurrentBet;
                    TK_TotalCash.Text = "$" + TotalCash.ToString();
                }
                else
                {
                    int.Parse("Lol no");
                }

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(15);
                timer.Tick += timer_Tick;
                timer.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Your betting amount didn't make sense. \nRemember to only use a number amount less than your total cash!");
                BU_GO.IsEnabled = true;
                TB_Bet.IsEnabled = true;
                CB_Horses.IsEnabled = true;
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            bool everyoneFinished = true;
            foreach (Horse i in HorseList)
            {
                if (!(i.Coords.X > Horse.LineX(i.Coords.Y, finishing_line)))
                {
                    i.Move();
                    everyoneFinished = false;
                    if (i.Coords.X > Horse.LineX(i.Coords.Y, finishing_line))
                    {
                        finished.Add(i);
                    }
                }
            }
            if (everyoneFinished)
            {
                timer.Stop();
                TB_Bet.IsEnabled = true;
                CB_Horses.IsEnabled = true;

                string dispMesg = "The race is over!\nHere's the standings:";
                for (int i = 0; i < finished.Count; i++)
                {
                    dispMesg += "\n#" + (i + 1).ToString() + " - " + finished[i].Name;
                }
                MessageBox.Show(dispMesg);

                CurrentBet = BetReturnAmount();
                TotalCash += Math.Round(CurrentBet, 2);
                CurrentBet = 0;
                TK_TotalCash.Text = "$" + Math.Round(TotalCash, 2).ToString();

                NewRace();
                return;
            }
        }

        //Other Methods
        void ResetHorseXPostitions()
        {
            foreach(Horse horse in HorseList)
            {
                horse.Coords = new Point(Horse.LineX(horse.Coords.Y, starting_line), horse.Coords.Y);
                horse.UpdateImage();
            }
        }
        void NewRace()
        {
            rand = new Random();
            
            //Reset Horses
            AllHorses = new List<Horse>
            {
            new Horse("Blinky", Colors.Red, rand),
            new Horse("Pinky", Colors.Magenta, rand),
            new Horse("Inky", Colors.DarkCyan, rand),
            new Horse("Clyde", Colors.DarkOrange, rand),
            new Horse("Betsy", Colors.DarkBlue, rand),
            new Horse("Izzy", Colors.Yellow, rand),
            new Horse("Poe", Colors.Black, rand),
            new Horse("Ignatz", Colors.YellowGreen, rand),
            new Horse("Boy Blue", Colors.Purple, rand),
            new Horse("Forrest", Colors.ForestGreen, rand)
            };
            HorseList = new List<Horse>();

            int num_horses = rand.Next(2, 7); //selects number of horses

            int rand_index;
            for (int i = 0; i < num_horses; i++)
            {
                rand_index = rand.Next(AllHorses.Count);
                HorseList.Add(AllHorses[rand_index]);
                AllHorses.RemoveAt(rand_index);
                HorseList[i].Coords = new Point(HorseList[i].Coords.X, HorseList[i].Coords.Y + i * 150 / (num_horses - 1)); //change y pos
            }

            //Update and Reset WPF
            TB_Bet.IsEnabled = true;
            TB_Bet.Clear();
            CB_Horses.IsEnabled = true;
            CB_Horses.SelectedItem = null;

            FixHorseOdds();
            CB_Horses.Items.Clear();
            for (int i = 0; i < num_horses; i++)
            {
                ComboBoxItem current_CBI = new ComboBoxItem();
                current_CBI.Content = HorseList[i].Name;
                current_CBI.Foreground = (Brush)brush_converter.ConvertFromString(HorseList[i].Colour.ToString());
                CB_Horses.Items.Add(current_CBI);
            }

            TK_TotalCash.Text = "$" + TotalCash.ToString();
            TK_HorseOdds.Text = "0%";

            CV_RaceScreen.Children.Clear();
            MakeTrack();
            MakeIMGs();
            ResetHorseXPostitions();

            BU_GO.IsEnabled = false;

        } //Resets game
        double BetReturnAmount() //My other masterpiece 
        {
            //Formula explanations and graphs:
            //https://www.desmos.com/calculator/fbuinbmp4n - Original Equation
            //https://www.desmos.com/calculator/xnr5e9afhv - New Equation using Placement Subtrahend

            double bet = CurrentBet; //Amount bet
            int place = 4; //Placing of Selected Horse
            double odd = 1; //Odds of Selected Horse

            for(int i = 0; i < finished.Count; i++) //Update place and odd
            {
                if (finished[i].Name == HorseList[CB_Horses.SelectedIndex].Name)
                {
                    place = i + 1;
                    odd = finished[i].Odds;
                    break;
                }
            }

            double PlaceSubtrahend = (-1D / (HorseList.Count - 1)) * (place - 1) + 2; //(1,2), (2,5/3), (3,4/3), (4,1) - The amount subtracted from based on placement
            return bet * (PlaceSubtrahend - odd);

        } //Gives a the new amount generated from bet after race
    }


    //A HORSE IS A HORSE, OF COURSE, OF COURSE.
    public class Horse
    {
        //members
        string name;
        Color colour;
        float odds;
        float raw_odds;
        double speed;
        Point coords = new Point(100,120);
        Image image;

        static double tracklength = 200;
        static double speedfactor = 0.015;

        //get set
        public string Name
        {
            get => name;
        }
        public Color Colour
        {
            get => colour;
            set
            {
                colour = value;
            }
        }
        public float Odds
        {
            get => odds;
            set
            {
                odds = value;
            }
        }
        public float RawOdds
        {
            get => raw_odds;
        }
        public Image IMG
        {
            get => image;
        }
        public Point Coords
        {
            get => coords;
            set
            {
                coords.X = value.X;
                coords.Y = value.Y;
            }
        }
        public static double TrackLength
        {
            get => tracklength;
            set
            {
                tracklength = value;
            }
        }

        //construction
        public Horse(string horse_name, Color horse_colour, Random seed)
        {
            name = horse_name;
            colour = horse_colour;
            speed = seed.NextDouble(); //Random double
            odds = (float)Math.Round(seed.NextDouble() * speed, 4); //Random float 0.XX
            raw_odds = odds;
            image = new Image();
            Uri temp_uri;
            try
            {
                string path = @"horseIMGscaled.gif";
                temp_uri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, path));
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Unable to use image file \"horseIMGscaled.gif\"!");
                throw;
            } //retrieve image uri
            try
            {
                image.Source = ColouringTool(temp_uri, colour);
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Colouring tool broke");
                throw;
            } //colourize image
            UpdateImage();
        }
        BitmapSource ColouringTool(Uri image_uri, Color image_colour) //My Masterpiece
        {
            BitmapImage bitmap = new BitmapImage(image_uri); //original bitmap (as gif, since gifs are compatable with BitmapPalettes)
            int bitmap_height = bitmap.PixelHeight; //bitmap height
            int bitmap_width = bitmap.PixelWidth; //bitmap width
            PixelFormat pixel_format = PixelFormats.Indexed8; //bitmap pixel format (how each byte describes a pixel)
            int stride = bitmap_width * ((bitmap.Format.BitsPerPixel + 7) / 8); //the width of the image in bytes

            List<Color> original_palette = bitmap.Palette.Colors.ToList();

            //Define colour palette
            List<Color> palette_list = new List<Color>() //new Color list, starting with transparent
            {
                Colors.Transparent
            };
            for (int i = 1; i < original_palette.Count; i++) //replaces each colour in the original palette with the wanted Color
            {
                palette_list.Add(image_colour);
            }
            BitmapPalette palette = new BitmapPalette(palette_list); //new palette using Color list
            
            //Make raw pixel data
            WriteableBitmap w_bitmap = new WriteableBitmap(bitmap); //new writeable bitmap from original bitmap
            byte[] raw_pixel_data = new byte[bitmap_height * stride]; //new byte array with correct size based on bitmap stride and height
            w_bitmap.CopyPixels(raw_pixel_data, stride, 0); //copy pixels from writeable bitmap to byte array

            //Create new bitmap with original pixel data and new palette
            BitmapSource s_bitmap = BitmapSource.Create(bitmap_width, bitmap_height, bitmap.DpiX, bitmap.DpiY, pixel_format, palette, raw_pixel_data, stride);
            return s_bitmap;
        }

        //methods
        public void Move()
        {
            coords.X += (tracklength * (Math.Log10(speed + 2)) * speedfactor);
            UpdateImage();
        }
        public static double LineY(double x, Line line)
        {
            double m = (line.Y2 - line.Y1) / (line.X2 - line.X1);
            double b = (line.Y1 - (m * line.X1));

            return (m * x) + b;
        }
        public static double LineX(double y, Line line)
        {
            double m = (line.Y2 - line.Y1) / (line.X2 - line.X1);
            double b = (line.Y1 - (m * line.X1));

            return (y - b) / m;
        }
        public void UpdateImage()
        {
            image.Margin = new Thickness()
            {
                Top = coords.Y,
                Left = coords.X - 50
            };
        }
    }
    
}