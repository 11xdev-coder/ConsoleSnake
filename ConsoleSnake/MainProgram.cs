using System;

namespace ConsoleSnake
{
    public static class MainProgram
    {
        public static void Main()
        {
            Snake snake = new Snake(new Position(16, 8), false);
            Field field = new Field(32, 16, snake);
            snake.LinkField(field);
            field.PrintField();
            
            snake.Start();

            Console.ReadKey();
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
                    // corners
                    if (h == 0 && w == 0) Console.Write("┏");
                    else if (h == Height - 1 && w == 0) Console.Write("┗");
                    else if (h == 0 && w == Width - 1) Console.Write("┓");
                    else if (h == Height - 1 && w == Width - 1) Console.Write("┛");
                    // borders
                    else if (h == 0 || h == Height - 1) Console.Write("━"); // top and bottom border
                    else if (w == 0 || w == Width - 1) Console.Write("┃"); // left and right border
                    else if (w == Snake.Position.X && h == Snake.Position.Y) Console.Write("■"); // snake
                    else Console.Write(" ");
                }
                Console.WriteLine();
            }
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
            while (!Lost)
            {
                _input = Console.ReadKey(!ShowInput);
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
                //Console.WriteLine(_input.KeyChar);
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
}
