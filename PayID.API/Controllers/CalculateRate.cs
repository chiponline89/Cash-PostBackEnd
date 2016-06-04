using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using System.Runtime.Serialization;
using MongoDB.Driver;
namespace PayID.API.Controllers
{   
    public class Charges
    {
        public double CodFee { get; set; }
        public double MainFee { get; set; }
        public double TotalFee { get; set; }
        public double ServiceFee { get; set; }
    }
   

    public interface ICalculateRate
    {
        double getCodFee(string maDichVu, int ngayGui, long giaTri);
        double getKeyCharges(string maBCGoc, string nuocNhan, string maBCTra,
            string nuocTra, long khoiLuong, int ngayGui, int loai, int cuocPPXD, string maKH);
        dynamic[] CalutaltorFee(string ServiceCode, long Value, string FromProvinceCode, string FromCountry, string ToProvinceCode,
           string ToCountry, long Weight, int Type);
        long TinhCuocDichVuKhongTheoBangCuocNew(string tohopma, int ngaygui, string nuoctra
            , string maTinhGoc, string maTinhTra, string makh);
        long TinhCuocDichVuKhongTheoBangCuocToanQuocNew(string tohopma, int ngaygui, string nuoctra, string makh);
        long TinhCuocDichVuKhongTheoBangCuoc(string tohopma, int ngaygui, string nuocTra, string matinhgoc, string matinhtra,
            string makh);

    }
    public class CalculateRate : ICalculateRate
    {
        //private MongoHelper _data = new MongoHelper();
        public long TinhCuocDichVuKhongTheoBangCuocNew(string tohopma, int ngaygui, string nuoctra, string maTinhGoc, string maTinhTra, string makh)
        {
            long cuocdichvu = 0;
            IMongoQuery query1 = Query.GTE("Den_Ngay", ngaygui);
            IMongoQuery query2 = Query.LTE("Tu_Ngay", ngaygui);
            IMongoQuery query3 = Query.EQ("Ma_Dich_Vu", tohopma);
            //IMongoQuery query4 = Query.EQ("Trong_Nuoc", true);
            IMongoQuery query5 = Query.EQ("Ma_BC_Chap_nhan", 0);
            IMongoQuery query6 = Query.EQ("Ma_BC_Phat_tra", 0);
            dynamic getTSTC = PayID.Portal.Areas.Lading.Configuration.Data.Get("Tham_So_Tinh_Cuoc", Query.And(query1, query2, query3, query5, query6));

            if (nuoctra == "VN" && getTSTC != null)
            {
                cuocdichvu = getTSTC.Cuoc_CD;
            }
            return cuocdichvu;
        }
        public long TinhCuocDichVuKhongTheoBangCuocToanQuocNew(string tohopma, int ngaygui, string nuoctra, string makh)
        {
            long cuocdichvu = 0;
            IMongoQuery query1 = Query.GTE("Den_Ngay", ngaygui);
            IMongoQuery query2 = Query.LTE("Tu_Ngay", ngaygui);
            IMongoQuery query3 = Query.EQ("Ma_Dich_Vu", tohopma);
            dynamic getTSTC = PayID.Portal.Areas.Lading.Configuration.Data.Get("Tham_So_Tinh_Cuoc", Query.And(query1, query2, query3));
            if (getTSTC != null)
            {
                if (nuoctra == "VN")
                {
                    cuocdichvu = getTSTC.Cuoc_CD;
                }
            }
            return cuocdichvu;
        }

