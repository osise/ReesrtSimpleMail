using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.IO;
using OfficeOpenXml;

namespace Реестры
{
    public partial class RadForm1 : Telerik.WinControls.UI.RadForm
    {
        private BackgroundWorker bw = new BackgroundWorker();
        public string ExcelFilePathFOL { get; set; } = string.Empty;
        public string ExcelFilePathXLS { get; set; } = string.Empty;
        string[] ourfiles;
        string sourcefile = "";
        DataTable addr = new DataTable();
        DataColumn column;
        DataRow row;
        string fio = "";
        string address = "";
        int cou = 0;
        StringBuilder sb = new StringBuilder();
        public RadForm1()
        {
            InitializeComponent();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        public void OpenDocX(string filename, int rows)
        {
            try
            {
                string datadocx = "";
                fio = "";
                address = "";
                // Open a WordprocessingDocument for editing using the filepath.
                WordprocessingDocument wordprocessingDocument =
                WordprocessingDocument.Open(filename, false);
                // Assign a reference to the existing document body.
                Body body = wordprocessingDocument.MainDocumentPart.Document.Body;
                try
                {
                    datadocx = body.ChildElements[4].InnerText; //получаем значение из надписи в документе
                }
                catch (Exception ex)
                {
                    radRichTextEditor1.Text += "Не могу найти данные в документе. Убедитесь, что содержание документа не изменилось" + "\n";
                    sb.Append(DateTime.Now + ": Не могу найти данные в документе. Убедитесь, что содержание документа не изменилось\r\n");
                }
               

                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options); //Убираем все множественные пробелы и прочие символы
                datadocx = regex.Replace(datadocx, " ");
                // foreach (Match m in Regex.Matches(aaa, @"[А-ЯЁ][а-яё]+\s(([А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+)|([А-ЯЁ]\.\s[А-ЯЁ]\.))"))
                foreach (Match m in Regex.Matches(datadocx, @"[А-ЯЁ][а-яё]+\s(([А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+)|([А-ЯЁ]\.[А-ЯЁ]\.))")) //Отбираем из строки ФИО по формату: "Фамилия И.О.
                {
                    fio = m.Value;
                }
                address = datadocx.Substring(fio.Length).TrimStart(' '); //Убрать пробел спереди

                row = addr.NewRow();
                row["fio"] = fio;
                row["addr"] = address;
                addr.Rows.Add(row);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }
        
        public void AddDataToReestr(string reestrname)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                //the path of the file
                string filePath = reestrname;

                //create a fileinfo object of an excel file on the disk
                FileInfo file = new FileInfo(filePath);

                //create a new Excel package from the file
                using (ExcelPackage excelPackage = new ExcelPackage(file))
                {
                    //create an instance of the the first sheet in the loaded file
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();

                    for (int i = 0; i < addr.Rows.Count; i++)
                    {
                        worksheet.Cells[i+2, 1].Value = addr.Rows[i].ItemArray[0];
                        worksheet.Cells[i+2, 2].Value = addr.Rows[i].ItemArray[1];
                    }
                    //add some data
                    

                    //save the changes
                    excelPackage.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }


        private void radButton1_Click(object sender, EventArgs e)
        {
            try
            {
                ExcelFilePathXLS = "";
                radRichTextEditor1.Text += "\n";
                sourcefile = "";
                sb.Append("\r\n");
                sb.Append("\r\n");
                sb.Append("------------------------ " + DateTime.Now + " ------------------------\r\n");

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Excel Files|*.xlsx";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    sourcefile = Path.GetFileNameWithoutExtension(ofd.SafeFileName);
                    ExcelFilePathXLS = ofd.FileName;
                    radRichTextEditor1.Text += "Выбран файл: " + ofd.FileName + "\n";
                    sb.Append(DateTime.Now + ": Выбран файл: " + ofd.FileName + "\r\n");
                    string Ext1 = Path.GetExtension(ExcelFilePathXLS);
                    if (Ext1 == ".xlsx")
                    {
                        radRichTextEditor1.Text += "Файл успешно открыт\n";
                        sb.Append(DateTime.Now + ": Файл успешно открыт\r\n");
                        if (ExcelFilePathFOL != "" && ExcelFilePathXLS != "")
                        {
                            if (ourfiles.Length > 0)
                            {
                                radButton5.Enabled = true;
                                radRichTextEditor1.Text += "Нажмите кнопку Начать\n";
                            }
                            else
                            {
                                radButton5.Enabled = false;
                                radRichTextEditor1.Text += "\n";
                                radRichTextEditor1.Text += "Выберите непустую папку с файлами\n";
                            }  
                        }
                        else
                        {
                            radButton5.Enabled = false;
                            radRichTextEditor1.Text += "\n";
                            radRichTextEditor1.Text += "Не выбрана папка с файлами и не выбран файл реестра\n";
                        }
                    }
                    else if (Ext1 == ".xls")
                    {
                        radRichTextEditor1.Text += "Не удалось открыть файл. Пересохраните файл в формате .xlsx!" + "\n";
                        sb.Append(DateTime.Now + ": Не удалось открыть файл. Пересохраните файл в формате .xlsx!\r\n");
                    }
                    else
                    {
                        radRichTextEditor1.Text += "Не удалось открыть файл. Это не файл MS Excel!" + "\n";
                        sb.Append(DateTime.Now + ": Не удалось открыть файл. Это не файл MS Excel!\r\n");
                    }
                }
                File.AppendAllText(@"C:\Sort-MIV\\log.txt", sb.ToString());
                sb.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }  //Открытие файла с реестром

        private void radButton4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }  //Выход

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                progressBar1.Text = "Отменено!";
                radRichTextEditor1.Text += "Отменено!\n";
                sb.Append("\r\n");
                sb.Append(DateTime.Now + ": Отменено!\r\n");
            }

