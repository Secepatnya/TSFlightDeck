using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Razzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private mControllerPanel controls;

        public MainWindow()
        {
            InitializeComponent();

            controls = new mControllerPanel();

            //status bar set context
            statAP.DataContext = controls.ap;
            statSat.DataContext = controls.satellite;
            statRec.DataContext = controls.satellite;
            statMusic1.DataContext = controls.player1;
            statMusic2.DataContext = controls.player2;
            modePlayer.DataContext = controls.player1;

            //sliders set context
            volSliderSat.DataContext = controls.satellite;
            volSliderPlayer1.DataContext = controls.player1;
            volSliderPlayer2.DataContext = controls.player2;
            volSliderPlayer3.DataContext = controls.player3;

            //players set context
            Playlist1.ItemsSource = controls.player1.playlist;
            Playlist2.ItemsSource = controls.player2.playlist;
            Playlist3.ItemsSource = controls.player3.playlist;
        }

        /* Sat Controls */
        private void buttonSatStart(object sender, RoutedEventArgs e)
        {
            controls.satellite.Start();
        }

        private void buttonSatStop(object sender, RoutedEventArgs e)
        {
            controls.satellite.Stop();
        }

        private void buttonSatRecStart(object sender, RoutedEventArgs e)
        {
            controls.satellite.RecordStart();
        }

        private void buttonSatRecStop(object sender, RoutedEventArgs e)
        {
            controls.satellite.RecordStop();
        }

        /* AP Controls */
        private void buttonAPStart(object sender, RoutedEventArgs e)
        {
            controls.ap.autoStart();
        }

        private void buttonAPStop(object sender, RoutedEventArgs e)
        {
            controls.ap.autoStop();
        }

        /* Mixers */
        // Satellite 
        private void faderSat(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controls.satellite.SetVolume((float)e.NewValue);
        }

        // Music Player
        private void faderPlayer1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controls.player1.SetVolume((float)e.NewValue);
        }

        private void buttonPlayPlayer1(object sender, RoutedEventArgs e)
        {
            controls.player1.Start(true);
        }

        private void buttonStopPlayer1(object sender, RoutedEventArgs e)
        {
            controls.player1.Stop();
        }

        private void buttonPrevPlayer1(object sender, RoutedEventArgs e)
        {
            controls.player1.skip(false, true);
        }

        private void buttonNextPlayer1(object sender, RoutedEventArgs e)
        {
            controls.player1.skip(true, true);
        }

        private void buttonAddPlayer1(object sender, RoutedEventArgs e)
        {
            controls.player1.browseFile();
        }

        private void buttonDelPlayer1(object sender, RoutedEventArgs e)
        {
            controls.player1.delTracks(Playlist1.SelectedItems);
        }

        private void Playlist1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                controls.player1.selectTrack((musicItem)e.AddedItems[0]);
            }
        }

        private void boxDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                foreach (string item in files)
                {
                    controls.player1.addTrack(item);
                }
            }
        }

        // Interfaces player 2

        private void boxDrop2(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                foreach (string item in files)
                {
                    controls.player2.addTrack(item);
                }
            }
        }

        // Music Player secondary 
        private void faderPlayer2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controls.player2.SetVolume((float)e.NewValue);
        }

        private void buttonPlayPlayer2(object sender, RoutedEventArgs e)
        {
            controls.player2.Start(true);
        }


        private void buttonDuckPlayPlayer2(object sender, RoutedEventArgs e)
        {
            controls.P2StartQuacking();
        }

        private void buttonStopPlayer2(object sender, RoutedEventArgs e)
        {
            controls.player2.Stop();
        }


        private void buttonAddPlayer2(object sender, RoutedEventArgs e)
        {
            controls.player2.browseFile();
        }

        private void buttonDelPlayer2(object sender, RoutedEventArgs e)
        {
            controls.player2.delTracks(Playlist2.SelectedItems);
        }

        private void Playlist2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                controls.player2.selectTrack((musicItem)e.AddedItems[0]);
            }
        }

        // Interfaces player 3

        private void faderPlayer3(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controls.player3.SetVolume((float)e.NewValue);
        }

        private void buttonPlayPlayer3(object sender, RoutedEventArgs e)
        {
            controls.player3.Start(true);
        }

        private void buttonStopPlayer3(object sender, RoutedEventArgs e)
        {
            controls.player3.Stop();
        }

        private void buttonAddPlayer3(object sender, RoutedEventArgs e)
        {
            controls.player3.browseFile();
        }

        private void buttonDelPlayer3(object sender, RoutedEventArgs e)
        {
            controls.player3.delTracks(Playlist3.SelectedItems);
        }

        private void Playlist3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                controls.player3.selectTrack((musicItem)e.AddedItems[0]);
            }
        }

        private void boxDrop3(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                foreach (string item in files)
                {
                    controls.player3.addTrack(item);
                }
            }
        }


        // Window controls
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            string msg = "Are you sure you want to close this program?";
            MessageBoxResult result =
                MessageBox.Show(
                    msg,
                    "Confirm Shutdown",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                // If user doesn't want to close, cancel closure
                e.Cancel = true;
            }
            else
            {
                controls.shutdown();
            }
        }

        private void buttonPlaySingle(object sender, RoutedEventArgs e)
        {
            controls.player1.selectedPlayMode = sourcePlayer.playModes.Single;
        }

        private void buttonPlayRepeatNormal(object sender, RoutedEventArgs e)
        {
            controls.player1.selectedPlayMode = sourcePlayer.playModes.Normal;
        }

        private void buttonPlayRepeatOne(object sender, RoutedEventArgs e)
        {
            controls.player1.selectedPlayMode = sourcePlayer.playModes.RepeatOne;
        }

        private void buttonPlayContinous(object sender, RoutedEventArgs e)
        {
            controls.player1.selectedPlayMode = sourcePlayer.playModes.Continous;
        }

        private void buttonPlayNormal(object sender, RoutedEventArgs e)
        {
            controls.player1.selectedPlayMode = sourcePlayer.playModes.Normal;
        }

        private void buttonClearList(object sender, RoutedEventArgs e)
        {
            controls.player1.clearPlaylist();
        }

        /* Menu Controls */
        private void MenuHelpUsermanual(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"manual.pdf");
            }
            catch
            {
                MessageBox.Show("Manual file is missing. Contact the Staff Chat for help.");
            }
        }

        private void MenuHelpAbout(object sender, RoutedEventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.Show();
        }

        private void MenuFileExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuFileTSPerms(object sender, RoutedEventArgs e)
        {
            RazzleSettings RSettingsWin = new RazzleSettings();
            RSettingsWin.ShowDialog();

        }

        private void MenuFileReset(object sender, RoutedEventArgs e)
        {
            controls.resetOutput();
        }


    }
}
