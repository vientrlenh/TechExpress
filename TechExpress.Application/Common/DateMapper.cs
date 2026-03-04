using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using TechExpress.Repository.CustomExceptions;

namespace TechExpress.Application.Common;

public class DateMapper
{
    private static readonly string[] dateTimeOffsetFormats =
    [
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-dd HH:mm:ss zzz",
        "MM/dd/yyyy HH:mm:ss zzz",
        "dd-MMM-yyyy HH:mm:sszzz",
    ];

    public static DateTimeOffset ConvertToDateTimeOffsetFromString(string dateTimeStr)
    {
        if (DateTimeOffset.TryParseExact(dateTimeStr, dateTimeOffsetFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dtOffset))
        {
            return dtOffset;
        }
        throw new BadRequestException("Định dạng ngày không hợp lệ hoặc không được hỗ trợ");
    }
} 
