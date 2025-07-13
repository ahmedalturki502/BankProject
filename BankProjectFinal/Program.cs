using System;
using System.Collections.Generic; // for lists
using System.ComponentModel.DataAnnotations;
using System.IO;                  // for file reading/writing
using System.Linq;                // searching/filtering

// تعريف الديليجيت للتنبيه بعد العمليات
public delegate void TransactionAlert(string message); // Delegate to print message

class Person
{
    private string name { get; set; }
    [Required]
    [EmailAddress]
    private int age { get; set; }

    public string Name
    {
        get { return name; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("Name cannot be empty");
            name = value;
        }
    }
    public int Age
    {
        get { return age; }
        set
        {
            if (value < 0)
                throw new Exception("Age cannot be negative");
            age = value;
        }
    }
        
    
}


class Customer : Person
{
    public string AccountNumber { get; set; }
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    private decimal balance;
    public decimal Balance
    {
        get { return balance; }
        set
        {
            if (value < 0)
                throw new Exception("Balance cannot be less than 0");
            balance = value;

        }
    }

    public override string ToString() =>
        $"Customer: {Name}, Age: {Age}, Balance: {Balance}";
}

class Account

{
    private string accountNumber;
    public string AccountNumber
    {
        get { return AccountNumber; }
        set
        {
            AccountNumber = value;
        }
    }
    private Customer owner;
    public Customer  Owner{
        get { return owner; }
        set
        {
            owner = value; 
        }

        }
    
    
    
}


class Transaction
{
    private DateTime date;
    private string type;
    private decimal amount;

    public DateTime Date
    {
        get { return date; }
        set { date = value; }
    }

    public string Type
    {
        get { return type; }
        set { type = value; }
    }

    public decimal Amount
    {
        get { return amount; }
        set { amount = value; }
    }

    public override string ToString() =>
        $"{Date:yyyy-MM-dd HH:mm} - {Type} - {Amount}";
}
class BankData<T>    // Generics
{
    public List<T> Items { get; set; } = new List<T>();     //here we create new list
    public void AddItem(T item) => Items.Add(item);         //here we add items in the list we created
    public List<T> GetAll() => Items;                       //here we return all the items in the list
}

class Program
{
    static BankData<Customer> customers = new BankData<Customer>();     //this is object from the main class (BankData) ; his job is save list contains all the costumers in the bank
    static string filePath = "accounts.txt";                             
    static TransactionAlert alert = ShowMessage;                        // هنا نربط الديليقيت اللي سويناه فوق بداله اسمها شوماسج .هذا يعني كل مره يصير سحب او ايداع راح تنفذ دالة شوماسج
    static int nextAccountNumber = 10000001; 

