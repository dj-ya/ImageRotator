using System;
using System.Linq;
using System.Windows;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Gat.Controls;

namespace ImageRotator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    
    // Initializing Open Dialog
    public partial class MainWindow : Window
    {
        public string pathFiles = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        public void showOpenFolderDialog(object sender, RoutedEventArgs e)
        {

            Gat.Controls.OpenDialogView openDialog = new Gat.Controls.OpenDialogView();
            Gat.Controls.OpenDialogViewModel vm = (Gat.Controls.OpenDialogViewModel)openDialog.DataContext;
            vm.IsDirectoryChooser = true;

            bool? result = vm.Show();
            if (result == true)
            {
                try
                {
                    RotateResult.Text = "";
                    pathFiles = !string.IsNullOrEmpty(vm.SelectedFilePath) ? vm.SelectedFilePath : vm.SelectedFolder.Path;
                    //MessageBox.Show(pathFiles);                   
                    Thread myThread = new Thread(new ThreadStart(rotateImages));
                    myThread.Start(); // запускаем поток                
                }
                catch (Exception ex)
                {

                }

            }
            else
            {
                MessageBox.Show("Вы не выбрали папку с изображениями!");
                //File.Text = string.Empty;
            }
        }


        public void rotateImages()
        {
            string[] fileExtations = { ".jpg", ".jpeg" };
            string[] files = Directory.GetFiles(pathFiles);
            if (files.Length > 0)
            {
                System.IO.Directory.CreateDirectory(pathFiles + "\\vertical");
                foreach (string file in files)
                {
                    if (!fileExtations.Contains(System.IO.Path.GetExtension(file).ToLower())) continue;
                    System.Drawing.Image img = System.Drawing.Image.FromFile(pathFiles + "\\" + System.IO.Path.GetFileName(file));
                    foreach (var prop in img.PropertyItems)
                    {
                        // MessageBox.Show(prop.Id.ToString());
                        if (prop.Id == 0x0112) //value of EXIF
                        {

                            int orientationValue = img.GetPropertyItem(prop.Id).Value[0];
                            RotateFlipType rotateFlipType = GetOrientationToFlipType(orientationValue);
                            img.RotateFlip(rotateFlipType);
                            prop.Value[0] = 1;
                            img.SetPropertyItem(prop);
                            try
                            {
                                img.Save(pathFiles + "\\vertical\\" + System.IO.Path.GetFileName(file), ImageFormat.Jpeg);
                                System.Windows.Application.Current.Dispatcher.Invoke(
                                   (System.Threading.ThreadStart)delegate ()
                                   {
                                       RotateResult.Text += System.IO.Path.GetFileName(file) + " - OK\n";
                                   });
                                
                            }
                            catch (Exception exception)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(
                                  (System.Threading.ThreadStart)delegate ()
                                  {
                                      RotateResult.Text += System.IO.Path.GetFileName(file) + " - ERROR\n";
                                  });
                               
                            }
                            break;
                        }
                    }
                }
                MessageBox.Show("Работа завершена");
            }
        }
        private static RotateFlipType GetOrientationToFlipType(int orientationValue)
        {
            RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone;

            switch (orientationValue)
            {
                case 1:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    rotateFlipType = RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    rotateFlipType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    rotateFlipType = RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    rotateFlipType = RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    rotateFlipType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    rotateFlipType = RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    rotateFlipType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
            }

            return rotateFlipType;
        }
    }
}
