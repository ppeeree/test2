using ACH.CMSWebClient.ControllerModel.DiagnosticReport;
using AutoMapper;

namespace ACH.CMSWebClient.Common
{
    /// <summary>
    /// AutoMapper帮助类，用于简化对象映射操作。
    /// </summary>
    public class AutoMapperHelper
    {
        private static readonly Lazy<IMapper> Mapper = new Lazy<IMapper>(InitializeMapper);

        public static IMapper MapperInstance => Mapper.Value;

        private static IMapper InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // 添加所有需要的映射配置
                cfg.AddProfile<DiagReportMappingProfile>();
                //cfg.AddProfile<MappingProfile>(); 如果你有其他的 MappingProfile 类，也可以在这里添加
            });

            return config.CreateMapper();
        }
        /// <summary>
        /// 映射单个对象
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return MapperInstance.Map<TSource, TDestination>(source);
        }
        /// <summary>
        /// 映射单个对象到已有目标对象
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return MapperInstance.Map(source, destination);
        }
        /// <summary>
        /// 映射集合对象到目标类型
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sources)
        {
            return MapperInstance.Map<IEnumerable<TDestination>>(sources);
        }
    }
}
