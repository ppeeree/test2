using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMSFramework.BusinessEntity;

namespace WindCMS.IAnalyzerDomain
{
    /// <summary>
    /// 机组报告管理服务 接口契约
    /// </summary>
    public interface ITurbineReportManagement
    {
        
        //-----------------------------------------------------------------------------------------------------------------------
        // author: liyanchao
        // create: 2017-3-7
        /// <summary>
        /// 上传诊断分析报告
        /// </summary>
        /// <param name="_report"></param>
        /// <returns></returns>
        void UploadDiagnosisReportHVersion(CMSFramework.BusinessEntity.WTDiagnosisReport _report);       

    }
}
