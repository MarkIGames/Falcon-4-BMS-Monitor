using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace F4toA3Monitor
{
    class DBConnect
    {

        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            // Move to Config File
            // Pass as an object into this class (create new object with getters/setters)
            server   = "";
            database = "";
            uid      = "";
            password = "";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection( monitorUi userDisplay )
        {
            // userDisplay.AppendTextBox("Connecting....\r\n");

            try
            {
                connection.Open();
                // userDisplay.AppendTextBox("Connected!\r\n");
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        userDisplay.AppendTextBox("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        userDisplay.AppendTextBox("Invalid username/password, please try again");
                        break;
                }

                userDisplay.AppendTextBox(ex.Number + "\r\n");

                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void saveUserToDatabase(double x, double y, double z, string name, monitorUi userDisplay)
        {
            string query = @"INSERT INTO flightunits ( x, y, z, name, active, source) VALUES ( " + x + ", " + y + ", " + z + ", '" + name + "', 1, 'Falcon4')";

            //userDisplay.AppendTextBox(query + "\r\n");

            //Open Connection
            if (this.OpenConnection( userDisplay ) == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteReader();

                //close Connection
                this.CloseConnection();
            }
        }

        public string[] getBombData(monitorUi userDisplay)
        {
            string query = @"SELECT * FROM `playerunits` WHERE (`lasery` != 0 OR `laserx` != 0 OR `laserz` != 0) AND (`lasercode` = 91016 OR `lasercode` = 48377) AND active = 1";

            //userDisplay.AppendTextBox(query + "\r\n");

            string[] numbers = new string[5];

            //Open Connection
            if (this.OpenConnection(userDisplay) == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        string code    = dr.GetString(7);
                        string laserx  = dr.GetString(8);
                        string lasery  = dr.GetString(9);
                        string laserz  = dr.GetString(10);
                        string profile = dr.GetString(11); ;

                        numbers = new string[5] { code, laserx, lasery, laserz, profile };
                    }
                }

                //close Connection
                this.CloseConnection();
            }

            return numbers;
        }

        public void updateUserInDatabase(double x, double y, double z, string name, monitorUi userDisplay)
        {
            string query = @"UPDATE flightunits SET x = " + x + ", y = " + y + ", z = " + z + " WHERE name = '" + name + "' AND active = 1";

            //userDisplay.AppendTextBox(query + "\r\n");

            //Open Connection
            if (this.OpenConnection(userDisplay) == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteReader();

                //close Connection
                this.CloseConnection();
            }
        }

        public void deactivateUserInDatabase(monitorUi userDisplay)
        {
            string name = userDisplay.getCallsign();

            string query = @"UPDATE flightunits SET active = 0 WHERE name = '" + name + "'";

            //userDisplay.AppendTextBox(query + "\r\n");

            //Open Connection
            if (this.OpenConnection( userDisplay ) == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteReader();

                //close Connection
                this.CloseConnection();
            }
        }

        public void saveBombToDatabase(double x, double y, double z, string profile, double altitude, monitorUi userDisplay)
        {
            string query = @"INSERT INTO bombdrops ( x, y, z, fired, type, profile, originz) VALUES ( " + x + ", " + y + ", " + z + ", 1, 1, '" + profile + "', " + altitude + ")";

            //userDisplay.AppendTextBox(query + "\r\n");

            //Open Connection
            if (this.OpenConnection(userDisplay) == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteReader();

                //close Connection
                this.CloseConnection();
            }
        }
    }
}
