using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using HTTP_Requests;


using HttpClient client = new HttpClient { BaseAddress = new Uri("https://jsonplaceholder.typicode.com") };

JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

async Task GetUsersAsync()
{
    try
    {
        HttpResponseMessage response = await client.GetAsync("users");
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        string json = await response.Content.ReadAsStringAsync();
        List<User>? users = JsonSerializer.Deserialize<List<User>>(json, options);

        foreach (User user in users)
        {
            Console.WriteLine($"{user.Id}: {user.Name}. Email: {user.Email}");
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Network wrror: {ex.Message}");
    }
}

async Task GetUserByIdAsync(int id)
{
    try
    {
        HttpResponseMessage response = await client.GetAsync($"users/{id}");
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"User {id} not found!");
            return;
        }

        string json = await response.Content.ReadAsStringAsync();
        User? user = JsonSerializer.Deserialize<User>(json, options);

        if (user is null)
        {
            Console.WriteLine("Failed to read user");
            return;
        }

        Console.WriteLine($"{user.Id}: {user.Name}. Email: {user.Email}");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Network error: {ex.Message}");
    }
}

async Task DeleteUserByIdAsync(int id)
{
    try
    {
        HttpResponseMessage response = await client.DeleteAsync($"users/{id}");
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");

        if (response.IsSuccessStatusCode)
            Console.WriteLine($"User {id} deleted!");
        else
            Console.WriteLine($"Failed to delete user {id}");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Network error: {ex.Message}");
    }
}

async Task CreateUserAsync()
{
    try
    {
        Console.Write("Name: ");
        string? name = Console.ReadLine()?.Trim();

        Console.Write("Username: ");
        string? username = Console.ReadLine()?.Trim();

        Console.Write("Email: ");
        string? email = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) ||string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("All fields are required!");
            return;
        }

        if (!email.Contains('@'))
        {
            Console.WriteLine("Invalid email!");
            return;
        }

        User newUser = new User { Name = name, Username = username, Email = email };

        string json = JsonSerializer.Serialize(newUser, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("users", content);
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");

        string body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response body: {body}");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Network error: {ex.Message}");
    }
}

async Task CheckStatusesAsync()
{
    string[] urls = { "users/1", "users/9999", "invalid-resource" };

    foreach (string url in urls)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);

            Console.WriteLine($"URL: {client.BaseAddress}{url}");
            Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Success: {response.IsSuccessStatusCode}");

            if (response.IsSuccessStatusCode)
                Console.WriteLine("Result: Resource is available");
            else if (response.StatusCode == HttpStatusCode.NotFound)
                Console.WriteLine("Result: Resource not found");
            else
                Console.WriteLine("Result: Request failed");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"URL: {url}");
            Console.WriteLine($"Network error: {ex.Message}");
        }

        Console.WriteLine();
    }
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("===== MENU =====");
    Console.WriteLine("1. Get all users");
    Console.WriteLine("2. Get user by ID");
    Console.WriteLine("3. Create new user");
    Console.WriteLine("4. Delete user by ID");
    Console.WriteLine("5. Check HTTP statuses");
    Console.WriteLine("6. Exit");
    Console.Write("Your choice: ");

    string? choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    if (choice == "1")
    {
        await GetUsersAsync();
    }
    else if (choice == "2")
    {
        Console.Write("Input ID for getting user info: ");
        int? id = Convert.ToInt32(Console.ReadLine());

        if (id <= 0)
        {
            Console.WriteLine("ID can't be negative!");
        }

        if (id is not null)
            await GetUserByIdAsync(id.Value);
    }
    else if (choice == "3")
    {
        await CreateUserAsync();
    }
    else if (choice == "4")
    {
        Console.Write("Input ID for deleting user: ");
        int? id = Convert.ToInt32(Console.ReadLine());

        if (id <= 0)
        {
            Console.WriteLine("ID can't be negative!");
        }

        if (id is not null)
            await DeleteUserByIdAsync(id.Value);
    }
    else if (choice == "5")
    {
        await CheckStatusesAsync();
    }
    else if (choice == "6")
    {
        Console.WriteLine("Goodbye!");
        break;
    }
    else
    {
        Console.WriteLine("Invalid choice! Enter a number from 1 to 6.");
    }
}