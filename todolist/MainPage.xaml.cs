using Windows.UI.Xaml.Controls;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace todolist
{
    public sealed partial class MainPage : Page
    {
        private int count = 0;
        private bool firstQuerry = true;
        private bool edit = false;
        private int idEdited = -1;
        private Dictionary<int, int> onGoing = new Dictionary<int, int>();
        private SqliteConnection db = new SqliteConnection("Filename=Todo.db");

        public MainPage()
        {
            this.InitializeComponent();
            Output.ItemsSource = Grab_Entries();
            firstQuerry = false;
        }
        private class TodoEntry
        {
            public string title { get; set; }
            public string content { get; set; }
            public DateTimeOffset date { get; set; }
            public int id { get; set; }
            public String done { get; set; }
        }
        private void Add_Text(object sender, RoutedEventArgs e)
        {
            if (!edit)
            {

                db.Open();

                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "INSERT INTO TodoList VALUES (NULL, @TitleEntry, @ContentEntry, @Id, @TicksEntry, @Done);"
                };
                insertCommand.Parameters.AddWithValue("@TitleEntry", Title_Box.Text);
                insertCommand.Parameters.AddWithValue("@ContentEntry", Input_Box.Text);
                insertCommand.Parameters.AddWithValue("@Id", count);
                insertCommand.Parameters.AddWithValue("@Done", 0);
                if (DatePicker.Date != null)
                    insertCommand.Parameters.AddWithValue("@TicksEntry", DatePicker.Date.Value.Ticks);
                else
                    insertCommand.Parameters.AddWithValue("@TicksEntry", "");
                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    System.Diagnostics.Debug.WriteLine(error.ToString());
                    return;
                }
                onGoing.Add(count, 0);
                count++;
                db.Dispose();

            }
            else
            {
                db.Open();
                SqliteCommand Command = new SqliteCommand("UPDATE TodoList SET Title_Entry = '" + Title_Box.Text + "', Content_Entry = '" + Input_Box.Text + "' WHERE Id = " + idEdited + ";", db);
                try
                {
                    Command.ExecuteReader();

                }
                catch (SqliteException error)
                {
                    System.Diagnostics.Debug.WriteLine(error.ToString());
                    return;
                }
                idEdited = -1;
                edit = false;
                db.Dispose();
                Edit_Mode.Visibility = Visibility.Collapsed;

            }
            Output.ItemsSource = Grab_Entries();
        }

        private List<TodoEntry> Grab_Entries()
        {
            List<TodoEntry> entries = new List<TodoEntry>();

            db.Open();
            SqliteCommand selectCommand = new SqliteCommand("SELECT Title_Entry, Content_Entry, Ticks_Entry, Id, Done from TodoList;", db);
            SqliteDataReader query;
            try
            {
                query = selectCommand.ExecuteReader();
            }
            catch (SqliteException error)
            {
                System.Diagnostics.Debug.WriteLine(error);
                return entries;
            }
            while (query.Read())
            {
                TodoEntry tmpEntry = new TodoEntry();
                tmpEntry.title = query.GetString(0);
                tmpEntry.content = query.GetString(1);
                System.Diagnostics.Debug.WriteLine(query.GetInt64(2));
                tmpEntry.date = new DateTimeOffset(new DateTime(query.GetInt64(2)));
                tmpEntry.id = query.GetInt16(3);
                if (query.GetInt16(4) == 0)
                    tmpEntry.done = "";
                else
                    tmpEntry.done = "DONE";
                entries.Add(tmpEntry);
                if (firstQuerry)
                {
                    onGoing.Add(tmpEntry.id, query.GetInt16(4));
                    count = tmpEntry.id;
                }
            }
            if (firstQuerry)
            {
                count++;
            }
            db.Dispose();
            firstQuerry = false;
            return entries;
        }

        private void Remove_Task(object sender, RoutedEventArgs e)
        {
            var tmp = sender as Button;
            db.Open();
            SqliteCommand selectCommand = new SqliteCommand("DELETE FROM TodoList WHERE Id = " + tmp.Tag.ToString() + ";", db);
            SqliteDataReader query;
            try
            {
                query = selectCommand.ExecuteReader();
            }
            catch (SqliteException error)
            {
                System.Diagnostics.Debug.WriteLine(error);
            }
            db.Dispose();
            Output.ItemsSource = Grab_Entries();
        }

        private void Edit_Task(object sender, RoutedEventArgs e)
        {
            var tmp = sender as Button;
            edit = true;
            System.Diagnostics.Debug.WriteLine("OK MEC " + idEdited + " " + tmp.Tag);
            if (idEdited == (Int32)tmp.Tag)
            {
                System.Diagnostics.Debug.WriteLine("IN ThE IF");
                edit = false;
                idEdited = -1;
                Edit_Mode.Visibility = Visibility.Collapsed;
            }
            else
            {
                idEdited = (Int32)tmp.Tag;
                Edit_Mode.Visibility = Visibility.Visible;
            }
        }

        private void Done_Task(object sender, RoutedEventArgs e)
        {
            var tmp = sender as Button;
            int done = 0;
            int id = (Int32)tmp.Tag;

            if (onGoing.ContainsKey(id))
            {
                int isDone = onGoing[id];
                if (isDone == 0)
                {
                    done = 1;
                    onGoing[id] = 1;
                }
                else
                {
                    done = 0;
                    onGoing[id] = 0;
                }
            }
            db.Open();
              SqliteCommand Command = new SqliteCommand("UPDATE TodoList SET Done = " + done + " WHERE Id = " + tmp.Tag + ";", db);
              try
              {
                  Command.ExecuteReader();

              }
              catch (SqliteException error)
              {
                  System.Diagnostics.Debug.WriteLine(error.ToString());
                  return;
              }
            db.Dispose();
           Output.ItemsSource = Grab_Entries();
        }
    }
}