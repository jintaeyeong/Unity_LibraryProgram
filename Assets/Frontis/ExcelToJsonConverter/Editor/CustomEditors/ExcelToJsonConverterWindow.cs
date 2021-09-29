using UnityEngine;
using UnityEditor;
using OfficeOpenXml;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Text;

public class ExcelToJsonConverterWindow : EditorWindow
{

    public static string kExcelToJsonConverterInputPathPrefsName = "ExcelToJson.InputPath";
    public static string kExcelToJsonConverterOuputPathPrefsName = "ExcelToJson.OutputPath";

    private string inputPath;
    private string outputPath;



    public void OnEnable()
    {
        // 이전 경로를 읽어 옵니다.
        //
        inputPath  = EditorPrefs.GetString(kExcelToJsonConverterInputPathPrefsName, Application.dataPath);
        outputPath = EditorPrefs.GetString(kExcelToJsonConverterOuputPathPrefsName, Application.dataPath);
    }


    public void OnDisable()
    {
        // 현재 경로를 저장합니다.
        //
        EditorPrefs.SetString(kExcelToJsonConverterInputPathPrefsName, inputPath);
        EditorPrefs.SetString(kExcelToJsonConverterOuputPathPrefsName, outputPath);
    }


    [MenuItem("Tools/Excel To Json Converter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ExcelToJsonConverterWindow), true, "Excel To Json Converter", true);
    }


    /**
     * @brief 전달받은 경로에 있는 엑셀 파일을 Json 파일로 변환합니다.
     */
    private void ConvertExcelFileToJson(string inPath, string outPath)
    {
        DataSet dataSet = GetExcelDataSet(inPath);

        if (dataSet == null)
        {
            Debug.LogError("Excel To Json Converter: Failed to process file: " + inPath);
            return;
        }

        string spreadSheetJson = "";

        // 각 스프레드 시트를 엑셀 파일로 처리합니다.
        //
        for (int i = 0; i < dataSet.Tables.Count; i++)
        {
            spreadSheetJson = GetSpreadSheetJson(dataSet, dataSet.Tables[i].TableName);

            if (string.IsNullOrEmpty(spreadSheetJson))
            {
                Debug.LogError("Excel To Json Converter: Failed to covert Spreadsheet '" + dataSet.Tables[i].TableName + "' to json.");

                return;
            }
            else
            {
                // 파일 이름은 Sheet 이름으로 하고, 공백은 제거합니다.
                //
                string fileName = dataSet.Tables[i].TableName.Replace(" ", string.Empty);

                WriteTextToFile(spreadSheetJson, outputPath + "/" + fileName + ".json");

                Debug.Log("Excel To Json Converter: " + dataSet.Tables[i].TableName + " successfully written to file.");
            }
        }
    }


    /**
     * @brief 전달받은 경로에서 Excel 데이터를 가져옵니다.
     */
    private DataSet GetExcelDataSet(string filePath)
    {
        FileInfo file   = new FileInfo(filePath);
        ExcelPackage ep = new ExcelPackage(file);
        DataSet dataSet = new DataSet();

        for (int i = 1; i <= ep.Workbook.Worksheets.Count; i++)
        {
            ExcelWorksheet sheet  = ep.Workbook.Worksheets[i];
            ExcelTable excelTable = new ExcelTable(sheet);

            DataTable dataTable = GetExcelSheetData(excelTable);

            if (dataTable != null)
            {                
                dataSet.Tables.Add(dataTable);  // DataSet에 Sheet의 Excel 데이터를 추가합니다.
            }
        }

        return dataSet;
    }


    /**
     * @brief 전달받은 경로에 존재하는 모든 파일 이름을 알려줍니다.
     */
    private List<string> GetExcelFileNamesInDirectory(string directory)
    {
        string[] directoryFiles = Directory.GetFiles(directory);
        List<string> excelFiles = new List<string>();

        // 2개의 Excel 파일 형식(xls 및 xlsx)과 일치하는 파일이 있는지 확인하기 위해 정규식을 사용합니다.
        // 확장자 .meta, ~$(Excel에서 만든 temp파일)로 된 파일은 무시합니다.
        //
        Regex excelRegex = new Regex(@"^((?!(~\$)).*\.(xlsx|xls$))$");

        for (int i = 0; i < directoryFiles.Length; i++)
        {
            string fileName = directoryFiles[i].Substring(directoryFiles[i].LastIndexOf('/') + 1);

            if (excelRegex.IsMatch(fileName))
            {
                excelFiles.Add(directoryFiles[i]);
            }
        }

        return excelFiles;
    }


    /**
     * @brief 전달받은 Sheet에서 Excel 데이터를 돌려줍니다.
     */
    private DataTable GetExcelSheetData(ExcelTable excelTable)
    {
        //Debug.Log("excelTable.TableName: " + excelTable.TableName);

        if(excelTable == null)
        {
            return null;
        }
        
        DataTable table = new DataTable(excelTable.TableName);  // Sheet 이름으로 테이블을 만듭니다.
        table.Clear();


        int rowCount = 1;
        int readColumnLenth = GetColumnLength(excelTable);

        bool rowIsEmpty = false;  // 빈 행인지 확인하기 위해 사용

        //Debug.LogFormat("NumberOfColumns: {0},  NumberOfRows: {1}", excelTable.NumberOfColumns, excelTable.NumberOfRows);

        // rows 및 columns을 읽습니다.
        //
        while (rowCount <= excelTable.NumberOfRows)
        {
            DataRow row = table.NewRow();
            rowIsEmpty  = true;

            for (int colCount = 1; colCount <= readColumnLenth; colCount++)
            {
                string value = excelTable.GetValue(rowCount, colCount) as string;
                
                if (rowCount == 1)
                {
                    table.Columns.Add(value);  // 첫번째 row이면 값을 column에 추가합니다.
                }
                else
                {
                    row[table.Columns[colCount - 1]] = value;  // 그렇지 않으면 row에 추가합니다.
                }

                // 한 column이라도 값이 있다면 빈 행이 아닌것으로 변경합니다.
                //
                if (!string.IsNullOrEmpty(value))
                {
                    rowIsEmpty = false;
                }
            }

            // 첫 행이 아니고, 행 전체가 비어있지 않다면 테이블에 추가합니다.
            // 
            if (rowCount != 1 && !rowIsEmpty)
            {
                table.Rows.Add(row);
            }
            rowCount++;
        }
        return table;
    }


    /// <summary>
    /// Json으로 변환할 Column의 개수를 알려줍니다.
    /// </summary>
    /// <param name="excelTable"></param>
    /// <returns></returns>
    private static int GetColumnLength(ExcelTable excelTable)
    {
        int firstRow = 1;
        int colCount = 0;

        for (int i = 1; i <= excelTable.NumberOfColumns; i++)
        {
            string value = excelTable.GetValue(firstRow, i) as string;

            // 한 column이라도 값이 있다면 빈 행이 아닌것으로 변경합니다.
            //
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value) && value != Environment.NewLine)
            {
                colCount++;
            }
        }