    static void Main()
    {
        LoadAccounts();     // Loads accounts from file at start
        while (true)
        {
            Console.WriteLine("\n1. Create new account\n2. Deposit\n3. Withdraw\n4. Show balance\n5. Save accounts\n6. Filter transactions\n7. Calculate Totals\n8. Group transactions\n9. Exit");
            Console.Write("Choose: ");
            string choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1": CreateAccount(); break;
                    case "2": Deposit(); break;
                    case "3": Withdraw(); break;
                    case "4": ShowBalance(); break;
                    case "5": SaveAccounts(); break;
                    case "6": FilterTransactions(); break;
                    case "7": CalculateTotals(); break;
                    case "8": GroupTransactions(); break;
                    case "9": SaveAccounts(); return;
                    default: Console.WriteLine("Invalid choice"); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    // إنشاء حساب جديد
    static void CreateAccount()
    {
        Console.Write("Customer name: ");
        string name = Console.ReadLine();
        Console.Write("Age: ");
        if (!int.TryParse(Console.ReadLine(), out int age))  //(TryPars) try to change the text that user write to integer and if he write wrong like (عشرين) will Exception ans show message
            throw new Exception("Invalid age");
        string accountNumber = (nextAccountNumber++).ToString(); // Generate unique account number
        var customer = new Customer { Name = name, Age = age, Balance = 0, AccountNumber = accountNumber };
        customers.AddItem(customer);
        alert?.Invoke($"Account created successfully! Your Account Number is: {accountNumber}");
    }

    // إيداع
    static void Deposit()
    {
        var customer = FindCustomer();
        Console.Write("Amount: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            throw new Exception("Invalid amount");
        customer.Balance += amount;
        customer.Transactions.Add(new Transaction { Date = DateTime.Now, Type = "Deposit", Amount = amount });
        alert?.Invoke("Deposit successful!");
    }

    // سحب
    static void Withdraw()
    {
        var customer = FindCustomer();
        Console.Write("Amount: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            throw new Exception("Invalid amount");
        if (amount > customer.Balance)
            throw new Exception("Insufficient balance");
        customer.Balance -= amount;
        customer.Transactions.Add(new Transaction { Date = DateTime.Now, Type = "Withdraw", Amount = amount });
        alert?.Invoke("Withdraw successful!");
    }

    // عرض الرصيد والمعاملات
    static void ShowBalance()
    {
        var customer = FindCustomer();
        Console.WriteLine(customer);
        Console.WriteLine("Transaction history:");
        foreach (var t in customer.Transactions)
            Console.WriteLine(t);
    }

    // البحث عن عميل بالاسم (LINQ)
    static Customer FindCustomer()
    {
        Console.Write("Search by (1) Name or (2) Account Number? ");
        string option = Console.ReadLine();
        if (option == "1")
        {
            Console.Write("Customer name: ");
            string name = Console.ReadLine();
            var customer = customers.GetAll().FirstOrDefault(c => c.Name == name);
            if (customer == null)
                throw new Exception("Customer not found");
            return customer;
        }
        else if (option == "2")
        {
            Console.Write("Account Number: ");
            string accNum = Console.ReadLine();
            var customer = customers.GetAll().FirstOrDefault(c => c.AccountNumber == accNum);
            if (customer == null)
                throw new Exception("Customer not found");
            return customer;
        }
        else
        {
            throw new Exception("Invalid option");
        }
    }

    // حفظ الحسابات في ملف نصي
    static void SaveAccounts()
    {
        using (var sw = new StreamWriter(filePath))                // Streams
        {
            foreach (var c in customers.GetAll())
            {
                sw.WriteLine($"{c.Name}|{c.Age}|{c.AccountNumber}|{c.Balance}|{string.Join(";", c.Transactions.Select(t => $"{t.Date:O},{t.Type},{t.Amount}"))}");
            }
        }
        alert?.Invoke("Accounts saved to file.");
    }

    // تحميل الحسابات من ملف نصي
    static void LoadAccounts()
    {
        if (!File.Exists(filePath)) return;
        foreach (var line in File.ReadAllLines(filePath))               // streams
        {
            var parts = line.Split('|');
            var customer = new Customer
            {
                Name = parts[0],
                Age = int.Parse(parts[1]),
                AccountNumber = parts[2],
                Balance = decimal.Parse(parts[3])
            };
            // ثم نفس كود المعاملات
            if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
            {
                foreach (var t in parts[4].Split(';'))
                {
                    var tparts = t.Split(',');
                    customer.Transactions.Add(new Transaction
                    {
                        Date = DateTime.Parse(tparts[0]),
                        Type = tparts[1],
                        Amount = decimal.Parse(tparts[2])
                    });
                }
            }
            customers.AddItem(customer);
        }
    }
    static void FilterTransactions()         //filter transaction By using Link
    {
        var customer = FindCustomer();
        Console.WriteLine("Filter by: 1) Date  2) Type");
        string option = Console.ReadLine();

        if (option == "1")
        {
            Console.Write("Enter date (yyyy-MM-dd): ");
            string dateInput = Console.ReadLine();
            if (DateTime.TryParse(dateInput, out DateTime date))
            {
                var filtered = customer.Transactions
                    .Where(t => t.Date.Date == date.Date)
                    .ToList();

                Console.WriteLine($"Transactions on {date:yyyy-MM-dd}:");
                foreach (var t in filtered)
                    Console.WriteLine(t);

                if (filtered.Count == 0)
                    Console.WriteLine("No transactions found for this date.");
            }
            else
            {
                Console.WriteLine("Invalid date format.");
            }
        }
        else if (option == "2")
        {
            Console.Write("Enter type (Deposit/Withdraw): ");
            string type = Console.ReadLine();
            var filtered = customer.Transactions
                .Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine($"Transactions of type '{type}':");
            foreach (var t in filtered)
                Console.WriteLine(t);

            if (filtered.Count == 0)
                Console.WriteLine("No transactions found for this type.");
        }
        else
        {
            Console.WriteLine("Invalid option.");
        }
    }
    static void CalculateTotals()
    {
        var customer = FindCustomer();

        // مجموع الإيداعات
        var totalDeposits = customer.Transactions
            .Where(t => t.Type.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        // مجموع السحوبات
        var totalWithdrawals = customer.Transactions
            .Where(t => t.Type.Equals("Withdraw", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        // مجموع كل المعاملات (إيداع + سحب)
        var totalTransactions = customer.Transactions.Sum(t => t.Amount);

        Console.WriteLine($"Total Deposits: {totalDeposits}");
        Console.WriteLine($"Total Withdrawals: {totalWithdrawals}");
        Console.WriteLine($"Total of all Transactions: {totalTransactions}");
    }
    static void GroupTransactions()
    {
        var customer = FindCustomer();
        Console.WriteLine("Group by: 1) Type  2) Date");
        string option = Console.ReadLine();

        if (option == "1")
        {
            var grouped = customer.Transactions
                .GroupBy(t => t.Type);

            foreach (var group in grouped)
            {
                Console.WriteLine($"\nType: {group.Key} (Count: {group.Count()})");
                foreach (var t in group)
                    Console.WriteLine(t);
            }
        }
        else if (option == "2")
        {
            var grouped = customer.Transactions
                .GroupBy(t => t.Date.Date);

            foreach (var group in grouped)
            {
                Console.WriteLine($"\nDate: {group.Key:yyyy-MM-dd} (Count: {group.Count()})");
                foreach (var t in group)
                    Console.WriteLine(t);
            }
        }
        else
        {
            Console.WriteLine("Invalid option.");
        }
    }


    static void ShowMessage(string msg) => Console.WriteLine($"[Alert] {msg}");   //delegate
}