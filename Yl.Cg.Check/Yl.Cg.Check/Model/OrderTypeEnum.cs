using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.Check.Model
{
    public enum OrderTypeEnum
    {
        未知 = 0,
        网上预约 = 1,
        现场售票 = 2,
        旅行社签单_窗口签单售票_扣旅行社余额 = 7,
        票点分销售票 = 8,
        嘉宾票 = 9,
        会客票 = 10,
        手撕票 = 11,
        期效票_预售网站已支付 = 12,
        期效票_窗口售票 = 13,
        预售旅行社网站售票 = 14,
        WAP团队购票 = 15,
        WAP散客购票 = 16,
        电话下单 = 17,
    }
}
