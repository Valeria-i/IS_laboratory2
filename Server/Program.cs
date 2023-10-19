using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using NLog;


class AsyncUdpServer
{
    private const int Port = 8001;
    private const string FilePath = "D:\\3_course\\IS_architecture\\IS_lab2.0\\Server\\data.csv";
    private static Logger logger = LogManager.GetCurrentClassLogger();

    static async Task Main()
    {
        UdpClient udpServer = new UdpClient(Port);
        Console.WriteLine("Сервер запущен и ждет подключения...");
      
        while (true)
        {
            UdpReceiveResult result = await udpServer.ReceiveAsync();
            string request = Encoding.UTF8.GetString(result.Buffer);
            string response = "";

            if (request == "getall")
            {
                response = GetAllRecords();
                logger.Info("Вывод всех записей"); 
            }
            else if (request.StartsWith("get"))
            {
                 int recordNumber; 
                 if( int.TryParse(request.Substring(3), out recordNumber))
                 { 
                    response = GetRecordByNumber(recordNumber);
                    logger.Info("Вывод одной записи");

                 }
                                  
            }
            else if (request.StartsWith("delete"))
            {
                int recordNumber;
                if (int.TryParse(request.Substring(6), out recordNumber))
                {
                    bool isDeleted = DeleteRecordByNumber(recordNumber);
                     if (isDeleted)
                     {
                    response = "Запись успешно удалена.";
                    logger.Info("Запись успешно удалена");
                     }
                    else
                    {
                    response = "Запись не найдена.";
                    logger.Info("Запись не найдена");
                    }

                }
               

            }
            else if (request.StartsWith("add"))
            {
                string data = request.Substring(3);
                bool isAdded = AddRecord(data);

                if (isAdded)
                {
                    response = "Запись успешно добавлена.";
                    logger.Info("Запись успешно удалена");
                }
                else
                {
                    response = "Ошибка при добавлении записи.";
                    logger.Info("Ошибка");
                }
            }

            byte[] responseData = Encoding.UTF8.GetBytes(response);
            await udpServer.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);
        }
    }

    private static string GetAllRecords()
    {
        string[] lines = File.ReadAllLines(FilePath);
        return string.Join(Environment.NewLine, lines);
    }

    private static string GetRecordByNumber(int number)
    {
        string[] lines = File.ReadAllLines(FilePath);

        if (number >= 0 && number < lines.Length)
        {
            return lines[number];
        }

        return "Запись не найдена.";
    }

    private static bool DeleteRecordByNumber(int number)
    {
        string[] lines = File.ReadAllLines(FilePath);

        if (number >= 0 && number < lines.Length)
        {
            var tempList = new List<string>(lines);
            tempList.RemoveAt(number);
            File.WriteAllLines(FilePath, tempList);
            return true;
        }

        return false;
    }

    private static bool AddRecord(string data)
    {
        string[] fields = data.Split(',');

        if (fields.Length == 5 &&
            fields[0] is string &&
            int.TryParse(fields[1], out _) &&
            fields[2] is string &&
            fields[3] is string &&
            bool.TryParse(fields[4], out _))
        {
            File.AppendAllText(FilePath, $"{Environment.NewLine}{data}");
            return true;
        }
        else
        {
            return false;
        }
    }
}