        // tính cước dịch vụ không theo bảng cước
        public long TinhCuocDichVuKhongTheoBangCuoc(string tohopma, int ngaygui, string nuocTra, string matinhgoc, string matinhtra, string makh)
        {
            long cuocdichvu = 0;
            IMongoQuery query1 = Query.GTE("Den_Ngay", ngaygui);
            IMongoQuery query2 = Query.LTE("Tu_Ngay", ngaygui);
            IMongoQuery query3 = Query.EQ("Ma_Dich_Vu", tohopma);
            dynamic getTSTC = PayID.Portal.Areas.Lading.Configuration.Data.Get("Tham_So_Tinh_Cuoc", Query.And(query1, query2, query3));
            if (getTSTC != null)
            {
                if (getTSTC.Toan_quoc == 1)
                {
                    cuocdichvu = TinhCuocDichVuKhongTheoBangCuocToanQuocNew(tohopma, ngaygui, nuocTra, makh);
                }
                else
                {
                    cuocdichvu = TinhCuocDichVuKhongTheoBangCuocNew(tohopma, ngaygui, nuocTra, matinhgoc, matinhtra, makh);
                }
            }
            return cuocdichvu;
        }


        public dynamic[] CalutaltorFee(string ServiceCode, long Value, string FromProvinceCode, string FromCountry, string ToProvinceCode,
          string ToCountry, long Weight, int Type)
        {
            Charges charges = new Charges();
            double v_CodFee = 0, v_MainFee = 0,v_ServiceFee = 0;
            try
            {
                int ngaygui = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                // Tính cước chính - cước vận chuyển (from - to - weight)
                v_MainFee = getKeyCharges(FromProvinceCode, FromCountry, ToProvinceCode, ToCountry, Weight, ngaygui, Type, 1, "kh");

                if (!String.IsNullOrEmpty(ServiceCode))
                {
                    if (ServiceCode.IndexOf(",") == -1 && ServiceCode != "")
                    {
                        if (ServiceCode != "COD")
                        {
                            v_ServiceFee = TinhCuocDichVuKhongTheoBangCuoc(ServiceCode, ngaygui, "VN", FromProvinceCode, ToProvinceCode, "14240024");
                        }
                        else if (ServiceCode == "COD" && Value != 0)
                        {
                            v_CodFee = getCodFee(ServiceCode, ngaygui, Value);
                        }
                      
                    }
                    else if (ServiceCode.IndexOf(",") != -1)
                    {
                        string[] mang = ServiceCode.Split(',');
                        foreach (var item in mang)
                        {
                            if (item == "COD" && Value != 0)
                            {
                                v_CodFee = getCodFee(item, ngaygui, Value);
                            }
                            else if (item != "COD")
                            {
                                v_ServiceFee = v_ServiceFee + TinhCuocDichVuKhongTheoBangCuoc(item, ngaygui, "VN", FromProvinceCode, ToProvinceCode, "14240024");
                            }
                          
                        }
                    }
                }
                else
                {
                    charges.MainFee = v_MainFee;
                    charges.CodFee = 0;
                    charges.ServiceFee = 0;

                }
                charges.MainFee = v_MainFee;
                charges.CodFee = v_CodFee;
                charges.ServiceFee = v_ServiceFee;
            }
            catch
            {
                charges.CodFee = 0;
                charges.MainFee = 0;
                charges.ServiceFee = 0;              
            }
            Charges[] mang2 = { charges };
            return mang2;
        }

