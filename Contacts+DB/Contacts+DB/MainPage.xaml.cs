using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Contacts_DB.Resources;
using Microsoft.Phone.UserData;
using System.Windows.Media.Imaging;
using SQLite;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AP.ViewModel;
using AP.Interactions;
using Microsoft.Xna.Framework;
using Microsoft.Phone.Tasks;

namespace Contacts_DB
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        //private SQLiteConnection dbConn;
        private InteractionManager _interactionManager = new InteractionManager();
        public MainPage()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();
            InitializeComponent();
            if (!store.FileExists("FirstRun.txt"))
                FirstRun();
            else
            retrievingData();

        }

        

        private void retrievingData()
        {
            ObservableCollection<ToDoItemViewModel> _todoItems = new ObservableCollection<ToDoItemViewModel>();

            using (var db = new SQLiteConnection(dbPath))
            {
                List<Person> existing = db.Query<Person>("select * from Person").ToList();
                if (existing != null)
                {
                    for (int i = 0; i < existing.Count; i++)
                    {
                        BitmapImage b = BytesToImage(existing[i].Images);
                        _todoItems.Add(new ToDoItemViewModel { Text = existing[i].Names, Text1 = existing[i].Numbers, Text2 = b, Text3 = existing[i].Id.ToString() });
                    }
                }
            }

            ContactList.DataContext = _todoItems;
            var swipeInteraction = new SwipeInteraction();
            swipeInteraction.Initialise(ContactList, _todoItems);
            _interactionManager.AddInteraction(swipeInteraction);
            FrameworkDispatcher.Update();
            
        }

        private void FirstRun()
        {
            MessageBox.Show("First run ;)");
            pb.Visibility = Visibility.Visible;
            Contacts cons = new Contacts();
            cons.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(Contacts_SearchCompleted);
            cons.SearchAsync(String.Empty, FilterKind.None, "Contacts Test #1");
        }


        public static string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
        private void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            int i = 0;
            byte[] dbimg = {0};
            foreach (Contact con in e.Results)
            {

                try
                {
                    GlobleContacts.Names.Add(con.DisplayName);
                }
                catch
                {
                    GlobleContacts.Names.Add("(No Name)");
                }
                try
                {
                    string temp = con.PhoneNumbers.FirstOrDefault().ToString();
                    string number = "";

                    int k = 0;
                    while (temp[k] != '(')
                    {
                        number += temp[k];
                        k++;
                    }
                    GlobleContacts.Numbers.Add(number);
                }
                catch
                {
                    GlobleContacts.Numbers.Add("(No Number)");
                }
                try
                {
                    var a = con.EmailAddresses.ToList();
                    GlobleContacts.Emails.Add(a[0].EmailAddress.ToString());
                }
                catch
                {
                    GlobleContacts.Emails.Add("(No Email Address)");
                }
                try
                {
                    var g = e.Results.ToList();
                    BitmapImage img = new BitmapImage();
                    img.SetSource(g[i].GetPicture());
                    GlobleContacts.Images.Add(img);
                }
                catch
                {
                    BitmapImage licoriceImage = new BitmapImage(new Uri("Default.jpeg", UriKind.Relative));
                    GlobleContacts.Images.Add(licoriceImage);
                }
                try
                {
                    var a = con.Addresses.ToList();
                    GlobleContacts.Addresses.Add(a[0].PhysicalAddress.City + ", " + a[0].PhysicalAddress.CountryRegion);
                }
                catch
                {
                    GlobleContacts.Addresses.Add("(No Address)");
                }
                try
                {
                    var a = con.Websites.ToList();
                    GlobleContacts.Websites.Add(a[0].ToString());
                }
                catch
                {
                    GlobleContacts.Websites.Add("(No Website)");
                }


                // ---- checking for fav. this need to be change --- /// 
                GlobleContacts.isFav.Add(0);
                // ---- checking for fav. this need to be change --- /// 


                dbimg = ImageToBytes(GlobleContacts.Images[i]);

                using (var db = new SQLiteConnection(dbPath))
                {
                    db.RunInTransaction(() =>
                    {
                        db.Insert(new Person()
                        {
                            Names = GlobleContacts.Names[i],
                            Numbers = GlobleContacts.Numbers[i],
                            Emails = GlobleContacts.Emails[i],
                            Images = dbimg,
                            Addresses = GlobleContacts.Addresses[i],
                            Websites = GlobleContacts.Websites[i],
                            isFav = GlobleContacts.isFav[i]
                        });
                    });
                }
                

                i++;
            }

            IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
            StreamWriter Writer = new StreamWriter(new IsolatedStorageFileStream("FirstRun.txt", FileMode.OpenOrCreate, fileStorage));
            Writer.WriteLine("DataStored");
            Writer.Close();
            
            pb.Visibility = Visibility.Collapsed;
            retrievingData();
        }

        public static byte[] ImageToBytes(BitmapImage img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    WriteableBitmap btmMap = new WriteableBitmap(img);
                    System.Windows.Media.Imaging.Extensions.SaveJpeg(btmMap, ms, img.PixelWidth, img.PixelHeight, 0, 100);
                    img = null;
                    return ms.ToArray();
                }
                catch
                {
                    return ms.ToArray();
                }
            }
        }

        public static BitmapImage BytesToImage(byte[] bytes)
        {
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    bitmapImage.SetSource(ms);
                    return bitmapImage;
                }
            }

            catch
            {
                //MessageBox.Show("Shit just happened!");
                return bitmapImage;
            }
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //List<Person> existing = db.Query<Person>("select * from Person where Names like '%" + searchBox.Text + "%'"   ).ToList();
            ObservableCollection<ToDoItemViewModel> _todoItems = new ObservableCollection<ToDoItemViewModel>();
            using (var db = new SQLiteConnection(dbPath))
            {
                List<Person> existing = db.Query<Person>("select * from Person where Names like '%" + searchBox.Text + "%'" + "OR Numbers like " + " '%" + searchBox.Text + "%'").ToList();
                if (existing != null)
                {
                    for (int i = 0; i < existing.Count; i++)
                    {
                        BitmapImage b = BytesToImage(existing[i].Images);
                        _todoItems.Add(new ToDoItemViewModel { Text = existing[i].Names, Text1 = existing[i].Numbers, Text2 = b, Text3 = existing[i].Id.ToString() });
                    }
                }
            }

            ContactList.DataContext = _todoItems;
            var swipeInteraction = new SwipeInteraction();
            swipeInteraction.Initialise(ContactList, _todoItems);
            _interactionManager.AddInteraction(swipeInteraction);
            FrameworkDispatcher.Update();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            _interactionManager.AddElement(sender as FrameworkElement);            
        }

        private void picccc_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var a = sender as Image;
            NavigationService.Navigate(new Uri("/Profile.xaml?msg=" + a.Tag, UriKind.Relative));
        }

        private void taskText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var a = sender as TextBlock;
            NavigationService.Navigate(new Uri("/Profile.xaml?msg=" + a.Tag, UriKind.Relative));
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var a = sender as TextBlock;
            NavigationService.Navigate(new Uri("/Profile.xaml?msg=" + a.Tag, UriKind.Relative));
        }

        private void onClickTickEvent(object sender, RoutedEventArgs e)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                FrameworkElement fe = sender as FrameworkElement;
                ToDoItemViewModel tappedItem = fe.DataContext as ToDoItemViewModel;
                List<Person> existing = db.Query<Person>("select * from Person where Id = " + tappedItem.Text3).ToList();
                if (existing != null)
                {
                    //existing.FirstName = Fname.Text;
                    if (existing[0].isFav == 0)
                    {
                        existing[0].isFav = 1;
                        MessageBox.Show("Added to Fav.");
                    }

                    else
                    {
                        existing[0].isFav = 0;
                        MessageBox.Show("Removed from Fav.");
                    }
                    db.RunInTransaction(() =>
                    {
                        db.Update(existing[0]);
                    });
                }
            }
        }

        private void OnClickEditEvent(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            ToDoItemViewModel tappedItem = fe.DataContext as ToDoItemViewModel;
            SmsComposeTask smsComposeTask = new SmsComposeTask();

            using (var db = new SQLiteConnection(dbPath))
            {
                List<Person> existing = db.Query<Person>("select * from Person where Id = " + tappedItem.Text3).ToList();
                if (existing != null)
                {
                    if (existing[0].Numbers == "(No Number)")
                        smsComposeTask.To = existing[0].Names;
                    else
                        smsComposeTask.To = existing[0].Numbers;
                    smsComposeTask.Body = "";
                    smsComposeTask.Show();
                }
            }
        }

        private void OnClickMusicEvent(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            ToDoItemViewModel tappedItem = fe.DataContext as ToDoItemViewModel;
            PhoneCallTask phoneCallTask = new PhoneCallTask();

            using (var db = new SQLiteConnection(dbPath))
            {
                List<Person> existing = db.Query<Person>("select * from Person where Id = " + tappedItem.Text3).ToList();
                if (existing != null)
                {
                    if (existing[0].Numbers == "(No Number)")
                        MessageBox.Show(existing[0].Names + "'s number is not saved.");
                    else
                    {
                        phoneCallTask.DisplayName = existing[0].Names;
                        phoneCallTask.PhoneNumber = existing[0].Numbers;
                        phoneCallTask.Show();
                    }
                }
            }
        }

        private void imgKeypad_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BitmapImage bm = new BitmapImage(new Uri(@"/images/NumberPad-S.png", UriKind.RelativeOrAbsolute));
            imgKeypad.Source = bm;
            Grid_Keypad.Visibility = Visibility.Visible;

            BitmapImage bm1 = new BitmapImage(new Uri(@"/images/ContactList.png", UriKind.RelativeOrAbsolute));
            imgContacts.Source = bm1;
            BitmapImage bm2 = new BitmapImage(new Uri(@"/images/Favorite.png", UriKind.RelativeOrAbsolute));
            imgFavs.Source = bm2;
            Grid_Contacts.Visibility = Visibility.Collapsed;
            Grid_Fav.Visibility = Visibility.Collapsed;

            number.Text = "";
        }

        private void imgContacts_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BitmapImage bm = new BitmapImage(new Uri(@"/images/ContactList-S.png", UriKind.RelativeOrAbsolute));
            imgContacts.Source = bm;
            Grid_Contacts.Visibility = Visibility.Visible;

            BitmapImage bm1 = new BitmapImage(new Uri(@"/images/NumberPad.png", UriKind.RelativeOrAbsolute));
            imgKeypad.Source = bm1;
            BitmapImage bm2 = new BitmapImage(new Uri(@"/images/Favorite.png", UriKind.RelativeOrAbsolute));
            imgFavs.Source = bm2;
            Grid_Keypad.Visibility = Visibility.Collapsed;
            Grid_Fav.Visibility = Visibility.Collapsed;
        }

        private void imgFavs_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BitmapImage bm = new BitmapImage(new Uri(@"/images/Favorite-S.png", UriKind.RelativeOrAbsolute));
            imgFavs.Source = bm;
            Grid_Fav.Visibility = Visibility.Visible;

            BitmapImage bm1 = new BitmapImage(new Uri(@"/images/NumberPad.png", UriKind.RelativeOrAbsolute));
            imgKeypad.Source = bm1;
            BitmapImage bm2 = new BitmapImage(new Uri(@"/images/ContactList.png", UriKind.RelativeOrAbsolute));
            imgContacts.Source = bm2;
            Grid_Keypad.Visibility = Visibility.Collapsed;
            Grid_Contacts.Visibility = Visibility.Collapsed;


            ObservableCollection<ToDoItemViewModel> _todoItems = new ObservableCollection<ToDoItemViewModel>();

            using (var db = new SQLiteConnection(dbPath))
            {
                List<Person> existing = db.Query<Person>("select * from Person where isFav = 1").ToList();
                if (existing != null)
                {
                    for (int i = 0; i < existing.Count; i++)
                    {
                        BitmapImage b = BytesToImage(existing[i].Images);
                        _todoItems.Add(new ToDoItemViewModel { Text = existing[i].Names, Text1 = existing[i].Numbers, Text2 = b, Text3 = existing[i].Id.ToString() });
                    }
                }
            }

            FavList.DataContext = _todoItems;
            var swipeInteraction = new SwipeInteraction();
            swipeInteraction.Initialise(FavList, _todoItems);
            _interactionManager.AddInteraction(swipeInteraction);
            FrameworkDispatcher.Update();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            number.Text += btn1.Content;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            number.Text += btn2.Content;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            number.Text += btn3.Content;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            number.Text += btn4.Content;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            number.Text += btn5.Content;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            number.Text += btn6.Content;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            number.Text += btn7.Content;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            number.Text += btn8.Content;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            number.Text += btn9.Content;
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            number.Text += btn0.Content;
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            number.Text += btnstar.Content;
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            number.Text += btnhash.Content;
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            PhoneCallTask phoneCallTask = new PhoneCallTask();
            phoneCallTask.PhoneNumber = number.Text;
            phoneCallTask.Show();
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();

            smsComposeTask.To = number.Text;
            smsComposeTask.Show();
        }



        private void number_TextChanged(object sender, TextChangedEventArgs e)
        {
            ObservableCollection<ToDoItemViewModel> _todoItems = new ObservableCollection<ToDoItemViewModel>();
            using (var db = new SQLiteConnection(dbPath))
            {
                List<Person> existing = db.Query<Person>("select * from Person where Names like '%" + number.Text + "%'" + "OR Numbers like " + " '%" + number.Text + "%'").ToList();
                if (existing != null)
                {
                    for (int i = 0; i < existing.Count; i++)
                    {
                        BitmapImage b = BytesToImage(existing[i].Images);
                        _todoItems.Add(new ToDoItemViewModel { Text = existing[i].Names, Text1 = existing[i].Numbers, Text2 = b, Text3 = existing[i].Id.ToString() });
                    }
                }
            }

            SearchedContactList.DataContext = _todoItems;
            var swipeInteraction = new SwipeInteraction();
            swipeInteraction.Initialise(SearchedContactList, _todoItems);
            _interactionManager.AddInteraction(swipeInteraction);
            FrameworkDispatcher.Update();
        }

        

        private bool _isSettingsOpen = false;
        private void imgMenu_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_isSettingsOpen)
            {
                VisualStateManager.GoToState(this, "SettingsClosedState", true);
                _isSettingsOpen = false;
                BitmapImage bm = new BitmapImage(new Uri(@"/images/menu.png", UriKind.RelativeOrAbsolute));
                imgMenu.Source = bm;
            }
            else
            {
                VisualStateManager.GoToState(this, "SettingsOpenState", true);
                _isSettingsOpen = true;

                BitmapImage bm = new BitmapImage(new Uri(@"/images/menuF.png", UriKind.RelativeOrAbsolute));
                imgMenu.Source = bm;
            }
        }

        
        private void addContact_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SaveContactTask saveContactTask;

            saveContactTask = new SaveContactTask();
            saveContactTask.FirstName = "";
            saveContactTask.LastName = "";
            saveContactTask.MobilePhone = "";

            saveContactTask.Show();
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (number.Text.Length > 0)
                number.Text = number.Text.Remove(number.Text.Length - 1);
        }

        
    }
}