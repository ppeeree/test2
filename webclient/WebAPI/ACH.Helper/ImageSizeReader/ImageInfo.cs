using ACH.DataEntity.Enum;

namespace ACH.Helper.Image
{
    public class ImageInfo
    {
        /// <summary>
        /// Byte[]数组
        /// </summary>
        public byte[] ImageByte { get; set; }

        /// <summary>
        /// 图片类型
        /// </summary>
        public EnumImageType ImageType { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        public long WidthEmu { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        public long HeightEmu { get; set; }
    }
}
