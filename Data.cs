using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendDeveloperTest
{
    class Data
    {
        public List<User> Users;
        public List<Order> Orders;

        public Data()
        {
            Users = new List<User>();
            Orders = new List<Order>();
        }

        /// <summary>
        /// main action of the query extarction
        /// </summary>
        /// <param name="fromList"></param>
        /// <param name="commandList"></param>
        /// <param name="selectList"></param>
        /// <param name="D"></param>
        /// <param name="bracket"></param>
        /// <returns></returns>
        public static command queryEngine(List<string> fromList, List<command> commandList, List<string> selectList, Data D, int bracket)
        {

            //build the tables of each numeric\ string commands ("string..." \ = \ < \ > \ <= \ >= )
            foreach (command d in commandList)
            {
                if (d.commtype == 0)
                {
                    Data.getTableForCommand(d, fromList, D);
                }
            }

            int tableindecator = -1; // 0 - users , 1 -orders
            //at this example we checking an simplefied query data extract -> single table check to get from data
            if (fromList.ElementAt(0) == "Users") //taking care at *User* data rows
            {
                tableindecator = 0;
            }
            else
            {
                if (fromList.ElementAt(0) == "Orders") //taking care at *Order* data rows
                {
                    tableindecator = 1;
                }
            }

            //now we perform logic commands with 2\1 sets of commands (and the tables they got accordingly) => comm   OR\AND   comm

            Stack<command> s1 = new Stack<command>();
            Stack<command> s2 = new Stack<command>();
            //organize the set of commands, which the first one waiting on the top of the stack s1
            for (int i = commandList.Count-1; i>=0 ;i--)
            {
                s1.Push(commandList.ElementAt(i));
            }

            int bcare = bracket; // get the bracket layer number taking care of brackets ("bracketcare")
            command temp1, temp2, temp3;
            //special case : only one command
            if (s1.Count == 1)
            {
                return s1.Pop();
            }
            //else : 3 or more commands
            while (bcare>=0)
            {
                bool finishedlayer = false;
                while (!finishedlayer)
                {      
                    while (s1.Count>0)
                    {
                        finishedlayer = true;
                        if (s1.Peek().priorityCount == bcare && s1.Count>2)// finding start of a bracket set of commands (minimum of 3 commands)
                        {
                            finishedlayer = false; // if detects a part of the layer, =false, and indicates to continue the next search 
                            //start treating the current set of commands
                            temp1 = s1.Pop();
                            temp2 = s1.Pop();
                            temp3 = s1.Pop();

                            if (temp1.priorityCount == temp2.priorityCount && temp1.priorityCount == temp3.priorityCount && temp2.commtype == 1
                                && (temp3.commtype == 0 || temp3.commtype == 3) && (temp1.commtype == 0 || temp1.commtype == 3))// check priority bracket and type of command
                            {
                                switch (tableindecator)
                                {
                                    case 0: // User table
                                        if (temp2.operand == "OR" || temp2.operand == "or")
                                        {
                                            temp2.quaryResUser = logicORListsUser(temp1.quaryResUser, temp3.quaryResUser);
                                        }
                                        else
                                        {
                                            temp2.quaryResUser = logicANDListUser(temp1.quaryResUser, temp3.quaryResUser);
                                        }
                                        break;
                                    case 1: // Order table
                                        if (temp2.operand == "OR" || temp2.operand == "or")
                                        {
                                            temp2.quaryResOrder = logicORListsOrder(temp1.quaryResOrder, temp3.quaryResOrder);
                                        }
                                        else
                                        {
                                            temp2.quaryResOrder = logicANDListOrder(temp1.quaryResOrder, temp3.quaryResOrder);
                                        }
                                        break;
                                }
                                temp2.commtype = 3;
                                if (temp2.priorityCount>0)
                                {
                                    temp2.priorityCount--;
                                }
                                s2.Push(temp2);
                            }
                        }
                        else
                        {   // continue the search
                            s2.Push(s1.Pop());
                        }
                    }
                    if (s1.Count==0)
                    {
                        break;
                    }
                }
           
                bcare--; // layer done

                while (s2.Count > 0) //move the elements from the end of the layer search back to s1
                {
                    s1.Push(s2.Pop());
                }

                if (bcare < 0 && s1.Count>1)
                {
                    bcare = 0; // need to go over layer 0 to perform all the commands
                }

                
     
            }

            //get people by the set of commands
            //pop command and return from s1
            return s1.Pop();
        }

        /// <summary>
        /// sets the 2 list<User\Order> the inside each command.
        /// after we search data rows according to the command demand we 
        /// will make logic action between table rows.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="fromlist"></param>
        /// <param name="D"></param>
        private static void getTableForCommand(command c, List<string> fromlist, Data D)
        {
            //case : User table
            if (fromlist.ElementAt(0) == "Users")
            {
                c.quaryResUser = new List<User>();
                //string action
                if (c.op1 == "Email" || c.op1 == "FullName")
                {
                    foreach (User u in D.Users)
                    {
                        if (c.op1 == "Email" && u.Email == (string)c.op2)
                        {
                            c.quaryResUser.Add(u);
                        }
                        if (c.op1 == "FullName" && u.FullName == (string)c.op2)
                        {
                            c.quaryResUser.Add(u);
                        }
                    }
                    return;
                }

                //numeric action

                if (c.op1 == "Age")
                {
                    int act = -1;
                    if (c.operand == "<")
                    {
                        act = 0;
                    }
                    if (c.operand == ">")
                    {
                        act = 1;
                    }
                    if (c.operand == "<=")
                    {
                        act = 2;
                    }
                    if (c.operand == ">=")
                    {
                        act = 3;
                    }
                    if (c.operand == "=")
                    {
                        act = 4;
                    }
                    foreach (User u in D.Users)
                    {
                        switch (act)
                        {
                            case 0:
                                if (u.Age < Convert.ToInt32(c.op2))
                                {
                                    c.quaryResUser.Add(u);
                                }
                                break;
                            case 1:
                                if (u.Age > Convert.ToInt32(c.op2))
                                {
                                    c.quaryResUser.Add(u);
                                }
                                break;
                            case 2:
                                if (u.Age <= Convert.ToInt32(c.op2))
                                {
                                    c.quaryResUser.Add(u);
                                }
                                break;
                            case 3:
                                if (u.Age >= Convert.ToInt32(c.op2))
                                {
                                    c.quaryResUser.Add(u);
                                }
                                break;
                            case 4:
                                if (u.Age == Convert.ToInt32(c.op2))
                                {
                                    c.quaryResUser.Add(u);
                                }
                                break;
                        }
                    }
                    return;
                }
                Console.WriteLine("wrong data variable to extarct from table!\n");
            }
            //case : Order table
            else
            {
                c.quaryResOrder = new List<Order>();
                if (fromlist.ElementAt(0) == "Orders")
                {
                    //string action
                    if (c.op1 == "orderUserName")
                    {
                        foreach (Order o in D.Orders)
                        {
                            if (c.op1 == "orderUserName" && o.orderUserName == (string)c.op2)
                            {
                                c.quaryResOrder.Add(o);
                            }
                        }
                        return;
                    }

                    //numeric action
                    if (c.op1 == "totalcost")
                    {
                        int act = -1;
                        if (c.operand == "<")
                        {
                            act = 0;
                        }
                        if (c.operand == ">")
                        {
                            act = 1;
                        }
                        if (c.operand == "<=")
                        {
                            act = 2;
                        }
                        if (c.operand == ">=")
                        {
                            act = 3;
                        }
                        if (c.operand == "=")
                        {
                            act = 4;
                        }
                        foreach (Order o in D.Orders)
                        {
                            switch (act)
                            {
                                case 0:
                                    if (o.totalcost < Convert.ToInt32(c.op2))
                                    {
                                        c.quaryResOrder.Add(o);
                                    }
                                    break;
                                case 1:
                                    if (o.totalcost > Convert.ToInt32(c.op2))
                                    {
                                        c.quaryResOrder.Add(o);
                                    }
                                    break;
                                case 2:
                                    if (o.totalcost <= Convert.ToInt32(c.op2))
                                    {
                                        c.quaryResOrder.Add(o);
                                    }
                                    break;
                                case 3:
                                    if (o.totalcost >= Convert.ToInt32(c.op2))
                                    {
                                        c.quaryResOrder.Add(o);
                                    }
                                    break;
                                case 4:
                                    if (o.totalcost == Convert.ToInt32(c.op2))
                                    {
                                        c.quaryResOrder.Add(o);
                                    }
                                    break;
                            }
                        }
                        return;
                    }
                    else
                    {
                        Console.WriteLine("table not found!");
                    }
                }
            }
        }

        /// <summary>
        /// "AND" logic action on Users 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static List<User> logicANDListUser(List<User> a, List<User> b)
        {
            List<User> retlist = new List<User>();

            foreach (User u in a)
            {
                if (b.Contains(u))
                {
                    retlist.Add(u);
                }
            }
            return retlist;
        }

        /// <summary>
        /// "OR" logic action on Users
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static List<User> logicORListsUser(List<User> a, List<User> b)
        {
            List<User> retlist = new List<User>();
            foreach (User u in a)
            {
                if (!retlist.Contains(u))
                {
                    retlist.Add(u);
                }
            }
            foreach (User u in b)
            {
                if (!retlist.Contains(u))
                {
                    retlist.Add(u);
                }
            }
            return retlist;
        }

        /// <summary>
        /// "AND" logic action on Orders
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static List<Order> logicANDListOrder(List<Order> a, List<Order> b)
        {
            List<Order> retlist = new List<Order>();
            foreach (Order o in a)
            {
                if (b.Contains(o))
                {
                    retlist.Add(o);
                }
            }
            return retlist;
        }

        /// <summary>
        /// "OR" logic action on Orders
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static List<Order> logicORListsOrder(List<Order> a, List<Order> b)
        {
            List<Order> retlist = new List<Order>();
    
            foreach (Order o in a)
            {
                if (!retlist.Contains(o))
                {
                    retlist.Add(o);
                }
            }
            foreach (Order o in b)
            {
                if (!retlist.Contains(o))
                {
                    retlist.Add(o);
                }
            }
            return retlist;
        }


    }
}
