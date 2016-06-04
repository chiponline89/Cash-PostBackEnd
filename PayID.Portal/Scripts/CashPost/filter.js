var filterConfig = {
    dateRange: { name: 'DateRange' },
    dateRangeOrder: { name: 'DateRangeOrder' }
};

var filter = {
    initDateRange: function (control, startDate, endDate) {
        $('#txt' + control.name).daterangepicker(
            {
                ranges: {
                    'Hôm nay':[moment(),moment()],
                    '7 ngày trước': [moment().subtract('days', 6), moment()],
                    '30 ngày trước': [moment().subtract('days', 29), moment()],
                    'Quí 1': ['1/1/' + moment().year(), '3/31/' + moment().year()],
                    'Quí 2': ['4/1/' + moment().year(), '6/30/' + moment().year()],
                    'Quí 3': ['7/1/' + moment().year(), '9/30/' + moment().year()],
                    'Quí 4': ['10/1/' + moment().year(), '12/31/' + moment().year()]
                },
                format: 'DD/MM/YYYY',
                startDate: (startDate == null || startDate == undefined || startDate == '') ?  moment() : startDate,
                endDate: (endDate == null || endDate == undefined || endDate == '') ? moment() : endDate,
                showDropdowns: true,
                opens:'left',
                //showWeekNumbers: true,
                locale: {
                    applyLabel: 'Đồng ý',
                    fromLabel: 'Từ ngày',
                    toLabel: 'Đến ngày',
                    customRangeLabel: 'Tùy chọn',
                    weekLabel: 'Tuần',
                    daysOfWeek: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
                    monthNames: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'],
                    firstDay: 1
                }
            }
            //,
            //function (start, end) {
            //    // Khi thời gian thay đổi ==> Lấy lại dữ liệu BookingId & BannerId (Dành riêng cho Báo cáo dữ liệu thực chạy từ hệ thống sản phẩm)
            //    if (utils.getHiddenField('RootGroupFieldName') == 'ReportDataFromProductSystem') {
            //        filter.buildDropDownList([
            //            { control: filterConfig.bookingId, keyword: filterConfig.defaultFilter, isOptionAll: false },
            //            { control: filterConfig.bannerId, keyword: filterConfig.defaultFilter, isOptionAll: false }
            //        ]);
            //    }
            //    console.log(start.toISOString(), end.toISOString());
            //}
            );
        $('#txt' + control.name).attr('readonly', 'readonly');
    }
}