            else if (!(e.Error == null))
            {
                progressBar1.Text = ("Ошибка: " + e.Error.Message);
                radRichTextEditor1.Text += "Ошибка: " + e.Error.Message + "\n";
                sb.Append("\r\n");
                sb.Append(DateTime.Now + ": Ошибка: " + e.Error.Message + "\r\n");
            }

            else
            {
                progressBar1.Text = "Выполнено!";
                radRichTextEditor1.Text += "Выполнено!\n";
                sb.Append("\r\n");
                sb.Append(DateTime.Now + ": Выполнено!\r\n");
            }
            radRichTextEditor1.Text += "Обработано записей в файле: " + addr.Rows.Count + "\n";
            sb.Append(DateTime.Now + ": Обработано записей в файле: " + addr.Rows.Count + "\r\n");


            File.AppendAllText(@"C:\Sort-MIV\log.txt", sb.ToString());
            sb.Clear();
        }  //Отмена
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value1 = e.ProgressPercentage;
            progressBar1.Text = (e.ProgressPercentage.ToString() + "%");
        }  //проценты

        private void radButton3_Click(object sender, EventArgs e)
        {
            try
            {
                ExcelFilePathFOL = "";
                sb.Append("\r\n");
                sb.Append("\r\n");
                sb.Append("------------------------ " + DateTime.Now + " ------------------------\r\n");

                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    radRichTextEditor1.Text += "\n";
                    ExcelFilePathFOL = fbd.SelectedPath;
                    radRichTextEditor1.Text += "Выбрана папка: " + ExcelFilePathFOL + "\n";
                    sb.Append(DateTime.Now + ": Выбрана папка: " + ExcelFilePathFOL + "\r\n");

                    ourfiles = Directory.GetFiles(ExcelFilePathFOL, "*.docx", SearchOption.AllDirectories);
                    radRichTextEditor1.Text += "Обнаружено файлов docx в папке: " + ourfiles.Count() + "\n";
                    sb.Append(DateTime.Now + ": Обнаружено файлов docx в папке: " + ourfiles.Count() + "\r\n");

                    if (ourfiles.Length > 0 || ExcelFilePathXLS != "")
                    {
                        radButton5.Enabled = true;
                        radRichTextEditor1.Text += "Нажмите кнопку Начать\n";
                    }
                    else
                    {
                        radButton5.Enabled = false;
                        radRichTextEditor1.Text += "\n";
                        radRichTextEditor1.Text += "Выберите непустую папку с файлами\n";
                    }
                }
                File.AppendAllText(@"C:\Sort-MIV\\log.txt", sb.ToString());
                sb.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
        }  //Открытие папки с файлами
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                cou = 0;

                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = "fio";
                addr.Columns.Add(column);

                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "addr";
                addr.Columns.Add(column);


                for (int y = 0; y < ourfiles.Length; y++)
                {
                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        int percentage = (y + 1) * 100 / ourfiles.Length;
                        OpenDocX(ourfiles[y], y);
                        worker.ReportProgress(percentage);
                    }
                    File.AppendAllText(@"C:\Sort-MIV\log.txt", sb.ToString());
                    sb.Clear();
                }
                AddDataToReestr(ExcelFilePathXLS);

                radRichTextEditor1.Text += "\n";
                radRichTextEditor1.Text += "Работа завершена\n";

                radButton5.Enabled = false;

                radRichTextEditor1.Text += "\n";
                radRichTextEditor1.Text += "Для повторной работы снова выберите файлы\n";

                addr.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void radButton5_Click(object sender, EventArgs e)
        {
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
        }

        private void radButton6_Click(object sender, EventArgs e)
        {
            if (bw.WorkerSupportsCancellation == true)
            {
                bw.CancelAsync();
            }
        }

        private void RadForm1_Load(object sender, EventArgs e)
        {
            radButton5.Enabled = false;
            radRichTextEditor1.Text += "1) Выберите папку с файлами для заполнения реестра"  + "\n";
            radRichTextEditor1.Text += "2) Выберите файл реестра в формате .xlsx"  + "\n";
            radRichTextEditor1.Text += "3) Нажмите кнопку Начать" + "\n";
        }
    }
}

