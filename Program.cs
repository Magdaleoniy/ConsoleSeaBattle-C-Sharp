using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;


// Проект Степанова Юрия Сергеевича
// Версия проекта: 0.0.3

// В этой версии использованы конструкции:
// Абстракных классов и методов (см. Game.Bot), виртуальных методов (Game.EasyBot),
// наследования (EasyBot и MediumBot), переопределения и интерфейсов (IShuffle)
// Добавлен бот средней сложности (находится в разработке), который умеет поражать
// клетки рядом с подбитой пабулой, чтобы добивать корабль после попадания

namespace SeaBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в Морской бой");
            Map my = new Map();

            my.InitializeMap();
            my.printMap();

            while (true)
            {
                Console.WriteLine("Напишите start, чтобы начать игру"
                    + "\n" + "Напишите clean, чтобы отчистить поле"
                    + "\n" + "Напишите print, чтобы показать ваше поле"
                    + "\n" + "Напишите end, чтобы завершить программу" + "\n");
                String s = Console.ReadLine();
                if (s.Equals("start"))
                {
                    Game session = new Game(my);
                    session.createMap();
                }
                else if (s.Equals("clean"))
                {
                    my.InitializeMap();
                    my.printMap();
                    Console.WriteLine();
                }
                else if (s.Equals("print"))
                    my.printMap();
                else if (s.Equals("end"))
                    break;
                else
                    Console.WriteLine("Вы ввели неверную команду :(" + "\n"
                        + "Попробуйте ещё раз");

            }


        }
        public class Game
        {
            public Map my;
            public Game(Map my)
            {
                this.my = my;
            }

            //Интерфейс, реализующий метод случайного перемешивания списка
            public interface IShuffle
            {
                void shuffle(List<int> x);
            }

            //Базовый класс для компьютеров-противников
            public abstract class Bot
            {
                protected int step = 1;
                public Map bot = new Map(), enemy;
                //Все боты обязательно должны вести огонь, но по-разному
                //Для этого используем абстрактный метод в базовом классе,
                //который переопределим у каждого бота 
                public abstract void shoot();
                public void initializeMap()
                {
                    bot.InitializeMap();
                    bot.InitializeClone();
                    enemy.InitializeClone();
                    bot.createRandomMap();
                }
            }

            //Бот легкой сложности, который поражает поля случайным образом
            class EasyBot : Bot
            {
                public string type;
                public EasyBot(Map enemy)
                {
                    this.enemy = enemy;
                    initializeMap();
                }
                public EasyBot(Map enemy, string type)
                {
                    this.enemy = enemy;
                    this.type = type;
                    initializeMap();
                    present();
                }

                //Обязательное переопределение абстрактного метода
                public override void shoot()
                {
                    //Этот цикл для выстрелов компьютера
                    while (true)
                    {
                        Console.WriteLine(step + "-ый ход компьютера");
                        Random r = new Random();
                        int x = r.Next(9) + 1;
                        int y = r.Next(9) + 1;
                        if (bot.annihilation(enemy, x, y) == false)
                            break;
                        step++;
                    }
                }

                //Виртуальный метод, который предсавляет бота пользователю
                public virtual void present()
                {
                    Console.WriteLine("\n" + "Приветствую, я " + type + " противник. Приятной игры" + "\n");
                }

            }

            //Клас MdediumBot расширяет класс EasyBot и реализует интерфейс IShuffle
            class MediumBot : EasyBot, IShuffle
            {
                bool action = false;
                int x = -1, y = -1;
                List<int> forShoot = new List<int>();
                Random r = new Random();

                //Конструктор, который обращается к конструктору родителя
                public MediumBot(Map enemy) : base(enemy)
                {
                    type = "средний";
                    present();
                }

                public override void shoot()
                {
                    //Этот цикл для выстрелов компьютера
                    while (true)
                    {
                        //Если бот попал по кораблю, он будет стрелять по соседним клеткам,
                        //чтобы его добить
                        Console.WriteLine(step + "-ый ход компьютера");
                        if ((action || forShoot.Count < 4) && x != -1)
                        {
                            if (forShoot.Count < 4)
                            {
                                forShoot.AddRange(new int[] { 1, 2, 3, 4 });
                                shuffle(forShoot);
                            }
                            if (forShoot[0] == 1)
                            {
                                if (!enemy.checkShip(x + 1, y))
                                {
                                    action = bot.annihilation(enemy, x + 1, y);
                                    if (!action)
                                        forShoot.Remove(forShoot[0]);
                                    else
                                        x += 1;
                                }
                                else
                                    forShoot.Remove(forShoot[0]);

                            }
                            if (forShoot[0] == 2)
                            {
                                if (!enemy.checkShip(x - 1, y))
                                {
                                    action = bot.annihilation(enemy, x - 1, y);
                                    if (!action)
                                        forShoot.Remove(forShoot[0]);
                                    else
                                        x -= 1;
                                }
                                else
                                    forShoot.Remove(forShoot[0]);

                            }
                            if (forShoot[0] == 3)
                            {
                                if (!enemy.checkShip(x, y + 1))
                                {
                                    action = bot.annihilation(enemy, x, y + 1);
                                    if (!action)
                                        forShoot.Remove(forShoot[0]);
                                    else
                                        y += 1;
                                }
                                else
                                    forShoot.Remove(forShoot[0]);

                            }
                            if (forShoot[0] == 4)
                            {
                                if (!enemy.checkShip(x, y - 1))
                                {
                                    action = bot.annihilation(enemy, x, y - 1);
                                    if (!action)
                                        forShoot.Remove(forShoot[0]);
                                    else
                                        y -= 1;
                                }
                                else
                                    forShoot.Remove(forShoot[0]);

                            }
                        }
                        else
                        {
                            forShoot.Clear();
                            x = r.Next(9) + 1;
                            y = r.Next(9) + 1;
                            action = bot.annihilation(enemy, x, y);
                        }

                        if (action == false)
                            break;
                        step++;
                    }
                }

                //Переопределение виртуального метода с добавлением ещё одной строчки
                public override void present()
                {
                    base.present();
                    Console.WriteLine("Но придется постараться ;)" + "\n");
                }

                //Обязательная реализация члена интерфейса IShuffle
                public void shuffle(List<int> x)
                {
                    Random r = new Random();
                    for (int i = x.Count - 1; i > 0; i--)
                    {
                        int j = r.Next(i + 1);
                        int t = x[i];
                        x[i] = x[j];
                        x[j] = t;
                    }
                }
            }

            public void createMap()
            {
                my.InitializeMap();
                Console.WriteLine("Если вы хотите сами установить корабли, введите: YES");
                Console.WriteLine("Если вы хотите расставить корабли случайно, введите: NO");
                string s = Console.ReadLine();
                if (s.Equals("YES"))
                {
                    Console.WriteLine("Введите координаты первого однопалубника:");
                    my.setShip(1, Console.ReadLine());
                    Console.WriteLine("Введите координаты второго однопалубника:");
                    my.setShip(1, Console.ReadLine());
                    Console.WriteLine("Введите координаты третьего однопалубника:");
                    my.setShip(1, Console.ReadLine());
                    Console.WriteLine("Введите координаты четвертого однопалубника:");
                    my.setShip(1, Console.ReadLine());

                    Console.WriteLine("Координаты многопалубных кораблей вводите через пробел в формате:"
                        + "\n" + "X1 X2"
                        + "\n" + "То есть: координата начала и координата конца");

                    Console.WriteLine("Введите координаты первого двупалубника:");
                    my.setShip(2, Console.ReadLine());
                    Console.WriteLine("Введите координаты второго двупалубника:");
                    my.setShip(2, Console.ReadLine());
                    Console.WriteLine("Введите координаты третьего двупалубника:");
                    my.setShip(2, Console.ReadLine());

                    Console.WriteLine("Введите координаты первого трехпалубника:");
                    my.setShip(3, Console.ReadLine());
                    Console.WriteLine("Введите координаты второго трехпалубника:");
                    my.setShip(3, Console.ReadLine());

                    Console.WriteLine("Введите координаты четырехпалубника:");
                    my.setShip(4, Console.ReadLine());

                    Console.WriteLine("Спасибо, что установили корабли :)");
                    Console.WriteLine("\n" + "Игра началась!" + "\n");
                    start();
                }
                else
                {
                    my.createRandomMap();
                    my.printMap();
                    Console.WriteLine("Ваши корабли расставлены случайным образом");
                    start();
                }
            }

            public void start()
            {
                Console.WriteLine("Выбирете желаемую сложность: 1, 2");
                string dif = Console.ReadLine();
                if (dif.Equals("1"))
                    battle(new EasyBot(my, "легкий"));
                else if (dif.Equals("2"))
                    battle(new MediumBot(my));
                else
                {
                    Console.WriteLine("Такой сложности не существует :(");
                    Console.WriteLine("Попробуйте ещё раз:");
                    start();
                }
            }
            public void battle(Bot enemy)
            {
                try
                {
                    bool end = false;
                    //Игра будет продолжаться, пока один из игроков не уничтожит все корабли противника
                    while (!end)
                    {
                        //Этот цикл для выстрелов игрока
                        while (true)
                        {
                            Console.WriteLine("(end - завершить игру, print - показать вашу карту, printe - показать карту соперника)" + "\n"
                                + "Введите координаты поля, которое хотите поразить:");
                            string s = Console.ReadLine();
                            if (s.Equals("print"))
                                my.printMap();
                            else if (s.Equals("printe"))
                                enemy.bot.clone.printMap();
                            else if (s.Equals("end"))
                            {
                                end = true;
                                break;
                            }
                            else
                            {
                                //С помощью метода convert класса Map переводим 
                                //строчку с координатами в массив с числами
                                List<int> coordinate = my.convert(s);
                                int x = coordinate[0];
                                int y = coordinate[1];
                                if (!my.annihilation(enemy.bot, x, y))
                                {
                                    enemy.bot.clone.printMap();
                                    break;
                                }
                                Console.WriteLine("\n" + "Вы подбили корабль!" + "\n");
                                enemy.bot.clone.printMap();
                            }
                        }

                        if (end)
                            break;

                        Console.WriteLine("\n" + "Вы промахнулись" + "\n");

                        //Этот цикл для выстрелов компьютера
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        enemy.shoot();

                        Console.WriteLine("\n" + "Ваше поле после атак противника: " + "\n");
                        my.printMap();

                        if (my.checkMap(my))
                        {
                            Console.WriteLine("Вы проиграли :(");
                            end = !end;
                        }
                        if (enemy.bot.checkMap(enemy.bot))
                        {
                            Console.WriteLine("Вы победили :)");
                            end = !end;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Вы ввели координаты не по правилам :( " + "\n"
                        + "Попробуйте ещё раз" + "\n");
                    this.battle(enemy);
                }
            }

        }
    }

    //Тип "пара", который хранит в себе две переменные
    public class Pair
    {
        public int x;
        public int y;
        public Pair(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }


    //Класс Map описывает поле "Морского боя" и
    //предоставляет методы для работы с ним
    public class Map
    {
        //Карта-клон, на которой мы не будем видеть корабли
        //соперника до тех пор, пока не подобьем их
        public Map clone;
        private static int size = 10;
        private static int plusSize = size + 2;
        //Создадимw двумерный массив для игрового поля
        private int[,] map = new int[plusSize, plusSize];
        //И строчку с буквенными значениями координат
        private static string nameMap = "ABCDEFGHIJ";

        public void InitializeMap()
        {
            for (int i = 0; i < plusSize; i++)
            {
                for (int j = 0; j < plusSize; j++)
                {
                    map[i, j] = 0;
                }
            }
        }

        //Метод, который инициализирует карту-клона 
        public void InitializeClone()
        {
            clone = new Map();
            clone.InitializeMap();
        }

        //Этот метод переводит строчку с координатами в массив с целыми числами
        public List<int> convert(string s)
        {
            int space = s.IndexOf(" ");
            int y = nameMap.IndexOf(s[0]) + 1;
            int x;
            if (space == -1)
                x = Convert.ToInt32(s.Substring(1));
            else
                x = Convert.ToInt32(s[1..s.IndexOf(" ")]);
            if (x > 10 || y > 10)
                throw new Exception();
            List<int> result = new List<int>();
            result.Add(x);
            result.Add(y);
            if (space == -1)
                return result;
            else
            {
                y = nameMap.IndexOf(s[space + 1]) + 1;
                x = Convert.ToInt32(s.Substring(space + 2));
                if (x > 10 || y > 10)
                    throw new Exception();
                result.Add(x);
                result.Add(y);
                return result;
            }
        }

        //Метод, который проверяет, были ли сбиты все корабли у карты x
        public bool checkMap(Map x)
        {
            int amount = 0;
            for (int i = 1; i < size; i++)
                for (int j = 1; j < size; j++)
                {
                    if (x.map[i, j] == 3)
                        amount++;
                }
            if (amount == 20)
                return true;
            else
                return false;
        }

        //Метод, который проверяет, можем ли мы по правилам установить палубу 
        //по заданным координатам
        public Boolean checkShip(int x, int y)
        {
            try
            {
                //Проврека, что проверяемое поле пустое
                if (map[x, y] != 1 && map[x + 1, y] != 1)
                {
                    //Проверка, что соседние поля сверху и справа пустые
                    if (map[x + 1, y] != 1 && map[x, y + 1] != 1)
                    {
                        //Проверка, что соседние поля слева и снизу пустые
                        if (map[x - 1, y] != 1 && map[x, y - 1] != 1)
                        {
                            //Проверка, что соседние по диагонали пустые
                            if (map[x + 1, y + 1] != 1 && map[x + 1, y - 1] != 1 && map[x - 1, y + 1] != 1
                                && map[x - 1, y - 1] != 1)
                                return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        //Метод, который проверяет, можно ли установить многопалубный корабль
        //по заданным координатам
        public Boolean mayShip(int x1, int y1, int x2, int y2, int decks)
        {
            //Проверяем, лежит корабль вертикально или горизонтально
            if (x1 == x2 || y1 == y2)
            {
                //Проверяем длинну корабля
                if (Math.Abs(y1 - y2) == decks - 1 || Math.Abs(x1 - x2) == decks - 1)
                {
                    //Проверяем, не задевает ли корабль другие корабли

                    for (int i = Math.Min(x1, x2); i < Math.Max(x1, x2) + 1; i++)
                    {
                        for (int j = Math.Min(y1, y2); j < Math.Max(y1, y2) + 1; j++)
                        {
                            if (!checkShip(i, j))
                                return false;
                        }
                    }

                    return true;

                }
            }

            return false;

        }

        //Метод, который устанавливает корабль на поле
        public void setShip(int decks, string s)
        {
            try
            {
                List<int> coordinate = convert(s);
                if (coordinate.Count > 2)
                {
                    int x1 = coordinate[0];
                    int y1 = coordinate[1];
                    int x2 = coordinate[2];
                    int y2 = coordinate[3];

                    if (mayShip(x1, y1, x2, y2, decks))
                    {
                        for (int i = Math.Min(x1, x2); i < Math.Max(x1, x2) + 1; i++)
                        {
                            for (int j = Math.Min(y1, y2); j < Math.Max(y1, y2) + 1; j++)
                            {
                                map[i, j] = 1;
                            }
                        }
                        this.printMap();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    int x = coordinate[0];
                    int y = coordinate[1];
                    if (checkShip(x, y))
                    {
                        map[x, y] = 1;
                        this.printMap();
                    }
                    else throw new Exception();
                }
            }
            catch
            {
                Console.WriteLine("Вы допустили ошибку при вводе координаты, попробуйте снова");
                string tryagain = Console.ReadLine();
                setShip(decks, tryagain);
            }
        }

        //Метод, который уничтожает клетку противника
        public bool annihilation(Map victim, int x, int y)
        {
            if (victim.map[x, y] == 1)
            {
                victim.map[x, y] = 3;
                victim.clone.map[x, y] = 3;
                //Уведомление о "сбитом" или "подбитом" корабле
                if (victim.checkShip(x, y))
                {
                    Console.WriteLine("Корабль сбит!");
                }
                else
                    Console.WriteLine("Корабль подбит!");
                return true;
            }
            else if (victim.map[x, y] == 3 || victim.map[x, y] == 2)
            {
                return true;
            }
            else if (victim.map[x, y] == 0)
            {
                victim.map[x, y] = 2;
                victim.clone.map[x, y] = 2;
                return false;
            }
            else
                throw new Exception();

        }

        //Метод, который рисует в консоли карту
        public void printMap()
        {
            for (int i = 0; i < plusSize - 1; i++)
            {
                for (int j = 0; j < plusSize - 1; j++)
                {
                    //В самом вверху прописываем буквы, чтобы
                    //было удобно ориентироваться на поле
                    if (i == 0 && j != 0 && j != plusSize - 1)
                    {
                        Console.Write(nameMap.Substring(j - 1, 1) + " ");
                    }
                    //По левому краю укажем номера строк
                    else if (j == 0 && i != 0 && i != 10)
                    {
                        Console.Write(i + ":" + "  ");
                    }
                    //Для левого верхнего края нулевую строку указывать
                    //не будем, а просто сделаем отступ
                    else if (i == 0)
                    {
                        Console.Write("    ");
                    }
                    //Поскольку "10" занимает больше пикселей в консоле,
                    //сделаем отступ от неё по короче (в один пробел)
                    else if (i == 10 && j == 0)
                    {
                        Console.Write(i + ":" + " ");
                    }
                    //В остальных случаях проврим, есть ли палуба в клетке.
                    //Если да - выведем "Y ", если нет - "- "
                    //Если палуба подбита - "X ", если подбита пустая клетка - "0 "
                    else if (map[i, j] == 0)
                        Console.Write("- ");
                    else if (map[i, j] == 1)
                        Console.Write("Y ");
                    else if (map[i, j] == 2)
                        Console.Write(0 + " ");
                    else
                        Console.Write("X ");
                }
                Console.WriteLine();

            }

        }

        //Метод, который генерирует случайную карту
        //с установленными кораблями
        public void createRandomMap()
        {
            Random r = new Random();
            //Этот метод подбирает свободную координату
            //и возвращает её с помощью типа Pair,
            //описанного после класса Game
            Pair ones()
            {
                do
                {
                    int x = r.Next(size) + 1;
                    int y = r.Next(size) + 1;
                    do
                    {
                        y = r.Next(size) + 1;
                    } while (y == x);
                    //Console.WriteLine("x: " + x + " y: " + y);
                    if (checkShip(x, y))
                    {
                        return new Pair(x, y);
                    }

                } while (true);
            }

            void setRandomShip(int shiplen)
            {
                shiplen -= 1;
                bool b;
                //Здесь мы берем случайную свободную координату методом ones();
                //Затем мы проверяем соседние координаты, и если какая-то из
                //них свободна, устанавливаем многопалубник, а если нет
                //то пробуем до тех пор, пока не получится
                do
                {
                    Pair coordinate = ones();

                    bool ifwhat(int j)
                    {
                        //Проверка на возможность поставить корабль:
                        //вертикально, добавив палубы снизу
                        if (j == 0)
                        {
                            if (checkShip(coordinate.x + shiplen, coordinate.y))
                            {
                                for (int i = coordinate.x; i < coordinate.x + shiplen + 1; i++)
                                {
                                    map[i, coordinate.y] = 1;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        //Проверка на возможно поставить корабль:
                        //вертикально, добавив палубы сверху
                        else if (j == 1)
                        {
                            if (checkShip(coordinate.x - shiplen, coordinate.y))
                            {
                                for (int i = coordinate.x - shiplen; i < coordinate.x + 1; i++)
                                {
                                    map[i, coordinate.y] = 1;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        //Проверка на возможно поставить корабль:
                        //горизонтально, добавив палубы на правую клетку
                        else if (j == 2)
                        {
                            if (checkShip(coordinate.x, coordinate.y + shiplen))
                            {
                                for (int i = coordinate.y; i < coordinate.y + shiplen + 1; i++)
                                {
                                    map[coordinate.x, i] = 1;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        //Проверка на возможно поставить корабль:
                        //горизонтально, добавив палубы на левую клетку
                        else if (j == 3)
                        {
                            if (checkShip(coordinate.x, coordinate.y - shiplen))
                            {
                                for (int i = coordinate.y - shiplen; i < coordinate.y + 1; i++)
                                {
                                    map[coordinate.x, i] = 1;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        return false;
                    }

                    //Следующий блок запускает проверки в случайном порядке
                    b = false;
                    for (int i = 0; i < 20; i++)
                    {
                        b = ifwhat(r.Next(4));
                        if (b) break;
                    }
                    //Console.WriteLine("ww");
                } while (b == false);
            }

            //Этот цикл расставляет однопалубные корабли
            for (int i = 0; i < 4; i++)
            {
                Pair coordinate = ones();
                map[coordinate.x, coordinate.y] = 1;
            }

            //Этот цикл расставляет двупалубные корабли
            for (int i = 0; i < 3; i++)
            {
                setRandomShip(2);
            }

            //Этот цикл расставляет трехпалубные
            for (int i = 0; i < 2; i++)
            {
                setRandomShip(3);
            }

            //Эта операция устанавливает четырехпалубник
            setRandomShip(4);

        }

    }

}
