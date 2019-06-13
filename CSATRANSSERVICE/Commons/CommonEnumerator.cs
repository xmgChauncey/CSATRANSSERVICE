using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSATRANSSERVICE
{
    public abstract class OperateType
    {
        //报文接收
        public const string MessageReceive = "1";

        //报文发送
        public const string MessageSend = "2";

            //报文标准化
        public const string MessageConvert = "3";

            //回执关联
        public const string AssociatedReceipts = "4";
     }

    public abstract class OperateName
    {
        //报文接收
        public const string MessageReceive = "报文接收";

        //报文发送
        public const string MessageSend = "报文发送";

        //报文标准化
        public const string MessageConvert = "报文转换";

        //回执关联
        public const string AssociatedReceipts = "回执关联";
    }

    public abstract class OperateResult
    {
        //操作成功
        public const string OperateSuccess = "S";

        //操作失败
        public const string OperateFail = "F";
    }

    public abstract class Sender
    {
        //发送电子口岸
        public const string SendToCport = "发送电子口岸";

        //发送企业
        public const string SendToCompany = "发送企业";

        //发送网络科
        public const string SendToNetWorkDepart = "发送网络科";
    }
}
