
using System.Formats.Asn1;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

Main main = new();
main.Start();

//bying an orange
// Product orange = new("orange", 100, 1);
// Product apple = new("apple", 50, 4);
// machine.ItemList.Add(orange);
// machine.ItemList.Add(apple);
// machine.Print();
// orange.Print();
// orange.BeBought(machine);
// orange.Print();
// machine.Print();

//adding cola
// Product cola = new("cola", 150, 10);
// machine.ItemList.Add(cola);
// machine.Print();
// cola.Refill(machine);
// machine.Print();



class Product
{
    public string Name { get; set; }
    public int Price { get; set; }
    public byte Stock { get; set; }
    public Product() { }

    public Product(string name, int price, byte stock)
    {
        Name = name; Price = price; Stock = stock;
    }

    public void BeBought(VendingMachine machine, int change)
    {
        Stock -= 1;
        change = Math.Abs(change);
        machine.Balance += Price + change;        
        if (Stock == 0)
        {
            machine.ItemList.Remove(this);
        }
        Console.WriteLine($"\nbuying {Name}...");
        machine.SaveToFile();
        if (machine.Balance >= change)
        {
            List<int> change_coins = new List<int>();
            int[] nominals = { 10, 5, 2, 1 };
            foreach (int i in nominals)
            {
                while (change >= i)
                {
                    change_coins.Add(i);
                    change -= i;
                }
            }
            Console.WriteLine($"\nhere's your {Name} ({Stock} left) and change ({string.Join(", ", change_coins)}), bye!");
            machine.Balance -= change;
            machine.SaveToFile();
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("\nsorry, cannot give the change. here's your {Name} ({Stock} left), bye!");
            Environment.Exit(0);
        }
              
    }
    public void TakeOut(VendingMachine machine)
    {
        machine.ItemList.Remove(this);
        Console.WriteLine($"taking out {Name}...");
        machine.SaveToFile();
    }
    public void Refill(VendingMachine machine)
    {
        Console.WriteLine($"\nthere are {Stock} pcs. how many pieces of {Name} would you like to add?");
        byte number = Convert.ToByte(Console.ReadLine());
        Stock += number;
        Console.WriteLine($"\nadded {number} pcs. now in stock {Stock} pcs.");
        machine.SaveToFile();
    }

    public void Print()
    {
        Console.WriteLine($"\nmeow my name is {Name}, I cost {Price} rubles, there are {Stock} of me");
    }
}


