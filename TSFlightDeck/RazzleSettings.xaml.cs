using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Razzle
{
    /// <summary>
    /// Interaction logic for TSPerms.xaml
    /// </summary>
    public partial class RazzleSettings : Window
    {

        ObservableCollection<tsuser> windowlist;

        public RazzleSettings()
        {
            InitializeComponent();

            windowlist = new ObservableCollection<tsuser>(tsInterface.allowuid);

            UserList.ItemsSource = windowlist;
            directoryf.Text = sourceSatellite.directory;
        }

        private void Button_Add(object sender, RoutedEventArgs e)
        {
            addUser(namef.Text, uidf.Text);
            namef.Text = "";
            uidf.Text = "";
        }

        private void Button_Remove(object sender, RoutedEventArgs e)
        {
            delUser(UserList.SelectedItems);
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            sourceSatellite.setRecordDirectory(directoryf.Text);
            tsInterface.allowuid = windowlist;
            Close();
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void addUser(string name, string uid)
        {
            windowlist.Add(
                new tsuser
                {
                    name = name,
                    uid = uid
                });
        }

        public void delUser(System.Collections.IList list)
        {
            List<tsuser> tempList = new List<tsuser>();
            foreach (tsuser item in list)
            {
                tempList.Add(item);
            }

            foreach (tsuser item in tempList)
            {
                windowlist.Remove(item);
            }
        }
    }
}
