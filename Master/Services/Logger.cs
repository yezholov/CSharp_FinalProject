namespace Master.Services
{
    public class Logger
    {
        /*
        This method is used to get the color of the text in the console.
        -9 – Error
        -8 – Warning
        0 – Master
        1 – Agent 1
        2 – Agent 2
        11 – Master / Agent 1
        12 – Master / Agent 2
        */
        private string GetColorAnsi(int id)
        {
            return id switch
            {
                -9 => "91",
                -8 => "93",
                0 => "94",
                1 => "92",
                2 => "95",
                11 => "32",
                12 => "35",
                _ => "97"
            };
        }

        /*
        This method is used to get the name of the logger.
        Limit id < 10.
        0 – Master
        > 0 and < 10 – Agent x
        > 10 and < 20 – Master / Agent x (x = id - 10)
        */
        private string GetName(int id)
        {
            return id switch
            {
                0 => "Master",
                > 0 and < 10 => "Agent " + id,
                > 10 and < 20 => "Master / Agent " + (id - 10),
                _ => "Unknown"
            };
        }

        private string _color;
        private string _name;

        public Logger(int id)
        {
            _color = GetColorAnsi(id);
            _name = GetName(id);
        }

        /*
        Log a message with name and color.
        If the message starts with "Error" or "Warning", call the Error or Warning method.
        */
        public void Log(string message)
        {
            if (message[0..5] == "Error")
            {
                Error(message);
            }
            else if (message[0..7] == "Warning")
            {
                Warning(message);
            }
            else
            {
                Console.WriteLine($"\u001b[{_color}m[{_name}] \u001b[0m{message}");
            }
        }

        /*
        Log an error message with name and color.
        */
        public void Error(string message)
        {
            //If the message starts with "Error*:", remove the "Error*:" part.
            //We will add it back later.
            if (message[0..5] == "Error")
            {
                message = message[(message.IndexOf(":") + 1)..];
            }
            Console.WriteLine($"\u001b[{GetColorAnsi(-9)}m[{_name}] Error: \u001b[0m{message}");
        }

        /*
        Log a warning message with name and color.
        */
        public void Warning(string message)
        {
            //If the message starts with "Warning*:", remove the "Warning*:" part.
            //We will add it back later.
            if (message[0..7] == "Warning")
            {
                message = message[(message.IndexOf(":") + 1)..];
            }
            Console.WriteLine($"\u001b[{GetColorAnsi(-8)}m[{_name}] Warning: \u001b[0m{message}");
        }

        /*
        Write line without color and name.
        */
        public void FreeLine(string message)
        {
            Console.WriteLine(message);
        }

        /*
        Write a spacer line.
        */
        public void Spacer()
        {
            Console.WriteLine();
        }
    }
}