        public double getServiceFee(string ServiceCode, string FromProvinceCode, int ngaygui, string ToProvinceCode)
        {
            try { return TinhCuocDichVuKhongTheoBangCuoc(ServiceCode, ngaygui, "VN", FromProvinceCode, ToProvinceCode, "14240024"); }
            catch { return 0; }
        }
        public double getCodFee(string ServiceCode, int ngayGui, long Value)
        {
            double cuocCOD = -1;
            try
            {

                dynamic[] lstDynamic = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Tham_So_Phu_Tro_Tinh_Cuoc", Query.And(Query.EQ("Don_Vi_Tinh", 4), Query.EQ("Ma_Dich_Vu", ServiceCode)));


                for (int i = 0; i < lstDynamic.Length; i++)
                {
                    if (Value <= long.Parse(lstDynamic[i].Den_Nac.ToString()))
                    {
                        if (lstDynamic[i].Phan_Tram_Cuoc != 0)
                        {
                            if (Math.Round((Value * lstDynamic[i].Phan_Tram_Cuoc)) > lstDynamic[i].Cuoc_Toi_Thieu)
                            {
                                cuocCOD = Math.Round(Value * lstDynamic[i].Phan_Tram_Cuoc);
                            }
                            else { cuocCOD = lstDynamic[i].Cuoc_Toi_Thieu; }
                        }
                        else { cuocCOD = lstDynamic[i].Muc_Cuoc; }
                        break;
                    }
                }
                return cuocCOD;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Ham tim ma tinh
        /// </summary>
        /// <returns></returns>       

        public double getKeyCharges(string FromProvinceCode, string FromCountry, string ToProvinceCode, string ToCountry, long Weight, int ngayGui, int loai, int cuocPPXD, string maKH)
        {
            double phiPhuXangDau = 0, phiPhuVungXa = 0, nacGui = 0, cuoc = 0, timduoc = 0, cuocMax = 0, nacGuimax = 0, cuocChinh = 0;

            int khuVuc = 0;
            if (loai == 1)
            {
                return 0;
            }
            //loai 0 hoac 2
            else if (loai == 0 || loai == 2)
            {

                //lấy khu vực dựa vào mã tỉnh gốc và mã tỉnh trả              

                dynamic zone = PayID.Portal.Areas.Lading.Configuration.Data.Get("Khu_Vuc_TN", Query.And(Query.EQ("Tinh_Chap_Nhan", FromProvinceCode),
                    Query.EQ("Tinh_Phat_Tra", ToProvinceCode)));

                if (zone.Khu_Vuc == 0)
                {
                    return 0;
                }
                //Loại 0
                if (loai == 0)
                {
                    dynamic[] lstCuocTN = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Cuoc_TN", Query.And(
                        Query.GT("Den_Ngay", ngayGui),
                        Query.LT("Tu_Ngay", ngayGui),
                        Query.EQ("Khu_Vuc", zone.Khu_Vuc),
                        Query.EQ("Cach_Tinh", false)));
                    //***                       

                    for (int i = 0; i < lstCuocTN.Length; i++)
                    {
                        nacGui = lstCuocTN[i].Khoi_Luong;
                        cuoc = lstCuocTN[i].Cuoc;
                        if (Weight <= nacGui)
                        {
                            timduoc = 1;
                            cuocMax = cuoc;
                            nacGuimax = nacGui;
                            cuocChinh = cuoc;
                        }
                        else
                        {
                            timduoc = 0;
                            cuocMax = cuoc;
                            nacGuimax = nacGui;
                        }

                        if (timduoc == 0)
                        {
                            dynamic[] lstCuocTN_Found = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Cuoc_TN", Query.And(
                       Query.GT("Den_Ngay", ngayGui),
                       Query.LT("Tu_Ngay", ngayGui),
                       Query.EQ("Khu_Vuc", zone.Khu_Vuc),
                       Query.EQ("Cach_Tinh", true)));

                            cuocChinh = Math.Ceiling(((Weight - nacGuimax) / long.Parse((lstCuocTN_Found[0].Khoi_Luong.ToString())))) * long.Parse(lstCuocTN_Found[0].Cuoc.ToString());
                            cuocChinh = cuocChinh + cuocMax;
                        }
                    }
                }
                #region Thoa thuan
                //if (loai == 2)
                //{
                //    dynamic[] lstCuocTNTT = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Cuoc_TN_TT", Query.And(
                //     Query.GT("Den_Ngay", ngayGui),
                //     Query.LT("Tu_Ngay", ngayGui),
                //     Query.EQ("Khu_Vuc", zone.Khu_Vuc),
                //     Query.EQ("Cach_Tinh", false)));                     //***


