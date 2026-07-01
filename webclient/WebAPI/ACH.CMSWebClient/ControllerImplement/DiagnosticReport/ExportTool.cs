using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DevTree.Entity;
using ACH.Helper.Comparer;
using ACH.Helper.Image;
using ACH.Helper.ImageSizeReader;
using ACH.Helper.ImageSizeReader;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Reflection;
using Draw = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace ACH.CMSWebClient.ControllerImplement.DiagnosticReport
{
    public static class ReportExportTool
    {
        /// <summary>
        /// 导出报告
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="outputPath"></param>
        /// <param name="data"></param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Export(string templatePath, string outputPath, WindparkDiagReportExportDTO data, IConfiguration configuration)
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"模板文件不存在: {templatePath}");
            }
            if (File.Exists(outputPath))
            {
                // 如果输出路径存在，则删除原有文件
                File.Delete(outputPath);
            }
            File.Copy(templatePath, outputPath, overwrite: true);
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据对象不能为空");
            }
            var placeholders = ConvertObjectToDictionary(data);
            // 打开模板文件
            using (WordprocessingDocument document = WordprocessingDocument.Open(outputPath, true))
            {
                // 初始化标题样式映射，以便在替换占位符时使用正确的样式
                InitHeadingStyle(document);
                // 遍历所有段落
                foreach (var paragraph in document.MainDocumentPart!.Document.Body!.Elements<Paragraph>())
                {
                    // 替换段落中的文本占位符
                    ReplacePlaceholdersInParagraph(paragraph, placeholders);
                    // 替换段落中机组报告占位符
                    ReplacePlaceholderWithReportInParagraph(document, paragraph, data.WindturbineReportGuid, configuration);
                }
                // 如果有表格，则也需要遍历表格中的每个单元格
                foreach (var table in document.MainDocumentPart.Document.Body.Elements<Table>())
                {
                    foreach (var row in table.Elements<TableRow>())
                    {
                        foreach (var cell in row.Elements<TableCell>())
                        {
                            foreach (var paragraph in cell.Elements<Paragraph>())
                            {
                                // 替换单元格中的段落中的占位符
                                ReplacePlaceholdersInParagraph(paragraph, placeholders);
                                // 替换段落中的表格占位符
                                ReplacePlaceholderWithTableInParagraph(paragraph, new Dictionary<string, Table>
                                {
                                    { "{{HealthStatusTable}}", CreateHealthStatusTable(data.WindturbineHealthStatusList) },
                                    { "{{DiagConclusionTable}}", CreateDiagConclusionTable(data.WindturbineDiagConclusionList,data.WindparkName) },
                                });
                            }
                        }
                    }
                }
                // 保存更改
                document.MainDocumentPart.Document.Save();
            }
        }


        #region 替换占位符内容
        /// <summary>
        /// 替换段落中的占位符为文本
        /// </summary>
        /// <param name="paragraph"></param>
        /// <param name="replacements"></param>       
        private static void ReplacePlaceholdersInParagraph(Paragraph paragraph, Dictionary<string, string> replacements)
        {
            // 初始化一个列表来存储需要缩进的占位符，以便在替换后进行缩进
            var needIndentationPlaceholder = new List<string> { "{{DangerWindturbineContent}}", "{{WarningWindturbineContent}}", "{{AttentionWindturbineContent}}" };
            foreach (var run in paragraph.Elements<Run>().ToList())
            {
                foreach (var text in run.Elements<Text>().ToList())
                {
                    foreach (var replacement in replacements)
                    {
                        if (text.Text.Contains(replacement.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (needIndentationPlaceholder.Contains(replacement.Key))
                            {
                                // 分割文本
                                string[] lines = replacement.Value.Split("\r\n", StringSplitOptions.None);
                                var newRun = new Run();
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    // 添加缩进，如果占位符在需要缩进的列表中
                                    newRun.Append(new Text("\t" + lines[i]));
                                    if (i < lines.Length - 1)
                                        newRun.Append(new Break());
                                }
                                run.Remove();
                                paragraph.Append(newRun);
                            }
                            else
                            {
                                // 如果不是多行文本，则直接替换
                                text.Text = text.Text.Replace(replacement.Key, replacement.Value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 替换段落中的占位符为表格
        /// </summary>
        /// <param name="paragraph"></param>
        /// <param name="replacements"></param>
        private static void ReplacePlaceholderWithTableInParagraph(Paragraph paragraph, Dictionary<string, Table> replacements)
        {
            var runsToRemove = new List<Run>();
            var runReplacements = new Dictionary<Run, List<(string placeholder, Table table)>>();

            // 1. 收集需要替换的 Run
            foreach (var run in paragraph.Elements<Run>().ToList())
            {
                var textElement = run.Elements<Text>().FirstOrDefault();
                if (textElement != null)
                {
                    foreach (var replacement in replacements)
                    {
                        if (textElement.Text.Contains(replacement.Key))
                        {
                            if (!runReplacements.ContainsKey(run))
                                runReplacements[run] = new List<(string, Table)>();

                            runReplacements[run].Add((replacement.Key, replacement.Value));
                            runsToRemove.Add(run);
                        }
                    }
                }
            }

            // 2. 执行替换
            if (runsToRemove.Count > 0)
            {
                bool isOnlyRun = paragraph.Elements<Run>().Count() == runsToRemove.Count;
                var parent = paragraph.Parent; // 可能是 Body 或 TableCell

                foreach (var run in runsToRemove)
                {
                    if (runReplacements.TryGetValue(run, out var matches))
                    {
                        foreach (var match in matches)
                        {
                            var clonedTable = (Table)match.table.CloneNode(true);

                            if (parent is Body body)
                            {
                                // 正文段落：插入到段落前，然后删除段落
                                body.InsertBefore(clonedTable, paragraph);
                            }
                            else if (parent is TableCell cell)
                            {
                                // 表格单元格：插入到段落前（单元格内）
                                cell.InsertBefore(clonedTable, paragraph);
                            }
                        }
                    }
                    run.Remove();
                }

                if (isOnlyRun && parent is Body)
                {
                    // 检查段落是否还有其他内容（如书签、注释等）
                    if (!paragraph.Elements().Any(e => e is not ParagraphProperties))
                    {
                        paragraph.Remove(); // 彻底删除空段落
                    }
                }
            }
        }

        /// <summary>
        /// 替换设备报告占位符
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="placeholder"></param>
        /// <param name="deviceReportsList"></param>
        private static void ReplacePlaceholderWithReportInParagraph(WordprocessingDocument doc, Paragraph placeholder, List<string> deviceReportsList, IConfiguration configuration)
        {

            foreach (Run run in placeholder.Elements<Run>().ToList())
            {
                Text? textElement = run.Elements<Text>().FirstOrDefault();
                if (textElement != null)
                {
                    string originalText = textElement.Text;
                    if (originalText.Contains("{{DeviceReports}}", StringComparison.OrdinalIgnoreCase))
                    {
                        // 先根据机组报告ID获取需要整合的机组报告
                        List<DiagnosisReportDTO> deviceReports = new List<DiagnosisReportDTO>();
                        for (int i = 0; i < deviceReportsList.Count; i++)
                        {
                            DiagnosisReportDTO windturbineReport = new DiagnosticReport(configuration).GetDiagnosisReport(deviceReportsList[i]);
                            deviceReports.Add(windturbineReport);
                        }

                        // 对所有报告按照机组ID排序
                        deviceReports = deviceReports.OrderBy(o => o.WindturbineId).ToList();


                        // 遍历所有报告，插入到风场报告中
                        for (int i = 0; i < deviceReports.Count; i++)
                        {
                            CreateDeviceReport(doc, placeholder, deviceReports[i]);
                            if (i < deviceReportsList.Count - 1)
                            {
                                // 插入分页符,在最后一个设备报告后不插入分页符
                                placeholder.InsertBeforeSelf(new Paragraph()
                                {
                                    ParagraphProperties = new ParagraphProperties(
                                        new PageBreakBefore()),
                                });
                            }
                        }

                        run.Remove();
                    }
                }
            }
        }
        #endregion


        #region 创建表格并在表格中添加数据
        /// <summary>
        /// 创建健康状态表格，并填充数据
        /// </summary>
        /// <returns></returns>
        private static Table CreateHealthStatusTable(List<WindturbineHealthStatusExportDTO> list)
        {
            // 创建表格
            Table table = CreateDefaultTable();
            //插入表头
            TableRow heard = new TableRow();
            heard.Append(CreateTableCell("风机编号"));
            heard.Append(CreateTableCell("主轴承"));
            heard.Append(CreateTableCell("齿轮箱"));
            heard.Append(CreateTableCell("发电机"));
            table.Append(heard);
            if (list == null || list.Count == 0)
            {
                return table;
            }
            foreach (var item in list.OrderBy(o => o.WindturbineId))
            {
                TableRow row = new TableRow();
                row.Append(CreateTableCell(item.WindturbineName));
                row.Append(CreateTableCell(item.MainBearingStatus));
                row.Append(CreateTableCell(item.GearBoxStatus));
                row.Append(CreateTableCell(item.GeneratorStatus));
                table.Append(row);
            }
            return table;
        }

        /// <summary>
        /// 创建诊断结论表格，并填充数据
        /// </summary>
        /// <returns></returns>
        private static Table CreateDiagConclusionTable(List<WindturbineDiagConclusionExportDTO> list, string windparkName)
        {
            // 创建表格
            Table table = CreateDefaultTable();
            //插入表头
            TableRow fristHeard = new TableRow();
            fristHeard.Append(CreateTableCell(windparkName, true, BackgroundColor.LightGray));
            for (int i = 0; i < 4; i++)
            {
                fristHeard.Append(CreateTableCell(""));
            }
            table.Append(fristHeard);
            //合并表头风电场名称单元格
            table.MergeCellsHorizontally(0, 0, 4);
            TableRow secondHeard = new TableRow();
            secondHeard.Append(CreateTableCell1("风机编号", true, "", true, true, "4000"));
            secondHeard.Append(CreateTableCell1("诊断结论", true, "", true, true, "6500"));
            secondHeard.Append(CreateTableCell1("状态", true, "", true, true, "3000"));
            secondHeard.Append(CreateTableCell1("维护建议", true, "", true, true, "7500"));
            secondHeard.Append(CreateTableCell1("运行建议", true, "", true, true, "4000"));
            table.Append(secondHeard);
            var rowIndex = 2;

            // 先按照机组名称排序
            var deviceList = list.GroupBy(x => x.WindturbineName).OrderBy(o => o.Key);
            foreach (var item in deviceList)
            {
                // 按照故障结论中的部件名称排序
                var data = item.ToList().SortByName(ascending: true, dictType: EnumSortType.CompName);

                foreach (var item2 in item)
                {
                    TableRow row = new TableRow();
                    row.Append(CreateTableCell1(item.Key, false, "", true, true));
                    row.Append(CreateTableCell1(item2.DiagnosisConclusion, false, "", false, true));
                    row.Append(CreateTableCell1(item2.Status, false, "", true, true));
                    row.Append(CreateTableCell1(item2.MaintainAdvice, false, "", false, true));
                    row.Append(CreateTableCell1(item2.RuningAdvice, false, "", true, true));
                    table.Append(row);
                }
                //合并单元格
                table.MergeCellsVertically(0, rowIndex, rowIndex + item.Count() - 1);
                table.MergeCellsVertically(4, rowIndex, rowIndex + item.Count() - 1);
                rowIndex += item.Count();
            }
            return table;
        }


        /// <summary>
        /// 创建设备报告部分。
        /// </summary>
        /// <param name="body"></param>
        private static void CreateDeviceReport(WordprocessingDocument doc, Paragraph placeholder, DiagnosisReportDTO reportDTO)
        {
            AddParagraphWithStyle(placeholder, reportDTO.WindTurbine.WindTurbineName, "Heading1");
            AddParagraphWithStyle(placeholder, "一、机组信息", "Heading2");
            CreateWindturbineInfoTable(placeholder, reportDTO.WindTurbine);
            AddParagraphWithStyle(placeholder, "二、健康状态", "Heading2");
            CreateWindturbineHealthStatusTable(placeholder, reportDTO.Conclusions);
            AddParagraphWithStyle(placeholder, "三、详细分析", "Heading2");
            CreateDeviceDetailedAnalysisTable(doc.MainDocumentPart!, placeholder, reportDTO);
            AddParagraphWithStyle(placeholder, "四、诊断结论", "Heading2");
            CreateDeviceDiagnosisConclusion(placeholder, reportDTO.Conclusions);
            AddParagraphWithStyle(placeholder, "五、维护建议", "Heading2");
            CreateDeviceMaintenanceAdvice(placeholder, reportDTO.Conclusions);
            AddParagraphWithStyle(placeholder, "六、运行建议", "Heading2");
            CreateDeviceRuningAdvice(placeholder, reportDTO.RuningAdvice);
        }
        /// <summary>
        /// 创建机组详细信息表格
        /// </summary>
        /// <param name="placeholder"></param>
        private static void CreateWindturbineInfoTable(Paragraph placeholder, DiagnosisWindTurbineDTO turbineDTO)
        {
            var table = CreateDefaultTable();
            var row1 = new TableRow();
            row1.Append(CreateTableCell("风机编号", true));
            row1.Append(CreateTableCell(turbineDTO.WindTurbineName));
            row1.Append(CreateTableCell("风场名称", true));
            row1.Append(CreateTableCell(turbineDTO.WindParkName));
            table.Append(row1);
            var row2 = new TableRow();
            row2.Append(CreateTableCell("风机厂家及型号", true));
            row2.Append(CreateTableCell(turbineDTO.Manufactory + "_" + turbineDTO.WindTurbineType));
            row2.Append(CreateTableCell("传动形式及传动比", true));
            row2.Append(CreateTableCell(turbineDTO.TransmissionFormAndRatio));
            table.Append(row2);
            var row3 = new TableRow();
            row3.Append(CreateTableCell("样本数据发电机转速", true));
            row3.Append(CreateTableCell($"{turbineDTO.SampleDataSpeed.ToString()}rpm"));
            row3.Append(CreateTableCell("发电机额定转速", true));
            row3.Append(CreateTableCell($"{turbineDTO.RatedGeneratorSpeed.ToString()}rpm"));
            table.Append(row3);
            placeholder.InsertBeforeSelf(table);
        }
        /// <summary>
        /// 创建机组健康状态表格
        /// </summary>
        /// <param name="placeholder"></param>
        private static void CreateWindturbineHealthStatusTable(Paragraph placeholder, List<DiagnosisReportConclusionTreeDTO> diagnoses)
        {
            var table = CreateDefaultTable();
            var row1 = new TableRow();
            row1.Append(CreateTableCell("部件名称", true));
            row1.Append(CreateTableCell("主轴承", true));
            row1.Append(CreateTableCell("齿轮箱", true));
            row1.Append(CreateTableCell("发电机", true));
            table.Append(row1);
            var row2 = new TableRow();
            row2.Append(CreateTableCell("健康等级", true));
            //主轴状态
            var status = diagnoses.Find(x => x.Name == "主轴")?.CompStatus;
            row2.Append(CreateTableCell(DiagnosticConclusion.ConvertStatusReverse(status)));
            //齿轮箱状态
            status = diagnoses.Find(x => x.Name == "齿轮箱")?.CompStatus;
            row2.Append(CreateTableCell(DiagnosticConclusion.ConvertStatusReverse(status)));
            //发电机状态
            status = diagnoses.Find(x => x.Name == "发电机")?.CompStatus;
            row2.Append(CreateTableCell(DiagnosticConclusion.ConvertStatusReverse(status)));
            table.Append(row2);
            placeholder.InsertBeforeSelf(table);
        }

        /// <summary>
        /// 创建机组详细分析部分。
        /// </summary>
        /// <param name="mainPart"></param>
        /// <param name="placeholder"></param>
        private static void CreateDeviceDetailedAnalysisTable(MainDocumentPart mainPart, Paragraph placeholder, DiagnosisReportDTO reportDTO)
        {
            var table = CreateStableImageTextTable();
            int imageIndex = 1;
            // 生成报告时对测点进行排序
            var analyzerRecord = reportDTO.AnalyzerRecords.SortByName(ascending: true, dictType: EnumSortType.MeaslocName);
            foreach (var record in analyzerRecord)
            {
                DevMeasLocation devMeasLocation = DevTreeRepsitory.Instance.GetMeasLocationByMeasID(record.MeaslocId);
                // 判定状态是否为正常，正常的话后面加“，未见异常频率。”
                var row = new TableRow();
                var imageName = $"图{imageIndex} {devMeasLocation.MeasLoctionName}{record.ImageType}";
                string description = record.Description == "未见异常" ? $"图{imageIndex}为{devMeasLocation.MeasLoctionName}{record.ImageType}，未见异常频率。" : $"图{imageIndex}为{devMeasLocation.MeasLoctionName}{record.ImageType}，{record.Description}";

                row.Append(CreateTableCellWithImage(record.Image, mainPart, imageName, "7200"));
                row.Append(CreateTableCell2(description, "1800"));
                table.Append(row);
                imageIndex++;
            }
            var lastrow = new TableRow();
            var lastRowText = $"小结：{string.Join(";", reportDTO.Conclusions.SelectMany(x => x.Children).Select(x => x.DiagnosisConclusion))}。";
            lastrow.Append(CreateTableCell1(lastRowText, true, "", false));
            lastrow.Append(CreateTableCell(""));
            table.Append(lastrow);
            table.MergeCellsHorizontally(reportDTO.AnalyzerRecords.Count, 0, 1);
            placeholder.InsertBeforeSelf(table);
        }

        /// <summary>
        /// 创建机组诊断结论
        /// </summary>
        /// <param name="placeholder"></param>
        private static void CreateDeviceDiagnosisConclusion(Paragraph placeholder, List<DiagnosisReportConclusionTreeDTO> diagnoses)
        {
            var number = 1;
            foreach (var conclusion in diagnoses.SelectMany(x => x.Children))
            {
                var advice = $"{number}. {conclusion.DiagnosisConclusion}。健康等级：";
                var paragraph = new Paragraph(new Run(new Text(advice)));
                Run run = new Run();
                RunProperties runProperties = new RunProperties();
                runProperties.Append(new Shading()
                {
                    Fill = conclusion.Status.ConvertStatusToColor(), // 设置文字背景色
                    Val = ShadingPatternValues.Clear,
                });
                run.Append(runProperties);
                run.Append(new Text(DiagnosticConclusion.ConvertStatusReverse(conclusion.Status)));
                paragraph.Append(run);
                placeholder.InsertBeforeSelf(paragraph);
                number++;
            }
        }

        /// <summary>
        /// 创建机组维护建议
        /// </summary>
        /// <param name="placeholder"></param>
        private static void CreateDeviceMaintenanceAdvice(Paragraph placeholder, List<DiagnosisReportConclusionTreeDTO> diagnoses)
        {
            var number = 1;
            foreach (var conclusion in diagnoses.SelectMany(x => x.Children))
            {
                var advice = $"{number}. {conclusion.MaintainAdvice}";
                var paragraph = new Paragraph(new Run(new Text(advice)));
                placeholder.InsertBeforeSelf(paragraph);
                number++;
            }
        }

        /// <summary>
        /// 创建机组运行建议表格
        /// </summary>
        /// <param name="placeholder"></param>
        private static void CreateDeviceRuningAdvice(Paragraph placeholder, string runingAdvice)
        {
            var paragraph = new Paragraph(new Run(new Text(runingAdvice)));
            placeholder.InsertBeforeSelf(paragraph);
        }

        #endregion


        #region 创建表格方法汇总
        /// <summary>
        /// 创建默认表格，并填充数据
        /// </summary>
        /// <returns></returns>
        private static Table CreateDefaultTable()
        {
            // 创建表格
            Table table = new Table();
            // 设置表格宽度和边框等样式
            TableProperties tblProp = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new TableLayout { Type = TableLayoutValues.Fixed }
                ),
                new TableWidth() { Type = TableWidthUnitValues.Pct, Width = "100%" } // 宽度设置为100%
            );
            table.AppendChild(tblProp);
            return table;
        }

        /// <summary>
        /// 为满足图片和文字比例为4:1的文字，特殊创建表格
        /// </summary>
        /// <returns></returns>
        public static Table CreateStableImageTextTable()
        {
            var table = new Table();

            table.AppendChild(new TableProperties(
                new TableLayout { Type = TableLayoutValues.Fixed }, // 必须保留
                    new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }, // 100%宽度
                    new TableStyle { Val = "TableGrid" },
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 12 },
                        new BottomBorder { Val = BorderValues.Single, Size = 12 },
                        new LeftBorder { Val = BorderValues.Single, Size = 12 },
                        new RightBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
                    ))
                );

            // 2. 列宽（必须）
            var grid = new TableGrid(new GridColumn { Width = "7200" }, new GridColumn { Width = "1800" });
            table.AppendChild(grid);

            return table;   // 后面再追加行
        }

        /// <summary>
        /// 创建文本单元格
        /// </summary>
        /// <param name="text">单元格文本</param>
        /// <param name="isBold">是否加粗</param>
        /// <param name="backgroundColor">单元格背景色</param>
        /// <returns></returns>
        private static TableCell CreateTableCell(string text, bool isBold = false, string backgroundColor = "")
        {
            var cell = new TableCell();
            var textContent = new Run(new Text(text));
            if (isBold)
            {
                textContent.RunProperties = new RunProperties(new Bold());
            }
            cell.AppendChild(new Paragraph(textContent));

            // 如果背景颜色值为空，根据文本内容获取背景色
            if (string.IsNullOrEmpty(backgroundColor))
            {
                backgroundColor = text.ConvertStatusToColor();
            }

            // 设置段落居中（水平居中）
            Paragraph paragraph = cell.Elements<Paragraph>().First();
            paragraph.ParagraphProperties = new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center }
            );
            // 设置垂直居中
            TableCellProperties cellProps = new TableCellProperties(
                new TableCellVerticalAlignment()
                {
                    Val = TableVerticalAlignmentValues.Center,// 垂直居中
                },
                new Shading()
                {
                    Fill = backgroundColor, // 背景颜色
                    Val = ShadingPatternValues.Clear // 清除默认填充模式
                }
            );
            cell.AppendChild(cellProps);
            return cell;

        }


        /// <summary>
        /// 新增：创建文本单元格
        /// </summary>
        /// <param name="text">单元格文本</param>
        /// <param name="isBold">是否加粗</param>
        /// <param name="backgroundColor">单元格背景色</param>
        /// <param name="isCenter">是否居中</param>
        /// <param name="noWrap">是否换行</param>
        /// <param name="widthDxA">宽度</param>
        /// <returns></returns>
        private static TableCell CreateTableCell1(string text, bool isBold = false, string backgroundColor = "", bool isCenter = true, bool noWrap = false, string widthDxA = null)
        {
            var cell = new TableCell();
            var cellProps = new TableCellProperties();


            if (!string.IsNullOrEmpty(widthDxA))
            {
                cellProps.Append(new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = widthDxA });
            }

            cellProps.Append(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });

            // 如果背景颜色值为空，根据文本内容获取背景色
            if (string.IsNullOrEmpty(backgroundColor))
            {
                backgroundColor = text.ConvertStatusToColor();
            }

            if (!string.IsNullOrEmpty(backgroundColor) && backgroundColor != "")
            {
                cellProps.Append(new Shading { Fill = backgroundColor, Val = ShadingPatternValues.Clear });
            }

            cell.Append(cellProps);

            // 段落和文本属性（同方案一）
            var runProps = new RunProperties();
            if (isBold) runProps.Append(new Bold());
            if (noWrap) runProps.Append(new NoWrap());

            var run = new Run(new Text(text));
            if (runProps.HasChildren) run.RunProperties = runProps;

            var paraProps = new ParagraphProperties(
                new Justification { Val = isCenter ? JustificationValues.Center : JustificationValues.Left }
            );

            var paragraph = new Paragraph();
            paragraph.Append(paraProps);
            paragraph.Append(run);

            cell.Append(paragraph);
            return cell;
        }

        /// <summary>
        /// 针对4:1的文本框创建单元格方法
        /// </summary>
        /// <param name="text"></param>
        /// <param name="widthDxA"></param>
        /// <returns></returns>
        private static TableCell CreateTableCell2(string text, string widthDxA = null)
        {
            var cell = new TableCell();
            var cellProps = new TableCellProperties();


            cell.Append(new TableCellProperties(
                new TableCellWidth { Width = widthDxA, Type = TableWidthUnitValues.Dxa },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            ));


            cellProps.Append(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });

            cell.PrependChild(cellProps);

            var run = new Run(new Text(text));
            var paraProps = new ParagraphProperties(
                new Justification { Val = JustificationValues.Center }
            );

            var paragraph = new Paragraph();
            paragraph.Append(paraProps);
            paragraph.Append(run);

            cell.Append(paragraph);
            return cell;
        }

        /// <summary>
        /// 4:1表格中的图片单元框，创建添加图片的单元格
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="mainPart"></param>
        /// <param name="imageName"></param>
        /// <param name="widthDxA"></param>
        /// <returns></returns>
        public static TableCell CreateTableCellWithImage(string imageUrl, MainDocumentPart mainPart, string imageName, string widthDxA = null)
        {
            var cell = new TableCell();

            // 单元格属性：居中 + 固定宽度
            cell.Append(new TableCellProperties(
                new TableCellWidth { Width = widthDxA.ToString(), Type = TableWidthUnitValues.Dxa },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            ));

            // 2. 处理图片
            ImageInfo image = ImageTypeReader.GetByteTypeByImageUri(imageUrl);
            // 图片缩放
            ImageSize size = ImageTypeReader.ScaleToFit(image, 11 * 360000L + 50 * 3600L, null);

            // 3. 预处理图片为高清版本
            // byte[] highResImageBytes = image.ImageByte;
            byte[] highResImageBytes = HighQualityImage.PrepareHighQualityImageBytes(image, size);

            // ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
            ImagePart imagePart = mainPart.AddNewPart<ImagePart>(ImageTypeReader.GetMimeType(image.ImageType));
            using (MemoryStream imageStream = new MemoryStream(highResImageBytes))
            {
                imagePart.FeedData(imageStream);
            }
            string imageId = mainPart.GetIdOfPart(imagePart);

            // 4. 图片段落（居中）
            cell.Append(CreateImageParagraph(imageId, size));

            // 5. 名称段落（居中）
            if (!string.IsNullOrEmpty(imageName))
            {
                cell.Append(CreateTextParagraph(imageName, JustificationValues.Center));
            }

            return cell;
        }

        /// <summary>
        /// 生成图片段落
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static Paragraph CreateImageParagraph(string imageId, ImageSize size)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center }
                ),
                new Run(
                    new Drawing(
                        new Inline(
                            new Extent { Cx = size.WidthEmu, Cy = size.HeightEmu },
                            new EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                            new DocProperties { Id = 1U, Name = "Image" },
                            new NonVisualGraphicFrameDrawingProperties(
                                new Draw.GraphicFrameLocks { NoChangeAspect = true }
                            ),
                            new Draw.Graphic(
                                new Draw.GraphicData(
                                    new PIC.Picture(
                                        new PIC.NonVisualPictureProperties(
                                            new PIC.NonVisualDrawingProperties { Id = 2U, Name = "Image" },
                                            new PIC.NonVisualPictureDrawingProperties()
                                        ),
                                        new PIC.BlipFill(
                                            new Draw.Blip { Embed = imageId },
                                            new Draw.SourceRectangle(),
                                            new Draw.Stretch(new Draw.FillRectangle())
                                        ),
                                        new PIC.ShapeProperties(
                                            new Draw.Transform2D(
                                                new Draw.Offset { X = 0L, Y = 0L },
                                                new Draw.Extents { Cx = size.WidthEmu, Cy = size.HeightEmu }
                                            ),
                                            new Draw.PresetGeometry(new Draw.AdjustValueList())
                                            {
                                                Preset = Draw.ShapeTypeValues.Rectangle
                                            }
                                        )
                                    )
                                )
                            )
                        )
                        {
                            DistanceFromTop = 0U,
                            DistanceFromBottom = 0U,
                            DistanceFromLeft = 0U,
                            DistanceFromRight = 0U
                        }
                    )
                )
            );

            return paragraph;
        }

        // 提取的公共方法：文本段落
        private static Paragraph CreateTextParagraph(string text, JustificationValues alignment)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = alignment }
                ),
                new Run(new Text(text))
            );
        }

        #endregion


        #region 单元格合并方法
        /// <summary>
        /// 合并表格单元格（水平方向）。
        /// </summary>
        /// <param name="table">表格对象</param>
        /// <param name="rowIndex">指定行</param>
        /// <param name="startCellIndex">开始列位置</param>
        /// <param name="endCellIndex">结束列位置</param>
        /// <exception cref="ArgumentException"></exception>
        private static void MergeCellsHorizontally(this Table table, int rowIndex, int startCellIndex, int endCellIndex)
        {
            // 获取指定行
            var row = table.Descendants<TableRow>().ElementAt(rowIndex);
            // 获取行中单元格的总数
            var cellCnt = row.Descendants<TableCell>().Count();
            // 检查索引是否有效
            if (!(startCellIndex >= 0 && startCellIndex < cellCnt && endCellIndex >= 0 && endCellIndex < cellCnt && startCellIndex <= endCellIndex))
                throw new ArgumentException("Invalid cell index");
            // 获取起始单元格
            var startCell = row.Descendants<TableCell>().ElementAt(startCellIndex);
            // 确保单元格属性存在
            var tableCellProperties = startCell.GetFirstChild<TableCellProperties>();
            if (tableCellProperties == null)
            {
                tableCellProperties = new TableCellProperties();
                startCell.PrependChild(tableCellProperties);
            }
            // 合并单元格
            var gridSpan = new GridSpan() { Val = endCellIndex - startCellIndex + 1 };
            // 确保没有重复的 GridSpan 属性
            var existingGridSpan = tableCellProperties.GetFirstChild<GridSpan>();
            if (existingGridSpan != null)
            {
                existingGridSpan.Remove();
            }

            tableCellProperties.Append(gridSpan);
            // 删除要合并的单元格（保留起始单元格）
            for (int i = endCellIndex; i > startCellIndex; i--)
            {
                var cellItem = row.Descendants<TableCell>().ElementAt(i);
                cellItem.Remove();
            }
        }
        /// <summary>
        /// 合并表格单元格（垂直方向）。
        /// </summary>
        /// <param name="table">表单对象</param>
        /// <param name="colIndex">指定列</param>
        /// <param name="startRowIndex">开始行</param>
        /// <param name="endRowIndex">结束行</param>
        /// <exception cref="ArgumentException"></exception>
        private static void MergeCellsVertically(this Table table, int colIndex, int startRowIndex, int endRowIndex)
        {
            var rows = table.Descendants<TableRow>().ToList();

            if (startRowIndex < 0 || startRowIndex >= rows.Count || endRowIndex < 0 || endRowIndex >= rows.Count || startRowIndex > endRowIndex)
                throw new ArgumentException("Invalid row index");
            var startRow = table.Descendants<TableRow>().ElementAt(startRowIndex);
            var startCell = startRow.Descendants<TableCell>().ElementAt(colIndex);
            var tableCellProperties = startCell.GetFirstChild<TableCellProperties>();
            if (tableCellProperties == null)
            {
                tableCellProperties = new TableCellProperties();
                startCell.PrependChild(tableCellProperties);
            }

            // 设置垂直合并属性
            var verticalMerge = new VerticalMerge() { Val = MergedCellValues.Restart };

            // 确保没有重复的 VerticalMerge 属性
            var existingVerticalMerge = tableCellProperties.GetFirstChild<VerticalMerge>();
            if (existingVerticalMerge != null)
            {
                existingVerticalMerge.Remove();
            }

            tableCellProperties.Append(verticalMerge);

            // 合并单元格
            for (int i = startRowIndex + 1; i <= endRowIndex; i++)
            {
                var row = table.Descendants<TableRow>().ElementAt(i);
                if (colIndex < row.Descendants<TableCell>().Count())
                {
                    var cellToRemove = row.Descendants<TableCell>().ElementAt(colIndex);
                    var cellProperties = cellToRemove.GetFirstChild<TableCellProperties>();
                    if (cellProperties != null)
                    {
                        var existingMerge = cellProperties.GetFirstChild<VerticalMerge>();
                        if (existingMerge != null)
                        {
                            existingMerge.Remove();
                        }
                        cellProperties.Append(new VerticalMerge() { Val = MergedCellValues.Continue });
                    }
                }
            }
        }
        #endregion


        #region 设置标题样式
        /// <summary>
        /// 初始化文档的标题样式。
        /// </summary>
        /// <param name="doc"></param>
        private static void InitHeadingStyle(WordprocessingDocument doc)
        {
            MainDocumentPart mainPart = doc.MainDocumentPart!;
            // 获取或创建样式定义部分
            StyleDefinitionsPart stylesPart = mainPart.StyleDefinitionsPart!;
            if (stylesPart == null)
            {
                stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
                stylesPart.Styles = new Styles();
            }
            // 添加标题1样式，并设置大纲级别
            AddHeadingStyle(stylesPart, "Heading1", "Heading 1", 24, true, 1);
            AddHeadingStyle(stylesPart, "Heading2", "Heading 2", 20, true, 2);
        }

        /// <summary>
        /// 添加标题样式到文档中。
        /// </summary>
        /// <param name="stylesPart"></param>
        /// <param name="styleId"></param>
        /// <param name="styleName"></param>
        /// <param name="fontSize"></param>
        /// <param name="isBold"></param>
        /// <param name="outlineLevel"></param>
        private static void AddHeadingStyle(StyleDefinitionsPart stylesPart, string styleId, string styleName, int fontSize, bool isBold, int outlineLevel)
        {
            // 检查样式是否已存在
            var existingStyle = stylesPart.Styles!.Elements<Style>().FirstOrDefault(s => s.StyleId == styleId);
            if (existingStyle != null)
            {
                Console.WriteLine($"样式 {styleId} 已存在。");
                return;
            }

            Style style = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = styleId,
                CustomStyle = true
            };

            style.StyleName = new StyleName() { Val = styleName };
            style.BasedOn = new BasedOn() { Val = "Normal" };
            style.NextParagraphStyle = new NextParagraphStyle() { Val = "Normal" };

            // 设置段落属性
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            paragraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "200" }; // 行距

            // 设置大纲级别
            OutlineLevel outlineLevelElement = new OutlineLevel() { Val = outlineLevel };
            paragraphProperties.Append(outlineLevelElement);

            style.Append(paragraphProperties);

            // 设置运行属性（字体）
            RunProperties runProperties = new RunProperties();
            runProperties.FontSize = new FontSize() { Val = (fontSize * 2).ToString() }; // OpenXML中字体大小单位是半磅
            if (isBold)
            {
                runProperties.Bold = new Bold();
            }
            style.Append(runProperties);

            stylesPart.Styles.Append(style);
        }

        /// <summary>
        /// 添加带有样式的段落。
        /// </summary>
        /// <param name="body"></param>
        /// <param name="text"></param>
        /// <param name="styleId"></param>
        private static void AddParagraphWithStyle(Paragraph placeholder, string text, string styleId, bool isCentered = false)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.ParagraphProperties = new ParagraphProperties(
                new ParagraphStyleId() { Val = styleId }
                );
            if (isCentered)
            {
                paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = styleId }, new Justification() { Val = JustificationValues.Center });
            }
            paragraph.Append(new Run(new Text(text)));
            placeholder.InsertBeforeSelf(paragraph);
        }
        #endregion


        #region 其余方法
        /// <summary>
        /// 将对象转换为字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ConvertObjectToDictionary(WindparkDiagReportExportDTO obj)
        {
            var properties = typeof(WindparkDiagReportExportDTO).GetProperties();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (PropertyInfo property in properties)
            {
                // 忽略列表属性，因为它们将在表格中处理
                if (property.PropertyType == typeof(List<>))
                {
                    continue;
                }
                object? value = property.GetValue(obj);
                if (value != null)
                {
                    dict[property.Name.ConvertToPlaceholder()] = value.ToString() ?? "";
                }
                else
                {
                    dict[property.Name.ConvertToPlaceholder()] = "";
                }
            }
            return dict;
        }

        /// <summary>
        /// 将文本转换为占位符格式。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string ConvertToPlaceholder(this string text)
        {
            return "{{" + text + "}}";
        }

        /// <summary>
        /// 将状态转换为对应的背景颜色。
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private static string ConvertStatusToColor(this string status)
        {
            string backgroundColor = status switch
            {
                "normal" or "正常" => BackgroundColor.Green,
                "attention" or "注意" => BackgroundColor.Yellow,
                "warning" or "警告" => BackgroundColor.Orange,
                "danger" or "危险" => BackgroundColor.Red,
                _ => "",
            };
            return backgroundColor;
        }
        #endregion
    }
    /// <summary>
    /// 背景色枚举。用于设置单元格的背景颜色。
    /// </summary>
    public class BackgroundColor
    {
        /// <summary>
        /// 正常：绿色背景色
        /// </summary>
        public const string Green = "00FF00";
        /// <summary>
        /// 注意：黄色背景色
        /// </summary>
        public const string Yellow = "FFFF00";
        /// <summary>
        /// 警告：橙色背景色
        /// </summary>
        public const string Orange = "FFA500";
        /// <summary>
        /// 危险：红色背景色 
        /// </summary>
        public const string Red = "FF0000";
        /// <summary>
        /// 数据质量异常：浅灰色
        /// </summary>
        public const string LightGray = "D3D3D3";
    }
}