class VendingMachine
{
    public int Balance { get; set; }
    public List<Product> ItemList { get; set; } = new List<Product>();
    private const string FilePath = "vending_machine.json";
    public void SaveToFile()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(FilePath, json);
        Console.WriteLine($"\ndata saved to {FilePath}");
    }
    public static VendingMachine LoadFromFile()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<VendingMachine>(json) ?? new VendingMachine();
        }
        return new VendingMachine(); // если файла нет, делаем новый автомат
    }

    // public void Payment(item)
    // {
    //     Console.WriteLine($"{item}'s price is ");
    // }
    public void Refill()
    {
        if (ItemList.Count != 0)
        {
            Console.WriteLine("what would you like to refill, master? pick a number");
            for (int i = 0; i < ItemList.Count; i++)
            {
                Console.WriteLine($"{i}. {ItemList[i].Name}, {ItemList[i].Price} rub., {ItemList[i].Stock} pcs.");
            }
            Console.WriteLine("\nfor exit type -1");
            byte item = Convert.ToByte(Console.ReadLine());
            if (item == -1)
            {
                Environment.Exit(0);
            }
            ItemList[item].Refill(this);
        }
    
        Console.WriteLine("\nwould you like to put something new in, master?\n1. yes\n2. no");
        string answer = Console.ReadLine();
        while (answer == "1")
        {
            Console.WriteLine("\ntell me the name, price and stock (up to 255 pcs.) of the product you wanna put in. like this:\ncola 120 10");
            string item = Console.ReadLine();
            string[] splitted = item.Split(' ', 3);
            try
            {
                Product product = new(splitted[0], Convert.ToInt16(splitted[1]), Convert.ToByte(splitted[2]));
                ItemList.Add(product);
                Console.WriteLine("\nokay! now the machine has ");
                for (int i = 0; i < ItemList.Count; i++)
                {
                    Console.WriteLine($"{i}. {ItemList[i].Name}, {ItemList[i].Price} rub., {ItemList[i].Stock} pcs.");
                }
            }
            catch
            {
                Console.WriteLine("wrong input format");
                Environment.Exit(0);
            }
            this.SaveToFile();
            Console.WriteLine("\nanything else, master? (1/2)\n1. yes\n2. no");
            answer = Console.ReadLine();
        }
        if (answer == "2")
        {
            Environment.Exit(0);
        }
        
    }
    public void CheckBalance()
    {
        Console.WriteLine($"\nhere's my balance: {this.Balance} rub.\ndo you wanna take it out? (1/2)\n1. yes\n2. no");
        string answer = Console.ReadLine();
        if (answer == "1")
        {
            this.Balance = 0;
            this.SaveToFile();
            Console.WriteLine($"\ntake your money, Big Lebovsky. current balance: {this.Balance}. bye!");
        }
        else if (answer == "2")
        {
            Environment.Exit(0);
        }
    }
    public void Print()
    {
        Console.WriteLine($"\nI'm a vending machine, here's what I have: ");
        if (ItemList.Count != 0)
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                Console.WriteLine($"{i}. {ItemList[i].Name}, {ItemList[i].Price} rub., {ItemList[i].Stock} pcs.");
            }
            Console.WriteLine("\ntype a number of the item you wanna get.\nfor exit type -1");
            int answer = Convert.ToInt16(Console.ReadLine());
            if (answer == -1)
            {
                Environment.Exit(0);
            }
            else if (answer < ItemList.Count)
            {
                Console.WriteLine($"\n{this.ItemList[answer].Name} costs {this.ItemList[answer].Price} rub. \nput your coins in like this: 1 5 2 10");
                string coins = Console.ReadLine();
                try
                {
                    string[] splitted = coins.Split();
                    List<int> numbers = splitted.Select(int.Parse).ToList();
                    int change = numbers.Sum() - this.ItemList[answer].Price;
                    while (change < 0)
                    {
                        Console.WriteLine($"\nnot enough money. will you put {-change} more?\nfor exit type -1");
                        string more = Console.ReadLine();
                        if (more == "-1")
                        {
                            Environment.Exit(0);
                        }
                        else
                        {
                            string[] spl = more.Split();
                            List<int> nums = spl.Select(int.Parse).ToList();
                            change += nums.Sum();
                        }
                    }
                    if (change >= 0)
                    {
                        ItemList[answer].BeBought(this, change);
                    }                    
                }
                catch
                {
                    Console.WriteLine("wrong input format");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("sorry, I don't have such an item");
                Environment.Exit(0);
            }
        }
        else { Console.WriteLine("nothing:(\n"); }
    }
}

class User
{
    public void Buy(VendingMachine machine)
    {
        machine.Print();
        if (VendingMachine.LoadFromFile().ItemList.Count != 0)
        {
            byte item = Convert.ToByte(Console.ReadLine());
        }
        else
        {
            Main main = new();
            main.Start();
        }
    }
}

class Admin
{
    public void Administrate(VendingMachine machine)
    {
        Console.WriteLine("what would you like to do, master? (1/2/3)\n1. take out an item\n2. refill\n3. balance");
        string action = Console.ReadLine();
        if (action == "1")
        {
            if (machine.ItemList.Count != 0)
            {
                Console.WriteLine("which item would you like to take out, master?");
                for (int i = 0; i < machine.ItemList.Count; i++)
                {
                    Console.WriteLine($"{i}. {machine.ItemList[i].Name}, {machine.ItemList[i].Price} rub., {machine.ItemList[i].Stock} pcs.");
                }
                Console.WriteLine("\nfor exit type -1");
                int answer = Convert.ToInt16(Console.ReadLine());
                if (answer == -1)
                {
                    Environment.Exit(0);
                }
                else if (answer < machine.ItemList.Count)
                {
                    machine.ItemList[answer].BeBought(VendingMachine.LoadFromFile(), 0);
                }
            }
            else
            {
                Console.WriteLine("the machine is empty, master. wanna go back?\n1. yes\n2. no");
                string answer = Console.ReadLine();
                if (answer == "1")
                {
                    this.Administrate(VendingMachine.LoadFromFile());
                }
                else if (answer == "2")
                {
                    Environment.Exit(0);
                }
            }

        }
        else if (action == "2")
        {
            machine.Refill();
        }
        else if (action == "3")
        {
            machine.CheckBalance();
        }
    }
}

class Main
{
    public void Start()
    {
        VendingMachine machine = VendingMachine.LoadFromFile();
        Console.WriteLine("welcome to the vending machine! who are you? (1/2)\n1. user\n2. admin");
        string role = Console.ReadLine();
        if (role == "1")
        {
            User user = new();
            user.Buy(VendingMachine.LoadFromFile());
        }
        else if (role == "2")
        {
            Admin admin = new();
            admin.Administrate(VendingMachine.LoadFromFile());
        }
    }
}