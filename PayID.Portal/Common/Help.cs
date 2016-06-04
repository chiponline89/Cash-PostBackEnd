using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace PayID.Portal.Common
{
    public class Help
    {
        public string Page(int intTotal, int intCurrPage, int intPageSize, int intRowPerPage, int intRowFrom, int intRowTo, string function)
        {
            StringBuilder strReturn = new StringBuilder();

            if (intCurrPage == 0)
            {
                intCurrPage = 1;
            }

            intRowPerPage = intRowPerPage == 0 ? 1 : intRowPerPage;

            int intPageNumber = 1,
                i = 1,
                intTotalPage,
                intStart = 0;

            if ((intTotal % intRowPerPage) > 0)
            {
                intTotalPage = (intTotal / intRowPerPage) + 1;
            }
            else
            {
                intTotalPage = intTotal / intRowPerPage;
            }

            if (intRowTo > intTotal) intRowTo = intTotal;

            if (intTotalPage >= 1)
            {
                strReturn.Append("<div id='pager' class='row'>");
                strReturn.Append("<div class='col-md-4 col-sm-6'>");
                strReturn.Append("<div class='dataTables_info' id='sample_1_info' role='status' aria-live='polite'>");
                strReturn.Append("Hiển thị ").Append(intRowFrom.ToString().Trim()).Append(" tới ").Append(intRowTo.ToString().Trim()).Append(" của ").Append(intTotal.ToString().Trim()).Append(" bản ghi");
                strReturn.Append("</div></div>");
                strReturn.Append("<div class='col-md-7 col-sm-6' style='float: right;text-align: right;'>");
                strReturn.Append("<div class='dataTables_paginate paging_bootstrap_full_number' id='sample_1_paginate'>");
                strReturn.Append("<ul class='pagination' style='visibility: visible;'>");

                if (intCurrPage <= intTotalPage)
                {
                    if (intCurrPage == 1)
                    {
                        intPageNumber = intPageSize;
                        if (intPageNumber > intTotalPage)
                        {
                            intPageNumber = intTotalPage;
                        }
                        intStart = 1;
                    }
                    else
                    {
                        strReturn.Append("<li class='prev'>");
                        strReturn.Append("<a onclick=").Append(function).Append("(1) href='javascript:void(0)' id='0' Title='First'>");
                        strReturn.Append("<i class='fa fa-angle-left'></i><i class='fa fa-angle-left'></i>");
                        strReturn.Append("</a></li>");
                        strReturn.Append("<li class='prev'>");
                        strReturn.Append("<a onclick=").Append(function).Append("(").Append((intCurrPage - 1).ToString().Trim()).Append(") href='javascript:void(0)' Title='Prev' id='").Append((intCurrPage - 1).ToString().Trim()).Append("'>");
                        strReturn.Append("<i class='fa fa-angle-left'></i>");
                        strReturn.Append("</a></li>");

                        if ((intTotalPage - intCurrPage) < (intPageSize / 2))
                        {
                            intStart = (intTotalPage - intPageSize) + 1;
                            if (intStart <= 0)
                            {
                                intStart = 1;
                            }
                            intPageNumber = intTotalPage;
                        }
                        else
                        {
                            if ((intCurrPage - (intPageSize / 2)) == 0)
                            {
                                intStart = 1;
                                intPageNumber = intCurrPage + (intPageSize / 2) + 1;
                                if (intTotalPage < intPageNumber)
                                {
                                    intPageNumber = intTotalPage;
                                }
                            }
                            else
                            {
                                intStart = intCurrPage - (intPageSize / 2);
                                if (intStart <= 0)
                                {
                                    intStart = 1;
                                }
                                intPageNumber = intCurrPage + (intPageSize / 2);
                                if (intTotalPage < intPageNumber)
                                {
                                    intPageNumber = intTotalPage;
                                }
                                else
                                {
                                    if (intPageNumber < intPageSize)
                                    {
                                        intPageNumber = intPageSize;
                                    }
                                }


                            }
                        }
                    }

                    i = intStart;
                    while (i <= intPageNumber)
                    {
                        if (i == intCurrPage)
                        {
                             strReturn.Append("<li class='active'><a href='javascript:void(0)'>").Append(i.ToString().Trim()).Append("</a></li>");
                        }
                        else
                        {
                            strReturn.Append("<li><a href='javascript:void(0)' onclick=").Append(function).Append("(").Append(i.ToString().Trim()).Append(") id ='").Append(i.ToString().Trim()).Append("'>").Append(i.ToString().Trim()).Append("</a></li>");
                        }

                        i++;
                    }

                    if (intCurrPage < intTotalPage)
                    {
                       strReturn.Append("<li class='next'>");
                       strReturn.Append("<a href='javascript:void(0)' onclick=" + function + "(" + (intCurrPage + 1).ToString().Trim() + ") Title='Next' id='" + (intCurrPage + 1).ToString().Trim() + "'>");
                       strReturn.Append("<i class='fa fa-angle-right'></i></a></li>");
                       strReturn.Append("<li class='next'>");
                       strReturn.Append("<a href='javascript:void(0)' onclick=" + function + "(" + intTotalPage.ToString().Trim() + ") Title ='Last' id='" + intTotalPage.ToString().Trim() + "'>");
                       strReturn.Append("<i class='fa fa-angle-right'></i><i class='fa fa-angle-right'></i></a></li>");
                    }

                    strReturn.Append("</ul></div></div></div>");
                }
            }
            return strReturn.ToString();
        }

        public static bool ToBoolean(string value)
        {
            switch (value.ToLower())
            {
                case "true":
                    return true;
                case "t":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                case "false":
                    return false;
                case "f":
                    return false;
                default:
                    throw new InvalidCastException("Không thể đổi kiểu dữ liệu thành Bool");
            }
        }
    }
}