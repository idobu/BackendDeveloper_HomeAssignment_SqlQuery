using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// Name: Ido Bueno
/// Date: 08/06/2022
/// </summary>
namespace BackendDeveloperTest
{
    /// <summary>
    /// 
    /// notes\assumptions:
    /// * added another table to extarct data from (Orders) as showen in the question's description
    ///   and added as an extra table to extract from.
    /// * the input (query sentence) and the output (query table data) is showen in the form under    
    ///   the assigned locations for an additional convenience.
    /// * added 'key-id' variables as background data like an actual table to the correctness and integrity of the data.
    /// * support in bracket veriations and quantities in the 'where' section "()(())...etc"
    /// 
    /// manual:
    /// 1) write a query extraction sentence in the multi-textbox area.
    /// 2) click the "GO" button.
    /// 3) the output will appear under the "Query Result" title if any relevant data was found.
    /// </summary>
    public partial class Form1 : Form
    {

        Data myData;

        public Form1()
        {
            InitializeComponent();
            //data insertion section! you may add additional data rows , format(User) : (email, fullname, age)
            //                                                           format(User) : (userid, totalcost, userOrdername)   
            ////user examples                                            
            this.myData = new Data();
            this.myData.Users.Add(new User("jobs@hibernatingrhinos.com", "John Doe", 35));
            this.myData.Users.Add(new User("selected.databases@ravendb.net", "ido bueno", 40));
            this.myData.Users.Add(new User("selected.databases@ravendb.net", "ruth yehu", 23));
            this.myData.Users.Add(new User("jobs@ravendb.net", "bar bar", 10));
            this.myData.Users.Add(new User("mynewsql@ravendb.net", "foo man", 23));
            this.myData.Users.Add(new User("getmyemail@ravendb.net", "foo1", 100));
            this.myData.Users.Add(new User("getmail@ravendb.net", "foo2", 70));
            ////order examples
            this.myData.Orders.Add(new Order(0, 100, "John Doe"));
            this.myData.Orders.Add(new Order(2, 200, "ruth roy"));
            this.myData.Orders.Add(new Order(1, 87, "ido bueno"));
            this.myData.Orders.Add(new Order(3, 16, "bar bar"));
            //end
        }

        /// <summary>
        /// the start of the main action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.queryresult.Text = "";

