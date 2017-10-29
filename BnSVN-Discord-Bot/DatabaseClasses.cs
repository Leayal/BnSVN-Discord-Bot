using System;
using System.Collections.Generic;
using System.Text;

namespace BnSVN_Discord_Bot
{
    class TradeItemInfo
    {
        public string Name { get; }
        public ulong Price { get; set; }

        public TradeItemInfo(string itemName)
        {
            this.Name = itemName;
        }
    }
}