                //    for (int i = 0; i < lstCuocTNTT.Length; i++)
                //    {
                //        nacGui = lstCuocTNTT[i].Khoi_Luong;
                //        cuoc = lstCuocTNTT[i].Cuoc;
                //        if (Weight <= nacGui)
                //        {
                //            timduoc = 1;
                //            cuocMax = cuoc;
                //            nacGuimax = nacGui;
                //            cuocChinh = cuoc;
                //        }
                //        else
                //        {
                //            timduoc = 0;
                //            cuocMax = cuoc;
                //            nacGuimax = nacGui;
                //        }
                //        if (timduoc == 0)
                //        {
                //            dynamic[] lstCuocTNTT_Found = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Cuoc_TN_TT", Query.And(
                //      Query.GT("Den_Ngay", ngayGui),
                //      Query.LT("Tu_Ngay", ngayGui),
                //      Query.EQ("Khu_Vuc", zone.Khu_Vuc),
                //      Query.EQ("Cach_Tinh", true)));
                //            cuocChinh = Math.Ceiling(((Weight - nacGuimax) / (lstCuocTN_Found[0].Khoi_Luong.ToString())) * long.Parse(lstCuocTN_Found[0].Cuoc.ToString());
                //            cuocChinh = cuocChinh + cuocMax;
                //        }
                //    }
                //}

                #endregion
                //Tính giảm giá:
                long tongGiamGia = 0, khachHangGiamGia = 0;

                dynamic[] lstCuoc_PPXD_Vat = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Cuoc_PPXD_Vat", Query.And(
                   Query.GT("Den_Ngay", ngayGui),
                   Query.LT("Tu_Ngay", ngayGui)));
                if (FromProvinceCode != ToProvinceCode)
                {
                    phiPhuXangDau = lstCuoc_PPXD_Vat[0].PPXD_TN_LT;
                    phiPhuVungXa = lstCuoc_PPXD_Vat[0].PPVX_TN_LT;
                }
                else
                {
                    phiPhuXangDau = lstCuoc_PPXD_Vat[0].PPXD_TN_NT;
                    phiPhuVungXa = lstCuoc_PPXD_Vat[0].PPVX_TN_NT;
                }

                //Co Phu Phi Vung Xa Trong Nuoc
                if (cuocPPXD == 1)
                {
                    cuocChinh = cuocChinh * (1 + phiPhuXangDau + phiPhuVungXa - tongGiamGia);
                }
                else
                {
                    cuocChinh = cuocChinh * (1 + phiPhuXangDau - tongGiamGia);
                }
                #region Khách hàng giảm giá nội tỉnh.
                if (FromProvinceCode == ToProvinceCode)
                {
                    if (Weight < 100)
                    {
                        khachHangGiamGia = 0;
                        dynamic[] lstKhach_Hang = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Khach_Hang", Query.And(
                 Query.EQ("Ma_KH", maKH)));
                        khachHangGiamGia = lstKhach_Hang[0].Giam_Cuoc;
                        cuocChinh = cuocChinh * (1 - (khachHangGiamGia / 100));
                    }
                }
                #endregion
            }
            #region
            if (loai == 3)
            {
                dynamic[] lstCuoc_Kinh_Te = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Cuoc_Kinh_Te", Query.And(
                  Query.GT("Den_Ngay", ngayGui),
                   Query.LT("Tu_Ngay", ngayGui),
                Query.EQ("Khu_Vuc", khuVuc)));
                dynamic[] lstCuoc_PPXD_Vat = PayID.Portal.Areas.Lading.Configuration.Data.ListDynamic("Cuoc_PPXD_Vat", Query.And(
                  Query.GT("Den_Ngay", ngayGui),
                   Query.LT("Tu_Ngay", ngayGui)));

                cuoc = lstCuoc_Kinh_Te[0].Cuoc;
                if (cuoc > 0)
                {
                    cuocChinh = cuocChinh + ((Weight + 999) / 1000) * cuoc;
                    phiPhuXangDau = 0;
                    if (FromProvinceCode != ToProvinceCode)
                    {
                        phiPhuXangDau = lstCuoc_PPXD_Vat[0].PPXD_TN_LT;
                    }
                    else
                    {
                        phiPhuXangDau = lstCuoc_PPXD_Vat[0].PPXD_TN_NT;
                    }
                    cuocChinh = cuocChinh * (1 + phiPhuXangDau);
                }

            }
            #endregion
            return Math.Round(cuocChinh);
        }

        public int lstCuocTN_Found { get; set; }
    }
}
