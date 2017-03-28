using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Util {
    public class QCModule {

        public UserRole GLZ;
        public UserRole ZGY;
        public String Name;

        private QCModule(String name, UserRole glz, UserRole zgy) {
            Name = name;
            GLZ = glz;
            ZGY = zgy;
        }

        public static QCModule KIndex = new QCModule("客观指标考核", UserRole.KGGBKHGLZ, UserRole.KGGBKHZGY);
        public static QCModule DValue = new QCModule("危急值监控", UserRole.WJZGLZ, UserRole.WJZZGY);
        public static QCModule HIExam = new QCModule("院内感染爆发", UserRole.YGBFGLZ, UserRole.YGBFZGY);
        public static QCModule CHIExam = new QCModule("ICU院内感染", UserRole.ICUYGGLZ, UserRole.ICUYGZGY);
        public static QCModule MedAdvice = new QCModule("围手术期抗生素", UserRole.WSSQKSSGLZ, UserRole.WSSQKSSZGY);
        public static QCModule ACheck = new QCModule("抗生素用药检测", UserRole.KGGBKHGLZ, UserRole.KGGBKHZGY);
        public static QCModule CGExam = new QCModule("传染病传报", UserRole.CRBCBCBGLZ, UserRole.CRBCBZGY);
        public static QCModule CReport = new QCModule("肿瘤传报", UserRole.ZLCBCBGLZ, UserRole.ZLCBZGY);
        public static QCModule EMReport = new QCModule("电子病案考核", UserRole.DZBAKHGLZ, UserRole.DZBAKHZGY);
    }
}