        return colCount;
    }

    /**
     * @brief 전달받은 DataSet에서 함께 전달받은 Sheet 이름에 해당하는 Excel 데이터를 json 데이터 변환해서 돌려줍니다.
     */
    private string GetSpreadSheetJson(DataSet excelDataSet, string sheetName)
    {
        //Debug.LogFormat("GetSpreadSheetJson - sheetName: {0}", sheetName);

        // 전달받은 Sheet 이름의 Excel 데이터를 가져옵니다.
        //
        DataTable dataTable = excelDataSet.Tables[sheetName];

        DataTable[] dataTableArray = GetDataTableArray(dataTable);


        // DataTable을 Json 문자열로 직렬화해서 돌려줍니다.
        //
        if (dataTableArray == null || dataTableArray.Length == 0)
        {
            RemoveEmptyColumns(ref dataTable);
            RemoveSpecialColumns(ref dataTable);

            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.Indented);


            return jsonString;
        }
        else
        {
            for (int i = 0; i < dataTableArray.Length; i++)
            {
                RemoveEmptyColumns(ref dataTableArray[i]);
                RemoveSpecialColumns(ref dataTableArray[i]);
            }

            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dataTableArray, Newtonsoft.Json.Formatting.Indented);

            jsonString = StringChangeJsonType(jsonString);

            return jsonString;

        }
    }

    /// <summary>
    /// 테이블에서 가져온 String 데이터를 원하는 Json 형식으로 만들어줌
    /// </summary>
    /// <param name="String"></param>
    /// <returns></returns>
    private static string StringChangeJsonType(string dataString)
    { 
        StringBuilder sb = new StringBuilder();

        //배열로 쪼개서 넣음
        string[] jsonStringArray = dataString.Split(new string[] { "]," }, StringSplitOptions.None);

        //원하는 데이터 형식에 맞게 변경
        for (int i = 0; i < jsonStringArray.Length; i++)
        {
            jsonStringArray[i] = jsonStringArray[i].Replace("[", "");
            jsonStringArray[i] = jsonStringArray[i].Replace("]", "");

            jsonStringArray[i] = "{\n \"Book\"" + ": [" + jsonStringArray[i] + "]\n}";

            //문자열 합침
            sb.AppendLine(jsonStringArray[i]);

            //,, 값 추가
            if (!(i == jsonStringArray.Length - 1))
            {
                sb.AppendLine(",,");
            }
        }

        return sb.ToString();
    }


    /// <summary>
    /// 빈 column을 제거합니다.
    /// </summary>
    /// <param name="table"></param>
    private static void RemoveEmptyColumns(ref DataTable table)
    {
        for (int col = table.Columns.Count - 1; col >= 0; col--)
        {
            bool removeColumn = true;

            foreach (DataRow row in table.Rows)
            {
                //Debug.LogFormat($"row[{col}]: {row[col]}");

                if (!row.IsNull(col))
                {
                    removeColumn = false;
                    break;
                }
            }

            if (removeColumn)
            {
                table.Columns.RemoveAt(col);
            }
        }
    }

    /// <summary>
    /// 아래 특수문자가 포함된 컬럼의 내용은 삭제합니다.
    /// (엑셀에서 확인용으로 사용하기 위함입니다.)
    /// </summary>
    /// <param name="table"></param>
    private static void RemoveSpecialColumns(ref DataTable table)
    {
        Regex columnNameRegex = new Regex(@"^~.*$");

        for (int i = table.Columns.Count - 1; i >= 0; i--)
        {
            //Debug.LogFormat($"table.Columns[{i}].ColumnName: {table.Columns[i].ColumnName}");

            if (columnNameRegex.IsMatch(table.Columns[i].ColumnName))
            {
                //Debug.LogFormat("GetSpreadSheetJson - table.Columns.RemoveAt({0})", i);

                table.Columns.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 배열로 만들지 않는 dataTable이라면 배열로 만들지 않고 Null이나 0으로 반환
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    private static DataTable[] GetDataTableArray(DataTable dataTable)
    {
        int totalCount = GetDataTableToJsonArrayLength(dataTable);
        int startIndex = 0;
        int endIndex = 0;

        List<DataTable> dataTableList = new List<DataTable>();

        for (int i = 0; i < totalCount; i++)
        {
            if (GetDataTableIndexOf(dataTable, out startIndex, out endIndex, i))
            {
                DataTable table = dataTable.Copy();
                dataTableList.Add(SubDataTable(table, startIndex, endIndex));
            }
        }

        return dataTableList.ToArray();
    }

    /// <summary>
    /// DataTable에서 Json 배열로 만들어야하는 개수를 알려줍니다.
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    private static int GetDataTableToJsonArrayLength(DataTable dataTable)
    {
        int count = 0;
        bool isStart = false;

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {

            if (IsCheckStartOrEndRow(dataTable.Rows[i]))
            {
                if (!isStart)
                {
                    isStart = true;
                }
                else
                {
                    count++;
                    isStart = false;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// 현재 행이 데이터의 시작 또는 끝인지 알려줍니다.
    /// </summary>
    /// <returns></returns>
    private static bool IsCheckStartOrEndRow(DataRow row)
    {
        if (!string.IsNullOrEmpty(row.ItemArray[1].ToString()) && !string.IsNullOrWhiteSpace(row.ItemArray[1].ToString()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 전달받은 DataTable에서 전달받은 arrayIndex에 해당하는 시작 인덱스와 끝 인덱스를 찾아줍니다.
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <param name="arrayIndex"></param>
    private static bool GetDataTableIndexOf(DataTable dataTable, out int startIndex, out int endIndex, int arrayIndex)
    {
        int count = 0;
        bool isStart = false;

        startIndex = 0;
        endIndex = 0;

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            if (IsCheckStartOrEndRow(dataTable.Rows[i]))
            {
                if (!isStart)
                {
                    isStart = true;

                    if (count == arrayIndex)
                    {
                        startIndex = i;
                    }
                }
                else
                {
                    isStart = false;

                    if (count == arrayIndex)
                    {
                        endIndex = i;
                        break;
                    }

                    count++;
                }
            }
        }

        if (endIndex > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 지정한 범위의 데이터만 돌려줍니다.
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <returns></returns>
    private static DataTable SubDataTable(DataTable dataTable, int startIndex, int endIndex)
    {
        int count = endIndex - startIndex;

        for (int i = 0; i < startIndex; i++)
        {
            dataTable.Rows.RemoveAt(0);
        }

        for (int i = dataTable.Rows.Count - 1; i > count; i--)
        {
            dataTable.Rows.RemoveAt(i);
        }

        return dataTable;
    }


    /**
     * @brief 전달받은 엑셀 파일들을 읽습니다.
     */
    private void LoadExcel(List<string> excelFiles)
    {
        for (int i = 0; i < excelFiles.Count; i++)
        {
            ConvertExcelFileToJson(excelFiles[i], outputPath);
        }

    }


    /**
     * @brief 전달받은 텍스트를 함께 전달받은 경로에 파일로 생성하거나 덮어씁니다.
     */
    private void WriteTextToFile(string text, string filePath)
    {
        File.WriteAllText(filePath, text);
    }


    void OnGUI()
    {
        // Excel 파일을 읽어올 경로
        //
        GUILayout.BeginHorizontal();

        GUIContent inputFolderContent = new GUIContent("Input Folder", "Select the folder where the excel files to be processed are located.");
        EditorGUIUtility.labelWidth   = 120.0f;
        EditorGUILayout.TextField(inputFolderContent, inputPath, GUILayout.MinWidth(120), GUILayout.MaxWidth(500));

        if (GUILayout.Button(new GUIContent("Select Folder"), GUILayout.MinWidth(80), GUILayout.MaxWidth(100)))
        {
            inputPath = EditorUtility.OpenFolderPanel("Select Folder with Excel Files", inputPath, Application.dataPath);
        }

        GUILayout.EndHorizontal();

        // 변환된 Json 파일을 저장할 경로
        //
        GUILayout.BeginHorizontal();

        GUIContent outputFolderContent = new GUIContent("Output Folder", "Select the folder where the converted json files should be saved.");
        EditorGUILayout.TextField(outputFolderContent, outputPath, GUILayout.MinWidth(120), GUILayout.MaxWidth(500));

        if (GUILayout.Button(new GUIContent("Select Folder"), GUILayout.MinWidth(80), GUILayout.MaxWidth(100)))
        {
            outputPath = EditorUtility.OpenFolderPanel("Select Folder to save json files", outputPath, Application.dataPath);
        }

        GUILayout.EndHorizontal();

        if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
        {
            GUI.enabled = false;
        }

        GUILayout.BeginArea(new Rect((Screen.width / 2) - (200 / 2), (Screen.height / 2) - (25 / 2), 200, 25));

        if (GUILayout.Button("Convert Excel Files"))
        {
            List<string> excelFiles = GetExcelFileNamesInDirectory(inputPath);

            LoadExcel(excelFiles);
        }

        GUILayout.EndArea();

        GUI.enabled = true;
    }


}
