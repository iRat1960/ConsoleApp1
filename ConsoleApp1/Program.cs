using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static string[] quest = { "1. ФИО", "2. Дата рождения", "3. Любимый язык программирования", "4. Опыт программирования на указанном языке", "5. Мобильный телефон" };
        static string cPath = Directory.GetCurrentDirectory();

        static void Main(string[] args)
        {
            bool flProfile = false, flErr = false, flExit = false, flHelp = false;

            string cErr = "\nОшибка ввода команды! \nДля просмотра всех команд наберите -help ";
            string soob = "Выберите действие: ";
            string[] Profile = new string[5];
            string soobHelp = "\nСписок доступных команд: " +
                    "\n cmd: -new_profile - Заполнить новую анкету" +
                    "\n cmd: -statistics - Показать статистику всех заполненных анкет" +
                    "\n cmd: -save - Сохранить заполненную анкету" +
                    "\n cmd: -goto_question <Номер вопроса> - Вернуться к указанному вопросу(*)" +
                    "\n cmd: -goto_prev_question - Вернуться к предыдущему вопросу(*)" +
                    "\n cmd: -restart_profile - Заполнить анкету заново(*)" +
                    "\n cmd: -find <Имя файла анкеты> - Найти анкету и показать её данные" +
                    "\n cmd: -delete <Имя файла анкеты> - Удалить указанную анкету" +
                    "\n cmd: -list - Показать список названий файлов всех сохранённых анкет" +
                    "\n cmd: -list_today - Показать список названий файлов всех сохранённых анкет за сегодня" +
                    "\n cmd: -zip <Имя файла анкеты> <Путь для сохранения архива> - Запаковать указанную анкету в архив и сохранить его по указанному пути" +
                    "\n cmd: -help - Показать список доступных команд с описанием" +
                    "\n cmd: -exit - Выйти из приложения" +
                    "\n (*) - Команда доступна только при заполнении анкеты и вводится вместо ответа на любой вопрос";

            if (Directory.Exists(cPath))
            {
                string[] dirs = Directory.GetDirectories(cPath);
                if (dirs.Length == 0)
                {
                    Directory.CreateDirectory(cPath + "\\Анкеты");
                }
            }
            while (!flExit)
            {
                Console.WriteLine(soob);
                flHelp = false; flErr = false;
                soob = "Выберите действие: ";
                string name = Console.ReadLine();
                if (name.Length > 0 && name.Substring(0, 1) == "-")
                {
                    string[] cmds = name.Split(' ');
                    string cmd = cmds[0];
                    switch (cmd)
                    {
                        case "-exit":
                            flExit = true;
                            break;
                        case "-help":
                            flHelp = true;
                            break;
                        case "-zip":
                            if (cmds.Length > 2)
                                funcProfile(cmd.Substring(1, 1), cmds[1], cmds[2]);
                            else
                                flErr = true;
                            break;
                        case "-delete":
                        case "-find":
                            if (cmds.Length > 1)
                                funcProfile(cmd.Substring(1, 1), cmds[1]);
                            else
                                flErr = true;
                            break;
                        case "-save":
                            if (flProfile) SaveProfile(Profile);
                            flProfile = false;
                            break;
                        case "-new_profile":
                            flProfile = NewProfile(out Profile);
                            break;
                        case "-statistics":
                            Statistics();
                            break;
                        case "-list":
                        case "-list_today":
                            ListProfile(cmd.Length == 5);
                            break;
                        default:
                            flErr = true;
                            break;
                    }
                }
                else
                {
                    if (name.Length != 0)
                        flErr = true;
                }
                if (flErr)
                { soob = cErr; }
                if (flHelp)
                { soob = soobHelp; }
            } 
        }
        static void ListProfile(bool fl)
        {
            string dir = cPath + "\\Анкеты";
            DateTime now = DateTime.Now.Date;
            var files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            foreach (string file in files)
            {
                if (!fl)
                {
                    string[] lines = File.ReadAllLines(dir + "\\" + file, Encoding.Default);
                    var v = (from c in lines select c).LastOrDefault();
                    if (v.Substring(18, 10) == now.ToShortDateString())
                    {
                        Console.WriteLine("> " + file);
                    }
                }
                else
                    Console.WriteLine("> " + file);
            }
        }
        static void Statistics()
        {
            int y = DateTime.Now.Year;
            int j = 0, age = 0;
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "dd.mm.yyyy", developer = "";
            string dir = cPath + "\\Анкеты";
            string[] files = Directory.GetFiles(dir);
            int[] avgs = new int[files.Length];
            string[] langs = new string[files.Length];
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file, Encoding.Default);
                string cName = "";
                for (int i = 0; i < lines[i].Length; i++)
                {
                    if (i < 4)
                    {
                        int idx = lines[i].IndexOf(':');
                        int len = lines[i].Length - idx;
                        string val = lines[i].Substring(idx + 1, len - 1);
                        if (i == 0) cName = val;
                        if (i == 1)
                        {
                            DateTime res = DateTime.ParseExact(val.Substring(1, 10), format, provider);
                            avgs[j] = y - res.Year;
                        }
                        if (i == 2)
                        {
                            langs[j] = val;
                        }
                        if (i == 3)
                        {
                            int num = Int32.Parse(val, NumberStyles.Integer, provider);
                            age = Math.Max(num, age);
                            developer = age == num ? cName : developer;
                        }
                    }
                    else
                        break;
                }
                j++;
            }
            int avg = (int)avgs.Average();
            var lang = (from c in langs select c).GroupBy(g => g).OrderByDescending(o => o.Count()).FirstOrDefault().Key;
            Console.WriteLine("1. Средний возраст всех опрошенных: {0}", funcAges(avg));
            Console.WriteLine("2. Самый популярный язык программирования: {0}", lang);
            Console.WriteLine("3. Самый опытный программист: {0}", developer);
        }
        static string funcAges(int age)
        {
            string cRet = "";
            if (age < 1 || age > 100)
            {
                cRet = "расчитан неверный возраст";
            }
            if (age >= 10 & age <= 20)
                cRet = " лет";
            else
            {
                int last = age % 10;
                switch (last)
                {
                    case 1:
                        cRet = " год";
                        break;
                    case 2:
                    case 3:
                    case 4:
                        cRet = " годa";
                        break;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 0:
                        cRet = " лет";
                        break;
                }
            }
            return age.ToString() + cRet;
        }
        static void funcProfile(string sim, string prm1, string prm2 = "")
        {
            string dir = cPath + "\\Анкеты";
            bool fl = Directory.GetFiles(dir).Any(o => o.Contains(prm1));
            if (fl)
            {
                string file = dir + "\\" + prm1 + ".txt";
                switch (sim)
                {
                    case "z":
                        if (Directory.Exists(prm2))
                            Compress(file, prm2 + "\\" + prm1 + ".zip");
                        else
                            Console.WriteLine("Указан не существующий путь для сохранения архива");
                        break;
                    case "d":
                        File.Delete(file);
                        break;
                    case "f":
                        findProfile(file);
                        break;
                }
            }
            else
            {
                Console.WriteLine("Указанная анкета не найдена");
            }
        }
        static void Compress(string sourceFile, string compressedFile)
        {
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open))
            {
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream);
                        Console.WriteLine("Сжатие файла {0} завершено. Исходный размер: {1}  сжатый размер: {2}.",
                            sourceFile, sourceStream.Length.ToString(), targetStream.Length.ToString());
                    }
                }
            }
        }
        static void findProfile(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path, Encoding.Default))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static void SaveProfile(string[] data)
        {
            string text = data[0].Replace(' ', '_');
            string wp = cPath + "\\Анкеты\\" + text + ".txt";
            try
            {
                using (StreamWriter sw = new StreamWriter(wp, false, System.Text.Encoding.Default))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        text = quest[i] + ": " + data[i];
                        sw.WriteLine(text);
                    }
                    sw.WriteLine("");
                    sw.WriteLine("Анкета заполнена: " + DateTime.Today.ToString().Substring(0, 10));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static bool NewProfile(out string[] data)
        {
            ConsoleKeyInfo cki;
            CultureInfo provider = CultureInfo.InvariantCulture;
            bool flRet = false; int ct = 0; string format = "dd.mm.yyyy";
            string[] soft = { "PHP", "JavaScript", "C", "C++", "Java", "C#", "Python", "Ruby" };
            data = new string[] { "", "", "", "", "" };
            Console.WriteLine("\n======= Начало заполнения анкеты =======");
            while (true)
            {
                string text = quest[ct] + (ct == 1 ? " (формат ДД.ММ.ГГГГ)" : ct == 3 ? " (полных лет)" : "") + ":";
                Console.WriteLine(text);
                string str = Console.ReadLine();
                if (str.Length > 0)
                {
                    string strError = "";
                    if (str.Substring(0, 1) == "-")
                    {
                        string[] cmds = str.Split(' ');
                        string cmd = cmds[0];
                        switch (cmd)
                        {
                            case "-goto_question":
                                if (cmds.Length > 1)
                                {
                                    int step = Convert.ToInt32(cmds[1]);
                                    if (ct > 0 && ct < 6)
                                        ct = --step;
                                    else
                                        strError = "Номер вопроса должен быть в пределах от 1 до 5.";
                                }
                                else
                                {
                                    strError = "В команде не указан номер вопроса.";
                                }
                                break;
                            case "-goto_prev_question":
                                ct = ct == 0 ? 0 : --ct;
                                break;
                            case "-restart_profile":
                                ct = 0;
                                data = new string[5];
                                break;
                            default:
                                strError = "Не предусмотрена такая команда.";
                                break;
                        }
                    }
                    else
                    {
                        switch (ct)
                        {
                            case 0:
                                data[0] = str;
                                break;
                            case 1:
                                DateTime result;
                                try
                                {
                                    result = DateTime.ParseExact(str, format, provider);
                                    data[1] = result.ToString().Substring(0, 10);
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Ошибка! {0} не корректный формат даты.", str);
                                }
                                break;
                            case 2:
                                var results = Array.FindAll(soft, s => s.Equals(str));
                                if (results.Length > 0)
                                { data[2] = str; }
                                else
                                    strError = "Можно указать лишь слежующие варианты:" +
                                        "\nPHP, JavaScript, C, C++, Java, C#, Python, Ruby";
                                break;
                            case 3:
                                try
                                {
                                    int number = Int32.Parse(str, NumberStyles.Integer, provider);
                                    data[3] = number.ToString();
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Ошибка! Должно быть целочисленное значение полных лет.", str);
                                }
                                break;
                            case 4:
                                if (Validate(str))
                                { data[4] = "+7" + str; }
                                else
                                    strError = "Номер телефона следует вводить по шаблону:(xxx)xxx-xx-xx ";
                                break;
                        }
                        if (strError.Length == 0)
                        {
                            ct++;
                            if (ct > 4)
                            {
                                flRet = true;
                                Console.WriteLine("\n========== Анкета заполнена =============");
                                break;
                            }
                        }
                    }
                    if (strError.Length > 0)
                    {
                        Console.WriteLine("Ошибка! " + strError);
                    }
                }
                else
                {
                    Console.WriteLine("Все вопросы обязательны для заполнения. \nЕсли хотите прервать заполнение анкеты нажмите Escape (Esc) ");
                    cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Escape) break;
                }
            } 
            return flRet;
        }

        static bool Validate(string input)
        {
            string template = "(xxx)xxx-xx-xx";
            if (input.Length != template.Length) return false;
            for (int i = 0; i < template.Length; i++)
            {
                if (template[i] == 'x' && Char.IsDigit(input[i])) continue;
                if (template[i] != 'x' && template[i] == input[i]) continue;
                return false;
            }
            return true;
        }

    }
}
