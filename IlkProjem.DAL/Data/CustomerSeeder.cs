using Bogus;
using IlkProjem.Core.Models; // Modellerin olduğu katman

namespace IlkProjem.DAL.Data;

public static class CustomerSeeder
{
    public static List<Customer> GetFakeCustomers(int count)
    {
        var customerFaker = new Faker<Customer>("tr")
            // Id'yi veritabanı vereceği için buraya hiç yazmıyoruz.
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name))
            
            // Bakiye başta 0 olarak belirlendi
            .RuleFor(c => c.Balance, f => 0m); 

        return customerFaker.Generate(count);
    }
}