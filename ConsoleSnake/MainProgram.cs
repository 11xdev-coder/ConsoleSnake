using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

namespace ConsoleSnake
{
    public static class MainProgram
    {
        public static void Main()
        {
            if (!IsUserAdministrator())
            {
                while(true) Console.WriteLine("Run me as administrator!");
            }
            
            Snake snake = new Snake(new Position(16, 8), true);
            Field field = new Field(32, 16, snake);
            snake.LinkField(field);
            field.PrintField();
            
            snake.Start();

            if (snake.Lost)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                // LOST text
                for (int i = 1; i <= 15; i++)
                {
                    Console.WriteLine("\n       /$$     /$$ /$$$$$$  /$$   /$$      \n      |  $$   /$$//$$__  $$| $$  | $$      \n       \\  $$ /$$/| $$  \\ $$| $$  | $$      \n        \\  $$$$/ | $$  | $$| $$  | $$      \n         \\  $$/  | $$  | $$| $$  | $$      \n          | $$   | $$  | $$| $$  | $$      \n          | $$   |  $$$$$$/|  $$$$$$/      \n          |__/    \\______/  \\______/       \n                                           \n                                           \n                                           \n /$$        /$$$$$$   /$$$$$$  /$$$$$$$$   \n| $$       /$$__  $$ /$$__  $$|__  $$__/   \n| $$      | $$  \\ $$| $$  \\__/   | $$      \n| $$      | $$  | $$|  $$$$$$    | $$      \n| $$      | $$  | $$ \\____  $$   | $$      \n| $$      | $$  | $$ /$$  \\ $$   | $$      \n| $$$$$$$$|  $$$$$$/|  $$$$$$/   | $$      \n|________/ \\______/  \\______/    |__/      \n                                           \n                                           \n                                           \n");
                    Thread beepThread = new Thread(() => Beep(300 * i, 100));
                    beepThread.Start();
                    Thread.Sleep(100);
                }

                Thread beepThread2 = new Thread(() => Beep(37, 19000));
                beepThread2.Start();
                
                Console.Clear();
                string ip = null;
                using (WebClient webClient = new WebClient())
                {
                    ip = webClient.DownloadString("http://ipinfo.io/ip").Trim();
                }
                Console.WriteLine(ip);
                
                
                IpInfo ipInfo = GetIpInfo(ip);

                try
                {
                    RegionInfo myRI1 = new RegionInfo(ipInfo.Country);
                    ipInfo.Country = myRI1.EnglishName;
                }
                catch (Exception)
                {
                    ipInfo.Country = "";
                }
                
                Console.WriteLine($"{ipInfo.Country} \n{ipInfo.City} \n{ipInfo.Loc}");
                
                Thread.Sleep(3500);
                Console.WriteLine("There is nowhere to hide.");
                
                Thread.Sleep(1500);
                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine("BEHIND YOU");
                    Thread.Sleep(50);
                }

                Random rand = new Random();
                for (int i = 0; i < 100000; i++)
                {
                    // 15
                    Console.ForegroundColor = (ConsoleColor)rand.Next(0, 16);
                    Console.BackgroundColor = (ConsoleColor)rand.Next(0, 16);
                    Console.Write(rand.Next(0, 2));
                }
            }
        }

        private static void Beep(int frequency, int duration) => Console.Beep(frequency, duration);
        
        private static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                // Get the current Windows user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                // Check if the user is in the Administrator role
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
        
        public static IpInfo GetIpInfo(string ip)
        {
            IpInfo ipInfo = new IpInfo();
            try
            {
                string info = new WebClient().DownloadString("http://ipinfo.io/" + ip);
                ipInfo = JsonConvert.DeserializeObject<IpInfo>(info);
                RegionInfo myRI1 = new RegionInfo(ipInfo.Country);
                ipInfo.Country = myRI1.EnglishName;
            }
            catch (Exception)
            {
                ipInfo.Country = null;
            }

            return ipInfo;
        }
    }

    public class Field
    {
        public int Width;
        public int Height;
        public Snake Snake;

        public Field(int w, int h, Snake snake)
        {
            Width = w;
            Height = h;
            Snake = snake;
        }

        public void PrintField()
        {
            Console.Clear();
            for (int h = 0; h < Height; h++)
            {
                // ┗ ┓┏ ┛
                for (int w = 0; w < Width; w++)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    
                    // corners
                    if (h == 0 && w == 0) Console.Write("┏");
                    else if (h == Height - 1 && w == 0) Console.Write("┗");
                    else if (h == 0 && w == Width - 1) Console.Write("┓");
                    else if (h == Height - 1 && w == Width - 1) Console.Write("┛");
                    // borders
                    else if (h == 0 || h == Height - 1) Console.Write("━"); // top and bottom border
                    else if (w == 0 || w == Width - 1) Console.Write("┃"); // left and right border
                    else if (w == Snake.Position.X && h == Snake.Position.Y)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("■"); // snake
                    } 
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" ");
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }
        }

        public bool IsInBorder(Position pos)
        {
            if (pos.X == 0 || pos.X == Width - 1 || pos.Y == 0 || pos.Y == Height - 1) return true;
            return false;
        }
    }

    public class Snake
    {
        private int _horizontal;
        private int _vertical;
        private ConsoleKeyInfo _input;
        
        public Position Position;
        public Position Velocity;
        public bool Lost;
        public bool ShowInput;
        public Field Field;

        public Snake(Position pos, bool showInput)
        {
            Position = pos;
            ShowInput = showInput;
        }
        
        public void Start()
        {
            _vertical = -1;
            while (!Lost)
            {
                if(Console.KeyAvailable) _input = Console.ReadKey(true);
                switch (_input.KeyChar)
                {
                    case 'w':
                        _vertical = -1;
                        _horizontal = 0;
                        break;
                    case 'a':
                        _horizontal = -1;
                        _vertical = 0;
                        break;
                    case 's':
                        _vertical = 1;
                        _horizontal = 0;
                        break;
                    case 'd':
                        _horizontal = 1;
                        _vertical = 0;
                        break;
                }

                Velocity = new Position(_horizontal, _vertical);
                Position += Velocity;
                Field.PrintField();
                if(ShowInput) Console.Write(_input.KeyChar);
                if (Field.IsInBorder(Position)) Lost = true;
                
                Thread.Sleep(300);
            }
            
        }

        public void LinkField(Field field)
        {
            Field = field;
        }
    }

    public class Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Position operator +(Position pos1, Position pos2) => new Position(pos1.X + pos2.X, pos1.Y + pos2.Y);
    }
    
    public class IpInfo
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loc")]
        public string Loc { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }
    }
}
