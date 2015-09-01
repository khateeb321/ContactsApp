using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SQLite;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;

namespace Contacts_DB
{
    public partial class Profile : PhoneApplicationPage
    {
        public Profile()
        {
            InitializeComponent();
            
        }

        List<Person> existing;
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string msg = "";
            int index = 0;
            if (NavigationContext.QueryString.TryGetValue("msg", out msg))
            {
                index = Convert.ToInt32(msg);
            }


            using (var db = new SQLiteConnection(MainPage.dbPath))
            {
                existing = db.Query<Person>("select * from Person where Id = " + index).ToList();
                if (existing != null)
                {
                    BitmapImage b = new BitmapImage();
                    b = MainPage.BytesToImage(existing[0].Images);


                    tb_name.Text = existing[0].Names;
                    tb_number.Text = existing[0].Numbers;
                    tb_email.Text = existing[0].Emails;
                    pic.Source = b;
                    
                    pic.Opacity = 10;
                    
                    
                    tb_address.Text = existing[0].Addresses;
                    tb_website.Text = existing[0].Websites;
                }
            }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tb_number.Text == "(No Number)")
                MessageBox.Show(tb_name.Text + "'s number is not saved.");
            else
            {
                PhoneCallTask phoneCallTask = new PhoneCallTask();
                phoneCallTask.DisplayName = tb_name.Text;
                phoneCallTask.PhoneNumber = tb_number.Text;
                phoneCallTask.Show();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();
            if (tb_number.Text == "(No Number)")
                smsComposeTask.To = tb_name.Text;
            else
                smsComposeTask.To = tb_number.Text;
            smsComposeTask.Body = "";
            smsComposeTask.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (tb_email.Text == "(No Email Address)")
                MessageBox.Show(tb_name.Text + "'s email address is not saved.");
            else
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();
                emailComposeTask.To = tb_email.Text;
                emailComposeTask.Show();
            }
        }
    }
}