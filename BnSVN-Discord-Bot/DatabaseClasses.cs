using System;
using System.Collections.Generic;
using System.Text;

namespace BnSVN_Discord_Bot
{
    struct RequestRegisterResult
    {
        public enum Code : byte
        {
            UnknownError,
            AlreadyExist,
            Success
        }
        public string Result { get; }
        public Code MessageCode { get; }
        public RequestRegisterResult(Code _code, string _result)
        {
            this.Result = _result;
            this.MessageCode = _code;
        }
    }

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
