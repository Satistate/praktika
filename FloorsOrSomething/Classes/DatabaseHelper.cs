using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Threading.Tasks;

namespace FloorsOrSomething.Classes
{
    public class DatabaseHelper
    {
        private string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Product> GetProducts()
        {
            var products = new List<Product>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT product_id, product_name FROM product", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            ProductName = reader.GetString(1)
                        });
                    }
                }
            }

            return products;
        }

        public List<Partner> GetPartners()
        {
            var partners = new List<Partner>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT partner_id, partner_name FROM partners", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        partners.Add(new Partner
                        {
                            PartnerId = reader.GetInt32(0),
                            PartnerName = reader.GetString(1)
                        });
                    }
                }
            }

            return partners;
        }

        public void AddPartnerProduct(int partnerId, int productId, int quantity, DateTime dateOfSale)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO partner_products (product, partner_name, quantity_of_products, date_of_sale) VALUES (@product, @partnerId, @quantity, @dateOfSale)", conn))
                {
                    cmd.Parameters.AddWithValue("product", productId);
                    cmd.Parameters.AddWithValue("partnerId", partnerId); 
                    cmd.Parameters.AddWithValue("quantity", quantity);
                    cmd.Parameters.AddWithValue("dateOfSale", dateOfSale);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddPartner(int partnerType, string partnerName, string director, string email, string phoneNumber, string legalAddress, string tin, decimal rating)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO partners (partner_type, partner_name, director, email, phone_number, legal_address, tin, rating) VALUES (@partnerType, @partnerName, @director, @email, @phoneNumber, @legalAddress, @tin, @rating)", conn))
                {
                    cmd.Parameters.AddWithValue("partnerType", partnerType);
                    cmd.Parameters.AddWithValue("partnerName", partnerName);
                    cmd.Parameters.AddWithValue("director", director);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("phoneNumber", phoneNumber);
                    cmd.Parameters.AddWithValue("legalAddress", legalAddress);
                    cmd.Parameters.AddWithValue("tin", tin);
                    cmd.Parameters.AddWithValue("rating", rating);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
    }

    public class Partner
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
    }


}