            if (this.sqlAction.TextLength > 0)
            {

                //split the query by the 3 elements: 0- from , 1- where, 2- select
                List<string> sqlSentence = this.sqlAction.Text.Split(new string[] { "from ", "FROM ", "select ", "SELECT ", "WHERE ", "where " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> sqltemp = new List<string>();
                //remove \r\n
                foreach (string s in sqlSentence)
                {
                    string t = s.Replace("\r\n", string.Empty);
                    sqltemp.Add(t.Replace("\"",string.Empty).Trim());
                }
                sqlSentence.Clear();
                sqlSentence = sqltemp;

                //from list
                List<string> fromSection = sqlSentence[0].Split(',').ToList();

                List<string> whereSection1 = sqlSentence[1].Split(new string[] { " OR ", " or ", " and ", " AND " }, StringSplitOptions.None).ToList();//only numeric\equal commands
                List<string> whereSection2 = new List<string>();// only logical commands
                List<string> whereSection = new List<string>(); //final list

                foreach (string s in this.sqlAction.Text.Split(' ').ToList())
                {
                    if (s == "or" || s == "OR" || s == "and" || s == "AND")
                    {
                        whereSection2.Add(s);
                    }
                }

                //where list
                while (whereSection1.Count > 0)
                {
                    if (whereSection2.Count > 0)
                    {
                        whereSection.Add(whereSection1.ElementAt(0));
                        whereSection1.RemoveAt(0);
                        whereSection.Add(whereSection2.ElementAt(0));
                        whereSection2.RemoveAt(0);
                    }
                    else
                    {
                        whereSection.Add(whereSection1.ElementAt(0));
                        whereSection1.RemoveAt(0);
                    }
                }

                int maxbraket = 0, Bcount = 0;
                List<command> myCommandsSet = new List<command>();
                foreach (string s in whereSection)
                {
                    //getting the number of braket sets
                    if (s.Contains("("))
                    {
                        Bcount += s.Split('(').Length - 1;
                    }
                    int cpriority = Bcount;
                    if (Bcount > maxbraket)
                    {
                        maxbraket = Bcount;
                    }
                    if (s.Contains(")"))
                    {
                        string[] temp = s.Split(')');
                        Bcount -= temp.Length - 1;
                    }
                    //set the string for extraction as a command
                    string a = s;
                    a = a.Replace('(', ' ');
                    a = a.Replace(')', ' ');
                    a = a.Replace('\'', ' ');
                    a = a.Trim();
                    string[] b = Regex.Replace(a, " {2,}", " ").Split(' ');
                    //if b is more then 3 strings, string [2..n] is multli-word name
                    if (b.Length > 3)
                    {
                        for(int i =3; i <b.Length;i++ )
                        {
                            b[2] = b[2] +" "+b[i];
                        }
                    }
                    command c;
                    if (b[0] == "or" || b[0] == "OR" || b[0] == "and" || b[0] == "AND")
                    {
                        c = new command(null, b[0], null, 1/*commtype*/);
                    }
                    else
                    {
                        c = new command(b[0], b[1], b[2], 0/*commtype*/);
                    }
                    c.priorityCount = cpriority;
                    myCommandsSet.Add(c);
                }



                //select list
                List<string> selectSection1 = sqlSentence[2].Split(',').ToList();
                List<string> selectSection = new List<string>();
                selectSection1.ElementAt(selectSection1.Count - 1).Trim('\n', '\r');
                for (int i = 0; i < selectSection1.Count; i++)
                    selectSection.Add(selectSection1.ElementAt(i).Trim());

                if (whereSection.Count == 0 || fromSection.Count == 0 || selectSection.Count == 0)
                {
                    Console.WriteLine("something went wrong with the query verification, please try again!");
                }

                //send the lists to the query engine
                command retquery = Data.queryEngine(fromSection, myCommandsSet, selectSection, this.myData, maxbraket);
                this.queryresult.Visible = true;
                if (fromSection.ElementAt(0) == "Users")
                {
                    this.queryresult.Text = PrintUsers(retquery.quaryResUser, selectSection);
                }
                else
                {
                    this.queryresult.Text = PrintOrders(retquery.quaryResOrder, selectSection);
                }
            }
            else
            {
                Console.WriteLine("Please enter a vaild query action!");
            }

        }

        /// <summary>
        /// returns string with the requested data columns of table Users
        /// </summary>
        /// <param name="lu"></param>
        /// <param name="selectlist"></param>
        /// <returns></returns>
        private string PrintUsers(List<User> lu, List<string> selectlist)
        {
            string s = "";
            foreach (User u in lu)
            {
                s += " |   ";
                if (selectlist.Contains("FullName"))
                    s += u.FullName + "   |   ";
                if (selectlist.Contains("Email"))
                    s += u.Email + "   |   ";
                if (selectlist.Contains("Age"))
                    s += u.Age + "   |   ";
                s += "\n";
            }
            return s;
        }

        /// <summary>
        /// returns string with the requested data columns of table Orders
        /// </summary>
        /// <param name="lo"></param>
        /// <param name="selectlist"></param>
        /// <returns></returns>
        private string PrintOrders(List<Order> lo, List<string> selectlist)
        {
            string s = "";
            foreach (Order u in lo)
            {
                s += " |   ";
                if (selectlist.Contains("orderUserName"))
                    s += u.orderUserName + "   |   ";
                if (selectlist.Contains("totalcost"))
                    s += u.totalcost + "   |   ";
                s += "\n";
            }
            return s;
        }
    }

    /// <summary>
    /// a class to help with the command-parsing operation
    /// </summary>
    class command
    {
        /// <summary>
        /// 3 vars to preform a command 
        /// </summary>
        public int commtype; // type =0 string\int operation
                             // type =1 or\and operation
                             // type =2 command has already executed and contains only table with answer (in order to make )
        public string op1;
        public string operand;
        public object op2;
        //is inside a brakets?
        public int priorityCount;
        //save resault from query
        public List<User> quaryResUser;
        public List<Order> quaryResOrder;
        public command(string o1, string operand, string o2, int commtype)
        {
            this.commtype = commtype;
            this.op1 = o1;
            this.operand = operand;
            this.op2 = o2;
            priorityCount = 0;
            quaryResUser = null; //depends on the query
            quaryResOrder = null; //depends on the query
            //null until determine the data type that going to be stored
        }
    }
}
