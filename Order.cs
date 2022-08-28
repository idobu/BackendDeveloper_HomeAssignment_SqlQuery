using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendDeveloperTest
{ 
    class Order
    {
        public static int currOrderId=0; // background data
        public int OrderId; // background data
        public int orderUserID; // background data
        public string orderUserName;
        public int totalcost;
    
        public Order(int orderUserid, int cost,string orderusername) // can be with type of: null OR/AND null, 
        {
            this.orderUserName = orderusername;
            this.OrderId = currOrderId++;
            this.totalcost = cost;
            this.orderUserID = orderUserid;
        }
    }

}
