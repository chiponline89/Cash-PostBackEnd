using PayID.Portal.Common.Log;
using PayID.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace PayID.Portal.Common.Service
{
    public class AccountService
    {
        public Account Login (string Username, string Password, ref bool result)
        {
            Account oAccount = new Account();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Account/Login";

                    oAccount.UserName = Username;
                    oAccount.Password = Password;

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oAccount).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.Value;
                        oAccount = new Account();
                        oAccount.Address = oValue.Address.ToString();
                        oAccount.FullName = oValue.FullName.ToString();
                        oAccount.PhoneNumber = oValue.PhoneNumber.ToString();
                        oAccount.Role = oValue.Role.ToString();
                        oAccount.Status = oValue.Status.ToString();
                        oAccount.UnitCode = oValue.UnitCode.ToString();
                        oAccount.UnitLink = oValue.UnitLink.ToString();
                        oAccount.UnitName = oValue.UnitName.ToString();
                        oAccount.UserName = oValue.UserName.ToString();
                        oAccount.UserOfficer = oValue.UserOfficer.ToString();

                        result = true;
                    }
                    else
                    {
                        oAccount = null;
                        result = false;
                    }

                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "AccountService.Login: " + ex.Message);
                oAccount = null;
                result = false;
            }
           
            return oAccount;
        }

        public Account GetAccountByUserName(string Username)
        {
            Account oAccount = new Account();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Account/GetAccountByUserName?userName={0}";

                    var response = new HttpResponseMessage();
                    response = client.PostAsync(string.Format(url,Username),null).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.Value;
                        oAccount.Address = oValue.Address.ToString();
                        oAccount.FullName = oValue.FullName.ToString();
                        oAccount.PhoneNumber = oValue.PhoneNumber.ToString();
                        oAccount.Role = oValue.Role.ToString();
                        oAccount.Status = oValue.Status.ToString();
                        oAccount.UnitCode = oValue.UnitCode.ToString();
                        oAccount.UnitLink = oValue.UnitLink.ToString();
                        oAccount.UnitName = oValue.UnitName.ToString();
                        oAccount.UserName = oValue.UserName.ToString();
                        oAccount.UserOfficer = oValue.UserOfficer.ToString();
                    }
                    else
                    {
                        oAccount = null;
                    }
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "AccountService.GetAccountByUserName: " + ex.Message);
                oAccount = null;
            }

            return oAccount;
        }

        public List<Account> GetAccountByUnitLink(string UnitLink)
        {
            List<Account> ListAccount = new List<Account>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Account/GetUserByUnitLink?UnitLink={0}";

                    var response = new HttpResponseMessage();
                    response = client.PostAsync(string.Format(url, UnitLink),null).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        foreach(var item in oValue)
                        {
                            Account oAccount = new Account();

                            oAccount.Address = item.Address.ToString();
                            oAccount.FullName = item.FullName.ToString();
                            oAccount.PhoneNumber = item.PhoneNumber.ToString();
                            oAccount.Role = item.Role.ToString();
                            oAccount.Status = item.Status.ToString();
                            oAccount.UnitCode = item.UnitCode.ToString();
                            oAccount.UnitLink = item.UnitLink.ToString();
                            oAccount.UnitName = item.UnitName.ToString();
                            oAccount.UserName = item.UserName.ToString();
                            oAccount.UserOfficer = item.UserOfficer.ToString();

                            ListAccount.Add(oAccount);
                        }

                    }
                    else
                    {
                        ListAccount = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "AccountService.GetAccountByUnitLink: " + ex.Message);
                ListAccount = null;
            }

            return ListAccount;
        }

        public List<Account> GetAccountByUnitCode(string UnitCode)
        {
            List<Account> ListAccount = new List<Account>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Account/GetUserByUnitCode?UnitCode={0}";

                    var response = new HttpResponseMessage();
                    response = client.PostAsync(string.Format(url, UnitCode), null).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    if (rs.Code == "00")
                    {
                        dynamic oValue = rs.ListValue;
                        foreach (var item in oValue)
                        {
                            Account oAccount = new Account();

                            oAccount.Address = item.Address.ToString();
                            oAccount.FullName = item.FullName.ToString();
                            oAccount.PhoneNumber = item.PhoneNumber.ToString();
                            oAccount.Role = item.Role.ToString();
                            oAccount.Status = item.Status.ToString();
                            oAccount.UnitCode = item.UnitCode.ToString();
                            oAccount.UnitLink = item.UnitLink.ToString();
                            oAccount.UnitName = item.UnitName.ToString();
                            oAccount.UserName = item.UserName.ToString();
                            oAccount.UserOfficer = item.UserOfficer.ToString();

                            ListAccount.Add(oAccount);
                        }

                    }
                    else
                    {
                        ListAccount = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "AccountService.GetAccountByUnitCode: " + ex.Message);
                ListAccount = null;
            }

            return ListAccount;
        }

        public string UpdateAccount(Account oAccount)
        {
            string sReturn = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Account/UpdateAccount";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oAccount).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString();
                    string Mes = rs.Message.ToString();

                    sReturn = Code + "|" + Mes;
                }
            }
            catch(Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "AccountService.UpdateAccount: " + ex.Message);
                sReturn = "-99|";
            }

            return sReturn;
        }

        public string ChangeStatusAccount(Account oAccount)
        {
            string sReturn = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["SHIPAGE_API"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Account/ChangeStatusAccount";

                    var response = new HttpResponseMessage();
                    response = client.PostAsJsonAsync(url, oAccount).Result;
                    var rs = response.Content.ReadAsAsync<dynamic>().Result;

                    string Code = rs.Code.ToString();
                    string Mes = rs.Message.ToString();

                    sReturn = Code + "|" + Mes;
                }
            }
            catch (Exception ex)
            {
                LogAPP.LogToFile(LogFileType.EXCEPTION, "AccountService.ChangeStatusAccount: " + ex.Message);
                sReturn = "-99|";
            }

            return sReturn;
        }
    }
}