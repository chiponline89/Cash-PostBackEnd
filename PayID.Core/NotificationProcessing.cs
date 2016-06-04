using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PayID.DataHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayID.Core
{
    public class NotificationProcessing
    {
        static System.Threading.Thread processThread;
        public static void OnStart()
        {
            if (processThread == null || processThread.ThreadState != System.Threading.ThreadState.Running)
            {
                processThread = new System.Threading.Thread(
                    new System.Threading.ThreadStart(Process));
                processThread.Start();
            }
        }

        public static void OnStop()
        {
            try { processThread.Abort(); }
            catch { }
        }

        static void Process() {
            IMongoSortBy _sort = MongoDB.Driver.Builders.SortBy.Ascending("system_created_time");
            while (true)
            {
                
                long _totalSize = 0;
                try { 
                dynamic[] _newMessageList = CoreService.Data.ListPagging("message", Query.EQ("status", "NEW"), _sort, 10, 1, out _totalSize);
                if (_totalSize > 0)
                {
                    foreach (dynamic _newMessage in _newMessageList)
                    {
                        string _messageType = _newMessage.type.ToString();
                        string _status = "SEND";
                        string _error_code = "00";
                        string _error_message = "";
                        switch (_messageType)
                        {
                            case "EMAIL":
                                try
                                {
                                    sendEmail(_newMessage);
                                }
                                catch (Exception ex)
                                {
                                    _error_code = "96";
                                    _error_message = ex.Message;
                                    _status = "ERROR";
                                }
                                break;
                            default:
                                break;
                        }

                        CoreService.Data.UpdateObject("message", Query.EQ("_id", _newMessage._id),
                            Update.Set("status", _status)
                            .Set("error_code", _error_code)
                            .Set("error_message", _error_message)
                            );
                    }
                }
                }
                catch{

                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void sendEmail(dynamic _newMessage)
        {
            Helper.SendMail(_newMessage.receiver, "[CỔNG DỊCH VỤ CASHPOST] - THÔNG BÁO", _newMessage.content);
        }
    }
}
