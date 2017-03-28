using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Util 
{
    public class UserRole {
        public string Title { get; private set; }
        public static IList<UserRole> Roles = new List<UserRole>();

        protected UserRole(String title) {
            Title = title;
            Roles.Add(this);
        }

        public override string ToString() {
            return Title;
        }

        public static UserRole Parse(String title) {
            return Roles.Single(r => r.Title == title.Trim());
        }

        public static UserRole ADMIN = new UserRole("系统管理员");
        public static UserRole CHAIR = new UserRole("院长");

        public static UserRole KSZGY = new UserRole("科室专管员");

        public static UserRole LCYS = new UserRole("临床医生");
        public static UserRole LCKZR = new UserRole("临床科主任");

        public static UserRole KGGBKHGLZ = new UserRole("客观指标考核管理者");
        public static UserRole KGGBKHZGY = new UserRole("客观指标考核专管员");

        public static UserRole CRBCBZGY = new UserRole("传染病传报专管员");
        public static UserRole CRBCBCBGLZ = new UserRole("传染病传报管理者");

        public static UserRole WJZZGY = new UserRole("危急值专管员");
        public static UserRole WJZGLZ = new UserRole("危急值管理者");

        public static UserRole ZLCBZGY = new UserRole("肿瘤传报专管员");
        public static UserRole ZLCBCBGLZ = new UserRole("肿瘤传报管理者");

        public static UserRole ICUYGZGY = new UserRole("ICU院感专管员");
        public static UserRole ICUYGGLZ = new UserRole("ICU院感管理者");

        public static UserRole YGBFZGY = new UserRole("院感爆发专管员");
        public static UserRole YGBFGLZ = new UserRole("院感爆发管理者");

        public static UserRole WSSQKSSZGY = new UserRole("围手术期抗生素专管员");
        public static UserRole WSSQKSSGLZ = new UserRole("围手术期抗生素管理者");

        public static UserRole KSSJCLZGY = new UserRole("抗生素检测率专管员");
        public static UserRole KSSJCLGLZ = new UserRole("抗生素检测率管理者");

        public static UserRole KSSNYXZGY = new UserRole("抗生素耐药性专管员");
        public static UserRole KSSNYXGLZ = new UserRole("抗生素耐药性管理者");

        public static UserRole DZBAKHZGY = new UserRole("电子病案考核专管员");
        public static UserRole DZBAKHGLZ = new UserRole("电子病案考核管理者");



    }
}

